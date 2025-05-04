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
        [JsonProperty("清理非法物品", Order = 5)]
        public bool ClearItem { get; set; } = true;
        [JsonProperty("合法物品表", Order = 6)]
        public HashSet<int> SecureItem { get; set; } = new HashSet<int>();
        [JsonProperty("角色表", Order = 7)]
        public List<MyData> MyDataList { get; set; } = new List<MyData>();
        #endregion

        #region 角色数据结构
        public class MyData : PlayerData
        {
            [JsonProperty("角色名", Order = -33)]
            public string Role { get; set; } = "";
            [JsonProperty("专属武器类型", Order = -32)]
            public int WeaponType { get; set; } = 0;
            [JsonProperty("允许使用物品ID", Order = -31)]
            public HashSet<int> AllowItem { get; set; } = new HashSet<int>();
            [JsonProperty("禁止使用物品ID", Order = -30)]
            public HashSet<int> DisableItem { get; set; } = new HashSet<int>();
            [JsonProperty("生命值", Order = -29)]
            public new int maxHealth { get; internal set; } = TShock.ServerSideCharacterConfig.Settings.StartingHealth;
            [JsonProperty("魔力值", Order = -28)]
            public new int maxMana { get; internal set; } = TShock.ServerSideCharacterConfig.Settings.StartingMana;
            [JsonProperty("Buff", Order = -27)]
            public Dictionary<int, int> Buff { get; set; } = new Dictionary<int, int>();
            [JsonProperty("当前盔甲饰品", Order = -26)]
            public NetItem[] armor { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("第2套盔甲饰品", Order = -25)]
            public NetItem[] loadout2Armor { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("第3套盔甲饰品", Order = -24)]
            public NetItem[] loadout3Armor { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("玩家背包表", Order = -23)]
            public new NetItem[] inventory { get; internal set; } = new RoleData().inventory;
            [JsonProperty("猪猪存钱罐", Order = -22)]
            public NetItem[] piggy { get; internal set; } = Array.Empty<NetItem>();
            [JsonProperty("装备工具栏", Order = -21)]
            public NetItem[] miscEquip { get; internal set; } = Array.Empty<NetItem>();

            public MyData() : base(new TSPlayer(-1)) { }
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
                    WeaponType = 0,
                    maxHealth = TShock.ServerSideCharacterConfig.Settings.StartingHealth,
                    maxMana = TShock.ServerSideCharacterConfig.Settings.StartingMana,
                    Buff = new Dictionary<int, int>(){ { 11, -1 } },
                    inventory = TShock.ServerSideCharacterConfig.Settings.StartingInventory.ToArray(),
                    miscEquip = new[]{ new NetItem(5098,1,0),}
                },

                new MyData()
                {
                    Role = "战士",
                    WeaponType = 1,
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
                    WeaponType = 2,
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
                    WeaponType = 3,
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

                    miscEquip = new[]{ new NetItem(0,0,0), new NetItem(115, 1,0) }
                },

                new MyData()
                {
                    Role = "召唤",
                    WeaponType = 4,
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