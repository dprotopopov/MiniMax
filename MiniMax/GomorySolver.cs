using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        private static readonly double Tolerance = 0.00000000001;

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

            var simplexMatrix = new SimplexMatrix
            {
                AppendLineCallback = appendLineCallback,
                CompliteCallback = compliteCallback,
                ProgressCallback = progressCallback,
            };

            //     1.	Решаем задачу симплексным методом без учета условия целочисленности. Если все компоненты оптимального плана
            //     целые, то он является оптимальным и для задачи целочисленного программирования. Если обнаруживается неразрешимость
            //     задачи, то и неразрешима задача целочисленного программирования.

            if (appendLineCallback != null)
                appendLineCallback("Решается задача линейного программирования без учета целочисленности.");
            Debug.WriteLine("Решается задача линейного программирования без учета целочисленности.");

            bool b = simplexMatrix.DoubleSimplexMethod(minimax);
            if (!b) return false;

            if (appendLineCallback != null) appendLineCallback("Текущий опорный план:");
            if (appendLineCallback != null) appendLineCallback(simplexMatrix.ToString());
            Debug.WriteLine("Текущий опорный план:");
            Debug.WriteLine(simplexMatrix.ToString());


            //     2.	Если среди компонент оптимального решения есть нецелые, то к ограничениям задачи добавляем новое ограничение,
            //     обладающее следующими свойствами:
            //     - оно должно быть линейным;
            //     - должно отсекать найденный оптимальный нецелочисленный план;
            //     - не должно отсекать ни одного целочисленного плана.
            //     Для построения ограничения выбираем компоненту оптимального плана с наибольшей дробной частью и по соответствующей
            //     этой компоненте k-й строке симплексной таблицы записываем ограничение Гомори.

            for (IndexColumnVector indexColumnVector = simplexMatrix.GetIndexColumnVector();
                indexColumnVector.HasFraction(n, simplexMatrix.RowsIndex, simplexMatrix.ColumnsIndex);
                indexColumnVector = simplexMatrix.GetIndexColumnVector())
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

                var rowsIndex1 = new StackListQueue<int>(simplexMatrix.RowsIndex);

                var withFraction =
                    new StackListQueue<int>(
                        Enumerable.Range(1, indexColumnVector.Count - 1)
                            .Where(
                                i =>
                                    rowsIndex1[i - 1] <= n &&
                                    Math.Abs(indexColumnVector[i] - Math.Round(indexColumnVector[i])) > Tolerance));

                var fractions =
                    new StackListQueue<double>(
                        withFraction.Select(i => Math.Abs(indexColumnVector[i] - Math.Floor(indexColumnVector[i]))));
                int k = withFraction[fractions.IndexOf(fractions.Max())]; // 1-based variable index 

                Debug.WriteLine(Math.Abs(indexColumnVector[k] - Math.Floor(indexColumnVector[k])));

                // Дополнительное ограничение составляем по строке k.
                if (appendLineCallback != null)
                    appendLineCallback(string.Format("Дополнительное ограничение составляем по строке {0}", k));
                Debug.WriteLine("Дополнительное ограничение составляем по строке {0}", k);

                rowsIndex1.Add(simplexMatrix.Columns);
                var vector = new Vector<double>(simplexMatrix[k].Select(a => Math.Floor(a) - a)) {1};
                foreach (var r in simplexMatrix) r.Add(0);
                simplexMatrix.Add(vector);
                simplexMatrix.RowsIndex = rowsIndex1;

                if (appendLineCallback != null) appendLineCallback("Текущий опорный план:");
                if (appendLineCallback != null) appendLineCallback(simplexMatrix.ToString());
                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(simplexMatrix.ToString());

                //     Transform.ByColumns:
                //     все элементы индексной строки отрицательные или ноль - при минимизации
                //     все элементы индексной строки положительные или ноль - при максимизации
                //     Transform.ByRows:
                //     все элементы индексного столбца положительные или ноль

                // Ищем допустимое и оптимальное решение
                simplexMatrix.SimplexMethod(SimplexMatrix.Transform.ByRows, minimax.Target);

                if (appendLineCallback != null) appendLineCallback("Текущий опорный план:");
                if (appendLineCallback != null) appendLineCallback(simplexMatrix.ToString());
                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(simplexMatrix.ToString());
            }
            double value = simplexMatrix[0][0];

            // Завершаем алгоритм и возвращаем найденное решение
            if (appendLineCallback != null)
                appendLineCallback("Завершаем алгоритм и возвращаем найденное решение");
            Debug.WriteLine("Завершаем алгоритм и возвращаем найденное решение");
            var rowsIndex = new StackListQueue<int>(simplexMatrix.RowsIndex);
            optimalVectors =
                new StackListQueue<Vector<T>>(
                    new Vector<T>(
                        Enumerable.Range(1, n)
                            .Select(
                                i =>
                                    (rowsIndex.Contains(i)
                                        ? (T) (dynamic) simplexMatrix[1 + rowsIndex.IndexOf(i)][0]
                                        : default(T)))));
            optimalValues = new StackListQueue<T>((T) (dynamic) value);
            if (compliteCallback != null) compliteCallback();
            return true;
        }

        private class IndexColumnVector : Vector<double>
        {
            public IndexColumnVector(double value)
                : base(value)
            {
            }

            public IndexColumnVector(IEnumerable<double> vector)
                : base(vector)
            {
            }

            public bool HasFraction(int n, IEnumerable<int> rowsIndex, IEnumerable<int> columnsIndex)
            {
                Debug.Assert(Count == rowsIndex.Count() + 1);
                return GetRange(1, Count - 1).Any(x => Math.Abs(x - Math.Round(x)) > Tolerance);
            }
        }

        private class IndexRowVector : Vector<double>
        {
            public IndexRowVector(double value)
                : base(value)
            {
            }

            public IndexRowVector(IEnumerable<double> vector)
                : base(vector)
            {
            }
        }

        private class SimplexMatrix : Matrix<double>, ITrace
        {
            public new enum Transform
            {
                ByRows = 1,
                ByBoth = 0,
                ByColumns = -1,
            };

            /// <summary>
            ///     Список базисных переменных
            /// </summary>
            public IEnumerable<int> RowsIndex { get; set; }

            /// <summary>
            ///     Список небазисных переменных
            /// </summary>
            public IEnumerable<int> ColumnsIndex { get; set; }

            #region

            public ProgressCallback ProgressCallback { get; set; }
            public AppendLineCallback AppendLineCallback { get; set; }
            public CompliteCallback CompliteCallback { get; set; }

            #endregion

            /// <summary>
            ///     Рассмотрим следующую задачу линейного программирования:
            ///     c^Tx -> max, Ax Le b, x Ge 0, b Ge 0.
            ///     Любая общая задача ЛП может быть приведена к канонической форме.
            ///     Приведение общей задачи ЛП к канонической форме достигается путем введения новых
            ///     (их называют дополнительными) переменных.
            ///     Двухфазный симплекс-метод
            ///     Причины использования
            ///     Если в условии задачи линейного программирования не все ограничения представлены неравенствами типа «≤», то далеко
            ///     не всегда нулевой вектор будет допустимым решением. Однако каждая итерация симплекс-метода является переходом от
            ///     одной вершины к другой, и если неизвестно ни одной вершины, алгоритм вообще не может быть начат.
            ///     Процесс нахождения исходной вершины не сильно отличается от однофазного симплекс-метода, однако может в итоге
            ///     оказаться сложнее, чем дальнейшая оптимизация.
            ///     Модификация ограничений
            ///     Все ограничения задачи модифицируются согласно следующим правилам:
            ///     ограничения типа «≤» переводятся на равенства созданием дополнительной переменной с коэффициентом «+1». Эта
            ///     модификация проводится и в однофазном симплекс-методе, дополнительные переменные в дальнейшем используются как
            ///     исходный базис.
            ///     ограничения типа «≥» дополняются одной переменной с коэффициентом «−1». Поскольку такая переменная из-за
            ///     отрицательного коэффициента не может быть использована в исходном базисе, необходимо создать ещё одну,
            ///     вспомогательную, переменную. Вспомогательные переменные всегда создаются с коэффициентом «+1».
            ///     ограничения типа «=» дополняются одной вспомогательной переменной.
            ///     Соответственно, будет создано некоторое количество дополнительных и вспомогательных переменных. В исходный базис
            ///     выбираются дополнительные переменные с коэффициентом «+1» и все вспомогательные. Осторожно: решение, которому
            ///     соответствует этот базис, не является допустимым.
            ///     После того, как было модифицировано условие, создаётся вспомогательная целевая функция
            ///     Если вспомогательные переменные были обозначены, как yi, i∈{1, .., k},
            ///     то вспомогательную функцию определим, как
            ///     z' = \sum_{i=1}^k y_i -> min
            ///     После этого проводится обыкновенный симплекс-метод относительно вспомогательной целевой функции.
            ///     Поскольку все вспомогательные переменные увеличивают значение z', в ходе алгоритма
            ///     они будут поочерёдно выводится из базиса, при этом после каждого перехода новое решение
            ///     будет всё ближе к множеству допустимых решений.
            ///     Когда будет найдено оптимальное значение вспомогательной целевой функции, могут возникнуть две ситуации:
            ///     оптимальное значение z' больше нуля. Это значит, что как минимум одна из вспомогательных переменных осталась в
            ///     базисе.
            ///     В таком случае можно сделать вывод, что допустимых решений данной задачи линейного программирования не существует.
            ///     оптимальное значение z' равно нулю. Это означает, что все вспомогательные переменные были выведены из базиса,
            ///     и текущее решение является допустимым.
            ///     Во втором случае мы имеем допустимый базис, или, иначе говоря, исходное допустимое решение. Можно проводить
            ///     дальнейшую оптимизацию с учётом исходной целевой функции, при этом уже не обращая внимания на вспомогательные
            ///     переменные. Это и является второй фазой решения.
            /// </summary>
            public bool DoubleSimplexMethod(ILinearMiniMax<T> minimax)
            {
                Debug.Assert(minimax.A.Rows == minimax.R.Count());
                Debug.Assert(minimax.A.Rows == minimax.B.Count());
                Debug.Assert(minimax.A.Columns == minimax.C.Count());

                AppendLineCallback appendLineCallback = AppendLineCallback;
                ProgressCallback progressCallback = ProgressCallback;
                CompliteCallback compliteCallback = CompliteCallback;

                // количество исходныx переменныx
                // количество неравенств==количество дополнительных переменных
                // количество вспомогательных переменных
                int n = Math.Min(minimax.A.Columns, minimax.C.Count());
                int count = Math.Min(minimax.A.Count(), minimax.B.Count());
                int count1 = minimax.R.Count(r => r == Comparer.Ge);

                var rowsIndex = new StackListQueue<int>(Enumerable.Range(1 + n + count1, count + 1));
                var columnsIndex = new StackListQueue<int>(Enumerable.Range(1, n + count1));

                Debug.WriteLine("count = " + count);
                Debug.WriteLine((double) minimax.Target);

                //     Модификация ограничений
                //     Все ограничения задачи модифицируются согласно следующим правилам:
                //    ограничения типа «≤» переводятся на равенства созданием дополнительной переменной с коэффициентом «+1». Эта
                //     модификация проводится и в однофазном симплекс-методе, дополнительные переменные в дальнейшем используются как
                //     исходный базис.
                //     ограничения типа «≥» дополняются одной переменной с коэффициентом «−1». Поскольку такая переменная из-за
                //     отрицательного коэффициента не может быть использована в исходном базисе, необходимо создать ещё одну,
                //     вспомогательную, переменную. Вспомогательные переменные всегда создаются с коэффициентом «+1».
                //     ограничения типа «=» дополняются одной вспомогательной переменной.
                //     Соответственно, будет создано некоторое количество дополнительных и вспомогательных переменных. В исходный базис
                //     выбираются дополнительные переменные с коэффициентом «+1» и все вспомогательные. Осторожно: решение, которому
                //     соответствует этот базис, не является допустимым.
                //После того, как было модифицировано условие, создаётся вспомогательная целевая функция
                //Если вспомогательные переменные были обозначены, как yi, i∈{1, .., k}, 
                //то вспомогательную функцию определим, как
                //z' = \sum_{i=1}^k y_i -> min
                //После этого проводится обыкновенный симплекс-метод относительно вспомогательной целевой функции. 
                //Поскольку все вспомогательные переменные увеличивают значение z', в ходе алгоритма 
                //они будут поочерёдно выводится из базиса, при этом после каждого перехода новое решение 
                //будет всё ближе к множеству допустимых решений.
                //Когда будет найдено оптимальное значение вспомогательной целевой функции, могут возникнуть две ситуации:
                //оптимальное значение z' больше нуля. Это значит, что как минимум одна из вспомогательных переменных осталась в базисе. 
                //В таком случае можно сделать вывод, что допустимых решений данной задачи линейного программирования не существует.
                //оптимальное значение z' равно нулю. Это означает, что все вспомогательные переменные были выведены из базиса, 
                //и текущее решение является допустимым.
                var random = new Random();
                var vector =
                    new Vector<double>(0)
                    {
                        Enumerable.Repeat(0.0, n),
                        Enumerable.Range(0, count1)
                            .Select(x => 100000000000000.0 + 10000000000000.0*random.NextDouble()),
                        Enumerable.Repeat(0.0, count),
                        0,
                    };
                Add(vector);

                int i1 = 1 + n + count1;
                int i2 = 1 + n;
                for (int i = 0; i < count; i++)
                {
                    var value = new Vector<double>();
                    switch (minimax.R.ElementAt(i))
                    {
                        case Comparer.Le:
                            // ограничения типа «≤» переводятся на равенства созданием дополнительной переменной с коэффициентом «+1»
                            value.Add(Convert.ToDouble(minimax.B.ElementAt(i)));
                            value.Add(minimax.A.ElementAt(i).Select(x => Convert.ToDouble(x)));
                            value.Add(Enumerable.Repeat(0.0, count + count1 + 1));
                            value[i1++] = 1;
                            break;
                        case Comparer.Eq:
                            // ограничения типа «=» дополняются одной вспомогательной переменной
                            // Вспомогательные переменные всегда создаются с коэффициентом «+1»
                            value.Add(Convert.ToDouble(minimax.B.ElementAt(i)));
                            value.Add(minimax.A.ElementAt(i).Select(x => Convert.ToDouble(x)));
                            value.Add(Enumerable.Repeat(0.0, count + count1 + 1));
                            value[i1++] = 1;
                            break;
                        case Comparer.Ge:
                            // ограничения типа «≥» дополняются одной переменной с коэффициентом «−1»
                            // Поскольку такая переменная из-за отрицательного коэффициента не может быть использована
                            // в исходном базисе, необходимо создать ещё одну, вспомогательную, переменную
                            // Вспомогательные переменные всегда создаются с коэффициентом «+1»
                            value.Add(-Convert.ToDouble(minimax.B.ElementAt(i)));
                            value.Add(minimax.A.ElementAt(i).Select(x => -Convert.ToDouble(x)));
                            value.Add(Enumerable.Repeat(0.0, count + count1 + 1));
                            value[i1++] = 1;
                            value[i2++] = 1;
                            break;
                    }
                    Add(value);
                }

                Add(new Vector<double>(0)
                {
                    minimax.C.Select(x => -Convert.ToDouble(x)),
                    Enumerable.Repeat(0.0, count1),
                    Enumerable.Repeat(0.0, count),
                    1,
                });

                RowsIndex = rowsIndex;
                ColumnsIndex = columnsIndex;

                if (appendLineCallback != null) appendLineCallback("Текущий опорный план:");
                if (appendLineCallback != null) appendLineCallback(ToString());
                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(ToString());

                //После этого проводится обыкновенный симплекс-метод относительно вспомогательной целевой функции. 
                //Поскольку все вспомогательные переменные увеличивают значение z', в ходе алгоритма 
                //они будут поочерёдно выводится из базиса, при этом после каждого перехода новое решение 
                //будет всё ближе к множеству допустимых решений.
                //Когда будет найдено оптимальное значение вспомогательной целевой функции, могут возникнуть две ситуации:
                //оптимальное значение z' больше нуля. Это значит, что как минимум одна из вспомогательных переменных осталась в базисе. 
                //В таком случае можно сделать вывод, что допустимых решений данной задачи линейного программирования не существует.
                //оптимальное значение z' равно нулю. Это означает, что все вспомогательные переменные были выведены из базиса, 
                //и текущее решение является допустимым.

                //     Transform.ByColumns:
                //     все элементы индексной строки отрицательные или ноль - при минимизации

                // Ищем допустимое решение (все элементы индексной строки отрицательные или ноль)
                SimplexMethod(Transform.ByColumns, Target.Minimum);

                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(ToString());

                if (this[0][0] > 0) return false;

                this[0]=this[this.Count]; this.RemoveAt(this.Count-1);

                rowsIndex = new StackListQueue<int>(RowsIndex);
                rowsIndex.Pop();
                RowsIndex = rowsIndex;

                columnsIndex = new StackListQueue<int>(ColumnsIndex);
                ColumnsIndex = new StackListQueue<int>(columnsIndex.Except(Enumerable.Range(1 + n + count, count1 + 1)));

                Debug.WriteLine("Текущий опорный план:");
                Debug.WriteLine(ToString());

                // Ищется допустимое и оптимальное решение 
                // (все элементы индексной строки отрицательные или ноль - при минимизации
                // все элементы индексной строки положительные или ноль - при максимизации
                // все элементы индексного столбца положительные или ноль)

                //     Transform.ByColumns:
                //     все элементы индексной строки отрицательные или ноль - при минимизации
                //     все элементы индексной строки положительные или ноль - при максимизации
                //     Transform.ByRows:
                //     все элементы индексного столбца положительные или ноль

                // Ищем допустимое решение (все элементы индексной строки отрицательные или ноль)
                // Ищем допустимое и оптимальное решение
                SimplexMethod(Transform.ByColumns, minimax.Target);
                SimplexMethod(Transform.ByRows, minimax.Target);

                return true;
            }


            public IndexRowVector GetIndexRowVector()
            {
                return new IndexRowVector(this[0]);
            }

            public IndexColumnVector GetIndexColumnVector()
            {
                return new IndexColumnVector(this.Select(Enumerable.First));
            }

            /// <summary>
            ///     Применение алгоритма симлекс-метода для решения задачи линейного программирования
            /// </summary>
            /// <param name="tr">
            ///     Признак проверки данных в индексной строке или индексном столбце.
            ///     Transform.ByColumns:
            ///     все элементы индексной строки отрицательные или ноль - при минимизации
            ///     все элементы индексной строки положительные или ноль - при максимизации
            ///     Transform.ByRows:
            ///     все элементы индексного столбца положительные или ноль
            ///     Transform.ByBoth:
            ///     все элементы индексной строки отрицательные или ноль - при минимизации
            ///     все элементы индексной строки положительные или ноль - при максимизации
            ///     все элементы индексного столбца положительные или ноль
            /// </param>
            /// <param name="target">
            ///     Признак решаемой задачи - поиск максимума или поиск минимума линейного функционала
            /// </param>
            public void SimplexMethod(Transform tr, Target target)
            {
                AppendLineCallback appendLineCallback = AppendLineCallback;
                ProgressCallback progressCallback = ProgressCallback;
                CompliteCallback compliteCallback = CompliteCallback;

                var rowsIndex = new StackListQueue<int>(RowsIndex);
                var columnsIndex = new StackListQueue<int>(ColumnsIndex);

                // Подготовка данных для шага симплекс метода
                for (;;)
                {
                    // Выбор ведущих строки и столбца.
                    if (appendLineCallback != null)
                        appendLineCallback("Выбор ведущих строки и столбца");
                    Debug.WriteLine("Выбор ведущих строки и столбца");

                    // В качестве ведущего выберем столбец, соответствующий переменной, так как наибольший коэффициент по модулю. 
                    // Из отрицательных коэффициентов индексной строки выбирается наибольший по абсолютной величине. 
                    // Затем элементы столбца свободных членов симплексной таблицы делит на элементы того же знака ведущего столбца.

                    // Если tr != Transform.ByBoth то ищется допустимое решение
                    // Если tr == Transform.ByBoth то ищется допустимое решение и оптимальное решение

                    var enumerable = from r in Enumerable.Range(1, rowsIndex.Count)
                        from c in columnsIndex
                        where Math.Abs(this[r][c]) > Tolerance
                        where this[r][0] < -Tolerance && this[r][0]*this[r][c] > Tolerance || tr != Transform.ByRows
                        where
                            this[0][c] * (double)target > Tolerance && this[0][c] * this[r][c] * (double)target > Tolerance ||
                            tr != Transform.ByColumns
                        select
                            new
                            {
                                row = r,
                                col = c,
                                value =
                                    Math.Abs((tr == Transform.ByRows)
                                        ? this[0][c]/this[r][c]
                                        : this[r][0]/this[r][c])
                            };

                    Debug.WriteLine(enumerable.Count());

                    if (!enumerable.Any()) break;

                    // В задаче минимизации вводимой переменной должно соответствовать наименьшее из указанных 
                    // отношений, а в задаче максимизации – отношение, наименьшее по абсолютной величине

                    double min = enumerable.Min(p => p.value);
                    var first = enumerable.First(p => Math.Abs(p.value - min) <= Tolerance);
                    int col = first.col;
                    int row = first.row;

                    Debug.WriteLine(string.Join(",",
                        rowsIndex.Select(x => x.ToString(CultureInfo.InvariantCulture))));
                    Debug.WriteLine(string.Join(",",
                        columnsIndex.Select(x => x.ToString(CultureInfo.InvariantCulture))));
                    if (appendLineCallback != null)
                        appendLineCallback(string.Format("строка = {0} столбец = {1}", row, col));
                    Debug.WriteLine("строка = {0} столбец = {1}", row, col);

                    //Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса
                    if (appendLineCallback != null)
                        appendLineCallback(
                            "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");
                    Debug.WriteLine(
                        "Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса");

                    GaussJordanStep(Matrix<double>.Transform.TransformByRows, row, col);

                    columnsIndex[columnsIndex.IndexOf(col)] = rowsIndex[row - 1];
                    rowsIndex[row - 1] = col;
                    RowsIndex = rowsIndex;
                    ColumnsIndex = columnsIndex;

                    if (appendLineCallback != null) appendLineCallback("Текущий опорный план:");
                    if (appendLineCallback != null) appendLineCallback(ToString());
                    Debug.WriteLine("Текущий опорный план:");
                    Debug.WriteLine(ToString());
                }
            }


            public override string ToString()
            {
                return string.Join(Environment.NewLine,
                    this.Select(
                        doubles =>
                            string.Join("|", doubles.Select(item => item.ToString(CultureInfo.InvariantCulture)))))
                       + Environment.NewLine + string.Join(",",
                           RowsIndex.Select(x => x.ToString(CultureInfo.InvariantCulture)))
                       + Environment.NewLine + string.Join(",",
                           ColumnsIndex.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}