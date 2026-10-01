using System;
using System.Collections.Generic;

/**
 * Stores all the unique ID values that must be consistent on the server and client under the categories:
 * 
 * Properties   - properties synched to the clients from the server
 * RPCs         - Calls made to/from the server
 * Controls     - Axis & Buttons for the input manager
 * Controllers  - Player Controller IDs, must be non-zero
 * Type         - Paths to various entities used for identification and spawning
 */
public class ID
{
    public struct PROP
    {
        public struct FIGHTER
        {
            public const uint FORWARD_SPEED     = 1010;
            public const uint STRAFE_SPEED      = 1011;
            public const uint HEALTH            = 1020;
            public const uint MAX_HEALTH        = 1021;
            public const uint ENERGY            = 1025;
            public const uint ENERGY_FREEZE     = 1026;
            public const uint MAX_ENERGY        = 1027;
            public const uint USING_SHIELD      = 1030;
            public const uint LASER_RELOAD      = 1040;
            public const uint MISSILE_RELOAD    = 1041;
            public const uint BOOST_RELOAD      = 1042;
            public const uint SHIELD_RELOAD     = 1043;
            public const uint LOCK_TARGET       = 1045;
            public const uint IS_TARGETED       = 1050;
            public const uint OUT_OF_BOUNDS     = 1060;
            public const uint MISSILE_COUNT     = 1070;
            public const uint MISSILE_CAPACITY  = 1071;
            public const uint SHIELD_COUNT      = 1072;
            public const uint SHIELD_CAPACITY   = 1073;
            public const uint LASER_SPEED       = 1080;
        }

        public struct TURRET
        {
            public const uint OWNER             = 2000;
            public const uint DIRECTION         = 2001;
            public const uint ENERGY            = 2002;
            public const uint ENERGY_FREEZE     = 2003;
            public const uint MAX_ENERGY        = 2004;
            public const uint LASER_RELOAD      = 2006;
        }

        public struct LASER
        {
            public const uint OWNER             = 3000;
        }

        public struct MISSILE
        {
            public const uint OWNER             = 4000;
        }

        public struct PLAYER
        {
            public const uint NAME               = 5000;
            public const uint SCORE              = 5001;
            public const uint TEAM_NUMBER        = 5002;
            public const uint TYPE               = 5003;
            public const uint RESPAWN_TIME_LEFT  = 5009;
            public const uint SPAWNED            = 5010;
            public const uint GLOW_COLOR         = 5011;
            public const uint KILLS              = 5020;
            public const uint DEATHS             = 5021;
        }

        public struct GAME_MANAGER
        {
            public const uint WIN_SCORE         = 6000;
            public const uint NUMBER_OF_TEAMS   = 6001;
        }

        public struct TEAM
        {
            // Add team number to property id to get the property for a given team.
            public const uint SCORE             = 7000;
            public const uint NAME              = 7100;
            public const uint COLOR             = 7200;
        }
    }

    public struct RPC
    {
        public const uint CHAT_TO_CLIENT            = 5;
        public const uint CHAT_TO_SERVER            = 6;
        public const uint PLAYER_SETTINGS           = 10;
        public const uint SPAWN                     = 11;
        public const uint GAME_TIMER                = 20;
        public const uint GAME_END_STATUS           = 50;
        public const uint FIGHTER_HIT               = 200;
        public const uint PLINK                     = 201;
        public const uint TURRET_LASER              = 500;
        public const uint TURRET_ELEVATION          = 502;
        public const uint TURRET_ROTATION           = 503;
        public const uint FIGHTER_TARGET            = 300;
    }

    public struct CONTROLS
    {
        public const uint TURN_X                    = 0;
        public const uint TURN_Y                    = 1;
        public const uint ROLL                      = 2;
        public const uint BOOST                     = 4;
        public const uint LASER                     = 8;
        public const uint MISSILE                   = 9;
        public const uint SHIELD                    = 14;
        public const uint ACCELERATE                = 30;
        public const uint STRAFE                    = 31;
    }

    public struct CONTROLLERS
    {
        public const uint FIGHTER                   = 1;
    }

    public struct TYPE
    {
        public const string FIGHTER                 = "PlayerShips/fighter";
        public const string MISSILE                 = "Projectiles/missile";
        public const string LASER                   = "Projectiles/laser";
        public const string MISSILE_CRATE           = "Collectables/MissileCrate";
        public const string SHIELD_CRATE            = "Collectables/ShieldCrate";
        public const string HEALTH_CRATE            = "Collectables/HealthCrate";
    }
}

/*
 * An enum describing the player's role.
 */
public enum PlayerType : byte
{
    NONE = 0,
    FIGHTER = 1,
    TURRET = 2,
}

/*
 * Stores collision layers for the game
 */
public class COLLISION
{
    public const uint FIGHTER = 1;
    public const uint BULLET = 1 << 1;
    public const uint COLLECTIBLE = 1 << 2;
    public const uint ENVIRONMENT = 1 << 3;
    public const uint TRIGGER = 1 << 4;
}