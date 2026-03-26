using System;
using System.Windows.Forms;

namespace Anoteitor
{
    public partial class FindDialog : Form {
        private readonly Main _Main;

        public string SelText { get; internal set; }

        public FindDialog(Main pMain) {
            InitializeComponent();
            _Main = pMain;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Hide();
            _Main.Activate();
            _Main.BringToFront();
        }

        private void FindDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
            _Main.Activate();
            _Main.BringToFront();
        }

        private void controlTextBox_TextChanged(object sender, EventArgs e) {
            UpdateFindNextButton();
        }

        private void UpdateFindNextButton() {
            buttonFindNext.Enabled = controlTextBox.Text.Length > 0;
        }

        private void FindDialog_Load(object sender, EventArgs e) {
            UpdateFindNextButton();
            this.controlTextBox.Text = this.SelText;

        }

        private void buttonFindNext_Click(object sender, EventArgs e) {
            var SearchText = controlTextBox.Text;
            var MatchCase = controlMatchCaseCheckbox.Checked;
            var bSearchDown = controlDownRadioButton.Checked;

            if (!_Main.FindAndSelect(SearchText, MatchCase, bSearchDown)) {
                MessageBox.Show(this, CONST.CannotFindMessage.FormatUsingObject(new { SearchText = SearchText }), "Anoteitor");
            }
        }

        public void Triggered() {
            controlTextBox.Focus();
        }

        private void controlTextBox_Enter(object sender, EventArgs e) {
            var Sender = (TextBox)sender;
            Sender.SelectAll();
        }

        public new void Show(IWin32Window window = null) {
            controlTextBox.Focus();
            controlTextBox.SelectAll();

            base.Show(_Main);
            //if (window == null) {
            //    base.Show();
            //} else {
            //    base.Show(window);
            //}
        }
    }
}
