using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameoff2018
{
    public static class Constants
    {
        public const int LEVEL_WIDTH = 40;
        public const int LEVEL_HEIGHT = 24;

        public const double CHARACTER_MOVE_SPEED = 240;

        public const double TILE_SIZE = 32;

        public const int TEX_ID_LAVA_BOMB = 0;
        public const int TEX_ID_TILE = 1;
        public const int TEX_ID_BG = 2;
        public const int TEX_ID_SPRITE_SUIT = 3;
        public const int TEX_ID_SPRITE_FONT = 4;

        public const double TEXT_KERNING = 14;
        public const double TEXT_DEFAULT_HEIGHT = 32;

        public const double SPRITE_SIZE = 96;
        public const double SPRITE_SUIT_FRAMES = 8;
        public const double SPRITE_FONT_FRAMES = 95;
        public const double SPRITE_FPS = 12;

        public const double LAVA_LAKE_SPRITE_SIZE = 64;
        public const double LAVA_LAKE_SPRITE_FPS = 1.66;
        public const double LAVA_LAKE_SPRITE_FRAMES = 2;

        public const double LAVA_SURFACE_SPRITE_SIZE = 64;
        public const double LAVA_SURFACE_SPRITE_FPS = 1.66;
        public const double LAVA_SURFACE_SPRITE_FRAMES = 2;

        public const double LAVA_BOMB_SIZE = 20;
        public const double GRAVITY = 360.0;
    }
}
