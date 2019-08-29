using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using Ultimate_Steam_Acount_Manager.DllImport;
using System.Runtime.InteropServices;

namespace Ultimate_Steam_Acount_Manager
{
    class SteamAPI
    {
        private static ISteamClient _steamClient;
        private static IntPtr steamclientdll;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr FnCreateInterface(string version, IntPtr unk);

        public static ISteamClient GetSteamClient()
        {
            if (_steamClient != null /*add valodation*/) return _steamClient;
            string path = null;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                if (key == null) throw new SteamException("Failed to open registry");
                path = key.GetValue("SteamPath").ToString();
            }
            if(path == null) throw new SteamException("Failed to get steam path from registry");
            Directory.SetCurrentDirectory(path);
            steamclientdll = Kernel32.LoadLibrary("steamclient.dll");
            if (steamclientdll == IntPtr.Zero) throw new SteamException("Filed to load steamclient.dll");
            IntPtr createInterfaceAdr = Kernel32.GetProcAddress(steamclientdll, "CreateInterface");
            if (createInterfaceAdr == IntPtr.Zero) throw new SteamException("failed to find CreateInterface");
            FnCreateInterface fnCreateInterface = Marshal.GetDelegateForFunctionPointer<FnCreateInterface>(createInterfaceAdr);
            IntPtr pSteamClient = fnCreateInterface("SteamClient019", IntPtr.Zero);
            if (pSteamClient == IntPtr.Zero) throw new SteamException("Failed to get SteamClient019 interface");
            _steamClient = new ISteamClient(pSteamClient);
            return _steamClient;
        }

        public class ISteamClient
        {
            private IntPtr baseObj;

            public ISteamClient(IntPtr baseObject)
            {
                this.baseObj = baseObject;
            }

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate Int32 fnCreateSteamPipe(IntPtr thisPtr);
            private Int32 CreateSteamPipe()
            {
                return Util.GetVirtualFunction<fnCreateSteamPipe>(baseObj, 0)(baseObj);
            }

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate Int32 fnConnectToGlobalUser(IntPtr thisPtr, Int32 hSteamPipe);
            private Int32 ConnectToGlobalUser(Int32 hSteamPipe)
            {
                return Util.GetVirtualFunction<fnConnectToGlobalUser>(baseObj, 2)(baseObj, hSteamPipe);
            }

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate IntPtr fnGetISteamFriends(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, string pchVersion);
            public ISteamFriends GetISteamFriends()
            {
                Int32 hSteamPipe = CreateSteamPipe();
                if (hSteamPipe == 0) throw new SteamException("Failed to create steam pipe");
                Int32 hSteamUser = ConnectToGlobalUser(hSteamPipe);
                if (hSteamUser == 0) throw new SteamException("Failed to connect to global user");
                IntPtr pISteamFriends = Util.GetVirtualFunction<fnGetISteamFriends>(baseObj, 8)
                    (baseObj, hSteamUser, hSteamPipe, "SteamFriends017");
                if (pISteamFriends == IntPtr.Zero) throw new SteamException("Failed to get SteamFriends017");
                return new ISteamFriends(pISteamFriends);
            }

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate IntPtr fnGetISteamUser(IntPtr thisPtr, Int32 hSteamUser, Int32 hSteamPipe, string pchVersion);
            public ISteamUser GetISteamUser()
            {
                Int32 hSteamPipe = CreateSteamPipe();
                if (hSteamPipe == 0) throw new SteamException("Failed to create steam pipe");
                Int32 hSteamUser = ConnectToGlobalUser(hSteamPipe);
                if (hSteamUser == 0) throw new SteamException("Failed to connect to global user");
                IntPtr pISteamUser = Util.GetVirtualFunction<fnGetISteamUser>(baseObj, 5)
                    (baseObj, hSteamUser, hSteamPipe, "SteamUser020");
                if (pISteamUser == IntPtr.Zero) throw new SteamException("Failed to get SteamUser020");
                return new ISteamUser(pISteamUser);
            }

        }

        public class ISteamFriends
        {
            private IntPtr baseObj;

            public ISteamFriends(IntPtr baseObj)
            {
                this.baseObj = baseObj;
            }

            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate IntPtr fnGetPersonaName(IntPtr thisPtr);
            public string GetPersonaName()
            {
                return Marshal.PtrToStringAnsi(Util.GetVirtualFunction<fnGetPersonaName>(baseObj, 0)(baseObj));
            }

        }

        public class ISteamUser
        {
            public IntPtr baseObj;

            public ISteamUser(IntPtr baseObj)
            {
                this.baseObj = baseObj;
            }

            //[return: MarshalAs(UnmanagedType.U8)]
            [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
            private delegate IntPtr fnGetSteamID(IntPtr thisPtr);
            public IntPtr GetSteamID()
            {
                return Util.GetVirtualFunction<fnGetSteamID>(baseObj, 2)(baseObj);
            }
        }

        class SteamException : Exception {

            public SteamException() { }

            public SteamException(string message) : base(message) { }

        }

    }
}
