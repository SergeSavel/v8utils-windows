// Copyright 2020 Sergey Savelev
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace SSavel.V8Utils.Windows.Platform.RemoteAdmin
{
    internal static class Parsers
    {
        internal static string TryParseString(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            return !dict.TryGetValue(key, out var stringValue) ? default : stringValue;
        }

        internal static int TryParseInt(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? int.Parse(stringValue) : default;
        }

        internal static uint TryParseUInt(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? uint.Parse(stringValue) : default;
        }

        internal static long TryParseLong(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? long.Parse(stringValue) : default;
        }

        internal static ulong TryParseULong(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? ulong.Parse(stringValue) : default;
        }

        internal static DateTime TryParseDateTime(IDictionary<string, string> dict, string key,
            bool throwIfNotFound = true)
        {
            if (!dict.TryGetValue(key, out var stringValue)) return default;
            return !string.IsNullOrWhiteSpace(stringValue) ? DateTime.Parse(stringValue) : default;
        }

        internal static bool TryParseBool(IDictionary<string, string> dict, string key, bool throwIfNotFound = true)
        {
            return !dict.TryGetValue(key, out var stringValue)
                ? default
                : "yes".Equals(stringValue, StringComparison.OrdinalIgnoreCase);
        }
    }
}