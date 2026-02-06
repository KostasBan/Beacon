using System;
using System.Collections.Generic;
using KBanakakis.Beacon.Repositories;
using NUnit.Framework;

namespace KBanakakis.Beacon.Tests
{
    public sealed class RepositorySmokeTests
    {
        [Test]
        public void BeaconClientReturnsSnapshotFromRepository()
        {
            var snapshot = new RepositorySnapshot(DateTimeOffset.UtcNow, new Dictionary<string, string>());
            var repository = new StubConfigRepository(snapshot);
            var client = new BeaconClient(repository);

            var result = client.GetSnapshot();

            Assert.AreSame(snapshot, result);
        }

        private sealed class StubConfigRepository : IConfigRepository
        {
            private readonly RepositorySnapshot _snapshot;

            public StubConfigRepository(RepositorySnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public RepositorySnapshot LoadSnapshot()
            {
                return _snapshot;
            }
        }
    }
}
