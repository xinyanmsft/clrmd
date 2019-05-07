// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace mempeek.Commands
{
    public class HelpCommand : ICommand
    {
        public virtual void Execute(CommandContext context)
        {
            context.Logger.Log("heapstat:\tshows heap summary");
            context.Logger.Log("dumptype:\tshows type details");
            context.Logger.Log("dumpobj:\tshows object value");
            context.Logger.Log("gcroot:\t\tcomputes path to root");
            context.Logger.Log("cache:\t\tbuilds GC root cache");
            context.Logger.Log("quit:\t\tquit this tool");
        }
    }
}
