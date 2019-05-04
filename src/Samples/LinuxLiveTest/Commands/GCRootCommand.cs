// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace mempeek.Commands
{
    public class GCRootCommand : ICommand
    {
        public void Execute(CommandContext context)
        {
            if (!context.Args.Any())
            {
                context.Logger.Log("Please specify instance address.");
                return;
            }
            ulong address;
            try
            {
                address = ulong.Parse(context.Args[0], System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                context.Logger.Log($"{context.Args[0]} is not a valid address.");
                return;
            }
            this.PrintGCRoot(context, address);
        }

        private void PrintGCRoot(CommandContext context, ulong address)
        {
            GCRoot gcRoot = context.GCRoot;
            context.Heap.StackwalkPolicy = ClrRootStackwalkPolicy.Exact;

            foreach (var rootPath in gcRoot.EnumerateGCRoots(address, context.CancellationToken))
            {
                StringBuilder s = new StringBuilder();
                s.Append($"{rootPath.Root} -> ");                
                s.Append(string.Join(" -> ", rootPath.Path.Select(obj => $"{obj.HexAddress} {obj.Type.Name}")));
            }
        }
    }
}
