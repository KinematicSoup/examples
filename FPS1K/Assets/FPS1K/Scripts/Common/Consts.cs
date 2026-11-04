// Input button ids
public class Buttons
{
    public const uint SHOOT = 0;
    public const uint JUMP = 1;
    public const uint DASH = 2;
    public const uint VERIFY_SHOT = 3;
}

// Input axis ids
public class Axes
{
    public const uint X = 0;
    public const uint Z = 1;
    public const uint YAW = 2;
    public const uint PITCH = 3;
}

// Property ids
public class Prop
{
    public const uint AIM = 0;
    public const uint HEALTH = 1;
    public const uint MOVE_STATE = 2;
    public const uint WEAPON = 3;
    public const uint STUNNED = 4;
    public const uint SIZE = 5;
    public const uint USERNAME = 6;
    public const uint SCORE = 7;
    public const uint GAME_OVER = 8;
}

// RPC ids
public class RPC
{
    public const uint SHOOT = 0;
    public const uint EXPLOSION = 1;
    public const uint SWITCH_WEAPON = 2;
    public const uint ROUND_TIME = 3;
    public const uint TRACE = 4;
    public const uint PREDICTED_SPAWN = 5;
    public const uint ADD_TIME = 6;
    public const uint DAMAGE = 7;
    public const uint KILL = 8;
    public const uint KILLED = 9;
    public const uint ADD_AMMO = 10;
}

// Move states for animation
public class MoveStates
{
    public const uint IDLE = 0;
    public const uint FORWARD = 1;
    public const uint BACKWARDS = 2;
    public const uint SPRINT = 3;
}

// Collision groups
public class Collision
{
    public const uint PLAYER = 1;
    public const uint TERRAIN = 1 << 1;
    public const uint DYNAMIC = 1 << 2;
    public const uint PICKUP = 1 << 3;
    // The player's head has no collision but can be hit with physics queries when shooting
    public const uint QUERY_ONLY_SHOOTABLE = 1 << 4;
    public const uint MOVING = 1 << 5;

    public const uint SOLID = PLAYER | TERRAIN | DYNAMIC;
}

// Unity layers
public class Layers
{
    public const int LOCAL_PLAYER = 6;
}

// World consts
public class World
{
    // The size of a tile in the level grid.
    public const float TILE_SIZE = 40f;
}

// Ammo types
public enum AmmoTypes
{
    NONE = -1,
    BULLETS = 0,
    GRENADES = 1
}

// Ammo consts
public class AmmoConsts
{
    // The number of ammo types.
    public const int NUM_TYPES = 2;
}
