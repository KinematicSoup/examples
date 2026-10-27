# Star Fighter

This project is up to date with:

Reactor Multiplayer Engine version: 1.1.3+3 

Unity version: 6.3

### Getting Started

* Check out this project, open it in Unity 6.3.
* Install Reactor Multiplayer Engine from [https://www.kinematicsoup.com/](https://www.kinematicsoup.com/reactor/install/)
* Open the scene file Assets/Scenes/arena.unity
* Go the Reactor menu, select "Rebuild All"
* Click the `GameController` object in the hierarchy, go to the `ksRoomInfo` component, and click "Start Local Server".
* Enter play mode

### About This Demo
Star Fighter is a space shooter where players can play as a fighter ship, or as turret that is attached to a fighter ship
controlled by another player. There must be at least one other player playing as a fighter ship with an unoccupied turret
for you to play as a turret. It uses Reactor Multiplayer Engine which provides efficient syncing and server-authoritative 
physics simulation.

### Scenes
There are two scenes used in Star Figther, all found under the Assets/Scenes folder:
- title is the main menu, which currently does not directly interact with any KS stuff.
- arena is the scene that the server should be built and run from.

### Team mode
Star Fighter can be played as a free-for all or in teams. Currently the game mode must be configured when you publish
the server config files and cannot be changed in game. To play with teams, set the 'Number Of Teams' field in
the GameManager script on the GameController object in the Arena scene to a value greater than 1.

### Licence
See LICENSE.md
