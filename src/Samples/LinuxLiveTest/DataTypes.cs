// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Newtonsoft.Json;

namespace mempeek
{
    public class HeapTypeInfo
    {
        [JsonProperty(Order = 10)]
        public ulong MethodTable { get; set; }

        [JsonProperty(Order = 20)]
        public int ObjectCount { get; set; }

        [JsonProperty(Order = 30)]
        public long TotalSize { get; set; }

        [JsonProperty(Order = 40)]
        public string TypeName { get; set; }
    }
}
