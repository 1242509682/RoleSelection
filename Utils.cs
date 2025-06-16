using System.Text;
using TShockAPI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

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

    #region 渐变着色方法 + 物品图标解析
    public static string TextGradient(string text)
    {
        // 如果文本中已包含 [c/xxx:] 自定义颜色标签，则不做渐变，只替换图标
        if (text.Contains("[c/"))
        {
            return ReplaceIconsOnly(text);
        }

        var name = new StringBuilder();
        int length = text.Length;

        for (int i = 0; i < length; i++)
        {
            char c = text[i];

            // 检查是否是图标标签 [i:xxx]
            if (c == '[' && i + 2 < length && text[i + 1] == 'i' && text[i + 2] == ':')
            {
                int end = text.IndexOf(']', i);
                if (end != -1)
                {
                    string tag = text.Substring(i, end - i + 1);
                    string content = tag[3..^1]; // 去掉 "[i:" 和 "]"

                    if (int.TryParse(content, out int itemID))
                    {
                        name.Append(ItemIcon(itemID));
                    }
                    else
                    {
                        name.Append(tag); // 无效ID保留原标签
                    }

                    i = end; // 跳过整个标签
                }
                else
                {
                    name.Append(c);
                    i++;
                }
            }
            else
            {
                // 渐变颜色计算
                var start = new Color(166, 213, 234);
                var endColor = new Color(245, 247, 175);
                float ratio = (float)i / (length - 1);
                var color = Color.Lerp(start, endColor, ratio);

                name.Append($"[c/{color.Hex3()}:{c}]");
            }
        }

        return name.ToString();
    }
    #endregion

    #region 只替换图标，不做渐变
    private static string ReplaceIconsOnly(string text)
    {
        var result = new StringBuilder();
        int index = 0;
        int length = text.Length;

        while (index < length)
        {
            char c = text[index];

            if (c == '[' && index + 2 < length && text[index + 1] == 'i' && text[index + 2] == ':')
            {
                int end = text.IndexOf(']', index);
                if (end != -1)
                {
                    string tag = text.Substring(index, end - index + 1);
                    string content = tag[3..^1];

                    if (int.TryParse(content, out int itemID))
                    {
                        result.Append(ItemIcon(itemID));
                    }
                    else
                    {
                        result.Append(tag);
                    }

                    index = end + 1;
                }
                else
                {
                    result.Append(c);
                    index++;
                }
            }
            else
            {
                result.Append(c);
                index++;
            }
        }

        return result.ToString();
    }
    #endregion

    #region 返回物品图标方法
    // 方法：ItemIcon，根据给定的物品对象返回插入物品图标的格式化字符串
    public static string ItemIcon(Item item)
    {
        return ItemIcon(item.type);
    }

    // 方法：ItemIcon，根据给定的物品ID返回插入物品图标的格式化字符串
    public static string ItemIcon(ItemID itemID)
    {
        return ItemIcon(itemID);
    }

    // 方法：ItemIcon，根据给定的物品整型ID返回插入物品图标的格式化字符串
    public static string ItemIcon(int itemID)
    {
        return $"[i:{itemID}]";
    }
    #endregion
}
