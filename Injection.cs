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

        public static bool Inject(Process process, string dll, string user, string pass, string guard = null)
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
                    if (!Kernel32.WriteProcessMemory(hProc, pTargetBase + (int)section.VirtualAddress, data, (int)section.SizeOfRawData, out _))
                        throw new InjectionException("Failed to write section to target process " + Marshal.GetLastWin32Error(), hProc, pTargetBase);
                }
                int LocationDelta = (int)pTargetBase - (int)peHeader.OptionalHeader32.ImageBase;
                if (LocationDelta != 0 && peHeader.OptionalHeader32.BaseRelocationTable.Size != 0)
                {
                    uint currentOffset = peHeader.OptionalHeader32.BaseRelocationTable.VirtualAddress;
                    var pRelocData = Kernel32.StructFromProcessMemory<PeHeaderReader.IMAGE_BASE_RELOCATION>(hProc, pTargetBase + (int)currentOffset);
                    while (pRelocData.VirtualAddress != 0)
                    {
                        byte[] block = new byte[pRelocData.SizeOfBlock];
                        Kernel32.ReadProcessMemory(hProc, pTargetBase + (int)currentOffset + Marshal.SizeOf<PeHeaderReader.IMAGE_BASE_RELOCATION>(), block, block.Length, out _);
                        BinaryReader reader1 = new BinaryReader(new MemoryStream(block));
                        int count = ((int)pRelocData.SizeOfBlock - Marshal.SizeOf<PeHeaderReader.IMAGE_BASE_RELOCATION>()) / sizeof(short);
                        for (int i = 0; i < count; i++)
                        {
                            ushort relocation = reader1.ReadUInt16();
                            var relocationType = relocation >> 12;
                            if (relocationType == 0x3)//IMAGE_REL_BASED_HIGHLOW
                            {
                                uint addr = (uint)pTargetBase + pRelocData.VirtualAddress + (ushort)(relocation & 0xFFF);
                                byte[] buff = new byte[4];
                                if (!Kernel32.ReadProcessMemory(hProc, (IntPtr)addr, buff, buff.Length, out _))
                                    throw new InjectionException("Failed to read process memory", hProc, pTargetBase);
                                buff = BitConverter.GetBytes(BitConverter.ToInt32(buff, 0) + LocationDelta);
                                if (!Kernel32.WriteProcessMemory(hProc, (IntPtr)addr, buff, buff.Length, out _))
                                    throw new InjectionException("Failed to write process memory", hProc, pTargetBase);
                            }
                        }
                        currentOffset += pRelocData.SizeOfBlock;
                        pRelocData = Kernel32.StructFromProcessMemory<PeHeaderReader.IMAGE_BASE_RELOCATION>(hProc, pTargetBase + (int)currentOffset);
                    }
                }
                if(peHeader.OptionalHeader32.ImportTable.Size != 0)
                {
                    uint currentOffset = peHeader.OptionalHeader32.ImportTable.VirtualAddress;
                    var pImportDescr = Kernel32.StructFromProcessMemory<Kernel32.IMAGE_IMPORT_DESCRIPTOR>(hProc, pTargetBase + (int)currentOffset);
                    while(pImportDescr.Name != IntPtr.Zero)
                    {
                        string module = Kernel32.ReadCharPointerString(hProc, pTargetBase + (int)pImportDescr.Name);
                        if (module == null) throw new InjectionException("Failed to gte module name from IAT", hProc, pTargetBase);
                        IntPtr moduleBase = Kernel32.RemoteLoadLibraryA(hProc, module);
                        if (moduleBase == IntPtr.Zero) throw new InjectionException("Filed to load Imported DLL into remote process", hProc, pTargetBase);

                        IntPtr pThunkRef = (IntPtr)pImportDescr.OriginalFirstThunk;
                        IntPtr pFuncRef = (IntPtr)pImportDescr.FirstThunk;

                        if (pThunkRef == IntPtr.Zero)
                            pThunkRef = pFuncRef;

                        var ThunkRef = Kernel32.StructFromProcessMemory<Kernel32.THUNK_DATA>(hProc, pTargetBase + (int)pThunkRef);

                        while (ThunkRef.AddressOfData != IntPtr.Zero)
                        {
                            IntPtr func = IntPtr.Zero;
                            if ((ThunkRef.Ordinal & 0x80000000) > 0) //IMAGE_SNAP_BY_ORDINAL
                                func = Kernel32.RemoteGetProcAddress(hProc, moduleBase, ThunkRef.Ordinal & (uint)0xffff);
                            else
                            {
                                string funcName = Kernel32.ReadCharPointerString(hProc, pTargetBase + (int)ThunkRef.AddressOfData + 2); //skip the hint
                                func = Kernel32.RemoteGetProcAddress(hProc, moduleBase, funcName);
                            }
                            if (func == IntPtr.Zero) throw new InjectionException("Filed to get Remote function adress", hProc, pTargetBase);
                            byte[] data = BitConverter.GetBytes((int)func);
                            if (!Kernel32.WriteProcessMemory(hProc, pTargetBase + (int)pFuncRef, data, data.Length, out _))
                                throw new InjectionException("Filed to write fuction adress to target process IAT", hProc, pTargetBase);
                            pThunkRef += 4;
                            pFuncRef += 4;
                            ThunkRef = Kernel32.StructFromProcessMemory<Kernel32.THUNK_DATA>(hProc, pTargetBase + (int)pThunkRef);
                        }
                        currentOffset += (uint)Marshal.SizeOf<Kernel32.IMAGE_IMPORT_DESCRIPTOR>();
                        pImportDescr = Kernel32.StructFromProcessMemory<Kernel32.IMAGE_IMPORT_DESCRIPTOR>(hProc, pTargetBase + (int)currentOffset);
                    }
                }
                if (peHeader.OptionalHeader32.TLSTable.Size != 0)
                {
                    var tls = Kernel32.StructFromProcessMemory<Kernel32.IMAGE_TLS_DIRECTORY32>(hProc, pTargetBase + (int)peHeader.OptionalHeader32.TLSTable.VirtualAddress);
                    IntPtr currentOffset = tls.AddressOfCallBacks;
                    var funaddr = currentOffset != IntPtr.Zero ? Kernel32.StructFromProcessMemory<IntPtr>(hProc, currentOffset) : IntPtr.Zero;
                    while (currentOffset != IntPtr.Zero && funaddr != IntPtr.Zero)
                    {
                        Kernel32.CallTlsCallback(hProc, pTargetBase, funaddr);
                        currentOffset += 4;
                        funaddr = Kernel32.StructFromProcessMemory<IntPtr>(hProc, currentOffset);
                    }
                }
                if(!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                {
                    byte[] userBytes = Encoding.ASCII.GetBytes(user);
                    byte[] passBytes = Encoding.ASCII.GetBytes(pass);
                    byte[] tokenBytes = !string.IsNullOrWhiteSpace(guard) ? Encoding.ASCII.GetBytes(guard) : new byte[] {  };
                    byte[] data = new byte[userBytes.Length + 1 + passBytes.Length + 1 + tokenBytes.Length + 1];
                    userBytes.CopyTo(data, 0);
                    passBytes.CopyTo(data, userBytes.Length + 1);
                    if (tokenBytes.Length > 0) tokenBytes.CopyTo(data, userBytes.Length + 1 + passBytes.Length + 1);
                    if (!Kernel32.WriteProcessMemory(hProc, pTargetBase, data, data.Length, out _))
                        throw new InjectionException("Failed to write login details to targer process", hProc, pTargetBase);
                }
                Kernel32.CallTlsCallback(hProc, pTargetBase, pTargetBase + (int)peHeader.OptionalHeader32.AddressOfEntryPoint);//call DLLMAIN, im to lazy to rename it XD
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
