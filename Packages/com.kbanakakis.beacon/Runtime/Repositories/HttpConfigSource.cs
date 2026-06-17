using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace KBanakakis.Beacon.Repositories
{
    public sealed class HttpConfigSource : IConfigSource
    {
        private readonly Uri _url;
        private readonly int _timeoutSeconds;
        private readonly int _retryCount;
        private readonly TimeSpan _retryBackoff;
        private readonly IReadOnlyDictionary<string, string>? _extraHeaders;
        private readonly Func<IReadOnlyDictionary<string, string>>? _headerProvider;
        private string? _lastKnownEtag;

        public HttpConfigSource(
            Uri url,
            int timeoutSeconds = 10,
            int retryCount = 2,
            TimeSpan? retryBackoff = null,
            IReadOnlyDictionary<string, string>? extraHeaders = null,
            Func<IReadOnlyDictionary<string, string>>? headerProvider = null)
        {
            _url = url ?? throw new ArgumentNullException(nameof(url));
            _timeoutSeconds = Math.Max(1, timeoutSeconds);
            _retryCount = Math.Max(0, retryCount);
            _retryBackoff = retryBackoff ?? TimeSpan.FromMilliseconds(500);
            _extraHeaders = extraHeaders;
            _headerProvider = headerProvider;
        }

        public HttpConfigSource(
            string url,
            int timeoutSeconds = 10,
            int retryCount = 2,
            TimeSpan? retryBackoff = null,
            IReadOnlyDictionary<string, string>? extraHeaders = null,
            Func<IReadOnlyDictionary<string, string>>? headerProvider = null)
            : this(new Uri(url ?? throw new ArgumentNullException(nameof(url))), timeoutSeconds, retryCount, retryBackoff, extraHeaders, headerProvider)
        {
        }

        public async Task<ConfigSourceResult> FetchAsync(CancellationToken ct)
        {
            ConfigSourceResult? lastFailure = null;

            for (var attempt = 0; attempt <= _retryCount; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                var result = await FetchOnceAsync(ct).ConfigureAwait(false);

                if (result.Success)
                {
                    return result;
                }

                lastFailure = result;
                if (attempt == _retryCount)
                {
                    break;
                }

                if (_retryBackoff > TimeSpan.Zero)
                {
                    var delay = TimeSpan.FromMilliseconds(_retryBackoff.TotalMilliseconds * Math.Pow(2, attempt));
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                }
            }

            return lastFailure ?? new ConfigSourceResult(false, null, null, "Failed to fetch config.");
        }

        private async Task<ConfigSourceResult> FetchOnceAsync(CancellationToken ct)
        {
            using (var request = UnityWebRequest.Get(_url.AbsoluteUri))
            {
                request.timeout = _timeoutSeconds;

                if (!string.IsNullOrEmpty(_lastKnownEtag))
                {
                    request.SetRequestHeader("If-None-Match", _lastKnownEtag);
                }

                ApplyHeaders(request, _extraHeaders);
                if (_headerProvider != null)
                {
                    ApplyHeaders(request, _headerProvider.Invoke());
                }

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    if (ct.IsCancellationRequested)
                    {
                        request.Abort();
                        ct.ThrowIfCancellationRequested();
                    }

                    await Task.Yield();
                }

                var statusCode = request.responseCode;
                if (statusCode == 304)
                {
                    var etag = request.GetResponseHeader("ETag");
                    if (!string.IsNullOrEmpty(etag))
                    {
                        _lastKnownEtag = etag;
                    }

                    return new ConfigSourceResult(true, null, _lastKnownEtag, null);
                }

                if (request.result == UnityWebRequest.Result.Success && statusCode == 200)
                {
                    var etag = request.GetResponseHeader("ETag");
                    var version = etag;
                    if (string.IsNullOrEmpty(version))
                    {
                        version = request.GetResponseHeader("Last-Modified");
                    }

                    if (!string.IsNullOrEmpty(etag))
                    {
                        _lastKnownEtag = etag;
                    }

                    return new ConfigSourceResult(true, request.downloadHandler.data, version, null);
                }

                return new ConfigSourceResult(
                    false,
                    null,
                    null,
                    $"HTTP {(int)statusCode}: {request.error}");
            }
        }

        private static void ApplyHeaders(UnityWebRequest request, IReadOnlyDictionary<string, string>? headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }
        }
    }
}
