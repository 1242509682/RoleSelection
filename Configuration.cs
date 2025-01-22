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

        [JsonProperty("角色表", Order = 2)]
        public List<MyData> MyDataList { get; set; } = new List<MyData>();
        #endregion

        #region 角色数据结构
        public class MyData
        {
            [JsonProperty("角色名", Order = 1)]
            public string Role { get; set; } = "";
            [JsonProperty("生命值", Order = 2)]
            public int MaxHealth { get; set; } = 100;
            [JsonProperty("魔力值", Order = 3)]
            public int MaxMana { get; set; } = 20;
            [JsonProperty("Buff", Order = 4)]
            public Dictionary<int, int> Buff { get; set; } = new Dictionary<int, int>();
            [JsonProperty("盔甲饰品表", Order = 5)]
            public List<NetItem>? Armor { get; internal set; }
            [JsonProperty("背包表", Order = 6)]
            public List<NetItem>? Inventory { get; internal set; }
        } 
        #endregion

        #region 预设参数方法
        public void SetDefault()
        {
            MyDataList = new List<MyData>()
            {
                new MyData()
                {
                    Role = "战士",
                    MaxHealth = 400,
                    MaxMana = 200,
                    Buff = new Dictionary<int, int>(){ { 25,-1 } },
                    Armor = new List<NetItem>()
                    {
                        //熔岩套
                        new NetItem(231, 1, 0), 
                        new NetItem(232, 1, 0), 
                        new NetItem(233, 1, 0), 
                    },

                    Inventory = new List<NetItem>()
                    {
                        new NetItem(273, 1, 81), //永夜
                    }
                }
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