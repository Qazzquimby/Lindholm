# Lindholm

Lindholm automates your Overwatch custom game, letting you provide a quality server without constant manual intervention.  
At this time, Lindholm can:
- Launch Overwatch and set up your custom game server.
- Detect team size imbalances and autobalance.
- Skip post game screens.
- Scramble teams.  

With more features in development, including custom map rotations, discord integration, game logging and analytics.

Learn more at https://discord.gg/XdfYVr9 (and the rest of the readme).

## Is Lindholm for me?

The current version of Lindholm is designed to run classic Overwatch styled custom games, with the same number of players on each team, and may not function well in boss style servers.  

The current version of Lindholm is not designed to enforce any rules that make up some custom games, such as swapping players on death, or requiring hero switches.  

Outside of these cases, Lindholm is useful for hosts who want to provide a more consistent service with less tedious management.

## Getting Started

These instructions will get Lindholm configured on your system and running your server.

### Prerequisites

- Only been tested on Windows 10, and is unlikely to work on other operating systems.
- Battlenet, and Overwatch.
- At least one custom game preset saved in Overwatch for automatic setup.
- Optionally an alternate Battlenet account with Overwatch, if you wish to play while the server is running.
- Optionally a Windows virtual machine. Some systems will require Overwatch be the active window while Lindholm is running, making the computer effectively unusable without a virtual machine.  
- Optionally an additional copy of Overwatch on a different machine or VM to allow you to play on the custom server while it is being automated.

### Installing

Click "Clone or Download" -> Download Zip  

Extract files anywhere.

Src folder can safely be deleted, but is present if you want to see or modify the code.

### Configuration

Open cfg.yaml in a text editor such as notepad.

Ensure the elements marked "REQUIRED" are correct.

```BattlenetExecutableFilePath``` must be an absolute path to your Battle.net.exe.
This should usually be ```C:\Program Files (x86)\Battle.net\Battle.net.exe```

```OverwatchSettingsFilePath``` must be an absolute path to your Overwatch Settings_v0.ini
This should usually be ```C:\Users\YOUR_USER\Documents\Overwatch\Settings\Settings_v0.ini``` where "YOUR_USER" is your user folder.
If it is present at that location, leave ```OverwatchSettingsFilePath``` commented out ("#" symbol to the far left).
If it is not present at that location, remove the "#" symbol on the line, and replace the value with the correct path.

```ServerName``` is the name your server will be given. 
Blizzard blocks certain names based on length and keywords. If your name doesn't load properly, try using a different name.

```PresetName``` is the name of the preset that will be loaded when your custom game is started.
The Lindholm console will give an alert if it fails to find the preset you request. This might happen if Overwatch is taking a very long time to load on your system. If this happens, you can close Lindholm, set the preset manually, and restart Lindholm, as detailed in "Correcting Errors."

### Running Lindholm

Lindholm can be run in two ways, for convenience.

#### Full Setup Run

**This is currently unstable, and not recommended.**
This run is triggered when Lindholm is launched while Overwatch is closed.
This will launch Overwatch, set up your custom game, and run the game.
1. Configuration must be complete.  
2. Overwatch must be closed.  
3. Launch Lindholm.exe

#### Existing Server Run

This run is triggered when Lindholm is launched while Overwatch open.
This run assumes a server is already set up, properly configured, and running.
This lets you stop and restart the bot without needing to repeat server setup.

1. Configuration must be complete.
2. Overwatch must be open, with a custom server running.
2. Overwatch must be windowed. Alt-Enter switches to windowed mode.
2. A game must be currently ongoing. The game can't be waiting in the lobby.
3. Overwatch must be viewing the server's lobby screen. The lobby screen is reached from the main pause screen through esc -> show lobby.
3. **Overwatch must have contrast set to minimum in settings.** This is set automatically during the Full Setup Run.


## Interacting with Overwatch

Maybe Lindholm made a mistake (see github issues) or maybe you want to change settings or swap a player.
Making changes while Lindholm is running can lead to confusion as you and Lindholm wrestle over controlls. 

To make changes safely:
1. Close Lindholm (the command window, not Overwatch)
2. Make any changes in overwatch.
3. Navigate back to the lobby screen.
4. Launch Lindholm, which will start using the Existing Server Run execution.

If you find Lindholm has added too many bots (a result of failing to remove bots), you may choose not to intervene.  
This problem is usually fixed at the beginning of a new match, or when more players join.

## FAQ

- #### It crashes on the main menu with an error saying the menu failed to load, though the menu did load.
Something about your system is preventing the screenshot method from working. This is currently in development. If enough people announce having this problem on the discord I'll implement a work-around in the mean time.

- #### I want to swap players, or use chat in the automated server.
This is usually safe to do, though can cause problems if the bot tries to act while you're working. If anything goes wrong, see "Correcting Errors" above.

- #### Can I make changes in the server settings while the bot is running?
This will certainly cause problems, because the menus will obscure the bot's vision. 
1. Close Lindholm (the command window, not Overwatch)
2. Make your changes in settings.
3. Navigate back to the lobby screen.
4. Launch Lindholm, which will start using the Existing Server Run execution.

- #### Overwatch looks grey?  
Lindholm requires Overwatch's contrast to be set to minimum to see properly, and automatically sets the contrast when it launches Overwatch.  
You can set the contrast back to normal in Overwatch's settings.

- #### This got flagged by my antivirus!
Lindholm accesses your Overwatch settings to save you a lot of setup. Some antivirus programs may think that looks suspicious and shut it down. If you want to see for yourself, you can check the source code yourself (it's pretty short) and compile your own exe from the source code. We recommend Visual Studio 2017 for this.

## Built With

* [Overwatch-Custom-Game-Automation](https://github.com/ItsDeltin/Overwatch-Custom-Game-Automation) - Makes interfacing with Overwatch custom games possible.

## Authors

* **Deltin** - *Overwatch custom game interfacing* - [Github](https://github.com/ItsDeltin) - [Website](https://www.abyxa.net/)
* **Qazzquimby** - *Server level work* - [Github](https://github.com/Qazzquimby)

## Contributing

Not organized yet. Consider creating an issue or contacting me [here](https://discord.gg/XdfYVr9)


## License

This project is licensed under the MIT License.
