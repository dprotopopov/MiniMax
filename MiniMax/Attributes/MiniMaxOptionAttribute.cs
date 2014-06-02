using System;

namespace MiniMax.Attributes
{
    /// <summary>
    ///     Описание пользовательской опции
    ///     Аргументом атрибута должно быть константное выражение, выражение typeof или выражение создания массива того же
    ///     типа, что и параметр атрибута
    ///     An attribute argument must be a constant expression, typeof expression or array creation expression of an attribute
    ///     parameter type
    ///     На типы аргументов, которые можно использовать с атрибутами, накладываются определенные ограничения. Обратите
    ///     внимание, что, помимо ограничений, указанных в сообщении об ошибке, НЕ ДОПУСКАЕТСЯ использовать в качестве
    ///     аргументов атрибутов следующие типы:
    ///     sbyte
    ///     ushort
    ///     uint
    ///     ulong
    ///     decimal
    /// </summary>
    public class MiniMaxOptionAttribute : Attribute
    {
        public MiniMaxOptionAttribute(params object[] values)
        {
            Values = values;
        }

        public object[] Values { get; set; }
    }
}