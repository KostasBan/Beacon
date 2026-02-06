namespace KBanakakis.Beacon.Repositories
{
    /// <summary>
    /// Abstraction for loading configuration data.
    /// </summary>
    public interface IConfigRepository
    {
        RepositorySnapshot LoadSnapshot();
    }
}
