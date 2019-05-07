// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace mempeek.Commands
{
    class DumpObjCommand : ICommand
    {
        public void Execute(CommandContext context)
        {
            if (!context.Args.Any())
            {
                context.Logger.Log("Please specify instance address.");
                return;
            }
            foreach (var address in context.Args)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                this.PrintObject(context, address);
            }
        }
        private void PrintObject(CommandContext context, string addressStr)
        {
            ulong address;
            try
            {
                address = ulong.Parse(addressStr, System.Globalization.NumberStyles.HexNumber);
            }
            catch(Exception)
            {
                context.Logger.Log($"{addressStr} is not a valid address.");
                return;
            }
            ClrObject obj = context.Heap.GetObject(address);
            if (obj.Type == null)
            {
                context.Logger.Log($"{addressStr} is not an object.");
                return;
            }
            context.Logger.Log(new ObjectInfo()
            {
                Address = address,
                Type = obj.Type.Name,
                Size = (int) obj.Size
            });
            List<FieldInfo> fields = new List<FieldInfo>();
            foreach(var f in obj.Type.Fields)
            {
                fields.Add(new FieldInfo()
                {
                    Name = f.Name,
                    Type = f.Type.Name,
                    Value = GetValueString(f.GetValue(address))
                });
            }
            context.Logger.Log(fields);
        }

        private string GetValueString(object v)
        {
            if (v == null)
            {
                return "null";
            }
            else
            {
                return v.ToString();
            }
        }
    }
}
