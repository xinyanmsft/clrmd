// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using mempeek.Commands;
using Microsoft.Diagnostics.Runtime;

namespace mempeek
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CommandOptions>(args).MapResult(
                (CommandOptions opts) =>
                {
                    return Execute(opts);
                },
                errs => 1);
        }

        private static int Execute(CommandOptions opts)
        {
            OutputLogger logger = new OutputLogger(opts.OutputJson ? OutputType.Json : OutputType.Text);
            try
            {
                using (var dataTarget = DataTargetFactory.Default.CreateFromProcess(opts.ProcessId, opts.SuspendTarget))
                {
                    var clrInfo = dataTarget.DataTarget.ClrVersions.First();
                    var runtime = clrInfo.CreateRuntime();
                    var heap = runtime.Heap;
                    if (!heap.CanWalkHeap)
                    {
                        // GC is actively collecting and can't walk the heap at this point. This should happen rare. 
                        logger.Log("The GC heap is invalid at this point (heap.CanWalkHeap == false). Please retry later.");
                    }

                    if (opts.TopByCount > 0 || opts.TopBySize > 0)
                    {
                        Dictionary<string, HeapTypeInfo> typeInfo = new Dictionary<string, HeapTypeInfo>();
                        foreach (ClrObject obj in heap.EnumerateObjects())
                        {
                            HeapTypeInfo hti;
                            if (!typeInfo.TryGetValue(obj.Type.Name, out hti))
                            {
                                hti = new HeapTypeInfo();
                                hti.TypeName = obj.Type.Name;
                                hti.MethodTable = obj.Type.MethodTable;
                                typeInfo.Add(obj.Type.Name, hti);
                            }
                            hti.TotalSize += (long) obj.Size;
                            hti.ObjectCount++;
                        }
                        var orderByCountList = typeInfo.OrderByDescending(t => t.Value.ObjectCount).Take(opts.TopByCount).Select(t => t.Value);
                        if (orderByCountList.Any())
                        {
                            logger.Log(orderByCountList);
                        }
                        var orderBySizeList = typeInfo.OrderByDescending(t => t.Value.TotalSize).Take(opts.TopBySize).Select(t => t.Value);
                        if (orderBySizeList.Any())
                        {
                            logger.Log(orderBySizeList);
                        }
                        return 0;
                    }
                    else
                    {
                        CommandContext context = new CommandContext(dataTarget.DataTarget, heap, logger);
                        CommandInterpreter interpreter = new CommandInterpreter(context);
                        return interpreter.Run();
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Log(ex.Message);
                return -1;
            }
        }
    }
}
