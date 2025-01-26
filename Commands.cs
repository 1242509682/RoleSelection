using System.Data;
using System.Text;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using static MonoMod.InlineRT.MonoModRule;
using static RoleSelection.Configuration;
using static RoleSelection.RoleSelection;

namespace RoleSelection;

public class Commands
{
    #region 主体指令方法
    public static void RoleCMD(CommandArgs args)
    {
        var plr = args.Player;
        var data = DB.GetData(plr.Name); //获取玩家自己的数据

        //子命令数量为0时
        if (args.Parameters.Count == 0) Help(plr, data);

        // 如果参数列表中只有一个元素，并且它可以被解析为整数，则认为它是索引并插入 "rank" 子命令
        if (args.Parameters.Count == 1 && int.TryParse(args.Parameters[0], out _))
        {
            args.Parameters.Insert(0, "rank"); // 在参数列表开头插入 "rank" 子命令
        }

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
                            plr.SendMessage("使用方法: /role up <序号|角色名>", 240, 250, 150);
                            plr.SendMessage("或者: /rl 序号", 240, 250, 150);
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
                                var InvText = role.inventory != null ?
                                    string.Join(" ", role.inventory.Select(item => Utils.GetItemsString(item))) : "";

                                var AmoText = role.armor != null ?
                                    string.Join(" ", role.armor.Select(item => Utils.GetItemsString(item))) : "";

                                InvText = Utils.Format(InvText, 30);
                                AmoText = Utils.Format(AmoText, 30);

                                // 发送带有索引的消息
                                plr.SendMessage($"\n[c/73E45C:{index}].角色:[c/5AE0D5:{role.Role}] " +
                                                $"生命:[c/F7636F:{role.maxHealth}] " +
                                                $"魔力:[c/5A9DE0:{role.maxMana}]\n" +
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
                                    maxHealth = TShock.ServerSideCharacterConfig.Settings.StartingHealth,
                                    maxMana = TShock.ServerSideCharacterConfig.Settings.StartingMana,
                                    Buff = new Dictionary<int, int>(),
                                    armor = new NetItem[] { },
                                    inventory = TShock.ServerSideCharacterConfig.Settings.StartingInventory.ToArray()
                                };

                                Utils.Parse(args.Parameters, out var val, 2);
                                Utils.UpdatePT(NewData, val);
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
                            if (plr2 == null) return;
                            var data2 = DB.GetData(other.Name);

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
                        else
                        {
                            plr.SendMessage("使用方法: /role set 玩家名 角色名", 240, 250, 150);
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
                                if (index <= 0 || index > Config.MyDataList.Count) break;

                                var role = Config.MyDataList[index - 1];
                                Config.MyDataList.RemoveAt(index - 1);
                                Config.Write();
                                DB.DelRole(role.Role);
                                plr.SendMessage($"已成功移除角色: [c/FF9667:{role.Role}]", 240, 250, 150);
                                break;
                            }
                            else // 如果参数是职业名称
                            {
                                var name = args.Parameters[1].ToLower();
                                for (var i = 0; i < Config.MyDataList.Count; i++)
                                {
                                    if (Config.MyDataList[i].Role.ToLower() != name) continue;

                                    var role = Config.MyDataList[i];
                                    Config.MyDataList.RemoveAt(i);
                                    Config.Write();
                                    DB.DelRole(role.Role);
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
                            var data2 = DB.GetData(other.Name);
                            if (data2 != null)
                            {
                                if (plr2 != null)
                                {
                                    ClearAll(plr2); //清除所有物品
                                }
                                else
                                {
                                    TShock.DB.Query($"DELETE FROM tsCharacter WHERE AccountID = {other.ID};");
                                }

                                DB.DelPlayer(other.Name);
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
                                ClearAll(plr2); //清除所有物品
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

}