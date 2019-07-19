# MTFPlus
A plugin that adds new subclasses to NTF Cadets.

# Usage

Place your classes in a .txt file (A MUST) into the MTFplus folder located at the AppData folder (windows), .config folder (Linux) or in a per-server basis (that is, in your server folder) if you're using the non-global config, if you're using the global one then the files will go in the same folder as MultiAdmin.exe

## Making a new class

Before anything else, Items and Roles are in the following link, they may be confusing without understanding what they are or where they come from: https://github.com/Grover-c13/Smod2/wiki/Enum-Lists

This is the template file. This one will be autogenerated first time you boot up the server (although I did something wrong, and it doesn't work on Linux. Going to be fixed).

`medic.txt`
```yaml
Inventory: SENIOR_GUARD_KEYCARD, P90, RADIO, DISARMER, MEDKIT, MEDKIT, MEDKIT, MEDKIT
Max: 2
Role: NTF_CADET
Probability: 80
Ammo5: 200
Ammo7: 70
Ammo9: 50
HP: 110
Broadcast: You're a <b>Medic</b>. Remember to heal your teammates!
```
You don't need to have every single field written. [Below is a table with every default value](https://github.com/RogerFK/MTFPlus#default-values). If some class is missing Probability, for example, it will use a default of 100 (default values can't be edited).

### ItemManager Support, and how to use it
Below is an example of an ItemManager usage for a "Heavy-Shotgun" class that uses the [HS8 Shotgun](https://github.com/Androxanik/HS8), using an item as `IM:XXX`:

`Heavy-Shotgun.txt`
```yaml
Inventory: MTF_COMMANDER_KEYCARD, E11_STANDARD_RIFLE, IM:105, MICROHID, RADIO, DISARMER, MEDKIT, WEAPON_MANAGER_TABLET
[...]
```
To add other ItemManager Items, just add `IM:XXX` where XXX is the number [as seen here.](https://github.com/Androxanik/ItemManager/wiki/Reserved-Psuedo-IDs)

### For reference, more examples and ideas are provided in the [wiki: https://github.com/RogerFK/MTFPlus/wiki](https://github.com/RogerFK/MTFPlus/wiki). Feel free to add yours.

# Configs
| Config Option | Value Type | Default Value | Description |
|:----------------------:|:-----------:|:--------------------:|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
| mtfp_enable | bool | true | Enables/disables this plugin completely |
| mtfp_debug | bool | false | Prints more info to the console and the MA logs |
| mtfp_aliases | string list | mtfp, mtfplus, m+ | Aliases to use the commands to spawn people as certain subclasses, or to check the list/info for a specific class. |
| mtfp_ranks | rank list | owner, admin, e-girl | Who can spawn people (everyone can still check the list) |
| mtfp_user_console_list | int | 2 | If normals users should be able to see the list of MTFPlus classes with `.mtfplist`. Three modes: 0: Nobody 1: Everybody, but only the names 2: Names, inventories and probabilities (recommended so people stop asking about it) |
| mtfp_list_delay | float | 0.3 | The delay for the plugin to check the list of players who spawned (don't set it to 0, or else the plugin will probably not work in some scenarios) |
| mtfp_delay | float | 0.1 | Delay for the class to be set after the user's role is changed (this shouldn't be too high, neither too low) |

# Default values
| Key | Default value | Definition |
|:-----------------:|:-------------------------------------------------------------------:|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:
| Inventory | `default_item_cadet` config from Smod2 | The items the guy will respawn with. Used as in Smod2 |
| Max | 1 | The maximum amount of players that can respawn as this class |
| Role | NTF_CADET | The role they will be after respawning. Yes, you can randomly respawn a peanut. |
| Probability | 100 | Works as a percentage. This is NOT the chance of one player to spawn as the class, this is the chance for the class to be spawned (and the player is randomly picked). So if it's too low, it will probably not respawn. |
| HP | Role's default HP | Max HP you want them to have. More realistic, as they show the player if they actually have 100% and it doesn't go over the bar |
| Ammo5/Ammo7/Ammo9 | Default ones that probably come from storm37k's Default Ammo plugin | Ammo5 is only used by the MTF E-11 Rifle. MP7 and Logicer use Ammo7.  COM15, P90 and USP use Ammo9. |
| Broadcast | Empty | Tells the player a broadcast after respawning as that class |
# Commands
Use the aliases (MTFPlus, m+. mtfp, or whatever the owner/admins told you), then input the following arguments:
| Command | Arguments | Description |
|---------|--------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| LIST | [ANYTHING] | Displays the subclasses name if LIST is typed alone, displays every subclass with their inventory if any other argument (1, true, full, complete...) is typed (example: m+ list ahfhahf) |
| DISPLAY | <subclass' name> | Display the complete info about a subclass based on its name |
| SPAWN | <player id/player's name> <subclass' name> | Spawns a player as a subclass. Useful for testing/events. |
# Simulate the plugin

In case you don't trust this plugin because you never respawned as a subclass (or anyone rages about it), here, have a simulation to check the chances: https://github.com/RogerFK/MTFPlusSim
