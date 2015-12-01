using System;
using System.Windows.Forms;

namespace ModPackager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args == null || args.Length == 0 || args[0] == "Make")
                Application.Run(new Form1());
            else if (args[0] == "Install")
                Application.Run(new Form2());
        }
    }
}