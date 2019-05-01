// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace Microsoft.Diagnostics.Runtime.Linux
{
    /// <summary>
    /// A data reader targets a Linux process, implemented by reading /proc/<pid>/maps 
    /// and /proc/<pid>/mem files.
    /// </summary>
    internal class LinuxLiveDataReader : IDataReader2
    {
        private List<MemoryMapEntry> _memoryMapEntries;
        private FileStream _memoryStream;

        public LinuxLiveDataReader(uint processId)
        {
            this.ProcessId = processId;
            _memoryMapEntries = this.LoadMemoryMap();
        }

        public uint ProcessId { get; private set; }

        public bool IsMinidump { get { return false; } }

        public void Close()
        {
            _memoryStream?.Dispose();
            _memoryStream = null;
        }

        public void Flush()
        {
        }

        public Architecture GetArchitecture()
        {
            return IntPtr.Size == 4 ? Architecture.X86 : Architecture.Amd64;
        }

        public uint GetPointerSize()
        {
            return (uint)IntPtr.Size;            
        }

        public IList<ModuleInfo> EnumerateModules()
        {
            List<ModuleInfo> result = new List<ModuleInfo>();
            foreach(var entry in _memoryMapEntries)
            {
                if (string.IsNullOrEmpty(entry.FilePath))
                {
                    continue;
                }
                var module = result.FirstOrDefault(m => m.FileName == entry.FileName);
                if (module == null)
                {
                    var fileInfo = new FileInfo(entry.FilePath);
                    ModuleInfo moduleInfo = new ModuleInfo(this)
                    {
                        ImageBase = entry.BeginAddr,
                        FileName = entry.FileName,
                        FileSize = (uint) fileInfo.Length,
                        TimeStamp = (uint) new DateTimeOffset(fileInfo.CreationTimeUtc).ToUnixTimeSeconds()
                    };
                    result.Add(moduleInfo);
                }
            }
            return result;
        }

        public void GetVersionInfo(ulong addr, out VersionInfo version)
        {
            foreach(var entry in _memoryMapEntries)
            {
                if (addr >= entry.BeginAddr && addr <= entry.EndAddr && !string.IsNullOrEmpty(entry.FilePath))
                {
                    Version v = null;
                    int i1 = entry.FileName.LastIndexOf(".so.");
                    if (i1 > 0)
                    {
                        v = this.ParseForVersion(entry.FileName.Substring(i1 + 4));
                    }
                    if ( v == null)
                    {
                        string dirName = Path.GetFileName(Path.GetDirectoryName(entry.FilePath));
                        v  = this.ParseForVersion(dirName);
                    }
                    if (v != null)
                    {
                        version = new VersionInfo(v.Major, v.Minor, v.Build, v.Revision);
                        return;
                    }
                }
            }
            version = new VersionInfo();
        }

        public bool ReadMemory(ulong address, byte[] buffer, int bytesRequested, out int bytesRead)
        {
            this.OpenMemFile();
            bytesRead = 0;
            try
            {
                _memoryStream.Seek((long) address, SeekOrigin.Begin);
                bytesRead = _memoryStream.Read(buffer, 0, bytesRequested);
                return bytesRead > 0;
            }
            catch(Exception )
            {
                return false;
            }
        }

        public bool ReadMemory(ulong address, IntPtr buffer, int bytesRequested, out int bytesRead)
        {
            this.OpenMemFile();
            bytesRead = 0;
            byte[] bytes = new byte[bytesRequested];
            try
            {
                _memoryStream.Seek((long) address, SeekOrigin.Begin);
                bytesRead = _memoryStream.Read(bytes, 0, bytesRequested);
                if (bytesRead > 0)
                {
                    Marshal.Copy(bytes, 0, buffer, bytesRead);
                }
                return bytesRead > 0;
            }
            catch(Exception )
            {
                return false;
            }
        }

        public ulong ReadPointerUnsafe(ulong address)
        {
            byte[] ptrBuffer = new byte[IntPtr.Size];
            if (!ReadMemory(address, ptrBuffer, IntPtr.Size, out int read))
            {
                return 0;
            }
            return IntPtr.Size == 4 ? BitConverter.ToUInt32(ptrBuffer, 0) : BitConverter.ToUInt64(ptrBuffer, 0);
        }

        public uint ReadDwordUnsafe(ulong address)
        {
            byte[] ptrBuffer = new byte[4];
            if (!ReadMemory(address, ptrBuffer, 4, out int read))
            {
                return 0;
            }
            return BitConverter.ToUInt32(ptrBuffer, 0);
        }

        public ulong GetThreadTeb(uint thread)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<uint> EnumerateAllThreads()
        {
            throw new NotImplementedException();
        }

        public bool VirtualQuery(ulong addr, out VirtualQueryData vq)
        {
            foreach(var entry in _memoryMapEntries)
            {
                if (entry.BeginAddr <= addr && entry.EndAddr >= addr)
                {
                    vq = new VirtualQueryData(entry.BeginAddr, entry.EndAddr - entry.BeginAddr + 1);
                    return true;
                }
            }
            vq = new VirtualQueryData();
            return false;
        }

        public bool GetThreadContext(uint threadID, uint contextFlags, uint contextSize, IntPtr context)
        {
            throw new NotImplementedException();
        }

        public bool GetThreadContext(uint threadID, uint contextFlags, uint contextSize, byte[] context)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<string> GetModulesFullPath()
        {
            return _memoryMapEntries.Where(e => !string.IsNullOrEmpty(e.FilePath)).Select(e => e.FilePath).Distinct();
        }

        private void OpenMemFile()
        {
            if (_memoryStream != null)
            {
                return;
            }
            _memoryStream = File.OpenRead($"/proc/{this.ProcessId}/mem");
        }

        private Version ParseForVersion(string s)
        {
            StringBuilder b = new StringBuilder();
            string[] parts = s.Split(new char[]{'-', '.', ','}, StringSplitOptions.RemoveEmptyEntries);
            foreach(var p in parts)
            {
                int i;
                if (int.TryParse(p, out i))
                {
                    if (b.Length > 0)
                    {
                        b.Append('.');
                    }
                    b.Append(p);
                }
            }
            Version v = null;
            Version.TryParse(b.ToString(), out v);
            if (v != null)
            {
                if (v.Major < 0)
                {
                    v = new Version(0, v.Minor, v.Build, v.Revision);
                }
                if (v.Minor < 0)
                {
                    v = new Version(v.Major, 0, v.Build, v.Revision);
                }
                if (v.Build < 0)
                {
                    v = new Version(v.Major, v.Minor, 0, v.Revision);
                }
                if (v.Revision < 0)
                {
                    v = new Version(v.Major, v.Minor, v.Build, 0);
                }
            }
            return v;
        }

        private List<MemoryMapEntry> LoadMemoryMap()
        {
            List<MemoryMapEntry> result = new List<MemoryMapEntry>();
            string mapsFilePath = $"/proc/{this.ProcessId}/maps";
            using(FileStream fs = File.OpenRead(mapsFilePath))
            using(StreamReader sr = new StreamReader(fs))
            {
                while(true)
                {
                    string line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }
                    string address, permission, offset, dev, inode, path;
                    string[] parts = line.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 5)
                    {
                        path = string.Empty;
                    }
                    else if (parts.Length == 6)
                    {
                        path = parts[5].StartsWith("[") ? string.Empty : parts[5];
                    }
                    else
                    {
                        // Unknown data format 
                        continue;
                    }
                    address = parts[0]; permission = parts[1]; offset = parts[2]; dev = parts[3]; inode = parts[4];
                    string[] addressBeginEnd = address.Split('-');
                    MemoryMapEntry entry = new MemoryMapEntry()
                    {
                        BeginAddr = Convert.ToUInt64(addressBeginEnd[0], 16),
                        EndAddr = Convert.ToUInt64(addressBeginEnd[1], 16),
                        FilePath = path,
                        FileName = string.IsNullOrEmpty(path) ? string.Empty : Path.GetFileName(path),
                        Permission = ParsePermission(permission)
                    };
                    result.Add(entry);
                }
            }
            return result;
        }

        private int ParsePermission(string permission)
        {
            // parse something like rwxp or r-xp
            if (permission.Length != 4)
            {
                return 0;
            }
            int r = permission[0] != '-' ? 8 : 0;
            int w = permission[1] != '-' ? 4 : 0;
            int x = permission[2] != '-' ? 2 : 0;
            int p = permission[3] != '-' ? 1 : 0;
            return r + w + x + p;
        }
    }

    internal class MemoryMapEntry
    {
        public ulong BeginAddr { get; set; }
        public ulong EndAddr { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int Permission { get; set; }
    }
}