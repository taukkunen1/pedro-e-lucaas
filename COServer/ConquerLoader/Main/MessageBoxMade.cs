namespace TrinityConquerLoader
{
    using Properties;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class MessageBoxMade : Form
    {
        private BackgroundWorker backgroundWorker1;
        private readonly IContainer components;
        private Label label1;
        private Label label3;
        private Button btnPlay;
        private PictureBox pictureBox1;
        private Panel panelBar;
        private Button btnClose;
        private bool isDragging = false;
        private bool useDesignSystem = false;


        public MessageBoxMade()
        {
            this.InitializeComponent();
        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Process launch = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = "Dragon.Launch.exe", // Dragon.Launch.exe
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true,
                Verb = "runas",
                Arguments = string.Format("{0} {1} {2}",
                Application.StartupPath, // Path to top directory
                "Conquer.exe",
                 "blacknull"), // blacknull
            });

            if (launch == null)
            {
                MessageBox.Show("Conquer executable not found or cannot execute (Try with Administrator Privilegies)", "TrinityConquerLoader", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            Application.Exit();
            Environment.Exit(0);
        }

        private void ConquerProcess_Exited(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("Conquer").Length <= 0)
            {
                Application.Exit(); // Normal close app
                Environment.Exit(0); // Force close
            }
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBoxMade));
            this.label1 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label3 = new System.Windows.Forms.Label();
            this.btnPlay = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelBar = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(867, 671);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Loading TrinityConquer...";
            this.label1.Visible = false;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(6, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(455, 27);
            this.label3.TabIndex = 3;
            this.label3.Text = "It will close after loading automatically.";
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.SystemColors.ControlText;
            this.btnPlay.FlatAppearance.BorderSize = 0;
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlay.ForeColor = System.Drawing.SystemColors.Control;
            this.btnPlay.Location = new System.Drawing.Point(12, 706);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(997, 50);
            this.btnPlay.TabIndex = 4;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = false;
            this.btnPlay.Click += new System.EventHandler(this.button1_Click);
            this.btnPlay.MouseEnter += new System.EventHandler(this.btnPlay_MouseEnter);
            this.btnPlay.MouseLeave += new System.EventHandler(this.btnPlay_MouseLeave);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::Properties.Resources.trinityconquer;
            this.pictureBox1.Location = new System.Drawing.Point(608, 52);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(404, 89);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // panelBar
            // 
            this.panelBar.BackColor = System.Drawing.Color.Transparent;
            this.panelBar.Controls.Add(this.btnClose);
            this.panelBar.Location = new System.Drawing.Point(0, 0);
            this.panelBar.Name = "panelBar";
            this.panelBar.Size = new System.Drawing.Size(1024, 40);
            this.panelBar.TabIndex = 12;
            this.panelBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelBar_MouseDown);
            this.panelBar.MouseEnter += new System.EventHandler(this.PanelBar_MouseEnter);
            this.panelBar.MouseLeave += new System.EventHandler(this.PanelBar_MouseLeave);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.SystemColors.ControlText;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.SystemColors.Control;
            this.btnClose.Location = new System.Drawing.Point(993, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(28, 34);
            this.btnClose.TabIndex = 13;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.btnClose.MouseEnter += new System.EventHandler(this.ButtonClose_MouseEnter);
            this.btnClose.MouseLeave += new System.EventHandler(this.ButtonClose_MouseLeave);
            // 
            // MessageBoxMade
            // 
            this.BackgroundImage = global::Properties.Resources.AsianBG;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.ControlBox = false;
            this.Controls.Add(this.panelBar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MessageBoxMade";
            this.ShowIcon = false;
            this.Text = "TrinityConquerLoader";
            this.Load += new System.EventHandler(this.MessageBoxMade_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursorFromFile(string fileName);

        private void MessageBoxMade_Load(object sender, EventArgs e)
        {
            SetCursor();
            if (useDesignSystem) {
                btnPlay.BackgroundImageLayout = ImageLayout.Zoom;
                btnPlay.BackgroundImage = Resources.ButtonNormal;
                btnPlay.MouseEnter += BtnPlay_MouseEnter;
                btnPlay.MouseLeave += BtnPlay_MouseLeave;
                btnPlay.MouseDown += BtnPlay_MouseDown;
                btnPlay.Width = Resources.ButtonNormal.Width;
                btnPlay.Height = Resources.ButtonNormal.Height;
                btnPlay.ForeColor = Color.White;
            }
        }
        private void SetCursor()
        {
            byte[] aniCursorBytes = Resources.CursorConquer;
            string tempFilePath = Path.Combine(Path.GetTempPath(), "cursorConquer.ani");
            File.WriteAllBytes(tempFilePath, aniCursorBytes);
            IntPtr cursorHandle = LoadCursorFromFile(tempFilePath);
            if (cursorHandle == IntPtr.Zero)
            {
                MessageBox.Show("Error loading the animated cursor for mouse.");
                return;
            }
            this.Cursor = new Cursor(cursorHandle);
            File.Delete(tempFilePath);
        }

        private void BtnPlay_MouseDown(object sender, MouseEventArgs e)
        {
            btnPlay.ForeColor = Color.White;
            btnPlay.BackgroundImage = Resources.ButtonClick;
        }

        private void BtnPlay_MouseLeave(object sender, EventArgs e)
        {
            btnPlay.ForeColor = Color.White;
            btnPlay.BackgroundImage = Resources.ButtonNormal;
        }

        private void BtnPlay_MouseEnter(object sender, EventArgs e)
        {
            btnPlay.ForeColor = Color.White;
            btnPlay.BackgroundImage = Resources.ButtonHover;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists("Conquer.exe"))
            {
                MessageBox.Show("Conquer.exe not found!", "TrinityConquerLoader", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            } else
            {
                label1.Visible = true;
                this.backgroundWorker1.RunWorkerAsync();
            }
        }

        private void btnPlay_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).ForeColor = Color.Black;
            (sender as Button).BackColor = Color.White;
            (sender as Button).Text = "Launch now";
        }

        private void btnPlay_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).ForeColor = Color.White;
            (sender as Button).BackColor = Color.Black;
            (sender as Button).Text = "Play";
        }

        private void PanelBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                isDragging = false;
            }
        }

        private void ButtonClose_MouseEnter(object sender, EventArgs e)
        {
            (sender as Button).ForeColor = Color.Black;
            (sender as Button).BackColor = Color.White;
        }

        private void ButtonClose_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).ForeColor = Color.White;
            (sender as Button).BackColor = Color.Black;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void PanelBar_MouseEnter(object sender, EventArgs e)
        {
            (sender as Panel).BackColor = Color.FromArgb(200, Color.Gray);
        }

        private void PanelBar_MouseLeave(object sender, EventArgs e)
        {
            if (!isDragging)
            {
                (sender as Panel).BackColor = Color.Transparent;
            }
        }
    }
}
