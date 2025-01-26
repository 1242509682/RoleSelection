# RoleSelection

- Authors: SAP、羽学
- Source:  TShock QQ Group: 816771079
- This is a Tshock server plugin mainly used for selecting character saves using commands.

## Update Log

```
v1.0.2
Added database storage logic and its corresponding command: /rl db
When 'Data Storage' is enabled, you need to enter /rl number twice to normally change roles.
The first entry initializes; when opening the /rl command menu, a corresponding prompt will be seen.
After initialization ends, the menu page will display the current character status normally.
Added 'Item Table Exemption'.
Configuration options now include armor accessories slots 1-3:
This configuration option is only used for presetting initial equipment (the first 3 are armors and the next 7 are accessories).
There is a 2-second role-changing delay upon switching roles, which requires the player to move or attack to take effect (designed to correctly save the current character state).
Fixed the bug where changing roles would clear the player's inventory.
Disabling 'Use Database Storage' only sets the default role in the configuration items and clears all current items instantly (and it is done immediately).

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
| /rl db | None |   role.admin    |    Enable or disable Database storage    |
| /rl reset | /rl rs |   role.admin    |    Clear all player data    |
| /reload  | None |   tshock.cfg.reload    |    Reload configuration file    |

## Configuration
> Configuration file location： tshock/角色选择系统.json
```json
{
  "Plugin Enabled": true,
  "Clear Coins": false,
  "Clean Inventory on Login": false,
  "Item Table Exemption": [
    71
  ],
  "Data Storage": true,
  "Refresh Delay": 2000.0,
  "Character Table": [
    {
      "Role Name": "Newbie",
      "Health": 100,
      "Mana": 20,
      "Buff": {
        "11": -1
      },
      "Current Armor Accessories": [],
      "Armor Accessories Slot 1": [],
      "Armor Accessories Slot 2": [],
      "Armor Accessories Slot 3": [],
      "Inventory": [
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