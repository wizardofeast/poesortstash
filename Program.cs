using System;
using System.Linq;
using WindowsInput;

namespace PoE.SortStash
{
    public static class Program
    {
       
        static void Main(string[] args)
        {
            var screen = new Screen("PathOfExile_x64Steam");

            screen.Focus();         

            screen.CollectStashItems();
         
            var sorted = screen.Stash.Slots.Where(i => i != null).ToArray();

            Array.Sort<ItemBase>(sorted);

            var invSize = screen.Inventory.Slots.Length;

            for (int i = 0; i < sorted.Length; i++)
            {
                if (i % invSize == 0)
                {
                    screen.ClearInventory();
                    screen.MoveToInventory(i);
                }

                screen.MoveToStash(sorted[i], i);
            }          

        }  

    }
}
