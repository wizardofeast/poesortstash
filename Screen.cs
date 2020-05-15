using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const int KEYDELAY = 120;
        



        private readonly static Dictionary<int, (SlotConfig Stash,SlotConfig Inventory)> Configurations = new Dictionary<int, (SlotConfig Stash, SlotConfig Inventory)>()
        {
            {3440,(new SlotConfig(12,12,219,22,65,5),new SlotConfig(5,12,787,2579,65,5))}
        };


        private readonly InputSimulator simulator;
        private readonly IntPtr WindowHandle;
        private readonly int Left;
        private readonly int Top;
        private readonly int Width;
        private readonly int Height;


        public readonly SlotContainer Stash;
        public readonly SlotContainer Inventory;


        public Screen(string processName)
        {
            var processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
                throw new ArgumentOutOfRangeException($"Can not find process named {processName}");

            WindowHandle = processes[0].MainWindowHandle;

            if (Utils.GetWindowRect(WindowHandle, out Utils.Rectangle wsize))
            {
                Top = wsize.Top;
                Left = wsize.Left;
                Width = wsize.Right - wsize.Left;
                Height = wsize.Bottom - wsize.Top;
                Stash = new SlotContainer(Configurations[Width].Stash);
                Inventory = new SlotContainer(Configurations[Width].Inventory);
                simulator = new InputSimulator();
            }
            else
            {
                throw new InvalidOperationException("Can not get window size of the process {processNam}");
            }      
        }
        

        private Point<double> GetSlotPosition(SlotContainer container,int index)
        {
            var pp = container.GetSlotCenter(index);
            var x = ((Left + pp.X) * SCREEN_CONVERT) / Width;
            var y = ((Top + pp.Y) * SCREEN_CONVERT) / Height;
            return new Point<double>(x, y);
        }

        private void CollectItems(SlotContainer container)
        {
           
            for (int i = 0; i < container.Slots.Length; i++)
            {
                var position = GetSlotPosition(container,i);

                if (Utils.TryEmptyClipboard())
                {                    
                    simulator.Mouse.MoveMouseTo(position.X, position.Y);
                    Thread.Sleep(MOVEDELAY);
                    simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    Thread.Sleep(KEYDELAY);
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

        public void CollectStashItems() => CollectItems(Stash);

        public void CollectInventoryItems() => CollectItems(Inventory);


        private void FastMove(Point<double> position)
        {
            simulator.Mouse.MoveMouseTo(position.X, position.Y);         
            Thread.Sleep(MOVEDELAY);
            simulator.Mouse.LeftButtonClick();
            Thread.Sleep(BUTTONDELAY);
        }


        private void Move(Point<double> from,Point<double> to)
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


        public bool MoveToStash(ItemBase item, int slot)
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
            Move(from, to);
            Stash.Slots[slot] = item;
            container.Slots[index] = null;
            return true;
        }


        public void ClearInventory()
        {
            simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Thread.Sleep(KEYDELAY);
            for (int i = 0; i < Inventory.Slots.Length; i++)
            {
                var position = GetSlotPosition(Inventory, i);
                FastMove(position);
             }

            simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Thread.Sleep(KEYDELAY);
        }

        public void MoveToInventory(int offset)
        {
            simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
            Thread.Sleep(KEYDELAY);

            for (int i = 0; i < Inventory.Slots.Length; i++)
            {
                var index = offset + i;
                if (Stash.Slots[index] == null)
                    continue;

                var invIndex = Array.FindIndex(Inventory.Slots, n => n == null);

                if (invIndex == -1)
                    break;

                var position = GetSlotPosition(Stash, offset+i);
                FastMove(position);
                Inventory.Slots[invIndex] = Stash.Slots[index];
                Stash.Slots[index] = null;
                Thread.Sleep(20);
            }

            simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            Thread.Sleep(KEYDELAY);

        }


        public void Focus()
        {
            Utils.SetForegroundWindow(WindowHandle);
            simulator.Mouse.MoveMouseTo(100, 100);
            Thread.Sleep(MOVEDELAY);
            simulator.Mouse.LeftButtonClick();
            Thread.Sleep(BUTTONDELAY);           

        }



    }
}
