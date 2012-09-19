using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SimpleJSON {
    public class JSONStreamEncoder {
        private struct EncoderContext {
            public bool IsObject;
            public bool IsEmpty;

            public EncoderContext(bool isObject, bool isEmpty) {
                IsObject = isObject;
                IsEmpty = isEmpty;
            }
        }

        private TextWriter _writer;
        private Stack<EncoderContext> _contextStack;

        public JSONStreamEncoder(TextWriter writer, int expectedNesting = 20) {
            _writer = writer;
            _contextStack = new Stack<EncoderContext>(expectedNesting);
        }

        public void BeginArray() {
            WriteSeparator();
            _contextStack.Push(new EncoderContext(false, true));
            _writer.Write('[');
        }

        public void EndArray() {
            if (_contextStack.Count == 0) {
                throw new InvalidDataException("EndArray called without BeginArray");
            }
            var ctx = _contextStack.Pop();

            if (ctx.IsObject) {
                throw new InvalidDataException("EndArray called after BeginObject");
            }

            _writer.Write(']');
        }

        public void BeginObject() {
            WriteSeparator();
            _contextStack.Push(new EncoderContext(true, true));
            _writer.Write('{');
        }

        public void EndObject() {
            if (_contextStack.Count == 0) {
                throw new InvalidDataException("EndObject called without BeginObject");
            }

            var ctx = _contextStack.Pop();
            if (!ctx.IsObject) {
                throw new InvalidDataException("EndObject called after BeginArray");
            }

            _writer.Write('}');
        }

        public void WriteString(string str) {
            WriteSeparator();
            WriteBareString(str);
        }

        public void WriteKey(string str) {
            if (_contextStack.Count == 0) {
                throw new InvalidDataException("WriteKey called without BeginObject");
            }

            WriteSeparator();
            WriteBareString(str);
            _writer.Write(':');

            var ctx = _contextStack.Pop();

            if (!ctx.IsObject) {
                throw new InvalidDataException("WriteKey called after BeginArray");
            }

            ctx.IsEmpty = true;
            _contextStack.Push(ctx);
        }

        public void WriteNumber(long l) {
            WriteSeparator();
            _writer.Write(l.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(ulong l) {
            WriteSeparator();
            _writer.Write(l.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(float f) {
            WriteSeparator();
            _writer.Write(f.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(double d) {
            WriteSeparator();
            _writer.Write(d.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNull() {
            WriteSeparator();
            _writer.Write("null");
        }

        public void WriteBool(bool b) {
            WriteSeparator();
            _writer.Write(b ? "true" : "false");
        }

        public void WriteJObject(JObject obj) {
            switch (obj.Kind) {
            case JObjectKind.Array:
                BeginArray();
                foreach (var elem in obj.ArrayValue) {
                    WriteJObject(elem);
                }
                EndArray();
                break;
            case JObjectKind.Boolean:
                WriteBool(obj.BooleanValue);
                break;
            case JObjectKind.Null:
                WriteNull();
                break;
            case JObjectKind.Number:
                if (obj.IsFractional) {
                    WriteNumber(obj.DoubleValue);
                } else if (obj.IsNegative) {
                    WriteNumber(obj.LongValue);
                } else {
                    WriteNumber(obj.ULongValue);
                }
                break;
            case JObjectKind.Object:
                BeginObject();
                foreach (var pair in obj.ObjectValue) {
                    WriteKey(pair.Key);
                    WriteJObject(pair.Value);
                }
                EndObject();
                break;
            case JObjectKind.String:
                WriteString(obj.StringValue);
                break;
            }
        }

        private void WriteBareString(string str) {
            _writer.Write('"');
            foreach (var c in str) {
                if (JSONEncoder.EscapeChars.ContainsKey(c)) {
                    _writer.Write(JSONEncoder.EscapeChars[c]);
                } else {
                    if (c > 0x80 || c < 0x20) {
                        _writer.Write("\\u" + Convert.ToString(c, 16)
                                                .ToUpper(CultureInfo.InvariantCulture)
                                                .PadLeft(4, '0'));
                    } else {
                        _writer.Write(c);
                    }
                }
            }
            _writer.Write('"');
        }

        private void WriteSeparator() {
            if (_contextStack.Count == 0) return;

            var ctx = _contextStack.Pop();
            if (!ctx.IsEmpty) {
                _writer.Write(',');
            }

            ctx.IsEmpty = false;
            _contextStack.Push(ctx);
        }
    }
}
