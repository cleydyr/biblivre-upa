using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace AssistenteDeReparo
{
    public static class Elevacao
    {
        public static bool EstaElevado()
        {
            try
            {
                using (var id = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(id);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Relança o processo com UAC, passando o drive a reparar.
        /// </summary>
        public static bool RelancarElevadoParaReparo(string driveRoot)
        {
            try
            {
                var letra = string.IsNullOrEmpty(driveRoot)
                    ? ""
                    : driveRoot.TrimEnd('\\', '/').Substring(0, 1);

                var psi = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "--reparar --drive=" + letra,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(psi);
                return true;
            }
            catch (Exception)
            {
                // Usuário cancelou o UAC
                return false;
            }
        }
    }
}
