using System;
using System.Drawing;
using System.Windows.Forms;

namespace Anoteitor
{
    public partial class SubAtividade : Form
    {
        private string Atividade = "";
        private string NomeSubAtividade = "";
        private string NomeSubAtividadeAnt = "";
        private int QtdSub = 0;        
        private int NrSub = 0;
        private INI cIni;
        private readonly bool SomenteNome;

        public SubAtividade(string Ativ, bool somenteNome = false)
        {
            InitializeComponent();
            this.Atividade = Ativ;
            this.SomenteNome = somenteNome;
            this.lbAtividade.Text = "Atividade: " + Ativ;
            this.NomeSubAtividadeAnt = "";
            txSub.Text = "";
            txSub.BackColor = SystemColors.Window;
        }

        private void button2_Click(object sender, EventArgs e)
        {            
            this.Grava();
        }

        private void Grava()
        {
            this.NomeSubAtividade = txSub.Text.Trim();
            if (this.NomeSubAtividade.Length == 0)
                return;

            if (this.SomenteNome)
            {
                this.DialogResult = DialogResult.OK;
                Close();
                return;
            }

            Funcoes Fun = new Funcoes();
#if DEBUG            
            this.cIni = new INI(Fun.Caminho());
#else
            this.cIni = new INI();
#endif
            this.QtdSub = cIni.ReadInt(this.Atividade, "QtdSub", 0);            
            if (this.VerificaSeTem(QtdSub) == false)
            {
                this.NomeSubAtividade = "";
                MessageBox.Show(this, "Já existe esta sub tarefa com este nome", "Anoteitor");                
            }                
            else
            {
                if (this.NomeSubAtividadeAnt.Length == 0)
                {
                    this.QtdSub++;
                    this.cIni.WriteInt(this.Atividade, "QtdSub", QtdSub);
                    this.NrSub = this.QtdSub;
                }
                else
                {
                    for (int j = 0; j < this.QtdSub; j++)
                    {
                        string Sub = "Sub" + (j + 1).ToString();
                        string NomeSub = cIni.ReadString(this.Atividade, Sub, "");
                        if (NomeSub == this.NomeSubAtividadeAnt)
                        {
                            this.NrSub = j + 1;
                            string PastaGeral = cIni.ReadString("Projetos", "Pasta", "");
                            string PastaSubAtual = this.NomeSubAtividadeAnt;
                            string PastaRaiz = PastaGeral + @"\" + this.Atividade + @"\";
                            Fun.Renomeia(PastaRaiz, PastaSubAtual, this.NomeSubAtividade);
                            break;
                        }
                    }
                }
                this.cIni.WriteString(this.Atividade, ("Sub" + this.NrSub.ToString()), this.NomeSubAtividade);
                this.cIni.WriteString(this.Atividade, "SubAtual", this.NomeSubAtividade);
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool VerificaSeTem(int Nr)
        {
            string nome = this.NomeSubAtividade.ToLower();
            for (int i = 1; i < Nr; i++)
            {
                string nmSub = "Sub" + i.ToString();
                string SubProj = this.cIni.ReadString(this.Atividade, nmSub, "").ToLower();
                if (SubProj == nome)
                    return false;
            }
            return true;
        }

        public string Nome()
        {
            return this.NomeSubAtividade;
        }

        private void txSub_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                this.Grava();
        }

        public int getQtdSub()
        {
            return this.QtdSub;
        }

        internal void SetNomeSubAtividade(string NomeSubAtividade)
        {
            this.NomeSubAtividadeAnt = NomeSubAtividade;
            txSub.Text = NomeSubAtividade;
            txSub.BackColor = SystemColors.Window;
        }

    }
}
