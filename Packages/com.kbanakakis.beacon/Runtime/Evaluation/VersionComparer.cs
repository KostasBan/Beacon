using System;

namespace KBanakakis.Beacon.Evaluation
{
    public static class VersionComparer
    {
        public static int Compare(string a, string b)
        {
            var left = (a ?? string.Empty).Split('.');
            var right = (b ?? string.Empty).Split('.');
            var max = Math.Max(left.Length, right.Length);

            for (var i = 0; i < max; i++)
            {
                var leftPart = ParsePart(left, i);
                var rightPart = ParsePart(right, i);
                if (leftPart < rightPart)
                {
                    return -1;
                }

                if (leftPart > rightPart)
                {
                    return 1;
                }
            }

            return 0;
        }

        private static int ParsePart(string[] parts, int index)
        {
            if (index >= parts.Length)
            {
                return 0;
            }

            return int.TryParse(parts[index], out var value) ? value : 0;
        }
    }
}
