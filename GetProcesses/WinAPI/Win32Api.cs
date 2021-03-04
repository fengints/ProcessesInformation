using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace GetProcesses.WinAPI
{
    #region ProcessBitness
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_INFO
    {
        public ushort processorArchitecture;
        ushort reserved;
        public uint pageSize;
        public IntPtr minimumApplicationAddress;
        public IntPtr maximumApplicationAddress;
        public IntPtr activeProcessorMask;
        public uint numberOfProcessors;
        public uint processorType;
        public uint allocationGranularity;
        public ushort processorLevel;
        public ushort processorRevision;
    }

    public enum ProcessorArch : ushort
    {

        PROCESSOR_ARCHITECTURE_AMD64 = 9,   //x64 (AMD or Intel)

        PROCESSOR_ARCHITECTURE_ARM = 5,     //ARM

        PROCESSOR_ARCHITECTURE_ARM64 = 12,  //ARM64

        PROCESSOR_ARCHITECTURE_IA64 = 6,    //Intel Itanium-based

        PROCESSOR_ARCHITECTURE_INTEL = 0,   //x86

        PROCESSOR_ARCHITECTURE_UNKNOWN = 0xffff //Unknown architecture. 
    }
    #endregion

    public class Win32Api
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool QueryFullProcessImageName([In] MySafeHandle hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern void GetNativeSystemInfo([MarshalAs(UnmanagedType.Struct)] [In][Out]
            ref SYSTEM_INFO lpSystemInfo);


        [DllImport("Psapi.dll")]
        internal static extern bool EnumProcesses(
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)][In][Out] Int32[] processIds,
            UInt32 arraySizeBytes,
            [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern MySafeHandle OpenProcess(
            ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
            int dwProcessId);


        [DllImport("psapi.dll", SetLastError = true)]
        internal static extern bool EnumProcessModulesEx(
            MySafeHandle hProcess,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [Out] IntPtr[] lphModule,
            uint cb,
            [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded,
            uint dwFilterFlag);


        /// <summary>
        /// Returns only name of module
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="hModule"></param>
        /// <param name="lpBaseName"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        [DllImport("psapi.dll")]
        internal static extern uint GetModuleBaseName(
            MySafeHandle hProcess,
            IntPtr hModule,
            [Out] StringBuilder lpBaseName,
            [In][MarshalAs(UnmanagedType.U4)] int nSize);



        /// <summary>
        /// Returns Full path to module
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="hModule"></param>
        /// <param name="lpFilename"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        [DllImport("Psapi.dll", SetLastError = true)]
        internal static extern int GetModuleFileNameEx(
           MySafeHandle hProcess,
           IntPtr hModule,
           StringBuilder lpFilename,
           int nSize);


        /// <summary>
        /// A pointer to a value that is set to TRUE
        /// if the process is running under WOW64
        /// on an Intel64 or x64 processor If the process is running under 32-bit Windows,
        /// the value is set to FALSE.
        /// If the process is a 32-bit application running under 64-bit Windows 10 on ARM,
        /// the value is set to FALSE.
        /// If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="Wow64Process"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool IsWow64Process(
            MySafeHandle hProcess,
            ref bool Wow64Process);

        private static uint STANDARD_RIGHTS_READ = 0x00020000;
        private static uint TOKEN_QUERY = 0x0008;
        public static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(MySafeHandle ProcessHandle, UInt32 DesiredAccess, out MySafeHandle TokenHandle);


        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool GetTokenInformation(
            MySafeHandle TokenHandle,
            TOKEN_INFORMATION_CLASS TokenInformationClass,
            IntPtr TokenInformation,
            uint TokenInformationLength, 
            out uint ReturnLength);


        [DllImport("advapi32.dll", SetLastError = true)]
        public extern static int LookupAccountSid(
            string systemName,
            IntPtr pSid,
            StringBuilder szName,
            ref int nameSize,
            StringBuilder szDomain,
            ref int domainSize, 
            ref int eUse);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public extern static string GetCommandLineA();

        [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
        public static extern WinVerifyTrustResult WinVerifyTrust(
            [In] IntPtr hwnd,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
            [In] WinTrustData pWVTData);
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x0010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x0400,
        LimitedQueryInformation = 0x1000,
        Synchronize = 0x00100000
    }

    #region WinTrustData struct field enums
    enum WinTrustDataUIChoice : uint
    {
        All = 1,
        None = 2,
        NoBad = 3,
        NoGood = 4
    }

    enum WinTrustDataRevocationChecks : uint
    {
        None = 0x00000000,
        WholeChain = 0x00000001
    }

    enum WinTrustDataChoice : uint
    {
        File = 1,
        Catalog = 2,
        Blob = 3,
        Signer = 4,
        Certificate = 5
    }

    enum WinTrustDataStateAction : uint
    {
        Ignore = 0x00000000,
        Verify = 0x00000001,
        Close = 0x00000002,
        AutoCache = 0x00000003,
        AutoCacheFlush = 0x00000004
    }

    [FlagsAttribute]
    enum WinTrustDataProvFlags : uint
    {
        UseIe4TrustFlag = 0x00000001,
        NoIe4ChainFlag = 0x00000002,
        NoPolicyUsageFlag = 0x00000004,
        RevocationCheckNone = 0x00000010,
        RevocationCheckEndCert = 0x00000020,
        RevocationCheckChain = 0x00000040,
        RevocationCheckChainExcludeRoot = 0x00000080,
        SaferFlag = 0x00000100,        // Used by software restriction policies. Should not be used.
        HashOnlyFlag = 0x00000200,
        UseDefaultOsverCheck = 0x00000400,
        LifetimeSigningFlag = 0x00000800,
        CacheOnlyUrlRetrieval = 0x00001000,      // affects CRL retrieval and AIA retrieval
        DisableMD2andMD4 = 0x00002000      // Win7 SP1+: Disallows use of MD2 or MD4 in the chain except for the root
    }

    enum WinTrustDataUIContext : uint
    {
        Execute = 0,
        Install = 1
    }
    #endregion

    //Digital signature
    #region WinTrust structures
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class WinTrustFileInfo
    {
        UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustFileInfo));
        IntPtr pszFilePath;                     // required, file name to be verified
        IntPtr hFile = IntPtr.Zero;             // optional, open handle to FilePath
        IntPtr pgKnownSubject = IntPtr.Zero;    // optional, subject type if it is known

        public WinTrustFileInfo(String _filePath)
        {
            pszFilePath = Marshal.StringToCoTaskMemAuto(_filePath);
        }
        public void Dispose()
        {
            if (pszFilePath != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pszFilePath);
                pszFilePath = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class WinTrustData
    {
        UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustData));
        IntPtr PolicyCallbackData = IntPtr.Zero;
        IntPtr SIPClientData = IntPtr.Zero;
        // required: UI choice
        WinTrustDataUIChoice UIChoice = WinTrustDataUIChoice.None;
        // required: certificate revocation check options
        WinTrustDataRevocationChecks RevocationChecks = WinTrustDataRevocationChecks.None;
        // required: which structure is being passed in?
        WinTrustDataChoice UnionChoice = WinTrustDataChoice.File;
        // individual file
        IntPtr FileInfoPtr;
        WinTrustDataStateAction StateAction = WinTrustDataStateAction.Ignore;
        IntPtr StateData = IntPtr.Zero;
        String URLReference = null;
        WinTrustDataProvFlags ProvFlags = WinTrustDataProvFlags.RevocationCheckChainExcludeRoot;
        WinTrustDataUIContext UIContext = WinTrustDataUIContext.Execute;

        // constructor for silent WinTrustDataChoice.File check
        public WinTrustData(WinTrustFileInfo _fileInfo)
        {
            // On Win7SP1+, don't allow MD2 or MD4 signatures
            if ((Environment.OSVersion.Version.Major > 6) ||
                ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor > 1)) ||
                ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 1) && !String.IsNullOrEmpty(Environment.OSVersion.ServicePack)))
            {
                ProvFlags |= WinTrustDataProvFlags.DisableMD2andMD4;
            }

            WinTrustFileInfo wtfiData = _fileInfo;
            FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
            Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);
        }
        public void Dispose()
        {
            if (FileInfoPtr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(FileInfoPtr);
                FileInfoPtr = IntPtr.Zero;
            }
        }
    }
    public enum WinVerifyTrustResult : uint
    {
        Success = 0,
        ProviderUnknown = 0x800b0001,           // Trust provider is not recognized on this system
        ActionUnknown = 0x800b0002,         // Trust provider does not support the specified action
        SubjectFormUnknown = 0x800b0003,        // Trust provider does not support the form specified for the subject
        SubjectNotTrusted = 0x800b0004,         // Subject failed the specified verification action
        FileNotSigned = 0x800B0100,         // TRUST_E_NOSIGNATURE - File was not signed
        SubjectExplicitlyDistrusted = 0x800B0111,   // Signer's certificate is in the Untrusted Publishers store
        SignatureOrFileCorrupt = 0x80096010,    // TRUST_E_BAD_DIGEST - file was probably corrupt
        SubjectCertExpired = 0x800B0101,        // CERT_E_EXPIRED - Signer's certificate was expired
        SubjectCertificateRevoked = 0x800B010C,     // CERT_E_REVOKED Subject's certificate was revoked
        UntrustedRoot = 0x800B0109          // CERT_E_UNTRUSTEDROOT - A certification chain processed correctly but terminated in a root certificate that is not trusted by the trust provider.
    }
    #endregion

    #region TokenInformation
    public struct TOKEN_USER
    {
        public SID_AND_ATTRIBUTES User;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SID_AND_ATTRIBUTES
    {
        public IntPtr Sid;
        public int Attributes;
    }
    public enum TOKEN_INFORMATION_CLASS
    {
        TokenUser = 1,
        TokenGroups,
        TokenPrivileges,
        TokenOwner,
        TokenPrimaryGroup,
        TokenDefaultDacl,
        TokenSource,
        TokenType,
        TokenImpersonationLevel,
        TokenStatistics,
        TokenRestrictedSids,
        TokenSessionId,
        TokenGroupsAndPrivileges,
        TokenSessionReference,
        TokenSandBoxInert,
        TokenAuditPolicy,
        TokenOrigin,
        TokenElevationType,
        TokenLinkedToken,
        TokenElevation,
        TokenHasRestrictions,
        TokenAccessInformation,
        TokenVirtualizationAllowed,
        TokenVirtualizationEnabled,
        TokenIntegrityLevel,
        TokenUIAccess,
        TokenMandatoryPolicy,
        TokenLogonSid,
        MaxTokenInfoClass
    }
    public enum TOKEN_ELEVATION_TYPE
    {
        TokenElevationTypeDefault = 1,
        TokenElevationTypeFull,
        TokenElevationTypeLimited
    }
    #endregion

    #region toolhelp
    public class ToolHelp
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern MySafeHandle CreateToolhelp32Snapshot(
            ToolHelpFlags dwFlags,
            int th32ProcessID);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool Process32First(
            MySafeHandle hSnapshot,
           [MarshalAs(UnmanagedType.Struct)][In][Out] ref ProcessEntry32 lppe);

        [DllImport("Kernel32.dll")]
        internal static extern bool Process32Next(
            MySafeHandle hSnapshot,
            [MarshalAs(UnmanagedType.Struct)][In][Out] ref ProcessEntry32 lppe);




        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool Module32First(
            MySafeHandle hSnapshot,
           [MarshalAs(UnmanagedType.Struct)][In][Out] ref MODULEENTRY32 lppe);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool Module32Next(
            MySafeHandle hSnapshot,
            [MarshalAs(UnmanagedType.Struct)][In][Out] ref MODULEENTRY32 lppe);
    }
    [Flags]
    enum ToolHelpFlags : uint
    {
        TH32CS_SNAPPROCESS = 0x00000002,
        TH32CS_SNAPNOHEAPS = 0x40000000,
        TH32CS_SNAPMODULE = 0x00000008,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessEntry32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ProcessID;
        public IntPtr th32DefaultHeapID; 
        public uint th32ModuleID;
        public uint cntThreads;
        public uint th32ParentProcessID;
        public int pcPriClassBase;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExeFile;
    }

    [StructLayout(LayoutKind.Sequential, CharSet =CharSet.Ansi)]
    public struct MODULEENTRY32
    {
        /*[MarshalAs(UnmanagedType.U4)]*/ public uint dwSize;
        public uint th32ModuleID;
        public uint th32ProcessID;
        public uint GlblcntUsage;
        public uint ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)] public string szModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 261)] public string szExePath;
    }
    #endregion
}
