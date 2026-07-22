using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace AssistenteDeReparo
{
    public static class MotorReparo
    {
        public static ResultadoReparo Executar(InstalacaoEncontrada instalacao)
        {
            var r = new ResultadoReparo();
            if (instalacao == null)
            {
                r.Erros.Add("Nenhuma Instalação selecionada.");
                return r;
            }

            IniciarSeParado(InstalacaoPaths.ServicoPostgres, r);
            IniciarSeParado(InstalacaoPaths.ServicoTomcat, r);
            IniciarSeParado(InstalacaoPaths.ServicoApache, r);

            var jvmAlterada = CorrigirJvmSeNecessario(instalacao, r);

            Thread.Sleep(2000);
            var smokeTomcat = SmokeHttp.Verificar(InstalacaoPaths.SmokeTomcatUrl);
            var smokeApache = SmokeHttp.Verificar(InstalacaoPaths.SmokeApacheUrl);

            if (!smokeTomcat.Sucesso || jvmAlterada)
            {
                Reiniciar(InstalacaoPaths.ServicoTomcat, r);
                Thread.Sleep(3000);
            }

            if (!smokeApache.Sucesso)
            {
                Reiniciar(InstalacaoPaths.ServicoApache, r);
                Thread.Sleep(2000);
            }

            r.PosReparo = MotorDiagnostico.Executar(instalacao);
            return r;
        }

        private static bool CorrigirJvmSeNecessario(InstalacaoEncontrada instalacao, ResultadoReparo r)
        {
            var jvm = JvmDoTomcat.Ler();
            var jres = BuscadorJre.EncontrarJresUsaveis(ListarDrivesFixos());

            var precisaExplicita =
                jvm.Modo == ModoJvmTomcat.Invalido
                || (jvm.Modo == ModoJvmTomcat.Explicita && !jvm.CaminhoExplicitoExiste)
                || (jvm.Modo == ModoJvmTomcat.Automatica && jres.Count == 0);

            if (!precisaExplicita)
                return false;

            var preferida = BuscadorJre.Preferida(jres, instalacao.DriveRoot);
            if (preferida == null)
            {
                r.Erros.Add("Não há JRE Usável para gravar JVM Explícita.");
                return false;
            }

            try
            {
                JvmDoTomcat.GravarExplicita(preferida.JvmDll);
                r.Acoes.Add("JVM Explícita gravada: " + preferida.JvmDll);
                return true;
            }
            catch (Exception ex)
            {
                r.Erros.Add("Falha ao gravar JVM Explícita: " + ex.Message);
                return false;
            }
        }

        private static List<string> ListarDrivesFixos()
        {
            var list = new List<string>();
            foreach (var d in DriveInfo.GetDrives())
            {
                if (d.DriveType == DriveType.Fixed && d.IsReady)
                    list.Add(d.RootDirectory.FullName);
            }
            return list;
        }

        private static void IniciarSeParado(string nome, ResultadoReparo r)
        {
            try
            {
                using (var sc = new ServiceController(nome))
                {
                    if (sc.Status == ServiceControllerStatus.Running) return;
                    if (sc.Status == ServiceControllerStatus.StartPending)
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                        return;
                    }

                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                    r.Acoes.Add("Serviço iniciado: " + nome);
                }
            }
            catch (Exception ex)
            {
                r.Erros.Add("Não foi possível iniciar " + nome + ": " + ex.Message);
            }
        }

        private static void Reiniciar(string nome, ResultadoReparo r)
        {
            try
            {
                using (var sc = new ServiceController(nome))
                {
                    if (sc.Status == ServiceControllerStatus.Running
                        || sc.Status == ServiceControllerStatus.StartPending)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(60));
                    }

                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(60));
                    r.Acoes.Add("Serviço reiniciado: " + nome);
                }
            }
            catch (Exception ex)
            {
                r.Erros.Add("Não foi possível reiniciar " + nome + ": " + ex.Message);
            }
        }
    }
}
