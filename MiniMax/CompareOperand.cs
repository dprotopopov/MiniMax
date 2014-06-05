namespace MiniMax
{
    /// <summary>
    ///     Система ограничений задана неравенствами смысла «≤», «≥» или равенством «=».
    /// </summary>
    public enum CompareOperand
    {
        Eq = 0,
        Ge = 1,
        Le = -1
    }
}