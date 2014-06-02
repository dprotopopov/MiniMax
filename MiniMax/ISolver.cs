using MyMath;

namespace MiniMax
{
    /// Пусть дана следующая задача:
    /// max {C(x)=∑cixi|∑ajixi{≤=≥}bj, j = 1,m}, 
    public interface ISolver<T>
    {
        void Execute(IMiniMax<T> minimax, ref Vector<T> optimalVector, ref T optimalValue);
    }
}