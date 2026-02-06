using System;
using KBanakakis.Beacon.Repositories;

namespace KBanakakis.Beacon
{
    /// <summary>
    /// Entry point for accessing Beacon configuration data.
    /// </summary>
    public sealed class BeaconClient
    {
        private readonly IConfigRepository _repository;

        public BeaconClient(IConfigRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public RepositorySnapshot GetSnapshot()
        {
            return _repository.LoadSnapshot();
        }
    }
}
