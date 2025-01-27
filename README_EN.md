# RoleSelection

- Authors: SAP、羽学
- Source:  TShock QQ Group: 816771079
- This is a Tshock server plugin mainly used for selecting character saves using commands.

## Update Log

```
v1.0.4
Optimized code performance by inheriting PlayerData to save players' role items.
Improved the logic of /rl add command, allowing new role data to be added directly from the user's inventory (less dependency on configuration file editing).
The above improvements were contributed by “少司命” (Shaosiming).
Enhanced /rl list and /rl all commands (showing 1 role and player per page).
Using /rl list, you can see if you own a specific role - it displays (yes) alongside your data if you do, or (no) showing preset configuration data if not.
/rl all will only display information about the roles currently being used by the player.

v1.0.3
Optimized Performance: Improved the execution performance of the /rl up command.
Bug Fixes:
Fixed an occasional issue where rapid inputs of the up command would cause the previous role's data to overwrite the new role's data.
Resolved a bug that required players to input the /rl up command twice to switch roles when they had no existing role.
Addressed a bug where deleting a role did not clear the player's inventory.
Fixed an issue where executing the delete command for a specified offline player failed.
Configuration Changes:
Removed the "First Set of Armor Accessories" configuration option.
Removed the "Refresh Interval" configuration option.
Added "Equipped Toolbar" and "Piggy Bank" configuration options to the roles table.
Documentation Updates:
Enhanced documentation for the /rl del and /rl rm commands to clarify their usage, especially in cases of empty input triggering.

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
  "Plugin Switch": true,
  "Clear Coins": false,
  "Clear Inventory on Join": false,
  "Items Not to Clear": [
    71
  ],
  "Data Storage": true,
  "Roles Table": [
    {
      "Role Name": "Newbie",
      "Health": 100,
      "Mana": 20,
      "Buff": {
        "11": -1
      },
      "Current Armor Accessories": [],
      "Second Set of Armor Accessories": [],
      "Third Set of Armor Accessories": [],
      "Player Backpack": [
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
      "Piggy Bank": [],
      "Equipped Toolbar": [
        {
          "netID": 5098,
          "prefix": 0,
          "stack": 1
        }
      ]
    },
    {
      "Role Name": "Warrior",
      "Health": 400,
      "Mana": 200,
      "Buff": {
        "25": -1
      },
      "Current Armor Accessories": [
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
      "Second Set of Armor Accessories": [],
      "Third Set of Armor Accessories": [],
      "Player Backpack": [
        {
          "netID": 65,
          "prefix": 81,
          "stack": 1
        }
      ],
      "Piggy Bank": [],
      "Equipped Toolbar": []
    }
  ]
}
```
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love