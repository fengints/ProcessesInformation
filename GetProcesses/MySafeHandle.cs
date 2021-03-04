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

namespace GetProcesses
{
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public class MySafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        // Create a SafeHandle, informing the base class
        // that this SafeHandle instance "owns" the handle,
        // and therefore SafeHandle should call
        // our ReleaseHandle method when the SafeHandle
        // is no longer in use.
        private MySafeHandle()
            : base(true)
        {
        }
        public MySafeHandle(IntPtr handle) : base(true)
        {
            this.SetHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        override protected bool ReleaseHandle()
        {
            // Here, we must obey all rules for constrained execution regions.
            return NativeMethods.CloseHandle(handle);
        }
    }

    [SuppressUnmanagedCodeSecurity()]
    internal static class NativeMethods
    {
        // Win32 constants for accessing files.
        internal const int GENERIC_READ = unchecked((int)0x80000000);

        // Allocate a file object in the kernel, then return a handle to it.
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        internal extern static MySafeHandle CreateFile(String fileName,
           int dwDesiredAccess, System.IO.FileShare dwShareMode,
           IntPtr securityAttrs_MustBeZero, System.IO.FileMode dwCreationDisposition,
           int dwFlagsAndAttributes, IntPtr hTemplateFile_MustBeZero);

        // Use the file handle.
        [DllImport("kernel32", SetLastError = true)]
        internal extern static int ReadFile(MySafeHandle handle, byte[] bytes,
           int numBytesToRead, out int numBytesRead, IntPtr overlapped_MustBeZero);

        // Free the kernel's file object (close the file).
        [DllImport("kernel32", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal extern static bool CloseHandle(IntPtr handle);
    }
}
