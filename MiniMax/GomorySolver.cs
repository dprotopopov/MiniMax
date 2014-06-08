using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MyLibrary.Collections;
using MyLibrary.Trace;
using MyMath;

namespace MiniMax
{
    /// <summary>
    ///     Алгоритм Гомори
    ///     Пусть дана следующая задача:
    ///     max {C(x)=∑cixi|∑ajixi{≤=≥}bj, j = 1,m, xi≥0 для всех i = 1,n}, (1)
    ///     В частично целочисленных задачах требование целочисленности накладывается не на все переменные,
    ///     а на одну или некоторые из них.
    ///     Одним из методов решения задач линейного целочисленного программирования является метод Гомори. Сущность метода
    ///     заключается в построении ограничений, отсекающих нецелочисленные решения задачи линейного программирования, но не
    ///     отсекающих ни одного целочисленного плана.
    ///     Рассмотрим алгоритм решения задачи линейного целочисленного программирования этим методом.
    ///     1.	Решаем задачу симплексным методом без учета условия целочисленности. Если все компоненты оптимального плана
    ///     целые, то он является оптимальным и для задачи целочисленного программирования. Если обнаруживается неразрешимость
    ///     задачи, то и неразрешима задача целочисленного программирования.
    ///     2.	Если среди компонент оптимального решения есть нецелые, то к ограничениям задачи добавляем новое ограничение,
    ///     обладающее следующими свойствами:
    ///     - оно должно быть линейным;
    ///     - должно отсекать найденный оптимальный нецелочисленный план;
    ///     - не должно отсекать ни одного целочисленного плана.
    ///     Для построения ограничения выбираем компоненту оптимального плана с наибольшей дробной частью и по соответствующей
    ///     этой компоненте k-й строке симплексной таблицы записываем ограничение Гомори.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GomorySolver<T> : ISolver<T>
    {
        private static readonly double Tollerance = 0.01;

        public bool Execute(ILinearMiniMax<T> minimax, ref IEnumerable<Vector<T>> optimalVectors,
            ref IEnumerable<T> optimalValues, ITrace trace)
        {
            Debug.Assert(minimax.A.Rows == minimax.R.Count());
            Debug.Assert(minimax.A.Rows == minimax.B.Count());
            Debug.Assert(minimax.A.Columns == minimax.C.Count());

            int n = Math.Min(minimax.A.Columns, minimax.C.Count()); // Количество переменных в уравнении

            var read = new object();
            var write = new object();

            AppendLineCallback appendLineCallback = trace != null ? trace.AppendLineCallback : null;
            ProgressCallback progressCallback = trace != null ? trace.ProgressCallback : null;
            CompliteCallback compliteCallback = trace != null ? trace.CompliteCallback : null;

            // 1. Составление псевдоплана. 
            // Систему ограничений исходной задачи приводят к системе неравенств смысла >=.
            if (appendLineCallback != null)
                appendLineCallback("Составление псевдоплана");
            Debug.WriteLine("Составление псевдоплана");

            var simplexMatrix = new SimplexMatrix(minimax);

            int count = simplexMatrix.Rows; // == количество независимых переменных + индексная строка
            int other = simplexMatrix.Columns - n; // == количество добавленных переменных + столбец значений
            int basic = simplexMatrix.Columns - simplexMatrix.Rows; // == количество базисных переменных
            int total = simplexMatrix.Columns - 1; // == количество переменных

            var cols = new StackListQueue<int>(Enumerable.Range(1, basic)); // 1-based variable indecies
            var rows = new StackListQueue<int>(Enumerable.Range(basic + 1, count - 1)); // 1-based variable indecies

            Debug.WriteLine(string.Join(Environment.NewLine,
                simplexMatrix.Select(
                    doubles =>
                        string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));

            if (appendLineCallback != null)
                appendLineCallback("Текущий опорный план:");
            if (appendLineCallback != null)
                appendLineCallback(string.Join(Environment.NewLine,
                    simplexMatrix.Select(
                        doubles =>
                            string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
            Debug.WriteLine("Текущий опорный план:");
            Debug.WriteLine(string.Join(Environment.NewLine,
                simplexMatrix.Select(
                    doubles =>
                        string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));

            if (appendLineCallback != null)
                appendLineCallback("Решается задача линейного программирования без учета целочисленности.");
            Debug.WriteLine("Решается задача линейного программирования без учета целочисленности.");
            // Решается задача линейного программирования без учета целочисленности.

            // 2. Проверка плана на оптимальность. 
            // Если в полученном опорном плане не выполняется условие оптимальности,
            // то задача решается симплексным методом.
            // 3. Выбор ведущих строки и столбца. 
            // Среди отрицательных значений базисных переменных
            // выбираются наибольшие по абсолютной величине.
            // 4. Строка, соответствующая этому значению, является ведущей.
            // Расчет нового опорного плана. Новый план получается в результате пересчета симплексной таблицы
            // методом Жордана-Гаусса. Далее переход к этапу 2.

            if (appendLineCallback != null)
                appendLineCallback("Текущий опорный план:");
            if (appendLineCallback != null)
                appendLineCallback(string.Join(Environment.NewLine,
                    simplexMatrix.Select(
                        doubles =>
                            string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
            Debug.WriteLine("Текущий опорный план:");
            Debug.WriteLine(string.Join(Environment.NewLine,
                simplexMatrix.Select(
                    doubles =>
                        string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));

            // 2. Проверка плана на оптимальность.
            for (IndexVector indexRow = simplexMatrix.GetIndexRow();
                !indexRow.IsOptimalRow(cols);
                indexRow = simplexMatrix.GetIndexRow())
            {
                // Текущий опорный план неоптимален, так как в индексной строке находятся отрицательные коэффициенты
                if (appendLineCallback != null)
                    appendLineCallback(
                        "Текущий опорный план неоптимален, так как в индексной строке находятся отрицательные коэффициенты");
                Debug.WriteLine(
                    "Текущий опорный план неоптимален, так как в индексной строке находятся отрицательные коэффициенты");

                // Если в полученном опорном плане не выполняется условие оптимальности,
                // то задача решается симплексным методом.

                // Выбор ведущих строки и столбца.
                if (appendLineCallback != null)
                    appendLineCallback("Выбор ведущих строки и столбца");
                Debug.WriteLine("Выбор ведущих строки и столбца");

                var enumerable = from j in cols
                    where indexRow[j] < -Tollerance
                    from i in Enumerable.Range(1, count - 1)
                    where Math.Abs(simplexMatrix[i][j]) > Tollerance
                    where Math.Abs(simplexMatrix[0][j]*simplexMatrix[i][0]/simplexMatrix[i][j]) > Tollerance
                    select new {row = i, col = j};

                double max = (from pair in enumerable
                    select
                        Math.Abs(simplexMatrix[0][pair.col]*simplexMatrix[pair.row][0]/simplexMatrix[pair.row][pair.col]))
                    .Max();
                var first = (from pair in enumerable
                    where
                        Math.Abs(
                            Math.Abs(simplexMatrix[0][pair.col]*simplexMatrix[pair.row][0]/
                                     simplexMatrix[pair.row][pair.col]) - max) <= Tollerance
                    select pair).First();

                int index1 = first.row;
                int row = rows[index1 - 1];
                int col = first.col;
                int index = cols.IndexOf(col);

                // Ведущей будет строка row, а переменную col следует вывести из базиса.                
                if (appendLineCallback != null)
                    appendLineCallback(string.Format(
                        "Ведущей будет строка {0}(переменная {1}), а переменную {2} следует ввести в базис",
                        index1, row, col));
                Debug.WriteLine(
                    "Ведущей будет строка {0}(переменная {1}), а переменную {2} следует ввести в базис",
                    index1, row, col);

                cols.RemoveAt(index);
                cols.Add(row);
                rows.RemoveAt(index1 - 1);
                rows.Add(col);

                //Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса
                if (appendLineCallback != null)
                    appendLineCallback(
                        "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");
                Debug.WriteLine(
                    "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");

                // Подготовка данных для шага симплекс метода

                var matrix = new double[simplexMatrix.Rows, simplexMatrix.Columns];
                var next = new double[simplexMatrix.Rows, simplexMatrix.Columns];

                Parallel.ForEach(
                    from i in Enumerable.Range(0, simplexMatrix.Rows)
                    from j in Enumerable.Range(0, simplexMatrix.Columns)
                    select new {row = i, col = j}, pair =>
                    {
                        int i = pair.row;
                        int j = pair.col;
                        double x;
                        lock (read) x = simplexMatrix[i][j];
                        lock (write) matrix[i, j] = x;
                    });

                Matrix<double>.GaussJordanStep(matrix, next, index1, col);

                double[,] t = matrix;
                matrix = next;
                next = t;

                // Считывание данных из симплекс-метода
                Parallel.ForEach(
                    from i in Enumerable.Range(0, simplexMatrix.Rows)
                    from j in Enumerable.Range(0, simplexMatrix.Columns)
                    select new {row = i, col = j}, pair =>
                    {
                        int i = pair.row;
                        int j = pair.col;
                        double x;
                        lock (read) x = matrix[i, j];
                        lock (write) simplexMatrix[i][j] = x;
                    });

                simplexMatrix.Add(simplexMatrix[index1]);
                simplexMatrix.RemoveAt(index1);

                if (appendLineCallback != null)
                    appendLineCallback("Текущий опорный план:");
                if (appendLineCallback != null)
                    appendLineCallback(string.Join(Environment.NewLine,
                        simplexMatrix.Select(
                            doubles =>
                                string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(string.Join(Environment.NewLine,
                    simplexMatrix.Select(
                        doubles =>
                            string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
                Debug.WriteLine(string.Join(",", rows.Select(item => item.ToString(CultureInfo.InvariantCulture))) +
                                "x" +
                                string.Join(",", cols.Select(item => item.ToString(CultureInfo.InvariantCulture))));
            }

            // Среди значений индексной строки нет отрицательных. 
            // Поэтому эта таблица определяет оптимальный план задачи.


            // В результате решения задачи с отброшенным условием целочисленности 
            // получена оптимальная симплекс-таблица и переменной xk 
            // соответствует строка базисной переменной vk этой таблицы. 

            for (IndexVector indexColumn = simplexMatrix.GetIndexColumn();
                indexColumn.HasColumnWithFraction(rows, n);
                indexColumn = simplexMatrix.GetIndexColumn())
            {
                // В полученном оптимальном плане переменная имеет дробную часть числа. 
                if (appendLineCallback != null)
                    appendLineCallback("В полученном оптимальном плане переменная имеет дробную часть числа.");
                Debug.WriteLine("В полученном оптимальном плане переменная имеет дробную часть числа.");

                if (appendLineCallback != null)
                    appendLineCallback(
                        "Составляется дополнительное ограничение для переменной, которая в оптимальном плане имеет максимальное дробное значение, хотя должна быть целой.");
                Debug.WriteLine(
                    "Составляется дополнительное ограничение для переменной, которая в оптимальном плане имеет максимальное дробное значение, хотя должна быть целой.");

                // Метод Гомори.
                // Составляется дополнительное ограничение для переменной, 
                // которая в оптимальном плане имеет максимальное дробное значение, хотя должна быть целой. 

                // Находим переменную с максимальной дробной частью
                if (appendLineCallback != null)
                    appendLineCallback("Находим переменную с максимальной дробной частью");
                Debug.WriteLine("Находим переменную с максимальной дробной частью");
                var indexWithFraction =
                    new StackListQueue<int>(
                        Enumerable.Range(1, n)
                            .Where(rows.Contains)
                            .Select(i => rows.IndexOf(i) + 1)
                            .Where(i => Math.Ceiling(indexColumn[i]) - Math.Floor(indexColumn[i]) > Tollerance));

                var fractions =
                    new StackListQueue<double>(
                        indexWithFraction.Select(i => Math.Abs(indexColumn[i] - Math.Floor(indexColumn[i]))));
                int k = indexWithFraction[fractions.IndexOf(fractions.Max())]; // 0-based variable index 

                Debug.WriteLine(Math.Abs(indexColumn[k] - Math.Floor(indexColumn[k])));

                // Дополнительное ограничение составляем по строке k.
                if (appendLineCallback != null)
                    appendLineCallback(string.Format("Дополнительное ограничение составляем по строке {0}", k));
                Debug.WriteLine("Дополнительное ограничение составляем по строке {0}", k);

                total++;
                count++;
                other++;

                foreach (var r in simplexMatrix) r.Add(0);

                var vector = new Vector<double>(simplexMatrix[k].Select(a => Math.Floor(a) - a));
                vector[total] = 1;
                simplexMatrix.Add(vector);

                rows.Add(total); // 1-based variable indecies

                if (appendLineCallback != null)
                    appendLineCallback("Текущий опорный план:");
                if (appendLineCallback != null)
                    appendLineCallback(string.Join(Environment.NewLine,
                        simplexMatrix.Select(
                            doubles =>
                                string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(string.Join(Environment.NewLine,
                    simplexMatrix.Select(
                        doubles =>
                            string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
                Debug.WriteLine(string.Join(",", rows.Select(item => item.ToString(CultureInfo.InvariantCulture))) +
                                "x" +
                                string.Join(",", cols.Select(item => item.ToString(CultureInfo.InvariantCulture))));

                var enumerable = from j in cols
                    where Math.Abs(simplexMatrix[count - 1][j]) > Tollerance
                    where
                        Math.Abs(simplexMatrix[0][j]*simplexMatrix[count - 1][0]/simplexMatrix[count - 1][j]) >
                        Tollerance
                    select new {row = count - 1, col = j};
                double max = (from pair in enumerable
                    select
                        Math.Abs(simplexMatrix[0][pair.col]*simplexMatrix[pair.row][0]/
                                 simplexMatrix[pair.row][pair.col]))
                    .Max();
                var first = (from pair in enumerable
                    where
                        Math.Abs(
                            Math.Abs(simplexMatrix[0][pair.col]*simplexMatrix[pair.row][0]/
                                     simplexMatrix[pair.row][pair.col]) - max) <= Tollerance
                    select pair).First();

                int index1 = first.row;
                int row = rows[index1 - 1];
                int col = first.col;
                int index = cols.IndexOf(col);

                // Ведущей будет строка row, а переменную col следует вывести из базиса.                
                if (appendLineCallback != null)
                    appendLineCallback(string.Format(
                        "Ведущей будет строка {0}(переменная {1}), а переменную {2} следует ввести в базис",
                        index1, row, col));
                Debug.WriteLine(
                    "Ведущей будет строка {0}(переменная {1}), а переменную {2} следует ввести в базис",
                    index1, row, col);

                cols.RemoveAt(index);
                cols.Add(row);
                rows.RemoveAt(index1 - 1);
                rows.Add(col);

                //Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса
                if (appendLineCallback != null)
                    appendLineCallback(
                        "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");
                Debug.WriteLine(
                    "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");

                // Подготовка данных для шага симплекс метода

                var matrix = new double[simplexMatrix.Rows, simplexMatrix.Columns];
                var next = new double[simplexMatrix.Rows, simplexMatrix.Columns];

                Parallel.ForEach(
                    from i in Enumerable.Range(0, simplexMatrix.Rows)
                    from j in Enumerable.Range(0, simplexMatrix.Columns)
                    select new {row = i, col = j}, pair =>
                    {
                        int i = pair.row;
                        int j = pair.col;
                        double x;
                        lock (read) x = simplexMatrix[i][j];
                        lock (write) matrix[i, j] = x;
                    });

                Matrix<double>.GaussJordanStep(matrix, next, index1, col);

                double[,] t = matrix;
                matrix = next;
                next = t;

                // Считывание данных из симплекс-метода
                Parallel.ForEach(
                    from i in Enumerable.Range(0, simplexMatrix.Rows)
                    from j in Enumerable.Range(0, simplexMatrix.Columns)
                    select new {row = i, col = j}, pair =>
                    {
                        int i = pair.row;
                        int j = pair.col;
                        double x;
                        lock (read) x = matrix[i, j];
                        lock (write) simplexMatrix[i][j] = x;
                    });

                simplexMatrix.Add(simplexMatrix[index1]);
                simplexMatrix.RemoveAt(index1);

                if (appendLineCallback != null)
                    appendLineCallback("Текущий опорный план:");
                if (appendLineCallback != null)
                    appendLineCallback(string.Join(Environment.NewLine,
                        simplexMatrix.Select(
                            doubles =>
                                string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(string.Join(Environment.NewLine,
                    simplexMatrix.Select(
                        doubles =>
                            string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture))))));
                Debug.WriteLine(string.Join(",", rows.Select(item => item.ToString(CultureInfo.InvariantCulture))) +
                                "x" +
                                string.Join(",", cols.Select(item => item.ToString(CultureInfo.InvariantCulture))));
            }

            var optimal =
                new Vector<double>(
                    Enumerable.Range(1, total)
                        .Select(index => rows.Contains(index) ? simplexMatrix[1 + rows.IndexOf(index)].First() : 0.0));
            double value = simplexMatrix.First().First()*(double) minimax.Target;
            if (appendLineCallback != null)
                appendLineCallback("Оптимальный план можно записать так:");
            if (appendLineCallback != null)
                appendLineCallback(string.Join(",", optimal.Select(x => x.ToString(CultureInfo.InvariantCulture))));
            if (appendLineCallback != null)
                appendLineCallback(value.ToString(CultureInfo.InvariantCulture));
            Debug.WriteLine("Оптимальный план можно записать так:");
            Debug.WriteLine(string.Join(",", optimal.Select(x => x.ToString(CultureInfo.InvariantCulture))));
            Debug.WriteLine(value.ToString(CultureInfo.InvariantCulture));


            // Завершаем алгоритм и возвращаем найденное решение
            if (appendLineCallback != null)
                appendLineCallback("Завершаем алгоритм и возвращаем найденное решение");
            Debug.WriteLine("Завершаем алгоритм и возвращаем найденное решение");
            optimalVectors =
                new StackListQueue<Vector<T>>(new Vector<T>(optimal.GetRange(0, n).Select(x => (T) (dynamic) x)));
            optimalValues = new StackListQueue<T>((T) (dynamic) value);
            if (compliteCallback != null) compliteCallback();
            return true;
        }

        private class IndexVector : Vector<double>
        {
            public IndexVector(double value)
                : base(value)
            {
            }

            public IndexVector(IEnumerable<double> vector)
                : base(vector)
            {
            }

            public bool IsOptimalRow(IEnumerable<int> columns)
            {
                return columns.All(index => this[index] >= -Tollerance);
            }

            public bool HasColumnWithFraction(IEnumerable<int> rows, int n)
            {
                var list = new StackListQueue<int>(rows);
                var list1 = new StackListQueue<int>(
                    Enumerable.Range(1, n)
                        .Where(list.Contains)
                        .Select(i => list.IndexOf(i) + 1));
                return list1.Any(i => Math.Abs(this[i] - Math.Floor(this[i])) > Tollerance);
            }
        }

        private class SimplexMatrix : Matrix<double>
        {
            /// <summary>
            ///     Создание матрицы плана
            ///     Утверждение. Любая общая задача ЛП может быть приведена к канонической форме.
            ///     Приведение общей задачи ЛП к канонической форме достигается путем введения новых
            ///     (их называют дополнительными) переменных.
            /// </summary>
            public SimplexMatrix(ILinearMiniMax<T> minimax)
            {
                Debug.Assert(minimax.A.Rows == minimax.R.Count());
                Debug.Assert(minimax.A.Rows == minimax.B.Count());
                Debug.Assert(minimax.A.Columns == minimax.C.Count());

                int n = Math.Min(minimax.A.Columns, minimax.C.Count());
                int count = Math.Min(minimax.A.Count(), minimax.B.Count());
                Debug.WriteLine("count = " + count);

                // Приведем функцию к цели min, 
                // умножив на (-1) если указана цель max
                var vector = new Vector<double>(0);
                vector.Add(minimax.C.Select(x => -Convert.ToDouble(x)*(double) minimax.Target));
                vector.Add(Enumerable.Repeat(0.0, count));

                Add(vector);

                for (int i = 0; i < count; i++)
                {
                    // Добавим положительные переменные
                    // и приведем систему ограничений к системе неравенств смысла <=, 
                    // умножив соответствующие строки на (-1).
                    var value =
                        new Vector<double>(Convert.ToDouble(minimax.B.ElementAt(i)));
                    value.Add(minimax.A.ElementAt(i).Select(x => Convert.ToDouble(x)));
                    value.Add(Enumerable.Repeat(0.0, count));
                    value[n + i + 1] = -(double) minimax.R.ElementAt(i);

                    Add(value);
                }
            }

            public IndexVector GetIndexColumn()
            {
                return new IndexVector(this.Select(Enumerable.First));
            }

            public IndexVector GetIndexRow()
            {
                return new IndexVector(this.First());
            }
        }
    }
}