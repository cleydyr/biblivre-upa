using System;
using System.Windows.Forms;

namespace AssistenteDeReparo
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool reparar = false;
            string drive = null;

            if (args != null)
            {
                foreach (var arg in args)
                {
                    if (string.Equals(arg, "--reparar", StringComparison.OrdinalIgnoreCase))
                        reparar = true;
                    else if (arg.StartsWith("--drive=", StringComparison.OrdinalIgnoreCase))
                        drive = arg.Substring("--drive=".Length).Trim();
                }
            }

            Application.Run(new MainForm(reparar, drive));
        }
    }
}
