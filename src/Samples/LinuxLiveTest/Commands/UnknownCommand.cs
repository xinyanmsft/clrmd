// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace mempeek.Commands
{
    public class UnknownCommand : HelpCommand
    {
        public override void Execute(CommandContext context)
        {
            context.Logger.Log("Unknown command.");
            base.Execute(context);
        }
    }
}
