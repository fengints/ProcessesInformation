using GetProcesses.WinAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GetProcesses.PI
{
    public class Win32ProcessInformation : IDisposable
    {
        public Win32ProcessInformation(int processId)
        {
            _pHandle = Win32Api.OpenProcess(_readFlags, false, processId);

            if (_pHandle.IsInvalid)
                throw new ArgumentException(nameof(processId));

            this._processId = processId;
        }
        public Win32ProcessInformation(int processId, ProcessAccessFlags flags)
        {
            _pHandle = Win32Api.OpenProcess(flags, false, processId);
            this._processId = processId;
        }

        private MySafeHandle _pHandle { get; }
        private int _processId { get; }
        private const ProcessAccessFlags _readFlags = ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VMRead;



        public static MODULEENTRY32[] GetRunningModulesOfProcess(int processId)
        {
            List<MODULEENTRY32> entries = new List<MODULEENTRY32>();
            using (var h = ToolHelp.CreateToolhelp32Snapshot(ToolHelpFlags.TH32CS_SNAPMODULE, processId))
            {
                if (h.IsInvalid)
                {
                    var code = Marshal.GetLastWin32Error();
                }
                MODULEENTRY32 entry = new MODULEENTRY32()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32))
                };

                var s = ToolHelp.Module32First(h, ref entry);

                if (!s)
                {
                    var code = Marshal.GetLastWin32Error();
                    Console.WriteLine(code);
                }

                bool f = true;
                while (f)
                {
                    var st = ToolHelp.Module32Next(h, ref entry);
                    var code = Marshal.GetLastWin32Error();
                    if (code == 18)
                        f = false;

                    Console.WriteLine(entry.szExePath);
                }

            }
            return entries.ToArray();
        }
        public void ProcessIcon()
        {
            //throw new NotImplementedException();
        }                 //get from filename
        public string ShortName()
        {
            int size = 260;
            StringBuilder name = new StringBuilder(size);

            int r = (int)Win32Api.GetModuleBaseName(_pHandle, IntPtr.Zero, name, size + 1);

            if (r == 0)
            {
                return null;
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }

            return name.ToString();

        }
        public string FullName()
        {
            int size = 260;
            StringBuilder fullName = new StringBuilder(size);

            bool r = Win32Api.QueryFullProcessImageName(_pHandle, 0, fullName, ref size);

            if (r is false)
            {
                return null;
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }

            return fullName.ToString();
        }
        public string CMDRunString()
        {
            throw new Exception();
        } // find winapi method
        public string RunningUserName()
        {
            //if (Marshal.GetLastWin32Error() == 5)
            //{
            //    //access denied
            //    return null;
            //    //throw new UnauthorizedAccessException();
            //}

            //Open access token

            //move outside
            uint TokenRead = 0x00020008;
            bool atSuccess = Win32Api.OpenProcessToken(_pHandle, TokenRead, out MySafeHandle tokenH);

            if (!atSuccess)
            {
                return null;
                //throw new Exception(Marshal.GetLastWin32Error().ToString());
            }

            var tokenUser = new TOKEN_USER();

            uint tokenLength = 0;
            bool Result;

            // first call gets length of TokenInformation, 
            // shows error but returns tokenlength
            Result = Win32Api.GetTokenInformation(tokenH, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, (uint)tokenLength, out tokenLength);

            IntPtr tokenUserPtr = Marshal.AllocHGlobal((int)tokenLength);

            //make it easier with elevationPtr
            var tokenInfo = Win32Api.GetTokenInformation(tokenH, TOKEN_INFORMATION_CLASS.TokenUser, tokenUserPtr, tokenLength, out uint returnedSize);

            if (!tokenInfo)
            {
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }

            ///According to: https://technet.microsoft.com/en-us/library/cc770642.aspx, 
            ///https://docs.microsoft.com/en-us/troubleshoot/windows-server/identity/naming-conventions-for-computer-domain-site-ou
            ///
            ///Max username 20 symbols
            ///Max domainname 15 symbols
            tokenUser = (TOKEN_USER)Marshal.PtrToStructure(tokenUserPtr, typeof(TOKEN_USER));



            var bufferSize = 20;
            var userName = new StringBuilder(bufferSize);

            var domainNameSize = 15;
            var domainName = new StringBuilder(domainNameSize);
            int accountType = 0;
            var accountSuccess = Win32Api.LookupAccountSid(null, tokenUser.User.Sid, userName, ref bufferSize, domainName, ref domainNameSize, ref accountType);
            //Check domainname and username length

            if (accountSuccess == 0)
            {
                throw new Exception(Marshal.GetLastWin32Error().ToString());
            }

            return userName.ToString();

        }
        public string GetProcessBitness()
        {
            if (Marshal.GetLastWin32Error() == 5)
            {
                //access denied
                return null;
                //throw new UnauthorizedAccessException();
            }

            bool isWow64 = false;

            var isWoW64Success = Win32Api.IsWow64Process(_pHandle, ref isWow64);

            if (!isWoW64Success)
                return null;
            //throw new Exception(Marshal.GetLastWin32Error().ToString());

            SYSTEM_INFO info = new SYSTEM_INFO();
            Win32Api.GetNativeSystemInfo(ref info);

            //Move systeminfo outside in a static class
            if (info.processorArchitecture == 9 &&
                isWow64 == false)
                return "x64";
            else
                return "x32";

        }
        public bool? IsElevated()
        {
            if (Marshal.GetLastWin32Error() == 5)
            {
                //access denied
                return null;
                //throw new UnauthorizedAccessException();
            }

            MySafeHandle tokenHandle;
            if (!Win32Api.OpenProcessToken(_pHandle, Win32Api.TOKEN_READ, out tokenHandle))
            {
                return null;
                //throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
            }

            TOKEN_ELEVATION_TYPE elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;

            int elevationResultSize = Marshal.SizeOf((int)elevationResult);
            uint returnedSize = 0;
            IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);

            bool success = Win32Api.GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, (uint)elevationResultSize, out returnedSize);
            if (success)
            {
                elevationResult = (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(elevationTypePtr);
                bool isProcessAdmin = elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
                return isProcessAdmin;
            }
            else
            {
                throw new ApplicationException("Unable to determine the current elevation.");
            }
        }

        // returns ProviderUnknown for some reason

        private const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";
        public bool SignatureExist(string fileName)
        {
            WinTrustFileInfo wtfi = new WinTrustFileInfo(fileName);
            WinTrustData wtd = new WinTrustData(wtfi);
            Guid guidAction = new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2);
            WinVerifyTrustResult result = Win32Api.WinVerifyTrust(new IntPtr(-1), guidAction, wtd);

            bool ret = (result == WinVerifyTrustResult.Success);
            wtfi.Dispose();
            wtd.Dispose();
            return ret;
        }
        public void Dispose()
        {
            _pHandle.Dispose();
        }


        public static List<Tuple<int, string>> GetRunningProcessIDs2()
        {
            UInt32 arraySize = 150;
            UInt32 arrayBytesSize = arraySize * sizeof(UInt32);
            var processIds = new Int32[arraySize];
            UInt32 bytesCopied;

            bool success = Win32Api.EnumProcesses(processIds, arrayBytesSize, out bytesCopied);
            Console.WriteLine(success);
            Console.WriteLine("Process count: " + processIds.Where(x => x != 0).Count());


            //add to list
            var procesIds = new List<Tuple<int, string>>();
            for (int i = 0; i < processIds.Length; i++)
            {
                if (processIds[i] != 0)
                {
                    procesIds.Add(new Tuple<int, string>(processIds[i], " basename: " + PrintProcessName(processIds[i])));
                }
            }

            return procesIds;
        }
        public static int[] GetRunningProcessIDs()
        {
            UInt32 arraySize = 150;
            UInt32 arrayBytesSize = arraySize * sizeof(UInt32);
            var processIds = new Int32[arraySize];
            UInt32 bytesCopied;

            bool success = Win32Api.EnumProcesses(processIds, arrayBytesSize, out bytesCopied);
            return processIds;
        }
        static string PrintProcessName(int processID)
        {
            //First approach
            using (var hProcess = Win32Api.OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VMRead, false, processID))
            {
                if (hProcess.IsInvalid == true)
                {
                    var code = Marshal.GetLastWin32Error();
                    return null;
                }

                var size = 260;
                StringBuilder stringBuilder = new StringBuilder(size);
                if (0 != Win32Api.GetModuleBaseName(hProcess, IntPtr.Zero, stringBuilder, size + 1))
                {
                    return stringBuilder.ToString();
                }
                else
                {
                    return Marshal.GetLastWin32Error().ToString();
                }
            }

        }

        //Create integration test that defines current process count
        public static uint[] GetRunningProcessIDsSnapshot()
        {
            List<uint> processes = new List<uint>();
            int processCount = 0;
            using (var toolhelpHandler = ToolHelp.CreateToolhelp32Snapshot(ToolHelpFlags.TH32CS_SNAPPROCESS | ToolHelpFlags.TH32CS_SNAPNOHEAPS, 0))
            {
                ProcessEntry32 processEntry32 = new ProcessEntry32
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(ProcessEntry32))
                };

                var success = ToolHelp.Process32First(toolhelpHandler, ref processEntry32);
                if (success)
                {
                    //catch code there
                }
                Console.WriteLine("Error" + Marshal.GetLastWin32Error());
                while (ToolHelp.Process32Next(toolhelpHandler, ref processEntry32))
                {
                    processes.Add(processEntry32.th32ProcessID);
                    processCount++;
                    //Console.WriteLine("Process id: " + processEntry32.th32ProcessID + " Process Name: " + processEntry32.szExeFile);
                }

                Console.WriteLine("Process count: " + processCount);
            }

            return processes.ToArray();
        }
    }
}
