// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading;
using Microsoft.Diagnostics.Runtime;

namespace mempeek.Commands
{
    public class CommandContext
    {
        private GCRoot _gcRoot;

        public CommandContext(DataTarget dataTarget, ClrHeap heap, OutputLogger logger)
        {
            this.DataTarget = dataTarget;
            this.Logger = logger;
            this.Heap = heap;
        }

        public DataTarget DataTarget { get; private set; }

        public OutputLogger Logger { get; private set; }

        public ClrHeap Heap { get; private set; }

        public string[] Args { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public GCRoot GCRoot
        {
            get
            {
                if (_gcRoot == null)
                {
                    _gcRoot = new GCRoot(this.Heap);
                }
                return _gcRoot;
            }
        }
    }
}
