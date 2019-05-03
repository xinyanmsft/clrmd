// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Newtonsoft.Json;

namespace mempeek
{
    public class OutputLogger
    {
        private OutputType _outputType;

        public OutputLogger(OutputType outputType)
        {
            _outputType = outputType;
        }

        public void Log(string message)
        {
            this.Log<StringMessage>(new StringMessage()
            {
                Message = message
            });
        }

        public void Log<T>(T value) where T : class
        {
            switch (_outputType)
            {
                case OutputType.Json:
                    Console.WriteLine(JsonConvert.SerializeObject(value));
                    break;
                case OutputType.Text:
                    if (value is StringMessage)
                    {
                        Console.WriteLine(((StringMessage)(object)value).Message);
                    }
                    else
                    {
                        this.PrettyPrint(value);
                    }
                    break;
            }
        }

        private void PrettyPrint<T>(T value) where T : class
        {
            System.Collections.IEnumerable list = value as System.Collections.IEnumerable;
            if (list == null)
            {
                Console.WriteLine(JsonConvert.SerializeObject(value));
                return;
            }
            var enumerator = list.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                // empty list
                Console.WriteLine();
                return;
            }
            Type type = enumerator.Current.GetType();
            var properties = type.GetProperties().Where(p =>
            {
                var ignores = p.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true);
                return !ignores.Any();
            }).OrderBy(p =>
            {
                var attrs = p.GetCustomAttributes(typeof(JsonPropertyAttribute), inherit: true);
                foreach (var attr in attrs)
                {
                    JsonPropertyAttribute jpa = attr as JsonPropertyAttribute;
                    if (jpa != null)
                        return jpa.Order;
                }
                return 999;
            }).ToArray();

            foreach(var item in list)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    string s;
                    var v = properties[i].GetValue(item);
                    if (v == null)
                    {
                        s = "null";
                    }
                    else if (v.GetType() == typeof(ulong))
                    {
                        // hack: ulong got printed as 64 bit HEX
                        s = ((ulong)v).ToString("X16");
                    }
                    else
                    {
                        s = v.ToString();
                    }
                    if (i > 0)
                    {
                        Console.Write('\t');
                    }
                    Console.Write(s);
                }
                Console.WriteLine();
            }
        }

        internal class StringMessage
        {
            public string Message { get; set; }
        }
    }

    public enum OutputType
    {
        Text,
        Json
    }
}
