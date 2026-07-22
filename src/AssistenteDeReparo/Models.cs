using System;
using System.Collections.Generic;

namespace AssistenteDeReparo
{
    public static class InstalacaoPaths
    {
        public const string ContextoRelativo =
            @"Program Files\Apache Software Foundation\Tomcat 7.0\webapps\Biblivre5";

        public const string TomcatRelativo =
            @"Program Files\Apache Software Foundation\Tomcat 7.0";

        public const string ApacheRelativo =
            @"Program Files (x86)\Apache Software Foundation\Apache2.2";

        public const string PostgresRelativo =
            @"Program Files\PostgreSQL\9.1";

        public const string ServicoTomcat = "Tomcat7";
        public const string ServicoApache = "Apache2.2";
        public const string ServicoPostgres = "postgresql-x64-9.1";

        public const string MarcadorLogin = "Core.submitForm('login', 'login', 'jsp')";

        public const string SmokeTomcatUrl = "http://localhost:8080/Biblivre5/";
        public const string SmokeApacheUrl = "http://localhost/Biblivre5/";

        public static string ContextoNoDrive(string root)
        {
            return System.IO.Path.Combine(root, ContextoRelativo);
        }

        public static string TomcatNoDrive(string root)
        {
            return System.IO.Path.Combine(root, TomcatRelativo);
        }

        public static string ApacheNoDrive(string root)
        {
            return System.IO.Path.Combine(root, ApacheRelativo);
        }

        public static string PostgresNoDrive(string root)
        {
            return System.IO.Path.Combine(root, PostgresRelativo);
        }
    }

    public sealed class InstalacaoEncontrada
    {
        public string DriveRoot { get; set; }
        public string ContextoBiblivre5 { get; set; }
        public string HomeTomcat { get; set; }
        public string HomeApache { get; set; }
        public string HomePostgres { get; set; }

        public string LetraDisco
        {
            get
            {
                if (string.IsNullOrEmpty(DriveRoot)) return "?";
                return DriveRoot.TrimEnd('\\', '/').Substring(0, 1).ToUpperInvariant();
            }
        }

        public string RotuloOperador
        {
            get { return "Biblivre no disco " + LetraDisco + ":"; }
        }

        public override string ToString()
        {
            return RotuloOperador;
        }
    }

    public enum EstadoServico
    {
        Ausente,
        Parado,
        EmExecucao,
        Outro
    }

    public sealed class ResultadoServico
    {
        public string Nome { get; set; }
        public EstadoServico Estado { get; set; }
        public string Detalhe { get; set; }
    }

    public enum ModoJvmTomcat
    {
        Invalido,
        Automatica,
        Explicita
    }

    public sealed class ResultadoJvmTomcat
    {
        public ModoJvmTomcat Modo { get; set; }
        public string ValorJvm { get; set; }
        public string CaminhoRegistro { get; set; }
        public bool CaminhoExplicitoExiste { get; set; }
        public bool Saudavel { get; set; }
        public string Mensagem { get; set; }
    }

    public sealed class JreUsavel
    {
        public string Home { get; set; }
        public string JvmDll { get; set; }
        public string JavaExe { get; set; }
        public string VersaoSaida { get; set; }
    }

    public sealed class ResultadoSmoke
    {
        public string Url { get; set; }
        public bool Sucesso { get; set; }
        public int StatusCode { get; set; }
        public bool MarcadorEncontrado { get; set; }
        public string Erro { get; set; }
    }

    public sealed class Diagnostico
    {
        public List<InstalacaoEncontrada> Instalacoes { get; set; }
        public InstalacaoEncontrada InstalacaoSelecionada { get; set; }
        public List<string> DrivesVarridos { get; set; }
        public ResultadoServico Tomcat { get; set; }
        public ResultadoServico Apache { get; set; }
        public ResultadoServico Postgres { get; set; }
        public ResultadoJvmTomcat JvmTomcat { get; set; }
        public List<JreUsavel> JresUsaveis { get; set; }
        public ResultadoSmoke SmokeTomcat { get; set; }
        public ResultadoSmoke SmokeApache { get; set; }
        public List<string> ProblemasOperador { get; set; }
        public List<string> LinhasTecnicas { get; set; }
        public bool PodeReparar { get; set; }
        public bool TudoOk { get; set; }

        public Diagnostico()
        {
            Instalacoes = new List<InstalacaoEncontrada>();
            DrivesVarridos = new List<string>();
            JresUsaveis = new List<JreUsavel>();
            ProblemasOperador = new List<string>();
            LinhasTecnicas = new List<string>();
        }
    }

    public sealed class ResultadoReparo
    {
        public List<string> Acoes { get; set; }
        public List<string> Erros { get; set; }
        public Diagnostico PosReparo { get; set; }

        public ResultadoReparo()
        {
            Acoes = new List<string>();
            Erros = new List<string>();
        }
    }
}
