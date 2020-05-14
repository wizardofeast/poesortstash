using System;
using System.Collections.Generic;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace PoE.SortStash
{
    public class Screen
    {
        private const double SCREEN_CONVERT = 65535d;

        private const int MOVEDELAY = 70;
        private const int BUTTONDELAY = 70;
        private const int KEYDELAY = 50;



        private readonly static Dictionary<int, (SlotConfig Stash,SlotConfig Inventory)> Configurations = new Dictionary<int, (SlotConfig Stash, SlotConfig Inventory)>()
        {
            {3440,(new SlotConfig(12,12,219,22,65,5),new SlotConfig(5,12,787,2579,65,5))}
        };

        public readonly IntPtr WindowHandle;
        public readonly int Left;
        public readonly int Top;
        public readonly int Width;
        public readonly int Height;



        public readonly SlotContainer Stash;
        public readonly SlotContainer Inventory;

        public Screen(IntPtr windowHandle,int top,int left,int width,int height)
        {
            WindowHandle = windowHandle;
            Top = top;
            Left = left;
            Width = width;
            Height = height;
            Stash = new SlotContainer(Configurations[Width].Stash);
            Inventory = new SlotContainer(Configurations[Width].Inventory);
        }

        private Point<double> GetSlotPosition(SlotContainer container,int index)
        {
            var pp = container.GetSlotCenter(index);
            var x = ((Left + pp.X) * SCREEN_CONVERT) / Width;
            var y = ((Top + pp.Y) * SCREEN_CONVERT) / Height;
            return new Point<double>(x, y);
        }

        private void CollectItems(SlotContainer container, InputSimulator simulator, int delay)
        {
           
            for (int i = 0; i < container.Slots.Length; i++)
            {
                var position = GetSlotPosition(container,i);

                if (Utils.TryEmptyClipboard())
                {                    
                    simulator.Mouse.MoveMouseTo(position.X, position.Y);
                    Thread.Sleep(20);
                    simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    Thread.Sleep(delay);
                    if (Utils.TryReadClipBoardText(out string clipboard))
                    {
                        if (!string.IsNullOrEmpty(clipboard))
                        {                           
                            var item = ItemBase.Parse(clipboard);
                            if(item != null)
                            {
                                container.Slots[i] = item;
                            }                            
                        }
                    }
                }
            }
        }

        public void CollectStashItems(InputSimulator simulator, int delay) => CollectItems(Stash, simulator, delay);

        public void CollectInventoryItems(InputSimulator simulator, int delay) => CollectItems(Inventory, simulator, delay);



        private void FastMove(Point<double> position,InputSimulator simulator)
        {
            simulator.Mouse.MoveMouseTo(position.X, position.Y);         
            Thread.Sleep(MOVEDELAY);
            simulator.Mouse.LeftButtonClick();
            Thread.Sleep(BUTTONDELAY);
        }


        private void Move(Point<double> from,Point<double> to,InputSimulator simulator)
        {           
            simulator.Mouse.MoveMouseTo(from.X, from.Y);
            Thread.Sleep(MOVEDELAY);
            simulator.Mouse.LeftButtonClick();
            Thread.Sleep(BUTTONDELAY);
            simulator.Mouse.MoveMouseTo(to.X, to.Y);
            Thread.Sleep(MOVEDELAY);
            simulator.Mouse.LeftButtonClick();
            Thread.Sleep(BUTTONDELAY);
        }   


        public bool MoveToStash(ItemBase item, int slot, InputSimulator simulator, int delay)
        {
            if (Stash.Slots[slot] != null)
                return false;

            var container = Inventory;
            var index = Array.IndexOf(container.Slots, item);
            if (index == -1)
            {
                container = Stash;
                index = Array.IndexOf(container.Slots, item);
            }

            if (index == -1)
                return false;

            var from = GetSlotPosition(container, index);
            var to = GetSlotPosition(Stash, slot);
            Move(from, to, simulator);
            Stash.Slots[slot] = item;
            container.Slots[index] = null;
            return true;
        }


        public void ClearInventory(InputSimulator simulator)
        {
            simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Thread.Sleep(20);
            for (int i = 0; i < Inventory.Slots.Length; i++)
            {
                var position = GetSlotPosition(Inventory, i);
                FastMove(position, simulator);
             }

            simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Thread.Sleep(20);
        }

        public void MoveToInventory(int offset,InputSimulator simulator)
        {
            simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Thread.Sleep(20);

            for (int i = 0; i < Inventory.Slots.Length; i++)
            {
                var index = offset + i;
                if (Stash.Slots[index] == null)
                    continue;

                var invIndex = Array.FindIndex(Inventory.Slots, n => n == null);

                if (invIndex == -1)
                    break;

                var position = GetSlotPosition(Stash, offset+i);
                FastMove(position, simulator);
                Inventory.Slots[invIndex] = Stash.Slots[index];
                Stash.Slots[index] = null;
                Thread.Sleep(20);
            }

            simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Thread.Sleep(20);

        }



    }
}
