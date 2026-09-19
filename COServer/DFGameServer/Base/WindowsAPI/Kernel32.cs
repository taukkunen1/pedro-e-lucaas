using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GameServer.WindowsAPI
{
#if USE_WINDOWS_API
    // Only use this class for Windows API Dependent features like Console Handler :)
    public class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleTitle(string lpConsoleTitle);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsole(IntPtr hConsoleInput, [Out] StringBuilder lpBuffer, uint nNumberOfCharsToRead, out uint lpNumberOfCharsRead, IntPtr lpReserved);
        [DllImport("kernel32.dll")]
        public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);
        [DllImport("kernel32.dll")]
        public static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);

        [DllImport("kernel32.dll")]
        public static extern uint GetConsoleTitle([Out] StringBuilder lpConsoleTitle, uint nSize);


        [DllImport("kernel32")]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AttachConsole(uint dwProcessId);
    }
    #endif
}