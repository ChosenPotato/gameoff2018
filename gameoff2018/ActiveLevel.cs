using OpenTK;
using OpenTK.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace gameoff2018
{
    public enum CharacterFacing { Left, Right };

    public class ActiveLevel
    {
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
        /// Tiles for the level, such as walls or hazards.
        /// </summary>
        public int[,] Tiles;

        /// <summary>
        /// Normalised frame to render (rate of increase may not be 1/sec).
        /// </summary>
        public double SpriteAnimationPosition;

        /// <summary>
        /// Normalised frame to render (rate of increase may not be 1/sec).
        /// </summary>
        public double LavaAnimationLoopValue;

        public ActiveLevel()
        {
            ResetLevel();
        }

        public void ResetLevel()
        {
            LavaBombs = new List<LavaBombEntity>();
            //LavaBombs.Add(new LavaBombEntity(new Vector2d(300, 300), new Vector2d(0, 360), 4));
            Angle = 0.0;
            McPosition = new Vector2d(256, 256);
            McVelocity = new Vector2d(0, 0);
            McGrounded = false;
            McRunning = false;
            LavaHeight = 0;
            facing = CharacterFacing.Left;
            Tiles = new int[Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT];
            Tiles[0, 0] = 1;
            Tiles[1, 0] = 1;
            Tiles[2, 0] = 1;
            Tiles[9, 6] = 1;
            SpriteAnimationPosition = 0;
            LavaAnimationLoopValue = 0;
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
                return Tiles[tileX, tileY] != 0;
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
            int McLeftTile = Convert.ToInt32(Math.Floor((playerX + (Constants.SPRITE_SUIT_SIZE / 2) - Constants.MC_PHYSICS_WIDTH) / Constants.TILE_SIZE));
            int McRightTile = Convert.ToInt32(Math.Floor((playerX + (Constants.SPRITE_SUIT_SIZE / 2) + Constants.MC_PHYSICS_WIDTH) / Constants.TILE_SIZE));
            int McBottomTile = Convert.ToInt32(Math.Floor(playerY / Constants.TILE_SIZE));
            int McTopTile = Convert.ToInt32(Math.Floor((playerY + Constants.SPRITE_SUIT_SIZE) / Constants.TILE_SIZE));
            
            for (int tileX = McLeftTile; tileX <= McRightTile; ++tileX)
                for (int tileY = McBottomTile; tileY <= McTopTile; ++tileY)
                {
                    if (tileX < 0 || tileX >= Constants.LEVEL_WIDTH
                        || tileY < 0 || tileY >= Constants.LEVEL_HEIGHT)
                        return true;
                    if (Tiles[tileX, tileY] != 0)
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
                .Where(x => x.Level > 1 && x.TimeCreated + 1000 <= DateTimeOffset.Now.ToUnixTimeMilliseconds());
            LavaBombs =
                LavaBombs
                .Where(x => x.TimeCreated + 1000 > DateTimeOffset.Now.ToUnixTimeMilliseconds())
                .ToList();
            IEnumerable<Vector2d> vecs = Enumerable.Range(1, 10).Select(x => Math.PI * 2 / 10 * x).Select(x => Util.VectorFromAngle(x) * 100.0);
            IEnumerable<LavaBombEntity> spawned =
                toSpawnFrom.SelectMany(x => vecs.Select(y => new LavaBombEntity(x.Position, x.Velocity + y * x.Level, x.Level - 1)));

            LavaBombs = LavaBombs.Concat(spawned).ToList();
        }
    }
}
