# RoleSelection 角色选择系统插件

- 作者: SAP、羽学
- 出处: Tshock官方群816771079
- 这是一个Tshock服务器插件，主要用于：使用指令选择角色存档

## 更新日志

```
v1.0.1
修复选择角色时没有装备的BUG
修复设置BUFF时会把所有角色BUFF都设置的BUG
直接使用/rl 序号，相当于直接执行 /rl up 序号
使用/rl del 角色时，会删除拥有该角色的玩家数据
加入了`进服清背包`的配置项
默认配置中补全了4职业的预设数据，
在不启用`进服清背包`时，当玩家没有数据时，默认会使用"萌新"角色

v1.0.0
使用/rl指令选择角色存档
帮SAP完善的小插件，主要用于PVP服务器
```

## 指令

| 语法                             | 别名  |       权限       |                   说明                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /rl  | 无 |   role.use    |    指令菜单    |
| /rl up <角色名/序号> | /rl 序号 |   role.use    |    选择角色    |
| /rl list | /rl l |   role.use    |    列出已有角色    |
| /rl all | /rl al |   role.use    |    列出其他玩家角色    |
| /rl set 玩家名 角色名 | /rl s 玩家名 角色名 |   role.admin    |    修改指定玩家角色    |
| /rl add 角色名 | /rl a 角色名 |   role.admin    |    添加角色    |
| /rl del 角色名 | /rl d 角色名 |   role.admin    |    删除角色    |
| /rl rm 玩家名 | /rl r 玩家名 |   role.admin    |    移除指定玩家数据    |
| /rl reset | /rl rs |   role.admin    |    清空所有玩家数据表    |
| /reload  | 无 |   tshock.cfg.reload    |    重载配置文件    |

## 配置
> 配置文件位置：tshock/角色选择系统.json
```json
{
  "插件开关": true,
  "清理钱币": false,
  "进服清背包": false,
  "角色表": [
    {
      "角色名": "萌新",
      "生命值": 100,
      "魔力值": 20,
      "Buff": {
        "11": -1
      },
      "盔甲饰品表": [],
      "背包表": [
        {
          "netID": -15,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": -13,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": -16,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 9,
          "prefix": 0,
          "stack": 500
        }
      ]
    },
    {
      "角色名": "战士",
      "生命值": 400,
      "魔力值": 200,
      "Buff": {
        "25": -1
      },
      "盔甲饰品表": [
        {
          "netID": 3187,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 3188,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 3189,
          "prefix": 0,
          "stack": 1
        }
      ],
      "背包表": [
        {
          "netID": 65,
          "prefix": 81,
          "stack": 1
        }
      ]
    },
    {
      "角色名": "射手",
      "生命值": 400,
      "魔力值": 20,
      "Buff": {
        "112": -1
      },
      "盔甲饰品表": [
        {
          "netID": 3374,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 3375,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 3376,
          "prefix": 0,
          "stack": 1
        }
      ],
      "背包表": [
        {
          "netID": 964,
          "prefix": 82,
          "stack": 1
        },
        {
          "netID": 1349,
          "prefix": 0,
          "stack": 9999
        }
      ]
    },
    {
      "角色名": "法师",
      "生命值": 400,
      "魔力值": 400,
      "Buff": {
        "6": -1
      },
      "盔甲饰品表": [
        {
          "netID": 228,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 229,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 230,
          "prefix": 0,
          "stack": 1
        }
      ],
      "背包表": [
        {
          "netID": 4062,
          "prefix": 83,
          "stack": 1
        }
      ]
    },
    {
      "角色名": "召唤",
      "生命值": 400,
      "魔力值": 200,
      "Buff": {
        "110": -1
      },
      "盔甲饰品表": [
        {
          "netID": 238,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 5068,
          "prefix": 0,
          "stack": 1
        },
        {
          "netID": 5001,
          "prefix": 0,
          "stack": 1
        }
      ],
      "背包表": [
        {
          "netID": 4913,
          "prefix": 81,
          "stack": 1
        },
        {
          "netID": 4273,
          "prefix": 83,
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