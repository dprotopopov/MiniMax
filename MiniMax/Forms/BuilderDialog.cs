using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using MyLibrary.Collections;

namespace MiniMax.Forms
{
    public partial class BuilderDialog : Form
    {
        private readonly MatrixIO _dataGridViewMatrix = new MatrixIO
        {
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            Dock = DockStyle.Fill,
            Name = "_dataGridViewMatrix",
            RowTemplate = {Height = 20},
            TabIndex = 0
        };

        private readonly VectorIO _dataGridViewVector = new VectorIO
        {
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            Dock = DockStyle.Fill,
            Name = "_dataGridViewVector",
            RowTemplate = {Height = 20},
            TabIndex = 0
        };

        public BuilderDialog(Type type)
        {
            Type = type;
            InitializeComponent();
            groupBoxMatrix.Controls.Add(_dataGridViewMatrix);
            groupBoxVector.Controls.Add(_dataGridViewVector);
            DataSource = new BindingList<object>();
        }

        private Type Type { get; set; }

        private BindingList<object> DataSource
        {
            get { return (BindingList<object>) dataGridView1.DataSource; }
            set { dataGridView1.DataSource = value; }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataSource.Clear();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            IEnumerable<object> dataSource = new StackListQueue<object>(DataSource);
            var dataArray = new Builder(Type);
            dataArray.LoadFrom(openFileDialog1.FileName, ref dataSource);
            DataSource = new BindingList<object>(new StackListQueue<object>(dataSource));
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            IEnumerable<object> dataSource = new StackListQueue<object>(DataSource);
            var dataArray = new Builder(Type);
            dataArray.SaveAs(saveFileDialog1.FileName, dataSource);
        }
    }
}