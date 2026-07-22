using System;
using System.IO;
using System.Net;
using System.Text;

namespace AssistenteDeReparo
{
    public static class SmokeHttp
    {
        public static ResultadoSmoke Verificar(string url)
        {
            var r = new ResultadoSmoke { Url = url };
            try
            {
                // TLS/ServicePoint defaults ok for localhost http
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 15000;
                request.ReadWriteTimeout = 15000;
                request.AllowAutoRedirect = true;
                request.UserAgent = "AssistenteDeReparo-Biblivre5/1.0";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream ?? Stream.Null, Encoding.UTF8))
                {
                    var body = reader.ReadToEnd();
                    r.StatusCode = (int)response.StatusCode;
                    r.MarcadorEncontrado = body != null
                        && body.IndexOf(InstalacaoPaths.MarcadorLogin, StringComparison.Ordinal) >= 0;
                    r.Sucesso = r.StatusCode >= 200 && r.StatusCode < 300 && r.MarcadorEncontrado;
                    if (!r.Sucesso && !r.MarcadorEncontrado)
                        r.Erro = "Resposta sem Marcador de Login";
                }
            }
            catch (WebException ex)
            {
                r.Sucesso = false;
                if (ex.Response is HttpWebResponse http)
                {
                    r.StatusCode = (int)http.StatusCode;
                    try
                    {
                        using (var stream = http.GetResponseStream())
                        using (var reader = new StreamReader(stream ?? Stream.Null, Encoding.UTF8))
                        {
                            var body = reader.ReadToEnd();
                            r.MarcadorEncontrado = body != null
                                && body.IndexOf(InstalacaoPaths.MarcadorLogin, StringComparison.Ordinal) >= 0;
                        }
                    }
                    catch { }
                }
                r.Erro = ex.Message;
            }
            catch (Exception ex)
            {
                r.Sucesso = false;
                r.Erro = ex.Message;
            }

            return r;
        }
    }
}
