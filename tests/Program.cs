using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetProcesses;
using System.Management;
using System.Security.Cryptography.Xml;
using System.IO;
using System.Runtime.InteropServices;
using GetProcesses.WinAPI;

namespace tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Utils utils = new Utils();

            

            //var procId = 5316;
            ////var fullFileName = utils.GetProcessFullName(procId); x86 not working
            //var userName = utils.GetProcessRunningUserName(procId);
            ////var shortname = utils.GetProcessShortName(procId);
            //var _ = utils.GetProcessBitness(procId);

            var testName = @"C:\Users\danz\AppData\Local\Programs\Python\Python38\python.exe";
            var cert = utils.GetProcessSignatureExist(testName);



            //var pss = utils.GetRunningProcessIDs();
            //var myadmin = pss.SingleOrDefault(x => x.Item1 == 6096);

            ////show from list
            //foreach (var p in pss.OrderBy(x => x.Item1))
            //{
            //    Console.WriteLine(p.Item1 + p.Item2);
            //}


            //var bitness = utils.GetProcessBitness(8388);

            //var iselevated = utils.GetProcessIsElevated(2572);


        }
        private void tests()
        {
            //UInt32 arraySize = 9000;
            //UInt32 arrayBytesSize = arraySize * sizeof(UInt32);
            //var processIds = new Int32[arraySize];
            //UInt32 bytesCopied;

            //bool success = EnumProcesses(processIds, arrayBytesSize, out bytesCopied);
            //Console.WriteLine(success);
            //Console.ReadLine();
        }
        private static string GetSignatureFromCatalog(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                return null;
            if (!File.Exists(filename))
                return null;

            try
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    SIGNATURE_INFO sigInfo = new SIGNATURE_INFO();
                    sigInfo.cbSize = (uint)Marshal.SizeOf(sigInfo);

                    IntPtr ppCertContext = IntPtr.Zero;
                    IntPtr phStateData = IntPtr.Zero;

                    try
                    {
                        int hresult = Win32Api.WTGetSignatureInfo(filename, stream.SafeFileHandle.DangerousGetHandle(),
                            SIGNATURE_INFO_FLAGS.SIF_CATALOG_SIGNED |
                            SIGNATURE_INFO_FLAGS.SIF_CATALOG_FIRST |
                            SIGNATURE_INFO_FLAGS.SIF_AUTHENTICODE_SIGNED |
                            SIGNATURE_INFO_FLAGS.SIF_BASE_VERIFICATION |
                            SIGNATURE_INFO_FLAGS.SIF_CHECK_OS_BINARY,
                            ref sigInfo, ref ppCertContext, ref phStateData);

                        if (hresult >= 0)
                        {
                            return GetResultFromSignatureState(sigInfo.nSignatureState);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    finally
                    {
                        if (phStateData != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(phStateData);
                        }

                        if (ppCertContext != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(phStateData);
                        }
                    }
                }
            }
            catch (TypeLoadException)
            {
                // If we don't have WTGetSignatureInfo
            }
            return null;
        }

        private static string GetResultFromSignatureState(SIGNATURE_STATE state)
        {
            switch (state)
            {
                case SIGNATURE_STATE.SIGNATURE_STATE_UNSIGNED_MISSING: return "TRUST_E_NOSIGNATURE";
                case SIGNATURE_STATE.SIGNATURE_STATE_UNSIGNED_UNSUPPORTED: return "TRUST_E_NOSIGNATURE";
                case SIGNATURE_STATE.SIGNATURE_STATE_UNSIGNED_POLICY: return "TRUST_E_NOSIGNATURE";
                case SIGNATURE_STATE.SIGNATURE_STATE_INVALID_CORRUPT: return "TRUST_E_BAD_DIGEST";
                case SIGNATURE_STATE.SIGNATURE_STATE_INVALID_POLICY: return "CRYPT_E_BAD_MSG";
                case SIGNATURE_STATE.SIGNATURE_STATE_VALID: return "NO_ERROR";
                case SIGNATURE_STATE.SIGNATURE_STATE_TRUSTED: return "NO_ERROR";
                case SIGNATURE_STATE.SIGNATURE_STATE_UNTRUSTED: return "TRUST_E_EXPLICIT_DISTRUST";

                // Should not happen
                default:
                    System.Diagnostics.Debug.Fail("Should not get here - could not map SIGNATURE_STATE");
                    return "TRUST_E_NOSIGNATURE";
            }
        }
    }

    
}
