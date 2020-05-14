using System;


namespace PoE.SortStash.Items
{
    public class Gem:ItemBase
    {
        private const string RARITYOFGEM = "Rarity: Gem";

        public readonly string[] Attributes;
        public readonly bool Corrupted;
        public readonly int Level;
        public readonly (long Current, long Next) Experience;
      
        private Gem(string name,string[] attributes, bool corrupted, int level, (long Current, long Next) experience)
            :base(ItemType.SkillGem,Rarity.Gem,name,1,1)
        {
        
            Attributes = attributes;            
            Corrupted = corrupted;
            Level = level;
            Experience = experience;
        }

        protected override int Compare(ItemBase other)
        {           

            if(other is Gem gem)
            {
                var result = Attributes.Compare(gem.Attributes);
                if (result == 0)
                    result = Corrupted.CompareTo(gem.Corrupted);
                if (result == 0)
                    result = Level.CompareTo(gem.Level);
                if (result == 0)
                    result = Experience.Current.CompareTo(gem.Experience.Current);
                return result;
            }
            else
            {
                throw new InvalidCastException("Other shoould be a gem also.");
            }           
        }

        public static bool TryParse(string clipboard,out ItemBase item)
        {
            item = null;

            string[] parts = clipboard.Split(new string[] { PARTSPLITTER }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return false;

            string[] part1 = parts[0].Split(NEWLINE,StringSplitOptions.RemoveEmptyEntries);

            if (part1.Length < 2)
                return false;


            if (part1[0] != RARITYOFGEM)
                return false;

            var name = part1[1];

            var part2 = parts[1].Split(NEWLINE, StringSplitOptions.RemoveEmptyEntries);

            var attributes = part2[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Array.Sort<string>(attributes);

            var level = 1;
            (long Current, long Next) experience = (0, 0);
            var corrupted = false;

            for (int i = 1; i < part2.Length; i++)
            {
                if(part2[i].StartsWith("Level: "))
                {
                    level = int.Parse(part2[i].Substring(7));                    
                }
   
                else if (part2[i].StartsWith("Experience: "))
                {
                    var exstrs = part2[i].Substring(12).Split('/');
                    experience = (long.Parse(exstrs[0].Replace(".","")), long.Parse(exstrs[1].Replace(".","")));
                }
            }

            for (int i = 2; i < parts.Length; i++)
            {
                var lines = parts[i].Split(NEWLINE, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < lines.Length; j++)
                {
                    if (lines[j] == CORRUPTED)
                        corrupted = true;
                }
            }


            item = new Gem(name, attributes, corrupted, level, experience);
            return true;
        }
    }
}
