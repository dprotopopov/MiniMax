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
    ///     Алгоритм Гомори для частично целочисленной задачи линейного программирования
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
        public bool Execute(ILinearMiniMax<T> minimax, ref IEnumerable<Vector<T>> optimalVectors,
            ref IEnumerable<T> optimalValues, ITrace trace)
        {
            Debug.Assert(minimax.A.Rows == minimax.R.Count());
            Debug.Assert(minimax.A.Rows == minimax.B.Count());
            Debug.Assert(minimax.A.Columns == minimax.C.Count());

            int n = Math.Min(minimax.A.Columns, minimax.C.Count()); // Количество переменных в уравнении

            AppendLineCallback appendLineCallback = trace != null ? trace.AppendLineCallback : null;
            ProgressCallback progressCallback = trace != null ? trace.ProgressCallback : null;
            CompliteCallback compliteCallback = trace != null ? trace.CompliteCallback : null;

            // 1. Составление псевдоплана. 
            // Систему ограничений исходной задачи приводят к системе неравенств смысла >=.
            if (appendLineCallback != null)
                appendLineCallback("Составление псевдоплана");
            Debug.WriteLine("Составление псевдоплана");

            var simplexMatrix = new SimplexMatrix(minimax);

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

            while (true)
            {
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

                int count = simplexMatrix.Rows; // == количество независимых переменных + индексная строка
                int other = simplexMatrix.Columns - n; // == количество добавленных переменных

                // Подготовка данных для шага симплекс метода
                if (appendLineCallback != null)
                    appendLineCallback("Подготовка данных для симплекс метода");
                Debug.WriteLine("Подготовка данных для симплекс метода");

                var matrix = new double[simplexMatrix.Rows, simplexMatrix.Columns];
                var next = new double[simplexMatrix.Rows, simplexMatrix.Columns];

                var read = new object();
                var write = new object();

                Parallel.ForEach(
                    from i in Enumerable.Range(0, simplexMatrix.Rows)
                    from j in Enumerable.Range(0, simplexMatrix.Columns)
                    select new[] {i, j}, pair =>
                    {
                        int i = pair[0];
                        int j = pair[1];
                        double x;
                        lock (read) x = simplexMatrix[i][j];
                        lock (write) matrix[i, j] = x;
                    });

                var rows = new StackListQueue<int>(Enumerable.Range(1 + n + other - count, count - 1));
                // 1-based variable indecies
                var cols = new StackListQueue<int>(Enumerable.Range(1, n)); // 1-based variable indecies

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
                for (IndexVector indexVector = simplexMatrix.GetIndexVector();
                    !indexVector.IsOptimal(rows);
                    indexVector = simplexMatrix.GetIndexVector())
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

                    IEnumerable<int> list = from j in Enumerable.Range(1, count - 1)
                        where simplexMatrix[j][0] < 0.0
                        select j;
                    double max = (from j in list
                        select Math.Abs(simplexMatrix[j][0])).Max();
                    int index1 = (from j in list
                        where Math.Abs(Math.Abs(simplexMatrix[j][0]) - max) <= 0.0
                        select j).First();
                    int row = rows[index1 - 1];

                    IEnumerable<int> list1 = from i in cols
                        where Math.Abs(simplexMatrix[0][i]) > 0.0
                        select i;
                    double max1 = (from i in list1
                        select Math.Abs(simplexMatrix[index1][i]/simplexMatrix[0][i])).Max();
                    int col = (from i in list1
                        where Math.Abs(Math.Abs(simplexMatrix[index1][i]/simplexMatrix[0][i]) - max1) <= 0.0
                        select i).First();
                    int index = cols.IndexOf(col);

                    // Ведущей будет строка row, а переменную col следует вывести из базиса.                
                    if (appendLineCallback != null)
                        appendLineCallback(string.Format(
                            "Ведущей будет строка {0}(переменная {1}), а переменную {2} следует вывести из базиса",
                            index1, row, col));
                    Debug.WriteLine(
                        "Ведущей будет строка {0}(переменная {1}), а переменную {2} следует вывести из базиса",
                        index1, row, col);

                    cols[index] = row;
                    rows[index1 - 1] = col;

                    //Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса
                    if (appendLineCallback != null)
                        appendLineCallback(
                            "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");
                    Debug.WriteLine(
                        "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");

                    Matrix<double>.GaussJordanStep(matrix, next, index1, col);

                    double[,] t = matrix;
                    matrix = next;
                    next = t;

                    // Считывание данных из симплекс-метода
                    Parallel.ForEach(
                        from i in Enumerable.Range(0, simplexMatrix.Rows)
                        from j in Enumerable.Range(0, simplexMatrix.Columns)
                        select new[] {i, j}, pair =>
                        {
                            int i = pair[0];
                            int j = pair[1];
                            double x;
                            lock (read) x = matrix[i, j];
                            lock (write) simplexMatrix[i][j] = x;
                        });

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
                }

                // Среди значений индексной строки нет отрицательных. 
                // Поэтому эта таблица определяет оптимальный план задачи.

                // Оптимальный план можно записать так: 

                var optimal =
                    new Vector<double>(
                        Enumerable.Range(1, n)
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

                // Проверяем на дробные части
                if (appendLineCallback != null)
                    appendLineCallback("Проверяем на дробные части");
                Debug.WriteLine("Проверяем на дробные части");
                var indexWithFraction =
                    new StackListQueue<int>(Enumerable.Range(0, n)
                        .Where(i => Math.Abs(Math.Floor(optimal[i]) - optimal[i]) > Math.Abs(0.0*optimal[i])));

                if (!indexWithFraction.Any())
                {
                    // Завершаем алгоритм и возвращаем найденное решение
                    if (appendLineCallback != null)
                        appendLineCallback("Завершаем алгоритм и возвращаем найденное решение");
                    Debug.WriteLine("Завершаем алгоритм и возвращаем найденное решение");
                    optimalVectors = new StackListQueue<Vector<T>>(new Vector<T>(optimal.Select(x => (T) (dynamic) x)));
                    optimalValues = new StackListQueue<T>((T) (dynamic) value);
                    if (compliteCallback != null) compliteCallback();
                    return true;
                }

                // В полученном оптимальном плане переменная имеет дробную часть числа. 
                if (appendLineCallback != null)
                    appendLineCallback("В полученном оптимальном плане переменная имеет дробную часть числа.");
                Debug.WriteLine("В полученном оптимальном плане переменная имеет дробную часть числа.");

                // В результате решения задачи с отброшенным условием целочисленности 
                // получена оптимальная симплекс-таблица и переменной xk 
                // соответствует строка базисной переменной vk этой таблицы. 

                // Метод Гомори.
                // Составляется дополнительное ограничение для переменной, 
                // которая в оптимальном плане имеет максимальное дробное значение, хотя должна быть целой. 
                if (appendLineCallback != null)
                    appendLineCallback(
                        "Составляется дополнительное ограничение для переменной, которая в оптимальном плане имеет максимальное дробное значение, хотя должна быть целой.");
                Debug.WriteLine(
                    "Составляется дополнительное ограничение для переменной, которая в оптимальном плане имеет максимальное дробное значение, хотя должна быть целой.");

                // Находим переменную с максимальной дробной частью
                var fractions =
                    new StackListQueue<double>(
                        indexWithFraction.Select(i => Math.Abs(optimal[i] - Math.Floor(optimal[i]))));
                int k = indexWithFraction[fractions.IndexOf(fractions.Max())]; // 0-based variable index 

                // Дополнительное ограничение составляем по строке, соответствующей переменной k.
                if (appendLineCallback != null)
                    appendLineCallback(
                        string.Format(
                            "Дополнительное ограничение составляем по строке, соответствующей переменной {0}", k));
                Debug.WriteLine("Дополнительное ограничение составляем по строке, соответствующей переменной {0}", k);

                //Выделим в βk целую и дробную часть и преобразуем (2) к виду
                double b = optimal[k];
                double f = b - Math.Floor(b);

                foreach (var r in simplexMatrix) r.Add(0);

                var vector = new Vector<double>(f);
                vector.Add(optimal.Select(a => (a >= 0.0) ? a : (a*f/(f - 1))));
                // где I+ – множество значений i, для которых αki > 0 ; I- – множество значений i , для которых αki< 0.                
                vector.Add(Enumerable.Repeat(0.0, other - 1));
                vector.Add(1);
                simplexMatrix.Add(vector);

                cols.Add(n + other + 1); // 1-based variable indecies
            }
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

            public bool IsOptimal(IEnumerable<int> rows)
            {
                return GetRange(1, Count - 1).All(x => x >= 0.0);
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
                var vector = new Vector<double>(0) // значение целевой функции в точке 0
                    ;
                vector.Add(minimax.C.Select(x => -Convert.ToDouble(x)*(double) minimax.Target));
                vector.Add(Enumerable.Repeat(0.0, count));

                Add(vector);

                for (int i = 0; i < count; i++)
                {
                    // Добавим положительные переменные
                    // и приведем систему ограничений к системе неравенств смысла <=, 
                    // умножив соответствующие строки на (-1).
                    var value =
                        new Vector<double>(-Convert.ToDouble(minimax.B.ElementAt(i))*(double) minimax.R.ElementAt(i));
                    value.Add(minimax.A.ElementAt(i).Select(x => -Convert.ToDouble(x)*(double) minimax.R.ElementAt(i)));
                    value.Add(Enumerable.Repeat(0.0, count));
                    value[n + i + 1] = 1;

                    Add(value);
                }
            }

            public ValueVector GetValueVector()
            {
                return new ValueVector(this.Select(Enumerable.First));
            }

            public IndexVector GetIndexVector()
            {
                return new IndexVector(this.Select(Enumerable.First));
            }
        }

        private class ValueVector : Vector<double>
        {
            public ValueVector(IEnumerable<double> vector)
                : base(vector)
            {
            }
        }
    }
}