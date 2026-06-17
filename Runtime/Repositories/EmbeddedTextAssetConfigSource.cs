using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KostasBan.Beacon.Repositories
{
    public sealed class EmbeddedTextAssetConfigSource : IConfigSource
    {
        private readonly TextAsset _textAsset;

        public EmbeddedTextAssetConfigSource(TextAsset textAsset)
        {
            _textAsset = textAsset ?? throw new ArgumentNullException(nameof(textAsset));
        }

        public Task<ConfigSourceResult> FetchAsync(CancellationToken ct)
        {
            var bytes = _textAsset.bytes;
            var result = new ConfigSourceResult(true, bytes, _textAsset.name, null);
            return Task.FromResult(result);
        }
    }
}
