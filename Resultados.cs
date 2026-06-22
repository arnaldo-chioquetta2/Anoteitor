using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
namespace Anoteitor {
    public partial class Resultados : Form {

        private readonly Main _Main;
        private readonly List<string> _resultFiles;
        private readonly CancellationTokenSource _cts;

        public Resultados(Main main, List<(string filePath, string displayText)> resultados, CancellationTokenSource cts)
        {
            InitializeComponent();
            _Main = main;
            _cts = cts;
            _resultFiles = new List<string>();

            foreach (var (filePath, displayText) in resultados)
            {
                string fileName = Path.GetFileName(filePath);
                string datePart = Helper.ExtractDateFromFileName(fileName);
                _resultFiles.Add(filePath);
                listBox1.Items.Add($"{datePart} ➝ {displayText}");
            }

            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;

            //listBox1.Click += ListBox1_Click;
            //listBox1.DoubleClick += ListBox1_DoubleClick;

            // Cancela a busca ao fechar a janela
            //this.FormClosing += Resultados_FormClosing;
        }

        private void Resultados_Load(object sender, EventArgs e) {
            
        }
        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            // Abre o arquivo e fecha a janela ao dar duplo clique
            if (OpenSelectedFile(true))
            {
                this.Close();
            }
        }
        private void listBox1_Click(object sender, EventArgs e)
        {
            OpenSelectedFile();
        }
        private bool OpenSelectedFile(bool ativar=false)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0 || index >= _resultFiles.Count)
                return false;

            string selectedText = listBox1.SelectedItem?.ToString() ?? "";
            int separatorIndex = selectedText.IndexOf('➝');
            string searchText = separatorIndex >= 0
                ? selectedText.Substring(separatorIndex + 1).Trim()
                : selectedText;
            string filePath = _resultFiles[index];

            _Main.Open(filePath, searchText, ativar: ativar);
            return true; // Indica que o arquivo foi aberto
        }

        public void AdicionarResultado(string filePath, string displayText)
        {
            string fileName = Path.GetFileName(filePath);
            string datePart = Helper.ExtractDateFromFileName(fileName);
            _resultFiles.Add(filePath);
            listBox1.Items.Add($"{datePart} ➝ {displayText}");

            // Mantém o último item visível
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel(); // Envia o sinal de cancelamento
                this.Text = "Anoteitor - Busca Cancelada";
            }
        }

        private void Resultados_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel(); // Cancela a busca ao fechar a tela de resultados
            _Main.Text = "Anoteitor - Busca Cancelada";
        }

    }
}
