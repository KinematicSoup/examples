# Star Fighter

This project is up to date with:

Reactor version: 1.1.3+3
Unity version: 6.1

### About This Demo
Star Fighter is a space shooter where players can play as a fighter ship, or as turret that is attached to a fighter ship
controlled by another player. There must be at least one other player playing as a fighter ship with an unoccupied turret
for you to play as a turret.

### Scenes
There are two scenes used in Star Figther, all found under the Assets/Scenes folder:
- title is the main menu, which currently does not directly interact with any KS stuff.
- arena is the scene that the server should be built and run from.

### Team mode
Star Fighter can be played as a free-for all or in teams. Currently the game mode must be configured when you publish
the server config files and cannot be changed in game. To play with teams, set the 'Number Of Teams' field in
the GameManager script on the GameController object in the Arena scene to a value greater than 1.

### Licence
You are free to use the code from this sample project in your own games. The music is creative commons and can be used freely
in your own projects. All other art assets are proprietary to KinematicSoup Technologies Inc and are protected by copyright.