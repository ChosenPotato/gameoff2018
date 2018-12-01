using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameoff2018
{
    public static class Constants
    {
        public const int LEVEL_WIDTH = 48;
        public const int LEVEL_HEIGHT = 96;

        /// <summary>
        /// Including frame tiles.
        /// </summary>
        public const int LEVEL_EXT_WIDTH = LEVEL_WIDTH + 2;

        public const double CHARACTER_MOVE_SPEED = 240;

        public const double BG_TILE_SIZE = 64;
        public const double TILE_SIZE = 32;

        public const double LAVA_RISE_SPEED = 10;

        public const int TEX_ID_LAVA_BOMB = 0;
        public const int TEX_ID_TILE = 1;
        public const int TEX_ID_BG = 2;
        public const int TEX_ID_SPRITE_SUIT = 3;
        public const int TEX_ID_SPRITE_FONT = 4;
        public const int TEX_ID_SPRITE_LAVA_LAKE = 5;
        public const int TEX_ID_SPRITE_LAVA_SURFACE = 6;
        public const int TEX_ID_STANDING = 7;

        public const double TEXT_KERNING = 14;
        public const double TEXT_DEFAULT_HEIGHT = 32;

        public const double MC_PHYSICS_WIDTH = 32;
        public const double SPRITE_SUIT_SIZE = 96;
        public const double SPRITE_SUIT_FPS = 12;
        public const int SPRITE_SUIT_FRAMES = 8;

        public const int SPRITE_FONT_FRAMES = 95;

        public const double LAVA_LAKE_SPRITE_SIZE = 32;
        public const double LAVA_LAKE_SPRITE_FPS = 1.66;
        public const int LAVA_LAKE_SPRITE_FRAMES = 2;

        public const double LAVA_SURFACE_SPRITE_SIZE = 32;
        public const double LAVA_SURFACE_SPRITE_FPS = 1.66;
        public const int LAVA_SURFACE_SPRITE_FRAMES = 2;

        public const double LAVA_BOMB_SIZE = 20;
        public const double GRAVITY = 1200.0;
        public const double JUMP_SPEED = 700.0;

        public const int INITIAL_SCREEN_WIDTH = 1280;
        public const int INITIAL_SCREEN_HEIGHT = 720;
    }
}
