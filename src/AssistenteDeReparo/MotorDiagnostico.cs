using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssistenteDeReparo
{
    public static class MotorDiagnostico
    {
        public static Diagnostico Executar(InstalacaoEncontrada selecionada)
        {
            var d = new Diagnostico();
            List<string> drives;
            d.Instalacoes = Descoberta.EncontrarInstalacoes(out drives);
            d.DrivesVarridos = drives;

            foreach (var drive in drives)
                d.LinhasTecnicas.Add("Drive fixo varrido: " + drive);

            if (d.Instalacoes.Count == 0)
            {
                d.ProblemasOperador.Add("Não encontrei o Biblivre neste computador.");
                d.LinhasTecnicas.Add("Instalação Ausente: nenhum Padrão de Instalação.");
                d.PodeReparar = false;
                d.TudoOk = false;
                return d;
            }

            if (selecionada == null && d.Instalacoes.Count == 1)
                selecionada = d.Instalacoes[0];

            if (selecionada == null)
            {
                d.ProblemasOperador.Add("Encontrei mais de uma instalação do Biblivre. Escolha o disco.");
                d.PodeReparar = false;
                d.TudoOk = false;
                return d;
            }

            // Rebind to fresh discovery entry for same drive
            d.InstalacaoSelecionada = d.Instalacoes.FirstOrDefault(i =>
                string.Equals(i.DriveRoot, selecionada.DriveRoot, StringComparison.OrdinalIgnoreCase))
                ?? selecionada;

            var inst = d.InstalacaoSelecionada;
            d.LinhasTecnicas.Add("Instalação selecionada: " + inst.RotuloOperador);
            d.LinhasTecnicas.Add("Contexto Biblivre5: " + inst.ContextoBiblivre5);
            d.LinhasTecnicas.Add("Home Tomcat existe: " + Directory.Exists(inst.HomeTomcat) + " — " + inst.HomeTomcat);
            d.LinhasTecnicas.Add("Home Apache existe: " + Directory.Exists(inst.HomeApache) + " — " + inst.HomeApache);
            d.LinhasTecnicas.Add("Home Postgres existe: " + Directory.Exists(inst.HomePostgres) + " — " + inst.HomePostgres);

            d.Tomcat = VerificadorServicos.Verificar(InstalacaoPaths.ServicoTomcat);
            d.Apache = VerificadorServicos.Verificar(InstalacaoPaths.ServicoApache);
            d.Postgres = VerificadorServicos.Verificar(InstalacaoPaths.ServicoPostgres);

            RegistrarServico(d, d.Tomcat, "O programa da biblioteca (servidor de aplicação)");
            RegistrarServico(d, d.Apache, "O acesso pela rede local (servidor web)");
            RegistrarServico(d, d.Postgres, "O banco de dados da biblioteca");

            d.JresUsaveis = BuscadorJre.EncontrarJresUsaveis(drives);
            d.LinhasTecnicas.Add("JREs Usáveis encontradas: " + d.JresUsaveis.Count);
            foreach (var j in d.JresUsaveis)
                d.LinhasTecnicas.Add("  JRE: " + j.Home + " | " + Truncar(j.VersaoSaida, 120));

            d.JvmTomcat = JvmDoTomcat.Ler();
            d.LinhasTecnicas.Add("JVM do Tomcat: " + d.JvmTomcat.Mensagem);
            d.LinhasTecnicas.Add("  Registro: " + d.JvmTomcat.CaminhoRegistro);
            d.LinhasTecnicas.Add("  Valor: " + (d.JvmTomcat.ValorJvm ?? "(null)"));

            AvaliarJvm(d);

            d.SmokeTomcat = SmokeHttp.Verificar(InstalacaoPaths.SmokeTomcatUrl);
            d.SmokeApache = SmokeHttp.Verificar(InstalacaoPaths.SmokeApacheUrl);
            RegistrarSmoke(d, d.SmokeTomcat, "O Biblivre não responde no endereço interno do servidor.");
            RegistrarSmoke(d, d.SmokeApache, "O Biblivre não abre pelo endereço usual no navegador.");

            d.TudoOk = d.ProblemasOperador.Count == 0
                && d.SmokeTomcat.Sucesso
                && d.SmokeApache.Sucesso
                && d.Tomcat.Estado == EstadoServico.EmExecucao
                && d.Apache.Estado == EstadoServico.EmExecucao
                && d.Postgres.Estado == EstadoServico.EmExecucao
                && d.JvmTomcat.Saudavel;

            if (d.TudoOk)
            {
                d.ProblemasOperador.Clear();
                d.ProblemasOperador.Add("Tudo certo: o Biblivre parece estar funcionando.");
            }

            d.PodeReparar = !d.TudoOk && inst != null;
            return d;
        }

        private static void RegistrarServico(Diagnostico d, ResultadoServico s, string rotuloOperador)
        {
            d.LinhasTecnicas.Add("Serviço " + s.Nome + ": " + s.Estado + " (" + s.Detalhe + ")");
            if (s.Estado == EstadoServico.Ausente)
                d.ProblemasOperador.Add(rotuloOperador + " não está instalado como esperado.");
            else if (s.Estado != EstadoServico.EmExecucao)
                d.ProblemasOperador.Add(rotuloOperador + " não está em execução.");
        }

        private static void AvaliarJvm(Diagnostico d)
        {
            if (d.JvmTomcat.Modo == ModoJvmTomcat.Invalido)
            {
                d.ProblemasOperador.Add("Faltou o Java do servidor (configuração inválida ou vazia).");
                d.JvmTomcat.Saudavel = false;
                return;
            }

            if (d.JvmTomcat.Modo == ModoJvmTomcat.Explicita && !d.JvmTomcat.CaminhoExplicitoExiste)
            {
                d.ProblemasOperador.Add("O Java que o servidor deveria usar não foi encontrado.");
                d.JvmTomcat.Saudavel = false;
                return;
            }

            if (d.JvmTomcat.Modo == ModoJvmTomcat.Automatica)
            {
                if (d.JresUsaveis.Count == 0)
                {
                    d.ProblemasOperador.Add("Não há um Java utilizável neste computador.");
                    d.JvmTomcat.Saudavel = false;
                }
                else
                {
                    d.JvmTomcat.Saudavel = true;
                }
                return;
            }

            // Explícita existente: sanity se possível
            if (d.JvmTomcat.Modo == ModoJvmTomcat.Explicita && d.JvmTomcat.CaminhoExplicitoExiste)
            {
                var home = DerivarHomeDeJvmDll(d.JvmTomcat.ValorJvm);
                var javaExe = home == null ? null : Path.Combine(home, "bin", "java.exe");
                string versao;
                if (javaExe != null && File.Exists(javaExe) && !BuscadorJre.SanityJava(javaExe, out versao))
                {
                    d.ProblemasOperador.Add("O Java configurado no servidor não responde corretamente.");
                    d.JvmTomcat.Saudavel = false;
                    d.LinhasTecnicas.Add("Sanity da JVM explícita falhou: " + versao);
                }
                else
                {
                    d.JvmTomcat.Saudavel = true;
                }
            }
        }

        private static void RegistrarSmoke(Diagnostico d, ResultadoSmoke s, string msgOperador)
        {
            d.LinhasTecnicas.Add("Smoke " + s.Url
                + " | ok=" + s.Sucesso
                + " | status=" + s.StatusCode
                + " | marcador=" + s.MarcadorEncontrado
                + (string.IsNullOrEmpty(s.Erro) ? "" : " | erro=" + s.Erro));

            if (!s.Sucesso)
                d.ProblemasOperador.Add(msgOperador);
        }

        private static string DerivarHomeDeJvmDll(string jvmDll)
        {
            try
            {
                // ...\jre\bin\server\jvm.dll -> ...\jre
                var serverDir = Path.GetDirectoryName(jvmDll);
                var binDir = serverDir != null ? Path.GetDirectoryName(serverDir) : null;
                return binDir != null ? Path.GetDirectoryName(binDir) : null;
            }
            catch
            {
                return null;
            }
        }

        private static string Truncar(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }

        public static string TextoOperador(Diagnostico d)
        {
            if (d == null) return "";
            var sb = new StringBuilder();
            var vistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in d.ProblemasOperador)
            {
                if (vistos.Add(p))
                    sb.AppendLine("• " + p);
            }
            return sb.ToString().TrimEnd();
        }
    }
}
