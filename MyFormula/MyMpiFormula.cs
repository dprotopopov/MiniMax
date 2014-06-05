using System;
using System.ComponentModel;
using MiniMax;
using MiniMax.Attributes;

namespace MyFormula
{
    /// <summary>
    ///     ƒл€ эффективного использовани€ графического и центрального процессора мне необходимо оптимизировать количество
    ///     нитей, используемых в параллельных алгоритмах сортировки массивов данных. — возрастанием количества нитей
    ///     производительность обработки массива должна возрастать, но в то же врем€ с возрастанием количества нитей будет
    ///     возрастать врем€ на организацию их работы. ѕринима€ во внимание эти факторы, мною была выбрана следующа€
    ///     математическа€ модель:
    /// </summary>
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class MyMpiFormula
    {
        /// <summary>
        ///     среднее врем€ на организацию вычислений;
        /// </summary>
        [Category(" онстанты")]
        [MiniMaxConstant]
        [MiniMaxInput]
        [Description("среднее врем€ на организацию вычислений")]
        public decimal C { get; set; }

        /// <summary>
        ///     среднее врем€ на организацию одного процесса;
        /// </summary>
        [Category(" онстанты")]
        [MiniMaxConstant]
        [MiniMaxInput]
        [Description("среднее врем€ на организацию одного процесса")]
        public decimal F { get; set; }

        /// <summary>
        ///     максимальное количество процессов MPI;
        /// </summary>
        [Category("ѕеременные")]
        [MiniMaxVariable(1, 3, 7, 15, 31, 63, 127)]
        [MiniMaxOutput]
        [Description("максимальное количество процессов MPI")]
        public decimal P { get; set; }

        /// <summary>
        ///     количество нитей задействованных в параллельном алгоритме сортировки массива
        ///     дл€ обработки i-того блока массива;
        /// </summary>
        [Category("ќпции")]
        [Description(
            "среднее количество нитей задействованных в параллельном алгоритме сортировки массива дл€ обработки i-того блока массива"
            )]
        [MiniMaxOption(1, 3, 7, 15, 31, 63, 127, 255)]
        [MiniMaxInput]
        public decimal Xi { get; set; }

        /// <summary>
        ///     врем€ обработки i-ого блока массива одним процессом;
        /// </summary>
        [Category("ќпции")]
        [Description("среднее врем€ обработки i-ого блока одним процессом")]
        [MiniMaxOption(0.1, 1, 10)]
        [MiniMaxInput]
        public decimal Di { get; set; }

        /// <summary>
        ///     число потоковых блоков в i-том блоке исходного массива.
        /// </summary>
        [Category("ќпции")]
        [Description("среднее число потоковых блоков в i-том блоке исходного массива")]
        [MiniMaxOption(1, 10, 100, 1000, 10000)]
        [MiniMaxInput]
        public decimal Bi { get; set; }

        /// <summary>
        ///     количество блоков массива;
        /// </summary>
        [Category("ќпции")]
        [Description("количество блоков массива")]
        [MiniMaxOption(1, 10, 100, 1000, 10000)]
        [MiniMaxInput]
        public decimal Q { get; set; }

        [Category("–асчЄт")]
        [MiniMaxTarget(Target.Minimum)]
        [MiniMaxCalculated]
        [Description("врем€ на вычислени€")]
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

        [Category("ќграничени€")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool xiGe1
        {
            get { return Xi >= 1; }
        }

        [Category("ќграничени€")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool xiLeP
        {
            get { return Xi <= P; }
        }
    }
}