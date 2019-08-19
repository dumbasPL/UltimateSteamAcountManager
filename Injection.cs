using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Ultimate_Steam_Acount_Manager.DllImport;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultimate_Steam_Acount_Manager
{
    class Injection
    {

        public static bool Inject(Process process, string dll)
        {
            IntPtr hProc = Kernel32.OpenProcess(process, Kernel32.ProcessAccessFlags.All);
            if (hProc == IntPtr.Zero) throw new InjectionException("Failed to open process " + Marshal.GetLastWin32Error());
            using (FileStream stream = new FileStream(dll, FileMode.Open, FileAccess.Read))
            {
                PeHeaderReader peHeader = new PeHeaderReader(stream);
                BinaryReader reader = new BinaryReader(stream);
                stream.Seek(0, SeekOrigin.Begin);
                IntPtr pTargetBase = Kernel32.VirtualAllocEx(
                    hProc,
                    (IntPtr)peHeader.OptionalHeader32.ImageBase,
                    peHeader.OptionalHeader32.SizeOfImage,
                    Kernel32.AllocationType.Commit | Kernel32.AllocationType.Reserve,
                    Kernel32.MemoryProtection.ExecuteReadWrite);
                if(pTargetBase == IntPtr.Zero)
                {
                    pTargetBase = Kernel32.VirtualAllocEx(
                        hProc,
                        IntPtr.Zero,
                        peHeader.OptionalHeader32.SizeOfImage,
                        Kernel32.AllocationType.Commit | Kernel32.AllocationType.Reserve,
                        Kernel32.MemoryProtection.ExecuteReadWrite);
                    if (pTargetBase == IntPtr.Zero) throw new InjectionException("Failed to alocate memory in remote process + " + Marshal.GetLastWin32Error());
                }
                foreach (var section in peHeader.ImageSectionHeaders)
                {
                    if (section.SizeOfRawData == 0) continue;
                    stream.Seek(section.PointerToRawData, SeekOrigin.Begin);
                    byte[] data = new byte[section.SizeOfRawData];
                    if (stream.Read(data, 0, data.Length) != data.Length) throw new InjectionException("Filed to read section to memory", hProc, pTargetBase);
                    if (!Kernel32.WriteProcessMemory(hProc, pTargetBase + (int)section.VirtualAddress, data, (int)section.SizeOfRawData, out var @int))
                        throw new InjectionException("Failed to write section to target process " + Marshal.GetLastWin32Error(), hProc, pTargetBase);
                }
                int LocationDelta = (int)pTargetBase - (int)peHeader.OptionalHeader32.ImageBase;
                if (LocationDelta != 0 && peHeader.OptionalHeader32.BaseRelocationTable.Size != 0)
                {
                    stream.Seek(peHeader.OptionalHeader32.BaseRelocationTable.VirtualAddress, SeekOrigin.Begin);
                    var pRelocData = PeHeaderReader.FromBinaryReader<PeHeaderReader.IMAGE_BASE_RELOCATION>(reader);
                }

            }
            return true;
        }

        public class InjectionException : Exception
        {

            public InjectionException() { }

            public InjectionException(string message) : base(message) { }

            public InjectionException(string message, IntPtr hProc, IntPtr lpAddress) : this(message)
            {
                Kernel32.VirtualFreeEx(hProc, lpAddress, 0, Kernel32.AllocationType.Release);
            }

        }

    }

}
