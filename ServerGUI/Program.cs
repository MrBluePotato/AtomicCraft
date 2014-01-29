using System;
using System.Windows.Forms;
using System.Net;

namespace fCraft.ServerGUI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            Application.Run(new MainForm());
#else
            try {
                Application.Run( new MainForm() );
            } catch( Exception ex ) {
                Logger.LogAndReportCrash( "Unhandled exception in ServerGUI", "ServerGUI", ex, true );
            }
#endif
        }
    }
}