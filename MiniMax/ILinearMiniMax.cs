using MyMath;

namespace MiniMax
{
    /// <summary>
    ///     Интерфейс задачи поиска максимума/минимума линейной функции Fx при заданных ограничениях AxRB
    ///     Пусть дана следующая задача:
    ///     max {C(x)=∑cixi|∑ajixi{≤=≥}bj, j = 1,m i = 1,n},
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILinearMiniMax<T>
    {
        /// <summary>
        ///     Матрица коэффициентов уравнений
        /// </summary>
        Matrix<T> A { get; set; }

        /// <summary>
        ///     Вектор правой части
        /// </summary>
        Vector<T> B { get; set; }

        /// <summary>
        ///     Вектор смысла ограничений
        /// </summary>
        Vector<Comparer> R { get; set; }

        /// <summary>
        ///     Коэффициенты целевой функции
        /// </summary>
        Vector<T> C { get; set; }

        /// <summary>
        ///     Искомая цель поиска
        /// </summary>
        Target Target { get; set; }
    }
}