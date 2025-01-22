using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace RoleSelection;

public class MyPlayerData
    {
        public int Account { get; set; } // ����˺�
        public string Name { get; set; } = ""; // �������
        public string Role { get; set; } = "��"; // ��ɫ����
        public MyPlayerData(int acc, string name = "", string role = "��")
        {
            this.Account = acc;
            this.Name = name ?? "";
            this.Role = role ?? "��";
        }
    }

public class Database
{
    #region ���ݿ��ṹ��ʹ��Tshock�Դ������ݿ���Ϊ�洢��
    private readonly string TableName;
    public Database()
    {
        TableName = "RoleSelection";
        var sql = new SqlTableCreator(TShock.DB, new SqliteQueryCreator());
        sql.EnsureTableStructure(new SqlTable(TableName, //����
            new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, AutoIncrement = true }, // ������
            new SqlColumn("Account", MySqlDbType.Int32), // �ǿ��ַ�����
            new SqlColumn("Name", MySqlDbType.TinyText) { NotNull = true }, // �ǿ��ַ�����
            new SqlColumn("Role", MySqlDbType.TinyText) { NotNull = true }
        ));
    }
    #endregion

    #region �������ݷ���
    public bool AddData(MyPlayerData data)
    {
        return TShock.DB.Query("INSERT INTO " + TableName + " (Account, Name, Role) VALUES (@0, @1, @2)",
            data.Account, data.Name, data.Role) != 0;
    }
    #endregion

    #region �������ݷ���
    public bool UpdateData(MyPlayerData data)
    {
        return TShock.DB.Query("UPDATE " + TableName + " SET Role = @0 WHERE Account = @1",
            data.Role, data.Account) != 0;
    }
    #endregion

    #region ��ȡ�������
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

    #region ��ȡ�����������
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

    #region �������ǿ�ƿ�������
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
            TShock.Log.Error("����UpdateTShockDB " + ex.ToString());
            Console.WriteLine("����UpdateTShockDB " + ex.ToString());
            return false;
        }
    }
    #endregion

    #region �����������ݷ���
    public bool ClearData()
    {
        return TShock.DB.Query("DELETE FROM " + TableName) != 0;
    }
    #endregion
}