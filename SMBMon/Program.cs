using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMBMon
{
    static class Program
    {
        public static MainForm MainForm;
        public static NTFilteredFileSystem FilteredFileSystem;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new MainForm();
            SMBLog.InitLog();
            Application.Run(MainForm);
        }
    }
}
