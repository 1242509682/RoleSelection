# RoleSelection

- Authors: SAP、羽学
- Source:  TShock QQ Group: 816771079
- This is a Tshock server plugin mainly used for selecting character saves using commands.

## Update Log

```
v1.0.9
Added chat prefix display for roles and its corresponding configuration option: "Use Chat Prefix"
Added support for setting buff-based punishment for illegal items.
Modified the usage of the /rl cl subcommand:
/rl cl 0 – Disable punishment for illegal items (1 = clear items, 2 = apply buff)

v1.0.8
Fixed an issue with the /rl rm command:
Could not delete player RoleData from the database.
No feedback message was provided upon successful execution of the command.
Added a "PageSize" configuration option:
Determines how many roles or players are displayed per page for the /rl list and /rl all commands.
Note: Directly using /rl s without any parameters will display the command instructions. The same applies to other commands as well.

v1.0.7
Added role lock functionality to the `/rl s` command:
Use **`/rl s <PlayerName> <RoleName> -L`** to lock or unlock a player’s ability to switch roles.
When locked, the player will be unable to use `/rl up` to change their current role.

v1.0.6
This version was compiled and released using the Nuget package of TShock 5.2.1 API.
Fixed an issue where the `/rl all` command couldn't view other players' role inventory.
Fixed an issue where the player name wasn't saved in the RoleData table.
Fixed a bug where new registered players wouldn't have data created upon joining, making `/rl up` unusable.
When clearing player data with `/rl rs`, it's no longer necessary for the player to rejoin; using `/rl` again will automatically create the data.

v1.0.5
This version was compiled and released using the Nuget package of TShock 5.2.1 API.
If there are bugs, users should update their Nuget according to their TShock version.
Added the ["Clear Illegal Items"] configuration option. When enabled:
Players using items from ["Exclusive Weapon Types"], ["Safe Item List"], ["Exempt Clear List"], or ["Allowed Items"] won't be penalized.
["Exclusive Weapon Types"]: 1-Melee / 2-Ranged / 3-Magic / 4-Summon (0 and -1 won't incur penalties).
Otherwise, using items from ["Prohibited Items"] or not belonging to ["Exclusive Weapon Types"] will result in immediate clearance.
This penalty only applies to players with the ["role.use"] permission; those with ["role.admin"] are immune.
Note: This penalty only takes effect when ["Data Storage"] is enabled;
["Legal Item List"] includes all items obtained by characters in the game that won't incur clearance penalties;
["Exempt Clear List"] includes items retained even when switching roles (both lists don't conflict).
Items already configured in the backpack list won't be cleared either, such as the Warrior's Star Wrath.
The clearance penalty only affects items currently being used; other items remain unaffected.
This function can be toggled via the `/rl cl` command.

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
| /rl  | None |   role.use    |    Command menu and create data   |
| /rl up <role/number> | /rl <number> |   role.use    |    Select character    |
| /rl list | /rl l |   role.use    |    List existing characters    |
| /rl all | /rl al |   role.use    |    List other players' characters    |
| /rl set <player name> <role> | /rl s |   role.admin    |    Modify specified player's character    |
| /rl set <player name> <role> -L | /rl s -L |   role.admin    |    Lock or unlock the player from switching roles    |
| /rl add <role> | /rl a |   role.admin    |    Add character    |
| /rl del <role> | /rl d |   role.admin    |    Remove character    |
| /rl rm <player name> | /rl r |   role.admin    |    Remove specified player's data    |
| /rl db | None |   role.admin    |    Enable or disable Database storage    |
| /rl clear <number> | /rl cl | role.admin | Illegal item punishment (0=off, 1=clear items, 2=set buff) |
| /rl reset | /rl rs |   role.admin    |    Clear all player data    |
| /reload  | None |   tshock.cfg.reload    |    Reload configuration file    |

## Configuration
> Configuration file location： tshock/角色选择系统.json
```json
{
  "Plugin Enabled": true,
  "UseChatPrefix": true,
  "ChatPrefixFormat": "[c/70A9CC:<{1}>] {2}:{3} {4}",
  "Clear Coins": false,
  "Clear Backpack on Join": false,
  "Exempt Clear List": [
    71
  ],
  "Data Storage": true,
  "Page Size": 5,
  "PunishmentForIllegalItems (1=clear, 2=add buff)": 0,
  "PunishmentBuffTable/seconds": {
    "149": 5
  },
  "Legal Item List": [],
  "Role Table": [
    {
      "Role Name": "Newbie",
      "Exclusive Weapon Type": 0,
      "Allowed Items": [],
      "Prohibited Items": [],
      "HP": 100,
      "MP": 20,
      "Buffs": {
        "11": -1
      },
      "Current Armor Accessories": [],
      "Second Set Armor Accessories": [],
      "Third Set Armor Accessories": [],
      "Player Inventory": [
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
      "Equipment Toolbar": [
        {
          "netID": 5098,
          "prefix": 0,
          "stack": 1
        }
      ]
    },
    {
      "Role Name": "Warrior",
      "Exclusive Weapon Type": 1,
      "Allowed Items": [],
      "Prohibited Items": [],
      "HP": 400,
      "MP": 200,
      "Buffs": {
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
      "Second Set Armor Accessories": [],
      "Third Set Armor Accessories": [],
      "Player Inventory": [
        {
          "netID": 65,
          "prefix": 81,
          "stack": 1
        }
      ],
      "Piggy Bank": [],
      "Equipment Toolbar": []
    },
    {
      "Role Name": "Archer",
      "Exclusive Weapon Type": 2,
      "Allowed Items": [],
      "Prohibited Items": [],
      "HP": 400,
      "MP": 20,
      "Buffs": {
        "112": -1
      },
      "Current Armor Accessories": [
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
      "Second Set Armor Accessories": [],
      "Third Set Armor Accessories": [],
      "Player Inventory": [
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
      "Piggy Bank": [],
      "Equipment Toolbar": []
    },
    {
      "Role Name": "Mage",
      "Exclusive Weapon Type": 3,
      "Allowed Items": [],
      "Prohibited Items": [],
      "HP": 400,
      "MP": 400,
      "Buffs": {
        "6": -1
      },
      "Current Armor Accessories": [
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
      "Second Set Armor Accessories": [],
      "Third Set Armor Accessories": [],
      "Player Inventory": [
        {
          "netID": 4062,
          "prefix": 83,
          "stack": 1
        }
      ],
      "Piggy Bank": [],
      "Equipment Toolbar": [
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
      "Role Name": "Summoner",
      "Exclusive Weapon Type": 4,
      "Allowed Items": [],
      "Prohibited Items": [],
      "HP": 400,
      "MP": 200,
      "Buffs": {
        "110": -1
      },
      "Current Armor Accessories": [
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
      "Second Set Armor Accessories": [],
      "Third Set Armor Accessories": [],
      "Player Inventory": [
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
      "Piggy Bank": [],
      "Equipment Toolbar": []
    }
  ]
}
```
## FeedBack
- Github Issue -> TShockPlugin Repo: https://github.com/UnrealMultiple/TShockPlugin
- TShock QQ Group: 816771079
- China Terraria Forum: trhub.cn, bbstr.net, tr.monika.love