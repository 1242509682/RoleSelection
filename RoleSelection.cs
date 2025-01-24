using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using static RoleSelection.Configuration;

namespace RoleSelection;

[ApiVersion(2, 1)]
public class RoleSelection : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "角色选择系统";
    public override string Author => "SAP 羽学";
    public override Version Version => new Version(1, 0, 1);
    public override string Description => "使用指令选择角色存档";
    #endregion

    #region 全局变量
    internal static Configuration Config = new();
    public static Database DB = new();
    #endregion

    #region 注册与释放
    public RoleSelection(Main game) : base(game) { }

    public override void Initialize()
    {
        LoadConfig();
        GeneralHooks.ReloadEvent += ReloadConfig;
        GetDataHandlers.PlayerSpawn.Register(this.OnPlayerSpawn);
        ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
        TShockAPI.Commands.ChatCommands.Add(new Command("role.use", Commands.RoleCMD, "role", "class", "rl"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GeneralHooks.ReloadEvent -= ReloadConfig;
            GetDataHandlers.PlayerSpawn.UnRegister(this.OnPlayerSpawn);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, this.OnGreetPlayer);
            TShockAPI.Commands.ChatCommands.RemoveAll(x => x.CommandDelegate == Commands.RoleCMD);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置重载读取与写入方法
    private static void ReloadConfig(ReloadEventArgs args = null!)
    {
        LoadConfig();
        args.Player.SendInfoMessage("[角色选择系统]重新加载配置完毕。");
    }

    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Config.Write();
    }
    #endregion

    #region 玩家生成事件（给BUFF用）
    private void OnPlayerSpawn(object? sender, GetDataHandlers.SpawnEventArgs e)
    {
        var plr = e.Player;
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !Config.Enabled || !plr.HasPermission("role.use")) return;

        SetBuff(plr);
    }
    #endregion

    #region 进服更新创建数据方法
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !Config.Enabled) return;

        var data = DB.GetData(plr.Name); //获取玩家数据方法
        if (data == null) //如果没有获取到的玩家数据
        {
            data = new DBData()
            {
                Name = plr.Name,
                Role = "萌新",
                Buff = new Dictionary<int, int>()
            };

            DB.AddData(data); //添加新数据

            //设置新玩家职业
            foreach (var role in Config.MyDataList)
            {
                if (data.Role == role.Role)
                {
                    Rank(plr, data, role);
                }
            }
        }
        else
        {
            SetBuff(plr);
        }

        if (Config.JoinClearItem) //进服清理一切
        {
            ClearAll(plr);
        }
    }
    #endregion

    #region 选职业方法
    internal static void Rank(TSPlayer plr, DBData? data, Configuration.CData CData)
    {
        if (data == null || CData.Role == "无") return;

        data.Role = CData.Role;
        data.Buff = CData.Buff;
        ClearAll(plr); //清除所有
        SetAll(plr, CData); //设置所有

        DB.UpdateData(data);
        SetBuff(plr); //设置玩家BUFF
        plr.SendData(PacketTypes.PlayerSlot, "", plr.Index);
        plr.SendMessage($"您已选角为 {CData.Role}", 240, 250, 150);
    }
    #endregion

    #region 设置玩家所有状态方法
    internal static void SetAll(TSPlayer plr, object data)
    {
        var tplr = plr.TPlayer;
        if (tplr == null || tplr.inventory == null) return;

        // 设置玩家生命上限和魔力上限
        var maxHealth = 0;
        var maxMana = 0;

        //设置玩家物品栏和盔甲栏
        NetItem[] netInv = new NetItem[] { };
        NetItem[] netArmor = new NetItem[] { };

        //data可以是配置文件里的也可以是TS强制开荒里的，取决于使用SetAll方法时对data的传参
        if (data is CData cdata && cdata.inventory != null)
        {
            maxHealth = cdata.maxHealth;
            maxMana = cdata.maxMana;
            netInv = cdata.inventory;
            netArmor = cdata.armor ?? Array.Empty<NetItem>();
        }
        else if (data is PlayerData pd && pd.inventory != null)
        {
            maxHealth = pd.maxHealth;
            maxMana = pd.maxMana;
            netInv = pd.inventory;
            netArmor = Array.Empty<NetItem>();
        }

        //设置背包物品
        if (netInv != null && netInv.Length > 0)
        {
            for (var i = 0; i < Math.Min(netInv.Count(), NetItem.MaxInventory); i++)
            {
                var inv = netInv[i];
                if (inv.NetId != 0)
                {
                    tplr.inventory[i] = TShock.Utils.GetItemById(inv.NetId);
                    tplr.inventory[i].stack = inv.Stack;
                    tplr.inventory[i].prefix = inv.PrefixId;
                    plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i);
                    plr.PlayerData.StoreSlot(i, tplr.inventory[i].type, tplr.inventory[i].prefix, tplr.inventory[i].stack);
                }
            }
        }

        // 设置盔甲与饰品
        if (netArmor != null && netArmor.Length > 0)
        {
            for (var i = 0; i < Math.Min(netArmor.Count(), NetItem.MaxInventory); i++)
            {
                var armor = netArmor[i];
                if (armor.NetId != 0)
                {
                    tplr.armor[i] = TShock.Utils.GetItemById(armor.NetId);
                    tplr.armor[i].stack = armor.Stack;
                    tplr.armor[i].prefix = armor.PrefixId;
                    plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i + NetItem.ArmorIndex.Item1);
                    plr.PlayerData.StoreSlot(i + NetItem.ArmorIndex.Item1, tplr.armor[i].type, tplr.armor[i].prefix, tplr.armor[i].stack);
                }
            }
        }

        //设置玩家生命上限
        tplr.statLife = tplr.statLifeMax = maxHealth;
        plr.SendData(PacketTypes.PlayerHp, "", plr.Index);

        //设置玩家魔力上限
        tplr.statMana = tplr.statManaMax = maxMana;
        plr.SendData(PacketTypes.PlayerMana, "", plr.Index);

        DB.UpdateTShockDB(plr.Account.ID, plr.PlayerData);
    }
    #endregion

    #region 清除所有
    internal static void ClearAll(TSPlayer plr)
    {
        var tplr = plr.TPlayer;

        for (var i = 0; i < NetItem.MaxInventory; i++)
        {
            // 清理主物品栏、盔甲栏和其他装备栏
            ClearSlots(tplr.inventory);
            ClearSlots(tplr.armor);
            ClearSlots(tplr.dye);
            ClearSlots(tplr.miscEquips);
            ClearSlots(tplr.miscDyes);
            ClearSlots(tplr.bank.item);
            ClearSlots(tplr.bank2.item);
            ClearSlots(tplr.bank3.item);
            ClearSlots(tplr.bank4.item);
            tplr.trashItem.TurnToAir();
            foreach (var loadout in tplr.Loadouts)
            {
                ClearSlots(loadout.Armor);
                ClearSlots(loadout.Dye);
            }

            //清空所有物品
            plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i);
        }

        //还原玩家生命上限
        tplr.statLife = tplr.statLifeMax = TShock.ServerSideCharacterConfig.Settings.StartingHealth;
        plr.SendData(PacketTypes.PlayerHp, "", plr.Index);

        //还原玩家魔力上限
        tplr.statMana = tplr.statManaMax = TShock.ServerSideCharacterConfig.Settings.StartingMana;
        plr.SendData(PacketTypes.PlayerMana, "", plr.Index);

        //关闭所有被动永久增益
        tplr.unlockedBiomeTorches = tplr.extraAccessory =
        tplr.ateArtisanBread = tplr.usedAegisCrystal =
        tplr.usedAegisFruit = tplr.usedArcaneCrystal =
        tplr.usedGalaxyPearl = tplr.usedGummyWorm =
        tplr.usedAmbrosia = tplr.unlockedSuperCart = false;
        plr.SendData(PacketTypes.PlayerInfo, "", plr.Index, 0f, 0f, 0f, 0);

        // 清空玩家所有BUFF
        Array.Clear(tplr.buffType, 0, tplr.buffType.Length);
        plr.SendData(PacketTypes.PlayerBuff, "", plr.Index);

        DB.UpdateTShockDB(plr.Account.ID, plr.PlayerData);
    }

    private static void ClearSlots(Item[] items)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            // 如果是钱币则跳过
            if (!Config.IsACoin && item.IsACoin) continue;

            item.TurnToAir();
        }
    }
    #endregion

    #region 设置BUFF方法
    public static void SetBuff(TSPlayer plr)
    {
        var data = DB.GetData(plr.Name);
        if (data == null) return;

        var timeLimit = (int.MaxValue / 60 / 60) - 1;

        foreach (var buff in data.Buff)
        {
            int id = buff.Key;
            int time = buff.Value;
            if (time < 0 || time > timeLimit) time = timeLimit;
            plr.SetBuff(id, time * 60 * 60);
        }
    }
    #endregion
}