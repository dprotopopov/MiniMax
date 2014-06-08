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
    public class BooleanTreeBranchAndBoundSolver<T> : ISolver<T>
    {
        public bool Execute(ILinearMiniMax<T> minimax, ref IEnumerable<Vector<T>> optimalVectors, ref IEnumerable<T> optimalValues, ITrace trace)
        {
            Debug.Assert(minimax.A.Rows == minimax.R.Count());
            Debug.Assert(minimax.A.Rows == minimax.B.Count());
            Debug.Assert(minimax.A.Columns == minimax.C.Count());

            AppendLineCallback appendLineCallback = trace != null ? trace.AppendLineCallback : null;
            ProgressCallback progressCallback = trace != null ? trace.ProgressCallback : null;
            CompliteCallback compliteCallback = trace != null ? trace.CompliteCallback : null;

            // Количество переменных
            int n = minimax.C.Count;

            if (progressCallback != null) progressCallback(0, minimax.C.Count);
            // Исходным выбранным частичным планом считается тот, базис которого пуст
            var list = new StackListQueue<BooleanTreePlan>(new BooleanTreePlan(
                new BooleanVector(),
                minimax));
            for (int k = 0;; k++)
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

                    list.RemoveAll(item => item.FuncMax < maxMin);

                    if (appendLineCallback != null)
                        appendLineCallback(string.Format("Количество подмножеств = {0}", list.Count));
                    Debug.WriteLine("Количество подмножеств = {0}", list.Count);
                }

                if (k == n) break;

                // Шаг деления множеств пополам (дописыванием нуля и единицы)
                if (appendLineCallback != null)
                    appendLineCallback("Шаг деления множеств пополам (дописыванием нуля и единицы)");
                var next = new StackListQueue<BooleanTreePlan>
                {
                    list.Select(item => new BooleanTreePlan(new BooleanVector(item.Vector) {false}, minimax)),
                    list.Select(item => new BooleanTreePlan(new BooleanVector(item.Vector) {true}, minimax))
                };
                list = next;
            }

            // Завершаем алгоритм и возвращаем найденное решение
            optimalVectors =
                new StackListQueue<Vector<T>>(
                    list.Select(item => new Vector<T>(item.Vector.Select(b => b ? (T) (dynamic) 1 : default(T)))));
            optimalValues =
                new StackListQueue<T>(list.Select(item => (T) (dynamic) ((double) minimax.Target*item.FuncMin)));
            Debug.Assert(list.All(item => Math.Abs(item.FuncMin - item.FuncMax) <= 0));
            if (compliteCallback != null) compliteCallback();
            return true;
        }

        private class BooleanTreePlan
        {
            public BooleanTreePlan(BooleanVector vector, ILinearMiniMax<T> minimax)
            {
                Vector = vector;
                double value =
                    Enumerable.Range(0, vector.Count)
                        .Where(index => vector[index])
                        .Sum(index => Convert.ToDouble(minimax.C[index]));
                switch (minimax.Target)
                {
                    case Target.Maximum:
                        FuncMin = value + Enumerable.Range(vector.Count, minimax.C.Count - vector.Count)
                            .Where(index => IsNegative(minimax.C[index]))
                            .Sum(index => Convert.ToDouble(minimax.C[index]));
                        FuncMax = value + Enumerable.Range(vector.Count, minimax.C.Count - vector.Count)
                            .Where(index => IsPositive(minimax.C[index]))
                            .Sum(index => Convert.ToDouble(minimax.C[index]));
                        break;
                    case Target.Minimum:
                        FuncMax = -(value + Enumerable.Range(vector.Count, minimax.C.Count - vector.Count)
                            .Where(index => IsNegative(minimax.C[index]))
                            .Sum(index => Convert.ToDouble(minimax.C[index])));
                        FuncMin = -(value + Enumerable.Range(vector.Count, minimax.C.Count - vector.Count)
                            .Where(index => IsPositive(minimax.C[index]))
                            .Sum(index => Convert.ToDouble(minimax.C[index])));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                ArgBound = new Vector<double>();
                int k = 0;
                foreach (double v in minimax.B.Select(b => -Convert.ToDouble(b)
                                                           + Enumerable.Range(0, vector.Count)
                                                               .Where(index => vector[index])
                                                               .Sum(index => Convert.ToDouble(minimax.A[k][index]))))
                {
                    switch (minimax.R[k])
                    {
                        case CompareOperand.Ge:
                            ArgBound.Add(v + Enumerable.Range(vector.Count, minimax.A[k].Count - vector.Count)
                                .Where(index => IsPositive(minimax.A[k][index]))
                                .Sum(index => Convert.ToDouble(minimax.A[k][index])));
                            break;
                        case CompareOperand.Le:
                            ArgBound.Add(-v - Enumerable.Range(vector.Count, minimax.A[k].Count - vector.Count)
                                .Where(index => IsNegative(minimax.A[k][index]))
                                .Sum(index => Convert.ToDouble(minimax.A[k][index])));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    k++;
                }
            }

            /// <summary>
            ///     Двоичный вектор определяет подмножество решений
            /// </summary>
            public BooleanVector Vector { get; set; }

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
        }
    }
}