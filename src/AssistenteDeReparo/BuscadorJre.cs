using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AssistenteDeReparo
{
    public static class BuscadorJre
    {
        public static List<JreUsavel> EncontrarJresUsaveis(IEnumerable<string> driveRoots)
        {
            var candidatas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var root in driveRoots)
            {
                foreach (var pastaBase in PastasPrioritarias(root))
                {
                    if (!Directory.Exists(pastaBase)) continue;
                    ColetarJvmDlls(pastaBase, candidatas, profundidadeMax: 6);
                }

                VarrerProgramFilesSeletivo(root, candidatas);
            }

            var usaveis = new List<JreUsavel>();
            foreach (var kv in candidatas)
            {
                var jvmDll = kv.Key;
                var home = kv.Value;
                var javaExe = Path.Combine(home, "bin", "java.exe");
                if (!File.Exists(javaExe))
                {
                    javaExe = Path.Combine(home, "bin", "java");
                    if (!File.Exists(javaExe)) continue;
                }

                string versao;
                if (!SanityJava(javaExe, out versao)) continue;

                usaveis.Add(new JreUsavel
                {
                    Home = home,
                    JvmDll = jvmDll,
                    JavaExe = javaExe,
                    VersaoSaida = versao
                });
            }

            return usaveis;
        }

        private static IEnumerable<string> PastasPrioritarias(string root)
        {
            yield return Path.Combine(root, "Program Files", "Java");
            yield return Path.Combine(root, "Program Files (x86)", "Java");
            yield return Path.Combine(root, "Program Files", "AdoptOpenJDK");
            yield return Path.Combine(root, "Program Files", "Eclipse Adoptium");
            yield return Path.Combine(root, "Program Files", "Amazon Corretto");
            yield return Path.Combine(root, "Program Files", "Zulu");
            yield return Path.Combine(root, "Program Files", "BellSoft");
            yield return Path.Combine(root, "Java");
        }

        private static void ColetarJvmDlls(string pasta, Dictionary<string, string> mapa, int profundidadeMax)
        {
            try
            {
                var serverJvm = Path.Combine(pasta, "bin", "server", "jvm.dll");
                if (File.Exists(serverJvm))
                {
                    mapa[serverJvm] = pasta;
                    return;
                }

                if (profundidadeMax <= 0) return;

                foreach (var sub in Directory.EnumerateDirectories(pasta))
                {
                    var nome = Path.GetFileName(sub);
                    if (IgnorarPasta(nome)) continue;
                    ColetarJvmDlls(sub, mapa, profundidadeMax - 1);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }

        /// <summary>
        /// Complementa a busca em pastas típicas: sob Program Files*, só entra em diretórios com "java"/"jre"/"jdk" no nome.
        /// </summary>
        private static void VarrerProgramFilesSeletivo(string root, Dictionary<string, string> mapa)
        {
            foreach (var pf in new[]
            {
                Path.Combine(root, "Program Files"),
                Path.Combine(root, "Program Files (x86)")
            })
            {
                if (!Directory.Exists(pf)) continue;
                try
                {
                    foreach (var sub in Directory.EnumerateDirectories(pf))
                    {
                        var nome = Path.GetFileName(sub) ?? "";
                        var n = nome.ToLowerInvariant();
                        if (n.Contains("java") || n.Contains("jre") || n.Contains("jdk")
                            || n.Contains("adopt") || n.Contains("zulu") || n.Contains("corretto"))
                        {
                            ColetarJvmDlls(sub, mapa, profundidadeMax: 5);
                        }
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }


        private static bool IgnorarPasta(string nome)
        {
            if (string.IsNullOrEmpty(nome)) return true;
            var n = nome.ToLowerInvariant();
            return n == "windows"
                || n == "winsxs"
                || n == "$recycle.bin"
                || n == "system volume information"
                || n == "node_modules"
                || n == ".git"
                || n == "temp"
                || n == "tmp";
        }

        public static bool SanityJava(string javaExe, out string saida)
        {
            saida = null;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = javaExe,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    if (p == null) return false;
                    var err = p.StandardError.ReadToEnd();
                    var std = p.StandardOutput.ReadToEnd();
                    if (!p.WaitForExit(15000))
                    {
                        try { p.Kill(); } catch { }
                        return false;
                    }

                    saida = (err + std).Trim();
                    return p.ExitCode == 0 || !string.IsNullOrEmpty(saida);
                }
            }
            catch (Exception ex)
            {
                saida = ex.Message;
                return false;
            }
        }

        public static JreUsavel Preferida(IList<JreUsavel> jres, string drivePreferido)
        {
            if (jres == null || jres.Count == 0) return null;

            if (!string.IsNullOrEmpty(drivePreferido))
            {
                foreach (var j in jres)
                {
                    if (j.Home != null
                        && j.Home.StartsWith(drivePreferido, StringComparison.OrdinalIgnoreCase))
                        return j;
                }
            }

            return jres[0];
        }
    }
}
