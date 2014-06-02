using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using MiniMax.Attributes;
using MyLibrary.Collections;
using MyLibrary.Trace;

namespace MiniMax.Forms
{
    public partial class BuildChooseDialog : Form, ITrace
    {
        public BuildChooseDialog(Type type)
        {
            InitializeComponent();
            Type = type;
            dynamic constant = Activator.CreateInstance(type);
            dynamic list = new BindingList<object>(new StackListQueue<object>());
            SelectedObject = constant;
            DataSource = list;
            Debug.Assert(SelectedObject != null);
            Debug.Assert(DataSource != null);
            var formula = new Formula(type);
            IEnumerable<PropertyInfo> props = formula.GetProperties(typeof (MiniMaxConstantAttribute));
            ProgressCallback = Progress;
        }

        private Type Type { get; set; }

        public object DataBoundItem
        {
            get { return dataGridView1.CurrentRow != null ? dataGridView1.CurrentRow.DataBoundItem : null; }
        }

        public MyLibrary.Collections.Properties Values
        {
            get
            {
                var formula = new Formula(Type);
                return new MyLibrary.Collections.Properties(DataBoundItem,
                    formula.GetProperties(typeof (MiniMaxOutputAttribute)));
            }
        }

        public object SelectedObject
        {
            get { return propertyGrid1.SelectedObject; }
            set { propertyGrid1.SelectedObject = value; }
        }

        public IEnumerable<object> DataSource
        {
            get { return (IEnumerable<object>) dataGridView1.DataSource; }
            set { dataGridView1.DataSource = value; }
        }

        private void buttonRebuild_Click(object sender, EventArgs e)
        {
            buttonRebuild.Enabled = false;
            buttonOpen.Enabled = false;
            buttonSave.Enabled = false;
            var dataBuilder = new Builder(Type)
            {
                ProgressCallback = ProgressCallback,
                AppendLineCallback = AppendLineCallback,
                CompliteCallback = CompliteCallback
            };
            IEnumerable<object> dataEnumerable = new StackListQueue<object>();
            dataBuilder.BuildOptionTable(ref dataEnumerable, SelectedObject);
            DataSource = dataEnumerable;
            buttonRebuild.Enabled = true;
            buttonOpen.Enabled = true;
            buttonSave.Enabled = true;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            IEnumerable<object> dataSource = new StackListQueue<object>(DataSource);
            var dataArray = new Builder(Type)
            {
                ProgressCallback = ProgressCallback,
                AppendLineCallback = AppendLineCallback,
                CompliteCallback = CompliteCallback
            };
            dataArray.LoadFrom(openFileDialog1.FileName, ref dataSource);
            DataSource = new BindingList<object>(new StackListQueue<object>(dataSource));
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;
            IEnumerable<object> dataSource = new StackListQueue<object>(DataSource);
            var dataArray = new Builder(Type)
            {
                ProgressCallback = ProgressCallback,
                AppendLineCallback = AppendLineCallback,
                CompliteCallback = CompliteCallback
            };
            dataArray.SaveAs(saveFileDialog1.FileName, dataSource);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            okButton.Enabled = dataGridView1.CurrentRow != null;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            okButton.Enabled = dataGridView1.CurrentRow != null;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow item = dataGridView1.CurrentRow;
            if (item == null) return;
            Close();
        }

        void Progress(long current, long total)
        {
            Debug.Assert(current <= total);
            if (progressBar1.InvokeRequired)
            {
                ProgressCallback d = Progress;
                object[] objects = { current, total };
                Invoke(d, objects);
            }
            else
            {
                progressBar1.Maximum = (int)Math.Min(total, 10000);
                progressBar1.Value = (int)(current * progressBar1.Maximum / (1 + total));
                Application.DoEvents();
            }
        }
        public ProgressCallback ProgressCallback { get; set; }
        public AppendLineCallback AppendLineCallback { get; set; }
        public CompliteCallback CompliteCallback { get; set; }
    }
}