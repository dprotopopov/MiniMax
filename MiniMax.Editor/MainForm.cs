using System;
using System.Windows.Forms;
using MiniMax.Editor.Properties;

namespace MiniMax.Editor
{
    public partial class MainForm : Form
    {
        private static readonly MiniMaxCreateDialog MiniMaxCreateDialog = new MiniMaxCreateDialog();
        private static readonly SettingsDialog SettingsDialog = new SettingsDialog();
        private static readonly RandomDialog RandomDialog = new RandomDialog();

        public MainForm()
        {
            InitializeComponent();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MiniMaxCreateDialog.ShowDialog() != DialogResult.OK) return;
            var child = new LinearMiniMaxForm(MiniMaxCreateDialog.Restrictions, MiniMaxCreateDialog.Variables)
            {
                MdiParent = this
            };
            child.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SettingsDialog.ShowDialog() != DialogResult.OK) return;
        }

        private void executeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = ActiveMdiChild as LinearMiniMaxForm;
            if (child == null) return;
            if (SettingsDialog.ShowDialog() != DialogResult.OK) return;
            if (!SettingsDialog.IsValid()) return;
            ISolver<double> solver;
            if (SettingsDialog.Gomory) solver = new GomorySolver<double>();
            else if (SettingsDialog.BranchAndBoundTree) solver = new BooleanTreeBranchAndBoundSolver<double>();
            else if (SettingsDialog.BranchAndBoundMulti) solver = new BooleanMultiBranchAndBoundSolver<double>();
            else if (SettingsDialog.Paragraph42) solver = new BooleanBranchAndBoundSolver42<double>();
            else throw new NotImplementedException();
            if (!child.IsValid())
            {
                MessageBox.Show(
                    Resources.ValidSymbolMessage,
                    Resources.ValidSymbolError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            child.Solve(solver);
        }

        private void randomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = ActiveMdiChild as LinearMiniMaxForm;
            if (child == null) return;
            if (RandomDialog.ShowDialog() != DialogResult.OK) return;
            child.Random(RandomDialog.Minimum, RandomDialog.Maximum);
        }

        private void simpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var child = ActiveMdiChild as LinearMiniMaxForm;
            if (child == null) return;
            child.Simple();
        }
    }
}