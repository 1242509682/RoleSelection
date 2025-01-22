# RoleSelection

- Authors: SAP、羽学
- Source:  TShock QQ Group: 816771079
- This is a Tshock server plugin mainly used for selecting character saves using commands.

## Update Log

```
v1.0.0
Use the /rl command to select character saves.
A small plugin improved for SAP, mainly for PVP servers.
```

## Commands

| Syntax                             | Alias  |       Permission       |                   Description                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /rl  | None |   role.use    |    Command menu    |
| /rl up <character name/number> | None |   role.use    |    Select character    |
| /rl list | None |   role.use    |    List existing characters    |
| /rl all | None |   role.use    |    List other players' characters    |
| /rl set <player name> <character name> | None |   role.admin    |    Modify specified player's character    |
| /rl add <character name> | None |   role.admin    |    Add character    |
| /rl del <character name> | None |   role.admin    |    Remove character    |
| /rl rm <player name> | None |   role.admin    |    Remove specified player's data    |
| /rl reset | None |   role.admin    |    Clear all player data    |
| /reload  | None |   tshock.cfg.reload    |    Reload configuration file    |

## Configuration
> Configuration file location： tshock/角色选择系统.json
```json
{
  "Plugin Enabled": true,
  "Clear Coins": false,
  "Character Table": [
    {
      "Character Name": "Warrior",
      "Health Points": 400,
      "Mana Points": 200,
      "Buffs": {
        "25": -1
      },
      "Armor and Accessories": [
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
      "Inventory": [
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
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love