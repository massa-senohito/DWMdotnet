using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TileManTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Form1 mainForm = null;
            try
            {
                Application.EnableVisualStyles( );
                Application.SetCompatibleTextRenderingDefault( false );
                mainForm = new Form1( );
                Application.Run( mainForm );
            }
            catch ( Exception exn )
            {
                DebugLogger.GlobalLogger.Error( exn );
                mainForm.CleanUp( );
            }
        }
    }
}
