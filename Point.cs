
namespace PoE.SortStash
{
    public struct Point<T> where T:struct
    {
        public readonly T X;
        public readonly T Y;

        public Point(T x,T y)
        {
            X = x;
            Y = y;
        }


    }
}
