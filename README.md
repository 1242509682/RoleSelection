# RoleSelection 角色选择系统插件

- 作者: SAP、羽学
- 出处: Tshock官方群816771079
- 这是一个Tshock服务器插件，主要用于：使用指令选择角色存档

## 更新日志

```
v1.0.3
优化了/rl up指令执行性能
修复了偶尔输入up指令时：手速过快导致上个角色会覆盖下个角色的BUG
修复了没有角色时：需要输入2次/rl up才能转换角色bug
修复删除角色时玩家不会清理背包的bug
修复删除指定离线玩家时指令执行失败的bug
移除了【第1套盔甲饰品】配置项
移除了【刷新间隔】配置项
角色表加入了【装备工具栏】、【猪猪存钱罐】的配置项
补充了/rl del和/rl rm的指令说明(空值输入触发)

v1.0.2
加入数据库储存逻辑与其对应指令：/rl db
当开启`数据储存`时，需要输2次/rl 序号才能正常转职
第一遍是为了初始化，打开/rl指令菜单时会看见对应提示
初始化结束后就会菜单页就会正常显示当前角色状态了
加入了`免清物品表`
配置项加入了盔甲饰品1-3栏位：
该配置项只用于预设初始装备（前3个是盔甲后7个是饰品）
转职时会有2秒的转职延迟，需要玩家移动或攻击才会生效
（为了能正确保存当前角色而设计）
修复了转职时会清除玩家背包的BUG
关闭`使用数据库储存`只会设置配置项中的默认角色并清空当前所有物品（并且是瞬间完成的）

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
| /rl db | 无 |   role.admin    |    开启或关闭数据存储    |
| /reload  | 无 |   tshock.cfg.reload    |    重载配置文件    |

## 配置
> 配置文件位置：tshock/角色选择系统.json
```json
{
  "插件开关": true,
  "清理钱币": false,
  "进服清背包": false,
  "免清物品表": [
    71
  ],
  "数据储存": true,
  "角色表": [
    {
      "角色名": "萌新",
      "生命值": 100,
      "魔力值": 20,
      "Buff": {
        "11": -1
      },
      "当前盔甲饰品": [],
      "第2套盔甲饰品": [],
      "第3套盔甲饰品": [],
      "玩家背包表": [
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
      ],
      "猪猪存钱罐": [],
      "装备工具栏": [
        {
          "netID": 5098,
          "prefix": 0,
          "stack": 1
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
      "当前盔甲饰品": [
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
      "第2套盔甲饰品": [],
      "第3套盔甲饰品": [],
      "玩家背包表": [
        {
          "netID": 65,
          "prefix": 81,
          "stack": 1
        }
      ],
      "猪猪存钱罐": [],
      "装备工具栏": []
    },
    {
      "角色名": "射手",
      "生命值": 400,
      "魔力值": 20,
      "Buff": {
        "112": -1
      },
      "当前盔甲饰品": [
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
      "第2套盔甲饰品": [],
      "第3套盔甲饰品": [],
      "玩家背包表": [
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
      ],
      "猪猪存钱罐": [],
      "装备工具栏": []
    },
    {
      "角色名": "法师",
      "生命值": 400,
      "魔力值": 400,
      "Buff": {
        "6": -1
      },
      "当前盔甲饰品": [
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
      "第2套盔甲饰品": [],
      "第3套盔甲饰品": [],
      "玩家背包表": [
        {
          "netID": 4062,
          "prefix": 83,
          "stack": 1
        }
      ],
      "猪猪存钱罐": [],
      "装备工具栏": [
        {
          "netID": 0,
          "prefix": 0,
          "stack": 0
        },
        {
          "netID": 115,
          "prefix": 0,
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
      "当前盔甲饰品": [
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
      "第2套盔甲饰品": [],
      "第3套盔甲饰品": [],
      "玩家背包表": [
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
      ],
      "猪猪存钱罐": [],
      "装备工具栏": []
    }
  ]
}
```
## 反馈
- 优先发issued -> 共同维护的插件库：https://github.com/UnrealMultiple/TShockPlugin
- 次优先：TShock官方群：816771079
- 大概率看不到但是也可以：国内社区trhub.cn ，bbstr.net , tr.monika.love