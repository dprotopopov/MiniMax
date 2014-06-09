using System;
using System.ComponentModel;
using MiniMax;
using MiniMax.Attributes;

namespace MyFormula
{
    /// <summary>
    ///     Для эффективного использования графического и центрального процессора мне необходимо оптимизировать количество
    ///     нитей, используемых в параллельных алгоритмах сортировки массивов данных. С возрастанием количества нитей
    ///     производительность обработки массива должна возрастать, но в то же время с возрастанием количества нитей будет
    ///     возрастать время на организацию их работы. Принимая во внимание эти факторы, мною была выбрана следующая
    ///     математическая модель:
    /// </summary>
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class MyCudaFormula
    {
        /// <summary>
        ///     среднее время на организацию вычислений;
        /// </summary>
        [Category("Константы")]
        [MiniMaxConstant]
        [MiniMaxInput]
        [Description("среднее время на организацию вычислений")]
        public decimal C { get; set; }

        /// <summary>
        ///     среднее время на организацию одной нити;
        /// </summary>
        [Category("Константы")]
        [MiniMaxConstant]
        [MiniMaxInput]
        [Description("среднее время на организацию одной нити")]
        public decimal F { get; set; }

        /// <summary>
        ///     максимальное количество активных нитей GPU;
        /// </summary>
        [Category("Константы")]
        [MiniMaxConstant]
        [MiniMaxInput]
        [Description("максимальное количество активных нитей GPU")]
        public decimal P { get; set; }

        /// <summary>
        ///     количество нитей задействованных в параллельном алгоритме сортировки массива
        ///     для обработки i-того блока массива;
        /// </summary>
        [Category("Опции")]
        [Description(
            "среднее количество нитей задействованных в параллельном алгоритме сортировки массива для обработки i-того блока массива"
            )]
        [MiniMaxOption(1, 3, 7, 15, 31, 63, 127, 255)]
        [MiniMaxInput]
        public decimal Xi { get; set; }

        /// <summary>
        ///     время обработки i-ого блока массива одной нитью;
        /// </summary>
        [Category("Опции")]
        [Description("среднее время обработки i-ого блока массива одной нитью")]
        [MiniMaxOption(0.1, 1, 10)]
        [MiniMaxInput]
        public decimal Di { get; set; }

        /// <summary>
        ///     число потоковых блоков в i-том блоке исходного массива.
        /// </summary>
        [Category("Опции")]
        [Description("среднее число потоковых блоков в i-том блоке исходного массива")]
        [MiniMaxOption(1, 10, 100, 1000, 10000)]
        [MiniMaxInput]
        public decimal Bi { get; set; }

        /// <summary>
        ///     количество блоков массива;
        /// </summary>
        [Category("Опции")]
        [Description("количество блоков массива")]
        [MiniMaxOption(1, 10, 100, 1000, 10000)]
        [MiniMaxInput]
        public decimal Q { get; set; }

        /// <summary>
        ///     число нитей в блоке;
        /// </summary>
        [Category("Переменные")]
        [Description("число нитей")]
        [MiniMaxVariable(1, 3, 7, 15, 31, 63, 127, 255, 511, 1023)]
        [MiniMaxOutput]
        public decimal N { get; set; }

        [Category("Расчёт")]
        [MiniMaxTarget(Target.Minimum)]
        [MiniMaxCalculated]
        [Description("время на вычисления")]
        public decimal TotalTime
        {
            get
            {
                try
                {
                    return Q*(C + (F*Xi) + (Di/Xi));
                }
                catch (Exception)
                {
                    return Decimal.MaxValue;
                }
            }
        }

        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool xiGe1
        {
            get { return Xi >= 1; }
        }

        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool xiLeP
        {
            get { return Xi <= P; }
        }

        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool xiLeBiN
        {
            get { return Xi <= Bi*N; }
        }
    }
}