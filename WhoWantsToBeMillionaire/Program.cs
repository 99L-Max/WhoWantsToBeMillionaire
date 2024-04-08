using System;
using System.Threading;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(ShowMessage);
            Application.Run(new MainForm());
        }

        private static void ShowMessage(object sender, ThreadExceptionEventArgs e) =>
            MessageBox.Show(e.Exception.ToString());
    }
}
