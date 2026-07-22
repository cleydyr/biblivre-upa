using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssistenteDeReparo
{
    public sealed class MainForm : Form
    {
        private readonly Label _titulo;
        private readonly Label _status;
        private readonly TextBox _resultado;
        private readonly ComboBox _discos;
        private readonly Label _labelDisco;
        private readonly Button _btnVerificar;
        private readonly Button _btnCorrigir;
        private readonly Button _btnRelatorio;
        private readonly ProgressBar _progresso;

        private Diagnostico _ultimo;
        private bool _autoReparar;
        private string _driveInicial;

        public MainForm(bool autoReparar = false, string driveLetra = null)
        {
            _autoReparar = autoReparar;
            _driveInicial = driveLetra;

            Text = "Assistente de Reparo — Biblivre";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(520, 420);
            Size = new Size(560, 480);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            BackColor = Color.White;

            _titulo = new Label
            {
                Text = "Biblivre",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 20)
            };

            _status = new Label
            {
                Text = "Clique em Verificar para analisar este computador.",
                AutoSize = false,
                Location = new Point(24, 58),
                Size = new Size(500, 40)
            };

            _labelDisco = new Label
            {
                Text = "Qual instalação?",
                AutoSize = true,
                Location = new Point(24, 108),
                Visible = false
            };

            _discos = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(24, 132),
                Width = 280,
                Visible = false
            };
            _discos.SelectedIndexChanged += (s, e) => AtualizarEstadoBotoes();

            _resultado = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(24, 170),
                Size = new Size(500, 180),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            _progresso = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Location = new Point(24, 360),
                Size = new Size(500, 16),
                Visible = false
            };

            _btnVerificar = new Button
            {
                Text = "Verificar",
                Location = new Point(24, 390),
                Size = new Size(120, 36)
            };
            _btnVerificar.Click += async (s, e) => await VerificarAsync();

            _btnCorrigir = new Button
            {
                Text = "Tentar corrigir",
                Location = new Point(156, 390),
                Size = new Size(140, 36),
                Enabled = false
            };
            _btnCorrigir.Click += async (s, e) => await CorrigirAsync();

            _btnRelatorio = new Button
            {
                Text = "Salvar relatório",
                Location = new Point(308, 390),
                Size = new Size(140, 36),
                Enabled = false
            };
            _btnRelatorio.Click += (s, e) => SalvarRelatorio();

            Controls.Add(_titulo);
            Controls.Add(_status);
            Controls.Add(_labelDisco);
            Controls.Add(_discos);
            Controls.Add(_resultado);
            Controls.Add(_progresso);
            Controls.Add(_btnVerificar);
            Controls.Add(_btnCorrigir);
            Controls.Add(_btnRelatorio);

            Resize += (s, e) => AjustarLayout();
            AjustarLayout();

            Shown += async (s, e) =>
            {
                if (_autoReparar)
                {
                    await VerificarAsync();
                    if (_ultimo != null && _ultimo.InstalacaoSelecionada != null)
                        await ExecutarReparoElevadoAsync();
                }
            };
        }

        private void AjustarLayout()
        {
            var margem = 24;
            var largura = ClientSize.Width - margem * 2;
            _status.Width = largura;
            _resultado.Width = largura;
            _resultado.Height = Math.Max(120, ClientSize.Height - 280);
            _progresso.Top = _resultado.Bottom + 10;
            _progresso.Width = largura;
            var yBotao = _progresso.Bottom + 14;
            _btnVerificar.Top = yBotao;
            _btnCorrigir.Top = yBotao;
            _btnRelatorio.Top = yBotao;
        }

        private async Task VerificarAsync()
        {
            DefinirOcupado(true);
            _status.Text = "Procurando o Biblivre e verificando se está funcionando…";
            _resultado.Text = "";
            try
            {
                InstalacaoEncontrada selecionada = null;
                if (_discos.Visible && _discos.SelectedItem is InstalacaoEncontrada item)
                    selecionada = item;
                else if (!string.IsNullOrEmpty(_driveInicial))
                {
                    List<string> drives;
                    var todas = Descoberta.EncontrarInstalacoes(out drives);
                    selecionada = todas.FirstOrDefault(i =>
                        string.Equals(i.LetraDisco, _driveInicial, StringComparison.OrdinalIgnoreCase));
                }

                var diagnostico = await Task.Run(() => MotorDiagnostico.Executar(selecionada));
                _ultimo = diagnostico;
                ApresentarDiagnostico(diagnostico);
            }
            catch (Exception ex)
            {
                _status.Text = "Não foi possível concluir a verificação.";
                _resultado.Text = ex.Message;
            }
            finally
            {
                DefinirOcupado(false);
                AtualizarEstadoBotoes();
            }
        }

        private void ApresentarDiagnostico(Diagnostico d)
        {
            if (d.Instalacoes.Count > 1 && d.InstalacaoSelecionada == null)
            {
                _labelDisco.Visible = true;
                _discos.Visible = true;
                _discos.Items.Clear();
                foreach (var i in d.Instalacoes)
                    _discos.Items.Add(i);
                _discos.DisplayMember = "RotuloOperador";
                if (_discos.Items.Count > 0) _discos.SelectedIndex = 0;
                _status.Text = "Encontrei mais de uma instalação. Escolha o disco e clique em Verificar de novo.";
                _resultado.Text = MotorDiagnostico.TextoOperador(d);
                return;
            }

            if (d.Instalacoes.Count > 1 && d.InstalacaoSelecionada != null)
            {
                _labelDisco.Visible = true;
                _discos.Visible = true;
                if (_discos.Items.Count == 0)
                {
                    foreach (var i in d.Instalacoes)
                        _discos.Items.Add(i);
                    _discos.DisplayMember = "RotuloOperador";
                }
            }

            if (d.TudoOk)
            {
                _status.Text = "Tudo certo.";
                _resultado.Text = "O Biblivre parece estar funcionando neste computador.";
            }
            else if (d.Instalacoes.Count == 0)
            {
                _status.Text = "Biblivre não encontrado.";
                _resultado.Text = "Não encontrei o Biblivre neste computador.\r\n\r\n"
                    + "Se ele deveria estar instalado aqui, salve o relatório e envie ao suporte.";
            }
            else
            {
                _status.Text = "Encontrei problemas.";
                _resultado.Text = MotorDiagnostico.TextoOperador(d);
            }
        }

        private async Task CorrigirAsync()
        {
            if (_ultimo == null || _ultimo.InstalacaoSelecionada == null)
            {
                MessageBox.Show(this,
                    "Primeiro verifique e selecione a instalação do Biblivre.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var consent = MessageBox.Show(this,
                "Posso tentar corrigir?\n\nO acesso ao Biblivre pode ficar fora por alguns minutos.",
                "Confirmar correção",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (consent != DialogResult.Yes) return;

            if (!Elevacao.EstaElevado())
            {
                _status.Text = "O Windows pode pedir autorização de administrador…";
                if (!Elevacao.RelancarElevadoParaReparo(_ultimo.InstalacaoSelecionada.DriveRoot))
                {
                    MessageBox.Show(this,
                        "A correção precisa de autorização de administrador.\nSem isso, não posso alterar o sistema.",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _status.Text = "Uma nova janela foi aberta para corrigir. Você pode fechar esta.";
                return;
            }

            await ExecutarReparoElevadoAsync();
        }

        private async Task ExecutarReparoElevadoAsync()
        {
            if (_ultimo == null || _ultimo.InstalacaoSelecionada == null) return;

            DefinirOcupado(true);
            _status.Text = "Tentando corrigir… isso pode levar alguns minutos.";
            try
            {
                var instalacao = _ultimo.InstalacaoSelecionada;
                var resultado = await Task.Run(() => MotorReparo.Executar(instalacao));
                _ultimo = resultado.PosReparo ?? _ultimo;

                var msg = "Correção concluída.\r\n\r\n";
                if (resultado.Acoes.Count > 0)
                    msg += "Ações realizadas (detalhe técnico no relatório).\r\n";
                if (resultado.Erros.Count > 0)
                    msg += "Algumas ações não puderam ser feitas.\r\n";
                msg += "\r\n" + MotorDiagnostico.TextoOperador(_ultimo);

                _resultado.Text = msg;
                _status.Text = _ultimo != null && _ultimo.TudoOk
                    ? "Pronto — o Biblivre parece estar funcionando."
                    : "Terminei a tentativa. Se ainda falhar, salve o relatório para o suporte.";

                try
                {
                    var caminho = RelatorioTecnico.Salvar(_ultimo, resultado);
                    _resultado.AppendText("\r\n\r\nRelatório salvo em:\r\n" + caminho);
                }
                catch { }
            }
            catch (Exception ex)
            {
                _status.Text = "Não foi possível concluir a correção.";
                _resultado.Text = ex.Message;
            }
            finally
            {
                DefinirOcupado(false);
                AtualizarEstadoBotoes();
            }
        }

        private void SalvarRelatorio()
        {
            if (_ultimo == null)
            {
                MessageBox.Show(this, "Não há resultado para salvar. Clique em Verificar antes.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var caminho = RelatorioTecnico.Salvar(_ultimo);
                MessageBox.Show(this,
                    "Relatório salvo em:\n" + caminho + "\n\nEnvie este arquivo ao suporte, se precisar.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Não foi possível salvar o relatório.\n" + ex.Message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DefinirOcupado(bool ocupado)
        {
            UseWaitCursor = ocupado;
            _progresso.Visible = ocupado;
            _btnVerificar.Enabled = !ocupado;
            _btnCorrigir.Enabled = !ocupado && PodeCorrigir();
            _btnRelatorio.Enabled = !ocupado && _ultimo != null;
            _discos.Enabled = !ocupado;
        }

        private void AtualizarEstadoBotoes()
        {
            _btnCorrigir.Enabled = PodeCorrigir();
            _btnRelatorio.Enabled = _ultimo != null;
        }

        private bool PodeCorrigir()
        {
            if (_ultimo == null) return false;
            if (_ultimo.InstalacaoSelecionada == null) return false;
            if (_ultimo.TudoOk) return false;
            return _ultimo.PodeReparar;
        }
    }
}
