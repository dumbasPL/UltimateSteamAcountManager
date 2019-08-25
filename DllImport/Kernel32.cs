using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Ultimate_Steam_Acount_Manager.DllImport
{
    class Kernel32
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
             ProcessAccessFlags processAccess,
             bool bInheritHandle,
             int processId
        );
        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags)
        {
            return OpenProcess(flags, false, proc.Id);
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            Int32 nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
            int dwSize, AllocationType dwFreeType);

        public static T StructFromProcessMemory<T>(IntPtr hProc, IntPtr address)
        {
            // Read in a byte array
            byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];
            ReadProcessMemory(hProc, address, bytes, bytes.Length, out var @int);

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess,
           IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress,
           IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Int32 WaitForSingleObject(IntPtr hHandle, Int32 dwMilliseconds);

        [DllImport("kernel32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        public static IntPtr RemoteLoadLibraryA(IntPtr hProcess, string name)
        {
            int LenWrite = name.Length + 1;
            IntPtr AllocMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)LenWrite, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
            if (AllocMem == IntPtr.Zero) return IntPtr.Zero;
            WriteProcessMemory(hProcess, AllocMem, name, LenWrite, out IntPtr _);
            IntPtr Injector = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (Injector == IntPtr.Zero) return IntPtr.Zero;
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, Injector, AllocMem, 0, out _);
            WaitForSingleObject(hThread, 5000);
            GetExitCodeThread(hThread, out IntPtr hModule);
            CloseHandle(hThread);
            VirtualFreeEx(hProcess, AllocMem, 0, AllocationType.Release);
            return hModule;
        }

        private static IntPtr _GetProcAddress = IntPtr.Zero;

        public static IntPtr RemoteGetProcAddress(IntPtr hProcess, IntPtr module, string name)
        {
            if(_GetProcAddress == IntPtr.Zero) _GetProcAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "GetProcAddress");
            byte[] shellcode = new byte[] {
                0x55,                           //push  ebp
                0x89, 0xE5,                     //mov   ebp,esp
                0x8B, 0x45, 0x08,               //mov   eax, DWORD PTR[ebp+8]
                0x50,                           //push  eax
                0x68, 0x00, 0x00, 0x00, 0x00,   //push  module
                0xb9, 0x00, 0x00, 0x00, 0x00,   //mov   ecx, <kernel32.GetProcAddress>
                0xff, 0xd1,                     //call  ecx
                0x5d,                           //pop   ebp
                0xc2, 0x04, 0x00                //ret   0x4
            };
            Array.Copy(BitConverter.GetBytes((int)module), 0, shellcode, 8, 4);
            Array.Copy(BitConverter.GetBytes((int)_GetProcAddress), 0, shellcode, 13, 4);
            int memLen = shellcode.Length + name.Length + 1;
            IntPtr pMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)memLen, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
            if (pMem == IntPtr.Zero) return IntPtr.Zero;
            byte[] buff = new byte[memLen];
            shellcode.CopyTo(buff, 0);
            Encoding.ASCII.GetBytes(name).CopyTo(buff, shellcode.Length);
            buff[buff.Length - 1] = 0x00; //idk if i need this
            if (!WriteProcessMemory(hProcess, pMem, buff, buff.Length, out _)) return IntPtr.Zero;
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pMem, pMem + shellcode.Length, 0, out _);
            WaitForSingleObject(hThread, 5000);
            GetExitCodeThread(hThread, out IntPtr address);
            CloseHandle(hThread);
            VirtualFreeEx(hProcess, pMem, 0, AllocationType.Release);
            return address;
        }

        public static IntPtr RemoteGetProcAddress(IntPtr hProcess, IntPtr module, uint ordinal)
        {
            if (_GetProcAddress == IntPtr.Zero) _GetProcAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "GetProcAddress");
            byte[] shellcode = new byte[] {
                0x55,                           //push  ebp
                0x89, 0xE5,                     //mov   ebp,esp
                0x8B, 0x45, 0x08,               //mov   eax, DWORD PTR[ebp+8]
                0x50,                           //push  eax
                0x68, 0x00, 0x00, 0x00, 0x00,   //push  module
                0xb9, 0x00, 0x00, 0x00, 0x00,   //mov   ecx, <kernel32.GetProcAddress>
                0xff, 0xd1,                     //call  ecx
                0x5d,                           //pop   ebp
                0xc2, 0x04, 0x00                //ret   0x4
            };
            Array.Copy(BitConverter.GetBytes((int)module), 0, shellcode, 8, 4);
            Array.Copy(BitConverter.GetBytes((int)_GetProcAddress), 0, shellcode, 13, 4);
            IntPtr pMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
            if (pMem == IntPtr.Zero) return IntPtr.Zero;
            if (!WriteProcessMemory(hProcess, pMem, shellcode, shellcode.Length, out _)) return IntPtr.Zero;
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pMem, (IntPtr)ordinal, 0, out _);
            WaitForSingleObject(hThread, 5000);
            GetExitCodeThread(hThread, out IntPtr address);
            CloseHandle(hThread);
            VirtualFreeEx(hProcess, pMem, 0, AllocationType.Release);
            return address;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_IMPORT_DESCRIPTOR
        {
            #region union
            /// <summary>
            /// CSharp doesnt really support unions, but they can be emulated by a field offset 0
            /// </summary>

            [FieldOffset(0)]
            public uint Characteristics;            // 0 for terminating null import descriptor
            [FieldOffset(0)]
            public uint OriginalFirstThunk;         // RVA to original unbound IAT (PIMAGE_THUNK_DATA)
            #endregion

            [FieldOffset(4)]
            public uint TimeDateStamp;
            [FieldOffset(8)]
            public uint ForwarderChain;
            [FieldOffset(12)]
            public IntPtr Name;
            [FieldOffset(16)]
            public uint FirstThunk;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct THUNK_DATA
        {
            [FieldOffset(0)]
            public uint ForwarderString;      // PBYTE 
            [FieldOffset(0)]
            public uint Function;             // PDWORD
            [FieldOffset(0)]
            public uint Ordinal;
            [FieldOffset(0)]
            public IntPtr AddressOfData;        // PIMAGE_IMPORT_BY_NAME
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_IMPORT_BY_NAME
        {
            [FieldOffset(0)]
            public ushort Hint;
        }

        public static string ReadCharPointerString(IntPtr hProcess, IntPtr pointer, int max_len = 512)
        {
            byte[] buff = new byte[max_len];
            if (!ReadProcessMemory(hProcess, pointer, buff, buff.Length, out _)) return null;
            int end = 0;
            while (end < buff.Length && buff[end] != 0) end++;
            return Encoding.ASCII.GetString(buff, 0, end);
        }

        public static void CallTlsCallback(IntPtr hProcess, IntPtr baseAddr, IntPtr callback)
        {
            byte[] shellcode = new byte[] {
                0x55,                           //push  ebp
                0x8B, 0xEC,                     //mov   ebp, esp
                0x6A, 0x00,                     //push  0
                0x6A, 0x01,                     //push  1
                0x8B, 0x45, 0x08,               //mov   eax, dword ptr [ebp+8]
                0x50,                           //push  eax
                0xB9, 0x00, 0x00, 0x00, 0x00,   //mov   ecx, 0x69696969
                0xFF, 0xD1,                     //call  ecx
                0x5D,                           //pop   ebp
                0xC2, 0x04, 0x00                //ret   4
            };
            Array.Copy(BitConverter.GetBytes((int)callback), 0, shellcode, 12, 4);
            IntPtr pMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
            if (pMem == IntPtr.Zero) return;
            if (!WriteProcessMemory(hProcess, pMem, shellcode, shellcode.Length, out _)) return;
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pMem, baseAddr, 0, out _);
            WaitForSingleObject(hThread, 5000);
            CloseHandle(hThread);
            VirtualFreeEx(hProcess, pMem, 0, AllocationType.Release);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_TLS_DIRECTORY32
        {
            public IntPtr StartAddressOfRawData;
            public IntPtr EndAddressOfRawData;
            public IntPtr AddressOfIndex;             // PDWORD
            public IntPtr AddressOfCallBacks;         // PIMAGE_TLS_CALLBACK *
            public uint SizeOfZeroFill;
            public uint Characteristics;
        }
    }
}
