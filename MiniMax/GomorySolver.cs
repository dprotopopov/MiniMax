using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MyLibrary.Collections;
using MyMath;

namespace MiniMax
{
    /// <summary>
    ///     Алгоритм Гомори для частично целочисленной задачи линейного программирования
    ///     В частично целочисленных задачах требование целочисленности накладывается не на все переменные,
    ///     а на одну или некоторые из них.
    ///     Пусть дана следующая задача:
    ///     max {C(x)=∑cixi|∑ajixi{≤=≥}bj, j = 1,m, xi≥0 для всех i = 1,n}, (1)
    ///     Базисные переменные это переменные, которые входят только в одно уравнение системы ограничений и притом с единичным
    ///     коэффициентом.
    ///     Базисное решение называется допустимым, если оно неотрицательно.
    ///     Метод Гомори
    ///     Метод Гомори используют для нахождения целочисленного решения в задачах линейного программирования.
    ///     Пусть найдено решение задачи ЛП: .
    ///     Решение Li будет целым числом, если Метод Гомори т.е. правильное отсечение Гомори.
    ///     Данное соотношение определяет правильное отсечение Гомори
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GomorySolver<T> : ISolver<T>
    {
        public void Execute(IMiniMax<T> minimax, ref Vector<T> optimalVector, ref T optimalValue)
        {
            // 1. Составление псевдоплана. 
            // Систему ограничений исходной задачи приводят к системе неравенств смысла «≤».
            var gomoryMatrix = new GomoryMatrix(minimax);

            while (true)
            {
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

                int count = gomoryMatrix.Rows; // == количество переменных + индексная строка
                int other = gomoryMatrix.Columns - gomoryMatrix.Rows; // == количество введёных переменных

                // Подготовка данных для симплекс метода
                var matrix = new double[gomoryMatrix.Rows, gomoryMatrix.Columns];
                var next = new double[gomoryMatrix.Rows, gomoryMatrix.Columns];

                var read = new object();
                var write = new object();

                Parallel.ForEach(
                    from i in Enumerable.Range(0, gomoryMatrix.Rows)
                    from j in Enumerable.Range(0, gomoryMatrix.Columns)
                    select new[] {i, j}, pair =>
                    {
                        int i = pair[0];
                        int j = pair[1];
                        double x;
                        lock (read) x = gomoryMatrix[i][j];
                        lock (write) matrix[i, j] = x;
                    });

                var variables = new StackListQueue<int>(Enumerable.Range(count, other)); // 1-based variable indecies

                // 2. Проверка плана на оптимальность.
                for (IndexVector indexVector = gomoryMatrix.GetIndexVector();
                    !indexVector.IsOptimal();
                    indexVector = gomoryMatrix.GetIndexVector())
                {
                    // Текущий опорный план неоптимален, так как в индексной строке находятся отрицательные коэффициенты

                    // Если в полученном опорном плане не выполняется условие оптимальности,
                    // то задача решается симплексным методом.

                    // Выбор ведущих строки и столбца.
                    // Среди значений базисных переменных выбираются наибольшие по абсолютной величине. 
                    // Строка, соответствующая этому значению, является ведущей.
                    double max =
                        variables.Max(i => Math.Abs(indexVector.ElementAt(i)));

                    int col = variables.First(i => Math.Abs(indexVector.ElementAt(i - 1)) == max);

                    // В качестве ведущего выберем столбец, соответствующий переменной col, 
                    // так как это наибольший коэффициент по модулю.

                    // Вычислим значения Di по строкам как частное от деления: bi / aicol
                    // и из них выберем наименьшее:
                    var d = new Vector<double>(
                        gomoryMatrix.GetRange(1, count - 1)
                            .Select(r => (r.First()/r[col])));
                    double min = d.Min();
                    int row = d.IndexOf(min) + 1; // 1-based variable index

                    // Ведущей будет строка row, а переменную col следует вывести из базиса.                

                    int index = variables.IndexOf(col);
                    variables.RemoveAt(index);
                    variables.Insert(index, row);

                    //Пересчет симплекс-таблицы. Выполняем преобразования симплексной таблицы методом Жордано-Гаусса
                    Matrix<double>.GaussJordanStep(matrix, next, row, col);

                    double[,] t = matrix;
                    matrix = next;
                    next = t;

                    // Считывание данных из симплекс-метода
                    Parallel.ForEach(
                        from i in Enumerable.Range(0, gomoryMatrix.Rows)
                        from j in Enumerable.Range(0, gomoryMatrix.Columns)
                        select new[] {i, j}, pair =>
                        {
                            int i = pair[0];
                            int j = pair[1];
                            double x;
                            lock (read) x = matrix[i, j];
                            lock (write) gomoryMatrix[i][j] = x;
                        });
                }

                // Среди значений индексной строки нет отрицательных. 
                // Поэтому эта таблица определяет оптимальный план задачи.

                // Оптимальный план можно записать так: 
                var optimal = new Vector<double>(gomoryMatrix.GetIndexVector()
                    .GetRange(1, count - 1)
                    .Select(x => x*(double) minimax.Target));
                double value = gomoryMatrix.First().First()*(double) minimax.Target;

                // Проверяем на дробные части
                var indexWithFraction =
                    new StackListQueue<int>(Enumerable.Range(0, count - 1)
                        .Where(i => Math.Abs(Math.Floor(optimal[i]) - optimal[i]) > (double) 0));

                if (!indexWithFraction.Any())
                {
                    // Завершаем алгоритм
                    optimalVector = new Vector<T>(optimal.Select(x => (T) (dynamic) x));
                    optimalValue = (T) (dynamic) value;
                    return;
                }

                // В полученном оптимальном плане переменная имеет дробную часть числа. 

                // В результате решения задачи с отброшенным условием целочисленности 
                // получена оптимальная симплекс-таблица и переменной xk 
                // соответствует строка базисной переменной vk этой таблицы. 

                // Метод Гомори.
                // Составляется дополнительное ограничение для переменной, 
                // которая в оптимальном плане имеет максимальное дробное значение, хотя должна быть целой. 

                // Находим переменную с максимальной дробной частью
                var fractions =
                    new StackListQueue<double>(
                        indexWithFraction.Select(i => Math.Abs(optimal[i] - Math.Floor(optimal[i]))));
                int k = indexWithFraction[fractions.IndexOf(fractions.Max())]; // 0-based variable index 

                // Дополнительное ограничение составляем по строке, соответствующей переменной k.

                //Выделим в βk целую и дробную часть и преобразуем (2) к виду
                double b = optimal[k];
                double f = b - Math.Floor(b);

                foreach (var r in gomoryMatrix) r.Add(0);

                var vector = new Vector<double>(f);
                vector.Add(Enumerable.Repeat((double) 0, count - 1));
                vector.Add(
                    gomoryMatrix[k + 1].GetRange(1, count + other - 1)
                        .Select(a => (a >= (double) 0) ? a : (a*f/(f - 1))));
                // где I+ – множество значений i, для которых αki > 0 ; I- – множество значений i , для которых αki< 0.                
                vector.Add(1);
                gomoryMatrix.Add(vector);

                gomoryMatrix.GaussJordan(); // составление первого опорного плана
            }
        }

        public class GomoryMatrix : Matrix<double>
        {
            /// <summary>
            ///     Создание матрицы плана
            ///     Утверждение. Любая общая задача ЛП может быть приведена к канонической форме.
            ///     Приведение общей задачи ЛП к канонической форме достигается путем введения новых
            ///     (их называют дополнительными) переменных.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            public GomoryMatrix(IMiniMax<T> minimax)
            {
                Debug.Assert(minimax.A.Count() == minimax.B.Count());

                int count = Math.Min(minimax.A.Count(), minimax.B.Count());

                // Добавим индексную строку
                var index = new Vector<double>(0); // значение целевой функции

                // Приведем систему ограничений к системе неравенств смысла ≤, 
                // умножив соответствующие строки на (-1).
                index.Add(minimax.R.Select(x => ((double) ((dynamic) x)*(double) minimax.Target)));
                index.Add(Enumerable.Repeat((double) 0, count));
                Add(index);

                for (int i = 0; i < count; i++)
                {
                    var value = new Vector<double>((double) (dynamic) minimax.B.ElementAt(i));
                    // Приведем систему ограничений к системе неравенств смысла ≤, 
                    // умножив соответствующие строки на (-1).
                    value.Add(minimax.A.ElementAt(i).Select(x => (double) (dynamic) x*(double) (dynamic) minimax.R.ElementAt(i)));
                    value.Add(Enumerable.Repeat((double) 0, count));
                    value[count + i + 1] = (double) (dynamic) minimax.R.ElementAt(i);
                    Add(value);
                }
            }

            public ValueVector GetValueVector()
            {
                return new ValueVector(this.Select(Enumerable.First));
            }

            public IndexVector GetIndexVector()
            {
                return new IndexVector(this.First());
            }
        }

        public class IndexVector : Vector<double>
        {
            public IndexVector(IEnumerable<double> vector)
                : base(vector)
            {
            }

            public bool IsOptimal()
            {
                return GetRange(1, Count - 1).All(x => x >= (double) 0);
            }
        }

        public class ValueVector : Vector<double>
        {
            public ValueVector(IEnumerable<double> vector)
                : base(vector)
            {
            }
        }
    }
}