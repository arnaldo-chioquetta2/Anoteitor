using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace Anoteitor
{
    public partial class Main : Form
    {
        private bool SalvarAutom = false;
        private bool HojeVazio = false;
        private bool FonteComErro = false;
        private bool _Carregado = false;
        private bool Logar = false;
        private bool _LastMatchCase;
        private bool _LastSearchDown;
        private bool _IsDirty;
        private bool MedeTempos = false;
        private bool NovaTarefa = false;
        private bool _isSanitizingContentText = false;
        private bool _suppressEditorChangeTracking = false;
        private int DataSalva;
        private int QtdCarac = 0;
        private int Segundos = 2;
        private int QtMinutos = 0;
        private int QtMinutosEsse = 0;
        private long Tick = 0;
        private string TitAplicativo = "";
        private string _LastSearchText;
        private string _Filename;
        private string _NomeArq;
        private string _PastaGeral = "";
        private string Atual;
        private string AtualAnt;
        private string cbArquivosOld = "";
        private string NomeLog = "";
        private string _SUbAtual = "";
        private string cbArquivosSUbOld = "";
        private CancellationTokenSource _cts;
        private cEscolhido Escolhido = null;
        private INI cIni;
        private FindDialog _FindDialog;
        private ReplaceDialog _ReplaceDialog;
        private Encoding _encoding = Encoding.ASCII;
        private PageSettings _PageSettings;
        private string _CurrentFileDate = "";
        private string _lastKnownEditorText = "";
        private string _arquivoOrigemConteudoHistorico = "";
        private readonly Stack<EditorSnapshot> _undoSnapshots = new Stack<EditorSnapshot>();
        private readonly List<NavigationEntry> _navigationHistory = new List<NavigationEntry>();
        private readonly List<ToolStripComboBox> _combosSubtarefas = new List<ToolStripComboBox>();
        private readonly List<string> _caminhoSubtarefas = new List<string>();
        private ComboBox _comboHierarquiaSelecionado;
        private int _nivelHierarquiaSelecionado = -1;
        private ContextMenuStrip _menuContextoHierarquia;
        private ComboBox _comboContextoHierarquia;
        private int _nivelContextoHierarquia = -1;
        private ToolStripMenuItem _itemContextoRenomear;
        private ToolStripMenuItem _itemContextoApagar;
        private ToolStripMenuItem _itemContextoMover;
        private ToolStripMenuItem _itemContextoNova;
        private ToolStripMenuItem _itemContextoCriarSubTarefa;
        private ToolStripSeparator _separadorContextoHierarquia;
        private int _navigationHistoryIndex = -1;
        private bool _isApplyingNavigationHistory = false;
        private bool _suppressNavigationHistory = false;
        private bool _atualizandoCombosSubtarefas = false;
        private const string EditorFontDefaultFamily = "Lucida Console";
        private const float EditorFontDefaultSize = 9.75f;
        private const float ComboFontDefaultSize = 8.25f;
        private const float ComboFontMinSize = 6f;
        private const float ComboFontMaxSize = 24f;
        private readonly Color _corComboNormal = SystemColors.Window;
        private readonly Color _corComboSelecionado = Color.LightYellow;

        public string PastaGeral
        {
            get
            {
                return _PastaGeral;
            }
            set
            {
                var oldvalue = value;
                _PastaGeral = value;
                OnFilenameChanged(oldvalue, value);
            }
        }

        private string SUbAtual
        {
            get
            {
                if (_SUbAtual == "GERAL")
                {
                    return "";
                }
                return _SUbAtual;
            }
            set
            {
                _SUbAtual = value;
            }
        }

        private Funcoes Fun = new Funcoes();

        private bool Carregado
        {
            get
            {
                this.Loga("Verificado valor de Carregado " + _Carregado.ToString());
                return _Carregado;
            }
            set
            {
                _Carregado = value;
                this.Loga("Carregado setado como " + _Carregado.ToString());
            }
        }

        private class ContentPosition
        {
            public int LineIndex;
            public int ColumnIndex;
        }

        private class EditorSnapshot
        {
            public string Text;
            public int SelectionStart;
            public int SelectionLength;
        }

        private class NavigationEntry
        {
            public string Projeto;
            public string Subprojeto;
            public string Data;
        }

        public Main()
        {
            InitializeComponent();
            try
            {
                string caminhoIcone = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "icon.ico");

                if (System.IO.File.Exists(caminhoIcone))
                    this.Icon = new System.Drawing.Icon(caminhoIcone);
            }
            catch
            {
                // O icone nao deve impedir a abertura do programa.
            }

            projetoToolStripMenuItem.Visible = false;
            _combosSubtarefas.Add(cbSubprojeto);
            cbSubprojeto.Overflow = ToolStripItemOverflow.AsNeeded;
            cbProjetos.Tag = -1;
            InicializarMenuContextoHierarquia();
            AssociarMenuContextoHierarquia(cbProjetos, -1);
            AssociarMenuContextoHierarquia(cbSubprojeto, 0);
            // this.Escolhido = new cEscolhido();
            Fun = new Funcoes();
            string etc = " ";
#if DEBUG
            etc = " Em Debug ";
            cIni = new INI(Fun.Caminho());
#else
            cIni = new INI();
#endif
            this.TitAplicativo = "Anoteitor" + etc + this.GetVersaoCurta();
            VeSeTemIni();
            this.Logar = cIni.ReadBool("Config", "Log", false);
            int X = cIni.ReadInt("Config", "X", 0);
            Rectangle ret;
            if (X < 1)
            {
                ret = new Rectangle(465, 185, 745, 500);
                StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                int Y = cIni.ReadInt("Config", "Y", 0);
                int W = cIni.ReadInt("Config", "W", 0);
                int H = cIni.ReadInt("Config", "H", 0);
                ret = new Rectangle(X, Y, W, H);
                StartPosition = FormStartPosition.Manual;
            }
            Bounds = ret;
        }

        private void VeSeTemIni()
        {
            if (!File.Exists(cIni.FileName))
            {
                this.PastaGeral = Application.StartupPath;
                cIni.WriteBool("Projetos", "SalvarAut", true);
                cIni.WriteString("Projetos", "Pasta", this.PastaGeral);
                cIni.WriteBool("Projetos", "CopiaOutroDia", true);
                cIni.WriteInt("Projetos", "Segundos", 2);
                cIni.WriteBool("Projetos", "MedeTempos", true);
                cIni.WriteInt("Projetos", "LimArqs", 30);
                cIni.WriteBool("Config", "Log", false);
                cIni.WriteString("Atualizacao", "ExecutavelPrincipal", Path.GetFileName(Application.ExecutablePath));
                cIni.WriteString("Atualizacao", "DiretorioInstalacao", Application.StartupPath);
                cIni.WriteString("Atualizacao", "CaminhoAtualizador", Path.Combine(Application.StartupPath, "Atualizador", "ATCAtualizeitor.exe"));
                cIni.WriteString("Atualizacao", "ServidorFTP", "");
                cIni.WriteString("Atualizacao", "UsuarioFTP", "");
                cIni.WriteString("Atualizacao", "SenhaFTP", "");
                cIni.WriteString("Atualizacao", "PastaFTP", "");
                cIni.WriteString("Atualizacao", "VersaoAtual", this.GetVersaoCurta());
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (this.Logar)
                this.PreparaLog();
            this.Loga("INICIO");
            UpdateTitle();
            menuitemFormatWordWrap.Checked = controlContentTextBox.WordWrap;
            try
            {
                Font fonteEditor = CurrentFont;
                this.controlContentTextBox.Font = fonteEditor;
            }
            catch (Exception Ex)
            {
                this.Loga("Erro ao carregar a fonte " + Ex.Message.ToString());
                this.FonteComErro = true;
            }
            UpdateStatusBar();
            controlContentTextBox.BringToFront(); // in order to docking to respond correctly to the status bar being turned off and on
            this.PastaGeral = cIni.ReadString("Projetos", "Pasta", "");
            this.Atual = cIni.ReadString("Projetos", "Atual", "");
            PrepararConfiguracaoDeAtualizacao();
            this.PreencheCombo(this.Atual);
            if (this.Atual.Length > 0)
            {
                this.CarregaArquivoDoProjeto(false);
                this.MostraArquivosDoProjeto();
                renomearToolStripMenuItem1.Enabled = true;
                toolStripMenuItem1.Enabled = true;
            }
            else
            {
                subAtividadesToolStripMenuItem.Enabled = false;
            }
            this.SalvarAutom = cIni.ReadBool("Projetos", "SalvarAut", false);
            this.cbProjetos.Text = this.Atual;
            this.Segundos = cIni.ReadInt("Projetos", "Segundos", 2);
            this.DataSalva = Fun.Agora().DayOfYear;
            this.MedeTempos = cIni.ReadBool("Projetos", "MedeTempos", true);
            this.timer2.Enabled = this.MedeTempos;
            this.temposToolStripMenuItem.Visible = this.MedeTempos;
            AplicarFonteDosCombos(CurrentComboFont);
            RegistrarNavegacaoAtual();  
            AtualizarBotoesHistorico();
            // this.Carregado = true; 
        }

        private void PreparaLog()
        {
            string pasta = Path.Combine(Application.StartupPath, "Log");

            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            // Nome fixo, sempre o mesmo
            NomeLog = Path.Combine(pasta, "Anoteitor.log");

            // Limpa o log anterior para manter apenas a execução corrente
            File.WriteAllText(NomeLog, string.Empty);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("timer1_Tick");
            this.timer1.Enabled = false;

            // ✅ SALVAR SEMPRE — lógica de snapshots fica no Save()
            this.Save();
        }

        private void Loga(string texto)
        {
#if DEBUG
            Console.WriteLine(texto);
#endif            
            if (this.Logar)
                File.AppendAllText(this.NomeLog, Fun.Agora().ToString() + " " + texto + Environment.NewLine);
        }

        #region Menus

        private void menuitemFormatWordWrap_Click(object sender, EventArgs e)
        {
            WordWrap = !WordWrap;
        }

        private void menuitemFormatWordWrap_CheckedChanged(object sender, EventArgs e)
        {
            var Sender = (ToolStripMenuItem)sender;
            WordWrap = Sender.Checked;
        }

        private void menuitemFileSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void menuitemFileSaveAs_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void menuitemFileNew_Click(object sender, EventArgs e)
        {
            New();
        }

        private void menuitemFilePageSetup_Click(object sender, EventArgs e)
        {
            var PageSetupDialog = new PageSetupDialog();
            PageSetupDialog.PageSettings = PageSettings;
            if (PageSetupDialog.ShowDialog(this) != DialogResult.OK) return;
            PageSettings = PageSetupDialog.PageSettings;
        }

        private void menuitemFilePrint_Click(object IGNORE_sender, EventArgs IGNORE_e)
        {
            var PrintDialog = new PrintDialog();

            if (Settings.MoreSettings.PrinterSettings != null)
            {
                PrintDialog.PrinterSettings = Settings.MoreSettings.PrinterSettings;
            }

            if (PrintDialog.ShowDialog(this) != DialogResult.OK) return;
            Settings.MoreSettings.PrinterSettings = PrintDialog.PrinterSettings;
            Settings.Save();
            var PrintDocument = new PrintDocument();
            PrintDocument.DefaultPageSettings = PageSettings;
            PrintDocument.PrinterSettings = Settings.MoreSettings.PrinterSettings;
            PrintDocument.DocumentName = DocumentName + " - " + this.TitAplicativo;
            var RemainingContentToPrint = Content;
            var PageIndex = 0;
            PrintDocument.PrintPage += (sender, e) => {
                { // header
                    var HeaderText = FormatHeaderFooterText(Settings.Header, PageIndex);
                    var Top = PageSettings.Margins.Top;
                    DrawStringAtPosition(e.Graphics, HeaderText.Left, Top, DrawStringPosition.Left);
                    DrawStringAtPosition(e.Graphics, HeaderText.Center, Top, DrawStringPosition.Center);
                    DrawStringAtPosition(e.Graphics, HeaderText.Right, Top, DrawStringPosition.Right);
                }

                { // body
                    var CharactersFitted = 0;
                    var LinesFilled = 0;
                    var MarginBounds = new RectangleF(e.MarginBounds.X, e.MarginBounds.Y + /* header */ CurrentFont.Height, e.MarginBounds.Width, e.MarginBounds.Height - (/* header and footer */ CurrentFont.Height * 2));
                    e.Graphics.MeasureString(RemainingContentToPrint, CurrentFont, MarginBounds.Size, StringFormat.GenericTypographic, out CharactersFitted, out LinesFilled);
                    e.Graphics.DrawString(RemainingContentToPrint, CurrentFont, Brushes.Black, MarginBounds, StringFormat.GenericTypographic);
                    RemainingContentToPrint = RemainingContentToPrint.Substring(CharactersFitted);
                    e.HasMorePages = (RemainingContentToPrint.Length > 0);
                }

                { // footer
                    var FooterText = FormatHeaderFooterText(Settings.Footer, PageIndex);
                    var Top = PageSettings.Bounds.Bottom - PageSettings.Margins.Bottom - CurrentFont.Height;
                    DrawStringAtPosition(e.Graphics, FooterText.Left, Top, DrawStringPosition.Left);
                    DrawStringAtPosition(e.Graphics, FooterText.Center, Top, DrawStringPosition.Center);
                    DrawStringAtPosition(e.Graphics, FooterText.Right, Top, DrawStringPosition.Right);
                }

                PageIndex++;
            };

            PrintDocument.Print();
        }

        private void menuitemFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuitemExecutarAtualizacao_Click(object sender, EventArgs e)
        {
            ExecutarAtualizacaoSemiAutomatica();
        }

        private void ExecutarAtualizacaoSemiAutomatica()
        {
            try
            {
                PrepararConfiguracaoDeAtualizacao();

                string caminhoAtualizador = cIni.ReadString(
                    "Atualizacao",
                    "CaminhoAtualizador",
                    Path.Combine(Application.StartupPath, "Atualizador", "ATCAtualizeitor.exe"));

                if (!File.Exists(caminhoAtualizador))
                {
                    MessageBox.Show(
                        this,
                        "O atualizador não foi encontrado.\n\n" + caminhoAtualizador,
                        this.TitAplicativo,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                DialogResult confirmar = MessageBox.Show(
                    this,
                    "O Anoteitor será salvo, passará o controle para o atualizador e será encerrado.\n\nDeseja continuar?",
                    "Atualização",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirmar != DialogResult.Yes)
                    return;

                if (this.IsDirty && !this.Save())
                    return;

                this.timer1.Enabled = false;
                this.timer2.Enabled = false;

                Process.Start(new ProcessStartInfo
                {
                    FileName = caminhoAtualizador,
                    WorkingDirectory = Path.GetDirectoryName(caminhoAtualizador)
                });

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                this.Loga("Erro ao iniciar atualização: " + ex.Message);
                MessageBox.Show(
                    this,
                    "Não foi possível iniciar a atualização.\n\n" + ex.Message,
                    "Atualização",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void PrepararConfiguracaoDeAtualizacao()
        {
            string caminhoAtualizadorPadrao = Path.Combine(Application.StartupPath, "Atualizador", "ATCAtualizeitor.exe");

            string executavelPrincipal = Path.GetFileName(Application.ExecutablePath);
            string diretorioInstalacao = Application.StartupPath;
            string caminhoAtualizador = cIni.ReadString("Atualizacao", "CaminhoAtualizador", caminhoAtualizadorPadrao);

            if (string.IsNullOrWhiteSpace(caminhoAtualizador))
                caminhoAtualizador = caminhoAtualizadorPadrao;

            cIni.WriteString("Atualizacao", "ExecutavelPrincipal", executavelPrincipal);
            cIni.WriteString("Atualizacao", "DiretorioInstalacao", diretorioInstalacao);
            cIni.WriteString("Atualizacao", "CaminhoAtualizador", caminhoAtualizador);
            cIni.WriteString("Atualizacao", "VersaoAtual", this.GetVersaoCurta());

            GarantirChaveIni("Atualizacao", "ServidorFTP", "");
            GarantirChaveIni("Atualizacao", "UsuarioFTP", "");
            GarantirChaveIni("Atualizacao", "SenhaFTP", "");
            GarantirChaveIni("Atualizacao", "PastaFTP", "");
        }

        private void GarantirChaveIni(string secao, string chave, string valorPadrao)
        {
            string valorAtual = cIni.ReadString(secao, chave, "");
            if (string.IsNullOrWhiteSpace(valorAtual))
                cIni.WriteString(secao, chave, valorPadrao);
        }

        private void menuitemEditUndo_Click(object sender, EventArgs e)
        {
            DesfazerAlteracaoDoEditor();
        }

        private void menuitemEditCut_Click(object sender, EventArgs e)
        {
            controlContentTextBox.Cut();
        }

        private void menuitemEditCopy_Click(object sender, EventArgs e)
        {
            controlContentTextBox.Copy();
        }

        private void menuitemEditPaste_Click(object sender, EventArgs e)
        {
            PasteNormalizedClipboardText();
        }

        private void menuitemEditDelete_Click(object sender, EventArgs e)
        {
            DeleteEditorSelectionOrNextCharacter();
        }

        private void menuitemEditSelectAll_Click(object sender, EventArgs e)
        {
            if (_Carregado)
            {
                controlContentTextBox.SelectAll();
            }
        }

        private void menuitemEditTimeDate_Click(object sender, EventArgs e)
        {
            SelectedText = Fun.Agora().ToShortTimeString() + " " + Fun.Agora().ToShortDateString();
        }

        private void menuitemEditGoTo_Click(object sender, EventArgs e)
        {
            var GoToLinePrompt = new GoToLinePrompt(LineIndex + 1);
            GoToLinePrompt.Left = Left + 5;
            GoToLinePrompt.Top = Top + 44;

            if (GoToLinePrompt.ShowDialog(this) != DialogResult.OK) return;

            var TargetLineIndex = GoToLinePrompt.LineNumber - 1;

            if (TargetLineIndex > controlContentTextBox.Lines.Length)
            {
                MessageBox.Show(this, "The line number is beyond the total number of lines", "Anoteitor - Goto Line");
                return;
            }

            LineIndex = TargetLineIndex;
        }

        private void menuitemAbout_Click(object sender, EventArgs e)
        {
            new About().ShowDialog(this);
        }

        private void menuitemEdit_DropDownOpening(object sender, EventArgs e)
        {
            menuitemEditUndo.Enabled = _undoSnapshots.Count > 0;

            menuitemEditCut.Enabled =
                menuitemEditCopy.Enabled =
                menuitemEditDelete.Enabled = (SelectionLength > 0);

            menuitemEditFind.Enabled =
                menuitemEditFindNext.Enabled = (Content.Length > 0);
        }

        private void menuitemEditFind_Click(object sender, EventArgs e)
        {
            Find();
        }

        private void menuitemEditFindNext_Click(object sender, EventArgs e)
        {
            if (_LastSearchText == null)
            {
                Find();
                return;
            }

            if (!FindAndSelect(_LastSearchText, _LastMatchCase, _LastSearchDown))
            {
                MessageBox.Show(this, CONST.CannotFindMessage.FormatUsingObject(new { SearchText = _LastSearchText }), this.TitAplicativo);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !EnsureWorkNotLost();
            if (e.Cancel == false)
            {
                this.Loga("FECHADO NORMALMENTE");
                this.Loga("");
            }
        }

        private void menuitemEditReplace_Click(object sender, EventArgs e)
        {
            if (Content.Length == 0) return;
            if (_ReplaceDialog == null)
                _ReplaceDialog = new ReplaceDialog(this);
            _ReplaceDialog.SelText = controlContentTextBox.SelectedText;
            _ReplaceDialog.Left = this.Left + 56;
            _ReplaceDialog.Top = this.Top + 113;
            if (!_ReplaceDialog.Visible)
                _ReplaceDialog.Show(this);
            else
                _ReplaceDialog.Show();
            _ReplaceDialog.Triggered();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            cIni.WriteInt("Config", "X", Bounds.X);
            cIni.WriteInt("Config", "Y", Bounds.Y);
            cIni.WriteInt("Config", "W", Bounds.Width);
            cIni.WriteInt("Config", "H", Bounds.Height);
            cIni.WriteInt(Atual, "Tempo", QtMinutos);
        }

        private void menuitemFileHeaderAndFooter_Click(object sender, EventArgs e)
        {
            var PageSetupHeaderFooter = new PageSetupHeaderFooter();
            PageSetupHeaderFooter.Header = Settings.Header;
            PageSetupHeaderFooter.Footer = Settings.Footer;
            if (PageSetupHeaderFooter.ShowDialog(this) != DialogResult.OK) return;
            Settings.Header = PageSetupHeaderFooter.Header;
            Settings.Footer = PageSetupHeaderFooter.Footer;
            Settings.Save();
        }

        private void menuitemFileOpen_Click(object sender, EventArgs e)
        {
            if (!EnsureWorkNotLost()) return;

            var OpenDialog = new SaveOpenDialog();
            OpenDialog.FileDlgDefaultExt = ".txt";
            OpenDialog.FileDlgFileName = Filename;
            OpenDialog.FileDlgFilter = "Documento de texto (*.txt)|*.txt|Todos Arquivos (*.*)|*.*";
            OpenDialog.FileDlgType = Win32Types.FileDialogType.OpenFileDlg;
            OpenDialog.FileDlgCaption = "Abrir";
            OpenDialog.FileDlgOkCaption = "Abrir";

            if (OpenDialog.ShowDialog(this) != DialogResult.OK) return;

            Open(OpenDialog.MSDialog.FileName, encoding: OpenDialog.Encoding);
        }

        private void menuitemFormatFont_Click(object sender, EventArgs e)
        {
            var FontDialog = new FontDialog();
            FontDialog.Font = CurrentFont;
            if (FontDialog.ShowDialog(this) != DialogResult.OK) return;
            CurrentFont = FontDialog.Font;
        }

        private void menuitemFormatCombosMaior_Click(object sender, EventArgs e)
        {
            AlterarTamanhoFonteCombos(1f);
        }

        private void menuitemFormatCombosMenor_Click(object sender, EventArgs e)
        {
            AlterarTamanhoFonteCombos(-1f);
        }

        private void novaSubAtividadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripComboBox comboPai = _combosSubtarefas.Count > 0
                ? _combosSubtarefas[_combosSubtarefas.Count - 1]
                : cbSubprojeto;
            CriarSubtarefaAbaixoDoCombo(comboPai);
        }

        private void apagarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_caminhoSubtarefas.Count == 0)
                return;

            List<string> caminhoAntigo = new List<string>(_caminhoSubtarefas);
            string nome = caminhoAntigo[caminhoAntigo.Count - 1];
            DialogResult resposta = MessageBox.Show(
                this,
                "Tem certeza que deseja excluir a sub-tarefa '" + nome + "' e todo o conteúdo abaixo dela?",
                this.TitAplicativo,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (resposta != DialogResult.Yes)
                return;

            string pasta = PastaDaSubtarefa(caminhoAntigo);
            Directory.Delete(pasta, true);

            List<string> caminhoPai = caminhoAntigo.Take(caminhoAntigo.Count - 1).ToList();
            _caminhoSubtarefas.Clear();
            _caminhoSubtarefas.AddRange(caminhoPai);
            cIni.WriteString(this.Atual, "CaminhoAtual", CaminhoSubtarefasAtual());
            AtualizarEstadoCaminhoSubtarefas();
            CarregarHierarquiaSubtarefas();
            AbrirAnotacaoDaHierarquiaAtual(true);
            AtualizarMenuSubAtividades();
        }

        #endregion

        #region  Manipulação de Arquivos

        public string Filename
        {
            get
            {
                return _Filename;
            }
            set
            {
                var oldvalue = value;
                _Filename = value;
                OnFilenameChanged(oldvalue, value);
            }
        }

        private void OnFilenameChanged(string oldvalue, string value)
        {
            OnDocumentNameChanged();
        }

        private void OnDocumentNameChanged()
        {
            UpdateTitle();
        }

        #region Salvamento

        private bool Save()
        {
            EnsureEditorContentSanitized();

            if (!IsDirty) return true;

            int Tam = Content.Length;
            if (Tam < 1)
            {
                this.Loga("Ia salvar vazio");
                return true;
            }

            toolStripStatusLabel1.Text = "Salvando arquivo";

            // Working copy canônico: sempre sem data e nunca ^current.txt.
            string workingCopy = NomeDoArquivo("current");
            string directory = Path.GetDirectoryName(workingCopy);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            try
            {
                string today = Fun.Agora().ToString("dd-MM-yyyy");

                if (File.Exists(workingCopy))
                {
                    DateTime lastWrite = File.GetLastWriteTime(workingCopy);
                    string lastWriteDate = lastWrite.ToString("dd-MM-yyyy");

                    // ✅ VIRADA DE DIA → criar snapshot com data do arquivo antigo
                    if (lastWriteDate != today)
                    {
                        // 🔥 FORÇAR nome com data (ESSENCIAL)
                        string historicPath = NomeDoArquivo(lastWriteDate, true);

                        CriarSnapshotSeNecessario(workingCopy, historicPath, "virada de dia");
                    }
                }
                else
                {
                    CriarSnapshotInicialSePossivel();
                }

                // ✅ Salvar working copy (estado atual)
                File.WriteAllText(workingCopy, SanitizeControlCharacters(Content), _encoding ?? Encoding.UTF8);

                IsDirty = false;
                this.Filename = workingCopy;
                this._arquivoOrigemConteudoHistorico = "";

                this.Loga($"Working copy salvo: {workingCopy} ({Tam} bytes)");

                string HoraSalva = Fun.Agora().ToString(@"HH\:mm\:ss");
                toolStripStatusLabel1.Text = "Gravado às : " + HoraSalva;

                this.AjustaCorFundo();
            }
            catch (Exception ex)
            {
                this.Loga($"❌ Erro ao salvar: {ex.Message}");
                MessageBox.Show($"Erro ao salvar:\n{workingCopy}\n\n{ex.Message}",
                               this.TitAplicativo, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #endregion

        private void CriarSnapshotSeNecessario(string origem, string destino, string motivo)
        {
            if (string.IsNullOrWhiteSpace(origem) || !File.Exists(origem))
                return;

            if (new FileInfo(origem).Length <= 3)
            {
                this.Loga($"ℹ️ Snapshot ignorado ({motivo}): origem vazia/só BOM");
                return;
            }

            string pastaDestino = Path.GetDirectoryName(destino);
            if (!Directory.Exists(pastaDestino))
                Directory.CreateDirectory(pastaDestino);

            if (!File.Exists(destino))
            {
                File.Copy(origem, destino);
                this.Loga($"✅ Snapshot criado ({motivo}): {destino}");
                return;
            }

            string origemConteudo = SanitizeControlCharacters(File.ReadAllText(origem, _encoding ?? Encoding.UTF8));
            string destinoConteudo = SanitizeControlCharacters(File.ReadAllText(destino, _encoding ?? Encoding.UTF8));
            if (string.Equals(origemConteudo, destinoConteudo, StringComparison.Ordinal))
            {
                this.Loga($"ℹ️ Snapshot já existe para {Path.GetFileName(destino)}");
                return;
            }

            string pasta = Path.GetDirectoryName(destino);
            string nome = Path.GetFileNameWithoutExtension(destino);
            string extensao = Path.GetExtension(destino);
            int revisao = 2;
            string destinoRevisao;

            do
            {
                destinoRevisao = Path.Combine(pasta, $"{nome}~{revisao}{extensao}");
                revisao++;
            }
            while (File.Exists(destinoRevisao));

            File.Copy(origem, destinoRevisao);
            this.Loga($"✅ Snapshot revisado criado ({motivo}): {destinoRevisao}");
        }

        private void CriarSnapshotInicialSePossivel()
        {
            string origem = "";

            if (!string.IsNullOrWhiteSpace(this._arquivoOrigemConteudoHistorico) && File.Exists(this._arquivoOrigemConteudoHistorico))
                origem = this._arquivoOrigemConteudoHistorico;
            else if (!string.IsNullOrWhiteSpace(this.Filename) && File.Exists(this.Filename) && ArquivoEhSnapshotHistorico(this.Filename))
                origem = this.Filename;

            if (string.IsNullOrWhiteSpace(origem))
            {
                this.Loga("ℹ️ Snapshot inicial não criado: nenhuma origem histórica disponível");
                return;
            }

            DateTime data = GetDataPeloNome(Path.GetFileName(origem));
            if (data == DateTime.MinValue)
                data = File.GetLastWriteTime(origem);

            string destino = NomeDoArquivo(data.ToString("dd-MM-yyyy"), true);
            CriarSnapshotSeNecessario(origem, destino, "primeiro salvamento do working copy");
        }

        private bool SaveAs()
        {
            var SaveDialog = new SaveOpenDialog();
            SaveDialog.FileDlgFileName = Filename;
            SaveDialog.FileDlgDefaultExt = ".txt";
            SaveDialog.FileDlgFilter = "Documento de texto (*.txt)|*.txt|Todos Arquivos (*.*)|*.*";
            SaveDialog.Encoding = _encoding;
            SaveDialog.FileDlgCaption = "Salvar";
            SaveDialog.FileDlgOkCaption = "Salvar";

            if (SaveDialog.ShowDialog(this) != DialogResult.OK) return false;

            var PotentialFilename = SaveDialog.MSDialog.FileName;

            _encoding = SaveDialog.Encoding;
            EnsureEditorContentSanitized();
            File.WriteAllText(PotentialFilename, SanitizeControlCharacters(Content), _encoding);

            Filename = PotentialFilename;
            IsDirty = false;

            return true;
        }

        // ✅ MÉTODO 1: Orquestrador principal (28 linhas)
        public void Open(string pFilename, string searchText = null, Encoding encoding = null, bool ativar = false)
        {
            this.Loga($"[v2.13] Open: {pFilename}");
            Console.WriteLine($"Abrindo {pFilename}");
            this._arquivoOrigemConteudoHistorico = "";

            // ✅ Passo 1: Determinar arquivo correto (working copy ou migração)
            string arquivoFinal = DeterminarArquivoCorreto(pFilename);

            // ✅ Passo 2: Criar arquivo se não existir
            if (!File.Exists(arquivoFinal))
                arquivoFinal = CriarArquivoNovo(arquivoFinal);

            // ✅ Passo 3: Ler conteúdo do arquivo
            LerConteudoArquivo(arquivoFinal, encoding);

            // ✅ Passo 4: Tratar arquivo vazio (carregar histórico se necessário)
            if (Content.Length == 0)
                TratarArquivoVazio(arquivoFinal);

            // ✅ Passo 5: Aplicar busca de texto (se fornecida)
            if (!string.IsNullOrEmpty(searchText))
                AplicarBuscaTexto(searchText, ativar);
            else
                SelectionStart = 0;

            // ✅ Passo 6: Finalizar abertura
            FinalizarAbertura(arquivoFinal);

            this.Loga($"Open finalizado - Filename={this.Filename}, Carregado={this.Carregado}, Tamanho={Content.Length}");
        }

        // ✅ MÉTODO 7: Finalizar abertura do arquivo (15 linhas)
        private void FinalizarAbertura(string arquivoFinal)
        {
            this.Filename = !string.IsNullOrWhiteSpace(this._arquivoOrigemConteudoHistorico)
                ? this._arquivoOrigemConteudoHistorico
                : arquivoFinal;
            IsDirty = false;
            toolStripStatusLabel1.Text = "";
            this.AjustaCorFundo();
            this.QtMinutosEsse = 0;
            this.QtMinutos = cIni.ReadInt(Atual, "Tempo", 0);
            this.MotraCaracteres();
            ResetUndoDaAtividadeAtual();
            this.Carregado = true; // ✅ FORÇAR Carregado = true após qualquer abertura
        }

        private void ResetUndoDaAtividadeAtual()
        {
            _undoSnapshots.Clear();
            _lastKnownEditorText = controlContentTextBox.Text;
        }

        // ✅ MÉTODO 6: Aplicar busca de texto (12 linhas)
        private void AplicarBuscaTexto(string searchText, bool ativar)
        {
            int index = Content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                controlContentTextBox.SelectionStart = index;
                controlContentTextBox.SelectionLength = searchText.Length;
                if (ativar) controlContentTextBox.Focus();
                controlContentTextBox.ScrollToCaret();
            }
        }
        // ✅ MÉTODO 5: Tratar arquivo vazio com busca histórica (42 linhas)
        private void TratarArquivoVazio(string arquivoAtual)
        {
            this.Loga("⚠️ Arquivo vazio - buscando conteúdo não vazio no histórico...");

            try
            {
                string pasta = Path.GetDirectoryName(arquivoAtual);
                DirectoryInfo dir = new DirectoryInfo(pasta);

                if (!dir.Exists) return;

                // Buscar arquivos históricos com data válida
                FileInfo[] arquivosHistoricos = dir.GetFiles("*^*.txt")
                    .Where(f => Regex.IsMatch(f.Name, @"\d{2}-\d{2}-\d{4}\.txt$") && f.Length > 3)
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToArray();

                this.Loga($"Encontrados {arquivosHistoricos.Length} arquivos históricos com conteúdo");

                // Encontrar primeiro arquivo com conteúdo real
                foreach (FileInfo arquivo in arquivosHistoricos)
                {
                    if (arquivo.FullName == arquivoAtual) continue;

                        string conteudo = SanitizeControlCharacters(File.ReadAllText(arquivo.FullName, Encoding.UTF8));
                    if (conteudo.Length > 3) // Conteúdo real além do BOM
                    {
                        this._arquivoOrigemConteudoHistorico = arquivo.FullName;
                        Content = conteudo;
                        controlContentTextBox.BackColor = Color.LightBlue; // Azul = baseado em histórico
                        IsDirty = true;

                        this.Loga($"✅ Conteúdo carregado de: {arquivo.Name} ({Content.Length} bytes)");

                        IsDirty = false;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Loga($"❌ Erro ao buscar conteúdo histórico: {ex.Message}");
            }
        }

        // ✅ MÉTODO 4: Ler conteúdo do arquivo com encoding (22 linhas)
        private void LerConteudoArquivo(string filename, Encoding encoding)
        {
            // Detectar encoding se não fornecido
            if (encoding == null)
            {
                using (var sr = new StreamReader(filename, detectEncodingFromByteOrderMarks: true))
                {
                    sr.ReadToEnd();
                    _encoding = sr.CurrentEncoding;
                    this.Loga($"Encoding detectado: {_encoding.EncodingName}");
                }
            }

            // Ler conteúdo
            string content = ReadAllText(filename, encoding);
            Content = content;

            this.Loga($"Conteúdo lido: {(Content.Length > 0 ? Content.Length + " caracteres" : "VAZIO")}");
        }
        // ✅ MÉTODO 3: Criar arquivo novo se não existir (18 linhas)
        private string CriarArquivoNovo(string filename)
        {
            Console.WriteLine("Arquivo não existe - criando novo");

            if (string.IsNullOrEmpty(Path.GetExtension(filename)))
                filename += ".txt";

            string directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filename, "", Encoding.UTF8);
            this.Loga($"🆕 Arquivo criado: {filename}");

            return filename;
        }

        // ✅ MÉTODO 2: Determinar arquivo correto com migração (38 linhas)
        private string DeterminarArquivoCorreto(string pFilename)
        {
            string pasta = Path.GetDirectoryName(pFilename);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(pFilename);
            string[] nameParts = fileNameWithoutExt.Split('^');

            // Construir nome do working copy CORRETO (sem data)
            string baseName = (nameParts.Length > 1 && Regex.IsMatch(nameParts[nameParts.Length - 1], @"\d{2}-\d{2}-\d{4}"))
                ? string.Join("^", nameParts.Take(nameParts.Length - 1))
                : fileNameWithoutExt;

            string workingCopyCorreto = Path.Combine(pasta, baseName + ".txt");
            string workingCopyAntigo = Path.Combine(pasta, fileNameWithoutExt + "^current.txt");

            // ✅ Prioridade 1: Working copy CORRETO (Projeto^Sub.txt)
            if (File.Exists(workingCopyCorreto) && new FileInfo(workingCopyCorreto).Length > 3)
            {
                this.Loga($"✅ Working copy encontrado (formato correto): {workingCopyCorreto}");
                return workingCopyCorreto;
            }

            // ✅ Prioridade 2: Working copy ANTIGO (^current.txt) - migração
            if (File.Exists(workingCopyAntigo) && new FileInfo(workingCopyAntigo).Length > 3)
            {
                this.Loga($"⚠️ Working copy antigo encontrado (^current.txt): {workingCopyAntigo}");
                this.Loga($"➡️ Migrando para formato correto: {workingCopyCorreto}");

                // Migrar conteúdo
                string conteudo = File.ReadAllText(workingCopyAntigo, Encoding.UTF8);
                File.WriteAllText(workingCopyCorreto, conteudo, Encoding.UTF8);
                File.Delete(workingCopyAntigo);

                this.Loga($"✅ Migração concluída para: {workingCopyCorreto}");
                return workingCopyCorreto;
            }

            // ✅ Prioridade 3: Usar arquivo original (para histórico)
            return pFilename;
        }

        private void AjustaCorFundo()
        {
            try
            {
                if (this.IsDirty)
                {
                    controlContentTextBox.BackColor = SystemColors.Window; // Branco = alteracao em andamento
                    return;
                }

                if (!File.Exists(this.Filename))
                {
                    controlContentTextBox.BackColor = SystemColors.Window; // Branco
                    return;
                }

                if (ArquivoEhSnapshotHistorico(this.Filename))
                {
                    controlContentTextBox.BackColor = Color.AliceBlue; // Azul = visualizacao historica
                    return;
                }

                controlContentTextBox.BackColor = ArquivoFoiModificadoHoje(this.Filename)
                    ? SystemColors.Window // Branco = houve alteração hoje
                    : Color.AliceBlue;    // Azul = ainda não houve alteração hoje
            }
            catch
            {
                controlContentTextBox.BackColor = SystemColors.Window; // Fallback branco
            }
        }

        private bool ArquivoEhSnapshotHistorico(string path)
        {
            string nome = Path.GetFileNameWithoutExtension(path);
            return Regex.IsMatch(nome, @"\^\d{2}-\d{2}-\d{4}$");
        }

        private bool ArquivoFoiModificadoHoje(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return false;

            DateTime hoje = Fun.Agora().Date;
            DateTime ultimaAlteracao = File.GetLastWriteTime(path).Date;
            return ultimaAlteracao == hoje;
        }

        private DateTime ExtrairDataSnapshot(string nomeArquivo)
        {
            Match match = Regex.Match(nomeArquivo, @"(\d{2}-\d{2}-\d{4})\.txt$");
            if (match.Success &&
                DateTime.TryParseExact(
                    match.Groups[1].Value,
                    "dd-MM-yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime data))
            {
                return data;
            }

            return DateTime.MinValue;
        }

        private string ReadAllText(string path, Encoding encoding = null)
        {
            this.Loga("[v2.3] ReadAllText: " + path);

            if (!File.Exists(path))
            {
                this.Loga("    Arquivo não existe");
                return "";
            }

            byte[] bom = new byte[3];
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = fs.Read(bom, 0, 3);
                fs.Seek(0, SeekOrigin.Begin);

                // Detectar BOM
                if (bytesRead >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                {
                    encoding = Encoding.UTF8;
                    this.Loga("    BOM UTF-8 detectado");
                }
                else if (bytesRead >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
                {
                    encoding = Encoding.Unicode;
                    this.Loga("    BOM UTF-16 LE detectado");
                }
                else if (bytesRead >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
                {
                    encoding = Encoding.BigEndianUnicode;
                    this.Loga("    BOM UTF-16 BE detectado");
                }
                else
                {
                    encoding = Encoding.Default; // ANSI/ASCII
                    this.Loga("    Sem BOM - usando encoding padrão");
                }
            }

            string content = SanitizeControlCharacters(File.ReadAllText(path, encoding));
            this.Loga("    Conteúdo lido: " + (content.Length > 0 ? content.Length + " caracteres" : "VAZIO"));
            return content;
        }

        private void UpdateTitle()
        {
            if (this.Tag == null)
            {
                this.Tag = base.Text;
            }
            string versao = GetVersaoCurta();
            base.Text = ((string)this.Tag).FormatUsingObject(new
            {
                DocumentName,
                Versao = versao
            });
        }

        private string GetVersaoCurta()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }

        public string DocumentName
        {
            get
            {
                if (Filename == null) return "Sem título";
                return Path.GetFileName(Filename);
            }
        }

        private bool New()
        {
            if (!EnsureWorkNotLost()) return false;

            Filename = null;
            Content = "";
            IsDirty = false;
            _encoding = Encoding.ASCII;

            return true;
        }

        #endregion

        #region Edição

        public string Content
        {
            get { return controlContentTextBox.Text; }
            set
            {
                DefinirTextoEditorProgramaticamente(SanitizeControlCharacters(value));
            }
        }

        private void controlContentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_suppressEditorChangeTracking)
            {
                _lastKnownEditorText = controlContentTextBox.Text;
                return;
            }

            string previousText = _lastKnownEditorText;
            SanitizeEditorTextIfNeeded();

            if (_suppressEditorChangeTracking)
                return;

            string currentText = controlContentTextBox.Text;
            if (currentText == previousText)
                return;

            Console.WriteLine("controlContentTextBox_TextChanged");
            RegistrarUndo(previousText);
            _lastKnownEditorText = currentText;
            IsDirty = true;
            SelecionarHojeNoComboArquivo();
            this.AjustaCorFundo();
            if (this.Carregado)
            {
                Console.WriteLine("Carregado = true");
                if (this.SalvarAutom)
                {
                    Console.WriteLine("SalvarAutom = true");
                    if (controlContentTextBox.Text.Length > 0)
                    {
                        Console.WriteLine("Text.Length > 0");
                        if (timer1.Enabled == false)
                        {
                            Console.WriteLine("timer1.Enabled = true");
                            timer1.Enabled = true;
                        }
                    }
                }
            }

        }

        private void EnsureEditorContentSanitized()
        {
            if (_isSanitizingContentText) return;

            SanitizeEditorTextIfNeeded();
        }

        private void SanitizeEditorTextIfNeeded()
        {
            if (_isSanitizingContentText) return;

            string originalText = controlContentTextBox.Text;
            string sanitizedText = SanitizeControlCharacters(originalText);

            if (sanitizedText == originalText) return;

            int selectionStart = controlContentTextBox.SelectionStart;
            int selectionLength = controlContentTextBox.SelectionLength;

            _isSanitizingContentText = true;
            try
            {
                _suppressEditorChangeTracking = true;
                controlContentTextBox.Text = sanitizedText;

                int newSelectionStart = RemapIndexAfterSanitization(originalText, selectionStart);
                int selectionEnd = Math.Min(originalText.Length, selectionStart + selectionLength);
                int newSelectionEnd = RemapIndexAfterSanitization(originalText, selectionEnd);

                controlContentTextBox.SelectionStart = newSelectionStart;
                controlContentTextBox.SelectionLength = Math.Max(0, newSelectionEnd - newSelectionStart);
            }
            finally
            {
                _suppressEditorChangeTracking = false;
                _isSanitizingContentText = false;
            }

            this.Loga("Caracteres de controle removidos do editor.");
        }

        private void DefinirTextoEditorProgramaticamente(string texto, int? selectionStart = null, int? selectionLength = null)
        {
            _suppressEditorChangeTracking = true;
            try
            {
                controlContentTextBox.Text = texto ?? "";
                controlContentTextBox.SelectionStart = Math.Max(0, Math.Min(selectionStart ?? 0, controlContentTextBox.TextLength));
                controlContentTextBox.SelectionLength = Math.Max(0, Math.Min(selectionLength ?? 0, controlContentTextBox.TextLength - controlContentTextBox.SelectionStart));
                _lastKnownEditorText = controlContentTextBox.Text;
            }
            finally
            {
                _suppressEditorChangeTracking = false;
            }
        }

        private void RegistrarUndo(string textoAnterior)
        {
            if (textoAnterior == null)
                textoAnterior = "";

            if (_undoSnapshots.Count > 0 && _undoSnapshots.Peek().Text == textoAnterior)
                return;

            _undoSnapshots.Push(new EditorSnapshot
            {
                Text = textoAnterior,
                SelectionStart = Math.Min(SelectionStart, textoAnterior.Length),
                SelectionLength = 0
            });
        }

        private void DesfazerAlteracaoDoEditor()
        {
            if (_undoSnapshots.Count == 0)
                return;

            EditorSnapshot snapshot = _undoSnapshots.Pop();
            DefinirTextoEditorProgramaticamente(snapshot.Text, snapshot.SelectionStart, snapshot.SelectionLength);
            IsDirty = true;
            this.AjustaCorFundo();

            if (this.Carregado && this.SalvarAutom && controlContentTextBox.TextLength > 0)
                timer1.Enabled = true;
        }

        private static int RemapIndexAfterSanitization(string text, int originalIndex)
        {
            if (string.IsNullOrEmpty(text) || originalIndex <= 0) return 0;
            if (originalIndex > text.Length) originalIndex = text.Length;

            int sanitizedIndex = 0;
            for (int i = 0; i < originalIndex; i++)
            {
                sanitizedIndex += GetNormalizedCharacterLength(text, i, out bool skipNext);
                if (skipNext)
                    i++;
            }

            return sanitizedIndex;
        }

        private static string SanitizeControlCharacters(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sanitized = null;

            for (int i = 0; i < text.Length; i++)
            {
                AppendNormalizedCharacter(text, i, ref sanitized, out bool skipNext);
                if (skipNext)
                    i++;
            }

            return sanitized == null ? text : sanitized.ToString();
        }

        private static void AppendNormalizedCharacter(string text, int index, ref StringBuilder sanitized, out bool skipNext)
        {
            skipNext = false;
            char current = text[index];
            string normalizedValue = GetNormalizedCharacterValue(text, index, out skipNext);

            if (normalizedValue == null)
            {
                if (sanitized == null)
                {
                    sanitized = new StringBuilder(text.Length);
                    sanitized.Append(text, 0, index);
                }
                return;
            }

            if (sanitized == null)
            {
                bool unchanged = normalizedValue.Length == 1 && normalizedValue[0] == current;
                if (unchanged)
                    return;

                sanitized = new StringBuilder(text.Length + 8);
                sanitized.Append(text, 0, index);
            }

            sanitized.Append(normalizedValue);
        }

        private static int GetNormalizedCharacterLength(string text, int index, out bool skipNext)
        {
            string normalizedValue = GetNormalizedCharacterValue(text, index, out skipNext);
            return normalizedValue == null ? 0 : normalizedValue.Length;
        }

        private static string GetNormalizedCharacterValue(string text, int index, out bool skipNext)
        {
            skipNext = false;
            char value = text[index];

            if (value == '\r')
            {
                if (index + 1 < text.Length && text[index + 1] == '\n')
                    skipNext = true;

                return Environment.NewLine;
            }

            if (value == '\n' || value == '\u0085' || value == '\u2028' || value == '\u2029' || value == '\v' || value == '\f')
                return Environment.NewLine;

            if (value == '\t')
                return "\t";

            return char.IsControl(value) ? null : value.ToString();
        }

        private static bool ShouldKeepCharacter(char value)
        {
            if (value == '\r' || value == '\n' || value == '\t')
                return true;

            return !char.IsControl(value);
        }

        private void PasteNormalizedClipboardText()
        {
            if (!Clipboard.ContainsText(TextDataFormat.UnicodeText) && !Clipboard.ContainsText())
            {
                controlContentTextBox.Paste();
                return;
            }

            string clipboardText = Clipboard.ContainsText(TextDataFormat.UnicodeText)
                ? Clipboard.GetText(TextDataFormat.UnicodeText)
                : Clipboard.GetText();

            if (string.IsNullOrEmpty(clipboardText))
                return;

            SelectedText = SanitizeControlCharacters(clipboardText);
        }

        public bool WordWrap
        {
            get
            {
                return controlContentTextBox.WordWrap;
            }
            set
            {
                menuitemFormatWordWrap.Checked = controlContentTextBox.WordWrap = value;
            }
        }

        private static Properties.Settings Settings
        {
            get { return Properties.Settings.Default; }
        }

        private Font CurrentFont
        {
            get
            {
                string familia = cIni.ReadString("Config", "EditorFontFamily", EditorFontDefaultFamily);
                float tamanho = cIni.ReadFloat("Config", "EditorFontSize", EditorFontDefaultSize);
                int estiloInt = cIni.ReadInt("Config", "EditorFontStyle", (int)FontStyle.Regular);

                if (tamanho < 6f)
                    tamanho = 6f;

                if (tamanho > 72f)
                    tamanho = 72f;

                FontStyle estilo = FontStyle.Regular;
                if (Enum.IsDefined(typeof(FontStyle), estiloInt))
                    estilo = (FontStyle)estiloInt;

                try
                {
                    return new Font(familia, tamanho, estilo);
                }
                catch
                {
                    return new Font(EditorFontDefaultFamily, EditorFontDefaultSize, FontStyle.Regular);
                }
            }
            set
            {
                controlContentTextBox.Font = value;
                cIni.WriteString("Config", "EditorFontFamily", value.FontFamily.Name);
                cIni.WriteFloat("Config", "EditorFontSize", value.Size);
                cIni.WriteInt("Config", "EditorFontStyle", (int)value.Style);
            }
        }

        private Font CurrentComboFont
        {
            get
            {
                float tamanho = ComboFontDefaultSize;

                try
                {
                    tamanho = cIni.ReadFloat("Config", "ComboFontSize", ComboFontDefaultSize);
                }
                catch (Exception ex)
                {
                    this.Loga("Erro ao ler tamanho da fonte dos combos: " + ex.Message);
                }

                if (tamanho < ComboFontMinSize)
                    tamanho = ComboFontMinSize;

                if (tamanho > ComboFontMaxSize)
                    tamanho = ComboFontMaxSize;

                Font baseFont = cbProjetos.Font ?? this.Font;
                return new Font(baseFont.FontFamily, tamanho, baseFont.Style, baseFont.Unit);
            }
            set
            {
                AplicarFonteDosCombos(value);

                try
                {
                    cIni.WriteFloat("Config", "ComboFontSize", value.Size);
                }
                catch (Exception ex)
                {
                    this.Loga("Erro ao salvar tamanho da fonte dos combos: " + ex.Message);
                }
            }
        }

        private void AplicarFonteDosCombos(Font fonte)
        {
            if (fonte == null)
                return;

            cbProjetos.Font = fonte;
            cbArquivos.Font = fonte;
            cbProjetos.ComboBox.Font = fonte;
            cbArquivos.ComboBox.Font = fonte;
            foreach (ToolStripComboBox combo in _combosSubtarefas)
            {
                combo.Font = fonte;
                combo.ComboBox.Font = fonte;
            }
            AjustarLarguraDosCombos();
            menubarMain.PerformLayout();
            this.PerformLayout();
            this.Refresh();
        }

        private void AjustarLarguraDosCombos()
        {
            int larguraArquivos = Math.Max(110, MedirLarguraCombo(cbArquivos, "99/99/9999"));

            cbProjetos.Size = new Size(155, cbProjetos.Height);
            foreach (ToolStripComboBox combo in _combosSubtarefas)
                combo.Size = new Size(145, combo.Height);
            cbArquivos.Size = new Size(larguraArquivos, cbArquivos.Height);
            cbArquivos.DropDownWidth = larguraArquivos;
        }

        private int MedirLarguraCombo(ToolStripComboBox combo, string textoBase)
        {
            Font fonte = combo.ComboBox.Font ?? combo.Font ?? this.Font;
            Size tamanhoTexto = TextRenderer.MeasureText(textoBase, fonte);
            return tamanhoTexto.Width + SystemInformation.VerticalScrollBarWidth + 18;
        }

        public bool IsDirty
        {
            get
            {
                if (Filename == null && Content.IsEmpty()) return false;
                return _IsDirty;
            }
            set
            {
                _IsDirty = value;
            }
        }

        private bool EnsureWorkNotLost()
        {
            if (!IsDirty) return true;

            if (controlContentTextBox.Text.Length == 0) return true;

            var DialogResult = new SaveChangesPrompt(Filename).ShowDialog(this);

            switch (DialogResult)
            {
                case DialogResult.Yes:
                    return Save();
                case DialogResult.No:
                    return true;
                case DialogResult.Cancel:
                    return false;
                default:
                    throw new Exception();
            }
        }

        private PageSettings PageSettings
        {
            get
            {
                if (_PageSettings == null)
                {
                    if (Settings.MoreSettings.PageSettings != null)
                    {
                        _PageSettings = Settings.MoreSettings.PageSettings;
                    }
                    else
                    {
                        var PageSettings = new PageSettings()
                        {
                            Margins = new Margins(75, 75, 100, 100), // 100 = 1 inch
                        };

                        _PageSettings = PageSettings;
                    }
                }

                return _PageSettings;
            }
            set
            {
                Settings.MoreSettings.PageSettings = value;
                Settings.Save();
            }
        }

        private enum DrawStringPosition
        {
            Left,
            Center,
            Right,
        }

        private void DrawStringAtPosition(Graphics pGraphics, string pText, int Top, DrawStringPosition pPosition)
        {
            var HeaderTextSize = new SizeF(pGraphics.MeasureString(pText, CurrentFont));
            var HeaderTextWidth = HeaderTextSize.Width;
            var PageWidth = PageSettings.Bounds.Right - PageSettings.Bounds.Left;

            float Left;

            if (pPosition == DrawStringPosition.Left)
            {
                Left = PageSettings.Margins.Left;
            }
            else if (pPosition == DrawStringPosition.Center)
            {
                Left = ((PageWidth - HeaderTextWidth) / 2);
            }
            else if (pPosition == DrawStringPosition.Right)
            {
                Left = PageWidth - PageSettings.Margins.Right - HeaderTextWidth;
            }
            else
            {
                throw new Exception();
            }

            pGraphics.DrawString(pText, CurrentFont, Brushes.Black, Left, Top);
        }

        private class HeaderOrFooterInfo
        {
            public string Left = "";
            public string Center = "";
            public string Right = "";
        }

        private HeaderOrFooterInfo FormatHeaderFooterText(string pText, int PageIndex)
        {
            var HeaderOrFooterInfo = GetHeaderOrFooterInfo(pText);

            HeaderOrFooterInfo.Left = FormatSingleHeaderFooterText(HeaderOrFooterInfo.Left, PageIndex);
            HeaderOrFooterInfo.Center = FormatSingleHeaderFooterText(HeaderOrFooterInfo.Center, PageIndex);
            HeaderOrFooterInfo.Right = FormatSingleHeaderFooterText(HeaderOrFooterInfo.Right, PageIndex);

            return HeaderOrFooterInfo;
        }

        private string FormatSingleHeaderFooterText(string pText, int PageIndex)
        {
            return pText
                        .Replace("&f", DocumentName)
                        .Replace("&p", (PageIndex + 1).ToString())
                        .Replace("&d", Fun.Agora().ToLongDateString())
                        .Replace("&t", Fun.Agora().ToLongTimeString())
                        ;
        }

        private static HeaderOrFooterInfo GetHeaderOrFooterInfo(string pText)
        {
            const string CONST_Left = "Left";
            const string CONST_Center = "Center";
            const string CONST_Right = "Right";

            var LeftIndexes = Helper.GetIndexes(pText, "&l", false);
            var CenterIndexes = Helper.GetIndexes(pText, "&c", false);
            var RightIndexes = Helper.GetIndexes(pText, "&r", false);

            var SideInfos =
                LeftIndexes.Select(o => new { Side = CONST_Left, Index = o })
                .Union(CenterIndexes.Select(o => new { Side = CONST_Center, Index = o }))
                .Union(RightIndexes.Select(o => new { Side = CONST_Right, Index = o }))
                .OrderBy(o => o.Index)
                .ToList()
                ;

            var HeaderOrFooterInfo = new HeaderOrFooterInfo();

            if (SideInfos.Count == 0)
            {
                HeaderOrFooterInfo.Center = pText;
                return HeaderOrFooterInfo;
            }


            for (int i = 0; i < SideInfos.Count; i++)
            {
                var SideInfo = SideInfos[i];
                var IsFirstSideInfo = (i == 0);
                var IsLastSideInfo = (i == (SideInfos.Count - 1));

                if (IsFirstSideInfo)
                {
                    if (SideInfo.Index != 0)
                    {
                        HeaderOrFooterInfo.Center = pText.Substring(0, SideInfo.Index - 1);
                    }
                }

                var StartIndex = SideInfo.Index + 2;

                var EndIndex = 0;
                if (IsLastSideInfo)
                {
                    EndIndex = pText.Length - 1;
                }
                else
                {
                    var NextSideInfo = SideInfos[i + 1];
                    EndIndex = NextSideInfo.Index - 1;
                }

                var Length = EndIndex - StartIndex + 1;
                var Text = pText.Substring(StartIndex, Length);

                switch (SideInfo.Side)
                {
                    case CONST_Left:
                        HeaderOrFooterInfo.Left += Text;
                        break;
                    case CONST_Center:
                        HeaderOrFooterInfo.Center += Text;
                        break;
                    case CONST_Right:
                        HeaderOrFooterInfo.Right += Text;
                        break;
                    default:
                        throw new Exception();
                }
            }
            return HeaderOrFooterInfo;
        }

        public string SelectedText
        {
            get { return controlContentTextBox.SelectedText; }
            set
            {
                controlContentTextBox.SelectedText = value;
                IsDirty = true;
            }
        }

        private ContentPosition CaretPosition
        {
            get { return CharIndexToPosition(SelectionStart); }
        }

        private ContentPosition CharIndexToPosition(int pCharIndex)
        {
            var CurrentCharIndex = 0;
            if (controlContentTextBox.Lines.Length == 0 && CurrentCharIndex == 0) return new ContentPosition { LineIndex = 0, ColumnIndex = 0 };
            for (var CurrentLineIndex = 0; CurrentLineIndex < controlContentTextBox.Lines.Length; CurrentLineIndex++)
            {
                var LineStartCharIndex = CurrentCharIndex;
                var Line = controlContentTextBox.Lines[CurrentLineIndex];
                var LineEndCharIndex = LineStartCharIndex + Line.Length + 1;
                if (pCharIndex >= LineStartCharIndex && pCharIndex <= LineEndCharIndex)
                {
                    var ColumnIndex = pCharIndex - LineStartCharIndex;
                    return new ContentPosition { LineIndex = CurrentLineIndex, ColumnIndex = ColumnIndex };
                }
                CurrentCharIndex += controlContentTextBox.Lines[CurrentLineIndex].Length + Environment.NewLine.Length;
            }
            return null;
        }

        private void UpdateStatusBar()
        {
            long x = Fun.Agora().Ticks;
            long inter = x - this.Tick;
            if (inter > 10000000)
            {
                /* if (this.QtdCarac < 1000)
                {
                    if (controlCaretPositionLabel.Tag == null)
                    {
                        controlCaretPositionLabel.Tag = controlCaretPositionLabel.Text;
                    }
                    controlCaretPositionLabel.Text = ((string)controlCaretPositionLabel.Tag).FormatUsingObject(new
                    {
                        LineNumber = CaretPosition.LineIndex + 1,
                        ColumnNumber = CaretPosition.ColumnIndex + 1,
                    });
                    controlCaretPositionLabel.Visible = true;
                }
                else
                    controlCaretPositionLabel.Visible = false; */
                this.MotraCaracteres();
                this.Tick = x;
            }
        }

        private void MotraCaracteres()
        {
            this.QtdCarac = controlContentTextBox.Text.Length;
            if (this.QtdCarac > 0)
                toolStripStatusLabel1.Text = this.QtdCarac.ToString() + " Caracteres";
            else
                toolStripStatusLabel1.Text = "";
        }

        private int LineIndex
        {
            get { return CaretPosition.LineIndex; }
            set
            {
                var TargetLineIndex = value;
                if (TargetLineIndex < 0)
                    TargetLineIndex = 0;
                if (TargetLineIndex >= controlContentTextBox.Lines.Length)
                    TargetLineIndex = controlContentTextBox.Lines.Length - 1;
                var CharIndex = 0;
                for (var CurrentLineIndex = 0; CurrentLineIndex < TargetLineIndex; CurrentLineIndex++)
                    CharIndex += controlContentTextBox.Lines[CurrentLineIndex].Length + Environment.NewLine.Length;
                SelectionStart = CharIndex;
                controlContentTextBox.ScrollToCaret();
            }
        }

        public int SelectionEnd
        {
            get { return SelectionStart + SelectionLength; }
        }

        public int SelectionStart
        {
            get { return controlContentTextBox.SelectionStart; }
            set
            {
                controlContentTextBox.SelectionStart = value;
                controlContentTextBox.ScrollToCaret();
            }
        }

        public int SelectionLength
        {
            get { return controlContentTextBox.SelectionLength; }
            set { controlContentTextBox.SelectionLength = value; }
        }

        private void DeleteEditorSelectionOrNextCharacter()
        {
            if (SelectionLength == 0)
            {
                int nextCharacterLength = GetNextDeletableCharacterLength();
                if (nextCharacterLength == 0)
                    return;

                SelectionLength = nextCharacterLength;
            }

            controlContentTextBox.SelectedText = "";
        }

        private int GetNextDeletableCharacterLength()
        {
            if (SelectionStart >= controlContentTextBox.TextLength)
                return 0;

            string text = controlContentTextBox.Text;
            char current = text[SelectionStart];

            if (current == '\r' && SelectionStart + 1 < text.Length && text[SelectionStart + 1] == '\n')
                return 2;

            return 1;
        }

        private bool TentarTabularSelecaoMultilinha()
        {
            if (SelectionLength <= 0)
                return false;

            int inicioSelecao = SelectionStart;
            int fimSelecao = SelectionStart + SelectionLength;
            int linhaInicial = controlContentTextBox.GetLineFromCharIndex(inicioSelecao);
            int indiceFinalParaLinha = Math.Max(inicioSelecao, fimSelecao - 1);
            int linhaFinal = controlContentTextBox.GetLineFromCharIndex(indiceFinalParaLinha);

            if (linhaInicial >= linhaFinal)
                return false;

            string textoOriginal = controlContentTextBox.Text;
            List<int> iniciosDasLinhas = new List<int>();
            StringBuilder textoTabulado = new StringBuilder(textoOriginal);
            int deslocamento = 0;

            for (int linha = linhaInicial; linha <= linhaFinal; linha++)
            {
                int inicioLinha = controlContentTextBox.GetFirstCharIndexFromLine(linha);
                if (inicioLinha < 0)
                    continue;

                iniciosDasLinhas.Add(inicioLinha);
                textoTabulado.Insert(inicioLinha + deslocamento, '\t');
                deslocamento++;
            }

            if (iniciosDasLinhas.Count == 0)
                return false;

            int tabsAntesDoInicio = iniciosDasLinhas.Count(indice => indice < inicioSelecao);
            int tabsAntesDoFim = iniciosDasLinhas.Count(indice => indice < fimSelecao);
            int novoInicioSelecao = inicioSelecao + tabsAntesDoInicio;
            int novoFimSelecao = fimSelecao + tabsAntesDoFim;

            RegistrarUndo(textoOriginal);
            DefinirTextoEditorProgramaticamente(
                textoTabulado.ToString(),
                novoInicioSelecao,
                Math.Max(0, novoFimSelecao - novoInicioSelecao));

            IsDirty = true;
            this.AjustaCorFundo();

            if (this.Carregado && this.SalvarAutom && controlContentTextBox.TextLength > 0)
                timer1.Enabled = true;

            return true;
        }

        private void controlContentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                DesfazerAlteracaoDoEditor();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Tab && !e.Control && !e.Alt && !e.Shift)
            {
                if (TentarTabularSelecaoMultilinha())
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    return;
                }
            }

            if (e.Shift && e.KeyCode == Keys.Delete && !e.Control && !e.Alt)
            {
                controlContentTextBox.Cut();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.Insert && !e.Shift && !e.Alt)
            {
                controlContentTextBox.Copy();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
            {
                PasteNormalizedClipboardText();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Delete && !e.Alt && !e.Control && !e.Shift)
            {
                DeleteEditorSelectionOrNextCharacter();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            UpdateStatusBar();
        }

        private void controlContentTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateStatusBar();
        }

        private void controlContentTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            UpdateStatusBar();
        }

        private void controlContentTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            int LetrasSel = controlContentTextBox.SelectedText.Length;
            if (LetrasSel > 0)
            {
                toolStripStatusLabel1.Text = LetrasSel.ToString() + " Caractres Selecionados";
            }
        }

        #endregion

        #region Busca

        public bool FindAndSelect(string pSearchText, bool pMatchCase, bool pSearchDown)
        {
            int Index;

            var eStringComparison = pMatchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            if (pSearchDown)
            {
                Index = Content.IndexOf(pSearchText, SelectionEnd, eStringComparison);
            }
            else
            {
                Index = Content.LastIndexOf(pSearchText, SelectionStart, SelectionStart, eStringComparison);
            }

            if (Index == -1) return false;

            _LastSearchText = pSearchText;
            _LastMatchCase = pMatchCase;
            _LastSearchDown = pSearchDown;

            SelectionStart = Index;
            SelectionLength = pSearchText.Length;

            return true;
        }

        private void Find()
        {
            if (Content.Length == 0) return;

            if (_FindDialog == null)
            {
                _FindDialog = new FindDialog(this);
            }

            _FindDialog.Left = this.Left + 56;
            _FindDialog.Top = this.Top + 160;

            _FindDialog.SelText = controlContentTextBox.SelectedText;

            if (!_FindDialog.Visible)
            {
                _FindDialog.Show(this);
            }
            else
            {
                _FindDialog.Show();
            }

            _FindDialog.Triggered();
        }

        #endregion

        #region Atividades

        public string NomeArq
        {
            get
            {
                if (_NomeArq == null)
                {
                    string Data = Fun.Agora().ToShortDateString().Replace(@"/", "-");
                    return this.Atual + "^" + Data + ".txt";
                }
                else
                {
                    return _NomeArq;
                }
            }
            set
            {
                _NomeArq = value;
            }
        }

        private void configurarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigProjeto FormConfigProjeto = new ConfigProjeto();
            FormConfigProjeto.ShowDialog();
            if (FormConfigProjeto.DialogResult == DialogResult.OK)
            {
                this.SalvarAutom = cIni.ReadBool("Projetos", "SalvarAut", false);
                this.timer1.Interval = this.Segundos * 1000;
                this.PastaGeral = FormConfigProjeto.PastaGeral;
                this.Logar = cIni.ReadBool("Config", "Log", false);
                if (this.Logar)
                    this.PreparaLog();
            }

        }

        private void novoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.PastaGeral == "")
            {
                MessageBox.Show(this, "É necessário configurar primeiro", this.TitAplicativo);
                ConfigProjeto FormConfigProjeto = new ConfigProjeto();
                FormConfigProjeto.ShowDialog();
                if (this.PastaGeral == "") { return; }
            }
            Projeto cPro = new Projeto();
            cPro.ShowDialog();
            Atual = cIni.ReadString("Projetos", "Atual", "");
            if (cPro.DialogResult == DialogResult.OK)
            {
                PreencheCombo(Atual);
                if (cbProjetos.SelectedText != Atual)
                {
                    int pos = cbProjetos.FindString(Atual);
                    cbProjetos.SelectedIndex = pos;
                }
                try
                {
                    this.Escolhido = new cEscolhido();
                    Escolhido.usado = true;
                }
                catch (Exception)
                {
                    // throw;
                }
                Escolhido.Nome = "";
                IsDirty = true;
                NovaTarefa = true;
                LimparEstadoSubtarefasAoTrocarProjeto();
                subAtividadesToolStripMenuItem.Enabled = true;
                renomearToolStripMenuItem1.Enabled = true;

            }
            this.CarregaArquivoDoProjeto(true);
        }

        private void LimparEstadoSubtarefasAoTrocarProjeto()
        {
            _caminhoSubtarefas.Clear();
            cIni.WriteString(this.Atual, "CaminhoAtual", "");
            cIni.WriteString(this.Atual, "SubAtual", "GERAL");
            LimparCombosSubtarefas();
            AtualizarEstadoCaminhoSubtarefas();
            AtualizarMenuSubAtividades();
        }

        private void LimparCombosSubtarefas()
        {
            foreach (ToolStripComboBox combo in _combosSubtarefas.ToList())
            {
                if (combo == null)
                    continue;

                if (combo == cbSubprojeto)
                    continue;

                if (menubarMain.Items.Contains(combo))
                    menubarMain.Items.Remove(combo);

                combo.Dispose();
            }

            _combosSubtarefas.Clear();

            if (cbSubprojeto != null && !cbSubprojeto.IsDisposed)
            {
                if (!_combosSubtarefas.Contains(cbSubprojeto))
                    _combosSubtarefas.Add(cbSubprojeto);

                cbSubprojeto.Visible = false;
                cbSubprojeto.Items.Clear();
                cbSubprojeto.Text = string.Empty;
            }
        }        

        private void PreencheCombo(string Atual)
        {
            cbProjetos.Items.Clear();
            int Qtd = this.cIni.ReadInt("Projetos", "Qtd", 0);
            for (int i = 0; i < Qtd; i++)
            {
                string nmProjeto = "Pro" + (i + 1).ToString();
                string Nome = this.cIni.ReadString("NmProjetos", nmProjeto, "");
                if (Nome.Length > 0)
                {
                    cbProjetos.Items.Add(Nome);
                    if (Nome == Atual)
                    {
                        try
                        {
                            cbProjetos.SelectedIndex = i;
                        }
                        catch (Exception)
                        {
                            try
                            {
                                cbProjetos.SelectedIndex = i - 1;
                            }
                            catch (Exception)
                            {
                                cbProjetos.SelectedIndex = i - 2;
                            }
                        }
                    }
                }
            }
        }

        private void VeSeTemSub(string EssaAtivi)
        {
            if (!string.IsNullOrWhiteSpace(EssaAtivi))
                subAtividadesToolStripMenuItem.Enabled = true;

            int QtdSub = this.cIni.ReadInt(EssaAtivi, "QtdSub", 0);
            this.Loga("Lendo do Ini a quantidade de SubAtividades da Atividade " + EssaAtivi);
            this.Loga("QtdSub = " + QtdSub.ToString());
            if (QtdSub > 0 || TemSubpastas(Path.Combine(this.PastaGeral, EssaAtivi)))
                this.MotraArqSub(QtdSub);
            else
            {
                cbSubprojeto.Visible = false;
                renomearToolStripMenuItem1.Enabled = false;
                AtualizarMenuSubAtividades();
            }
        }

        private void MotraArqSub(int QtdSub)
        {
            CarregarHierarquiaSubtarefas();
        }

        private void InicializarMenuContextoHierarquia()
        {
            _menuContextoHierarquia = new ContextMenuStrip();

            _itemContextoNova = new ToolStripMenuItem("Nova");
            _itemContextoRenomear = new ToolStripMenuItem("Renomear");
            _itemContextoApagar = new ToolStripMenuItem("Apagar");
            _itemContextoMover = new ToolStripMenuItem("Mover");
            _itemContextoCriarSubTarefa = new ToolStripMenuItem("Criar Sub Tarefa");
            _separadorContextoHierarquia = new ToolStripSeparator();

            _itemContextoNova.Click += (s, e) => NovaHierarquiaContexto();
            _itemContextoRenomear.Click += (s, e) => RenomearHierarquiaContexto();
            _itemContextoApagar.Click += (s, e) => ApagarHierarquiaContexto();
            _itemContextoMover.Click += (s, e) => MoverHierarquiaContexto();
            _itemContextoCriarSubTarefa.Click += (s, e) => CriarSubTarefaHierarquiaContexto();

            _menuContextoHierarquia.Items.Add(_itemContextoNova);
            _menuContextoHierarquia.Items.Add(_itemContextoRenomear);
            _menuContextoHierarquia.Items.Add(_itemContextoApagar);
            _menuContextoHierarquia.Items.Add(_itemContextoMover);
            _menuContextoHierarquia.Items.Add(new ToolStripSeparator());
            _menuContextoHierarquia.Items.Add(_itemContextoCriarSubTarefa);
        }

        private void PrepararMenuContextoHierarquia()
        {
            _itemContextoNova.Visible = true;
            _itemContextoRenomear.Visible = true;
            _itemContextoApagar.Visible = true;
            _itemContextoMover.Visible = true;
            _itemContextoCriarSubTarefa.Visible = true;
        }

        private void AssociarMenuContextoHierarquia(ToolStripComboBox combo, int nivel)
        {
            if (combo == null)
                return;

            combo.ComboBox.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Right || _menuContextoHierarquia == null)
                    return;

                _comboContextoHierarquia = combo.ComboBox;
                _nivelContextoHierarquia = nivel;
                PrepararMenuContextoHierarquia();
                _menuContextoHierarquia.Show(combo.ComboBox, e.Location);
            };
        }

        private List<string> CaminhoDoContextoHierarquia()
        {
            if (_nivelContextoHierarquia < 0)
                return new List<string>();

            if (_caminhoSubtarefas == null || _caminhoSubtarefas.Count == 0)
                return new List<string>();

            int nivel = Math.Min(_nivelContextoHierarquia, _caminhoSubtarefas.Count - 1);

            return _caminhoSubtarefas
                .Take(nivel + 1)
                .ToList();
        }

        private List<string> CaminhoPaiMesmoNivelDoContexto()
        {
            if (_nivelContextoHierarquia <= 0)
                return new List<string>();

            if (_caminhoSubtarefas == null || _caminhoSubtarefas.Count == 0)
                return new List<string>();

            int quantidadePai = Math.Min(_nivelContextoHierarquia, _caminhoSubtarefas.Count);

            return _caminhoSubtarefas
                .Take(quantidadePai)
                .ToList();
        }

        private void NovaSubAtividadeHierarquiaContexto()
        {
            NovaHierarquiaContexto();
        }

        private void NovaHierarquiaContexto()
        {
            if (_nivelContextoHierarquia < 0)
            {
                novoToolStripMenuItem_Click(null, EventArgs.Empty);
                return;
            }

            var caminhoPai = CaminhoPaiMesmoNivelDoContexto();
            CriarSubtarefaAbaixoDoCaminho(caminhoPai);
        }

        private void CriarSubTarefaHierarquiaContexto()
        {
            string texto = Convert.ToString(_comboContextoHierarquia?.SelectedItem);
            if (string.IsNullOrWhiteSpace(texto))
                texto = _comboContextoHierarquia?.Text;

            bool ehGeral = string.Equals(texto, "GERAL", StringComparison.OrdinalIgnoreCase);

            if (ehGeral && _nivelContextoHierarquia >= 0)
            {
                var caminhoPaiMesmoNivel = CaminhoPaiMesmoNivelDoContexto();
                CriarSubtarefaAbaixoDoCaminho(caminhoPaiMesmoNivel);
                return;
            }

            List<string> caminhoPai;

            if (_nivelContextoHierarquia < 0)
            {
                caminhoPai = new List<string>();
            }
            else
            {
                caminhoPai = CaminhoDoContextoHierarquia();
            }

            CriarSubtarefaAbaixoDoCaminho(caminhoPai);
        }

        private void MoverHierarquiaContexto()
        {
            if (_nivelContextoHierarquia < 0)
            {
                MessageBox.Show(
                    "Movimentação de projeto principal será implementada depois.",
                    "Mover",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (_caminhoSubtarefas == null || _caminhoSubtarefas.Count == 0)
                return;

            if (_nivelContextoHierarquia >= _caminhoSubtarefas.Count)
                return;

            List<string> caminhoOrigem = _caminhoSubtarefas
                .Take(_nivelContextoHierarquia + 1)
                .ToList();

            string projetoOrigem = this.Atual;
            string pastaProjetoOrigem = PastaDoProjetoAtual();

            using (var frm = new MoverHierarquia(this.PastaGeral, projetoOrigem, caminhoOrigem, pastaProjetoOrigem))
            {
                if (frm.ShowDialog(this) != DialogResult.OK)
                    return;

                string projetoDestino = frm.ProjetoDestinoSelecionado;
                string pastaProjetoDestino = frm.PastaProjetoDestinoSelecionada;
                List<string> caminhoDestino = frm.CaminhoDestinoSelecionado ?? new List<string>();

                if (string.IsNullOrWhiteSpace(projetoDestino) || string.IsNullOrWhiteSpace(pastaProjetoDestino))
                    return;

                if (string.Equals(projetoOrigem, projetoDestino, StringComparison.OrdinalIgnoreCase)
                    && DestinoEhDentroDaOrigem(caminhoOrigem, caminhoDestino))
                {
                    MessageBox.Show(
                        "Não é possível mover uma subtarefa para dentro dela mesma ou de uma subtarefa filha.",
                        "Mover",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MoverSubatividade(
                    caminhoOrigem,
                    pastaProjetoOrigem,
                    projetoOrigem,
                    caminhoDestino,
                    pastaProjetoDestino,
                    projetoDestino);
            }
        }

        private bool DestinoEhDentroDaOrigem(List<string> origem, List<string> destino)
        {
            if (origem == null || destino == null)
                return false;

            if (destino.Count < origem.Count)
                return false;

            for (int i = 0; i < origem.Count; i++)
            {
                if (!string.Equals(origem[i], destino[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private void MoverSubatividade(
            List<string> caminhoOrigem,
            string pastaProjetoOrigem,
            string projetoOrigem,
            List<string> caminhoDestinoPai,
            string pastaProjetoDestino,
            string projetoDestino)
        {
            string nomeMovido = caminhoOrigem[caminhoOrigem.Count - 1];
            string pastaOrigem = PastaDaSubtarefa(pastaProjetoOrigem, caminhoOrigem);

            List<string> caminhoNovo = new List<string>(caminhoDestinoPai);
            caminhoNovo.Add(nomeMovido);

            string pastaDestino = PastaDaSubtarefa(pastaProjetoDestino, caminhoNovo);

            if (!Directory.Exists(pastaOrigem))
            {
                MessageBox.Show(
                    "A pasta de origem não foi encontrada:\n\n" + pastaOrigem,
                    "Mover",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (Directory.Exists(pastaDestino))
            {
                MessageBox.Show(
                    "Já existe uma subtarefa com esse nome no destino.",
                    "Mover",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Directory.Move(pastaOrigem, pastaDestino);
            RenomearArquivosDaSubarvoreMovida(
                projetoOrigem,
                caminhoOrigem,
                projetoDestino,
                caminhoNovo,
                pastaDestino);

            if (!string.Equals(this.Atual, projetoDestino, StringComparison.OrdinalIgnoreCase))
                AplicarSelecaoProjeto(projetoDestino);

            _caminhoSubtarefas.Clear();
            _caminhoSubtarefas.AddRange(caminhoNovo);

            cIni.WriteString(this.Atual, "CaminhoAtual", CaminhoSubtarefasAtual());

            AtualizarEstadoCaminhoSubtarefas();
            CarregarHierarquiaSubtarefas();
            AbrirAnotacaoDaHierarquiaAtual(true);
            AtualizarMenuSubAtividades();
        }

        private void RenomearArquivosDaSubarvoreMovida(
            string projetoOrigem,
            List<string> caminhoAntigoRaiz,
            string projetoDestino,
            List<string> caminhoNovoRaiz,
            string pastaNovaRaiz)
        {
            string prefixoAntigo = NomeBaseAnotacao(projetoOrigem, caminhoAntigoRaiz);
            string prefixoNovo = NomeBaseAnotacao(projetoDestino, caminhoNovoRaiz);

            foreach (string arquivo in Directory.GetFiles(pastaNovaRaiz, "*.txt", SearchOption.AllDirectories))
            {
                string pasta = Path.GetDirectoryName(arquivo);
                string nomeArquivo = Path.GetFileNameWithoutExtension(arquivo);
                string extensao = Path.GetExtension(arquivo);

                if (!nomeArquivo.StartsWith(prefixoAntigo, StringComparison.OrdinalIgnoreCase))
                    continue;

                string novoNomeArquivo = prefixoNovo + nomeArquivo.Substring(prefixoAntigo.Length) + extensao;
                string novoCaminhoArquivo = Path.Combine(pasta, novoNomeArquivo);

                if (!File.Exists(novoCaminhoArquivo))
                    File.Move(arquivo, novoCaminhoArquivo);
            }
        }

        private void RenomearHierarquiaContexto()
        {
            if (_nivelContextoHierarquia < 0)
            {
                renomearToolStripMenuItem1_Click(this, EventArgs.Empty);
                return;
            }

            RenomearSubatividadePorNivel(_nivelContextoHierarquia);
        }

        private void ApagarHierarquiaContexto()
        {
            if (_comboContextoHierarquia == null)
                return;

            string texto = Convert.ToString(_comboContextoHierarquia.SelectedItem);
            if (string.IsNullOrWhiteSpace(texto))
                texto = _comboContextoHierarquia.Text;

            if (string.Equals(texto, "GERAL", StringComparison.OrdinalIgnoreCase))
                return;

            if (_nivelContextoHierarquia < 0)
            {
                toolStripMenuItem1_Click(this, EventArgs.Empty);
                return;
            }

            ApagarSubatividadePorNivel(_nivelContextoHierarquia);
        }

        private void ApagarSubatividade()
        {
            if (_caminhoSubtarefas == null || _caminhoSubtarefas.Count == 0)
                return;

            ApagarSubatividadePorNivel(_caminhoSubtarefas.Count - 1);
        }

        private void ApagarSubatividadePorNivel(int nivel)
        {
            if (_caminhoSubtarefas == null || _caminhoSubtarefas.Count == 0)
                return;

            if (nivel < 0 || nivel >= _caminhoSubtarefas.Count)
                return;

            List<string> caminhoAtualCompleto = new List<string>(_caminhoSubtarefas);
            string nome = caminhoAtualCompleto[nivel];

            if (string.Equals(nome, "GERAL", StringComparison.OrdinalIgnoreCase))
                return;

            List<string> caminhoParaApagar = caminhoAtualCompleto
                .Take(nivel + 1)
                .ToList();

            string pastaParaApagar = PastaDaSubtarefa(caminhoParaApagar);

            if (!Directory.Exists(pastaParaApagar))
            {
                MessageBox.Show(
                    "A pasta da subtarefa não foi encontrada:\n\n" + pastaParaApagar,
                    "Apagar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult resp = MessageBox.Show(
                "Deseja apagar a subtarefa \"" + nome + "\" e todas as subtarefas abaixo dela?",
                "Confirmar exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (resp != DialogResult.Yes)
                return;

            try
            {
                Directory.Delete(pastaParaApagar, true);

                List<string> caminhoPai = caminhoAtualCompleto
                    .Take(nivel)
                    .ToList();

                _caminhoSubtarefas.Clear();
                _caminhoSubtarefas.AddRange(caminhoPai);

                cIni.WriteString(this.Atual, "CaminhoAtual", CaminhoSubtarefasAtual());

                AtualizarEstadoCaminhoSubtarefas();
                CarregarHierarquiaSubtarefas();
                AbrirAnotacaoDaHierarquiaAtual(true);
                AtualizarMenuSubAtividades();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível apagar a subtarefa.\n\n" + ex.Message,
                    "Erro ao apagar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AtualizarMenuSubAtividades()
        {
            const string nomeSeparador = "menuSubAtividadeHierarquiaSeparador";
            const string nomeMenuHierarquia = "menuSubAtividadeHierarquia";
            bool temSubatividadeSelecionada = _caminhoSubtarefas.Count > 0;

            renomearToolStripMenuItem.Visible = temSubatividadeSelecionada;
            apagarToolStripMenuItem.Visible = temSubatividadeSelecionada;

            for (int i = subAtividadesToolStripMenuItem.DropDownItems.Count - 1; i >= 0; i--)
            {
                ToolStripItem item = subAtividadesToolStripMenuItem.DropDownItems[i];
                if (item.Name == nomeSeparador || item.Name == nomeMenuHierarquia)
                    subAtividadesToolStripMenuItem.DropDownItems.RemoveAt(i);
            }

            ToolStripSeparator separador = new ToolStripSeparator();
            separador.Name = nomeSeparador;
            ToolStripMenuItem menuHierarquia = new ToolStripMenuItem("Sub-atividade");
            menuHierarquia.Name = nomeMenuHierarquia;

            if (temSubatividadeSelecionada)
            {
                separador.Visible = true;
                menuHierarquia.Visible = true;
                PreencherMenuSubAtividades(menuHierarquia.DropDownItems, 0, Math.Max(1, _caminhoSubtarefas.Count));
                subAtividadesToolStripMenuItem.DropDownItems.Add(separador);
                subAtividadesToolStripMenuItem.DropDownItems.Add(menuHierarquia);
            }
            else
            {
                menuHierarquia.Visible = false;
            }
        }

        private void PreencherMenuSubAtividades(ToolStripItemCollection itens, int nivel, int profundidadeTotal)
        {
            if (nivel < 0 || nivel >= profundidadeTotal)
                return;

            List<string> caminhoAtual = _caminhoSubtarefas.Take(Math.Min(nivel + 1, _caminhoSubtarefas.Count)).ToList();

            if (caminhoAtual.Count == 0)
            {
                ToolStripMenuItem novaSo = new ToolStripMenuItem("Nova");
                novaSo.Click += (sender, e) => CriarSubtarefaAbaixoDoCaminho(new List<string>());
                itens.Add(novaSo);
                return;
            }

            ToolStripMenuItem nova = new ToolStripMenuItem("Nova");
            nova.Click += (sender, e) => CriarSubtarefaAbaixoDoCaminho(new List<string>());
            itens.Add(nova);

            itens.Add(CriarItemAcaoSubatividade("Renomear", () => RenomearSubatividade(caminhoAtual)));
            itens.Add(CriarItemAcaoSubatividade("Apagar", () => ApagarSubatividade(caminhoAtual)));
            itens.Add(new ToolStripSeparator());

            ToolStripMenuItem submenu = new ToolStripMenuItem("Sub-atividade");
            itens.Add(submenu);
            List<string> caminhoFilho = new List<string>(caminhoAtual);
            ToolStripMenuItem itemNovaFilha = new ToolStripMenuItem("Nova");
            itemNovaFilha.Click += (s, e) =>
            {
                CriarSubtarefaAbaixoDoCaminho(caminhoFilho);
            };
            submenu.DropDownItems.Add(itemNovaFilha);
        }

        private ToolStripMenuItem CriarItemAcaoSubatividade(string texto, Action acao)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(texto);
            item.Click += (sender, e) => acao();
            return item;
        }

        private void CarregarHierarquiaSubtarefas()
        {
            string caminhoSalvo = cIni.ReadString(this.Atual, "CaminhoAtual", "");
            if (string.IsNullOrWhiteSpace(caminhoSalvo))
                caminhoSalvo = cIni.ReadString(this.Atual, "SubAtual", "");

            string[] partes = caminhoSalvo
                .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            _atualizandoCombosSubtarefas = true;
            try
            {
                _comboHierarquiaSelecionado = null;
                RemoverCombosSubtarefasDepoisDoNivel(0);
                _caminhoSubtarefas.Clear();
                PopularComboSubtarefa(cbSubprojeto, PastaDoProjetoAtual(), true);
                cbSubprojeto.Visible = cbSubprojeto.Items.Count > 1;

                ToolStripComboBox comboAtual = cbSubprojeto;
                for (int nivel = 0; nivel < partes.Length; nivel++)
                {
                    int indice = comboAtual.FindStringExact(partes[nivel]);
                    if (indice < 0)
                        break;

                    comboAtual.SelectedIndex = indice;
                    _caminhoSubtarefas.Add(partes[nivel]);

                    string pastaAtual = PastaDaSubtarefaAtual();
                    if (!TemSubpastas(pastaAtual))
                        break;

                    comboAtual = CriarComboSubtarefaDinamico();
                    PopularComboSubtarefa(comboAtual, pastaAtual, false);
                }

                if (_caminhoSubtarefas.Count == 0 && cbSubprojeto.Items.Count > 0)
                    cbSubprojeto.SelectedIndex = cbSubprojeto.FindStringExact("GERAL");

                AtualizarEstadoCaminhoSubtarefas();
                AtualizarMenuSubAtividades();
            }
            finally
            {
                _atualizandoCombosSubtarefas = false;
            }
        }

        private void PopularComboSubtarefa(ToolStripComboBox combo, string pastaPai, bool primeiroNivel)
        {
            combo.Items.Clear();
            AdicionarItemUnico(combo.Items, "GERAL");

            SortedSet<string> nomes = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
            if (Directory.Exists(pastaPai))
            {
                foreach (string pasta in Directory.GetDirectories(pastaPai))
                    nomes.Add(Path.GetFileName(pasta));
            }

            if (primeiroNivel)
            {
                int qtdSub = cIni.ReadInt(this.Atual, "QtdSub", 0);
                for (int i = 1; i <= qtdSub; i++)
                {
                    string nome = cIni.ReadString(this.Atual, "Sub" + i, "");
                    if (!string.IsNullOrWhiteSpace(nome) && Directory.Exists(Path.Combine(pastaPai, nome)))
                        nomes.Add(nome);
                }
            }

            foreach (string nome in nomes)
                AdicionarItemUnico(combo.Items, nome);

            combo.Visible = combo.Items.Count > 1;
        }

        private void AdicionarItemUnico(System.Windows.Forms.ComboBox.ObjectCollection items, string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return;

            foreach (object item in items)
            {
                if (string.Equals(Convert.ToString(item), valor, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            items.Add(valor);
        }

        private ToolStripComboBox CriarComboSubtarefaDinamico()
        {
            ToolStripComboBox combo = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(145, cbSubprojeto.Height),
                Font = cbSubprojeto.Font,
                Name = "cbSubprojetoNivel" + (_combosSubtarefas.Count + 1),
                Overflow = ToolStripItemOverflow.AsNeeded
            };

            combo.Tag = _combosSubtarefas.Count;

            int nivelCombo = combo.Tag is int n ? n : _combosSubtarefas.Count;

            AssociarMenuContextoHierarquia(combo, nivelCombo);

            combo.SelectedIndexChanged += cbSubprojeto_SelectedIndexChanged;

            int indiceDatas = menubarMain.Items.IndexOf(cbArquivos);
            menubarMain.Items.Insert(indiceDatas, combo);

            _combosSubtarefas.Add(combo);

            return combo;
        }

        private void RemoverCombosSubtarefasDepoisDoNivel(int nivel)
        {
            for (int i = _combosSubtarefas.Count - 1; i > nivel; i--)
            {
                ToolStripComboBox combo = _combosSubtarefas[i];
                menubarMain.Items.Remove(combo);
                combo.Dispose();
                _combosSubtarefas.RemoveAt(i);
            }
        }

        private bool TemSubpastas(string pasta)
        {
            return Directory.Exists(pasta) && Directory.GetDirectories(pasta).Length > 0;
        }

        private string PastaDoProjetoAtual()
        {
            return Path.Combine(this.PastaGeral, this.Atual);
        }

        private string PastaDaSubtarefaAtual()
        {
            string pasta = PastaDoProjetoAtual();
            foreach (string parte in _caminhoSubtarefas)
                pasta = Path.Combine(pasta, parte);
            return pasta;
        }

        private string PastaDaSubtarefa(IEnumerable<string> caminhoSubtarefas)
        {
            return PastaDaSubtarefa(PastaDoProjetoAtual(), caminhoSubtarefas);
        }

        private string PastaDaSubtarefa(string pastaProjeto, IEnumerable<string> caminhoSubtarefas)
        {
            string pasta = pastaProjeto;
            foreach (string parte in caminhoSubtarefas)
            {
                if (!string.IsNullOrWhiteSpace(parte) &&
                    !string.Equals(parte, "GERAL", StringComparison.OrdinalIgnoreCase))
                {
                    pasta = Path.Combine(pasta, parte);
                }
            }
            return pasta;
        }

        private string CaminhoSubtarefasAtual()
        {
            return string.Join("\\", _caminhoSubtarefas);
        }

        private string CaminhoSubtarefasAtual(IEnumerable<string> caminhoSubtarefas)
        {
            return string.Join("\\", caminhoSubtarefas);
        }

        private string NomeBaseAnotacaoAtual()
        {
            return NomeBaseAnotacao(this.Atual, _caminhoSubtarefas);
        }

        private string NomeBaseAnotacao(IEnumerable<string> caminhoSubtarefas)
        {
            return NomeBaseAnotacao(this.Atual, caminhoSubtarefas);
        }

        private string NomeBaseAnotacao(string projeto, IEnumerable<string> caminhoSubtarefas)
        {
            List<string> partes = new List<string>();
            partes.Add(projeto);

            if (caminhoSubtarefas != null)
            {
                partes.AddRange(caminhoSubtarefas.Where(p =>
                    !string.IsNullOrWhiteSpace(p) &&
                    !string.Equals(p, "GERAL", StringComparison.OrdinalIgnoreCase)));
            }

            return string.Join("^", partes);
        }

        private void CarregarCaminhoSubtarefasPersistido()
        {
            string caminho = cIni.ReadString(this.Atual, "CaminhoAtual", "");
            if (string.IsNullOrWhiteSpace(caminho))
                caminho = cIni.ReadString(this.Atual, "SubAtual", "");

            _caminhoSubtarefas.Clear();
            string pastaAtual = PastaDoProjetoAtual();

            foreach (string parte in caminho.Split(new[] { '\\', '/', '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (parte == "GERAL")
                    continue;

                string pastaFilha = Path.Combine(pastaAtual, parte);
                if (!Directory.Exists(pastaFilha))
                    break;

                _caminhoSubtarefas.Add(parte);
                pastaAtual = pastaFilha;
            }
            this._SUbAtual = CaminhoSubtarefasAtual();
        }

        private void AtualizarEstadoCaminhoSubtarefas()
        {
            string caminho = CaminhoSubtarefasAtual();
            this._SUbAtual = caminho;
            cIni.WriteString(this.Atual, "CaminhoAtual", caminho);
            cIni.WriteString(this.Atual, "SubAtual", _caminhoSubtarefas.Count > 0 ? _caminhoSubtarefas[0] : "GERAL");
            renomearToolStripMenuItem.Enabled = _caminhoSubtarefas.Count > 0;
            apagarToolStripMenuItem.Enabled = renomearToolStripMenuItem.Enabled;
            if (this.Atual.Length > 0)
                AtualizarMenuSubAtividades();
        }

        private void CriarSubtarefaAbaixoDoCombo(ToolStripComboBox combo)
        {
            int nivel = _combosSubtarefas.IndexOf(combo);
            if (nivel < 0)
                nivel = _combosSubtarefas.Count - 1;

            List<string> caminhoPai = _caminhoSubtarefas.Take(Math.Min(nivel + 1, _caminhoSubtarefas.Count)).ToList();
            CriarSubtarefaAbaixoDoCaminho(caminhoPai);
        }

        private void CriarSubtarefaAbaixoDoCaminho(List<string> caminhoPai)
        {
            string tituloPai = caminhoPai.Count == 0 ? this.Atual : caminhoPai[caminhoPai.Count - 1];
            SubAtividade dialogo = new SubAtividade(tituloPai, true);
            dialogo.ShowDialog();
            if (dialogo.DialogResult != DialogResult.OK)
                return;

            string nome = dialogo.Nome();
            if (nome.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(this, "O nome contém caracteres inválidos.", this.TitAplicativo);
                return;
            }

            string pastaPai = PastaDaSubtarefa(caminhoPai);
            string novaPasta = Path.Combine(pastaPai, nome);
            if (Directory.Exists(novaPasta))
            {
                MessageBox.Show(this, "Já existe uma sub-tarefa com este nome.", this.TitAplicativo);
                return;
            }

            Directory.CreateDirectory(novaPasta);
            List<string> novoCaminho = new List<string>(caminhoPai);
            novoCaminho.Add(nome);

            _caminhoSubtarefas.Clear();
            _caminhoSubtarefas.AddRange(novoCaminho);
            cIni.WriteString(this.Atual, "CaminhoAtual", CaminhoSubtarefasAtual());
            AtualizarEstadoCaminhoSubtarefas();
            CarregarHierarquiaSubtarefas();
            AbrirAnotacaoDaHierarquiaAtual(true);
        }

        private void RenomearSubatividade(List<string> caminhoAtual)
        {
            if (caminhoAtual == null)
                return;

            RenomearSubatividadePorNivel(caminhoAtual.Count - 1);
        }

        private void RenomearSubatividadePorNivel(int nivel)
        {
            if (nivel < 0 || nivel >= _caminhoSubtarefas.Count)
                return;

            List<string> caminhoAtualCompleto = new List<string>(_caminhoSubtarefas);
            string nomeAntigo = caminhoAtualCompleto[nivel];
            SubAtividade dialogo = new SubAtividade(nomeAntigo, true);
            dialogo.SetNomeSubAtividade(nomeAntigo);
            dialogo.ShowDialog();
            if (dialogo.DialogResult != DialogResult.OK)
                return;

            string nomeNovo = dialogo.Nome();
            if (nomeNovo == nomeAntigo)
                return;
            if (nomeNovo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show(this, "O nome contém caracteres inválidos.", this.TitAplicativo);
                return;
            }

            List<string> caminhoPai = caminhoAtualCompleto.Take(nivel).ToList();
            List<string> caminhoAntigoRenomeado = new List<string>(caminhoPai);
            caminhoAntigoRenomeado.Add(nomeAntigo);
            string pastaAtual = PastaDaSubtarefa(caminhoAntigoRenomeado);

            List<string> caminhoNovoRenomeado = new List<string>(caminhoPai);
            caminhoNovoRenomeado.Add(nomeNovo);
            string pastaNova = PastaDaSubtarefa(caminhoNovoRenomeado);

            if (Directory.Exists(pastaNova))
            {
                MessageBox.Show(this, "Já existe uma sub-tarefa com este nome.", this.TitAplicativo);
                return;
            }
            if (!Directory.Exists(pastaAtual))
            {
                MessageBox.Show(this, "A pasta original não foi encontrada.", this.TitAplicativo);
                return;
            }

            string prefixoAntigo = NomeBaseAnotacao(caminhoAtualCompleto);
            Directory.Move(pastaAtual, pastaNova);

            caminhoAtualCompleto[nivel] = nomeNovo;
            string prefixoNovo = NomeBaseAnotacao(caminhoAtualCompleto);
            RenomearArquivosDaHierarquia(pastaNova, prefixoAntigo, prefixoNovo);
            cIni.WriteString(this.Atual, "CaminhoAtual", CaminhoSubtarefasAtual(caminhoAtualCompleto));
            _caminhoSubtarefas.Clear();
            _caminhoSubtarefas.AddRange(caminhoAtualCompleto);
            AtualizarEstadoCaminhoSubtarefas();
            CarregarHierarquiaSubtarefas();
            AbrirAnotacaoDaHierarquiaAtual(true);
        }

        private void ApagarSubatividade(List<string> caminhoAtual)
        {
            if (caminhoAtual.Count == 0)
                return;

            string nome = caminhoAtual[caminhoAtual.Count - 1];
            DialogResult resposta = MessageBox.Show(
                this,
                "Tem certeza que deseja excluir a sub-tarefa '" + nome + "' e todo o conteúdo abaixo dela?",
                this.TitAplicativo,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (resposta != DialogResult.Yes)
                return;

            List<string> caminhoPai = caminhoAtual.Take(caminhoAtual.Count - 1).ToList();
            string pasta = PastaDaSubtarefa(caminhoAtual);
            Directory.Delete(pasta, true);

            _caminhoSubtarefas.Clear();
            _caminhoSubtarefas.AddRange(caminhoPai);
            cIni.WriteString(this.Atual, "CaminhoAtual", CaminhoSubtarefasAtual());
            CarregarHierarquiaSubtarefas();
            AbrirAnotacaoDaHierarquiaAtual(false);
        }

        private void AbrirAnotacaoDaHierarquiaAtual(bool nova)
        {
            this.timer1.Enabled = false;
            string data = Fun.Agora().ToString("dd-MM-yyyy");

            if (nova)
            {
                string arquivoNovo = NomeDoArquivo(data, true);
                string pasta = Path.GetDirectoryName(arquivoNovo);
                if (!Directory.Exists(pasta))
                    Directory.CreateDirectory(pasta);
                if (!File.Exists(arquivoNovo))
                    File.WriteAllText(arquivoNovo, "", Encoding.UTF8);

                this.Filename = arquivoNovo;
                DefinirTextoEditorProgramaticamente("");
                this.IsDirty = false;
                ResetUndoDaAtividadeAtual();
                controlContentTextBox.BackColor = SystemColors.Window;
            }
            else
            {
                this.Filename = NomeDoArquivo(data);
                this.Open(this.Filename);
            }

            this.Text = this.TitAplicativo + " " + Path.GetFileName(this.Filename);
            this.PreencheComboArquivo(PastaDaSubtarefaAtual());
            this.cbArquivosOld = this.cbArquivos.Text;
        }

        private bool TemArqHoje(string Pasta, ref string DtHoje)
        {
            bool OK = false;
            DirectoryInfo info = new DirectoryInfo(Pasta);
            FileInfo[] arquivos = info.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();
            foreach (FileInfo arquivo in arquivos)
            {
                string nome = arquivo.Name;
                DateTime DtCriacao = this.GetDataPeloNome(nome);
                string data = DtCriacao.ToShortDateString();
                if (DtHoje == data)
                    OK = true;
                break;
            }
            return OK;
        }

        private void CarregaArquivoDoProjeto(bool MarcarCarregado)
        {
            this.Loga("[v2.2] CarregaArquivoDoProjeto");
            this.HojeVazio = false;
            controlContentTextBox.Clear();
            CarregarCaminhoSubtarefasPersistido();
            string Data = Fun.Agora().ToShortDateString().Replace(@"/", "-");
            this.Filename = NomeDoArquivo(Data);
            this.Loga("Abrindo arquivo: " + this.Filename);
            this.Open(this.Filename);
            this.Text = this.TitAplicativo + " " + Path.GetFileName(this.Filename);
            this.Loga("CarregaArquivoDoProjeto finalizado - Carregado=" + this.Carregado);
        }

        private string NormalizarSubprojeto(string subprojeto)
        {
            return string.IsNullOrWhiteSpace(subprojeto) || subprojeto == "GERAL"
                ? ""
                : subprojeto;
        }

        private string NormalizarDataNavegacao(string data)
        {
            if (string.IsNullOrWhiteSpace(data) || data == "TODAS")
                return Fun.Agora().ToShortDateString();

            return data;
        }

        private NavigationEntry ObterNavegacaoAtual()
        {
            if (string.IsNullOrWhiteSpace(this.Atual))
                return null;

            return new NavigationEntry
            {
                Projeto = this.Atual,
                Subprojeto = NormalizarSubprojeto(this.SUbAtual),
                Data = NormalizarDataNavegacao(this.cbArquivos.Text)
            };
        }

        private bool MesmaNavegacao(NavigationEntry esquerda, NavigationEntry direita)
        {
            if (esquerda == null || direita == null)
                return false;

            return string.Equals(esquerda.Projeto, direita.Projeto, StringComparison.OrdinalIgnoreCase)
                && string.Equals(NormalizarSubprojeto(esquerda.Subprojeto), NormalizarSubprojeto(direita.Subprojeto), StringComparison.OrdinalIgnoreCase)
                && string.Equals(NormalizarDataNavegacao(esquerda.Data), NormalizarDataNavegacao(direita.Data), StringComparison.OrdinalIgnoreCase);
        }

        private void RegistrarNavegacaoAtual()
        {
            if (_suppressNavigationHistory || _isApplyingNavigationHistory)
            {
                AtualizarBotoesHistorico();
                return;
            }

            NavigationEntry atual = ObterNavegacaoAtual();
            if (atual == null)
            {
                AtualizarBotoesHistorico();
                return;
            }

            if (_navigationHistoryIndex >= 0 && MesmaNavegacao(_navigationHistory[_navigationHistoryIndex], atual))
            {
                AtualizarBotoesHistorico();
                return;
            }

            if (_navigationHistoryIndex < _navigationHistory.Count - 1)
            {
                _navigationHistory.RemoveRange(
                    _navigationHistoryIndex + 1,
                    _navigationHistory.Count - (_navigationHistoryIndex + 1));
            }

            _navigationHistory.Add(atual);
            _navigationHistoryIndex = _navigationHistory.Count - 1;
            AtualizarBotoesHistorico();
        }

        private void AtualizarBotoesHistorico()
        {
            this.btnHistoricoVoltar.Enabled = _navigationHistoryIndex > 0;
            this.btnHistoricoAvancar.Enabled = _navigationHistoryIndex >= 0 && _navigationHistoryIndex < _navigationHistory.Count - 1;
        }

        private void NavegarPeloHistorico(int deslocamento)
        {
            int novoIndice = _navigationHistoryIndex + deslocamento;
            if (novoIndice < 0 || novoIndice >= _navigationHistory.Count)
                return;

            NavigationEntry destino = _navigationHistory[novoIndice];
            _isApplyingNavigationHistory = true;
            _suppressNavigationHistory = true;

            try
            {
                AplicarNavegacao(destino);
                _navigationHistoryIndex = novoIndice;
            }
            finally
            {
                _suppressNavigationHistory = false;
                _isApplyingNavigationHistory = false;
                AtualizarBotoesHistorico();
            }
        }

        private void AplicarNavegacao(NavigationEntry destino)
        {
            if (destino == null || string.IsNullOrWhiteSpace(destino.Projeto))
                return;

            if (this.Carregado && this.IsDirty)
                this.Save();

            this.timer1.Enabled = false;

            AplicarSelecaoProjeto(destino.Projeto);

            string subprojetoDestino = NormalizarSubprojeto(destino.Subprojeto);
            string dataDestino = NormalizarDataNavegacao(destino.Data);

            if (!string.Equals(NormalizarSubprojeto(this.SUbAtual), subprojetoDestino, StringComparison.OrdinalIgnoreCase))
                AplicarSelecaoSubprojeto(subprojetoDestino);

            if (!string.Equals(NormalizarDataNavegacao(this.cbArquivos.Text), dataDestino, StringComparison.OrdinalIgnoreCase))
                AplicarSelecaoData(dataDestino);
        }

        private void AplicarSelecaoProjeto(string projeto)
        {
            if (string.IsNullOrWhiteSpace(projeto))
                return;

            this.Atual = projeto;
            this.cbProjetos.Text = projeto;
            cIni.WriteString("Projetos", "Atual", projeto);
            this.CarregaArquivoDoProjeto(true);
            this.MostraArquivosDoProjeto();
            this.AtualAnt = this.Atual;
            this.VeSeTemSub(projeto);
        }

        private void AplicarSelecaoSubprojeto(string subprojeto)
        {
            string caminhoDestino = NormalizarSubprojeto(subprojeto);
            if (string.Equals(CaminhoSubtarefasAtual(), caminhoDestino, StringComparison.OrdinalIgnoreCase))
                return;

            if (this.Carregado && this.IsDirty)
            {
                this.Loga("Salvando antes de trocar de subatividade");
                this.Save();
            }

            cIni.WriteString(this.Atual, "CaminhoAtual", caminhoDestino);
            CarregarHierarquiaSubtarefas();
            AbrirAnotacaoDaHierarquiaAtual(false);
            this.cbArquivosSUbOld = CaminhoSubtarefasAtual();
        }

        private void AplicarSelecaoData(string dataSelecionada)
        {
            if (string.IsNullOrWhiteSpace(dataSelecionada) || dataSelecionada == "TODAS")
                return;

            this.cbArquivos.Text = dataSelecionada;
            AtuArqASerMostrado();
        }

        private void cbProjetos_DropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("cbProjetos_DropDownClosed");
            if (string.Equals(this.Atual, cbProjetos.Text, StringComparison.OrdinalIgnoreCase))
                return;

            _suppressNavigationHistory = true;
            try
            {
                AplicarSelecaoProjeto(cbProjetos.Text);
            }
            finally
            {
                _suppressNavigationHistory = false;
            }

            RegistrarNavegacaoAtual();
        }

        private void MostraArquivosDoProjeto()
        {
            int QtdSub = this.cIni.ReadInt(this.Atual, "QtdSub", 0);
            if (QtdSub > 0 || TemSubpastas(PastaDoProjetoAtual()))
            {
                this.MotraArqSub(QtdSub);
            }
            else
            {
                this.SUbAtual = "";
                this.cbArquivosSUbOld = "";
                this.cbSubprojeto.Visible = false;
                this.cbSubprojeto.Items.Clear();
                this.cbSubprojeto.Text = "";
                RemoverCombosSubtarefasDepoisDoNivel(0);
                _caminhoSubtarefas.Clear();
                renomearToolStripMenuItem.Enabled = false;
                AtualizarMenuSubAtividades();
            }
            apagarToolStripMenuItem.Enabled = renomearToolStripMenuItem.Enabled;
            this.PreencheComboArquivo(PastaDaSubtarefaAtual());
            this.cbArquivosOld = this.cbArquivos.Text;
        }

        private DateTime GetDataPeloNome(string nomeArquivo)
        {
            //this.Loga("[v2.2] GetDataPeloNome: " + nomeArquivo);

            try
            {
                // Extrair data do formato ^DD-MM-AAAA.txt
                System.Text.RegularExpressions.Match match =
                    System.Text.RegularExpressions.Regex.Match(nomeArquivo, @"\d{2}-\d{2}-\d{4}");

                if (match.Success)
                {
                    DateTime data = DateTime.ParseExact(match.Value, "dd-MM-yyyy", null);
                    //this.Loga("Data extraída: " + data.ToShortDateString());
                    return data;
                }
                else
                {
                    //this.Loga("⚠️ Data não encontrada no nome, usando data mínima");
                    return DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                this.Loga("❌ Erro ao extrair data: " + ex.Message);
                return DateTime.MinValue;
            }
        }

        private void PreencheComboArquivo(string pasta, bool mostrarTodas = false)
        {
            PreparaComboArquivo(pasta, mostrarTodas);
        }

        private void PreparaComboArquivo(string Pasta, bool mostrarTodas = false)
        {
            this.Loga("[v2.15] PreparaComboArquivo");

            bool adicionarTodas = false;

            if (this.mostrarSóDoDiaToolStripMenuItem.Checked)
                return;

            int LimArqs = cIni.ReadInt("Projetos", "LimArqs", 31);
            List<DateTime> ArqsAdds = new List<DateTime>();

            try
            {
                DirectoryInfo info = new DirectoryInfo(Pasta);
                if (!info.Exists)
                {
                    Directory.CreateDirectory(Pasta);
                    this.Loga("Pasta criada: " + Pasta);
                }

                string prefixoHistorico = NomeBaseAnotacaoAtual() + "^";

                FileInfo[] arquivos = info.GetFiles("*.txt")
                    .OrderBy(p => p.CreationTime)
                    .ToArray();

                foreach (FileInfo arquivo in arquivos)
                {
                    string nome = arquivo.Name;

                    // Só aceita arquivo histórico real:
                    // Projeto^Sub^DD-MM-AAAA.txt
                    // ou Projeto^DD-MM-AAAA.txt no GERAL
                    if (!nome.StartsWith(prefixoHistorico, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!Regex.IsMatch(nome, @"\^\d{2}-\d{2}-\d{4}\.txt$", RegexOptions.IgnoreCase))
                        continue;

                    if (arquivo.Length <= 3)
                    {
                        this.Loga("Ignorando histórico vazio/só BOM: " + arquivo.FullName);
                        continue;
                    }

                    DateTime data = this.GetDataPeloNome(nome);

                    if (data == DateTime.MinValue)
                        continue;

                    if (!ArqsAdds.Contains(data))
                        ArqsAdds.Add(data);
                }
            }
            catch (Exception ex)
            {
                this.Loga("Erro em PreparaComboArquivo: " + ex.Message);
            }

            cbArquivos.Visible = true;
            cbArquivos.Items.Clear();

            ArqsAdds.Sort();

            int qtdArqs = ArqsAdds.Count;
            int ini = qtdArqs - LimArqs;

            if (mostrarTodas || ini < 0)
                ini = 0;
            else
                adicionarTodas = true;

            for (int i = ini; i < qtdArqs; i++)
                cbArquivos.Items.Add(ArqsAdds[i].ToShortDateString());

            string dataHoje = Fun.Agora().ToShortDateString();
            string dataSelecionada = DataParaComboArquivoAtual(dataHoje);

            if (!cbArquivos.Items.Contains(dataSelecionada))
                cbArquivos.Items.Add(dataSelecionada);

            if (ArquivoFoiModificadoHoje(this.Filename) && !cbArquivos.Items.Contains(dataHoje))
                cbArquivos.Items.Add(dataHoje);

            cbArquivos.Text = dataSelecionada;

            if (adicionarTodas)
                cbArquivos.Items.Add("TODAS");
        }

        private string DataParaComboArquivoAtual(string dataPadrao)
        {
            if (!string.IsNullOrWhiteSpace(this._arquivoOrigemConteudoHistorico) && File.Exists(this._arquivoOrigemConteudoHistorico))
            {
                DateTime dataOrigem = GetDataPeloNome(Path.GetFileName(this._arquivoOrigemConteudoHistorico));
                if (dataOrigem != DateTime.MinValue)
                    return dataOrigem.ToShortDateString();

                return File.GetLastWriteTime(this._arquivoOrigemConteudoHistorico).ToShortDateString();
            }

            if (!string.IsNullOrWhiteSpace(this.Filename) && File.Exists(this.Filename) && !ArquivoFoiModificadoHoje(this.Filename))
                return File.GetLastWriteTime(this.Filename).ToShortDateString();

            return dataPadrao;
        }

        private void SelecionarHojeNoComboArquivo()
        {
            string dataHoje = Fun.Agora().ToShortDateString();

            if (!cbArquivos.Items.Contains(dataHoje))
                cbArquivos.Items.Add(dataHoje);

            cbArquivos.Text = dataHoje;
            cbArquivosOld = dataHoje;
            RegistrarNavegacaoAtual();
        }

        private string NomeDoArquivo(string Data, bool forcarDataEspecifica = false)
        {
            string Pasta = PastaDaSubtarefaAtual();
            string nomeBase = NomeBaseAnotacaoAtual();
            string workingCopy = Path.Combine(Pasta, nomeBase + ".txt");
            string workingCopyAntigo = Path.Combine(Pasta, nomeBase + "^current.txt");

            if (string.Equals(Data, "current", StringComparison.OrdinalIgnoreCase))
            {
                MigrarWorkingCopyAntigoSeNecessario(workingCopyAntigo, workingCopy);
                return workingCopy;
            }

            // ✅ Se forçar data específica, abrir EXATAMENTE o arquivo da data selecionada
            if (forcarDataEspecifica)
            {
                string sDataX = Data.Replace(@"/", "-");
                string arquivoData = Path.Combine(Pasta, nomeBase + "^" + sDataX + ".txt");
                this.Loga($"Forçando abertura do arquivo da data {Data}: {arquivoData}");
                return arquivoData;
            }

            // ✅ PRIORIDADE 1: Working copy SEM data (ex: Empregos^Cristian.txt)
            if (File.Exists(workingCopy))
            {
                this.Loga("Working copy encontrado: " + workingCopy);
                return workingCopy;
            }

            // ✅ PRIORIDADE 2: Working copy ANTIGO (^current.txt) - migração
            if (File.Exists(workingCopyAntigo))
            {
                this.Loga("Working copy antigo encontrado (^current.txt): " + workingCopyAntigo);
                MigrarWorkingCopyAntigoSeNecessario(workingCopyAntigo, workingCopy);
                return File.Exists(workingCopy) ? workingCopy : workingCopyAntigo;
            }

            // ✅ PRIORIDADE 3: Arquivo do dia atual (ex: Empregos^Cristian^17-03-2026.txt)
            string sData = Data.Replace(@"/", "-");
            string todayFile = Path.Combine(Pasta, nomeBase + "^" + sData + ".txt");

            this.Loga("Working copy não encontrado, usando arquivo do dia: " + todayFile);
            return todayFile;
        }

        private void MigrarWorkingCopyAntigoSeNecessario(string antigo, string correto)
        {
            if (string.IsNullOrWhiteSpace(antigo) || string.IsNullOrWhiteSpace(correto) || !File.Exists(antigo))
                return;

            try
            {
                string pastaCorreta = Path.GetDirectoryName(correto);
                if (!Directory.Exists(pastaCorreta))
                    Directory.CreateDirectory(pastaCorreta);

                if (!File.Exists(correto) || new FileInfo(correto).Length <= 3)
                {
                    File.Copy(antigo, correto, true);
                    this.Loga($"✅ Working copy antigo migrado para formato correto: {correto}");
                    return;
                }

                string antigoConteudo = SanitizeControlCharacters(File.ReadAllText(antigo, Encoding.UTF8));
                string corretoConteudo = SanitizeControlCharacters(File.ReadAllText(correto, Encoding.UTF8));
                if (!string.Equals(antigoConteudo, corretoConteudo, StringComparison.Ordinal))
                {
                    string dataAntigo = File.GetLastWriteTime(antigo).ToString("dd-MM-yyyy");
                    string snapshotAntigo = NomeDoArquivo(dataAntigo, true);
                    CriarSnapshotSeNecessario(antigo, snapshotAntigo, "migração de ^current.txt");
                }
            }
            catch (Exception ex)
            {
                this.Loga($"❌ Erro ao migrar working copy antigo: {ex.Message}");
            }
        }

        private void AtuArqASerMostrado()
        {
            this.Loga("[v2.14] AtuArqASerMostrado");
            this.Loga($"Carregado = {this.Carregado}, cbArquivos.Text = '{cbArquivos.Text}'");

            if (!this.Carregado || string.IsNullOrEmpty(cbArquivos.Text))
                return;

            if (cbArquivos.Text == "TODAS")
            {
                string dataSelecionada = string.IsNullOrEmpty(cbArquivosOld) ? Fun.Agora().ToShortDateString() : cbArquivosOld;
                string pastaAtual = this.PastaGeral + @"\" + this.Atual +
                    (string.IsNullOrEmpty(this.SUbAtual) ? "" : @"\" + this.SUbAtual);

                this.Loga("Recarregando combo de datas em modo TODAS");
                this.PreencheComboArquivo(pastaAtual, true);

                int posicaoData = cbArquivos.Items.IndexOf(dataSelecionada);
                if (posicaoData > -1)
                    cbArquivos.SelectedIndex = posicaoData;
                else if (cbArquivos.Items.Count > 0)
                    cbArquivos.SelectedIndex = cbArquivos.Items.Count - 1;

                this.cbArquivosOld = cbArquivos.Text;
                return;
            }

            bool mudouData = cbArquivos.Text != this.cbArquivosOld;

            if (!mudouData)
            {
                Loga("⛔ Mesma data - ignorando");
                return;
            }

            // 🔥 CORREÇÃO CRÍTICA
            //if (EhWorkingCopyAtual())
            //{
            //    this.Loga("⛔ Working copy ativo - NÃO carregar histórico");
            //    return;
            //}

            if (cbArquivos.Text != this.cbArquivosOld)
            {
                ProcessarSelecaoDeDataHistorica();
            }

            if (Atual != this.AtualAnt)
            {
                this.AtualAnt = this.Atual;
                VeSeTemSub(Atual);
            }

            RegistrarNavegacaoAtual();
        }

        private bool EhWorkingCopyAtual()
        {
            // Se o arquivo atual NÃO tem data no nome → é working copy
            return !this.Filename.Contains("^" + DateTime.Now.ToString("dd-MM-yyyy"));
        }

        private void ProcessarSelecaoDeDataHistorica()
        {
            // Salvar alterações antes de sair do working copy
            if (this.IsDirty)
            {
                this.Loga("⚠️ Salvando working copy atual antes de visualizar histórico");
                this.Save();
            }

            this.timer1.Enabled = false;

            string dataSelecionadaStr = cbArquivos.Text.Replace("/", "-").Replace(".", "-");
            string hojeStr = Fun.Agora().ToString("dd-MM-yyyy");

            string pasta = PastaDaSubtarefaAtual();
            string nomeBase = NomeBaseAnotacaoAtual();
            string arquivoDataSelecionada = Path.Combine(pasta, nomeBase + "^" + dataSelecionadaStr + ".txt");
            string workingCopy = Path.Combine(pasta, nomeBase + ".txt");

            this.Loga($"🔍 Tentando abrir arquivo EXATO da data {dataSelecionadaStr}: {arquivoDataSelecionada}");

            // 1) Se existe o arquivo EXATO da data, abre ele
            if (File.Exists(arquivoDataSelecionada) && new FileInfo(arquivoDataSelecionada).Length > 3)
            {
                this.OpenHistoricalFileOnly(arquivoDataSelecionada);
                this.cbArquivosOld = cbArquivos.Text;
                this.Loga($"🎨 Conteúdo histórico carregado com fundo AZUL");
                return;
            }

            this.Loga($"ℹ️ Arquivo EXATO não encontrado/vazio: {arquivoDataSelecionada}");

            // 2) Se a data selecionada é HOJE, manter/abrir o working copy
            if (dataSelecionadaStr == hojeStr)
            {
                if (File.Exists(workingCopy) && new FileInfo(workingCopy).Length > 0)
                {
                    this.Loga($"ℹ️ Data de hoje sem snapshot. Mantendo working copy: {workingCopy}");
                    this.Open(workingCopy);
                    controlContentTextBox.BackColor = Color.White;
                    this.cbArquivosOld = cbArquivos.Text;
                    return;
                }

                this.Loga($"⚠️ Working copy não encontrado para hoje: {workingCopy}");
                return;
            }

            // 3) Para datas antigas: buscar histórico anterior mais próximo com conteúdo
            DateTime dataSelecionada;

            if (!DateTime.TryParse(cbArquivos.Text, out dataSelecionada))
            {
                this.Loga($"⚠️ Data inválida no combo: {cbArquivos.Text}");
                cbArquivos.Text = cbArquivosOld;
                return;
            }

            string prefixoHistorico = nomeBase + "^";

            this.Loga($"🔎 Buscando histórico anterior mais próximo de {cbArquivos.Text}");

            string historicoAnterior = EncontrarHistoricoAnteriorComConteudo(
                pasta,
                prefixoHistorico,
                dataSelecionada);

            if (!string.IsNullOrEmpty(historicoAnterior))
            {
                this.Loga($"✅ Histórico anterior encontrado: {historicoAnterior}");

                this.OpenHistoricalFileOnly(historicoAnterior);

                DateTime dataReal = GetDataPeloNome(Path.GetFileName(historicoAnterior));
                string textoDataReal = dataReal.ToShortDateString();

                this.cbArquivosOld = textoDataReal;
                this.cbArquivos.Text = textoDataReal;

                this.Loga($"🎨 Conteúdo histórico anterior carregado com fundo AZUL: {textoDataReal}");
                return;
            }

            this.Loga($"⚠️ Nenhum histórico anterior com conteúdo encontrado para {cbArquivos.Text}");

            // Sem MessageBox.
            // Apenas volta a seleção anterior.
            cbArquivos.Text = cbArquivosOld;
        }

        /// <summary>
        /// Abre EXATAMENTE o arquivo especificado SEM redirecionamento para working copy
        /// Usado apenas para visualização de datas históricas
        /// </summary>
        private void OpenHistoricalFileOnly(string filePath)
        {
            this.Loga($"[v2.14] OpenHistoricalFileOnly: {filePath}");

            try
            {
                // ✅ Ler conteúdo DIRETAMENTE do arquivo especificado (sem lógica de working copy)
                string content = ReadAllText(filePath, null);
                Content = content;

                // ✅ Configurar estado
                this.Filename = filePath;
                this.IsDirty = false;
                this.Carregado = true;
                ResetUndoDaAtividadeAtual();

                // ✅ FORÇAR fundo AZUL para indicar visualização de histórico
                controlContentTextBox.BackColor = Color.AliceBlue;

                this.MotraCaracteres();
                this.Loga($"✅ Conteúdo histórico carregado ({Content.Length} caracteres)");
            }
            catch (Exception ex)
            {
                this.Loga($"❌ Erro ao abrir arquivo histórico: {ex.Message}");
                MessageBox.Show($"Erro ao abrir arquivo da data selecionada:\n{ex.Message}",
                               "Anoteitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbArquivos_DropDownClosed(object sender, EventArgs e)
        {
            AtuArqASerMostrado();
        }

        private void cbProjetos_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtuArqASerMostrado();
        }

        private void cbProjetos_KeyUp(object sender, KeyEventArgs e)
        {
            Console.WriteLine("cbProjetos_KeyUp");
            if ((e.KeyCode == Keys.Down) || (e.KeyCode == Keys.Up))
            {
                if (string.Equals(this.Atual, cbProjetos.Text, StringComparison.OrdinalIgnoreCase))
                    return;

                _suppressNavigationHistory = true;
                try
                {
                    AplicarSelecaoProjeto(cbProjetos.Text);
                }
                finally
                {
                    _suppressNavigationHistory = false;
                }

                RegistrarNavegacaoAtual();
            }
        }

        private void cbArquivos_KeyUp(object sender, KeyEventArgs e)
        {
            AtuArqASerMostrado();
        }

        private void mostrarSóDoDiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.mostrarSóDoDiaToolStripMenuItem.Checked)
            {
                this.mostrarSóDoDiaToolStripMenuItem.Checked = false;
                cbArquivos.Enabled = true;
                this.PreencheCombo(Atual);
            }
            else
            {
                string DtHoje = Fun.Agora().ToShortDateString();
                cbProjetos.Items.Clear();
                cbSubprojeto.Items.Clear();
                int Qtd = cIni.ReadInt("Projetos", "Qtd", 0);
                int a = 0;
                for (int i = 0; i < Qtd; i++)
                {
                    string nmProjeto = "Pro" + (i + 1).ToString();
                    string NomeAtiv = cIni.ReadString("NmProjetos", nmProjeto, "");
                    if (NomeAtiv.Length > 0)
                    {
                        this.Loga(NomeAtiv);
                        string Pasta = this.PastaGeral + @"\" + NomeAtiv;
                        DirectoryInfo info = new DirectoryInfo(Pasta);
                        if (info.Exists)
                        {
                            FileInfo arquivo = null;
                            try
                            {
                                arquivo = info.GetFiles().OrderByDescending(p => p.CreationTime).First();
                            }
                            catch (Exception)
                            {
                                this.Loga("Pasta Vazia");
                            }
                            if (arquivo != null)
                            {
                                try
                                {
                                    string UltAdic = "";
                                    string nome = arquivo.Name;
                                    this.Loga(nome);
                                    DateTime DtCriacao = this.GetDataPeloNome(nome);
                                    string sCriacao = DtCriacao.ToShortDateString();
                                    if (!this.VeSeTemHoje(DtHoje, sCriacao, NomeAtiv, ref UltAdic, ref a))
                                    {
                                        int QtdSub = this.cIni.ReadInt(NomeAtiv, "QtdSub", 0);
                                        if (QtdSub > 0)
                                        {
                                            for (int j = 0; j < QtdSub; j++)
                                            {
                                                string Sub = "Sub" + (j + 1).ToString();
                                                string NomeSub = cIni.ReadString(NomeAtiv, Sub, "");
                                                this.Loga("    " + NomeSub);
                                                string SubPasta = Pasta + @"\" + NomeSub;
                                                DirectoryInfo infSub = new DirectoryInfo(SubPasta);
                                                try
                                                {
                                                    FileInfo arqSub = infSub.GetFiles().OrderByDescending(p => p.CreationTime).First();
                                                    if (arqSub.Length > 0)
                                                    {
                                                        string nomeArqSub = arqSub.Name;
                                                        DateTime DtCriacaoSub = this.GetDataPeloNome(nomeArqSub);
                                                        string sCriacaoSub = DtCriacaoSub.ToShortDateString();
                                                        this.VeSeTemHoje(DtHoje, sCriacaoSub, NomeAtiv, ref UltAdic, ref a);
                                                    }
                                                }
                                                catch (Exception exception)
                                                {
                                                    this.Loga("Diretório Vazio");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    this.Loga("Diretório sem arquivos mas com diretórios");
                                }
                            }
                        }
                    }
                }
                if (a == 0)
                {
                    this.Loga("Não há arquivos gravados no dia");
                    MessageBox.Show(this, "Não há arquivos gravador no dia", this.TitAplicativo);
                    this.PreencheCombo(Atual);
                }
                else
                {
                    cbArquivos.Items.Clear();
                    cbArquivos.Items.Add(DtHoje);
                    cbArquivos.SelectedIndex = 0;
                    cbArquivos.Enabled = false;
                    this.mostrarSóDoDiaToolStripMenuItem.Checked = true;
                }
            }
        }

        private bool VeSeTemHoje(string DtHoje, string sCriacaoSub, string NomeAtiv, ref string UltAdic, ref int a)
        {
            bool ret = false;
            if (DtHoje == sCriacaoSub)
            {
                UltAdic = NomeAtiv;
                cbProjetos.Items.Add(NomeAtiv);
                if (NomeAtiv == Atual)
                {
                    cbProjetos.SelectedIndex = a;
                    ret = true;
                }
                a++;
            }
            return ret;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            this.QtMinutosEsse++;
            int QtAgora = this.QtMinutos + QtMinutosEsse;
            string Tempo = this.HoraFmt(QtAgora);
            string TmpAgora = this.HoraFmt(QtMinutosEsse);
            lbTempDecorr.Text = Tempo + "  -  " + TmpAgora;
            cIni.WriteInt(Atual, "Tempo", QtAgora);
        }

        private string HoraFmt(int QtAgora)
        {
            int horas = QtAgora / 60;
            int min = QtAgora - (horas * 60);
            return min.ToString("00") + "  :  " + horas.ToString("00");
        }

        private void temposToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tempos fTempos = new Tempos();
            fTempos.Show();
        }

        private void cbSubprojeto_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Loga("[v2.2] cbSubprojeto_SelectedIndexChanged");
            if (_atualizandoCombosSubtarefas)
                return;

            ToolStripComboBox combo = sender as ToolStripComboBox;
            if (combo == null)
                combo = cbSubprojeto;

            int nivel = _combosSubtarefas.IndexOf(combo);
            if (nivel < 0 || combo.Items.Count == 0)
                return;

            this.Loga("Carregado=" + this.Carregado + ", Nivel=" + nivel + ", Text=" + combo.Text);

            if (this.Carregado && this.IsDirty)
                this.Save();

            _nivelHierarquiaSelecionado = combo.Tag is int nivelSelecionado ? nivelSelecionado : -1;
            _atualizandoCombosSubtarefas = true;
            try
            {
                while (_caminhoSubtarefas.Count > nivel)
                    _caminhoSubtarefas.RemoveAt(_caminhoSubtarefas.Count - 1);

                if (!string.IsNullOrWhiteSpace(combo.Text) && combo.Text != "GERAL")
                    _caminhoSubtarefas.Add(combo.Text);

                RemoverCombosSubtarefasDepoisDoNivel(nivel);

                string pastaAtual = PastaDaSubtarefaAtual();
                if (combo.Text != "GERAL" && TemSubpastas(pastaAtual))
                {
                    ToolStripComboBox proximo = CriarComboSubtarefaDinamico();
                    PopularComboSubtarefa(proximo, pastaAtual, false);
                    proximo.SelectedIndex = proximo.FindStringExact("GERAL");
                }

                AtualizarEstadoCaminhoSubtarefas();
            }
            finally
            {
                _atualizandoCombosSubtarefas = false;
            }

            AbrirAnotacaoDaHierarquiaAtual(false);
            RegistrarNavegacaoAtual();
        }

        private void renomearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenomearSubatividadePorNivel(_caminhoSubtarefas.Count - 1);
        }

        private void RenomearArquivosDaHierarquia(string pasta, string prefixoAntigo, string prefixoNovo)
        {
            foreach (string arquivo in Directory.GetFiles(pasta, "*.txt", SearchOption.AllDirectories))
            {
                string nome = Path.GetFileName(arquivo);
                if (!nome.StartsWith(prefixoAntigo, StringComparison.OrdinalIgnoreCase))
                    continue;

                string nomeNovo = prefixoNovo + nome.Substring(prefixoAntigo.Length);
                string destino = Path.Combine(Path.GetDirectoryName(arquivo), nomeNovo);
                if (!File.Exists(destino))
                    File.Move(arquivo, destino);
            }
        }

        private void renomearToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Projeto cProj = new Projeto();
            cProj.SetNomeAtividade(this.Atual);
            cProj.ShowDialog();
            string Texto = controlContentTextBox.Text;
            if (cProj.DialogResult == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                string NomeAtividade = cProj.getNomeAtividade();
                Fun.Renomeia(this.PastaGeral, @"\" + this.Atual, @"\" + NomeAtividade);
                int Nr = Fun.NumePeloNomeAtividade(ref this.cIni, this.Atual);
                if (Nr > -1)
                {
                    string nmProjeto = "Pro" + Nr.ToString();
                    this.cIni.WriteString("NmProjetos", nmProjeto, NomeAtividade);
                }
                PreencheCombo(NomeAtividade);
                this.Filename = this.Filename.Replace(this.Atual, NomeAtividade);
                this.Save();
                cbProjetos.Text = NomeAtividade;
                cIni.WriteString("Projetos", "Atual", NomeAtividade);
                controlContentTextBox.Text = Texto;
                this.Cursor = Cursors.Default;
                this.Atual = NomeAtividade;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Mensagem frmMensagem = new Mensagem();
            frmMensagem.Titulo = this.Atual;
            frmMensagem.Tipo = "Tarefa";
            string PastaAtual = this.PastaGeral + @"\" + this.Atual;
            frmMensagem.PastaAtual = PastaAtual;
            frmMensagem.Atual = this.Atual;
            frmMensagem.PastaGeral = this.PastaGeral;
            frmMensagem.ShowDialog();
            if (frmMensagem.DialogResult == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = "Tarefa " + this.Atual + " foi apagada";
                cbProjetos.SelectedIndex = 0;
                this.Atual = cbProjetos.Text;
                PreencheCombo(this.Atual);
                cIni.WriteString("Projetos", "Atual", cbProjetos.Text);
                this.CarregaArquivoDoProjeto(true);
                this.MostraArquivosDoProjeto();
            }
        }

        #endregion

        private void procurarPorTudoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindInAllFilesRecursive(this.PastaGeral);
        }

        private async void FindInAllFilesRecursive(string baseDirectory)
        {
            // Tentei refatorar pelo Qwen em 12/03/2026 e deu errado
            if (string.IsNullOrWhiteSpace(Content)) return;
            string searchText = ObterTextoBusca();
            if (string.IsNullOrWhiteSpace(searchText)) return;
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            try
            {
                Resultados resultWindow = null;
                await ExecutarBusca(baseDirectory, searchText, token, rw => resultWindow = rw, () => resultWindow);
                Invoke(new Action(() => { this.Text = this.TitAplicativo + " - Busca Concluída"; }));
                if (resultWindow == null)
                {
                    MessageBox.Show("Nenhuma ocorrência encontrada.", "Anoteitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar arquivos: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObterTextoBusca()
        {
            string searchText = controlContentTextBox.SelectedText;
            if (string.IsNullOrEmpty(searchText))
            {
                searchText = ShowInputDialog("Busca Global", "Digite o termo que deseja buscar:");
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return null;
                }
            }
            return searchText;
        }

        private async Task ExecutarBusca(string baseDirectory, string searchText, CancellationToken token, Action<Resultados> setWindow, Func<Resultados> getWindow)
        {
            List<string> allFiles = Directory.GetFiles(baseDirectory, "*.*", SearchOption.AllDirectories).ToList();
            int totalFiles = allFiles.Count;
            int processedFiles = 0;
            await Task.Run(() =>
            {
                foreach (var file in allFiles)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    processedFiles++;
                    UpdateProgress(processedFiles, totalFiles);
                    ProcessarArquivo(file, searchText, setWindow, getWindow);
                }
            }, token);
        }

        private void ProcessarArquivo(string file, string searchText, Action<Resultados> setWindow, Func<Resultados> getWindow)
        {
            try
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    string line;
                    int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (line.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            TratarOcorrencia(file, line, setWindow, getWindow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show($"Erro ao ler o arquivo {file}: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private void TratarOcorrencia(string file, string line, Action<Resultados> setWindow, Func<Resultados> getWindow)
        {
            var window = getWindow();
            if (window == null)
            {
                Invoke(new Action(() =>
                {
                    var nova = new Resultados(this, new List<(string, string)> { (file, line) }, _cts);
                    nova.StartPosition = FormStartPosition.CenterScreen;
                    nova.Show();
                    setWindow(nova);
                }));
            }
            else
            {
                Invoke(new Action(() =>
                {
                    window.AdicionarResultado(file, line);
                }));
            }
        }

        // Atualiza o título do programa com o percentual da busca
        private void UpdateProgress(int processed, int total)
        {
            if (total > 0)
            {
                int percent = (processed * 100) / total;
                Invoke(new Action(() => { this.Text = $"{this.TitAplicativo} - Buscando... {percent}%"; }));
            }
        }

        private void procurarEmTodasDatasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string PastaSubAtual = "";
            if (this.SUbAtual != "")
            {
                PastaSubAtual = @"\" + this.SUbAtual;
            }
            string PastaSub = this.PastaGeral + @"\" + this.Atual + PastaSubAtual;
            FindInAllFiles(PastaSub);
        }

        private void FindInAllFiles(string PastaSub)
        {
            string searchText = controlContentTextBox.SelectedText;
            if (string.IsNullOrEmpty(searchText))
            {
                searchText = ShowInputDialog("Busca em Todos os Arquivos", "Digite o termo que deseja buscar:");
                if (string.IsNullOrWhiteSpace(searchText)) return;
            }
            string taskName = this.Atual; // Nome da tarefa atual
            List<string> matchingFiles = Directory.GetFiles(PastaSub, $"{taskName}*")
                .OrderBy(f => new FileInfo(f).CreationTime)
                .ToList();
            List<(string filePath, string displayText)> foundOccurrences = new List<(string, string)>();
            foreach (var file in matchingFiles)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(file))
                    {
                        string line;
                        int lineNumber = 0;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lineNumber++;
                            if (line.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                string fileName = Path.GetFileName(file);
                                string datePart = Helper.ExtractDateFromFileName(fileName);
                                foundOccurrences.Add((file, line));
                                // foundOccurrences.Add($"{datePart} : {line}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ler o arquivo {file}: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (foundOccurrences.Count > 0)
            {
                Resultados resultWindow = new Resultados(this, foundOccurrences, _cts);
                resultWindow.StartPosition = FormStartPosition.CenterScreen;
                resultWindow.Show();
            }
            else
            {
                MessageBox.Show("Nenhuma ocorrência encontrada.", "Anoteitor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string ShowInputDialog(string title, string promptText)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = "";

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancelar";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            return dialogResult == DialogResult.OK ? textBox.Text : "";
        }

        //private void SafeDisableTimer()
        //{
        //    if (this.timer1.Enabled)
        //    {
        //        this.timer1.Enabled = false;
        //        // Forçar salvamento imediato se houver alterações
        //        if (this.IsDirty) this.Save();
        //    }
        //}

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.NomeLog) || !File.Exists(this.NomeLog))
            {
                MessageBox.Show("Arquivo de log não encontrado.\nVerifique se o log está habilitado nas configurações.",
                               this.TitAplicativo, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                System.Diagnostics.Process.Start("notepad.exe", this.NomeLog);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o log:\n{ex.Message}",
                               this.TitAplicativo, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFonteMenos_Click(object sender, EventArgs e)
        {
            AlterarTamanhoFonte(-1f);
        }

        private void AlterarTamanhoFonte(float delta)
        {
            try
            {
                Font fonteAtual = this.controlContentTextBox.Font;

                float novoTamanho = fonteAtual.Size + delta;

                // Limites de segurança
                if (novoTamanho < 6f)
                    novoTamanho = 6f;

                if (novoTamanho > 72f)
                    novoTamanho = 72f;

                Font novaFonte = new Font(
                    fonteAtual.FontFamily,
                    novoTamanho,
                    fonteAtual.Style,
                    fonteAtual.Unit);

                // Aplica no editor
                this.controlContentTextBox.Font = novaFonte;

                // Mantém a configuração global do programa
                this.CurrentFont = novaFonte;

                this.Loga($"Fonte alterada para {novoTamanho}");
            }
            catch (Exception ex)
            {
                this.Loga("Erro ao alterar fonte: " + ex.Message);
                MessageBox.Show(
                    "Erro ao alterar o tamanho da fonte.\n\n" + ex.Message,
                    this.TitAplicativo,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AlterarTamanhoFonteCombos(float delta)
        {
            try
            {
                Font fonteAtual = this.CurrentComboFont;
                float novoTamanho = fonteAtual.Size + delta;

                if (novoTamanho < ComboFontMinSize)
                    novoTamanho = ComboFontMinSize;

                if (novoTamanho > ComboFontMaxSize)
                    novoTamanho = ComboFontMaxSize;

                Font novaFonte = new Font(
                    fonteAtual.FontFamily,
                    novoTamanho,
                    fonteAtual.Style,
                    fonteAtual.Unit);

                this.CurrentComboFont = novaFonte;
                this.Loga($"Fonte dos combos alterada para {novoTamanho}");
            }
            catch (Exception ex)
            {
                this.Loga("Erro ao alterar fonte dos combos: " + ex.Message);
                MessageBox.Show(
                    "Erro ao alterar o tamanho da fonte dos combos.\n\n" + ex.Message,
                    this.TitAplicativo,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnFonteMais_Click(object sender, EventArgs e)
        {
            AlterarTamanhoFonte(1f);
        }

        private void btnHistoricoVoltar_Click(object sender, EventArgs e)
        {
            NavegarPeloHistorico(-1);
        }

        private void btnHistoricoAvancar_Click(object sender, EventArgs e)
        {
            NavegarPeloHistorico(1);
        }

        private string EncontrarHistoricoAnteriorComConteudo(string pasta, string prefixoHistorico, DateTime dataSelecionada)
        {
            try
            {
                DirectoryInfo info = new DirectoryInfo(pasta);

                if (!info.Exists)
                    return null;

                FileInfo[] arquivos = info.GetFiles("*.txt")
                    .Where(f =>
                        f.Name.StartsWith(prefixoHistorico, StringComparison.OrdinalIgnoreCase) &&
                        Regex.IsMatch(f.Name, @"\^\d{2}-\d{2}-\d{4}\.txt$", RegexOptions.IgnoreCase) &&
                        f.Length > 3)
                    .OrderByDescending(f => GetDataPeloNome(f.Name))
                    .ToArray();

                foreach (FileInfo arquivo in arquivos)
                {
                    DateTime dataArquivo = GetDataPeloNome(arquivo.Name);

                    if (dataArquivo == DateTime.MinValue)
                        continue;

                    if (dataArquivo.Date >= dataSelecionada.Date)
                        continue;

                    string conteudo = ReadAllText(arquivo.FullName, null);

                    if (!string.IsNullOrWhiteSpace(conteudo))
                        return arquivo.FullName;
                }
            }
            catch (Exception ex)
            {
                this.Loga("Erro em EncontrarHistoricoAnteriorComConteudo: " + ex.Message);
            }

            return null;
        }


    }

    partial class cEscolhido
    {
        public string Nome = "";
        public bool usado = false;
    }

}
