namespace TrinityConquerLoader
{
    using System;
    using System.Windows.Forms;

    internal static class Program
    {
        [MTAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); 
            Application.Run(new MessageBoxMade());           
        }
    }
}

