using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Authy_Bluetooth_Sync
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!File.Exists(Environment.CurrentDirectory + "\\" + "authy_sync.config"))
            {
                Application.Run(new FormWarning());
            }
            else
            {
                Application.Run(new Form2());
            }
        }
    }
}
