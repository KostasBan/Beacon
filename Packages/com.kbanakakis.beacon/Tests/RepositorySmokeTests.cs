using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Evaluation;
using KBanakakis.Beacon.Repositories;
using NUnit.Framework;

namespace KBanakakis.Beacon.Tests
{
    public sealed class RepositorySmokeTests
    {
        [Test]
        public void BeaconClientReturnsSnapshotFromRepository()
        {
            var snapshot = new RepositorySnapshot(
                "1.0.0",
                1,
                DateTimeOffset.UtcNow,
                RepositorySnapshot.ConfigProvenance.Default,
                null);
            var repository = new InMemoryConfigRepository(snapshot);
            var client = new BeaconClient(repository, new StubContextProvider(), new JsonFlagEvaluator());

            var result = client.GetSnapshot();

            Assert.AreEqual("1.0.0", result.ConfigVersion);
        }

        [Test]
        public void SnapshotChangedFiresWhenSnapshotUpdates()
        {
            var snapshot = new RepositorySnapshot(
                "1.0.0",
                1,
                DateTimeOffset.UtcNow,
                RepositorySnapshot.ConfigProvenance.Default,
                null);
            var repository = new InMemoryConfigRepository(snapshot);
            var client = new BeaconClient(repository, new StubContextProvider(), new JsonFlagEvaluator());
            var wasCalled = false;

            client.SnapshotChanged += _ => wasCalled = true;

            repository.SetSnapshotForTests(new RepositorySnapshot(
                "1.0.1",
                2,
                DateTimeOffset.UtcNow,
                RepositorySnapshot.ConfigProvenance.Remote,
                null));

            Assert.IsTrue(wasCalled);
        }

        [Test]
        public async Task DefaultRepositoryRefreshUpdatesSnapshot()
        {
            var payload = Encoding.UTF8.GetBytes("{\"meta\":{\"schemaVersion\":2,\"configVersion\":\"1.2.3\"}} ");
            var source = new FakeConfigSource(payload);
            var store = new FakeConfigStore();
            var validator = new BasicConfigValidator();
            var repository = new DefaultConfigRepository(source, store, validator);
            var wasCalled = false;

            repository.SnapshotChanged += _ => wasCalled = true;

            var result = await repository.RefreshAsync(CancellationToken.None);

            Assert.IsTrue(result.Changed);
            Assert.IsTrue(wasCalled);
            var snapshot = repository.GetSnapshot();
            Assert.AreEqual("1.2.3", snapshot.ConfigVersion);
            Assert.AreEqual(2, snapshot.SchemaVersion);
            Assert.IsNotNull(store.Stored);
        }

        [Test]
        public async Task DefaultRepositoryRefreshWithNoBytesTreatsAsNotChanged()
        {
            var existingPayload = Encoding.UTF8.GetBytes("{\"meta\":{\"schemaVersion\":1,\"configVersion\":\"1.0.0\"}}");
            var source = new SequenceConfigSource(
                new ConfigSourceResult(true, null, "etag-1", null));
            var store = new FakeConfigStore(new StoredConfig(existingPayload, "1.0.0"));
            var repository = new DefaultConfigRepository(source, store, new BasicConfigValidator());
            var wasCalled = false;
            repository.SnapshotChanged += _ => wasCalled = true;

            var result = await repository.RefreshAsync(CancellationToken.None);

            Assert.IsFalse(result.Changed);
            Assert.IsNull(result.Error);
            Assert.IsFalse(wasCalled);
            Assert.AreEqual(0, store.StoreCalls);
            Assert.AreEqual("1.0.0", repository.GetSnapshot().ConfigVersion);
        }

        [Test]
        public async Task DefaultRepositoryRefreshWithNoBytesAndNoCacheReturnsFailure()
        {
            var source = new SequenceConfigSource(
                new ConfigSourceResult(true, null, "etag-1", null));
            var store = new FakeConfigStore();
            var repository = new DefaultConfigRepository(source, store, new BasicConfigValidator());
            var wasCalled = false;
            repository.SnapshotChanged += _ => wasCalled = true;

            var result = await repository.RefreshAsync(CancellationToken.None);

            Assert.IsFalse(result.Changed);
            Assert.AreEqual("No config payload available and no cached snapshot exists.", result.Error);
            Assert.IsFalse(wasCalled);
            Assert.AreEqual(0, store.StoreCalls);
            Assert.IsNull(repository.GetSnapshot().Bytes);
        }

        [Test]
        public async Task FetchFailureDoesNotOverwriteStoredSnapshot()
        {
            var existingPayload = Encoding.UTF8.GetBytes("{\"meta\":{\"schemaVersion\":3,\"configVersion\":\"2.0.0\"}}");
            var source = new SequenceConfigSource(
                new ConfigSourceResult(false, null, null, "network down"));
            var store = new FakeConfigStore(new StoredConfig(existingPayload, "2.0.0"));
            var repository = new DefaultConfigRepository(source, store, new BasicConfigValidator());
            var before = repository.GetSnapshot();

            var result = await repository.RefreshAsync(CancellationToken.None);

            Assert.IsFalse(result.Changed);
            Assert.AreEqual("network down", result.Error);
            Assert.AreEqual(0, store.StoreCalls);
            var after = repository.GetSnapshot();
            Assert.AreEqual(before.ConfigVersion, after.ConfigVersion);
            Assert.AreEqual(before.SchemaVersion, after.SchemaVersion);
            Assert.AreEqual(before.Provenance, after.Provenance);
            CollectionAssert.AreEqual(before.Bytes, after.Bytes);
        }

        [Test]
        public async Task InvalidPayloadDoesNotOverwriteLastKnownGood()
        {
            var existingPayload = Encoding.UTF8.GetBytes("{\"meta\":{\"schemaVersion\":4,\"configVersion\":\"2.1.0\"}}");
            var invalidPayload = Encoding.UTF8.GetBytes("{\"meta\":{\"schemaVersion\":4\"");
            var source = new SequenceConfigSource(
                new ConfigSourceResult(true, invalidPayload, "etag-2", null));
            var store = new FakeConfigStore(new StoredConfig(existingPayload, "2.1.0"));
            var repository = new DefaultConfigRepository(source, store, new BasicConfigValidator());
            var before = repository.GetSnapshot();

            var result = await repository.RefreshAsync(CancellationToken.None);

            Assert.IsFalse(result.Changed);
            Assert.IsNotNull(result.Error);
            Assert.AreEqual(0, store.StoreCalls);
            var after = repository.GetSnapshot();
            Assert.AreEqual(before.ConfigVersion, after.ConfigVersion);
            CollectionAssert.AreEqual(before.Bytes, after.Bytes);
        }

        private sealed class StubContextProvider : IContextProvider
        {
            public BeaconContext GetContext()
            {
                return new BeaconContext("install", "Standalone", "1.0.0");
            }
        }

        private sealed class FakeConfigSource : IConfigSource
        {
            private readonly byte[] _bytes;

            public FakeConfigSource(byte[] bytes)
            {
                _bytes = bytes;
            }

            public Task<ConfigSourceResult> FetchAsync(CancellationToken ct)
            {
                return Task.FromResult(new ConfigSourceResult(true, _bytes, "test", null));
            }
        }

        private sealed class FakeConfigStore : IConfigStore
        {
            private readonly StoredConfig? _initialStored;

            public FakeConfigStore(StoredConfig? initialStored = null)
            {
                _initialStored = initialStored;
            }

            public StoredConfig? Stored { get; private set; }

            public int StoreCalls { get; private set; }

            public bool TryLoad(out StoredConfig stored)
            {
                if (_initialStored.HasValue)
                {
                    stored = _initialStored.Value;
                    return true;
                }

                stored = default;
                return false;
            }

            public Task StoreAsync(StoredConfig stored, CancellationToken ct)
            {
                Stored = stored;
                StoreCalls++;
                return Task.CompletedTask;
            }
        }

        private sealed class SequenceConfigSource : IConfigSource
        {
            private readonly ConfigSourceResult[] _results;
            private int _index;

            public SequenceConfigSource(params ConfigSourceResult[] results)
            {
                _results = results;
            }

            public Task<ConfigSourceResult> FetchAsync(CancellationToken ct)
            {
                var idx = Math.Min(_index, _results.Length - 1);
                _index++;
                return Task.FromResult(_results[idx]);
            }
        }
    }
}
