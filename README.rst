SimpleJSON for .NET
===================

SimpleJSON aims to be a small and fuss-free library for parsing and
generating JSON from the .NET languages. It is /not/ a replacement for
BinaryFormatter, XMLSerializer or JSON.NET, since it won't traverse
your object graph trying to be clever.

It supports serialization of most primitive datatypes (strings,
numbers and boolean with the exception of decimal), as well as
instances of ``IDictionary`` to objects and ``IEnumerable`` to arrays.

When deserializing, it will build a tree of JObject instances; a small
class that represents the data types that JSON supports.

Usage
-----

To serialize something, use ``JSONEncoder.Encode(object)``::

    JSONEncoder.Encode(new[] { 1, 2, 3}); // = [1,2,3]
    JSONEncoder.Encode(new Dictionary<string, object>
                           { {"key", "value"}, {"other", 123} });
                       // = {"key":"value","other":123}

To deserialize something, use ``JSONDecoder.Decode(string)``::

    JObject obj = JSONDecoder.Decode("[1,2,3]");
    // Cast the JObject to the primitive type you're looking for
    Debug.Assert((int)obj[0] == 1);
    Debug.Assert((int)obj[1] == 2);
    Debug.Assert((int)obj[2] == 3);

    obj = JSONDecoder.Decode("{\"key\": {\"nested\": {\"more\": 123} }  }");
    // Nested objects are accessed with ease
    Debug.Assert((int)obj["key"]["nested"]["more"] == 123);

If decode encounters an invalid string, a ``ParseError`` is thrown
with a position indicator where it went wrong.

For numbers ``JObject`` populates all fields big enough to hold the
parsed number. For example;

 * the number "150" will populate all number
   fields except ``sbyte`` (which has a upper bound of +127), and the
   ``MinInteger`` field will be set to ``Int8``.
 * -1500000 will only populate the signed fields ``int`` and
   ``long``.
 * the number "150.5" will only populate the ``float`` and ``double``
   fields, since they are the only ones that can represent fractional
   numbers.

This means that you don't have to worry about if the original JSON
representation had an integral or fractional value; just use the type
you are looking for. If you want to check for overflows, use the
``IsNegative``, ``MinInteger`` and ``MinFloat`` field to determine if
the parsed number was bigger than what your data type can fit.

License
-------

SimpleJSON.NET is licensed under a 3-Clause BSD License:

Copyright (c) 2011, Boldai AB
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
 * Neither the name of the <organization> nor the
   names of its contributors may be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE
