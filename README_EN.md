# RoleSelection

- Authors: SAP、羽学
- Source:  TShock QQ Group: 816771079
- This is a Tshock server plugin mainly used for selecting character saves using commands.

## Update Log

```
v1.0.1
Fixed the bug where characters had no equipment when selected.
Fixed the bug where setting buffs would apply to all characters.
Using /rl <number> now directly executes /rl up <number>.
Using /rl del <character> will delete player data associated with that character.
Added a `clear inventory on join` configuration option.
Default configuration now includes preset data for four classes.
When `clear inventory on join` is not enabled, players without data will default to the "Newbie" character.

v1.0.0
Use the /rl command to select character saves.
A small plugin improved for SAP, mainly for PVP servers.
```

## Commands

| Syntax                             | Alias  |       Permission       |                   Description                   |
| -------------------------------- | :---: | :--------------: | :--------------------------------------: |
| /rl  | None |   role.use    |    Command menu    |
| /rl up <role/number> | /rl <number> |   role.use    |    Select character    |
| /rl list | /rl l |   role.use    |    List existing characters    |
| /rl all | /rl al |   role.use    |    List other players' characters    |
| /rl set <player name> <role> | /rl s |   role.admin    |    Modify specified player's character    |
| /rl add <role> | /rl a |   role.admin    |    Add character    |
| /rl del <role> | /rl d |   role.admin    |    Remove character    |
| /rl rm <player name> | /rl r |   role.admin    |    Remove specified player's data    |
| /rl reset | /rl rs |   role.admin    |    Clear all player data    |
| /reload  | None |   tshock.cfg.reload    |    Reload configuration file    |

## Configuration
> Configuration file location： tshock/角色选择系统.json
```json
{
  "PluginEnabled": true,
  "ClearCoins": false,
  "ClearInventoryOnJoin": false,
  "CharacterList": [
    {
      "CharacterName": "Newbie",
      "Health": 100,
      "Mana": 20,
      "Buff": {
        "11": -1
      },
      "ArmorAccessoryList": [],
      "InventoryList": [
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
    // ... (other character presets)
  ]
}
```
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love