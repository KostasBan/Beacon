using KBanakakis.Beacon.Context;

namespace KBanakakis.Beacon.Evaluation
{
    public interface IFlagEvaluator
    {
        bool IsEnabled(byte[] configBytes, BeaconContext ctx, string flagKey, bool defaultValue);
    }
}
