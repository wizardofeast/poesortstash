using PoE.SortStash.Items;
using System;

namespace PoE.SortStash
{
    public abstract class ItemBase:IComparable<ItemBase>
    {
        private static readonly ItemTryParseDelegate[] parsers = new ItemTryParseDelegate[]
        {
            Gem.TryParse
        };


        public static ItemBase Parse(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {

                foreach (var parser in parsers)
                {
                    if (parser(value, out ItemBase item))
                        return item;
                }
            }

            return null;
        }


        protected const string PARTSPLITTER = "--------\r\n";
        protected const string CORRUPTED = "Corrupted";
        protected readonly static char[] NEWLINE = new char[] { '\r', '\n' };        
   
        public readonly ItemType Type;

        public readonly Rarity Rarity;

        public readonly string Name;

        public readonly int Width;

        public readonly int Height;

        protected ItemBase(ItemType type, Rarity rarity, string name,int width,int height)
        {
            Name = name;
            Rarity = rarity;
            Type = type;
            Width = width;
            Height = height;
        }

        protected abstract int Compare(ItemBase other);

        public int CompareTo(ItemBase other)
        {
            int result = Type.CompareTo(other.Type);
            if (result == 0)
                result = Name.CompareTo(other.Name);
            if (result == 0)
                result = Rarity.CompareTo(other.Rarity);
            if (result == 0)
                result = Compare(other);

            return result;            
        }     

        public static int Comparison(ItemBase x,ItemBase y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return 1;
            else if (y == null)
                return -1;
            else
                return x.CompareTo(y);
        }

    }
}
