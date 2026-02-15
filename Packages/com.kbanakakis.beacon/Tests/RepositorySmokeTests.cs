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
            public StoredConfig? Stored { get; private set; }

            public bool TryLoad(out StoredConfig stored)
            {
                stored = default;
                return false;
            }

            public Task StoreAsync(StoredConfig stored, CancellationToken ct)
            {
                Stored = stored;
                return Task.CompletedTask;
            }
        }
    }
}
