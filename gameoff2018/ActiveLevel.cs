using OpenTK;
using OpenTK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace gameoff2018
{
    public enum CharacterFacing { Left, Right };

    public class ActiveLevel
    {
        /// <summary>
        /// Tiles for the level, such as walls or hazards.
        /// </summary>
        public int[,] Tiles;

        public List<LavaBombEntity> LavaBombs;
        public double Angle;

        public Vector2d McPosition;
        public Vector2d McVelocity;

        public bool McGrounded;
        public bool McRunning;

        /// <summary>
        /// Lava reaches this height.
        /// </summary>
        public double LavaHeight;
        public CharacterFacing facing;

        /// <summary>
        /// Normalised frame to render (rate of increase may not be 1/sec).
        /// </summary>
        public double SpriteAnimationPosition;

        /// <summary>
        /// Normalised frame to render (rate of increase may not be 1/sec).
        /// </summary>
        public double LavaAnimationLoopValue;

        /// <summary>
        /// 
        /// </summary>
        public double SpitterLoopValue;

        public ActiveLevel()
        {
            ResetLevel();
        }

        public void ResetLevel()
        {
            LoadTilesFromFile();

            LavaBombs = new List<LavaBombEntity>();
            //LavaBombs.Add(new LavaBombEntity(new Vector2d(300, 300), new Vector2d(0, 360), 4));
            Angle = 0.0;
            Vector2d startPositionToSet = new Vector2d(256, 256);
            for (int tileX = 0; tileX < Constants.LEVEL_WIDTH; ++tileX)
                for (int tileY = 0; tileY < Constants.LEVEL_HEIGHT; ++tileY)
                    if (Tiles[tileX, tileY] == Constants.TILE_ID_FLAG_WHITE)
                    {
                        startPositionToSet = new Vector2d(tileX * Constants.TILE_SIZE + Constants.TILE_SIZE * 0.5 - Constants.SPRITE_SUIT_SIZE / 2, (tileY + 1) * Constants.TILE_SIZE);
                        goto Found;
                    }
            
            Found:
            {
                McPosition = startPositionToSet;
                McVelocity = new Vector2d(0, 0);
                McGrounded = false;
                McRunning = false;
                LavaHeight = 0;
                facing = CharacterFacing.Left;
                //Tiles = new int[Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT];
                //Tiles[0, 0] = 1;
                //Tiles[1, 0] = 1;
                //Tiles[2, 0] = 1;
                //Tiles[9, 6] = 1;
                SpriteAnimationPosition = 0;
                LavaAnimationLoopValue = 0;
            }
        }

        // provided in world coords
        public Vector2d GetWorldToScreenOffset()
        {
            return
                new Vector2d(Constants.TILE_SIZE, Constants.TILE_SIZE)
                + (
                        McPosition.Y > Constants.TILE_SIZE * 12
                            ? new Vector2d(0.0, Constants.TILE_SIZE * 12 - McPosition.Y)
                            : Vector2d.Zero
                    );
        }

        public double WorldToScreenScaleFactor(int screenWidth)
        {
            return screenWidth / (Constants.TILE_SIZE * Constants.LEVEL_EXT_WIDTH);
        }

        public Point ConvertWorldToScreenCoords(Vector2d worldCoords, int screenWidth)
        {
            Vector2d toScale = worldCoords + GetWorldToScreenOffset();
            Vector2d doubleValues = toScale * WorldToScreenScaleFactor(screenWidth);
            return new Point((int)doubleValues.X, (int)doubleValues.Y);
        }

        public Vector2d ConvertScreenToWorldCoords(Point screenCoords, int screenWidth)
        {
            Vector2d asVector = new Vector2d(screenCoords.X, screenCoords.Y);
            Vector2d scaled = asVector / WorldToScreenScaleFactor(screenWidth);
            return scaled - GetWorldToScreenOffset();
        }

        public void SaveTilesToFile()
        {
            byte[] bytesToSave;

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, Tiles);
                bytesToSave = memoryStream.ToArray();
            }

            File.WriteAllBytes("level_1", bytesToSave);
        }

        public void LoadTilesFromFile()
        {
            byte[] bytesLoaded = File.ReadAllBytes("level_1");

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(bytesLoaded))
            {
                int[,] tilesToSet = (int[,])binaryFormatter.Deserialize(memoryStream);
                Tiles = new int[Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT];
                for (int x = 0; x < tilesToSet.GetLength(0); x++)
                    for (int y = 0; y < tilesToSet.GetLength(1); y++)
                    {
                        Tiles[x, y] = tilesToSet[x, y];
                    }
            }
        }

        /// <summary>
        /// Used for collisions.
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <returns></returns>
        public bool IsSolidAt (int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= Constants.LEVEL_WIDTH
                || tileY < 0 || tileY >= Constants.LEVEL_HEIGHT)
                return false;
            else
                return Tiles[tileX, tileY] != Constants.TILE_ID_EMPTY;
        }

        public bool IsLavaCollision(double playerY)
        {
            return playerY < LavaHeight;
        }

        /// <summary>
        /// Check each tile in the rectangle of tiles the player can intersect with. If any are not empty, returns true.
        /// </summary>
        /// <param name="playerX"></param>
        /// <param name="playerY"></param>
        /// <returns></returns>
        public bool IsIntersectionWithLevel(double playerX, double playerY)
        {
            int McLeftTile = Convert.ToInt32(Math.Floor((playerX + (Constants.SPRITE_SUIT_SIZE / 2) - Constants.CHAR_PHYSICS_WIDTH) / Constants.TILE_SIZE));
            int McRightTile = Convert.ToInt32(Math.Floor((playerX + (Constants.SPRITE_SUIT_SIZE / 2) + Constants.CHAR_PHYSICS_WIDTH) / Constants.TILE_SIZE));
            int McBottomTile = Convert.ToInt32(Math.Floor(playerY / Constants.TILE_SIZE));
            int McTopTile = Convert.ToInt32(Math.Floor((playerY + Constants.SPRITE_SUIT_SIZE) / Constants.TILE_SIZE));
            
            for (int tileX = McLeftTile; tileX <= McRightTile; ++tileX)
                for (int tileY = McBottomTile; tileY <= McTopTile; ++tileY)
                {
                    if (tileX < 0 || tileX >= Constants.LEVEL_WIDTH
                        || tileY < 0 || tileY >= Constants.LEVEL_HEIGHT)
                        return true;
                    if (Tiles[tileX, tileY] == Constants.TILE_ID_ROCK)
                        return true;
                }

            return false;
        }

        /// <summary>
        /// Process collisions - X first then Y.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public void ProcessPlayerMovement(KeyboardState prevKeyState, KeyboardState keyState, double elapsedTime)
        {
            if (keyState.IsKeyDown(Key.F1) && !prevKeyState.IsKeyDown(Key.F1))
                SaveTilesToFile();
            if (keyState.IsKeyDown(Key.F2) && !prevKeyState.IsKeyDown(Key.F2))
                LoadTilesFromFile();

            Vector2d newPosition = McPosition;

            if (keyState.IsKeyDown(Key.Left) && !keyState.IsKeyDown(Key.Right))
            {
                McRunning = true;
                newPosition.X -= Constants.CHARACTER_MOVE_SPEED * elapsedTime;
                facing = CharacterFacing.Left;
            }
            else if (keyState.IsKeyDown(Key.Right) && !keyState.IsKeyDown(Key.Left))
            {
                McRunning = true;
                newPosition.X += Constants.CHARACTER_MOVE_SPEED * elapsedTime;
                facing = CharacterFacing.Right;
            }
            else
            {
                McRunning = false;
            }
            if (keyState.IsKeyDown(Key.Space) && !prevKeyState.IsKeyDown(Key.Space))
            {
                if (McGrounded)
                {
                    McGrounded = false;
                    McVelocity.Y += Constants.JUMP_SPEED;
                }
            }

            if (!IsIntersectionWithLevel(newPosition.X, newPosition.Y))
                McPosition = newPosition;
            newPosition = McPosition;

            // collisions - process Y
            McVelocity.Y -= Constants.GRAVITY * elapsedTime;
            newPosition.Y += McVelocity.Y * elapsedTime;

            if (!IsIntersectionWithLevel(newPosition.X, newPosition.Y))
            {
                McPosition = newPosition;
                McGrounded = false;
            }
            else
            {
                McVelocity = Vector2d.Zero;
                McGrounded = true;
            }

            if (IsLavaCollision(McPosition.Y))
                ResetLevel();
        }

        public void Update(KeyboardState prevKeyState, KeyboardState keyState, double elapsedTime)
        {
            Angle += elapsedTime * Math.PI;

            LavaHeight += elapsedTime * Constants.LAVA_RISE_SPEED;

            SpitterLoopValue += elapsedTime * Constants.SPITTER_LOOP_SPEED;
            if (SpitterLoopValue > 1.0)
            {
                SpitterLoopValue -= 1.0;

                for (int tileX = 0; tileX < Constants.LEVEL_WIDTH; ++tileX)
                    for (int tileY = 0; tileY < Constants.LEVEL_HEIGHT; ++tileY)
                        if (Tiles[tileX,tileY] == Constants.TILE_ID_SPITTER)
                        {
                            LavaBombs.Add(new LavaBombEntity(
                                new Vector2d(tileX * Constants.TILE_SIZE + Constants.TILE_SIZE * 0.5, (tileY + 1) * Constants.TILE_SIZE),
                                new Vector2d(0, 750),
                                2));
                        }
            }

            SpriteAnimationPosition += elapsedTime * Constants.SPRITE_SUIT_FPS / Constants.SPRITE_SUIT_FRAMES;
            if (SpriteAnimationPosition > 1.0)
                SpriteAnimationPosition -= 1.0;

            LavaAnimationLoopValue += elapsedTime * Constants.LAVA_LAKE_SPRITE_FPS / Constants.LAVA_LAKE_SPRITE_FRAMES;
            if (LavaAnimationLoopValue > 1.0)
                LavaAnimationLoopValue -= 1.0;

            foreach (LavaBombEntity b in LavaBombs)
            {
                b.Velocity.Y -= Constants.GRAVITY * elapsedTime;
                b.Position += b.Velocity * elapsedTime;
            }

            ProcessPlayerMovement(prevKeyState, keyState, elapsedTime);

            IEnumerable<LavaBombEntity> toSpawnFrom =
                LavaBombs
                .Where(x => x.Level > 1 && x.TimeCreated + Constants.LAVA_BOMB_TIMER_MS <= DateTimeOffset.Now.ToUnixTimeMilliseconds());
            LavaBombs =
                LavaBombs
                .Where(x =>
                    x.TimeCreated + (x.Level == 2 ? Constants.LAVA_BOMB_TIMER_MS : Constants.LAVA_BULLET_TIMER_MS)
                    > DateTimeOffset.Now.ToUnixTimeMilliseconds())
                .ToList();
            IEnumerable<Vector2d> vecs = Enumerable.Range(1, 10).Select(x => Math.PI * 2 / 10 * x).Select(x => Util.VectorFromAngle(x) * 100.0);
            IEnumerable<LavaBombEntity> spawned =
                toSpawnFrom.SelectMany(x => vecs.Select(y => new LavaBombEntity(x.Position, x.Velocity + y * x.Level, x.Level - 1)));

            LavaBombs = LavaBombs.Concat(spawned).ToList();
        }
    }
}
