using System.Text;
using KostasBan.Beacon.Context;
using KostasBan.Beacon.Evaluation;
using NUnit.Framework;

namespace KostasBan.Beacon.Tests
{
    public sealed class FlagEvaluatorTests
    {
        private readonly JsonFlagEvaluator _evaluator = new JsonFlagEvaluator();

        [Test]
        public void KillSwitchDisablesNonAllowlistedFlag()
        {
            var json = "{\"safety\":{\"killAllExperiments\":true,\"allowlistFlagsWhenKilled\":[\"other\"]},\"flags\":{\"featureA\":{\"enabled\":true}}}";
            var result = Evaluate(json, new BeaconContext("id", "Android", "1.0.0"), "featureA", true);

            Assert.IsFalse(result);
        }

        [Test]
        public void AllowlistedFlagStillEvaluatesWhenKillSwitchIsOn()
        {
            var json = "{\"safety\":{\"killAllExperiments\":true,\"allowlistFlagsWhenKilled\":[\"featureA\"]},\"flags\":{\"featureA\":{\"enabled\":true}}}";
            var result = Evaluate(json, new BeaconContext("id", "Android", "1.0.0"), "featureA", false);

            Assert.IsTrue(result);
        }

        [Test]
        public void RolloutIsDeterministicForSameInstallId()
        {
            var json = "{\"flags\":{\"featureA\":{\"enabled\":true,\"rolloutPercent\":50,\"salt\":\"stable\"}}}";
            var context = new BeaconContext("fixed-install", "Android", "1.0.0");

            var first = Evaluate(json, context, "featureA", false);
            var second = Evaluate(json, context, "featureA", false);

            Assert.AreEqual(first, second);
        }


        [Test]
        public void InvalidRolloutPercentDisablesFlag()
        {
            var json = "{\"flags\":{\"featureA\":{\"enabled\":true,\"rolloutPercent\":\"ten\"}}}";

            var result = Evaluate(json, new BeaconContext("id", "Android", "1.0.0"), "featureA", true);

            Assert.IsFalse(result);
        }

        [Test]
        public void PlatformTargetingWorks()
        {
            var json = "{\"flags\":{\"featureA\":{\"enabled\":true,\"targets\":{\"platforms\":[\"iOS\"]}}}}";

            var androidResult = Evaluate(json, new BeaconContext("id", "Android", "1.0.0"), "featureA", false);
            var iosResult = Evaluate(json, new BeaconContext("id", "iOS", "1.0.0"), "featureA", false);

            Assert.IsFalse(androidResult);
            Assert.IsTrue(iosResult);
        }

        [Test]
        public void MinAppVersionTargetingWorks()
        {
            var json = "{\"flags\":{\"featureA\":{\"enabled\":true,\"targets\":{\"minAppVersion\":\"1.2.0\"}}}}";

            var oldVersion = Evaluate(json, new BeaconContext("id", "Android", "1.1.9"), "featureA", false);
            var exactVersion = Evaluate(json, new BeaconContext("id", "Android", "1.2.0"), "featureA", false);
            var newVersion = Evaluate(json, new BeaconContext("id", "Android", "1.2.1"), "featureA", false);

            Assert.IsFalse(oldVersion);
            Assert.IsTrue(exactVersion);
            Assert.IsTrue(newVersion);
        }

        [Test]
        public void MinAppVersionWithSuffixesWorks()
        {
            var json = "{\"flags\":{\"featureA\":{\"enabled\":true,\"targets\":{\"minAppVersion\":\"1.2.3\"}}}}";

            var result = Evaluate(json, new BeaconContext("id", "Android", "1.2.3+45"), "featureA", false);

            Assert.IsTrue(result);
        }

        private bool Evaluate(string json, BeaconContext context, string flagKey, bool defaultValue)
        {
            return _evaluator.IsEnabled(Encoding.UTF8.GetBytes(json), context, flagKey, defaultValue);
        }
    }
}
