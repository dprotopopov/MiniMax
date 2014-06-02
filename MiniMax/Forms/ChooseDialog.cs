using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using MiniMax.Attributes;
using MyLibrary.Collections;
using MyLibrary.Trace;

namespace MiniMax.Forms
{
    public partial class ChooseDialog : Form, ITrace
    {
        public ChooseDialog(IEnumerable<object> dataEnumerable)
        {
            InitializeComponent();
            dataGridView1.DataSource = dataEnumerable;
        }

        public ChooseDialog(string fileName, Type type)
        {
            Type = type;
            InitializeComponent();
            var dataBuilder = new Builder(type)
            {
                ProgressCallback = ProgressCallback,
                AppendLineCallback = AppendLineCallback,
                CompliteCallback = CompliteCallback
            };
            IEnumerable<object> dataEnumerable = new BindingList<object>(new StackListQueue<object>());
            dataBuilder.LoadFrom(fileName, ref dataEnumerable);
            DataSource = dataEnumerable;
        }

        private Type Type { get; set; }

        public MyLibrary.Collections.Properties Values
        {
            get
            {
                var formula = new Formula(Type);
                return new MyLibrary.Collections.Properties(DataBoundItem,
                    formula.GetProperties(typeof (MiniMaxOutputAttribute)));
            }
        }

        public object DataBoundItem
        {
            get { return dataGridView1.CurrentRow != null ? dataGridView1.CurrentRow.DataBoundItem : null; }
        }

        public IEnumerable<object> DataSource
        {
            get { return (IEnumerable<object>) dataGridView1.DataSource; }
            set { dataGridView1.DataSource = value; }
        }

        public ProgressCallback ProgressCallback { get; set; }
        public AppendLineCallback AppendLineCallback { get; set; }
        public CompliteCallback CompliteCallback { get; set; }
    }
}