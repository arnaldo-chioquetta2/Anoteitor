using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
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

        public Main()
        {
            InitializeComponent();
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
                CurrentFont = Settings.CurrentFont;
                this.controlContentTextBox.Font = CurrentFont;
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

        private void menuitemEditUndo_Click(object sender, EventArgs e)
        {
            controlContentTextBox.Undo();
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
            if (SelectionLength == 0)
                SelectionLength = 1;
            SelectedText = "";
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

        private void novaSubAtividadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SubAtividade cSubAtiv = new SubAtividade(Atual);
            cSubAtiv.ShowDialog();
            if (cSubAtiv.DialogResult == DialogResult.OK)
            {
                string Nome = cSubAtiv.Nome();
                string sData = Fun.Agora().ToShortDateString();
                string Data = sData.Replace(@"/", "-");
                this.NomeArq = this.Atual + "^" + Nome + "^" + Data + ".txt";
                this.Text = this.NomeArq + " - " + this.TitAplicativo;
                toolStripStatusLabel1.Text = this.NomeArq;
                this.SUbAtual = Nome;
                int QtdSub = cSubAtiv.getQtdSub();
                this.MotraArqSub(QtdSub);
                controlContentTextBox.BackColor = SystemColors.Window;
            }
        }

        private void apagarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mensagem frmMensagem = new Mensagem();
            frmMensagem.Titulo = this.SUbAtual;
            frmMensagem.Tipo = "Sub Tarefa";
            string PastaAtual = this.PastaGeral + @"\" + this.Atual;
            frmMensagem.PastaAtual = PastaAtual;
            frmMensagem.Atual = this.Atual;
            int QtdSub = cIni.ReadInt(this.Atual, "QtdSub", 0);
            frmMensagem.QtdSub = QtdSub;
            frmMensagem.ShowDialog();
            if (frmMensagem.DialogResult == DialogResult.OK)
            {
                cbSubprojeto.SelectedIndex = 0;
                this.SUbAtual = "GERAL";
                if ((QtdSub - 1) > 0)
                {
                    MotraArqSub(QtdSub);
                }
                else
                {
                    cbSubprojeto.Visible = false;
                    apagarToolStripMenuItem.Enabled = false;
                }
            }
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

            // ✅ Working copy (sempre sem data)
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

                        if (!File.Exists(historicPath))
                        {
                            File.Copy(workingCopy, historicPath);
                            this.Loga($"✅ Snapshot criado (virada de dia): {historicPath}");
                        }
                        else
                        {
                            this.Loga($"ℹ️ Snapshot já existe para {lastWriteDate}");
                        }
                    }
                }

                // ✅ Salvar working copy (estado atual)
                File.WriteAllText(workingCopy, SanitizeControlCharacters(Content), _encoding ?? Encoding.UTF8);

                IsDirty = false;
                this.Filename = workingCopy;

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
            this.Filename = arquivoFinal;
            IsDirty = false;
            toolStripStatusLabel1.Text = "";
            this.AjustaCorFundo();
            this.QtMinutosEsse = 0;
            this.QtMinutos = cIni.ReadInt(Atual, "Tempo", 0);
            this.MotraCaracteres();
            this.Carregado = true; // ✅ FORÇAR Carregado = true após qualquer abertura
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
                        Content = conteudo;
                        controlContentTextBox.Text = Content;
                        controlContentTextBox.BackColor = Color.LightBlue; // Azul = baseado em histórico
                        IsDirty = true;

                        this.Loga($"✅ Conteúdo carregado de: {arquivo.Name} ({Content.Length} bytes)");

                        // Salvar imediatamente no working copy
                        string pastaArq = Path.GetDirectoryName(arquivoAtual);
                        string nomeBase = Path.GetFileNameWithoutExtension(arquivoAtual).Split('^')[0];
                        string workingCopy = Path.Combine(pastaArq, nomeBase + ".txt");

                        File.WriteAllText(workingCopy, SanitizeControlCharacters(Content), Encoding.UTF8);
                        this.Loga($"✅ Conteúdo salvo no working copy: {workingCopy}");

                        this.Filename = workingCopy;
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
            controlContentTextBox.Text = Content;

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
                if (!File.Exists(this.Filename))
                {
                    controlContentTextBox.BackColor = SystemColors.Window; // Branco
                    return;
                }

                // ✅ Usar DATA DE MODIFICAÇÃO, não nome do arquivo
                DateTime lastWrite = File.GetLastWriteTime(this.Filename);
                string lastWriteDate = lastWrite.ToString("dd-MM-yyyy");
                string today = Fun.Agora().ToShortDateString().Replace(@"/", "-");

                if (lastWriteDate == today)
                    controlContentTextBox.BackColor = SystemColors.Window; // Branco = hoje
                else
                    controlContentTextBox.BackColor = Color.AliceBlue; //  Color.LightBlue; // Azul = arquivo antigo
            }
            catch
            {
                controlContentTextBox.BackColor = SystemColors.Window; // Fallback branco
            }
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
                controlContentTextBox.Text = SanitizeControlCharacters(value);
            }
        }

        private void controlContentTextBox_TextChanged(object sender, EventArgs e)
        {
            SanitizeEditorTextIfNeeded();

            Console.WriteLine("controlContentTextBox_TextChanged");
            IsDirty = true;
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
                controlContentTextBox.Text = sanitizedText;

                int newSelectionStart = RemapIndexAfterSanitization(originalText, selectionStart);
                int selectionEnd = Math.Min(originalText.Length, selectionStart + selectionLength);
                int newSelectionEnd = RemapIndexAfterSanitization(originalText, selectionEnd);

                controlContentTextBox.SelectionStart = newSelectionStart;
                controlContentTextBox.SelectionLength = Math.Max(0, newSelectionEnd - newSelectionStart);
            }
            finally
            {
                _isSanitizingContentText = false;
            }

            this.Loga("Caracteres de controle removidos do editor.");
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
                return Settings.CurrentFont;
            }
            set
            {
                controlContentTextBox.Font = Settings.CurrentFont = value;
                Settings.Save();
            }
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

        private void controlContentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
            {
                PasteNormalizedClipboardText();
                e.SuppressKeyPress = true;
                e.Handled = true;
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
                Escolhido.usado = true;
                Escolhido.Nome = "";
                IsDirty = true;
                NovaTarefa = true;
                subAtividadesToolStripMenuItem.Enabled = true;
                renomearToolStripMenuItem1.Enabled = true;

            }
            this.CarregaArquivoDoProjeto(true);
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
            int QtdSub = this.cIni.ReadInt(EssaAtivi, "QtdSub", 0);
            this.Loga("Lendo do Ini a quantidade de SubAtividades da Atividade " + EssaAtivi);
            this.Loga("QtdSub = " + QtdSub.ToString());
            if (QtdSub > 0)
                this.MotraArqSub(QtdSub);
            else
            {
                cbSubprojeto.Visible = false;
                renomearToolStripMenuItem1.Enabled = false;
            }
        }

        private void MotraArqSub(int QtdSub)
        {
            cbSubprojeto.Visible = true;
            cbSubprojeto.Items.Clear();
            string DtHoje = Fun.Agora().ToShortDateString();
            string PastaAtual = this.PastaGeral + @"\" + this.Atual;
            bool AdicGeral = true;
            if (this.mostrarSóDoDiaToolStripMenuItem.Checked)
                AdicGeral = this.TemArqHoje(PastaAtual, ref DtHoje);
            if (AdicGeral)
                cbSubprojeto.Items.Add("GERAL");
            this.SUbAtual = cIni.ReadString(this.Atual, "SubAtual", "");
            this.Loga("SUbAtual = " + this.SUbAtual);
            List<String> Subs = new List<String>();
            for (int i = 0; i < QtdSub; i++)
            {
                string nmSubAtiv = "Sub" + (i + 1).ToString();
                string Nome = this.cIni.ReadString(this.Atual, nmSubAtiv, "");
                if (Nome.Length > 0)
                    Subs.Add(Nome);
            }
            Subs.Sort();
            for (int i = 0; i < Subs.Count; i++)
            {
                string Nome = Subs[i];
                bool Adic = true;
                if (this.mostrarSóDoDiaToolStripMenuItem.Checked)
                {
                    string PastaSub = PastaAtual + @"\" + Nome;
                    Adic = this.TemArqHoje(PastaSub, ref DtHoje);
                }
                if (Adic)
                {
                    this.Loga("Adicionado na Sub " + Nome);
                    cbSubprojeto.Items.Add(Nome);
                    if (Nome == this.SUbAtual)
                    {
                        try
                        {
                            cbSubprojeto.SelectedIndex = i + 1;
                        }
                        catch (Exception)
                        {
                            cbSubprojeto.SelectedIndex = cbSubprojeto.Items.Count - 1;
                        }
                    }
                }

            }
            if (this.SUbAtual == "")
            // if (this.SUbAtual == "GERAL")
            {
                renomearToolStripMenuItem.Enabled = false;
                cbSubprojeto.SelectedIndex = cbSubprojeto.FindStringExact("GERAL");
            }
            else
                renomearToolStripMenuItem.Enabled = true;
            apagarToolStripMenuItem.Enabled = renomearToolStripMenuItem.Enabled;
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
            string Data = Fun.Agora().ToShortDateString().Replace(@"/", "-");
            this.Filename = NomeDoArquivo(Data);
            this.Loga("Abrindo arquivo: " + this.Filename);
            this.Open(this.Filename);
            this.Text = this.TitAplicativo + " " + Path.GetFileName(this.Filename);
            this.Loga("CarregaArquivoDoProjeto finalizado - Carregado=" + this.Carregado);
        }

        private void cbProjetos_DropDownClosed(object sender, EventArgs e)
        {
            Console.WriteLine("cbProjetos_DropDownClosed");

            // ✅ SALVAR antes de trocar
            if (this.Carregado && this.IsDirty)
            {
                this.Save();
            }

            // ✅ Desabilitar timer
            this.timer1.Enabled = false;

            this.Atual = cbProjetos.Text;
            cIni.WriteString("Projetos", "Atual", cbProjetos.Text);
            this.CarregaArquivoDoProjeto(true);
            this.MostraArquivosDoProjeto();
        }

        private void MostraArquivosDoProjeto()
        {
            int QtdSub = this.cIni.ReadInt(this.Atual, "QtdSub", 0);
            if (QtdSub > 0)
            {
                this.MotraArqSub(QtdSub);
            }
            else
                renomearToolStripMenuItem.Enabled = false;
            apagarToolStripMenuItem.Enabled = renomearToolStripMenuItem.Enabled;
            this.PreencheComboArquivo(this.PastaGeral + @"\" + this.Atual + @"\" + this.SUbAtual);
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
            this.Loga("[v2.1] PreparaComboArquivo");

            bool adicionarTodas = false;
            if (this.mostrarSóDoDiaToolStripMenuItem.Checked == false)
            {
                int LimArqs = cIni.ReadInt("Projetos", "LimArqs", 31);
                int QtdArqs = 0;
                List<DateTime> ArqsAdds = new List<DateTime>();

                try
                {
                    DirectoryInfo info = new DirectoryInfo(Pasta);
                    if (!info.Exists)
                    {
                        Directory.CreateDirectory(Pasta);
                        this.Loga("Pasta criada: " + Pasta);
                    }

                    FileInfo[] arquivos = info.GetFiles("*.txt").OrderBy(p => p.CreationTime).ToArray();

                    foreach (FileInfo arquivo in arquivos)
                    {
                        string nome = arquivo.Name;
                        DateTime data = this.GetDataPeloNome(nome);
                        if (nome.IndexOf(this.Atual) > -1 && !ArqsAdds.Contains(data))
                        {
                            ArqsAdds.Add(data);
                            QtdArqs++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Loga("Erro em PreparaComboArquivo: " + ex.Message);
                }

                cbArquivos.Visible = true;
                int Ini = QtdArqs - LimArqs;
                if (mostrarTodas || Ini < 0)
                    Ini = 0;
                else
                    adicionarTodas = true;

                cbArquivos.Items.Clear();
                ArqsAdds.Sort();

                for (int i = Ini; i < QtdArqs; i++)
                    cbArquivos.Items.Add(ArqsAdds[i].ToShortDateString());

                string Data = Fun.Agora().ToShortDateString();
                int Pos = cbArquivos.Items.IndexOf(Data);

                if (Pos > -1)
                    cbArquivos.SelectedIndex = Pos;
                else
                {
                    cbArquivos.Items.Add(Data);
                    cbArquivos.Text = Data;
                }
            }

            if (adicionarTodas)
                this.cbArquivos.Items.Add("TODAS");
        }

        private string NomeDoArquivo(string Data, bool forcarDataEspecifica = false)
        {
            string nmSUb = "";
            string dirSub = "";
            this._SUbAtual = cIni.ReadString(this.Atual, "SubAtual", "");
            if (this._SUbAtual.Length > 0)
            {
                if (this._SUbAtual != "GERAL")
                {
                    nmSUb = this._SUbAtual + "^";
                    dirSub = @"\" + this._SUbAtual;
                }
            }
            string Pasta = this.PastaGeral + @"\" + this.Atual + dirSub;

            // ✅ Se forçar data específica, abrir EXATAMENTE o arquivo da data selecionada
            if (forcarDataEspecifica)
            {
                string sDataX = Data.Replace(@"/", "-");
                string arquivoData = Pasta + @"\" + this.Atual + "^" + nmSUb + sDataX + ".txt";
                this.Loga($"Forçando abertura do arquivo da data {Data}: {arquivoData}");
                return arquivoData;
            }

            // ✅ PRIORIDADE 1: Working copy SEM data (ex: Empregos^Cristian.txt)
            string workingCopy = Pasta + @"\" + this.Atual + "^" + nmSUb.TrimEnd('^') + ".txt";

            // Se working copy existe, usar ele
            if (File.Exists(workingCopy))
            {
                this.Loga("Working copy encontrado: " + workingCopy);
                return workingCopy;
            }

            // ✅ PRIORIDADE 2: Working copy ANTIGO (^current.txt) - migração
            string workingCopyAntigo = Pasta + @"\" + this.Atual + "^" + nmSUb + "current.txt";
            if (File.Exists(workingCopyAntigo))
            {
                this.Loga("Working copy antigo encontrado (^current.txt): " + workingCopyAntigo);
                return workingCopyAntigo;
            }

            // ✅ PRIORIDADE 3: Arquivo do dia atual (ex: Empregos^Cristian^17-03-2026.txt)
            string sData = Data.Replace(@"/", "-");
            string todayFile = Pasta + @"\" + this.Atual + "^" + nmSUb + sData + ".txt";

            this.Loga("Working copy não encontrado, usando arquivo do dia: " + todayFile);
            return todayFile;
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

            string nmSUb = string.IsNullOrEmpty(this.SUbAtual) || this.SUbAtual == "GERAL"
                ? "" : this.SUbAtual + "^";

            string dirSub = string.IsNullOrEmpty(this.SUbAtual) || this.SUbAtual == "GERAL"
                ? "" : @"\" + this.SUbAtual;

            string pasta = this.PastaGeral + @"\" + this.Atual + dirSub;

            string arquivoDataSelecionada =
                pasta + @"\" + this.Atual + "^" + nmSUb + dataSelecionadaStr + ".txt";

            string workingCopy =
                pasta + @"\" + this.Atual + "^" + nmSUb.TrimEnd('^') + ".txt";

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

            // 3) Para datas antigas: NÃO abrir 'mais próximo'
            this.Loga($"⚠️ Nenhum arquivo exato encontrado para a data {cbArquivos.Text}");

            MessageBox.Show(
                $"Não existe arquivo salvo para a data {cbArquivos.Text}.",
                "Anoteitor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Volta a seleção anterior
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
                controlContentTextBox.Text = Content;

                // ✅ Configurar estado
                this.Filename = filePath;
                this.IsDirty = false;
                this.Carregado = true;

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
                Atual = cbProjetos.Text;
                AtuArqASerMostrado();
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
            this.Loga("Carregado=" + this.Carregado + ", Text=" + cbSubprojeto.Text + ", Old=" + this.cbArquivosSUbOld);

            // ✅ REMOVIDO: if (this.Carregado) — bloqueava carga inicial
            if (cbSubprojeto.Text != this.cbArquivosSUbOld)
            {
                // ✅ Salvar antes de trocar SOMENTE se houver conteúdo não salvo
                if (this.Carregado && this.IsDirty)
                {
                    this.Loga("Salvando antes de trocar de subatividade");
                    this.Save();
                }

                string sData = Fun.Agora().ToShortDateString();
                string Data = sData.Replace(@"/", "-");
                this.SUbAtual = cbSubprojeto.Text;
                cIni.WriteString(this.Atual, "SubAtual", this.SUbAtual);

                this.timer1.Enabled = false; // Desabilitar timer durante transição

                this.Filename = NomeDoArquivo(Data);
                this.Loga("Abrindo novo arquivo: " + this.Filename);
                this.Open(this.Filename); // ✅ Open() agora define Carregado = true
                this.cbArquivosSUbOld = this.SUbAtual;

                renomearToolStripMenuItem.Enabled = (this.SUbAtual != "" && this.SUbAtual != "GERAL");
                apagarToolStripMenuItem.Enabled = renomearToolStripMenuItem.Enabled;

                string PastaSub = this.PastaGeral + @"\" + this.Atual +
                                 (string.IsNullOrEmpty(this.SUbAtual) || this.SUbAtual == "GERAL" ? "" : @"\" + this.SUbAtual);

                this.PreencheComboArquivo(PastaSub);
            }
        }

        private void renomearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SubAtividade cSubAtiv = new SubAtividade(Atual);
            cSubAtiv.SetNomeSubAtividade(cbSubprojeto.Text);
            cSubAtiv.ShowDialog();
            if (cSubAtiv.DialogResult == DialogResult.OK)
            {
                string Nome = cSubAtiv.Nome();
                if (Nome.Length > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    string sData = Fun.Agora().ToShortDateString();
                    string Data = sData.Replace(@"/", "-");
                    this.NomeArq = this.Atual + "^" + Nome + "^" + Data + ".txt";
                    this.Text = this.NomeArq + " - " + this.TitAplicativo;
                    toolStripStatusLabel1.Text = this.NomeArq;
                    this.SUbAtual = Nome;
                    cIni.WriteString(this.Atual, "SubAtual", this.SUbAtual);
                    int QtdSub = this.cIni.ReadInt(this.Atual, "QtdSub", 0);
                    this.MotraArqSub(QtdSub);
                    controlContentTextBox.BackColor = SystemColors.Window;
                    this.Cursor = Cursors.Default;
                }
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
                Invoke(new Action(() => { this.Text = "Anoteitor - Busca Concluída"; }));
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
                Invoke(new Action(() => { this.Text = $"Anoteitor - Buscando... {percent}%"; }));
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

                // Salva permanentemente
                Settings.CurrentFont = novaFonte;
                Settings.Save();

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
        private void btnFonteMais_Click(object sender, EventArgs e)
        {
            AlterarTamanhoFonte(1f);
        }



    }

    partial class cEscolhido
    {
        public string Nome = "";
        public bool usado = false;
    }

}
