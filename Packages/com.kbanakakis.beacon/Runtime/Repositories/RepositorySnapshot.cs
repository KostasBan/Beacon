using System;
using System.Collections.Generic;

namespace KBanakakis.Beacon.Repositories
{
    /// <summary>
    /// Immutable snapshot of configuration entries.
    /// </summary>
    public sealed class RepositorySnapshot
    {
        public RepositorySnapshot(DateTimeOffset capturedAt, IReadOnlyDictionary<string, string> entries)
        {
            CapturedAt = capturedAt;
            Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        public DateTimeOffset CapturedAt { get; }

        public IReadOnlyDictionary<string, string> Entries { get; }
    }
}
