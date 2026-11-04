# FPS1K

This is a 1st person shooter sample project that supports over 1000 concurrent players in a single match in a randomly generated level.

### Getting Started

- Check out this project, open it in Unity 6.3.
- Open the scene file Assets/FPS1K/Scenes/RandomScene.unity
- Go the Reactor menu, select "Rebuild All"
- Click the `Room` object in the hierarchy, go to the `ksRoomInfo` component, and click "Start Local Server".
- Enter play mode

### Level Generation
The game scene is Assets/FPS1KDemo/Scenes/RandomScene. Levels are randomly generates from a list of Tile entities. Each tile prefab should be 40x40 and (0, 0, 0) should be centered in the middle of the
tile. There is no need to maintain a list of tile prefabs; the list is generated automatically when you build the scene config. You need to add a Tile script to the root game object of
each tile prefab. Tile prefabs are located in Assets/FPS1KDemo/Resources/Tiles. The tiles are randomly rotated to face one of 4 directions.

`srLevelGenerator` generates the level. It has a GridSize field for controlling the size of the grid. 14 or 15 is the target size for 1000 players. It will optionally remove dynamic 
rigid bodies from the level, which improves performance and allow for higher player counts.

### Sound effects
To create a new sound effect asset, right-click > Create -> Sound. Assign a sound clip. You can also assign a Volume, Pitch, Delay before playing, StartTime and EndTime. To play a Sound,
using SoundManager.Instance.Play(Sound, source, loop) where source is the game object that plays the sound.

### Weapons
Weapons are created from ksScriptAssets inheriting from sWeapon. To create a new weapon asset, create the ksScriptAsset in Assets/FPS1KDemo/Weapons. This asset contains the server configuration data 
for weapon. Then create a prefab with the same in Assets/FPS1KDemo/Resources/Weapons containing the weapon mesh to render on the client, and any client scripts the weapon uses. Then select the
Resouces/Entities/Avatar prefab and add the weapon ksScriptAsset to the seCharacter's weapons list. The weapon will be available to the player when cycling through weapons using the 'F' key.

### Adding Bots
Set the 'Num Bots' field on the srPlayerSpawner in the inspector for the Room object to set the number of bots for testing. The bots are very basic and run around randomly, sometimes shooting
straight ahead of them. They have unlimited ammo. Note that the default online servers cannot handle 1000 bots. If you wish to do a load test with high bot counts, you will need to contact us
to set up a more powerful server.

### Large Scale Testing
If you wisth to do large scale testing with 1000+ connections, contact us.