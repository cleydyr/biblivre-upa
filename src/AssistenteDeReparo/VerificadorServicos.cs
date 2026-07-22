using System;
using System.ServiceProcess;

namespace AssistenteDeReparo
{
    public static class VerificadorServicos
    {
        public static ResultadoServico Verificar(string nomeServico)
        {
            try
            {
                using (var sc = new ServiceController(nomeServico))
                {
                    var status = sc.Status;
                    if (status == ServiceControllerStatus.Running)
                    {
                        return new ResultadoServico
                        {
                            Nome = nomeServico,
                            Estado = EstadoServico.EmExecucao,
                            Detalhe = status.ToString()
                        };
                    }

                    if (status == ServiceControllerStatus.Stopped
                        || status == ServiceControllerStatus.StopPending
                        || status == ServiceControllerStatus.StartPending
                        || status == ServiceControllerStatus.Paused)
                    {
                        return new ResultadoServico
                        {
                            Nome = nomeServico,
                            Estado = EstadoServico.Parado,
                            Detalhe = status.ToString()
                        };
                    }

                    return new ResultadoServico
                    {
                        Nome = nomeServico,
                        Estado = EstadoServico.Outro,
                        Detalhe = status.ToString()
                    };
                }
            }
            catch (InvalidOperationException ex)
            {
                return new ResultadoServico
                {
                    Nome = nomeServico,
                    Estado = EstadoServico.Ausente,
                    Detalhe = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new ResultadoServico
                {
                    Nome = nomeServico,
                    Estado = EstadoServico.Ausente,
                    Detalhe = ex.Message
                };
            }
        }
    }
}
