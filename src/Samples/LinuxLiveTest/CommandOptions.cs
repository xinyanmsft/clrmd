// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommandLine;

namespace mempeek
{
    internal class CommandOptions
    {
        [Option('p', "pid", Required = true, HelpText = "Process Id. Requires SYS_PTRACE capability on Linux.")]
        public int ProcessId { get; set; }

        [Option('s', "suspend", Required = false, Default = false, HelpText = "Suspend the target process to get a better result.")]
        public bool SuspendTarget { get; set; }

        [Option("top-by-count", Required = false, Default = 0, HelpText = "List top N types ordered by instance count.")]
        public int TopByCount { get; set; }

        [Option("top-by-size", Required = false, Default = 0, HelpText = "List top N types ordered by allocate size.")]
        public int TopBySize { get; set; }

        [Option("json", Required = false, HelpText = "Output data in json format.")]
        public bool OutputJson { get; set; }
    }
}
