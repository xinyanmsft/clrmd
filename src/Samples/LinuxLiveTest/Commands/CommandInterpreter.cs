// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading;

namespace mempeek.Commands
{
    public class CommandInterpreter
    {
        private CommandContext _context;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isExecuting;

        public CommandInterpreter(CommandContext context)
        {
            _context = context;
        }

        public int Run()
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            while (true)
            {
                Console.Write("(mempeek)>  ");
                string commandArgs = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(commandArgs))
                {
                    continue;
                }
                string[] args = commandArgs.Split(' ', StringSplitOptions.None);
                var command = CommandFactory.Default.Create(args);
                if (command == null)
                {
                    break;
                }
                _cancellationTokenSource = new CancellationTokenSource();
                _context.CancellationToken = _cancellationTokenSource.Token;
                _context.Args = args.Skip(1).ToArray();

                _isExecuting = true;
                command.Execute(_context);
                _isExecuting = false;
            }
            return 0;
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_isExecuting)
            {
                _cancellationTokenSource?.Cancel();
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }
    }
}
