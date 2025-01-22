using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace RoleSelection;

[ApiVersion(2, 1)]
public class RoleSelection : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "角色选择系统";
    public override string Author => "SAP 羽学";
    public override Version Version => new Version(1, 0, 0);
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
        TShockAPI.Commands.ChatCommands.Add(new Command("role.use", Commands.RoleCMD, "role", "class","rl"));
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

    #region 进服更新创建数据方法
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !Config.Enabled) return;

        var data = DB.GetData(plr.Account.ID); //获取玩家数据方法

        if (data == null) //如果没有获取到的玩家数据
        {
            data = new MyPlayerData(plr.Account.ID, plr.Name, "无");
            DB.AddData(data); //添加新数据
        }
        else
        {
            foreach (var role in Config.MyDataList)
            {
                if (data.Role == role.Role)
                {
                    SetBuff(plr);
                }
            }
        }
    }
    #endregion

    #region 玩家生成事件（给BUFF用）
    private void OnPlayerSpawn(object? sender, GetDataHandlers.SpawnEventArgs e)
    {
        var plr = e.Player;
        if (plr == null || !plr.Active || !plr.IsLoggedIn || !Config.Enabled || !plr.HasPermission("role.use")) return;
        var data = DB.GetData(plr.Account.ID);
        if (data == null) return;
        if (data.Role == "无") return;

        foreach (var role in Config.MyDataList)
        {
            if (data.Role == role.Role)
            {
                SetBuff(plr);
            }
        }
    }
    #endregion

    #region 设置BUFF方法
    public static void SetBuff(TSPlayer plr)
    {
        var timeLimit = (int.MaxValue / 60 / 60) - 1;
        foreach (var role in Config.MyDataList)
        {
            foreach (var buff in role.Buff)
            {
                int id = buff.Key;
                int time = buff.Value;
                if (time < 0 || time > timeLimit) time = timeLimit;
                plr.SetBuff(id, time * 60 * 60);
            }
        }
    } 
    #endregion
}