using System;
using System.Windows.Forms;

namespace MiniMax.Forms
{
    public partial class EnterDialog : Form
    {
        public EnterDialog(Type type)
        {
            Type = type;
            InitializeComponent();
            SelectedObject = Activator.CreateInstance(type);
        }

        private Type Type { get; set; }

        public object SelectedObject
        {
            get { return propertyGrid1.SelectedObject; }
            set { propertyGrid1.SelectedObject = value; }
        }
    }
}