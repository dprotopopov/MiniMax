using System;
using System.Windows.Forms;

namespace MiniMax.Editor
{
    public partial class MiniMaxCreateDialog : Form
    {
        public MiniMaxCreateDialog()
        {
            InitializeComponent();
        }

        public int Variables
        {
            get { return Convert.ToInt32(numericUpDownVariables.Value); }
            set { numericUpDownVariables.Value = value; }
        }

        public int Restrictions
        {
            get { return Convert.ToInt32(numericUpDownRestrictions.Value); }
            set { numericUpDownRestrictions.Value = value; }
        }
    }
}