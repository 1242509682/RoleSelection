# RoleSelection 角色选择系统插件

- 作者: SAP、羽学
- 出处: Tshock官方群816771079
- 这是一个Tshock服务器插件，主要用于：使用指令选择角色存档

## 更新日志

```
v1.0.0
使用/rl指令选择角色存档
帮SAP完善的小插件，主要用于PVP服务器
```

## 指令

| 语法                             | 别名  |       权限       |                   说明                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /rl  | 无 |   role.use    |    指令菜单    |
| /rl up <角色名/序号> | 无 |   role.use    |    选择角色    |
| /rl list | 无 |   role.use    |    列出已有角色    |
| /rl all | 无 |   role.use    |    列出其他玩家角色    |
| /rl set 玩家名 角色名 | 无 |   role.admin    |    修改指定玩家角色    |
| /rl add 角色名 | 无 |   role.admin    |    添加角色    |
| /rl del 角色名 | 无 |   role.admin    |    移除角色    |
| /rl rm 玩家名 | 无 |   role.admin    |    移除指定玩家数据    |
| /rl reset | 无 |   role.admin    |    清空所有玩家数据表    |
| /reload  | 无 |   tshock.cfg.reload    |    重载配置文件    |

## 配置
> 配置文件位置：tshock/角色选择系统.json
```json
{
  "插件开关": true,
  "清理钱币": false,
  "角色表": [
    {
      "角色名": "战士",
      "生命值": 400,
      "魔力值": 200,
      "Buff": {
        "25": -1
      },
      "盔甲饰品表": [
        {
          "netID": 231,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 232,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 233,
          "prefix": 0,
          "stack": 1
        }
      ],
      "背包表": [
        {
          "netID": 273,
          "prefix": 81,
          "stack": 1
        }
      ]
    }
  ]
}
```
## 反馈
- 优先发issued -> 共同维护的插件库：https://github.com/UnrealMultiple/TShockPlugin
- 次优先：TShock官方群：816771079
- 大概率看不到但是也可以：国内社区trhub.cn ，bbstr.net , tr.monika.love