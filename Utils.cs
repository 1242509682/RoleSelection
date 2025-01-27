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
}
