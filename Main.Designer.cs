using System;

namespace Anoteitor
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.controlContentTextBox = new System.Windows.Forms.TextBox();
            this.menubarMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemFilePageSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFileHeaderAndFooter = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFilePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemEditFind = new System.Windows.Forms.ToolStripMenuItem();
            this.procurarEmTodasDatasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.procurarPorTudoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditFindNext = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditGoTo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemEditSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemEditTimeDate = new System.Windows.Forms.ToolStripMenuItem();
            this.formatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFormatWordWrap = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemFormatFont = new System.Windows.Forms.ToolStripMenuItem();
            this.projetoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.novoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subAtividadesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.novaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renomearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.apagarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renomearToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.configurarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mostrarSóDoDiaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.temposToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuitemHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.menuitemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.btnFonteMenos = new System.Windows.Forms.ToolStripButton();
            this.btnFonteMais = new System.Windows.Forms.ToolStripButton();
            this.cbProjetos = new System.Windows.Forms.ToolStripComboBox();
            this.cbSubprojeto = new System.Windows.Forms.ToolStripComboBox();
            this.cbArquivos = new System.Windows.Forms.ToolStripComboBox();
            this.controlStatusBar = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbTempDecorr = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.menubarMain.SuspendLayout();
            this.controlStatusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // controlContentTextBox
            // 
            this.controlContentTextBox.AcceptsReturn = true;
            this.controlContentTextBox.AcceptsTab = true;
            this.controlContentTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.controlContentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlContentTextBox.Font = new System.Drawing.Font("Lucida Console", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.controlContentTextBox.HideSelection = false;
            this.controlContentTextBox.Location = new System.Drawing.Point(0, 24);
            this.controlContentTextBox.Margin = new System.Windows.Forms.Padding(10);
            this.controlContentTextBox.MaxLength = 0;
            this.controlContentTextBox.Multiline = true;
            this.controlContentTextBox.Name = "controlContentTextBox";
            this.controlContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.controlContentTextBox.Size = new System.Drawing.Size(632, 369);
            this.controlContentTextBox.TabIndex = 0;
            this.controlContentTextBox.WordWrap = false;
            this.controlContentTextBox.TextChanged += new System.EventHandler(this.controlContentTextBox_TextChanged);
            this.controlContentTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.controlContentTextBox_KeyDown);
            this.controlContentTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.controlContentTextBox_KeyUp);
            this.controlContentTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.controlContentTextBox_MouseDown);
            this.controlContentTextBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.controlContentTextBox_MouseMove);
            // 
            // menubarMain
            // 
            this.menubarMain.GripMargin = new System.Windows.Forms.Padding(0);
            this.menubarMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menubarMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.menuitemEdit,
            this.formatToolStripMenuItem,
            this.projetoToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.btnFonteMenos,
            this.btnFonteMais,
            this.cbProjetos,
            this.cbSubprojeto,
            this.cbArquivos});
            this.menubarMain.Location = new System.Drawing.Point(0, 0);
            this.menubarMain.Name = "menubarMain";
            this.menubarMain.Padding = new System.Windows.Forms.Padding(0);
            this.menubarMain.Size = new System.Drawing.Size(632, 24);
            this.menubarMain.TabIndex = 1;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemFileNew,
            this.menuitemFileOpen,
            this.menuitemFileSave,
            this.menuitemFileSaveAs,
            this.toolStripSeparator1,
            this.menuitemFilePageSetup,
            this.menuitemFileHeaderAndFooter,
            this.menuitemFilePrint,
            this.toolStripSeparator2,
            this.toolStripMenuItem2,
            this.menuitemFileExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(61, 24);
            this.fileToolStripMenuItem.Text = "&Arquivo";
            // 
            // menuitemFileNew
            // 
            this.menuitemFileNew.Name = "menuitemFileNew";
            this.menuitemFileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.menuitemFileNew.Size = new System.Drawing.Size(178, 22);
            this.menuitemFileNew.Text = "&Novo";
            this.menuitemFileNew.Click += new System.EventHandler(this.menuitemFileNew_Click);
            // 
            // menuitemFileOpen
            // 
            this.menuitemFileOpen.Name = "menuitemFileOpen";
            this.menuitemFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.menuitemFileOpen.Size = new System.Drawing.Size(178, 22);
            this.menuitemFileOpen.Text = "&Abrir...";
            this.menuitemFileOpen.Click += new System.EventHandler(this.menuitemFileOpen_Click);
            // 
            // menuitemFileSave
            // 
            this.menuitemFileSave.Name = "menuitemFileSave";
            this.menuitemFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuitemFileSave.Size = new System.Drawing.Size(178, 22);
            this.menuitemFileSave.Text = "&Salvar";
            this.menuitemFileSave.Click += new System.EventHandler(this.menuitemFileSave_Click);
            // 
            // menuitemFileSaveAs
            // 
            this.menuitemFileSaveAs.Name = "menuitemFileSaveAs";
            this.menuitemFileSaveAs.Size = new System.Drawing.Size(178, 22);
            this.menuitemFileSaveAs.Text = "Salvar &Como...";
            this.menuitemFileSaveAs.Click += new System.EventHandler(this.menuitemFileSaveAs_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(175, 6);
            // 
            // menuitemFilePageSetup
            // 
            this.menuitemFilePageSetup.Name = "menuitemFilePageSetup";
            this.menuitemFilePageSetup.Size = new System.Drawing.Size(178, 22);
            this.menuitemFilePageSetup.Text = "Page Set&up...";
            this.menuitemFilePageSetup.Click += new System.EventHandler(this.menuitemFilePageSetup_Click);
            // 
            // menuitemFileHeaderAndFooter
            // 
            this.menuitemFileHeaderAndFooter.Name = "menuitemFileHeaderAndFooter";
            this.menuitemFileHeaderAndFooter.Size = new System.Drawing.Size(178, 22);
            this.menuitemFileHeaderAndFooter.Text = "&Header && Footer...";
            this.menuitemFileHeaderAndFooter.Click += new System.EventHandler(this.menuitemFileHeaderAndFooter_Click);
            // 
            // menuitemFilePrint
            // 
            this.menuitemFilePrint.Name = "menuitemFilePrint";
            this.menuitemFilePrint.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.menuitemFilePrint.Size = new System.Drawing.Size(178, 22);
            this.menuitemFilePrint.Text = "&Impressão...";
            this.menuitemFilePrint.Click += new System.EventHandler(this.menuitemFilePrint_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(175, 6);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(178, 22);
            this.toolStripMenuItem2.Text = "Log";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // menuitemFileExit
            // 
            this.menuitemFileExit.Name = "menuitemFileExit";
            this.menuitemFileExit.Size = new System.Drawing.Size(178, 22);
            this.menuitemFileExit.Text = "Sai&r";
            this.menuitemFileExit.Click += new System.EventHandler(this.menuitemFileExit_Click);
            // 
            // menuitemEdit
            // 
            this.menuitemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemEditUndo,
            this.toolStripSeparator5,
            this.menuitemEditCut,
            this.menuitemEditCopy,
            this.menuitemEditPaste,
            this.menuitemEditDelete,
            this.toolStripSeparator6,
            this.menuitemEditFind,
            this.procurarEmTodasDatasToolStripMenuItem,
            this.procurarPorTudoToolStripMenuItem,
            this.menuitemEditFindNext,
            this.menuitemEditReplace,
            this.menuitemEditGoTo,
            this.toolStripSeparator3,
            this.menuitemEditSelectAll,
            this.menuitemEditTimeDate});
            this.menuitemEdit.Name = "menuitemEdit";
            this.menuitemEdit.Size = new System.Drawing.Size(49, 24);
            this.menuitemEdit.Text = "&Editar";
            this.menuitemEdit.DropDownOpening += new System.EventHandler(this.menuitemEdit_DropDownOpening);
            // 
            // menuitemEditUndo
            // 
            this.menuitemEditUndo.Name = "menuitemEditUndo";
            this.menuitemEditUndo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.menuitemEditUndo.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditUndo.Text = "&Undo";
            this.menuitemEditUndo.Click += new System.EventHandler(this.menuitemEditUndo_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(271, 6);
            // 
            // menuitemEditCut
            // 
            this.menuitemEditCut.Name = "menuitemEditCut";
            this.menuitemEditCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.menuitemEditCut.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditCut.Text = "Recortar";
            this.menuitemEditCut.Click += new System.EventHandler(this.menuitemEditCut_Click);
            // 
            // menuitemEditCopy
            // 
            this.menuitemEditCopy.Name = "menuitemEditCopy";
            this.menuitemEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.menuitemEditCopy.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditCopy.Text = "Copiar";
            this.menuitemEditCopy.Click += new System.EventHandler(this.menuitemEditCopy_Click);
            // 
            // menuitemEditPaste
            // 
            this.menuitemEditPaste.Name = "menuitemEditPaste";
            this.menuitemEditPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.menuitemEditPaste.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditPaste.Text = "Colar";
            this.menuitemEditPaste.Click += new System.EventHandler(this.menuitemEditPaste_Click);
            // 
            // menuitemEditDelete
            // 
            this.menuitemEditDelete.Name = "menuitemEditDelete";
            this.menuitemEditDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.menuitemEditDelete.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditDelete.Text = "Apagar";
            this.menuitemEditDelete.Click += new System.EventHandler(this.menuitemEditDelete_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(271, 6);
            // 
            // menuitemEditFind
            // 
            this.menuitemEditFind.Name = "menuitemEditFind";
            this.menuitemEditFind.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.menuitemEditFind.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditFind.Text = "Procurar...";
            this.menuitemEditFind.Click += new System.EventHandler(this.menuitemEditFind_Click);
            // 
            // procurarEmTodasDatasToolStripMenuItem
            // 
            this.procurarEmTodasDatasToolStripMenuItem.Name = "procurarEmTodasDatasToolStripMenuItem";
            this.procurarEmTodasDatasToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.procurarEmTodasDatasToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.procurarEmTodasDatasToolStripMenuItem.Text = "Procurar em todas datas";
            this.procurarEmTodasDatasToolStripMenuItem.Click += new System.EventHandler(this.procurarEmTodasDatasToolStripMenuItem_Click);
            // 
            // procurarPorTudoToolStripMenuItem
            // 
            this.procurarPorTudoToolStripMenuItem.Name = "procurarPorTudoToolStripMenuItem";
            this.procurarPorTudoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.F)));
            this.procurarPorTudoToolStripMenuItem.Size = new System.Drawing.Size(274, 22);
            this.procurarPorTudoToolStripMenuItem.Text = "Procurar por tudo";
            this.procurarPorTudoToolStripMenuItem.Click += new System.EventHandler(this.procurarPorTudoToolStripMenuItem_Click);
            // 
            // menuitemEditFindNext
            // 
            this.menuitemEditFindNext.Name = "menuitemEditFindNext";
            this.menuitemEditFindNext.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.menuitemEditFindNext.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditFindNext.Text = "Procurar Próximo...";
            this.menuitemEditFindNext.Click += new System.EventHandler(this.menuitemEditFindNext_Click);
            // 
            // menuitemEditReplace
            // 
            this.menuitemEditReplace.Name = "menuitemEditReplace";
            this.menuitemEditReplace.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.menuitemEditReplace.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditReplace.Text = "Trocar";
            this.menuitemEditReplace.Click += new System.EventHandler(this.menuitemEditReplace_Click);
            // 
            // menuitemEditGoTo
            // 
            this.menuitemEditGoTo.Name = "menuitemEditGoTo";
            this.menuitemEditGoTo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.menuitemEditGoTo.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditGoTo.Text = "Ir para...";
            this.menuitemEditGoTo.Click += new System.EventHandler(this.menuitemEditGoTo_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(271, 6);
            // 
            // menuitemEditSelectAll
            // 
            this.menuitemEditSelectAll.Name = "menuitemEditSelectAll";
            this.menuitemEditSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.menuitemEditSelectAll.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditSelectAll.Text = "Selecionar Tudo";
            this.menuitemEditSelectAll.Click += new System.EventHandler(this.menuitemEditSelectAll_Click);
            // 
            // menuitemEditTimeDate
            // 
            this.menuitemEditTimeDate.Name = "menuitemEditTimeDate";
            this.menuitemEditTimeDate.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.menuitemEditTimeDate.Size = new System.Drawing.Size(274, 22);
            this.menuitemEditTimeDate.Text = "Time/&Date";
            this.menuitemEditTimeDate.Click += new System.EventHandler(this.menuitemEditTimeDate_Click);
            // 
            // formatToolStripMenuItem
            // 
            this.formatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemFormatWordWrap,
            this.menuitemFormatFont});
            this.formatToolStripMenuItem.Name = "formatToolStripMenuItem";
            this.formatToolStripMenuItem.Size = new System.Drawing.Size(49, 24);
            this.formatToolStripMenuItem.Text = "Fonte";
            // 
            // menuitemFormatWordWrap
            // 
            this.menuitemFormatWordWrap.Name = "menuitemFormatWordWrap";
            this.menuitemFormatWordWrap.Size = new System.Drawing.Size(134, 22);
            this.menuitemFormatWordWrap.Text = "&Word Wrap";
            this.menuitemFormatWordWrap.CheckedChanged += new System.EventHandler(this.menuitemFormatWordWrap_CheckedChanged);
            this.menuitemFormatWordWrap.Click += new System.EventHandler(this.menuitemFormatWordWrap_Click);
            // 
            // menuitemFormatFont
            // 
            this.menuitemFormatFont.Name = "menuitemFormatFont";
            this.menuitemFormatFont.Size = new System.Drawing.Size(134, 22);
            this.menuitemFormatFont.Text = "Fonte";
            this.menuitemFormatFont.Click += new System.EventHandler(this.menuitemFormatFont_Click);
            // 
            // projetoToolStripMenuItem
            // 
            this.projetoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.novoToolStripMenuItem,
            this.subAtividadesToolStripMenuItem,
            this.renomearToolStripMenuItem1,
            this.toolStripMenuItem1,
            this.configurarToolStripMenuItem,
            this.mostrarSóDoDiaToolStripMenuItem,
            this.temposToolStripMenuItem});
            this.projetoToolStripMenuItem.Name = "projetoToolStripMenuItem";
            this.projetoToolStripMenuItem.Size = new System.Drawing.Size(74, 24);
            this.projetoToolStripMenuItem.Text = "Atividades";
            // 
            // novoToolStripMenuItem
            // 
            this.novoToolStripMenuItem.Name = "novoToolStripMenuItem";
            this.novoToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.novoToolStripMenuItem.Text = "Nova";
            this.novoToolStripMenuItem.Click += new System.EventHandler(this.novoToolStripMenuItem_Click);
            // 
            // subAtividadesToolStripMenuItem
            // 
            this.subAtividadesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.novaToolStripMenuItem,
            this.renomearToolStripMenuItem,
            this.apagarToolStripMenuItem});
            this.subAtividadesToolStripMenuItem.Name = "subAtividadesToolStripMenuItem";
            this.subAtividadesToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.subAtividadesToolStripMenuItem.Text = "SubAtividades";
            // 
            // novaToolStripMenuItem
            // 
            this.novaToolStripMenuItem.Name = "novaToolStripMenuItem";
            this.novaToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.novaToolStripMenuItem.Text = "Nova";
            this.novaToolStripMenuItem.Click += new System.EventHandler(this.novaSubAtividadeToolStripMenuItem_Click);
            // 
            // renomearToolStripMenuItem
            // 
            this.renomearToolStripMenuItem.Name = "renomearToolStripMenuItem";
            this.renomearToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.renomearToolStripMenuItem.Text = "Renomear";
            this.renomearToolStripMenuItem.Click += new System.EventHandler(this.renomearToolStripMenuItem_Click);
            // 
            // apagarToolStripMenuItem
            // 
            this.apagarToolStripMenuItem.Name = "apagarToolStripMenuItem";
            this.apagarToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.apagarToolStripMenuItem.Text = "Apagar";
            this.apagarToolStripMenuItem.Click += new System.EventHandler(this.apagarToolStripMenuItem_Click);
            // 
            // renomearToolStripMenuItem1
            // 
            this.renomearToolStripMenuItem1.Enabled = false;
            this.renomearToolStripMenuItem1.Name = "renomearToolStripMenuItem1";
            this.renomearToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.renomearToolStripMenuItem1.Text = "Renomear";
            this.renomearToolStripMenuItem1.Click += new System.EventHandler(this.renomearToolStripMenuItem1_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.toolStripMenuItem1.Text = "Apagar";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // configurarToolStripMenuItem
            // 
            this.configurarToolStripMenuItem.Name = "configurarToolStripMenuItem";
            this.configurarToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.configurarToolStripMenuItem.Text = "Configurar";
            this.configurarToolStripMenuItem.Click += new System.EventHandler(this.configurarToolStripMenuItem_Click);
            // 
            // mostrarSóDoDiaToolStripMenuItem
            // 
            this.mostrarSóDoDiaToolStripMenuItem.Enabled = false;
            this.mostrarSóDoDiaToolStripMenuItem.Name = "mostrarSóDoDiaToolStripMenuItem";
            this.mostrarSóDoDiaToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.mostrarSóDoDiaToolStripMenuItem.Text = "Mostrar só do dia";
            this.mostrarSóDoDiaToolStripMenuItem.Click += new System.EventHandler(this.mostrarSóDoDiaToolStripMenuItem_Click);
            // 
            // temposToolStripMenuItem
            // 
            this.temposToolStripMenuItem.Enabled = false;
            this.temposToolStripMenuItem.Name = "temposToolStripMenuItem";
            this.temposToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.temposToolStripMenuItem.Text = "Tempos";
            this.temposToolStripMenuItem.Click += new System.EventHandler(this.temposToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuitemHelp,
            this.toolStripSeparator4,
            this.menuitemAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // menuitemHelp
            // 
            this.menuitemHelp.Name = "menuitemHelp";
            this.menuitemHelp.Size = new System.Drawing.Size(160, 22);
            this.menuitemHelp.Text = "View &Help";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(157, 6);
            // 
            // menuitemAbout
            // 
            this.menuitemAbout.Name = "menuitemAbout";
            this.menuitemAbout.Size = new System.Drawing.Size(160, 22);
            this.menuitemAbout.Text = "&About Anoteitor";
            this.menuitemAbout.Click += new System.EventHandler(this.menuitemAbout_Click);
            // 
            // btnFonteMenos
            // 
            this.btnFonteMenos.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFonteMenos.Name = "btnFonteMenos";
            this.btnFonteMenos.Size = new System.Drawing.Size(24, 21);
            this.btnFonteMenos.Text = "A-";
            this.btnFonteMenos.ToolTipText = "Diminuir fonte";
            this.btnFonteMenos.Click += new System.EventHandler(this.btnFonteMenos_Click);
            // 
            // btnFonteMais
            // 
            this.btnFonteMais.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFonteMais.Name = "btnFonteMais";
            this.btnFonteMais.Size = new System.Drawing.Size(27, 21);
            this.btnFonteMais.Text = "A+";
            this.btnFonteMais.ToolTipText = "Aumentar fonte";
            this.btnFonteMais.Click += new System.EventHandler(this.btnFonteMais_Click);
            // 
            // cbProjetos
            // 
            this.cbProjetos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProjetos.Name = "cbProjetos";
            this.cbProjetos.Size = new System.Drawing.Size(130, 24);
            this.cbProjetos.Sorted = true;
            this.cbProjetos.DropDownClosed += new System.EventHandler(this.cbProjetos_DropDownClosed);
            this.cbProjetos.SelectedIndexChanged += new System.EventHandler(this.cbProjetos_SelectedIndexChanged);
            this.cbProjetos.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cbProjetos_KeyUp);
            // 
            // cbSubprojeto
            // 
            this.cbSubprojeto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSubprojeto.Name = "cbSubprojeto";
            this.cbSubprojeto.Size = new System.Drawing.Size(121, 24);
            this.cbSubprojeto.Visible = false;
            this.cbSubprojeto.SelectedIndexChanged += new System.EventHandler(this.cbSubprojeto_SelectedIndexChanged);
            // 
            // cbArquivos
            // 
            this.cbArquivos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbArquivos.DropDownWidth = 79;
            this.cbArquivos.Name = "cbArquivos";
            this.cbArquivos.Size = new System.Drawing.Size(90, 24);
            this.cbArquivos.Visible = false;
            this.cbArquivos.DropDownClosed += new System.EventHandler(this.cbArquivos_DropDownClosed);
            this.cbArquivos.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cbArquivos_KeyUp);
            // 
            // controlStatusBar
            // 
            this.controlStatusBar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.controlStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lbTempDecorr});
            this.controlStatusBar.Location = new System.Drawing.Point(0, 371);
            this.controlStatusBar.Name = "controlStatusBar";
            this.controlStatusBar.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.controlStatusBar.Size = new System.Drawing.Size(632, 22);
            this.controlStatusBar.SizingGrip = false;
            this.controlStatusBar.TabIndex = 2;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 0);
            // 
            // lbTempDecorr
            // 
            this.lbTempDecorr.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.lbTempDecorr.Name = "lbTempDecorr";
            this.lbTempDecorr.Size = new System.Drawing.Size(0, 17);
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Interval = 60000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 393);
            this.Controls.Add(this.controlStatusBar);
            this.Controls.Add(this.controlContentTextBox);
            this.Controls.Add(this.menubarMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menubarMain;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "{DocumentName} - Anoteitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.menubarMain.ResumeLayout(false);
            this.menubarMain.PerformLayout();
            this.controlStatusBar.ResumeLayout(false);
            this.controlStatusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        //private void btnFonteMais_Click(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        private System.Windows.Forms.TextBox controlContentTextBox;
        private System.Windows.Forms.MenuStrip menubarMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuitemFileNew;
        private System.Windows.Forms.ToolStripMenuItem menuitemFileOpen;
        private System.Windows.Forms.ToolStripMenuItem menuitemFileSave;
        private System.Windows.Forms.ToolStripMenuItem menuitemFileSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuitemFilePageSetup;
        private System.Windows.Forms.ToolStripMenuItem menuitemFilePrint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuitemFileExit;
        private System.Windows.Forms.ToolStripMenuItem menuitemEdit;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditFind;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditFindNext;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditReplace;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditGoTo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditSelectAll;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditTimeDate;
        private System.Windows.Forms.ToolStripMenuItem formatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuitemFormatWordWrap;
        private System.Windows.Forms.ToolStripMenuItem menuitemFormatFont;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuitemHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem menuitemAbout;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditUndo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditCut;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditCopy;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditPaste;
        private System.Windows.Forms.ToolStripMenuItem menuitemEditDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.StatusStrip controlStatusBar;
        private System.Windows.Forms.ToolStripMenuItem menuitemFileHeaderAndFooter;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem projetoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem novoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configurarToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox cbProjetos;
        private System.Windows.Forms.ToolStripComboBox cbArquivos;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem mostrarSóDoDiaToolStripMenuItem;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.ToolStripStatusLabel lbTempDecorr;
        private System.Windows.Forms.ToolStripMenuItem temposToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox cbSubprojeto;
        private System.Windows.Forms.ToolStripMenuItem subAtividadesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem novaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renomearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renomearToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem apagarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem procurarEmTodasDatasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem procurarPorTudoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripButton btnFonteMenos;
        private System.Windows.Forms.ToolStripButton btnFonteMais;
    }
}