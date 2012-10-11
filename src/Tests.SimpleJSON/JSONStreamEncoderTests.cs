using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using SimpleJSON;

namespace Tests.SimpleJSON {
    [TestFixture]
    class JSONStreamEncoderTests {
        private Dictionary<string, string> _stringTestCases;

        [SetUp]
        public void SetUp() {
            _stringTestCases = new Dictionary<string, string>
                                   {
                                       { "string", "\"string\"" },
                                       { "\" \\ \b \f \n \r \t", "\"\\\" \\\\ \\b \\f \\n \\r \\t\"" },
                                       { "\"\\\b\f\n\r\t", "\"\\\"\\\\\\b\\f\\n\\r\\t\"" },
                                       { "foo \" \\ \b \f \n \r \t bar", "\"foo \\\" \\\\ \\b \\f \\n \\r \\t bar\"" },
                                       { "\u03A0", "\"\\u03A0\"" },
                                       { "foo \u03A0 bar", "\"foo \\u03A0 bar\"" },
                                       { "\0", "\"\\u0000\"" },
                                       { "foo \0 bar", "\"foo \\u0000 bar\"" },
                                       { "\U0001d120", "\"\\uD834\\uDD20\"" },
                                       { "bar \U0001d120 foo", "\"bar \\uD834\\uDD20 foo\"" },
                                       { "å", "\"\\u00E5\"" }
                                   };
        }

        [Test]
        public void String() {
            foreach (var pair in _stringTestCases) {
                Assert.AreEqual(pair.Value, EncodeSimple(e => e.WriteString(pair.Key)));
            }
        }

        [Test]
        public void Int() {
            Assert.AreEqual("0", EncodeSimple(e => e.WriteNumber(0)));
            Assert.AreEqual("123", EncodeSimple(e => e.WriteNumber(123)));
            Assert.AreEqual("2147483647", EncodeSimple(e => e.WriteNumber(Int32.MaxValue)));
            Assert.AreEqual("-2147483648", EncodeSimple(e => e.WriteNumber(Int32.MinValue)));

            Assert.AreEqual("123", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, false, false, 123, 0, 0, 0))));
            Assert.AreEqual("2147483647", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, false, false, 2147483647, 0, 0, 0))));
            Assert.AreEqual("-2147483648", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(true, false, false, 2147483648, 0, 0, 0))));
        }

        [Test]
        public void UInt() {
            Assert.AreEqual("0", EncodeSimple(e => e.WriteNumber(0U)));
            Assert.AreEqual("123", EncodeSimple(e => e.WriteNumber(123U)));
            Assert.AreEqual("4294967295", EncodeSimple(e => e.WriteNumber(UInt32.MaxValue)));

            Assert.AreEqual("4294967295", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, false, false, 4294967295, 0, 0, 0))));
        }

        [Test]
        public void Long() {
            Assert.AreEqual("0", EncodeSimple(e => e.WriteNumber(0L)));
            Assert.AreEqual("123", EncodeSimple(e => e.WriteNumber(123L)));
            Assert.AreEqual("9223372036854775807", EncodeSimple(e => e.WriteNumber(Int64.MaxValue)));
            Assert.AreEqual("-9223372036854775808", EncodeSimple(e => e.WriteNumber(Int64.MinValue)));

            Assert.AreEqual("9223372036854775807",
                            EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, false, false, 9223372036854775807, 0, 0, 0))));
            Assert.AreEqual("-9223372036854775808",
                            EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(true, false, false, 9223372036854775808, 0, 0, 0))));
        }

        [Test]
        public void Double() {
            Assert.AreEqual("0", EncodeSimple(e => e.WriteNumber(0.0)));
            Assert.AreEqual("1.5", EncodeSimple(e => e.WriteNumber(1.5)));
            Assert.AreEqual("1000000", EncodeSimple(e => e.WriteNumber(1.0e6)));
            Assert.AreEqual("-1000000", EncodeSimple(e => e.WriteNumber(-1.0e6)));
            Assert.AreEqual("0.000005", EncodeSimple(e => e.WriteNumber(5.0e-6)));
            Assert.AreEqual("0.012", EncodeSimple(e => e.WriteNumber(0.012)));

            Assert.AreEqual("1.5", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, true, false, 1, 5, 1, 0))));
            Assert.AreEqual("1000000", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, false, false, 1000000, 0, 1, 0))));
            Assert.AreEqual("-1000000", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(true, false, false, 1000000, 0, 1, 0))));
            Assert.AreEqual("0.000005", EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, true, true, 5, 0, 0, 6))));
        }

        [Test]
        public void Float() {
            Assert.AreEqual("0", EncodeSimple(e => e.WriteNumber(0.0f)));
            Assert.AreEqual("1.5", EncodeSimple(e => e.WriteNumber(1.5f)));
            Assert.AreEqual("1000000", EncodeSimple(e => e.WriteNumber(1.0e6f)));
            Assert.AreEqual("-1000000", EncodeSimple(e => e.WriteNumber(-1.0e6f)));

            Assert.AreEqual(10.68728f, JSONDecoder.Decode(EncodeSimple(e => e.WriteNumber(10.68728f))).FloatValue);
            Assert.AreEqual(-10.68728f, JSONDecoder.Decode(EncodeSimple(e => e.WriteNumber(-10.68728f))).FloatValue);
        }

        [Test]
        public void ULong() {
            Assert.AreEqual("123", EncodeSimple(e => e.WriteNumber(123UL)));
            Assert.AreEqual("18446744073709551615", EncodeSimple(e => e.WriteNumber(UInt64.MaxValue)));

            Assert.AreEqual("18446744073709551615",
                            EncodeSimple(e => e.WriteJObject(JObject.CreateNumber(false, false, false, 18446744073709551615, 0, 0, 0))));
        }

        [Test]
        public void Null() {
            Assert.AreEqual("null", EncodeSimple(e => e.WriteNull()));
            Assert.AreEqual("null", EncodeSimple(e => e.WriteJObject(JObject.CreateNull())));
        }

        [Test]
        public void Boolean() {
            Assert.AreEqual("true", EncodeSimple(e => e.WriteBool(true)));
            Assert.AreEqual("false", EncodeSimple(e => e.WriteBool(false)));

            Assert.AreEqual("true", EncodeSimple(e => e.WriteJObject(JObject.CreateBoolean(true))));
            Assert.AreEqual("false", EncodeSimple(e => e.WriteJObject(JObject.CreateBoolean(false))));
        }

        [Test]
        public void Array() {
            Assert.AreEqual("[1,2,3]", EncodeSimple(e => {
                e.BeginArray();
                e.WriteNumber(1);
                e.WriteNumber(2);
                e.WriteNumber(3);
                e.EndArray();
            }));
            Assert.AreEqual("[[],\"str\",1.5]", EncodeSimple(e => {
                e.BeginArray();
                e.BeginArray();
                e.EndArray();
                e.WriteString("str");
                e.WriteNumber(1.5);
                e.EndArray();
            }));

            Assert.AreEqual("[1,2,3]",
                            EncodeSimple(e => e.WriteJObject(JObject.CreateArray(new List<JObject> {
                                                                     JObject.CreateNumber(false, false, false, 1, 0, 0, 0),
                                                                     JObject.CreateNumber(false, false, false, 2, 0, 0, 0),
                                                                     JObject.CreateNumber(false, false, false, 3, 0, 0, 0)
                                                                 }))));
            Assert.AreEqual("[[],\"str\",1.5]",
                            EncodeSimple(e => e.WriteJObject(JObject.CreateArray(new List<JObject> {
                                                                     JObject.CreateArray(new List<JObject>()),
                                                                     JObject.CreateString("str"),
                                                                     JObject.CreateNumber(false, true, false, 1, 5, 1, 0)
                                                                 }))));
        }

        [Test]
        public void Dictionary() {
            Assert.AreEqual("{\"X\":10,\"Y\":20}", EncodeSimple(e => {
                e.BeginObject();
                e.WriteKey("X");
                e.WriteNumber(10);
                e.WriteKey("Y");
                e.WriteNumber(20);
                e.EndObject();
            }));

            Assert.AreEqual("{\"X\":10,\"Y\":20}",
                            EncodeSimple(e => e.WriteJObject(JObject.CreateObject(new Dictionary<string, JObject> {
                                                                      { "X", JObject.CreateNumber(false, false, false, 10, 0, 0, 0) },
                                                                      { "Y", JObject.CreateNumber(false, false, false, 20, 0, 0, 0) }
                                                                  }))));
        }

        [Test]
        public void Nesting() {
            Assert.AreEqual("{\"array\":[{\"a\":true},{\"a\":false}]}", EncodeSimple(e => {
                e.BeginObject();     // {
                e.WriteKey("array");
                e.BeginArray();      // [
                e.BeginObject();     // {
                e.WriteKey("a");
                e.WriteBool(true);
                e.EndObject();       // }
                e.BeginObject();     // {
                e.WriteKey("a");
                e.WriteBool(false);
                e.EndObject();       // }
                e.EndArray();        // ]
                e.EndObject();       // }
            }));
        }

        [Test]
        public void Indentation() {
            Assert.AreEqual("{\n \"array\":[\n  {\"a\":true},\n  {\"a\":false}\n ]\n}", EncodeSimple(e => {
                e.BeginObject();     // {
                e.InsertNewline();
                e.WriteKey("array");
                e.BeginArray();      // [
                e.InsertNewline();
                e.BeginObject();     // {
                e.WriteKey("a");
                e.WriteBool(true);
                e.EndObject();       // }
                e.InsertNewline();
                e.BeginObject();     // {
                e.WriteKey("a");
                e.WriteBool(false);
                e.EndObject();       // }
                e.InsertNewline();
                e.EndArray();        // ]
                e.InsertNewline();
                e.EndObject();       // }
            }));
        }

        private string EncodeSimple(Action<JSONStreamEncoder> callback) {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            var encoder = new JSONStreamEncoder(writer);
            callback(encoder);
            writer.Flush();
            return writer.GetStringBuilder().ToString();
        }
    }
}
