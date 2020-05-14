
namespace PoE.SortStash
{
    public struct SlotConfig
    {

        public readonly int Rows;
        public readonly int Columns;
        public readonly int Top;
        public readonly int Left;
        public readonly int Size;
        public readonly int Border;

        public SlotConfig(int rows,int columns,int top,int left,int size,int border)
        {
            Rows = rows;
            Columns = columns;
            Top = top;
            Left = left;
            Size = size;
            Border = border;
        }

    }
}
