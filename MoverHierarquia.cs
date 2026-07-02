using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Anoteitor
{
    public partial class MoverHierarquia : Form
    {
        private class NoDestinoMover
        {
            public string Projeto { get; set; }
            public string PastaProjeto { get; set; }
            public List<string> CaminhoSubtarefas { get; set; }
        }

        private readonly string _pastaRaizProjetos;
        private readonly string _projetoOrigem;
        private readonly List<string> _caminhoOrigem;

        public string ProjetoDestinoSelecionado { get; private set; }
        public string PastaProjetoDestinoSelecionada { get; private set; }
        public List<string> CaminhoDestinoSelecionado { get; private set; }

        public MoverHierarquia(
            string pastaRaizProjetos,
            string projetoOrigem,
            IEnumerable<string> caminhoOrigem,
            string pastaProjetoOrigem)
        {
            InitializeComponent();
            _pastaRaizProjetos = pastaRaizProjetos;
            _projetoOrigem = projetoOrigem;
            _caminhoOrigem = (caminhoOrigem ?? Enumerable.Empty<string>()).ToList();
            ProjetoDestinoSelecionado = projetoOrigem;
            PastaProjetoDestinoSelecionada = pastaProjetoOrigem;
            CaminhoDestinoSelecionado = null;
            CarregarTree();
        }

        private void CarregarTree()
        {
            tvDestino.BeginUpdate();
            tvDestino.Nodes.Clear();

            if (Directory.Exists(_pastaRaizProjetos))
            {
                foreach (string pastaProjeto in Directory
                    .GetDirectories(_pastaRaizProjetos)
                    .OrderBy(p => Path.GetFileName(p), StringComparer.CurrentCultureIgnoreCase))
                {
                    string nomeProjeto = Path.GetFileName(pastaProjeto);
                    TreeNode noProjeto = CriarNo(nomeProjeto, nomeProjeto, pastaProjeto, new List<string>());
                    tvDestino.Nodes.Add(noProjeto);
                    CarregarSubarvore(noProjeto, nomeProjeto, pastaProjeto, new List<string>());
                    noProjeto.Expand();
                }
            }

            tvDestino.EndUpdate();

            if (tvDestino.Nodes.Count > 0)
                tvDestino.SelectedNode = tvDestino.Nodes[0];
        }

        private void CarregarSubarvore(TreeNode noPai, string projeto, string pastaPai, List<string> caminhoAtual)
        {
            if (!Directory.Exists(pastaPai))
                return;

            foreach (string pasta in Directory
                .GetDirectories(pastaPai)
                .OrderBy(p => Path.GetFileName(p), StringComparer.CurrentCultureIgnoreCase))
            {
                string nome = Path.GetFileName(pasta);
                List<string> caminhoFilho = new List<string>(caminhoAtual) { nome };
                TreeNode noFilho = CriarNo(nome, projeto, Path.Combine(_pastaRaizProjetos, projeto), caminhoFilho);
                noPai.Nodes.Add(noFilho);
                CarregarSubarvore(noFilho, projeto, pasta, caminhoFilho);
            }
        }

        private TreeNode CriarNo(string texto, string projeto, string pastaProjeto, List<string> caminhoSubtarefas)
        {
            return new TreeNode(texto)
            {
                Tag = new NoDestinoMover
                {
                    Projeto = projeto,
                    PastaProjeto = pastaProjeto,
                    CaminhoSubtarefas = new List<string>(caminhoSubtarefas ?? new List<string>())
                }
            };
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            NoDestinoMover destino = ObterDestinoSelecionado();
            if (destino == null)
            {
                MessageBox.Show(this, "Selecione um destino.", "Mover", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ProjetoDestinoSelecionado = destino.Projeto;
            PastaProjetoDestinoSelecionada = destino.PastaProjeto;
            CaminhoDestinoSelecionado = destino.CaminhoSubtarefas.ToList();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCriarSubTarefa_Click(object sender, EventArgs e)
        {
            NoDestinoMover destino = ObterDestinoSelecionado();
            if (destino == null)
            {
                MessageBox.Show(this, "Selecione um destino antes de criar a sub tarefa.", "Mover", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string tituloPai = destino.CaminhoSubtarefas.Count == 0
                ? destino.Projeto
                : destino.CaminhoSubtarefas[destino.CaminhoSubtarefas.Count - 1];

            using (SubAtividade dialogo = new SubAtividade(tituloPai, true))
            {
                dialogo.ShowDialog(this);
                if (dialogo.DialogResult != DialogResult.OK)
                    return;

                string nome = dialogo.Nome();
                if (string.IsNullOrWhiteSpace(nome))
                    return;

                if (nome.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    MessageBox.Show(this, "O nome contém caracteres inválidos.", "Mover", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string pastaPai = PastaDoContexto(destino);
                string novaPasta = Path.Combine(pastaPai, nome);

                if (Directory.Exists(novaPasta))
                {
                    MessageBox.Show(this, "Já existe uma sub tarefa com esse nome no destino.", "Mover", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Directory.CreateDirectory(novaPasta);

                List<string> novoCaminho = destino.CaminhoSubtarefas.ToList();
                novoCaminho.Add(nome);

                CarregarTree();
                SelecionarNo(destino.Projeto, novoCaminho);
            }
        }

        private NoDestinoMover ObterDestinoSelecionado()
        {
            return tvDestino.SelectedNode?.Tag as NoDestinoMover;
        }

        private string PastaDoContexto(NoDestinoMover destino)
        {
            string pasta = destino.PastaProjeto;

            foreach (string parte in destino.CaminhoSubtarefas)
            {
                if (!string.IsNullOrWhiteSpace(parte) &&
                    !string.Equals(parte, "GERAL", StringComparison.OrdinalIgnoreCase))
                {
                    pasta = Path.Combine(pasta, parte);
                }
            }

            return pasta;
        }

        private void SelecionarNo(string projeto, List<string> caminhoSubtarefas)
        {
            foreach (TreeNode noProjeto in tvDestino.Nodes)
            {
                NoDestinoMover destinoProjeto = noProjeto.Tag as NoDestinoMover;
                if (destinoProjeto == null ||
                    !string.Equals(destinoProjeto.Projeto, projeto, StringComparison.OrdinalIgnoreCase))
                    continue;

                TreeNode encontrado = EncontrarNo(noProjeto, projeto, caminhoSubtarefas ?? new List<string>());
                if (encontrado != null)
                {
                    tvDestino.SelectedNode = encontrado;
                    encontrado.EnsureVisible();
                    return;
                }
            }
        }

        private TreeNode EncontrarNo(TreeNode atual, string projeto, List<string> caminhoSubtarefas)
        {
            NoDestinoMover destino = atual.Tag as NoDestinoMover;
            if (destino != null &&
                string.Equals(destino.Projeto, projeto, StringComparison.OrdinalIgnoreCase) &&
                destino.CaminhoSubtarefas.SequenceEqual(caminhoSubtarefas, StringComparer.OrdinalIgnoreCase))
            {
                return atual;
            }

            foreach (TreeNode filho in atual.Nodes)
            {
                TreeNode encontrado = EncontrarNo(filho, projeto, caminhoSubtarefas);
                if (encontrado != null)
                    return encontrado;
            }

            return null;
        }
    }
}
