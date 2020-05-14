
namespace PoE.SortStash
{
    public class SlotContainer
    {
        public readonly SlotConfig Config;

        public readonly ItemBase[] Slots;

        public SlotContainer(SlotConfig config)
        {
            Config = config;
            Slots = new ItemBase[config.Rows * config.Columns];
        }

        public Point<int> GetSlotCenter(int index)
        {
            int x = Config.Left + (((index - (index % Config.Rows)) / Config.Rows) * (Config.Size + Config.Border)) + ((Config.Size + 1) / 2);

            int y = Config.Top + ((index % Config.Rows) * (Config.Size + Config.Border)) + ((Config.Size + 1) / 2);

            return new Point<int>(x, y);

        }

    }
}
