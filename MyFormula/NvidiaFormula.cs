using System;
using System.ComponentModel;
using MiniMax;
using MiniMax.Attributes;

namespace MyFormula
{
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class NvidiaFormula
    {
        /// <summary>
        ///     число нитей в блоке;
        /// </summary>
        [Category("Переменные")]
        [Description("число нитей в блоке")]
        [MiniMaxVariable]
        public decimal N { get; set; }

        /// <summary>
        ///     число варпов;
        ///     Число варпов рассчитывается как частное числа нитей в блоке на число нитей в варпе.
        /// </summary>
        [Category("Переменные")]
        [Description("число варпов")]
        [MiniMaxCalculated]
        public decimal W
        {
            get { return Math.Ceiling(N/Nw); }
        }

        /// <summary>
        ///     число активных нитей на мультипроцессор;
        /// </summary>
        [Category("Переменные")]
        [Description("число активных нитей на мультипроцессор")]
        [MiniMaxTarget(Target.Maximum)]
        [MiniMaxCalculated]
        public decimal Na
        {
            get { return Math.Min(Nmax, Ba*N); }
        }

        /// <summary>
        ///     число активных варпов на мультипроцессор;
        /// </summary>
        [Category("Переменные")]
        [Description("число активных варпов на мультипроцессор")]
        [MiniMaxCalculated]
        public decimal Wa { get; set; }

        /// <summary>
        ///     число активных блоков на мультипроцессор;
        ///     Для эффективного использования GPU число активных нитей и активных блоков на мультипроцессоре должно быть
        ///     максимальным.
        /// </summary>
        [Category("Переменные")]
        [Description("число активных блоков на мультипроцессор")]
        [MiniMaxTarget(Target.Maximum)]
        [MiniMaxCalculated]
        public decimal Ba
        {
            get { return Math.Min(Bmax, Math.Floor(Wmax/W)); }
        }

        /// <summary>
        ///     число нитей в варпе;
        /// </summary>
        [Category("Переменные")]
        [Description("число нитей в варпе")]
        [MiniMaxVariable]
        public decimal Nw { get; set; }

        /// <summary>
        ///     максимальное число нитей на блок;
        /// </summary>
        [Category("Переменные")]
        [Description("максимальное число нитей на блок")]
        [MiniMaxConstant]
        public decimal Nbmax { get; set; }

        /// <summary>
        ///     максимальное число варпов на мультипроцессор;
        /// </summary>
        [Description("максимальное число варпов на мультипроцессор")]
        [MiniMaxConstant]
        public decimal Wmax { get; set; }

        /// <summary>
        ///     максимальное число блоков на мультипроцессор;
        /// </summary>
        [Category("Переменные")]
        [Description("максимальное число блоков на мультипроцессор")]
        [MiniMaxConstant]
        public decimal Bmax { get; set; }

        /// <summary>
        ///     максимальное число нитей на мультипроцессор;
        /// </summary>
        [Category("Переменные")]
        [MiniMaxConstant]
        [Description("максимальное число нитей на мультипроцессор")]
        public decimal Nmax { get; set; }

        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool waLeWmax
        {
            get { return Wa <= Wmax; }
        }

        /// Число активных варпов на мультипроцессоре – это произведение числа активных блоков на число варпов, и получившееся
        /// произведение должно быть меньше максимального числа варпов на мультипроцессоре.
        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool waEqBaW
        {
            get { return Wa == Ba*W; }
        }

        /// Максимальное число нитей на мультипроцессоре – это произведение максимального числа варпов на число нитей в варпе
        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool nmaxEqWmaxN
        {
            get { return Nmax == Wmax*N; }
        }

        /// Число нитей в блоке целое число, которое не может быть меньше единицы и больше максимального числа нитей в блоке.
        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool nGe1
        {
            get { return N >= 1; }
        }

        [Category("Ограничения")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool nLeNbmax
        {
            get { return N <= Nbmax; }
        }
    }
}