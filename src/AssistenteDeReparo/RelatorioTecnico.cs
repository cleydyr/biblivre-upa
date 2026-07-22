using System;
using System.IO;
using System.Text;

namespace AssistenteDeReparo
{
    public static class RelatorioTecnico
    {
        public static string Salvar(Diagnostico d, ResultadoReparo reparo = null)
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "Biblivre-Relatorios");
            Directory.CreateDirectory(dir);

            var nome = "relatorio-biblivre-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            var caminho = Path.Combine(dir, nome);

            var sb = new StringBuilder();
            sb.AppendLine("Relatório Técnico — Assistente de Reparo Biblivre 5");
            sb.AppendLine("Gerado em: " + DateTime.Now.ToString("o"));
            sb.AppendLine("Máquina: " + Environment.MachineName);
            sb.AppendLine("Usuário: " + Environment.UserName);
            sb.AppendLine("Elevado: " + Elevacao.EstaElevado());
            sb.AppendLine(new string('-', 60));

            if (d != null)
            {
                sb.AppendLine("Drives varridos:");
                foreach (var drive in d.DrivesVarridos)
                    sb.AppendLine("  " + drive);

                sb.AppendLine();
                sb.AppendLine("Instalações encontradas: " + d.Instalacoes.Count);
                foreach (var i in d.Instalacoes)
                {
                    sb.AppendLine("  " + i.RotuloOperador);
                    sb.AppendLine("    Contexto: " + i.ContextoBiblivre5);
                    sb.AppendLine("    Tomcat:   " + i.HomeTomcat);
                    sb.AppendLine("    Apache:   " + i.HomeApache);
                    sb.AppendLine("    Postgres: " + i.HomePostgres);
                }

                sb.AppendLine();
                sb.AppendLine("Linhas técnicas:");
                foreach (var linha in d.LinhasTecnicas)
                    sb.AppendLine("  " + linha);

                sb.AppendLine();
                sb.AppendLine("Mensagens ao Operador:");
                foreach (var p in d.ProblemasOperador)
                    sb.AppendLine("  " + p);
            }

            if (reparo != null)
            {
                sb.AppendLine();
                sb.AppendLine("Reparos executados:");
                foreach (var a in reparo.Acoes)
                    sb.AppendLine("  OK: " + a);
                foreach (var e in reparo.Erros)
                    sb.AppendLine("  ERRO: " + e);

                if (reparo.PosReparo != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("Diagnóstico pós-reparo:");
                    sb.AppendLine(MotorDiagnostico.TextoOperador(reparo.PosReparo));
                    foreach (var linha in reparo.PosReparo.LinhasTecnicas)
                        sb.AppendLine("  " + linha);
                }
            }

            File.WriteAllText(caminho, sb.ToString(), Encoding.UTF8);
            return caminho;
        }
    }
}
