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
    public enum CollisionOutcome { None, Collision, Victory }
    public enum LevelResetCause { Start, Death, Victory }

    public class ActiveLevel
    {
        public int LevelNumber;

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
            ResetLevel(LevelResetCause.Start);
        }

        public void ResetLevel(LevelResetCause resetCause)
        {
            if (resetCause == LevelResetCause.Start)
                LevelNumber = 1;
            else if (resetCause == LevelResetCause.Victory)
                ++LevelNumber;

            LoadTilesFromFile();
            Vector2d startPositionToSet = new Vector2d(256, 256);
            for (int tileX = 0; tileX < Constants.LEVEL_WIDTH; ++tileX)
                for (int tileY = 0; tileY < Constants.LEVEL_HEIGHT; ++tileY)
                    if (Tiles[tileX, tileY] == Constants.TILE_ID_FLAG_WHITE)
                    {
                        startPositionToSet = new Vector2d(
                            tileX * Constants.TILE_SIZE + Constants.TILE_SIZE * 0.5 - Constants.SPRITE_SUIT_SIZE / 2,
                            (tileY + 0.05) * Constants.TILE_SIZE);
                        goto Found;
                    }
            
            Found:
            {
                LavaBombs = new List<LavaBombEntity>();
                Angle = 0.0;
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

            File.WriteAllBytes($"level_{LevelNumber}", bytesToSave);
        }

        public void LoadTilesFromFile()
        {
            byte[] bytesLoaded;
            try
            {
                bytesLoaded = File.ReadAllBytes($"level_{LevelNumber}");

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
            catch
            {
                Tiles = new int[Constants.LEVEL_WIDTH, Constants.LEVEL_HEIGHT];
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

        public BoundingBox GetPlayerBoundingBox(double playerX, double playerY)
        {
            double Left = (playerX + (Constants.SPRITE_SUIT_SIZE / 2) - Constants.CHAR_PHYSICS_WIDTH) / Constants.TILE_SIZE;
            double Right = (playerX + (Constants.SPRITE_SUIT_SIZE / 2) + Constants.CHAR_PHYSICS_WIDTH) / Constants.TILE_SIZE;
            double Bottom = playerY / Constants.TILE_SIZE;
            double Top = (playerY + Constants.SPRITE_SUIT_SIZE) / Constants.TILE_SIZE;

            return new BoundingBox(Left, Right, Bottom, Top);
        }

        /// <summary>
        /// Check each tile in the rectangle of tiles the player can intersect with. If any are not empty, returns true.
        /// </summary>
        /// <param name="playerX"></param>
        /// <param name="playerY"></param>
        /// <returns></returns>
        public CollisionOutcome IsIntersectionWithLevel(double playerX, double playerY)
        {
            BoundingBox boundingBox = GetPlayerBoundingBox(playerX, playerY);

            int McLeftTile = Convert.ToInt32(Math.Floor(boundingBox.Left));
            int McRightTile = Convert.ToInt32(Math.Floor(boundingBox.Right));
            int McBottomTile = Convert.ToInt32(Math.Floor(boundingBox.Bottom));
            int McTopTile = Convert.ToInt32(Math.Floor(boundingBox.Top));

            bool collision = false;

            for (int tileX = McLeftTile; tileX <= McRightTile; ++tileX)
                for (int tileY = McBottomTile; tileY <= McTopTile; ++tileY)
                {
                    if (tileX < 0 || tileX >= Constants.LEVEL_WIDTH
                        || tileY < 0 || tileY >= Constants.LEVEL_HEIGHT)
                        collision = true;
                    else if (Tiles[tileX, tileY] == Constants.TILE_ID_ROCK)
                        collision = true;
                    else  if (Tiles[tileX, tileY] == Constants.TILE_ID_FLAG_RED)
                        return CollisionOutcome.Victory;
                }

            if (collision)
                return CollisionOutcome.Collision;
            else
                return CollisionOutcome.None;
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

            CollisionOutcome outcome = IsIntersectionWithLevel(newPosition.X, newPosition.Y);
            switch (outcome)
            {
                case CollisionOutcome.None:
                    McPosition = newPosition;
                    break;
                case CollisionOutcome.Victory:
                    ResetLevel(LevelResetCause.Victory);
                    return;
                default:
                    break;
            }
            
            newPosition = McPosition;

            // collisions - process Y
            McVelocity.Y -= Constants.GRAVITY * elapsedTime;
            newPosition.Y += McVelocity.Y * elapsedTime;

            outcome = IsIntersectionWithLevel(newPosition.X, newPosition.Y);
            switch (outcome)
            {
                case CollisionOutcome.None:
                    McPosition = newPosition;
                    McGrounded = false;
                    break;
                case CollisionOutcome.Victory:
                    ResetLevel(LevelResetCause.Victory);
                    return;
                default:
                    McVelocity = Vector2d.Zero;
                    McGrounded = true;
                    break;
            }

            if (IsLavaCollision(McPosition.Y))
            {
                ResetLevel(LevelResetCause.Death);
                return;
            }
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
                if (b.Level == 2)
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
            IEnumerable<Vector2d> vecs = Enumerable.Range(1, 10).Select(x => Math.PI * 2 / 10 * x).Select(x => Util.VectorFromAngle(x) * 150.0);
            IEnumerable<LavaBombEntity> spawned =
                toSpawnFrom.SelectMany(x => vecs.Select(y => new LavaBombEntity(x.Position, x.Velocity + y * x.Level, x.Level - 1)));

            LavaBombs = LavaBombs.Concat(spawned).ToList();
        }
    }
}
