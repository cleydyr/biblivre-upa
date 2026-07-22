using System;
using System.Collections.Generic;
using System.IO;

namespace AssistenteDeReparo
{
    public static class Descoberta
    {
        public static List<InstalacaoEncontrada> EncontrarInstalacoes(out List<string> drivesVarridos)
        {
            var resultado = new List<InstalacaoEncontrada>();
            drivesVarridos = new List<string>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed) continue;
                if (!drive.IsReady) continue;

                var root = drive.RootDirectory.FullName;
                drivesVarridos.Add(root);

                var contexto = InstalacaoPaths.ContextoNoDrive(root);
                if (!Directory.Exists(contexto)) continue;

                resultado.Add(new InstalacaoEncontrada
                {
                    DriveRoot = root,
                    ContextoBiblivre5 = contexto,
                    HomeTomcat = InstalacaoPaths.TomcatNoDrive(root),
                    HomeApache = InstalacaoPaths.ApacheNoDrive(root),
                    HomePostgres = InstalacaoPaths.PostgresNoDrive(root)
                });
            }

            return resultado;
        }
    }
}
