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
    /// </summary>
    public class BooleanBranchAndBoundSolver42<T> : ISolver<T>
    {
        public BooleanBranchAndBoundSolver42()
        {
            H = 1;
        }

        /// <summary>
        ///     Количество переменных вводимых на одном шаге
        /// </summary>
        private int H { get; set; }

        /// <summary>
        ///     Решение задачи линейного программирования
        /// </summary>
        /// <param name="minimax"></param>
        /// <param name="optimalVectors"></param>
        /// <param name="optimalValues"></param>
        /// <param name="trace"></param>
        /// <returns></returns>
        public bool Execute(ILinearMiniMax<T> minimax, ref IEnumerable<Vector<T>> optimalVectors,
            ref IEnumerable<T> optimalValues, ITrace trace)
        {
            // Реализуем алгоритм только для случая наращивания базиса по одной переменной
            Debug.Assert(H == 1);

            Debug.Assert(minimax.A.Rows == minimax.R.Count());
            Debug.Assert(minimax.A.Rows == minimax.B.Count());
            Debug.Assert(minimax.A.Columns == minimax.C.Count());

            AppendLineCallback appendLineCallback = trace != null ? trace.AppendLineCallback : null;
            ProgressCallback progressCallback = trace != null ? trace.ProgressCallback : null;
            CompliteCallback compliteCallback = trace != null ? trace.CompliteCallback : null;

            if (progressCallback != null) progressCallback(0, minimax.C.Count);

            // Количество переменных
            int n = minimax.C.Count;

            // Величине рекорда присваивается заведомо плохое решение
            double r = minimax.Target == Target.Maximum ? Double.MinValue : Double.MaxValue;
            BooleanEnumPlan optimal = null;

            // Исходным выбранным частичным планом считается тот, базис которого пуст
            for (
                var stack =
                    new StackListQueue<BooleanEnumPlan>(
                        new BooleanEnumPlan(
                            new BooleanVector(),
                            new Vector<int>(),
                            minimax));
                stack.Any();
                )
            {
                BooleanEnumPlan element = stack.Pop();
                Debug.WriteLine("element = {0}", element);
                // Генерируем все возможные соседние планы, связанные с введением в базис H переменных
                // и вычисляем их оценки
                if (appendLineCallback != null)
                    appendLineCallback(
                        string.Format(
                            "Генерируем все возможные соседние планы, связанные с введением в базис {0} переменных",
                            H));
                Debug.WriteLine(
                    "Генерируем все возможные соседние планы, связанные с введением в базис {0} переменных", H);
                var list = new StackListQueue<BooleanEnumPlan>(new[] {true, false}.Select(value =>
                    new BooleanEnumPlan(new BooleanVector(element.Vector) {value},
                        new Vector<int>(element.Indeces) {Enumerable.Range(0, n).Except(element.Indeces).First()},
                        minimax))
                    .Where(item => item.ArgBound.All(x => x >= 0.0)));
                stack.Push(list);
                if (appendLineCallback != null)
                    appendLineCallback(string.Format("Количество подмножеств = {0}", stack.Count));
                Debug.WriteLine("Количество подмножеств = {0}", stack.Count);

                for (; stack.Any();)
                {
                    // На множестве планов выбрать планы с наибольшим числом введёных переменных
                    // На выбраном множестве выбрать с наилучшей оценкой
                    if (appendLineCallback != null)
                        appendLineCallback(
                            "На множестве планов выбрать планы с наибольшим числом введёных переменных");
                    if (appendLineCallback != null)
                        appendLineCallback("На выбраном множестве выбрать с наилучшей оценкой");
                    Debug.WriteLine("На множестве планов выбрать планы с наибольшим числом введёных переменных");
                    Debug.WriteLine("На выбраном множестве выбрать с наилучшей оценкой");

                    int max1 = stack.Max(item => item.Indeces.Count);
                    list = new StackListQueue<BooleanEnumPlan>(stack.Where(item => item.Indeces.Count == max1));
                    double maxMax = list.Max(item => item.FuncMax);
                    double minMin = list.Min(item => item.FuncMin);
                    element = minimax.Target == Target.Maximum
                        ? list.First(item => item.FuncMax == maxMax)
                        : list.First(item => item.FuncMin == minMin);
                    stack.Remove(element);
                    Debug.WriteLine("element = {0}", element);

                    if (minimax.Target == Target.Maximum && r <= maxMax ||
                        minimax.Target == Target.Minimum && r >= minMin)
                    {
                        // Если оценка элемента лучше рекорда
                        if (appendLineCallback != null)
                            appendLineCallback("Оценка элемента лучше рекорда");
                        if (element.Indeces.Count == n)
                        {
                            // Если в базис введены все элементы
                            if (appendLineCallback != null)
                                appendLineCallback("В базис введены все элементы");
                            // Запоминаем в рекорд оценку (т.е. значение для этого вектора)
                            // и запоминаем этот план
                            if (appendLineCallback != null)
                                appendLineCallback("Запоминаем в рекорд оценку");
                            r = minimax.Target == Target.Maximum ? element.FuncMax : element.FuncMin;
                            optimal = new BooleanEnumPlan(element);
                        }
                        else
                        {
                            // Иначе переходим к генерации соседних планов
                            stack.Push(element);
                            break;
                        }
                    }
                    // Выводим из базиса H последних переменных, отбросив остальные планы с таким же числом переменных
                    if (appendLineCallback != null)
                        appendLineCallback(
                            string.Format(
                                "Выводим из базиса {0} последних переменных, отбросив остальные планы с таким же числом переменных",
                                H));
                    element = new BooleanEnumPlan(
                        new BooleanVector(element.Vector.GetRange(0, element.Vector.Count - 1)),
                        new Vector<int>(element.Indeces.GetRange(0, element.Indeces.Count - 1)),
                        minimax);
                    stack.RemoveAll(item => item.Indeces.Count == element.Indeces.Count);

                    Debug.WriteLine("element = {0}", element);

                    if (appendLineCallback != null)
                        appendLineCallback(string.Format("Количество подмножеств = {0}", stack.Count));
                    Debug.WriteLine("Количество подмножеств = {0}", stack.Count);

                    if (element.Indeces.Count == 0) break;
                }
            }

            // Завершаем алгоритм и возвращаем найденное решение
            optimalVectors =
                new StackListQueue<Vector<T>>(
                    new Vector<T>(optimal.Indeces.Select(index => optimal.Vector[index] ? (T) (dynamic) 1 : default(T))));
            optimalValues = new StackListQueue<T>((T) (dynamic) r);
            if (compliteCallback != null) compliteCallback();
            return true;
        }

        private class BooleanEnumPlan
        {
            public BooleanEnumPlan(BooleanVector vector, Vector<int> indeces, ILinearMiniMax<T> minimax)
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
                                                               .Sum(
                                                                   index =>
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

            public BooleanEnumPlan(BooleanEnumPlan element)
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
    }
}