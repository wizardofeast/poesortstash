using System;
using System.Linq;
using WindowsInput;

namespace PoE.SortStash
{
    public static class Program
    {
       
        static void Main(string[] args)
        {
            var pname = "PathOfExile_x64Steam";

            var screen = Utils.GetScreen(pname);

            var simulator = new InputSimulator();
            simulator.Mouse.MoveMouseTo(100,100);
            simulator.Mouse.LeftButtonClick();

            screen.ClearInventory(simulator);

            screen.CollectStashItems(simulator, 150);
         
            var sorted = screen.Stash.Slots.Where(i => i != null).ToArray();

            Array.Sort<ItemBase>(sorted);

            screen.MoveToInventory(0, simulator);

            for (int i = 0; i < screen.Inventory.Slots.Length; i++)
            {
                screen.MoveToStash(sorted[i], i, simulator, 100);                
            }
        }  

    }
}
