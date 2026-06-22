using System;
using System.Linq;
using System.Windows.Forms;
using System.IO.Compression;
using System.IO;

namespace Anoteitor
{
    public partial class Mensagem : Form
    {
        public Mensagem()
        {
            InitializeComponent();
        }

        public string Titulo { get; internal set; }
        public string Tipo { get; internal set; }
        public string PastaAtual { get; internal set; }

        public string Atual { get; internal set; }
        public int QtdSub { get; internal set; }
        public string PastaGeral { get; internal set; }

        private void button1_Click(object sender, EventArgs e)
        {
            string PastaSub = "";
            string ArqZip = "";
            if (Tipo == "Tarefa")
            {
                TiraDoIni();
                PastaSub = PastaAtual;
                ArqZip = PastaGeral + @"\" + Titulo + ".zip";
            } else
            {
                TiraDoIniSub();
                PastaSub = PastaAtual + @"\" + Titulo;
                ArqZip = PastaAtual + @"\" + Titulo + ".zip";
            }
            if (checkBox1.Checked)
            {
                this.Text = "Compactando..";
                ZipFile.CreateFromDirectory(PastaSub, ArqZip);
            }
            this.Text = "Apagando..";
            DirectoryInfo info = new DirectoryInfo(PastaSub);
            FileInfo[] arquivos = info.GetFiles();
            button1.Visible = false;
            button2.Visible = false;            
            progressBar1.Visible = true;
            progressBar1.Enabled = true;
            if (Tipo == "Tarefa")
            {
                ApagaTarefa();
            } else
            {
                ApagaSub(arquivos, PastaSub);
            }                
            progressBar1.Enabled = false;
            progressBar1.Visible = false;
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void ApagaTarefa()
        {
            if (!Directory.Exists(PastaAtual))
                return;

            DirectoryInfo info = new DirectoryInfo(PastaAtual);
            FileInfo[] arquivos = info.GetFiles("*", SearchOption.AllDirectories);
            DirectoryInfo[] diretorios = info.GetDirectories("*", SearchOption.AllDirectories)
                .OrderByDescending(d => d.FullName.Length)
                .ToArray();

            int Max = arquivos.Length + diretorios.Length + 1;
            progressBar1.Maximum = Math.Max(Max, 1);
            progressBar1.Value = 0;

            foreach (FileInfo Arq in arquivos)
            {
                try
                {
                    Arq.Attributes = FileAttributes.Normal;
                    Arq.Delete();
                }
                catch (Exception)
                {
                    // Mantém o comportamento atual em caso de arquivo travado ou protegido.
                }

                if (progressBar1.Value < progressBar1.Maximum)
                    progressBar1.Value++;
            }

            foreach (DirectoryInfo Dir in diretorios)
            {
                try
                {
                    Dir.Attributes = FileAttributes.Normal;
                    Dir.Delete();
                }
                catch (Exception)
                {
                    // Mantém o comportamento atual em caso de pasta já removida ou travada.
                }

                if (progressBar1.Value < progressBar1.Maximum)
                    progressBar1.Value++;
            }

            try
            {
                info.Delete();
                if (progressBar1.Value < progressBar1.Maximum)
                    progressBar1.Value++;
            }
            catch (Exception)
            {
                // Não faz nada, mas deveria informar no log
            }            
        }

        private void ApagaSub(FileInfo[] arquivos, string PastaSub)
        {
            progressBar1.Maximum = arquivos.Length;
            int Cont = 0;
            foreach (FileInfo arquivo in arquivos)
            {
                File.Delete(arquivo.FullName);
                progressBar1.Value = Cont;
                Cont++;
            }
            try
            {
                File.Delete(PastaSub);
            }
            catch (Exception)
            {
                // Não faz nada
            }

        }

        private void TiraDoIni()
        {
            INI cIni;
            Funcoes Fun = new Funcoes();
            cIni = new INI(Fun.Caminho());
            bool Achou = false;
            int Qtd = cIni.ReadInt("Projetos", "Qtd", 0);
            for (int i = 1; i < (Qtd + 1); i++)
            {
                string nmAtiv = "Pro" + i.ToString();
                string Ativ = cIni.ReadString("NmProjetos", nmAtiv, "");
                if (Achou)
                {
                    nmAtiv = "Pro" + (i - 1).ToString();
                    cIni.WriteString("NmProjetos", nmAtiv, Ativ);
                }
                else
                    if (Ativ == Titulo)
                    Achou = true;
            }
            cIni.WriteInt("Projetos", "Qtd", Qtd - 1);
        }


        private void TiraDoIniSub()
        {
            INI cIni;
            Funcoes Fun = new Funcoes();
            cIni = new INI(Fun.Caminho());
            bool Achou = false;
            for (int i = 1; i < (QtdSub+1); i++)
            {
                string nmSubAtiv = "Sub" + i.ToString();
                string Sub = cIni.ReadString(Atual, nmSubAtiv, "");
                if (Achou)
                {
                    nmSubAtiv = "Sub" + (i - 1).ToString();
                    cIni.WriteString(Atual, nmSubAtiv, Sub);
                } else
                    if (Sub == Titulo)
                        Achou = true;
            }
            cIni.WriteString(Atual, ("Sub" + QtdSub.ToString()), "");
            cIni.WriteInt(Atual, "QtdSub", QtdSub - 1);
            cIni.WriteString(Atual, "SubAtual", "GERAL");
        }

        private void Mensagem_Activated(object sender, EventArgs e)
        {
            label1.Text = "Tem certeza que deseja excluir a sub tarefa '" + Titulo+"'";
            this.Text = "Deletar " + Tipo;
        }
    }
}
