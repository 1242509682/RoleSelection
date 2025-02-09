﻿using Terraria;
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
    public override string Author => "SAP 羽学 少司命";
    public override Version Version => new Version(1, 0, 4);
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
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !plr.HasPermission("role.use") || !Config.Enabled) return;

        var data = DB.GetData(plr.Name); //获取玩家数据方法
        if (data == null) //如果没有获取到的玩家数据
        {
            data = new PlayerRole()
            {
                Name = plr.Name,
                Role = "萌新",
                Buff = new Dictionary<int, int>(),
            };

            DB.AddPlayer(data); //添加新数据

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
    internal static void Rank(TSPlayer plr, PlayerRole? data, MyData my)
    {
        if (data == null || my.Role == "无" || !plr.HasPermission("role.use") || !Config.Enabled) return;

        //不使用数据库存储
        if (!Config.UseDBSave)
        {
            ClearAll(plr); //清除所有
            ConfigRole(plr, my); //设置所有
            plr.SendMessage($"已清空背包使用角色 [c/F86570:{my.Role}]", 170, 170, 170);
        }
        else
        {
            var NewRole = new RoleData(plr, my.Role);
            var roles = DB.GetRole(plr.Account.ID);
            var data2 = roles.FirstOrDefault(p => p.Role == my.Role);
            var old = DB.GetData2(plr, data.Role);

            if (data2 == null)
            {
                //保存上个角色
                SaveOldRole(plr, data, old);
                ClearAll(plr);
                ConfigRole(plr, my);
                NewRole.CopyCharacter(plr);
                DB.SetAndUpdate(plr, NewRole, my.Role);
                plr.SendMessage($"已添加角色 [c/F74F5D:{my.Role}]", 170, 170, 170);
            }
            else
            {
                //保存上个角色
                SaveOldRole(plr, data, old);
                if (data.Role != my.Role)
                {
                    data2.RestoreCharacter(plr);
                    plr.SendMessage($"已更换角色 [c/8BE978:{data2.Role}]", 170, 170, 170);
                }
            }
        }

        data.Role = my.Role;
        data.Buff = my.Buff;
        DB.UpdatePlayer(data);
        SetBuff(plr); // 设置玩家BUFF
    }
    #endregion

    #region 保存上个角色方法
    private static void SaveOldRole(TSPlayer plr, PlayerRole? data, RoleData? old)
    {
        if (!string.IsNullOrEmpty(data!.Role) && old != null)
        {
            old.CopyCharacter(plr);
            DB.SetAndUpdate(plr, old, data.Role);
            plr.SendMessage($"已保存角色 [c/5B9DE1:{data.Role}]", 170, 170, 170);
        }
    }
    #endregion

    #region 设置玩家为配置文件角色方法
    internal static void ConfigRole(TSPlayer plr, MyData my)
    {
        var tplr = plr.TPlayer;
        if (tplr == null) return;

        // 设置玩家生命上限
        tplr.statLife = tplr.statLifeMax = my.maxHealth;
        plr.SendData(PacketTypes.PlayerHp, "", plr.Index);

        // 设置玩家魔力上限
        tplr.statMana = tplr.statManaMax = my.maxMana;
        plr.SendData(PacketTypes.PlayerMana, "", plr.Index);

        SetItem(plr, tplr.inventory, my.inventory, NetItem.InventoryIndex.Item1, NetItem.InventorySlots);
        SetItem(plr, tplr.armor, my.armor, NetItem.ArmorIndex.Item1, NetItem.ArmorSlots);
        SetItem(plr, tplr.miscEquips, my.miscEquip, NetItem.MiscEquipIndex.Item1, NetItem.MiscEquipSlots);
        SetItem(plr, tplr.bank.item, my.piggy, NetItem.PiggyIndex.Item1, NetItem.PiggySlots);
        SetItem(plr, tplr.Loadouts[1].Armor, my.loadout2Armor, NetItem.Loadout2Armor.Item1, NetItem.LoadoutArmorSlots);
        SetItem(plr, tplr.Loadouts[2].Armor, my.loadout3Armor, NetItem.Loadout3Armor.Item1, NetItem.LoadoutArmorSlots);
    }
    #endregion

    #region 设置物品方法
    private static void SetItem(TSPlayer plr, Item[] plrItem, NetItem[] items, int start, int MaxSlot)
    {
        if (items != null && items.Length > 0)
        {
            for (var i = 0; i < Math.Min(items.Length, MaxSlot); i++)
            {
                if (items[i].NetId != 0)
                {
                    plrItem[i] = TShock.Utils.GetItemById(items[i].NetId);
                    plrItem[i].stack = items[i].Stack;
                    plrItem[i].prefix = items[i].PrefixId;
                    plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i + start);
                }
            }
        }
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

    }

    private static void ClearSlots(Item[] items)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            // 如果是钱币 或免清物品表的物品ID则跳过
            if ((!Config.IsACoin && item.IsACoin) ||
                  Config.ExemptList.Contains(item.netID)) continue;
            item.TurnToAir();
        }
    }
    #endregion

    #region 设置BUFF方法
    public static void SetBuff(TSPlayer plr)
    {
        var data = DB.GetData(plr.Name);
        if (data == null || !Config.Enabled) return;

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