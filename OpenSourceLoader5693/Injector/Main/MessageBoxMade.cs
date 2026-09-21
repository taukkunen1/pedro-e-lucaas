namespace TrinityConquerLoader
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public class MessageBoxMade : Form
    {
        private BackgroundWorker backgroundWorker1;
        private readonly IContainer components;
        private Label label1;
        private Label label3;        
        private ProgressBar progressBar1;
        public static ProcessFactory Factory;
        private static List<string> DetectionProcessNames { get; set; }

        public static string ConquerHookDllFilename { get; set; }
       

        public MessageBoxMade()
        {
            this.InitializeComponent();
            DetectionProcessNames = new List<string>
            {
                "speed",
                "Cheat",
                "Clicker",
                "speedhack",
                "COSpeedv5",
                "Charles"
            };
            ConquerHookDllFilename = "ConquerHook.dll";
            if (File.Exists("ConquerHookDev.dll") && File.Exists("ConquerHookProd.dll"))
            {
                DialogResult dRes = MessageBox.Show("Detected two version hook. You need the development[YES] or the production mode[NO]?", "Try mode opening", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dRes == DialogResult.Yes)
                {
                    ConquerHookDllFilename = "ConquerHookDev.dll";
                } else
                {
                    ConquerHookDllFilename = "ConquerHookProd.dll";
                }
            }
            this.backgroundWorker1.RunWorkerAsync();
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            PROCESS_INFORMATION lpProcessInformation = new PROCESS_INFORMATION();
            SECURITY_ATTRIBUTES structure = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES security_attributes2 = new SECURITY_ATTRIBUTES();
            STARTUPINFO lpStartupInfo = new STARTUPINFO();
            structure.nLength = Marshal.SizeOf(structure);
            security_attributes2.nLength = Marshal.SizeOf(security_attributes2);
            if (!CreateProcess(Application.StartupPath + @"\Conquer.exe", " blacknull", ref structure, ref security_attributes2, false, 0x4000000, IntPtr.Zero, null, ref lpStartupInfo, out lpProcessInformation))
            {
                MessageBox.Show("Conquer executable not found or cannot execute (Try with Administrator Privilegies)", "Bo0oM-Loader", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Application.Exit();
                Environment.Exit(0);
            }
            else
            {               
                worker.ReportProgress(10);
                Thread.Sleep(0x1b58);
                InjectDLL(lpProcessInformation.hProcess, ConquerHookDllFilename, worker);
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length <= 1)
                {
                    Factory = new ProcessFactory(1);
                    Factory.AddProcess(KillingProg);
                    Factory.Start();
                    this.Hide();
                    Process.GetProcessById(lpProcessInformation.dwProcessId).Exited += ConquerProcess_Exited;
                } else
                {
                    Application.Exit();
                    Environment.Exit(0);
                }
            }
        }

        private void ConquerProcess_Exited(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("Conquer").Length <= 0)
            {
                Application.Exit(); // Normal close app
                Environment.Exit(0); // Force close
            }
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }
        
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);
        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBoxMade));
            this.label1 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(571, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Loading TrinityConquer...";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker1_ProgressChanged);
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.SystemColors.HotTrack;
            this.progressBar1.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.progressBar1.Location = new System.Drawing.Point(12, 6);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(701, 29);
            this.progressBar1.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(9, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(455, 21);
            this.label3.TabIndex = 3;
            this.label3.Text = "It will close after loading automatically.";
            // 
            // MessageBoxMade
            // 
            this.BackgroundImage = global::Properties.Resources.Hero;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(725, 177);
            this.ControlBox = false;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MessageBoxMade";
            this.ShowIcon = false;
            this.Text = "TrinityConquerLoader";
            this.Load += new System.EventHandler(this.MessageBoxMade_Load);
            this.ResumeLayout(false);

        }

        public static bool InjectDLL(IntPtr hProcess, string strDLLName, BackgroundWorker worker)
        {
            IntPtr ptr;
            string str;
            int num = strDLLName.Length + 1;
            IntPtr lpBaseAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (uint) num, 0x1000, 0x40);
            worker.ReportProgress(20);
            if ((lpBaseAddress == IntPtr.Zero) && (lpBaseAddress == IntPtr.Zero))
            {
                str = "Unable to allocate memory to target process.\n";
                str = str + "Error code: " + Marshal.GetLastWin32Error();
                return false;
            }
            WriteProcessMemory(hProcess, lpBaseAddress, strDLLName, (UIntPtr) num, out ptr);
            worker.ReportProgress(50);
            if (Marshal.GetLastWin32Error() != 0x514)
            {
                str = "Please run it as an administrator";
                str = str + "Error code: " + Marshal.GetLastWin32Error();
            }
            else if (Marshal.GetLastWin32Error() != 0)
            {
                str = "Unable to write memory to process.";
                str = str + "Error code: " + Marshal.GetLastWin32Error();
                return false;
            }
            UIntPtr procAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            worker.ReportProgress(60);
            if (procAddress == ((UIntPtr) 0))
            {
                str = "Unable to find address of \"LoadLibraryA\".\n";
                MessageBox.Show(str + "Error code: " + Marshal.GetLastWin32Error());
                return false;
            }
            IntPtr handle = CreateRemoteThread(hProcess, IntPtr.Zero, 0, procAddress, lpBaseAddress, 0, out ptr);
            worker.ReportProgress(80);
            if (handle == IntPtr.Zero)
            {
                str = "Unable to load dll into memory.";
                str = str + "Error code: " + Marshal.GetLastWin32Error();
                return false;
            }
            long num2 = WaitForSingleObject(handle, 0x2710);
            worker.ReportProgress(90);
            switch (num2)
            {
                case 0x80L:
                case 0x102L:
                case 0xffffffffL:
                    CloseHandle(handle);
                    return false;
            }
            Thread.Sleep(0x3e8);
            VirtualFreeEx(hProcess, lpBaseAddress, (UIntPtr) 0, 0x8000);
            worker.ReportProgress(100);
            CloseHandle(handle);
            return true;
        }

        private void MessageBoxMade_Load(object sender, EventArgs e)
        {
            
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);
        [DllImport("kernel32", SetLastError=true, ExactSpelling=true)]
        internal static extern int WaitForSingleObject(IntPtr handle, int milliseconds);
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, UIntPtr nSize, out IntPtr lpNumberOfBytesWritten);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            //public unsafe byte* lpSecurityDescriptor;
            public int bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public static class VAE_Enums
        {
            public enum AllocationType
            {
                MEM_COMMIT = 0x1000,
                MEM_RESERVE = 0x2000,
                MEM_RESET = 0x80000
            }

            public enum ProtectionConstants
            {
                PAGE_EXECUTE = 0x10,
                PAGE_EXECUTE_READ = 0x20,
                PAGE_EXECUTE_READWRITE = 0x40,
                PAGE_EXECUTE_WRITECOPY = 0x80,
                PAGE_NOACCESS = 1
            }
        }

        private static void KillingProg()
        {
            while (true)            
            {
                ProcessKill(DetectionProcessNames);
                Thread.Sleep(2000); // each 2s
            }  
        }

        private static void ProcessKill(List<string> p)
        {
            Process[] Prog = Process.GetProcesses();
            foreach (Process TargetProgram in Prog)
            {
                if (!TargetProgram.ProcessName.Contains("chrome") && !TargetProgram.ProcessName.Contains("edge"))
                {
                    foreach(string pName in p)
                    {
                        if (TargetProgram.MainWindowTitle.Contains(pName) || TargetProgram.ProcessName.Contains(pName) || TargetProgram.ProcessName.StartsWith(pName))
                        {
                            TargetProgram.Kill();
                            MessageBox.Show($"TrinityConquerLoader has detected a Cheat. Report if this is a false positive. [Detected:{TargetProgram.ProcessName}]", "TrinityConquerLoader", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        }
                    }
                }
                Thread.Sleep(0);
            }
        }
    }
}
