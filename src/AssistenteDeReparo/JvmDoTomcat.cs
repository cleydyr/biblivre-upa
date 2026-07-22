using System;
using System.IO;
using Microsoft.Win32;

namespace AssistenteDeReparo
{
    public static class JvmDoTomcat
    {
        private static readonly string[] ChavesJava =
        {
            @"SOFTWARE\Apache Software Foundation\Procrun 2.0\Tomcat7\Parameters\Java",
            @"SOFTWARE\WOW6432Node\Apache Software Foundation\Procrun 2.0\Tomcat7\Parameters\Java"
        };

        public static ResultadoJvmTomcat Ler()
        {
            foreach (var chave in ChavesJava)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(chave))
                    {
                        if (key == null) continue;

                        var jvm = key.GetValue("Jvm") as string;
                        return Avaliar(jvm, @"HKLM\" + chave);
                    }
                }
                catch (Exception ex)
                {
                    return new ResultadoJvmTomcat
                    {
                        Modo = ModoJvmTomcat.Invalido,
                        Saudavel = false,
                        CaminhoRegistro = @"HKLM\" + chave,
                        Mensagem = ex.Message
                    };
                }
            }

            return new ResultadoJvmTomcat
            {
                Modo = ModoJvmTomcat.Invalido,
                Saudavel = false,
                Mensagem = "Registro Procrun do Tomcat7 não encontrado"
            };
        }

        private static ResultadoJvmTomcat Avaliar(string jvm, string caminhoRegistro)
        {
            var r = new ResultadoJvmTomcat
            {
                ValorJvm = jvm,
                CaminhoRegistro = caminhoRegistro
            };

            if (string.IsNullOrWhiteSpace(jvm))
            {
                r.Modo = ModoJvmTomcat.Invalido;
                r.Saudavel = false;
                r.Mensagem = "JVM vazia (Use default desmarcado sem path)";
                return r;
            }

            if (string.Equals(jvm.Trim(), "auto", StringComparison.OrdinalIgnoreCase))
            {
                r.Modo = ModoJvmTomcat.Automatica;
                r.Saudavel = true;
                r.Mensagem = "JVM Automática (auto)";
                return r;
            }

            r.Modo = ModoJvmTomcat.Explicita;
            r.CaminhoExplicitoExiste = File.Exists(jvm);
            if (!r.CaminhoExplicitoExiste)
            {
                r.Saudavel = false;
                r.Mensagem = "JVM Explícita aponta para arquivo inexistente: " + jvm;
                return r;
            }

            r.Saudavel = true;
            r.Mensagem = "JVM Explícita: " + jvm;
            return r;
        }

        public static void GravarExplicita(string caminhoJvmDll)
        {
            if (string.IsNullOrWhiteSpace(caminhoJvmDll))
                throw new ArgumentException("caminhoJvmDll");

            if (!File.Exists(caminhoJvmDll))
                throw new FileNotFoundException("jvm.dll não encontrada", caminhoJvmDll);

            Exception ultimo = null;
            foreach (var chave in ChavesJava)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(chave, writable: true))
                    {
                        if (key == null) continue;
                        key.SetValue("Jvm", caminhoJvmDll, RegistryValueKind.String);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ultimo = ex;
                }
            }

            // Cria a chave se o serviço existe mas a árvore Java sumiu
            try
            {
                using (var key = Registry.LocalMachine.CreateSubKey(ChavesJava[0]))
                {
                    if (key == null) throw new InvalidOperationException("Não foi possível criar a chave Procrun.");
                    key.SetValue("Jvm", caminhoJvmDll, RegistryValueKind.String);
                    return;
                }
            }
            catch (Exception ex)
            {
                if (ultimo != null)
                    throw new InvalidOperationException(ultimo.Message + " | " + ex.Message, ex);
                throw;
            }
        }
    }
}
