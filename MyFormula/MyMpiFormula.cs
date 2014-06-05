using System;
using System.ComponentModel;
using MiniMax;
using MiniMax.Attributes;

namespace MyFormula
{
    /// <summary>
    ///     ��� ������������ ������������� ������������ � ������������ ���������� ��� ���������� �������������� ����������
    ///     �����, ������������ � ������������ ���������� ���������� �������� ������. � ������������ ���������� �����
    ///     ������������������ ��������� ������� ������ ����������, �� � �� �� ����� � ������������ ���������� ����� �����
    ///     ���������� ����� �� ����������� �� ������. �������� �� �������� ��� �������, ���� ���� ������� ���������
    ///     �������������� ������:
    /// </summary>
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class MyMpiFormula
    {
        /// <summary>
        ///     ������� ����� �� ����������� ����������;
        /// </summary>
        [Category("���������")]
        [MiniMaxConstant]
        [MiniMaxInput]
        [Description("������� ����� �� ����������� ����������")]
        public decimal C { get; set; }

        /// <summary>
        ///     ������� ����� �� ����������� ������ ��������;
        /// </summary>
        [Category("���������")]
        [MiniMaxConstant]
        [MiniMaxInput]
        [Description("������� ����� �� ����������� ������ ��������")]
        public decimal F { get; set; }

        /// <summary>
        ///     ������������ ���������� ��������� MPI;
        /// </summary>
        [Category("����������")]
        [MiniMaxVariable(1, 3, 7, 15, 31, 63, 127)]
        [MiniMaxOutput]
        [Description("������������ ���������� ��������� MPI")]
        public decimal P { get; set; }

        /// <summary>
        ///     ���������� ����� ��������������� � ������������ ��������� ���������� �������
        ///     ��� ��������� i-���� ����� �������;
        /// </summary>
        [Category("�����")]
        [Description(
            "������� ���������� ����� ��������������� � ������������ ��������� ���������� ������� ��� ��������� i-���� ����� �������"
            )]
        [MiniMaxOption(1, 3, 7, 15, 31, 63, 127, 255)]
        [MiniMaxInput]
        public decimal Xi { get; set; }

        /// <summary>
        ///     ����� ��������� i-��� ����� ������� ����� ���������;
        /// </summary>
        [Category("�����")]
        [Description("������� ����� ��������� i-��� ����� ����� ���������")]
        [MiniMaxOption(0.1, 1, 10)]
        [MiniMaxInput]
        public decimal Di { get; set; }

        /// <summary>
        ///     ����� ��������� ������ � i-��� ����� ��������� �������.
        /// </summary>
        [Category("�����")]
        [Description("������� ����� ��������� ������ � i-��� ����� ��������� �������")]
        [MiniMaxOption(1, 10, 100, 1000, 10000)]
        [MiniMaxInput]
        public decimal Bi { get; set; }

        /// <summary>
        ///     ���������� ������ �������;
        /// </summary>
        [Category("�����")]
        [Description("���������� ������ �������")]
        [MiniMaxOption(1, 10, 100, 1000, 10000)]
        [MiniMaxInput]
        public decimal Q { get; set; }

        [Category("������")]
        [MiniMaxTarget(Target.Minimum)]
        [MiniMaxCalculated]
        [Description("����� �� ����������")]
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

        [Category("�����������")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool xiGe1
        {
            get { return Xi >= 1; }
        }

        [Category("�����������")]
        [MiniMaxRestriction]
        [MiniMaxCalculated]
        public bool xiLeP
        {
            get { return Xi <= P; }
        }
    }
}