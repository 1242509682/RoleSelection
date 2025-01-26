using System.Text;
using TShockAPI;
using Microsoft.Xna.Framework;
using Terraria;

namespace RoleSelection;

internal class Utils
{
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
        for (var i = 0; i < words.Count; i++)
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

    #region 解析输入参数的距离 如:lf 100
    public static void Parse(List<string> part, out Dictionary<string, string> val, int Index)
    {
        val = new Dictionary<string, string>();
        for (int i = Index; i < part.Count; i += 2)
        {
            if (i + 1 < part.Count) // 确保有下一个参数
            {
                var key = part[i].ToLower();
                var value = part[i + 1];
                val[key] = value;
            }
        }
    }
    #endregion

    #region 解析输入参数的属性名 通用方法 如:lf = 生命
    public static void UpdatePT(Configuration.MyData newItem, Dictionary<string, string> itemVal)
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
                    if (int.TryParse(kvp.Value, out var lf)) newItem.maxHealth = lf;
                    propName = "生命";
                    break;

                case "ma":
                case "mana":
                case "魔力":
                    if (int.TryParse(kvp.Value, out var ma)) newItem.maxMana = ma;
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

    #region 将字符串转换为 NetItem 数组
    public static NetItem[] StringToItem(string inv)
    {
        if (string.IsNullOrEmpty(inv))
            return new NetItem[NetItem.MaxInventory];

        var items = inv.Split('~').Select(str =>
        {
            if (str == "0") return new NetItem();
            return NetItem.Parse(str);
        }).ToArray();

        if (items.Length < NetItem.MaxInventory)
        {
            Array.Resize(ref items, NetItem.MaxInventory);
        }

        return items;
    }
    #endregion

    #region 将字符串转换为 NetItem 数组 2（用来解析所有盔甲都在一个MaxInventory的情况）
    public static NetItem[] StringToItem2(string inv, int startIndex = 0, int length = -1)
    {
        if (string.IsNullOrEmpty(inv))
            return new NetItem[NetItem.MaxInventory];

        // 分割字符串并转换为 NetItem 数组
        var items = inv.Split('~')
                       .Where(str => !string.IsNullOrWhiteSpace(str)) // 过滤掉无效的字符串
                       .Skip(startIndex) // 跳过不需要的部分
                       .Take(length == -1 ? int.MaxValue : length) // 提取指定长度的部分
                       .Select(str => NetItem.Parse(str))
                       .ToArray();

        // 如果数组长度小于指定长度，则填充空的 NetItem 对象
        if (length > 0 && items.Length < length)
        {
            Array.Resize(ref items, length);
        }

        return items;
    }
    #endregion

    #region 将 Item数组 转换为字符串
    public static string ItemToString(Item[] items)
    {
        if (items == null || items.Length == 0)
            return string.Empty;

        var netItem = items
            .Where(item => item.netID != 0)
            .Select(item =>{ return item.ToString();})
            .Where(str => str != null); // 过滤掉转换失败的 null 值

        // 使用 "~" 连接字符串数组
        return string.Join("~", netItem);
    }
    #endregion

    #region 将 long 转换为 Color
    public static Color DecodeColor(long colorValue)
    {
        var r = (byte)(colorValue & 0xFF);
        var g = (byte)((colorValue >> 8) & 0xFF);
        var b = (byte)((colorValue >> 16) & 0xFF);
        var a = (byte)((colorValue >> 24) & 0xFF);
        return new Color(r, g, b, a);
    }
    #endregion

    #region Color 转换为 long（这个方法Tshock自带的）
    public static long EncodeColor(Color color)
    {
        return ((long)color.A << 24) | ((long)color.R << 16) | ((long)color.G << 8) | color.B;
    }
    #endregion
}
