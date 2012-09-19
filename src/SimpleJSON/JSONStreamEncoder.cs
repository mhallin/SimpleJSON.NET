using System;
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
        private EncoderContext[] _contextStack;
        private int _contextStackPointer = -1;

        public JSONStreamEncoder(TextWriter writer, int expectedNesting = 20) {
            _writer = writer;
            _contextStack = new EncoderContext[expectedNesting];
        }

        public void BeginArray() {
            WriteSeparator();
            PushContext(new EncoderContext(false, true));
            _writer.Write('[');
        }

        public void EndArray() {
            if (_contextStackPointer == -1) {
                throw new InvalidOperationException("EndArray called without BeginArray");
            } else if (_contextStack[_contextStackPointer].IsObject) {
                throw new InvalidOperationException("EndArray called after BeginObject");
            }

            PopContext();
            _writer.Write(']');
        }

        public void BeginObject() {
            WriteSeparator();
            PushContext(new EncoderContext(true, true));
            _writer.Write('{');
        }

        public void EndObject() {
            if (_contextStackPointer == -1) {
                throw new InvalidOperationException("EndObject called without BeginObject");
            } else if (!_contextStack[_contextStackPointer].IsObject) {
                throw new InvalidOperationException("EndObject called after BeginArray");
            }

            PopContext();
            _writer.Write('}');
        }

        public void WriteString(string str) {
            WriteSeparator();
            WriteBareString(str);
        }

        public void WriteKey(string str) {
            if (_contextStackPointer == -1) {
                throw new InvalidOperationException("WriteKey called without BeginObject");
            } else if (!_contextStack[_contextStackPointer].IsObject) {
                throw new InvalidOperationException("WriteKey called after BeginArray");
            }

            WriteSeparator();
            WriteBareString(str);
            _writer.Write(':');

            _contextStack[_contextStackPointer].IsEmpty = true;
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
            if (_contextStackPointer == -1) return;

            if (!_contextStack[_contextStackPointer].IsEmpty) {
                _writer.Write(',');
            }

            _contextStack[_contextStackPointer].IsEmpty = false;
        }

        private void PushContext(EncoderContext ctx) {
            if (_contextStackPointer + 1 == _contextStack.Length) {
                throw new StackOverflowException("Too much nesting for context stack, increase expected nesting when creating the encoder");
            }

            _contextStack[++_contextStackPointer] = ctx;
        }

        private void PopContext() {
            if (_contextStackPointer == -1) {
                throw new InvalidOperationException("Stack underflow");
            }

            --_contextStackPointer;
        }
    }
}
