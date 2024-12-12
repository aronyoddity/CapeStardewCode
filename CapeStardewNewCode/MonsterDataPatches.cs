using StardewValley;
using System.Collections.Generic;

namespace CapeStardewCode
{
    public class MonsterDataPatches
    {
        private static readonly Dictionary<string, string> MonsterDataUpdates = new Dictionary<string, string>
        {
            { "Custom_SeaCavernDepths3", "Pepper Rex:300/15/0/0/false/3000/114 .50/5/.04/3/5/.0/true/7/Pepper Rex" },
            { "Custom_SeaCavernDepths4", "Pepper Rex:300/15/0/0/false/3000/114 .50/5/.04/3/5/.0/true/7/Pepper Rex" },
            { "Custom_AreaSecretBossLvl4", "Pepper Rex:300/15/0/0/true/3000/875 .75/5/.04/3/5/.0/true/7/Pepper Rex" }
        };

        public static void ApplyMonsterDataPatch(string locationName)
        {
            if (MonsterDataUpdates.TryGetValue(locationName, out string? updateData))
            {
                string[] data = updateData.Split(':');
                string monsterName = data[0];
                string stats = data[1];

                // Parse and apply the stats
                var monsterData = Game1.content.Load<Dictionary<string, string>>("Data/Monsters");
                if (monsterData.ContainsKey(monsterName))
                {
                    monsterData[monsterName] = stats;
                }
                else
                {
                    monsterData.Add(monsterName, stats);
                }
            }
        }
    }
}
