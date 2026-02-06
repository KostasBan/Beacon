using System;
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
            var client = new BeaconClient(repository);

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
            var client = new BeaconClient(repository);
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
    }
}
