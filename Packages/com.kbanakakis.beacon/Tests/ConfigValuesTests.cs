using System;
using System.Text;
using KBanakakis.Beacon.Context;
using KBanakakis.Beacon.Evaluation;
using KBanakakis.Beacon.Repositories;
using KBanakakis.Beacon.Values;
using NUnit.Framework;

namespace KBanakakis.Beacon.Tests
{
    public sealed class ConfigValuesTests
    {
        [Test]
        public void JsonConfigValues_MissingValuesObject_ReturnsFalse()
        {
            var values = new JsonConfigValues("{\"meta\":{\"schemaVersion\":1}}");

            Assert.IsFalse(values.TryGetBool("show_debug", out _));
            Assert.IsFalse(values.TryGetInt("max_lives", out _));
            Assert.IsFalse(values.TryGetFloat("ui_scale", out _));
            Assert.IsFalse(values.TryGetString("ui_home_title", out _));
        }

        [Test]
        public void JsonConfigValues_ValidTypes_AreReturned()
        {
            var values = new JsonConfigValues("{\"values\":{\"ui_home_title\":\"Hello Beacon\",\"ui_scale\":1.1,\"max_lives\":5,\"show_debug\":true}}");

            Assert.IsTrue(values.TryGetString("ui_home_title", out var title));
            Assert.AreEqual("Hello Beacon", title);
            Assert.IsTrue(values.TryGetFloat("ui_scale", out var scale));
            Assert.AreEqual(1.1f, scale);
            Assert.IsTrue(values.TryGetInt("max_lives", out var lives));
            Assert.AreEqual(5, lives);
            Assert.IsTrue(values.TryGetBool("show_debug", out var showDebug));
            Assert.IsTrue(showDebug);
        }

        [Test]
        public void JsonConfigValues_MissingKey_ReturnsFalse()
        {
            var values = new JsonConfigValues("{\"values\":{\"max_lives\":5}}");

            Assert.IsFalse(values.TryGetInt("unknown", out _));
        }

        [Test]
        public void BeaconClient_MissingValuesObject_ReturnsDefaults()
        {
            var client = CreateClientWithRawBytes(Encoding.UTF8.GetBytes("{\"meta\":{\"schemaVersion\":1,\"configVersion\":\"demo-1\"}}"));

            Assert.AreEqual(false, client.GetBool("show_debug"));
            Assert.AreEqual(7, client.GetInt("max_lives", 7));
            Assert.AreEqual(1.5f, client.GetFloat("ui_scale", 1.5f));
            Assert.AreEqual("fallback", client.GetString("ui_home_title", "fallback"));
        }

        [Test]
        public void BeaconClient_WrongTypes_ReturnDefaults()
        {
            var json = "{\"values\":{\"max_lives\":\"5\",\"ui_scale\":\"1.1\",\"show_debug\":\"true\"}}";
            var client = CreateClientWithRawBytes(Encoding.UTF8.GetBytes(json));

            Assert.AreEqual(10, client.GetInt("max_lives", 10));
            Assert.AreEqual(2.5f, client.GetFloat("ui_scale", 2.5f));
            Assert.AreEqual(true, client.GetBool("show_debug", true));
        }

        [Test]
        public void BeaconClient_InvalidJson_ReturnsDefaults()
        {
            var client = CreateClientWithRawBytes(Encoding.UTF8.GetBytes("{"));

            Assert.AreEqual(4, client.GetInt("max_lives", 4));
            Assert.AreEqual("fallback", client.GetString("ui_home_title", "fallback"));
        }

        [Test]
        public void BeaconClient_InvalidUtf8Payload_ReturnsDefaults()
        {
            var client = CreateClientWithRawBytes(new byte[] { 0xC3, 0x28 });

            Assert.AreEqual(false, client.GetBool("show_debug"));
            Assert.AreEqual(3.0f, client.GetFloat("ui_scale", 3.0f));
        }

        private static BeaconClient CreateClientWithRawBytes(byte[] rawBytes)
        {
            var snapshot = new RepositorySnapshot(
                "demo-1",
                1,
                DateTimeOffset.UtcNow,
                RepositorySnapshot.ConfigProvenance.Remote,
                rawBytes);

            var repository = new InMemoryConfigRepository(snapshot);
            return new BeaconClient(repository, new StubContextProvider(), new JsonFlagEvaluator());
        }

        private sealed class StubContextProvider : IContextProvider
        {
            public BeaconContext GetContext()
            {
                return new BeaconContext("install", "Standalone", "1.0.0");
            }
        }
    }
}
