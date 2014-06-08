using System.Windows.Forms;
using MyLibrary.Types;

namespace MiniMax.Editor
{
    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        public bool Gomory
        {
            get { return radioButtonGomory.Checked; }
            set { radioButtonGomory.Checked = value; }
        }

        public bool BranchAndBoundTree
        {
            get { return radioButtonBranchAndBoundTree.Checked; }
            set { radioButtonBranchAndBoundTree.Checked = value; }
        }

        public bool BranchAndBoundMulti
        {
            get { return radioButtonBranchAndBoundMulti.Checked; }
            set { radioButtonBranchAndBoundMulti.Checked = value; }
        }

        public bool Paragraph42
        {
            get { return radioButtonParagraph42.Checked; }
            set { radioButtonParagraph42.Checked = value; }
        }

        public bool IsValid()
        {
            return Boolean.Xor(
                Gomory,
                BranchAndBoundTree,
                Paragraph42,
                BranchAndBoundMulti);
        }
    }
}