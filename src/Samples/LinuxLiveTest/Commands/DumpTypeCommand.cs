// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace mempeek.Commands
{
    public class DumpTypeCommand : ICommand
    {
        public void Execute(CommandContext context)
        {
            if (!context.Args.Any())
            {
                context.Logger.Log("Please specify type name or method table value.");
                return;
            }
            foreach(var nameOrAddress in context.Args)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                this.PrintType(context, nameOrAddress);
            }
        }

        private void PrintType(CommandContext context, string nameOrAddress)
        {
            ulong address = 0;
            string typeName = string.Empty;
            try
            {
                address = ulong.Parse(nameOrAddress, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                typeName = nameOrAddress;
            }
            ClrType type = address == 0 ? context.Heap.GetTypeByName(typeName) : context.Heap.GetTypeByMethodTable(address);
            if (type == null)
            {
                context.Logger.Log($"Type {nameOrAddress} can't be found.");
                return;
            }
            context.Logger.Log(new TypeInfo()
            {
                MT = type.MethodTable,
                Name = type.Name,
                Size = type.BaseSize
            });
            foreach(var obj in context.Heap.EnumerateObjects())
            {
                if (obj.Type.Name == type.Name)
                {
                    context.Logger.Log(new ObjectInstanceInfo()
                    {
                        Address = obj.Address,
                        Size = (int)obj.Size
                    });
                }
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
