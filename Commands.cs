﻿using System.Data;
using System.Text;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using static RoleSelection.Configuration;
using static RoleSelection.RoleSelection;

namespace RoleSelection;

public class Commands
{
    #region 主体指令方法
    public static void RoleCMD(CommandArgs args)
    {
        var plr = args.Player;
        var data = DB.GetData(plr.Account.ID); //获取玩家自己的数据

        //子命令数量为0时
        if (args.Parameters.Count == 0) Help(plr, data);

        //子命令数量超过1个时
        if (args.Parameters.Count >= 1)
        {
            switch (args.Parameters[0].ToLower())
            {
                case "u":
                case "up":
                case "rank":
                case "升级":
                    if (Config.MyDataList != null && data != null)
                    {
                        if (args.Parameters.Count >= 2)
                        {
                            var isIndex = int.TryParse(args.Parameters[1], out var index);

                            if (isIndex) // 如果参数是索引
                            {
                                // 确保索引有效且在范围内（考虑到用户可能从1开始计数）
                                if (index > 0 && index <= Config.MyDataList.Count)
                                {
                                    var role = Config.MyDataList[index - 1];
                                    Rank(plr, data, role);
                                }
                            }
                            else // 如果参数是职业名称
                            {
                                var name = args.Parameters[1].ToLower();
                                foreach (var role in Config.MyDataList)
                                {
                                    if (role.Role.ToLower() != name) continue;
                                    Rank(plr, data, role);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role up <索引|角色名>", 240, 250, 150);
                        }
                    }
                    break;

                case "l":
                case "list":
                case "列出":
                    if (Config.MyDataList != null && data != null)
                    {
                        var tplr = plr.TPlayer;
                        var index = 0; // 索引计数器

                        foreach (var role in Config.MyDataList)
                        {
                            if (role.Role != "无")
                            {
                                index++; // 每个有效角色增加索引

                                // 构建buff字符串
                                string buff;
                                if (role.Buff.Any())
                                {
                                    var buffNames = role.Buff.Keys.Select(buffId => Lang.GetBuffName(buffId));
                                    buff = string.Join(",", buffNames);
                                }
                                else
                                {
                                    buff = "无";
                                }

                                // 收集所有物品信息到一个字符串中
                                var InvText = role.Inventory != null ? string.Join(" ", role.Inventory.Select(item => GetItemsString(item))) : "";
                                var AmoText = role.Armor != null ? string.Join(" ", role.Armor.Select(item => GetItemsString(item))) : "";

                                InvText = Format(InvText, 30);
                                AmoText = Format(AmoText, 30);

                                // 发送带有索引的消息
                                plr.SendMessage($"\n[c/73E45C:{index}].角色:[c/5AE0D5:{role.Role}] 生命:[c/F7636F:{role.MaxHealth}] 魔力:[c/5A9DE0:{role.MaxMana}]\n" +
                                                $"buff: [c/FF9567:{buff}]\n" +
                                                $"装备: {AmoText}\n" +
                                                $"物品: {InvText}", 240, 250, 150);
                            }
                        }
                    }
                    break;

                case "a":
                case "add":
                case "添加":
                    if (plr.HasPermission("role.admin"))
                    {
                        if (args.Parameters.Count >= 2)
                        {
                            var name = args.Parameters[1];

                            // 检查是否有重复的角色名
                            if (Config.MyDataList.Any(data => data.Role.Equals(name, StringComparison.OrdinalIgnoreCase)))
                            {
                                plr.SendMessage($"角色名 [c/FF9667:{name}] 已存在，请选择其他名称。", 240, 250, 150);
                            }
                            else
                            {
                                var NewData = new MyData()
                                {
                                    Role = name,
                                    MaxHealth = 100,
                                    MaxMana = 20,
                                    Buff = new Dictionary<int, int>(),
                                    Armor = new List<NetItem>(),
                                    Inventory = new List<NetItem>(),
                                };

                                Parse(args.Parameters, out var val, 2);
                                UpdatePT(NewData, val);
                                Config.MyDataList.Add(NewData);
                                Config.Write();
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role add 角色名 生命 400 魔力 200", 240, 250, 150);
                        }
                    }
                    break;

                case "d":
                case "del":
                case "delete":
                case "删除":
                    if (plr.HasPermission("role.admin"))
                    {
                        if (args.Parameters.Count >= 2)
                        {
                            var isIndex = int.TryParse(args.Parameters[1], out var index);

                            if (isIndex) // 如果参数是索引
                            {
                                if (index > 0 && index <= Config.MyDataList.Count)
                                {
                                    var role = Config.MyDataList[index - 1];
                                    Config.MyDataList.RemoveAt(index - 1);
                                    Config.Write();
                                    plr.SendMessage($"已成功移除角色: [c/FF9667:{role.Role}]", 240, 250, 150);
                                }
                            }
                            else // 如果参数是职业名称
                            {
                                var name = args.Parameters[1].ToLower();
                                for (int i = 0; i < Config.MyDataList.Count; i++)
                                {
                                    if (Config.MyDataList[i].Role.ToLower() != name) continue;

                                    var role = Config.MyDataList[i];
                                    Config.MyDataList.RemoveAt(i);
                                    Config.Write();
                                    plr.SendMessage($"已成功移除角色: [c/FF9667:{role.Role}]", 240, 250, 150);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role del <索引|角色名>", 240, 250, 150);
                        }
                    }
                    break;

                case "s":
                case "set":
                case "设置":
                case "修改":
                    if (plr.HasPermission("role.admin"))
                    {
                        if (args.Parameters.Count >= 3)
                        {
                            var other = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                            var plr2 = TShock.Players.FirstOrDefault(p => p != null && p.IsLoggedIn && p.Active && p.Account.ID == other.ID);
                            var data2 = DB.GetData(other.ID);
                            if (plr2 != null)
                            {
                                var isIndex = int.TryParse(args.Parameters[2], out var index);
                                if (isIndex) // 如果参数是索引
                                {
                                    // 确保索引有效且在范围内（考虑到用户可能从1开始计数）
                                    if (index > 0 && index <= Config.MyDataList.Count)
                                    {
                                        var role = Config.MyDataList[index - 1];
                                        Rank(plr2, data2, role);
                                        plr.SendMessage($"玩家 [c/5B9DE1:{plr2.Name}] 角色设为 [c/FF9667:{role.Role}]", 240, 250, 150);
                                    }
                                }
                                else // 如果参数是职业名称
                                {
                                    var name = args.Parameters[2].ToLower();
                                    foreach (var role in Config.MyDataList)
                                    {
                                        if (role.Role.ToLower() != name) continue;

                                        Rank(plr2, data2, role);
                                        plr.SendMessage($"玩家 [c/5B9DE1:{plr2.Name}] 角色设为 [c/FF9667:{role.Role}]", 240, 250, 150);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role set 玩家名 角色名", 240, 250, 150);
                        }
                    }
                    break;

                case "r":
                case "rm":
                case "remove":
                case "移除":
                    if (plr.HasPermission("role.admin"))
                    {
                        if (args.Parameters.Count >= 2)
                        {
                            var other = TShock.UserAccounts.GetUserAccountByName(args.Parameters[1]);
                            var plr2 = TShock.Players.FirstOrDefault(p => p != null && p.IsLoggedIn && p.Active && p.Account.ID == other.ID);
                            var data2 = DB.GetData(other.ID);
                            if (data2 != null)
                            {
                                if (plr2 != null)
                                {
                                    ClearItem(plr2); //清除所有物品
                                }
                                else
                                {
                                    var del = $"DELETE FROM tsCharacter WHERE AccountID = {other.ID};";
                                    TShock.DB.Query(del);
                                }

                                data2.Role = "无";
                                DB.UpdateData(data2);
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role rm 玩家名", 240, 250, 150);
                        }
                    }
                    break;

                case "al":
                case "all":
                case "所有":
                    {
                        var datas = DB.GetAllData();
                        if (datas == null || !datas.Any())
                        {
                            plr.SendMessage("没有找到任何玩家数据。", 240, 250, 150);
                            break;
                        }

                        // 构建一个字符串列表来存储每条玩家信息
                        var mess = new List<string>();
                        var Index = 0;
                        foreach (var d in datas)
                        {
                            Index++;
                            // 每个玩家的信息行
                            var playerInfo = $"{Index}.玩家:[c/FF9667:{d.Name}] 角色:[c/5AE0D5:{d.Role}]";
                            mess.Add(playerInfo);
                        }

                        // 将所有信息合并成一个整体的消息，如果需要的话可以分段发送
                        var comb = string.Join("\n", mess);

                        // 如果消息过长，分段发送
                        for (int i = 0; i < comb.Length; i += 256)
                        {
                            plr.SendMessage(comb.Substring(i, Math.Min(256, comb.Length - i)), 240, 250, 150);
                        }
                    }
                    break;

                case "rs":
                case "reset":
                case "重置":
                    if (plr.HasPermission("role.admin"))
                    {
                        var user = TShock.UserAccounts.GetUserAccounts();
                        foreach (var acc in user)
                        {
                            var plr2 = TShock.Players.FirstOrDefault(p => p != null && p.IsLoggedIn && p.Active && p.Account.ID == acc.ID);
                            if (plr2 != null)
                            {
                                ClearItem(plr2); //清除所有物品
                                plr2.SendMessage("您的角色已被重置，请重进服务器", 240, 250, 150);
                            }
                        }

                        DB.ClearData();
                        TShock.DB.Query("delete from tsCharacter");
                        plr.SendMessage("已清空玩家数据表", 240, 250, 150);
                    }
                    break;

                default:
                    Help(plr, data);
                    break;
            }
        }
    }
    #endregion

    #region 菜单指令方法
    private static void Help(TSPlayer plr, MyPlayerData? data)
    {
        if (plr.HasPermission("role.admin"))
        {
            plr.SendMessage("角色选择指令菜单\n" +
                            "/rl up <角色名/序号> ——选择角色\n" +
                            "/rl list ——列出已有角色\n" +
                            "/rl all ——列出其他玩家角色\n" +
                            "/rl set 玩家名 角色名 ——修改玩家角色\n" +
                            "/rl add 角色名 ——添加角色\n" +
                            "/rl del 角色名 ——移除角色\n" +
                            "/rl rm 玩家名 ——移除指定玩家数据\n" +
                            "/rl reset ——清空所有玩家数据表", 240, 250, 150);
        }
        else
        {
            plr.SendMessage("角色选择指令菜单\n" +
                            "/rl up <角色名/序号> ——选择角色\n" +
                            "/rl all ——列出所有玩家角色\n" +
                            "/rl list ——列出已有角色", 240, 250, 150);
        }

        if (data != null)
        {
            plr.SendMessage($"您的角色为:[c/FEF766:{data.Role}]", 170, 170, 170);
        }
    }
    #endregion

    #region 升级方法
    private static void Rank(TSPlayer plr, MyPlayerData? data, Configuration.MyData role)
    {
        if (data == null) return;
        if (role.Role == "无") return;

        SetItem(plr, role);
        UpdateStats(plr, role);

        DB.UpdateTShockDB(plr.Account.ID, plr.PlayerData);
        plr.SendMessage($"您已选角为 {role.Role}", 240, 250, 150);

        data.Role = role.Role;
        DB.UpdateData(data);
        plr.SendData(PacketTypes.PlayerSlot, "", plr.Index);
    }
    #endregion

    #region 更新玩家属性方法
    private static void UpdateStats(TSPlayer plr, Configuration.MyData role)
    {
        var tplr = plr.TPlayer;

        //设置玩家生命上限
        tplr.statLife = role.MaxHealth;
        tplr.statLifeMax = role.MaxHealth;
        plr.SendData(PacketTypes.PlayerHp, "", plr.Index);

        //设置玩家魔力上限
        tplr.statMana = role.MaxMana;
        tplr.statManaMax = role.MaxMana;
        plr.SendData(PacketTypes.PlayerMana, "", plr.Index);

        //设置指定BUFF
        SetBuff(plr);
    }
    #endregion

    #region 获取物品的图标
    public static string GetItemsString(NetItem item)
    {
        if (item.NetId == 0) return "";
        else return item.PrefixId != 0 ? $"[i/p{item.PrefixId}:{item.NetId}] " : $"[i/s{item.Stack}:{item.NetId}] ";
    }
    #endregion

    #region 给出一个字符串和每行几个物品数，返回排列好的字符串，按空格进行分割
    public static string Format(string str, int num)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return "";
        }

        // 移除多余的空格并分割字符串为单词列表
        var words = str.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        // 如果需要每行显示的物品数大于等于单词总数，则直接返回原字符串
        if (num >= words.Count)
        {
            return string.Join(" ", words);
        }

        var list = new List<string>();
        for (int i = 0; i < words.Count; i++)
        {
            list.Add(words[i]);

            // 每满一行就添加换行符，但不要在最后一行之后添加
            if ((i + 1) % num == 0 && i < words.Count - 1)
            {
                list.Add("\n");
            }
            else if (i < words.Count - 1) // 非换行处添加空格分隔符，但不要在最后一个元素后添加
            {
                list.Add(" ");
            }
        }

        return string.Join("", list);
    }
    #endregion

    #region 更新所有物品方法
    private static void SetItem(TSPlayer plr, Configuration.MyData role)
    {
        var tplr = plr.TPlayer;
        if (tplr.inventory != null)
        {
            //清除所有物品
            ClearItem(plr);

            //设置背包物品
            if (role.Inventory != null)
            {
                for (var i = 0; i < Math.Min(role.Inventory.Count, NetItem.MaxInventory); i++)
                {
                    var inv = role.Inventory[i];
                    if (inv.NetId != 0)
                    {
                        tplr.inventory[i] = TShock.Utils.GetItemById(inv.NetId);
                        tplr.inventory[i].stack = inv.Stack;
                        tplr.inventory[i].prefix = inv.PrefixId;
                        plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i);
                    }
                }
            }

            // 设置盔甲与饰品
            if (role.Armor != null)
            {
                for (var i = 0; i < Math.Min(role.Armor.Count, NetItem.MaxInventory); i++)
                {
                    var armor = role.Armor[i];
                    if (armor.NetId != 0)
                    {
                        tplr.armor[i] = TShock.Utils.GetItemById(armor.NetId);
                        tplr.armor[i].stack = armor.Stack;
                        tplr.armor[i].prefix = armor.PrefixId;
                        plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i + NetItem.ArmorIndex.Item1);
                    }
                }
            }
        }
    }
    #endregion

    #region 清除所有物品(从ZhiPlayerManager抄来的)
    private static void ClearItem(TSPlayer plr)
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

            // 清理存档栏位
            foreach (var loadout in tplr.Loadouts)
            {
                ClearSlots(loadout.Armor);
                ClearSlots(loadout.Dye);
            }

            //清空所有物品
            plr.SendData(PacketTypes.PlayerSlot, "", plr.Index, i);

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

    #region 解析输入参数的距离 如:lf 100
    private static void Parse(List<string> part, out Dictionary<string, string> val, int Index)
    {
        val = new Dictionary<string, string>();
        for (int i = Index; i < part.Count; i += 2)
        {
            if (i + 1 < part.Count) // 确保有下一个参数
            {
                string propertyName = part[i].ToLower();
                string value = part[i + 1];
                val[propertyName] = value;
            }
        }
    }
    #endregion

    #region 解析输入参数的属性名 通用方法 如:lf = 生命
    private static void UpdatePT(MyData newItem, Dictionary<string, string> itemVal)
    {
        var mess = new StringBuilder();
        mess.Append($"已添加新角色: {newItem.Role} ");
        foreach (var kvp in itemVal)
        {
            string propName;
            switch (kvp.Key.ToLower())
            {
                case "lf":
                case "life":
                case "生命":
                    if (int.TryParse(kvp.Value, out var lf)) newItem.MaxHealth = lf;
                    propName = "生命";
                    break;

                case "ma":
                case "mana":
                case "魔力":
                    if (int.TryParse(kvp.Value, out var ma)) newItem.MaxMana = ma;
                    propName = "魔力";
                    break;
                default:
                    propName = kvp.Key;
                    break;
            }
            mess.AppendFormat("[c/94D3E4:{0}]([c/94D6E4:{1}]):[c/FF6975:{2}] ", propName, kvp.Key, kvp.Value);
        }

        TShock.Utils.Broadcast($"{mess}", 240, 250, 150);
    }
    #endregion
}