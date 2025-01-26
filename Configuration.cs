using Newtonsoft.Json;
using TShockAPI;

namespace RoleSelection
{
    internal class Configuration
    {
        #region 实例变量
        [JsonProperty("插件开关", Order = 0)]
        public bool Enabled { get; set; } = true;
        [JsonProperty("清理钱币", Order = 1)]
        public bool IsACoin { get; set; } = false;
        [JsonProperty("进服清背包", Order = 2)]
        public bool JoinClearItem { get; set; } = false;
        [JsonProperty("免清物品表", Order = 3)]
        public List<int> ExemptList { get; set; } = new List<int>();
        [JsonProperty("数据储存", Order = 4)]
        public bool UseDBSave { get; set; } = true;
        [JsonProperty("角色表", Order = 6)]
        public List<MyData> MyDataList { get; set; } = new List<MyData>();
        #endregion

        #region 角色数据结构
        public class MyData
        {
            [JsonProperty("角色名", Order = 0)]
            public string Role { get; set; } = "";
            [JsonProperty("生命值", Order = 1)]
            public int maxHealth { get; set; } = 100;
            [JsonProperty("魔力值", Order = 2)]
            public int maxMana { get; set; } = 20;
            [JsonProperty("Buff", Order = 3)]
            public Dictionary<int, int> Buff { get; set; } = new Dictionary<int, int>();
            [JsonProperty("当前盔甲饰品", Order = 5)]
            public NetItem[] armor { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("第2套盔甲饰品", Order = 6)]
            public NetItem[] loadout2Armor { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("第3套盔甲饰品", Order = 7)]
            public NetItem[] loadout3Armor { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("玩家背包表", Order = 8)]
            public NetItem[] inventory { get; internal set; } = TShock.ServerSideCharacterConfig.Settings.StartingInventory.ToArray();
            [JsonProperty("猪猪存钱罐", Order = 9)]
            public NetItem[] PiggySlots { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("装备工具栏", Order = 10)]
            public NetItem[] MiscEquipSlots { get; internal set; } = Array.Empty<NetItem>();

        }
        #endregion

        #region 预设参数方法
        public void SetDefault()
        {
            ExemptList = new List<int>() { 71 };
            MyDataList = new List<MyData>()
            {
                new MyData()
                {
                    Role = "萌新",
                    maxHealth = TShock.ServerSideCharacterConfig.Settings.StartingHealth,
                    maxMana = TShock.ServerSideCharacterConfig.Settings.StartingMana,
                    Buff = new Dictionary<int, int>(){ { 11, -1 } },
                    inventory = TShock.ServerSideCharacterConfig.Settings.StartingInventory.ToArray(),
                    MiscEquipSlots = new[]{ new NetItem(5098,1,0),}
                },

                new MyData()
                {
                    Role = "战士",
                    maxHealth = 400,
                    maxMana = 200,
                    Buff = new Dictionary<int, int>(){ { 25,-1 } },
                    armor = new[]
                    {
                        new NetItem(3187, 1, 0),
                        new NetItem(3188, 1, 0),
                        new NetItem(3189, 1, 0),
                    },

                    inventory = new[]
                    {
                        new NetItem(65, 1, 81),
                    },
                },

                new MyData()
                {
                    Role = "射手",
                    maxHealth = 400,
                    maxMana = 20,
                    Buff = new Dictionary<int, int>(){ { 112, -1 } },
                    armor = new[]
                    {
                        new NetItem(3374, 1, 0),
                        new NetItem(3375, 1, 0),
                        new NetItem(3376, 1, 0),
                    },

                    inventory = new[]
                    {
                        new NetItem(964, 1, 82),
                        new NetItem(1349, 9999, 0),
                    }
                },

                new MyData()
                {
                    Role = "法师",
                    maxHealth = 400,
                    maxMana = 400,
                    Buff = new Dictionary<int, int>(){ { 6, -1 } },
                    armor = new[]
                    {
                        new NetItem(228, 1, 0),
                        new NetItem(229, 1, 0),
                        new NetItem(230, 1, 0),
                    },

                    inventory = new[]
                    {
                        new NetItem(4062, 1, 83),
                    },

                    MiscEquipSlots = new[]{ new NetItem(0,0,0), new NetItem(115, 1,0) }
                },

                new MyData()
                {
                    Role = "召唤",
                    maxHealth = 400,
                    maxMana = 200,
                    Buff = new Dictionary<int, int>(){ { 110, -1 } },
                    armor = new[]
                    {
                        new NetItem(238, 1, 0),
                        new NetItem(5068, 1, 0),
                        new NetItem(5001, 1, 0),
                    },

                    inventory = new[]
                    {
                        new NetItem(4913, 1, 81),
                        new NetItem(4273, 1, 83),
                    }
                },
            };
        }
        #endregion

        #region 读取与创建配置文件方法
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "角色选择系统.json");
        public void Write()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static Configuration Read()
        {
            if (!File.Exists(FilePath))
            {
                var NewConfig = new Configuration();
                NewConfig.SetDefault();
                new Configuration().Write();
                return NewConfig;
            }
            else
            {
                var jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }
        #endregion
    }
}