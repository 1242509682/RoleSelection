using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace RoleSelection;

public class MyPlayerData
    {
        public int Account { get; set; } // 玩家账号
        public string Name { get; set; } = ""; // 玩家名字
        public string Role { get; set; } = "无"; // 角色名称
        public MyPlayerData(int acc, string name = "", string role = "无")
        {
            this.Account = acc;
            this.Name = name ?? "";
            this.Role = role ?? "无";
        }
    }

public class Database
{
    #region 数据库表结构（使用Tshock自带的数据库作为存储）
    private readonly string TableName;
    public Database()
    {
        TableName = "RoleSelection";
        var sql = new SqlTableCreator(TShock.DB, new SqliteQueryCreator());
        sql.EnsureTableStructure(new SqlTable(TableName, //表名
            new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, AutoIncrement = true }, // 主键列
            new SqlColumn("Account", MySqlDbType.Int32), // 非空字符串列
            new SqlColumn("Name", MySqlDbType.TinyText) { NotNull = true }, // 非空字符串列
            new SqlColumn("Role", MySqlDbType.TinyText) { NotNull = true }
        ));
    }
    #endregion

    #region 创建数据方法
    public bool AddData(MyPlayerData data)
    {
        return TShock.DB.Query("INSERT INTO " + TableName + " (Account, Name, Role) VALUES (@0, @1, @2)",
            data.Account, data.Name, data.Role) != 0;
    }
    #endregion

    #region 更新数据方法
    public bool UpdateData(MyPlayerData data)
    {
        return TShock.DB.Query("UPDATE " + TableName + " SET Role = @0 WHERE Account = @1",
            data.Role, data.Account) != 0;
    }
    #endregion

    #region 获取玩家数据
    public MyPlayerData? GetData(int acc)
    {
        using var reader = TShock.DB.QueryReader("SELECT * FROM " + TableName + " WHERE Account = @0", acc);

        if (reader.Read())
        {
            return new MyPlayerData(
                acc: reader.Get<int>("Account"),
                name: reader.Get<string>("Name"),
                role: reader.Get<string>("Role")
            );
        }

        return null;
    }
    #endregion

    #region 获取所有玩家数据
    public List<MyPlayerData> GetAllData()
    {
        var data = new List<MyPlayerData>();
        using (var reader = TShock.DB.QueryReader("SELECT * FROM " + TableName))
        {
            while (reader.Read())
            {
                data.Add(new MyPlayerData(
                    acc: reader.Get<int>("Account"),
                    name: reader.Get<string>("Name"),
                    role: reader.Get<string>("Role")
                ));
            }
        }

        return data;
    }
    #endregion

    #region 更新玩家强制开荒数据
    public bool UpdateTShockDB(int acc, TShockAPI.PlayerData TSData)
    {
        if (TSData == null || !TSData.exists)
        {
            return false;
        }
        try
        {
            var data = TShock.CharacterDB.GetPlayerData(new TSPlayer(-1), acc);
            if (data != null && data.exists)
            {
                TShock.DB.Query("UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3, Inventory = @4 WHERE Account = @5;", new object[]
                {
                        TSData.health,
                        TSData.maxHealth,
                        TSData.mana,
                        TSData.maxMana,
                        string.Join<NetItem>("~", TSData.inventory),
                        acc
                });
            }
            return true;
        }
        catch (Exception ex)
        {
            TShock.Log.Error("错误：UpdateTShockDB " + ex.ToString());
            Console.WriteLine("错误：UpdateTShockDB " + ex.ToString());
            return false;
        }
    }
    #endregion

    #region 清理所有数据方法
    public bool ClearData()
    {
        return TShock.DB.Query("DELETE FROM " + TableName) != 0;
    }
    #endregion
}