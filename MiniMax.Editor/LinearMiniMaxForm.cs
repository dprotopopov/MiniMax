using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniMax.Editor.Properties;
using MyLibrary.Trace;
using MyMath;
using Double = MyLibrary.Types.Double;

namespace MiniMax.Editor
{
    public partial class LinearMiniMaxForm : Form, ILinearMiniMax<double>, ITrace
    {
        private static readonly Random Rnd = new Random();

        public LinearMiniMaxForm(int restrictions, int variables, int targets = 1)
        {
            Debug.Assert(targets == 1);

            Variables = variables;
            Restrictions = restrictions;
            Targets = targets;
            InitializeComponent();
            groupBoxSystem.Controls.Add(DataGridViewSystem = new MiniMaxSystemIO(restrictions, variables)
            {
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Dock = DockStyle.Fill,
                Name = "_dataGridViewSystem",
                RowTemplate = {Height = 20},
                TabIndex = 0
            });
            groupBoxSolution.Controls.Add(DataGridViewSolution = new MiniMaxSolutionIO(1, variables)
            {
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Dock = DockStyle.Fill,
                Name = "_dataGridViewSolution",
                RowTemplate = {Height = 20},
                TabIndex = 0
            });

            A = new Matrix<double>(
                Enumerable.Range(0, restrictions).Select(r => new Vector<double>(Enumerable.Repeat(0.0, variables))));
            B = new Vector<double>(Enumerable.Repeat(0.0, restrictions));
            C = new Vector<double>(Enumerable.Repeat(0.0, variables));
            R = new Vector<CompareOperand>(Enumerable.Repeat(CompareOperand.Ge, restrictions));
            Target = Target.Maximum;

            string[,] theData = DataGridViewSystem.TheData;
            var read = new object();
            var write = new object();
            theData[0, variables] = Target == Target.Maximum ? "max" : Target == Target.Minimum ? "min" : "";
            Parallel.ForEach(Enumerable.Range(0, variables),
                index =>
                {
                    double c;
                    lock (read) c = C[index];
                    string s = c.ToString(CultureInfo.InvariantCulture);
                    lock (write) theData[0, index] = s;
                });
            Parallel.ForEach(
                from i in Enumerable.Range(0, restrictions)
                from j in Enumerable.Range(0, variables)
                select new {row = i, col = j}, pair =>
                {
                    int i = pair.row;
                    int j = pair.col;
                    double a;
                    lock (read) a = A[i][j];
                    string s = a.ToString(CultureInfo.InvariantCulture);
                    lock (write) theData[i + targets, j] = s;
                });
            Parallel.ForEach(Enumerable.Range(0, restrictions), index =>
            {
                CompareOperand r;
                double b;
                lock (read) r = R[index];
                lock (read) b = B[index];
                string sr = r == CompareOperand.Ge ? ">=" : r == CompareOperand.Le ? "<=" : "==";
                string sb = b.ToString(CultureInfo.InvariantCulture);
                lock (write) theData[index + targets, variables] = sr;
                lock (write) theData[index + targets, variables + 1] = sb;
            });
            DataGridViewSystem.TheData = theData;
            AppendLineCallback = AppendLine;
        }

        public MiniMaxSystemIO DataGridViewSystem { get; set; }
        public MiniMaxSolutionIO DataGridViewSolution { get; set; }
        public int Variables { get; set; }
        public int Restrictions { get; set; }
        public int Targets { get; set; }
        public Matrix<double> A { get; set; }
        public Vector<double> B { get; set; }
        public Vector<CompareOperand> R { get; set; }
        public Vector<double> C { get; set; }
        public Target Target { get; set; }

        public ProgressCallback ProgressCallback { get; set; }
        public AppendLineCallback AppendLineCallback { get; set; }
        public CompliteCallback CompliteCallback { get; set; }

        public bool IsValid()
        {
            string[,] theData = DataGridViewSystem.TheData;
            int variables = Variables;
            int restrictions = Restrictions;
            int targets = Targets;
            return (string.CompareOrdinal(theData[0, variables], "min") == 0 ||
                    string.CompareOrdinal(theData[0, variables], "max") == 0) &&
                   Enumerable.Range(targets, restrictions).All(row =>
                       (string.CompareOrdinal(theData[row, variables], ">=") == 0 ||
                        string.CompareOrdinal(theData[row, variables], "<=") == 0));
        }

        public void Solve(ISolver<double> solver)
        {
            string[,] theData = DataGridViewSystem.TheData;
            int variables = Variables;
            int restrictions = Restrictions;
            int targets = Targets;

            var read = new object();
            var write = new object();
            string st = theData[0, variables];
            Target = string.CompareOrdinal(st, "min") == 0 ? Target.Minimum : Target.Maximum;
            Parallel.ForEach(Enumerable.Range(0, variables),
                index =>
                {
                    string s;
                    lock (read) s = theData[0, index];
                    double c = Double.ParseAsString(s.Replace('.', ','));
                    lock (write) C[index] = c;
                });
            Parallel.ForEach(
                from i in Enumerable.Range(targets, restrictions)
                from j in Enumerable.Range(0, variables)
                select new {row = i, col = j}, pair =>
                {
                    int i = pair.row;
                    int j = pair.col;
                    string s;
                    lock (read) s = theData[i, j];
                    double a = Double.ParseAsString(s.Replace('.', ','));
                    lock (write) A[i - targets][j] = a;
                });
            Parallel.ForEach(Enumerable.Range(targets, restrictions), i =>
            {
                string sr;
                string sb;
                lock (read) sr = theData[i, variables];
                lock (read) sb = theData[i, variables + 1];
                CompareOperand r = string.CompareOrdinal(sr, ">=") == 0
                    ? CompareOperand.Ge
                    : string.CompareOrdinal(sr, "<=") == 0
                        ? CompareOperand.Le
                        : CompareOperand.Eq;
                double b = Double.ParseAsString(sb.Replace('.', ','));
                lock (write) R[i - targets] = r;
                lock (write) B[i - targets] = b;
            });

            IEnumerable<Vector<double>> vectors = null;
            IEnumerable<double> values = null;

            bool solved = solver.Execute(this, ref vectors, ref values, this);

            if (solved)
            {
                var theSolution = new string[vectors.Count(), variables + 2];
                Parallel.ForEach(
                    from i in Enumerable.Range(0, vectors.Count())
                    from j in Enumerable.Range(0, variables)
                    select new {row = i, col = j}, pair =>
                    {
                        int i = pair.row;
                        int j = pair.col;
                        double x;
                        lock (read) x = vectors.ElementAt(i)[j];
                        string s = x.ToString(CultureInfo.InvariantCulture);
                        lock (write) theSolution[i, j] = s;
                    });
                Parallel.ForEach(Enumerable.Range(0, vectors.Count()), i =>
                {
                    double x;
                    lock (read) x = values.ElementAt(i);
                    string s = x.ToString(CultureInfo.InvariantCulture);
                    lock (write) theSolution[i, variables] = string.Empty;
                    lock (write) theSolution[i, variables + 1] = s;
                });
                DataGridViewSolution.TheData = theSolution;
            }
            else
            {
                MessageBox.Show(Resources.SystemHasNoSolution);
            }
        }

        private void AppendLine(string line)
        {
            textBox1.AppendText(string.Format("{0}\t{1}{2}", DateTime.Now, line, Environment.NewLine));
        }

        public void Random(double minimum, double maximum)
        {
            var theData = new string[Targets + Restrictions, Variables + 2];
            var read = new object();
            var write = new object();
            theData[0, Variables] = new[] {"max", "min"}[Rnd.Next()%2];
            Parallel.ForEach(Enumerable.Range(0, Variables),
                index =>
                {
                    double c;
                    lock (read) c = minimum + (maximum - minimum)*Rnd.NextDouble();
                    string s = c.ToString(CultureInfo.InvariantCulture);
                    lock (write) theData[0, index] = s;
                });
            Parallel.ForEach(
                from i in Enumerable.Range(Targets, Restrictions)
                from j in Enumerable.Range(0, Variables)
                select new {row = i, col = j}, pair =>
                {
                    int i = pair.row;
                    int j = pair.col;
                    double a;
                    lock (read) a = minimum + (maximum - minimum)*Rnd.NextDouble();
                    string s = a.ToString(CultureInfo.InvariantCulture);
                    lock (write) theData[i, j] = s;
                });
            Parallel.ForEach(Enumerable.Range(Targets, Restrictions), index =>
            {
                double b;
                string sr;
                lock (read) sr = new[] {"<=", ">="}[Rnd.Next()%2];
                lock (read) b = minimum + (maximum - minimum)*Rnd.NextDouble();
                string sb = b.ToString(CultureInfo.InvariantCulture);
                lock (write) theData[index, Variables] = sr;
                lock (write) theData[index, Variables + 1] = sb;
            });
            DataGridViewSystem.TheData = theData;
        }

        public void Simple()
        {
            var theData = new string[Targets + Restrictions, Variables + 2];
            var write = new object();
            theData[0, Variables] = "min";
            Parallel.ForEach(Enumerable.Range(0, Variables),
                index => { lock (write) theData[0, index] = "1"; });
            Parallel.ForEach(
                from i in Enumerable.Range(Targets, Restrictions)
                from j in Enumerable.Range(0, Variables)
                select new {row = i, col = j}, pair =>
                {
                    int i = pair.row;
                    int j = pair.col;
                    lock (write) theData[i, j] = ((i + 1)%(j + 1) == 0 || (j + 1)%(i + 1) == 0) ? "1" : "0";
                });
            Parallel.ForEach(Enumerable.Range(Targets, Restrictions), index =>
            {
                lock (write) theData[index, Variables] = "<=";
                lock (write) theData[index, Variables + 1] = (Variables>>1).ToString();
            });
            DataGridViewSystem.TheData = theData;
        }
    }
}