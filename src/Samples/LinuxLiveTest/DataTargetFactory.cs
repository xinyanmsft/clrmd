// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime;

namespace mempeek
{
    public class DataTargetFactory
    {
        public static DataTargetFactory Default = new DataTargetFactory();

        public DataTargetFactory()
        { }

        public DataTargetWrapper CreateFromProcess(int processId, bool suspend)
        {
            if (suspend)
            {
                // send SIGSTOP signal to the target process.
                kill(processId, SIGSTOP);
            }

            // "ptrace attach" to the target process 
            ulong ret = ptrace(PTRACE_ATTACH, processId, IntPtr.Zero, IntPtr.Zero);
            if (ret != 0)
            {
                throw new InvalidOperationException($"ptrace attach failed with 0x{ret:x}. Please ensure SYS_PTRACE capability is enabled. When running inside a Docker container, add 'SYS_PTRACE' to securityContext.capabilities.");
            }
            // wait till ptrace attach takes effect.
            int ret2 = wait(IntPtr.Zero);
            if (ret2 != processId)
            {
                throw new InvalidOperationException($"wait failed with {ret2}. is process {processId} still running?");
            }

            var dataTarget = DataTarget.AttachToProcess(processId, 0, AttachFlag.Passive);
            return new DataTargetWrapper(dataTarget, suspend);
        }

        public class DataTargetWrapper : IDisposable
        {
            private bool _suspended;

            public DataTargetWrapper(DataTarget dataTarget, bool suspended)
            {
                this.DataTarget = dataTarget;
                _suspended = suspended;
            }

            public DataTarget DataTarget { get; private set; }

            public void Dispose()
            {
                if (_suspended)
                {
                    _suspended = false;
                    kill((int) this.DataTarget.ProcessId, SIGCONT);
                }
                this.DataTarget?.Dispose();
                this.DataTarget = null;
            }
        }

        [DllImport("libc", SetLastError = true)]
        private static extern ulong ptrace(uint command, int pid, IntPtr addr, IntPtr data);

        [DllImport("libc", SetLastError = true)]
        private static extern uint kill(int pid, int signal);

        [DllImport("libc", SetLastError = true)]
        private static extern int wait(IntPtr status);

        private const int SIGSTOP = 17;
        private const int SIGCONT = 19;
        private const int PTRACE_ATTACH = 16;
    }
}
