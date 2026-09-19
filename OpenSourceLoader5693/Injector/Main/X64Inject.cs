namespace TrinityConquerLoader
{
    using System;
    using System.Runtime.InteropServices;

    public class X64Inject
    {
        [DllImport("wow64ext.dll")]
        public static extern int CloseHandle(IntPtr hObject);
        [DllImport("wow64ext.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);
        [DllImport("wow64ext.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("wow64ext.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
        public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("wow64ext.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);
        [DllImport("wow64ext.dll", SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("wow64ext.dll", SetLastError=true, ExactSpelling=true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);
        [DllImport("wow64ext.dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int WaitForSingleObject(IntPtr handle, int milliseconds);
        [DllImport("wow64ext.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);
    }
}

