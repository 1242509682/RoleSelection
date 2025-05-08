using System.Data;
using System.Text;
using NuGet.Protocol.Plugins;
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
        var data = Db.GetData(plr.Name); //获取玩家自己的数据

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
                            //如果玩家被锁定角色则不执行任何操作
                            if (data.Lock)
                            {
                                plr.SendMessage($"你已被锁定 {data.Role} 角色，无法切换！", 255, 50, 50);
                                return;
                            }

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
                    var page = 1; // 默认第一页
                    if (args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out page))
                    {
                        ListRole(plr, page);
                    }
                    else
                    {
                        ListRole(plr, page = 1);
                    }
                    break;

                case "a":
                case "add":
                case "添加":
                    if (plr.HasPermission("role.admin"))
                    {
                        if (args.Parameters.Count >= 2)
                        {
                            var role = args.Parameters[1];

                            if (Config.MyDataList.Any(data => data.Role.Equals(role, StringComparison.OrdinalIgnoreCase)))
                            {
                                plr.SendMessage($"角色名 [c/FF9667:{role}] 已存在，请选择其他名称。", 240, 250, 150);
                            }
                            else
                            {
                                var NewData = TShock.CharacterDB.GetPlayerData(plr, plr.Account.ID);
                                var NewRole = new MyData()
                                {
                                    Role = role,
                                    maxHealth = plr.TPlayer.statLifeMax,
                                    maxMana = plr.TPlayer.statManaMax,
                                    Buff = new Dictionary<int, int>(),
                                    inventory = NewData.inventory
                                };

                                //获取指令使用者身上BUFF
                                for (var i = 0; i < plr.TPlayer.buffType.Length; i++)
                                    if (plr.TPlayer.buffTime[i] > 0 && plr.TPlayer.buffType[i] > 0)
                                        NewRole.Buff.Add(plr.TPlayer.buffType[i], plr.TPlayer.buffTime[i]);

                                NewData.CopyCharacter(plr);
                                Config.MyDataList.Add(NewRole);
                                Config.Write();
                                TShock.Utils.Broadcast($"管理员 [c/4792DD:{plr.Name}] 已添加新角色:[c/47DDD0:{role}]", 240, 250, 150);
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role add 角色名", 240, 250, 150);
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
                            var data2 = Db.GetData(other.Name);
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

                            //是否锁定角色
                            var lockRole = args.Parameters.Contains("-L");
                            if (lockRole)
                            {
                                data2!.Lock = !data2.Lock; // 切换锁的状态
                                Db.RoleLock(plr2.Name, data2.Lock); // 更新数据库中的锁定状态
                                var text = data2.Lock ? "[c/73E55C:锁定]" : "[c/F86470:解锁]";
                                plr.SendMessage($"角色：已{text}", 240, 250, 150);
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /rl s 玩家名 角色名", 240, 250, 150);
                            plr.SendMessage("锁定/解锁角色: /rl s 玩家名 角色名 -L", 240, 250, 150);
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
                                RestorePlayer(role); //删除角色时帮玩家回到萌新角色方法
                                Config.MyDataList.RemoveAt(index - 1);
                                Config.Write();
                                Db.DelRole(role.Role);
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
                                    RestorePlayer(role); //删除角色时帮玩家回到萌新角色方法
                                    Config.MyDataList.RemoveAt(i);
                                    Config.Write();
                                    Db.DelRole(role.Role);
                                    plr.SendMessage($"已成功移除角色: [c/FF9667:{role.Role}]", 240, 250, 150);

                                    break;
                                }
                            }
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role del <索引|角色名>", 240, 250, 150);
                            plr.SendMessage($"[c/F86470:注:]指令会使拥有该角色的玩家,恢复《角色表》里的第1个角色", 170, 170, 170);
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
                            var data2 = Db.GetData(other.Name);
                            if (data2 != null)
                            {
                                if (plr2 != null)
                                {
                                    ClearAll(plr2); //清除所有物品
                                }
                                else
                                {
                                    TShock.DB.Query($"DELETE FROM tsCharacter WHERE Account = {other.ID}");
                                }

                                Db.DelPlayer(other.Name);
                                foreach (var role in Config.MyDataList)
                                {
                                    Db.RmPlayer(other.ID, role.Role);
                                }
                            }

                            plr.SendMessage($"已成功移除玩家数据: [c/FF9667:{other.Name}]", 240, 250, 150);
                        }
                        else
                        {
                            plr.SendMessage("使用方法: /role rm 玩家名", 240, 250, 150);
                            plr.SendMessage($"[c/F86470:注:]玩家如果不在线会清理他的SSC强制开荒数据", 170, 170, 170);
                        }
                    }
                    break;

                case "al":
                case "all":
                case "所有":
                    var page2 = 1; // 默认第一页
                    if (args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out page2))
                    {
                        ListAll(plr, page2);
                    }
                    else
                    {
                        ListAll(plr, page2 = 1);
                    }
                    break;

                case "db":
                    if (plr.HasPermission("role.admin"))
                    {
                        var enabled = Config.UseDBSave;
                        Config.UseDBSave = !enabled;
                        var text = enabled ? "[c/F86470:禁用]" : "[c/73E55C:启用]";
                        Config.Write();
                        plr.SendMessage($"已{text}数据库储存功能。", 170, 170, 170);
                    }
                    break;

                case "cl":
                case "clear":
                    if (plr.HasPermission("role.admin"))
                    {
                        var enabled = Config.ClearItem;
                        Config.ClearItem = !enabled;
                        var text = enabled ? "[c/F86470:禁用]" : "[c/73E55C:启用]";
                        Config.Write();
                        plr.SendMessage($"已{text}非法物品清理功能。", 170, 170, 170);
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
                                plr2.SendMessage("您的角色已被重置，请重进服务器或使用一次指令:/rl", 240, 250, 150);
                            }

                            foreach (var role in Config.MyDataList)
                            {
                                Db.RmPlayer(acc.ID, role.Role);
                            }
                        }

                        Db.ClearData();
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
    private static void Help(TSPlayer plr, PlayerRole? data)
    {
        if (plr.HasPermission("role.admin"))
        {
            plr.SendMessage("角色选择指令菜单\n" +
                            "/rl 序号 ——选择角色\n" +
                            "/rl up <角色名/序号> ——选择角色\n" +
                            "/rl list ——列出已有角色\n" +
                            "/rl all ——列出其他玩家角色\n" +
                            "/rl s 玩家名 角色名 ——修改指定玩家角色\n" +
                            "/rl s 玩家名 角色名 -L ——锁定或解锁指定角色\n" +
                            "/rl add 角色名 ——以自己背包为基础添加新角色\n" +
                            "/rl del 角色名 ——移除角色\n" +
                            "/rl rm 玩家名 ——移除指定玩家数据\n" +
                            "/rl db ——开启|关闭数据存储\n" +
                            "/rl cl ——开启|关闭非法物品清理\n" +
                            "/rl rs ——清空所有玩家数据表", 240, 250, 150);
        }
        else
        {
            plr.SendMessage("角色选择指令菜单\n" +
                            "/rl 序号 ——选择角色\n" +
                            "/rl up <角色名/序号> ——选择角色\n" +
                            "/rl all ——列出其他玩家角色\n" +
                            "/rl list ——列出已有角色", 240, 250, 150);
        }

        if (data != null)
        {
            plr.SendMessage($"您的角色为:[c/FEF766:{data.Role}]({(data.Lock ? "已" : "未")}锁定)", 170, 170, 170);
        }
        else if (plr != TSPlayer.Server) //帮新注册的玩家建数据 双保险
        {
            data = CreateData(plr);
            plr.SendMessage($"您的角色为:[c/FEF766:{data.Role}]", 170, 170, 170);
        }

    }
    #endregion

    #region 用于移除指定角色时帮玩家回到萌新背包方法
    private static void RestorePlayer(MyData role)
    {
        var user = TShock.UserAccounts.GetUserAccounts();
        foreach (var acc in user)
        {
            Db.RmPlayer(acc.ID, role.Role);
            var cdata = Config.MyDataList.FirstOrDefault();
            if (cdata == null) continue;
            var data = Db.GetData(acc.Name);
            if (data != null && data.Role == role.Role)
            {
                var plr2 = TShock.Players.FirstOrDefault(p => p != null && p.IsLoggedIn && p.Active && p.Name == acc.Name);
                if (plr2 != null)
                {
                    plr2.SendMessage($"因[c/FFFFFF:{role.Role}]角色被管理员移除,已将您更换为:[c/5BE1DA:{cdata.Role}]", 170, 170, 170);
                    ClearAll(plr2); //清除所有
                    ConfigRole(plr2, cdata); //设置配置文件里的物品
                    data.Role = cdata.Role;
                    Db.UpdatePlayer(data);
                    SetBuff(plr2); //设置玩家BUFF
                }
                else
                {
                    TShock.DB.Query($"DELETE FROM tsCharacter WHERE Account = {acc.ID}");
                    data.Role = cdata.Role;
                    Db.UpdatePlayer(data);
                }
            }
        }
    }
    #endregion

    #region 列出自己角色方法
    private static void ListRole(TSPlayer plr, int page)
    {
        if (Config.MyDataList != null)
        {
            // 每页显示的角色数量
            var Size = Config.PageSize;

            // 总项目数和总页数
            var MaxItems = Config.MyDataList.Count(x => x != null);
            var MaxPages = (int)Math.Ceiling((double)MaxItems / Size);

            // 检查页码的有效性
            if (page < 1 || page > MaxPages)
            {
                plr.SendErrorMessage("无效的页码, 总共有 {0} 个。", MaxPages);
                return;
            }

            // 计算当前页的起始索引和结束索引
            var start = (page - 1) * Size;
            var end = Math.Min(start + Size, MaxItems);

            // 构建消息
            var mess = new StringBuilder();

            for (var i = start; i < end; i++)
            {
                var my = Config.MyDataList[i];

                if (my != null)
                {
                    // 构建buff字符串
                    string buff;
                    if (my.Buff.Any())
                    {
                        buff = string.Join(",", my.Buff.Keys.Select(buffId => Lang.GetBuffName(buffId)));
                    }
                    else
                    {
                        buff = "无";
                    }

                    // 收集所有物品信息到一个字符串中
                    string role;
                    int maxHealth;
                    int maxMana;
                    string InvText;
                    string AmoText;
                    string miscEquip;
                    string pigText;
                    var DBRole = Db.GetRole(plr.Account.ID);
                    var db = DBRole.FirstOrDefault(x => x.Role == my.Role);
                    if (DBRole != null && db != null)
                    {
                        role = db.Role + "(yes)";
                        maxHealth = db.maxHealth;
                        maxMana = db.maxMana;
                        InvText = Utils.Format(db.inventory != null ?
                            string.Join("  ", db.inventory.Take(NetItem.InventorySlots)
                            .Select(item => Utils.GetItemsString(item))) : "", 15);

                        AmoText = Utils.Format(db.inventory != null ?
                            string.Join(" ", db.inventory.Skip(NetItem.ArmorIndex.Item1).Take(NetItem.ArmorSlots)
                            .Select(item => Utils.GetItemsString(item))) : "", 20);

                        miscEquip = Utils.Format(db.inventory != null ?
                            string.Join(" ", db.inventory.Skip(NetItem.MiscEquipIndex.Item1).Take(NetItem.MiscEquipSlots)
                            .Select(item => Utils.GetItemsString(item))) : "", 7);

                        pigText = Utils.Format(db.inventory != null ?
                            string.Join(" ", db.inventory.Skip(NetItem.PiggyIndex.Item1).Take(NetItem.PiggySlots)
                            .Select(item => Utils.GetItemsString(item))) : "", 10);
                    }
                    else
                    {
                        role = my.Role + "(no)";
                        maxHealth = my.maxHealth;
                        maxMana = my.maxMana;
                        InvText = Utils.Format(my.inventory != null ?
                            string.Join("  ", my.inventory.Select(item => Utils.GetItemsString(item))) : "", 15);

                        AmoText = Utils.Format(my.armor != null ?
                        string.Join(" ", my.armor.Select(item => Utils.GetItemsString(item))) : "", 20);

                        miscEquip = Utils.Format(my.miscEquip != null ?
                        string.Join(" ", my.miscEquip.Select(item => Utils.GetItemsString(item))) : "", 7);

                        pigText = Utils.Format(my.piggy != null ?
                        string.Join(" ", my.piggy.Select(item => Utils.GetItemsString(item))) : "", 10);
                    }

                    // 发送带有索引的消息
                    mess.AppendLine($"[c/73E45C:{i + 1}].角色:[c/5AE0D5:{role}] " +
                                              $"生命:[c/F7636F:{maxHealth}] " +
                                              $"魔力:[c/5A9DE0:{maxMana}]\n" +
                                              $"buff:[c/FF9567:{buff}]\n" +
                                              $"装备:{AmoText}\n" +
                                              $"工具:{miscEquip}\n" +
                                              $"物品:{InvText}\n" +
                                              $"存钱罐:{pigText}\n");
                }
            }

            // 翻页提示信息
            var all = mess.ToString();
            if (page < MaxPages)
            {
                var prompt = $"请输入 [c/68A7E8:/rl list {page + 1}] 查看下一个";
                all += $"{prompt}";
            }
            else if (page > MaxPages)
            {
                var prompt = $"请输入 [c/68A7E8:/rl list {page - 1}] 查看上一个";
                all += $"{prompt}";
            }

            // 发送消息
            plr.SendMessage($"\n[c/FE727D:《角色列表》]第 [c/68A7E8:{page}] 个，共 [c/EC6AC9:{MaxPages}] 个:\n{all}", 255, 244, 150);
        }
    }
    #endregion

    #region 列出所有玩家当前角色
    private static void ListAll(TSPlayer plr, int page2)
    {
        var datas = Db.GetAllData();
        if (datas == null || !datas.Any())
        {
            plr.SendMessage("没有找到任何玩家数据。", 240, 250, 150);
            return;
        }

        // 总项目数和总页数
        var Size = Config.PageSize;
        var MaxItems = datas.Count;
        var MaxPages = (int)Math.Ceiling((double)MaxItems / Size); // 每页显示一个玩家

        // 检查页码的有效性
        if (page2 < 1 || page2 > MaxPages)
        {
            plr.SendErrorMessage("无效的页码, 总共有 {0} 个。", MaxPages);
            return;
        }

        // 计算当前页的起始索引
        var index = page2 - 1;

        // 获取当前页的玩家数据
        var data2 = datas[index];

        // 构建玩家信息
        var mess = new StringBuilder();
        mess.Append($"玩家:[c/FF9667:{data2.Name}] ");
        mess.Append($"角色:[c/5AE0D5:{data2.Role}] ");

        if (!string.IsNullOrEmpty(data2.Role))
        {
            var acc = TShock.UserAccounts.GetUserAccountByName(data2.Name);
            if (acc == null)
            {
                plr.SendMessage($"无法找到玩家 {data2.Name} 的账号信息。", 255, 100, 100);
                return;
            }
            var roles = Db.GetRole(acc.ID); // 获取该玩家的所有角色
            var dbRole = roles.FirstOrDefault(x => x.Role == data2.Role);
            if (dbRole != null)
            {
                mess.Append($"生命:[c/F7636F:{dbRole.maxHealth}] ");
                mess.Append($"魔力:[c/5A9DE0:{dbRole.maxMana}] ");

                string buff;
                if (data2.Buff.Any())
                {
                    buff = string.Join(",", data2.Buff.Keys.Select(buffId => Lang.GetBuffName(buffId)));
                }
                else
                {
                    buff = "无";
                }
                mess.AppendLine($"\nbuff:[c/FF9567:{buff}]");

                var InvText = Utils.Format(dbRole.inventory != null ?
                    string.Join("  ", dbRole.inventory.Take(NetItem.InventorySlots)
                    .Select(item => Utils.GetItemsString(item))) : "", 15);

                var AmoText = Utils.Format(dbRole.inventory != null ?
                    string.Join(" ", dbRole.inventory.Skip(NetItem.ArmorIndex.Item1).Take(NetItem.ArmorSlots)
                    .Select(item => Utils.GetItemsString(item))) : "", 20);

                var PigText = Utils.Format(dbRole.inventory != null ?
                    string.Join(" ", dbRole.inventory.Skip(NetItem.PiggyIndex.Item1).Take(NetItem.PiggySlots)
                    .Select(item => Utils.GetItemsString(item))) : "", 10);

                var miscEquip = Utils.Format(dbRole.inventory != null ?
                    string.Join(" ", dbRole.inventory.Skip(NetItem.MiscEquipIndex.Item1).Take(NetItem.MiscEquipSlots)
                    .Select(item => Utils.GetItemsString(item))) : "", 7);

                mess.AppendLine($"装备:{AmoText}");
                mess.AppendLine($"工具:{miscEquip}");
                mess.AppendLine($"物品:{InvText}");
                mess.AppendLine($"存钱罐:{PigText}");
            }
        }

        // 翻页提示信息
        var all = mess.ToString();
        if (page2 < MaxPages)
        {
            var prompt = $"\n请输入 [c/68A7E8:/rl all {page2 + 1}] 查看下一个";
            all += $"{prompt}";
        }
        else if (page2 > MaxPages)
        {
            var prompt = $"\n请输入 [c/68A7E8:/rl all {page2 - 1}] 查看上一个";
            all += $"{prompt}";
        }

        // 发送消息
        plr.SendMessage($"\n[c/FE727D:《玩家列表》]第 [c/68A7E8:{page2}] 个，共 [c/EC6AC9:{MaxPages}] 个:\n{all}", 255, 244, 150);
    }
    #endregion

}