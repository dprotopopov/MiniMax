using System;
using System.ComponentModel;
using MiniMax;
using MiniMax.Attributes;

namespace MyFormula
{
    /// <summary>
    ///     Соединение моей формулы и формулы NVidia в одну формулу
    /// </summary>
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class MyCudaNvidiaFormula
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
        [MiniMaxOption(1, 3, 7, 15, 31, 63, 127)]
        [MiniMaxInput]
        public decimal Xi { get; set; }

        /// <summary>
        ///     время обработки i-ого блока массива одной нитью;
        /// </summary>
        [Category("Опции")]
        [Description("среднее время обработки i-ого блока массива одной нитью")]
        [MiniMaxOption(1, 10)]
        [MiniMaxInput]
        public decimal Di { get; set; }

        /// <summary>
        ///     число потоковых блоков в i-том блоке исходного массива.
        /// </summary>
        [Category("Опции")]
        [Description("среднее число потоковых блоков в i-том блоке исходного массива")]
        [MiniMaxOption(1, 10, 100, 1000)]
        [MiniMaxInput]
        public decimal Bi { get; set; }

        /// <summary>
        ///     количество блоков массива;
        /// </summary>
        [Category("Опции")]
        [Description("количество блоков массива")]
        [MiniMaxOption(1, 10, 100, 1000)]
        [MiniMaxInput]
        public decimal Q { get; set; }

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

        [Category("Справочно")]
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

        /// <summary>
        ///     число активных блоков на мультипроцессор;
        ///     Для эффективного использования GPU число активных нитей и активных блоков на мультипроцессоре должно быть
        ///     максимальным.
        /// </summary>
        [Category("Справочно")]
        [Description("число активных блоков на мультипроцессор")]
        [MiniMaxCalculated]
        public decimal Ba
        {
            get { return Math.Min(Bmax, Math.Floor(Wmax/W)); }
        }

        /// <summary>
        ///     число активных нитей на мультипроцессор;
        /// </summary>
        [Category("Справочно")]
        [Description("число активных нитей на мультипроцессор")]
        [MiniMaxCalculated]
        public decimal Na
        {
            get { return Ba*BlockSize; }
        }

        /// <summary>
        ///     число активных варпов на мультипроцессор;
        /// </summary>
        [Category("Справочно")]
        [Description("число активных варпов на мультипроцессор")]
        [MiniMaxCalculated]
        public decimal Wa
        {
            get { return Ba*W; }
        }

        /// <summary>
        ///     число нитей в варпе;
        /// </summary>
        [Category("Переменные")]
        [Description("число нитей в варпе")]
        [MiniMaxVariable(1, 3, 7, 15, 31, 63, 127)]
        [MiniMaxInput]
        public decimal Nw { get; set; }

        /// <summary>
        ///     число варпов;
        ///     Число варпов рассчитывается как частное числа нитей в блоке на число нитей в варпе.
        /// </summary>
        [Category("Справочно")]
        [Description("число варпов")]
        [MiniMaxCalculated]
        public decimal W
        {
            get
            {
                try
                {
                    return Math.Ceiling(BlockSize/Nw);
                }
                catch (Exception)
                {
                    return Decimal.MaxValue;
                }
            }
        }

        /// <summary>
        ///     максимальное число нитей на блок;
        /// </summary>
        [Category("Константы")]
        [Description("максимальное число нитей на блок")]
        [MiniMaxConstant]
        public decimal Nbmax { get; set; }

        /// <summary>
        ///     максимальное число варпов на мультипроцессор;
        /// </summary>
        [Category("Константы")]
        [Description("максимальное число варпов на мультипроцессор")]
        [MiniMaxConstant]
        public decimal Wmax { get; set; }

        /// <summary>
        ///     максимальное число блоков на мультипроцессор;
        /// </summary>
        [Category("Константы")]
        [Description("максимальное число блоков на мультипроцессор")]
        [MiniMaxConstant]
        public decimal Bmax { get; set; }

        /// <summary>
        ///     максимальное число нитей на мультипроцессор;
        /// </summary>
        [Category("Константы")]
        [MiniMaxConstant]
        [Description("максимальное число нитей на мультипроцессор")]
        public decimal Nmax { get; set; }


        /// Максимальное число нитей на мультипроцессоре – это произведение максимального числа варпов на число нитей в варпе
        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool nmaxLeWmaxN
        {
            get { return Nmax <= Wmax*BlockSize; }
        }

        /// Число нитей в блоке целое число, которое не может быть меньше единицы и больше максимального числа нитей в блоке.
        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool nGe1
        {
            get { return BlockSize >= 1; }
        }

        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool nLeNbmax
        {
            get { return BlockSize <= Nbmax; }
        }

        #region

        /// <summary>
        ///     число нитей в блоке;
        /// </summary>
        [Category("Справочно")]
        [Description("число нитей")]
        [MiniMaxCalculated]
        public decimal N
        {
            get { return GridSize*BlockSize; }
        }

        /// <summary>
        ///     число нитей в блоке;
        /// </summary>
        [Category("Переменные")]
        [Description("число нитей в блоке")]
        [MiniMaxVariable(1, 3, 7, 15, 31, 63, 127)]
        [MiniMaxOutput]
        public decimal BlockSize { get; set; }

        /// <summary>
        ///     число нитей в блоке;
        /// </summary>
        [Category("Переменные")]
        [Description("число блоков в гриде")]
        [MiniMaxVariable(1, 3, 7, 15, 31, 63, 127)]
        [MiniMaxOutput]
        public decimal GridSize { get; set; }

        [Category("Расчёт")]
        [MiniMaxTarget(Target.Minimum)]
        [MiniMaxCalculated]
        [Description("агрегированный критерий TotalTime/Ba -> min")]
        public decimal Criteria
        {
            get
            {
                try
                {
                    return TotalTime/Ba;
                }
                catch (Exception)
                {
                    return Decimal.MaxValue;
                }
            }
        }

        #endregion
    }
}