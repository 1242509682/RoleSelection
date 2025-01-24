using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TShockAPI;
using TShockAPI.DB;

namespace RoleSelection;

public class DBData
{
    public string Name { get; set; } = ""; // �������
    public string Role { get; set; } = "��"; // ��ɫ����
    public Dictionary<int, int> Buff { get; set; } = new Dictionary<int, int>();
    public DBData(string name = "", string role = "��", Dictionary<int, int> buff = null!)
    {
        this.Name = name ?? "";
        this.Role = role ?? "��";
        this.Buff = buff ?? new Dictionary<int, int>();
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
            new SqlColumn("Name", MySqlDbType.TinyText) 
            { 
                NotNull = true, Primary = true, Unique = true 
            },
            new SqlColumn("Role", MySqlDbType.TinyText) { NotNull = true },
            new SqlColumn("Buff", MySqlDbType.Text)
        ));
    }
    #endregion

    #region �������ݷ���
    public bool AddData(DBData data)
    {
        var buff = JsonConvert.SerializeObject(data.Buff);
        return TShock.DB.Query("INSERT INTO " + TableName + " (Name, Role, Buff) VALUES (@0, @1, @2)",
        data.Name, data.Role, buff) != 0;
    }
    #endregion

    #region �������ݷ���
    public bool UpdateData(DBData data)
    {
        var buff = JsonConvert.SerializeObject(data.Buff);
        return TShock.DB.Query("UPDATE " + TableName + " SET Role = @0, Buff = @1 WHERE Name = @2",
            data.Role, buff, data.Name) != 0;
    }
    #endregion

    #region �������ǿ�ƿ�������
    public bool UpdateTShockDB(int acc, TShockAPI.PlayerData TSData)
    {
        if (TSData == null || !TSData.exists) return false;

        return TShock.DB.Query(
            "UPDATE tsCharacter SET Health = @0, MaxHealth = @1, Mana = @2, MaxMana = @3, Inventory = @4 WHERE Account = @5",
            TSData.health, TSData.maxHealth, TSData.mana, TSData.maxMana, string.Join("~", TSData.inventory), acc) != 0;
    }
    #endregion

    #region ��ȡ�������
    public DBData? GetData(string name)
    {
        using var reader = TShock.DB.QueryReader("SELECT * FROM " + TableName + " WHERE Name = @0", name);

        if (reader.Read())
        {
            return new DBData(
                name: reader.Get<string>("Name"),
                role: reader.Get<string>("Role"),
                buff: JsonConvert.DeserializeObject<Dictionary<int, int>>(reader.Get<string>("Buff"))!
            );
        }

        return null;
    }
    #endregion

    #region ��ȡ�����������
    public List<DBData> GetAllData()
    {
        var data = new List<DBData>();
        using (var reader = TShock.DB.QueryReader("SELECT * FROM " + TableName))
        {
            while (reader.Read())
            {
                data.Add(new DBData(
                    name: reader.Get<string>("Name"),
                    role: reader.Get<string>("Role"),
                    buff: JsonConvert.DeserializeObject<Dictionary<int, int>>(reader.Get<string>("Buff"))!
                ));
            }
        }

        return data;
    }
    #endregion

    #region ɾ��ָ�����ݷ���
    public bool DelPlayer(string name)
    {
        return TShock.DB.Query("DELETE FROM " + TableName + " WHERE Name = @0", name) != 0;
    }

    public bool DelRole(string role)
    {
        return TShock.DB.Query("DELETE FROM " + TableName + " WHERE Role = @0", role) != 0;
    }
    #endregion

    #region �����������ݷ���
    public bool ClearData()
    {
        return TShock.DB.Query("DELETE FROM " + TableName) != 0;
    }
    #endregion
}