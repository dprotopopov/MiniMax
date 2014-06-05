using System.Collections.Generic;
using MyLibrary.Trace;
using MyMath;

namespace MiniMax
{
    /// Пусть дана следующая задача:
    /// max {C(x)=∑cixi|∑ajixi{≤=≥}bj, j = 1,m},
    public interface ISolver<T>
    {
        bool Execute(ILinearMiniMax<T> minimax, ref IEnumerable<Vector<T>> optimalVectors,
            ref IEnumerable<T> optimalValues, ITrace trace);
    }
}