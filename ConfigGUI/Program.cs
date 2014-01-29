using System;
using System.Windows.Forms;

namespace fCraft.ConfigGUI
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            Application.Run(new MainForm());
#else
            try {
                Application.Run( new MainForm() );
            } catch( Exception ex ) {
                Logger.LogAndReportCrash( "Error in ConfigGUI", "ConfigGUI", ex, true );
                if( !Server.HasArg( ArgKey.ExitOnCrash ) ) {
                    MessageBox.Show( ex.ToString(), "Configuration has crashed" );
                }
            }
#endif
        }
    }
}