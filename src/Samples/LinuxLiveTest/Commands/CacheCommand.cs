// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace mempeek.Commands
{
    public class CacheCommand : ICommand
    {
        public void Execute(CommandContext context)
        {
            var gcRoot = context.GCRoot;
            if (gcRoot.IsFullyCached)
            {
                context.Logger.Log("GCRoot is already cached.");
            }
            else
            {
                try
                {
                    gcRoot.BuildCache(context.CancellationToken);
                    context.Logger.Log("GCRoot is cached.");
                }
                catch (OperationCanceledException)
                {
                    context.Logger.Log("Operation cancelled.");
                }
            }
        }
    }
}
