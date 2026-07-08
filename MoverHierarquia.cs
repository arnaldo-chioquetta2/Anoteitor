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
        private string _projetoOrigem;
        private string _pastaProjetoOrigem;
        private List<string> _caminhoOrigem;
        private string _ultimoCaminhoSelecionadoTree;

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
            ConfigurarOrigemAtual(projetoOrigem, pastaProjetoOrigem, caminhoOrigem);
            CaminhoDestinoSelecionado = null;
            this.FormClosing += MoverHierarquia_FormClosing;
            CarregarTreePreservandoEstado();
        }

        public void ConfigurarOrigemAtual(
            string projetoOrigem,
            string pastaProjetoOrigem,
            IEnumerable<string> caminhoOrigem)
        {
            _projetoOrigem = projetoOrigem;
            _pastaProjetoOrigem = pastaProjetoOrigem;
            _caminhoOrigem = (caminhoOrigem ?? Enumerable.Empty<string>()).ToList();
            LimparSelecaoDestino();
        }

        public void AtualizarArvoreSeNecessario()
        {
            CarregarTreePreservandoEstado();
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

        private void CarregarTreePreservandoEstado()
        {
            SalvarSelecaoTree();
            HashSet<string> expandidos = CapturarNosExpandidos();

            CarregarTree();

            RestaurarNosExpandidos(expandidos);
            RestaurarSelecaoTree();
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
            Hide();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Hide();
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

                SalvarSelecaoTree();
                HashSet<string> expandidos = CapturarNosExpandidos();
                Directory.CreateDirectory(novaPasta);

                List<string> novoCaminho = destino.CaminhoSubtarefas.ToList();
                novoCaminho.Add(nome);

                _ultimoCaminhoSelecionadoTree = ChaveNo(destino.Projeto, novoCaminho);
                CarregarTree();
                RestaurarNosExpandidos(expandidos);
                RestaurarSelecaoTree();
                SelecionarNo(destino.Projeto, novoCaminho);
            }
        }

        private void MoverHierarquia_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                DialogResult = DialogResult.Cancel;
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

        private void LimparSelecaoDestino()
        {
            ProjetoDestinoSelecionado = null;
            PastaProjetoDestinoSelecionada = null;
            CaminhoDestinoSelecionado = null;
        }

        private string ChaveNo(string projeto, IEnumerable<string> caminhoSubtarefas)
        {
            List<string> caminho = caminhoSubtarefas?.ToList() ?? new List<string>();
            return projeto + "|" + string.Join("\\", caminho);
        }

        private string ChaveNo(TreeNode node)
        {
            NoDestinoMover info = node?.Tag as NoDestinoMover;
            if (info == null)
                return "";

            return ChaveNo(info.Projeto, info.CaminhoSubtarefas);
        }

        private void SalvarSelecaoTree()
        {
            _ultimoCaminhoSelecionadoTree = ChaveNo(tvDestino.SelectedNode);
        }

        private void RestaurarSelecaoTree()
        {
            if (string.IsNullOrWhiteSpace(_ultimoCaminhoSelecionadoTree))
                return;

            TreeNode node = EncontrarNoPorChave(tvDestino.Nodes, _ultimoCaminhoSelecionadoTree);
            if (node != null)
            {
                tvDestino.SelectedNode = node;
                node.EnsureVisible();
            }
        }

        private TreeNode EncontrarNoPorChave(TreeNodeCollection nodes, string chave)
        {
            foreach (TreeNode node in nodes)
            {
                if (string.Equals(ChaveNo(node), chave, StringComparison.OrdinalIgnoreCase))
                    return node;

                TreeNode encontrado = EncontrarNoPorChave(node.Nodes, chave);
                if (encontrado != null)
                    return encontrado;
            }

            return null;
        }

        private HashSet<string> CapturarNosExpandidos()
        {
            HashSet<string> chaves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CapturarNosExpandidos(tvDestino.Nodes, chaves);
            return chaves;
        }

        private void CapturarNosExpandidos(TreeNodeCollection nodes, HashSet<string> chaves)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded)
                    chaves.Add(ChaveNo(node));

                CapturarNosExpandidos(node.Nodes, chaves);
            }
        }

        private void RestaurarNosExpandidos(HashSet<string> chaves)
        {
            RestaurarNosExpandidos(tvDestino.Nodes, chaves);
        }

        private void RestaurarNosExpandidos(TreeNodeCollection nodes, HashSet<string> chaves)
        {
            foreach (TreeNode node in nodes)
            {
                if (chaves != null && chaves.Contains(ChaveNo(node)))
                    node.Expand();

                RestaurarNosExpandidos(node.Nodes, chaves);
            }
        }
    }
}
