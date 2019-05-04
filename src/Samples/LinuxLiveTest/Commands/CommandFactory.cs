// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace mempeek.Commands
{
    public interface ICommand
    {
        void Execute(CommandContext context);
    }

    public class CommandFactory
    {
        public static CommandFactory Default = new CommandFactory();

        public CommandFactory()
        { }

        public ICommand Create(string[] args)
        {
            if (!args.Any())
            {
                return new HelpCommand();
            }
            string cmd = args[0].ToLowerInvariant();
            switch(cmd)
            {
                case "heapstat":
                    return new HeapStatCommand();

                case "dumptype":
                    return new DumpTypeCommand();

                case "dumpobj":
                    return new DumpObjCommand();

                case "gcroot":
                    return new GCRootCommand();

                case "help":
                    return new HelpCommand();

                case "quit":
                    return null;

                case "cache":
                    return new CacheCommand();

                default:
                    return new UnknownCommand();
            }
        }
    }
}
