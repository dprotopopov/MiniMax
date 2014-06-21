using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyLibrary.Collections;
using MyLibrary.Trace;
using MyMath;
using MyMath.GF2;

namespace MiniMax
{
    /// <summary>
    ///     Пусть дана следующая задача:
    ///     max {C(x)=∑cixi|∑ajixi{≤=≥}bj, j = 1,m, xi=0,1 для всех i = 1,n}, (1)
    ///     Процедура ветвления состоит в разбиении множества допустимых значений переменной x на подобласти (подмножества)
    ///     меньших размеров. Процедуру можно рекурсивно применять к подобластям. Полученные подобласти образуют дерево,
    ///     называемое деревом поиска или деревом ветвей и границ. Узлами этого дерева являются построенные подобласти
    ///     (подмножества множества значений переменной x).
    ///     Процедура нахождения оценок заключается в поиске верхних и нижних границ для решения задачи на подобласти
    ///     допустимых значений переменной x.
    ///     В основе метода ветвей и границ лежит следующая идея: если нижняя граница значений функции на подобласти A дерева
    ///     поиска больше, чем верхняя граница на какой-либо ранее просмотренной подобласти B, то A может быть исключена из
    ///     дальнейшего рассмотрения (правило отсева).
    /// </summary>
    public class BooleanMultiBranchAndBoundSolver<T> : ISolver<T>
    {
        public BooleanMultiBranchAndBoundSolver()
        {
            H = 1;
        }

        /// <summary>
        ///     Количество переменных вводимых на одном шаге
        /// </summary>
        private int H { get; set; }

        public bool Execute(ILinearMiniMax<T> minimax, ref IEnumerable<Vector<T>> optimalVectors,
            ref IEnumerable<T> optimalValues, ITrace trace)
        {
            // Реализуем алгоритм только для случая наращивания базиса по одной переменной
            Debug.Assert(H == 1);

            Debug.Assert(minimax.A.Rows == minimax.R.Count());
            Debug.Assert(minimax.A.Rows == minimax.B.Count());
            Debug.Assert(minimax.A.Columns == minimax.C.Count());

            // Количество переменных
            int n = minimax.C.Count;

            AppendLineCallback appendLineCallback = trace != null ? trace.AppendLineCallback : null;
            ProgressCallback progressCallback = trace != null ? trace.ProgressCallback : null;
            CompliteCallback compliteCallback = trace != null ? trace.CompliteCallback : null;

            if (progressCallback != null) progressCallback(0, minimax.C.Count);
            // Исходным выбранным частичным планом считается тот, базис которого пуст
            var list =
                new StackListQueue<BooleanMultiPlan>(new[] {true, false}
                    .SelectMany(value => Enumerable.Range(0, n).Select(
                        index => new BooleanMultiPlan(new BooleanVector(value),
                            new Vector<int>(index), minimax))
                        .Where(item => item.ArgBound.All(x => x >= 0.0))));
            for (int k = 1;; k++)
            {
                if (appendLineCallback != null)
                    appendLineCallback(string.Format("Количество подмножеств = {0}", list.Count));
                if (progressCallback != null) progressCallback(k, minimax.C.Count);
                // Удаляем подмножества с заведомо недопустимыми значениями
                if (appendLineCallback != null)
                    appendLineCallback("Удаляем подмножества с заведомо недопустимыми значениями");
                Debug.WriteLine("Удаляем подмножества с заведомо недопустимыми значениями");
                list.RemoveAll(item => item.ArgBound.Any(x => x < 0));
                if (appendLineCallback != null)
                    appendLineCallback(string.Format("Количество подмножеств = {0}", list.Count));
                Debug.WriteLine("Количество подмножеств = {0}", list.Count);

                if (list.Count == 0) return false;

                for (;;)
                {
                    double maxMin = list.Max(item => item.FuncMin);
                    double minMax = list.Min(item => item.FuncMax);

                    if (appendLineCallback != null)
                        appendLineCallback(string.Format("maxMin = {0} minMax = {1}", maxMin, minMax));
                    Debug.WriteLine("maxMin = {0} minMax = {1}", maxMin, minMax);

                    if (maxMin <= minMax) break;

                    // Удаляем варианты подмножеств, для которых существует более лучшая оценка целевой функции
                    if (appendLineCallback != null)
                        appendLineCallback(
                            "Удаляем варианты подмножеств, для которых существует более лучшая оценка целевой функции");

                    switch (minimax.Target)
                    {
                        case Target.Maximum:
                            list.RemoveAll(item => item.FuncMax < maxMin);
                            break;
                        case Target.Minimum:
                            list.RemoveAll(item => item.FuncMin > minMax);
                            break;
                    }

                    if (appendLineCallback != null)
                        appendLineCallback(string.Format("Количество подмножеств = {0}", list.Count));
                    Debug.WriteLine("Количество подмножеств = {0}", list.Count);
                }

                if (k == n) break;

                // Генерируем все возможные соседние планы, связанные с введением в базис H переменных
                // и вычисляем их оценки
                if (appendLineCallback != null)
                    appendLineCallback(
                        string.Format(
                            "Генерируем все возможные соседние планы, связанные с введением в базис {0} переменных",
                            H));
                Debug.WriteLine(
                    "Генерируем все возможные соседние планы, связанные с введением в базис {0} переменных", H);
                var next = new SortedStackListQueue<BooleanMultiPlan>(new[] {true, false}
                    .SelectMany(value => list.Select(
                        element => new BooleanMultiPlan(new BooleanVector(element.Vector) {value},
                            new Vector<int>(element.Indeces) {(element.Indeces.Last() + 1)%n}, minimax))
                        .Where(item => item.ArgBound.All(x => x >= 0.0))))
                {
                    Comparer = new BooleanMultiPlanComparer()
                };
                if (appendLineCallback != null)
                    appendLineCallback("Удаляем дупликаты");
                Debug.WriteLine("Удаляем дупликаты");
                list = new StackListQueue<BooleanMultiPlan>(next.Distinct());
                Debug.WriteLine("Количество подмножеств = {0}", list.Count);
            }

            // Завершаем алгоритм и возвращаем найденное решение
            optimalVectors =
                new StackListQueue<Vector<T>>(
                    list.Select(item =>
                        new Vector<T>(
                            Enumerable.Range(0, n)
                                .Select(index =>
                                    item.Vector[item.Indeces.IndexOf(index)] ? (T) (dynamic) 1 : default(T)))));
            optimalValues =
                new StackListQueue<T>(list.Select(item => (T) (dynamic) (item.FuncMin)));
            Debug.Assert(list.All(item => item.FuncMin <= item.FuncMax));
            if (compliteCallback != null) compliteCallback();
            return true;
        }

        private class BooleanMultiPlan
        {
            public BooleanMultiPlan(BooleanVector vector, Vector<int> indeces, ILinearMiniMax<T> minimax)
            {
                Vector = vector;
                Indeces = indeces;

                // Количество переменных
                int n = minimax.C.Count;

                double value =
                    Enumerable.Range(0, vector.Count)
                        .Where(index => vector[index])
                        .Sum(index => Convert.ToDouble(minimax.C[indeces[index]]));
                FuncMin = value + Enumerable.Range(0, n).Except(indeces)
                    .Where(index => IsNegative(minimax.C[index]))
                    .Sum(index => Convert.ToDouble(minimax.C[index]));
                FuncMax = value + Enumerable.Range(0, n).Except(indeces)
                    .Where(index => IsPositive(minimax.C[index]))
                    .Sum(index => Convert.ToDouble(minimax.C[index]));
                ArgBound = new Vector<double>();
                int k = 0;
                foreach (double v in minimax.B.Select(b => -Convert.ToDouble(b)
                                                           + Enumerable.Range(0, vector.Count)
                                                               .Where(index => vector[index])
                                                               .Sum(index =>
                                                                   Convert.ToDouble(minimax.A[k][indeces[index]]))))
                {
                    switch (minimax.R[k])
                    {
                        case Comparer.Ge:
                            ArgBound.Add(v + Enumerable.Range(0, n).Except(indeces)
                                .Where(index => IsPositive(minimax.A[k][index]))
                                .Sum(index => Convert.ToDouble(minimax.A[k][index])));
                            break;
                        case Comparer.Le:
                            ArgBound.Add(-v - Enumerable.Range(0, n).Except(indeces)
                                .Where(index => IsNegative(minimax.A[k][index]))
                                .Sum(index => Convert.ToDouble(minimax.A[k][index])));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    k++;
                }
            }

            public BooleanMultiPlan(BooleanMultiPlan element)
            {
                Vector = new BooleanVector(element.Vector);
                Indeces = new Vector<int>(element.Indeces);
                ArgBound = new Vector<double>(element.ArgBound);
                FuncMin = element.FuncMin;
                FuncMax = element.FuncMax;
            }

            /// <summary>
            ///     Двоичный вектор определяет подмножество решений
            /// </summary>
            public BooleanVector Vector { get; set; }

            public Vector<int> Indeces { get; set; }

            /// <summary>
            ///     Наилучшая нижняя оценка функции на подмножестве
            /// </summary>
            public double FuncMin { get; set; }

            /// <summary>
            ///     Наилучшая верхняя оценка функции на подмножестве
            /// </summary>
            public double FuncMax { get; set; }

            /// <summary>
            ///     Оценка растояния до правой части
            ///     Если ArgBound отрицательно, то все переменные на подмножестве
            ///     принимают недопустимые значения
            /// </summary>
            public Vector<double> ArgBound { get; set; }

            private static bool IsNegative(T p)
            {
                return (dynamic) p < default(T);
            }

            private static bool IsPositive(T p)
            {
                return (dynamic) p > default(T);
            }

            public override string ToString()
            {
                return string.Format("{0}:{1}[{2},{3}]", Vector, Indeces, FuncMin, FuncMax);
            }
        }

        private class BooleanMultiPlanComparer : IComparer<BooleanMultiPlan>
        {
            public int Compare(BooleanMultiPlan x, BooleanMultiPlan y)
            {
                int value = x.Indeces.Count - y.Indeces.Count;
                if (value != 0) return value;
                var list1 = new StackListQueue<int>(x.Indeces);
                var list2 = new StackListQueue<int>(y.Indeces);
                list1.Sort();
                list2.Sort();
                value = list1.Select((i, index) => i - list2[index]).FirstOrDefault(i => i != 0);
                if (value != 0) return value;
                value =
                    list1.Select(
                        i => (x.Vector[x.Indeces.IndexOf(i)] ? 1 : 0) - (y.Vector[y.Indeces.IndexOf(i)] ? 1 : 0))
                        .FirstOrDefault(v => v != 0);
                return value;
            }
        }
    }
}