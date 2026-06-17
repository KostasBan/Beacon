using KostasBan.Beacon.Context;

namespace KostasBan.Beacon.Evaluation
{
    public interface IFlagEvaluator
    {
        bool IsEnabled(byte[] configBytes, BeaconContext ctx, string flagKey, bool defaultValue);
    }
}
