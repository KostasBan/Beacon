using System.Text;

namespace KostasBan.Beacon.Evaluation
{
    internal static class StableBucketer
    {
        public static uint Fnv1aHash(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            const uint offsetBasis = 2166136261;
            const uint prime = 16777619;

            var hash = offsetBasis;
            for (var i = 0; i < bytes.Length; i++)
            {
                hash ^= bytes[i];
                hash *= prime;
            }

            return hash;
        }

        public static int GetBucket0To99(string value)
        {
            return (int)(Fnv1aHash(value) % 100);
        }
    }
}
