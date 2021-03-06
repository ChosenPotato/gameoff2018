﻿namespace gameoff2018
{
    public static class Constants
    {
        public const int INITIAL_SCREEN_WIDTH = 1280;
        public const int INITIAL_SCREEN_HEIGHT = 720;

        public const int LEVEL_WIDTH = 48;
        public const int LEVEL_HEIGHT = 96;

        public const int FPS = 60;

        /// <summary>
        /// Including border tiles.
        /// </summary>
        public const int LEVEL_EXT_WIDTH = LEVEL_WIDTH + 2;

        public const double CHARACTER_MOVE_SPEED = 240;

        public const double BG_TILE_SIZE = 128;
        public const double TILE_SIZE = 32;

        public const double CHAR_PHYSICS_WIDTH = 24;

        public const double LAVA_BOMB_SIZE = 10;
        public const double LAVA_BULLET_SIZE = 6;
        public const double GRAVITY = 1200.0;
        public const double JUMP_SPEED = 700.0;

        public const double SPITTER_LOOP_SPEED = 0.5;
        public const double FLAME_SPITTER_LOOP_SPEED = 0.15;
        public const double FLAMES_LOOP_SPEED = 8.0;

        public const double LAVA_BOMB_TIMER_MS = 500;
        public const double LAVA_BULLET_TIMER_MS = 1200;

        // Sprite constants.
        public const double SPRITE_SUIT_SIZE = 96;
        public const double SPRITE_SUIT_FPS = 12;
        public const int SPRITE_SUIT_FRAMES = 8;

        public const int SPRITE_FONT_FRAMES = 95;
        public const double TEXT_KERNING = 14;
        public const double TEXT_DEFAULT_HEIGHT = 32;

        public const int SPRITE_FLAMES_SIZE = 64;
        public const int SPRITE_FLAMES_FRAMES = 2;

        public const double LAVA_LAKE_SPRITE_SIZE = 32;
        public const double LAVA_LAKE_SPRITE_FPS = 1.66;
        public const int LAVA_LAKE_SPRITE_FRAMES = 2;

        public const double LAVA_SURFACE_SPRITE_SIZE = 32;
        public const double LAVA_SURFACE_SPRITE_FPS = 1.66;
        public const int LAVA_SURFACE_SPRITE_FRAMES = 2;

        public const int TILE_ID_EMPTY = 0;
        public const int TILE_ID_ROCK = 1;
        public const int TILE_ID_SPITTER = 2;
        public const int TILE_ID_FLAG_RED = 3;
        public const int TILE_ID_FLAG_WHITE = 4;
        public const int TILE_ID_FLAME_SPITTER = 5;

        // Texture constants.
        public const int TEX_ID_LAVA_BOMB = 0;
        public const int TEX_ID_ROCK = 1;
        public const int TEX_ID_BG = 2;
        public const int TEX_ID_SPRITE_SUIT = 3;
        public const int TEX_ID_SPRITE_FONT = 4;
        public const int TEX_ID_SPRITE_LAVA_LAKE = 5;
        public const int TEX_ID_SPRITE_LAVA_SURFACE = 6;
        public const int TEX_ID_STANDING = 7;
        public const int TEX_ID_SPITTER = 8;
        public const int TEX_ID_BULLET = 9;
        public const int TEX_ID_FLAG_RED = 10;
        public const int TEX_ID_FLAG_WHITE = 11;
        public const int TEX_ID_FLAME_SPITTER = 12;
        public const int TEX_ID_SPRITE_FLAMES_BIG = 13;
    }
}
