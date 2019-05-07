// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace mempeek.Commands
{
    public class HeapStatCommand : ICommand
    {
        public void Execute(CommandContext context)
        {
            Dictionary<string, HeapTypeInfo> typeInfo = new Dictionary<string, HeapTypeInfo>();
            foreach (ClrObject obj in context.Heap.EnumerateObjects())
            {
                HeapTypeInfo hti;
                if (!typeInfo.TryGetValue(obj.Type.Name, out hti))
                {
                    hti = new HeapTypeInfo();
                    hti.TypeName = obj.Type.Name;
                    hti.MethodTable = obj.Type.MethodTable;
                    typeInfo.Add(obj.Type.Name, hti);
                }
                hti.TotalSize += (long)obj.Size;
                hti.ObjectCount++;
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
            var orderBySizeList = typeInfo.OrderBy(t => t.Value.TotalSize).Select(t => t.Value);
            context.Logger.Log(orderBySizeList);
        }
    }
}
