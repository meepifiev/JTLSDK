using System.Collections.Generic;
using JTLStudio.SDK.Services.Json;
using NUnit.Framework;

namespace JTLStudio.SDK.Tests.Core
{
    public class JsonTests
    {
        [Test]
        public void RoundTripsNestedDocument()
        {
            Dictionary<string, object> document = new Dictionary<string, object>
            {
                { "int", 42L },
                { "negative", -7L },
                { "float", 0.5 },
                { "bool", true },
                { "null", null },
                { "text", "line\n\"quoted\" \\ tab\t" },
                { "list", new List<object> { 1L, "two", false } },
                { "nested", new Dictionary<string, object> { { "deep", 1L } } }
            };

            string json = new JsonWriter().Write(document);
            Dictionary<string, object> parsed = (Dictionary<string, object>)new JsonParser().Parse(json);

            Assert.AreEqual(42L, parsed["int"]);
            Assert.AreEqual(-7L, parsed["negative"]);
            Assert.AreEqual(0.5, (double)parsed["float"], 0.000001);
            Assert.AreEqual(true, parsed["bool"]);
            Assert.IsNull(parsed["null"]);
            Assert.AreEqual("line\n\"quoted\" \\ tab\t", parsed["text"]);
            Assert.AreEqual(3, ((List<object>)parsed["list"]).Count);
            Assert.AreEqual(1L, ((Dictionary<string, object>)parsed["nested"])["deep"]);
        }

        [Test]
        public void ParsesWhitespaceAndUnicodeEscapes()
        {
            object parsed = new JsonParser().Parse(" { \"a\" : [ 1 , 2.5e1 , \"\\u0041\" ] } ");
            List<object> list = (List<object>)((Dictionary<string, object>)parsed)["a"];

            Assert.AreEqual(1L, list[0]);
            Assert.AreEqual(25.0, (double)list[1], 0.000001);
            Assert.AreEqual("A", list[2]);
        }

        [Test]
        public void RejectsMalformedInput()
        {
            Assert.Throws<System.FormatException>(() => new JsonParser().Parse("{\"a\":}"));
            Assert.Throws<System.FormatException>(() => new JsonParser().Parse("[1,2"));
            Assert.Throws<System.FormatException>(() => new JsonParser().Parse("{} extra"));
        }
    }
}
