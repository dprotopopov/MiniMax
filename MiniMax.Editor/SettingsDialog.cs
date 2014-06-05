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

        public bool BranchAndBound
        {
            get { return radioButtonBranchAndBound.Checked; }
            set { radioButtonBranchAndBound.Checked = value; }
        }

        public bool IsValid()
        {
            return Boolean.Xor(Gomory, BranchAndBound);
        }
    }
}