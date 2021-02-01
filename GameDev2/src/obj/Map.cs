using GameDev2.src.util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GameDev2.src.obj
{
    public class Map : IDisposable
    {
        private Tile[,] tileMap;
        private Texture2D backgroundTex;
        private ContentManager content;

        private GlobalProps globalProps;

        // TODO: Add enemies and coins later;
        private List<Coin> coins = new List<Coin>(); 
        //private List<Enemy> enemies; 

        private const int backgroundLayer = 0;
        private const int mainLayer = 1;
        private const int coinLayer = 2;

        private static Point illegalPos = new Point(-2, -2);
        private Vector2 startPos;
        private Point endPos = illegalPos;

        private Player player;

        private int score;
        private const int pointsPerSec = 10;
        public TimeSpan timeRemaining;
        private bool exitReached;

        // Dimensions of the map measured in tiles.
        private int width;
        private int height;

        private int mapWidth;

        // Getters
        public Player Player
        {
            get {return player; }
        }

        public bool ReachedExit
        {
            get { return exitReached; }
        }

        public int Score
        {
            get { return score; }
        }
        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }

        public ContentManager Content
        {
            get { return content; }
        }

        public int Width
        {
            get { return tileMap.GetLength(0); }
        }

        public int Height
        {
            get { return tileMap.GetLength(1); }
        }

        // Load the map
        public Map(IServiceProvider serviceProvider, Stream mapStream, int lvlIndex)
        {
            content = new ContentManager(serviceProvider, "Content");
            globalProps = new GlobalProps();
            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(mapStream);

            backgroundTex = content.Load<Texture2D>("backgrounds/background_0");

            width = GlobalProps.ScreenSize.X / GlobalProps.TileSize;
            height = GlobalProps.ScreenSize.Y / GlobalProps.TileSize;
        }
        
        private void LoadTiles(Stream mapStream)
        {
            mapWidth = 0;
            List<string> mapRows = new List<string>();

            ValidateMap(mapStream, mapRows);

            tileMap = new Tile[mapWidth, mapRows.Count];

            // Loop through all tiles that need to be assigned and Load the correct tile.
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    char tileType = mapRows[y][x];
                    tileMap[x, y] = LoadTile(tileType, x, y);
                }
            }

            ValidateStart();
        }

        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                case '.':
                    return new Tile(null, TileCollision.Passable);
                case '#':
                    return new Tile(content.Load<Texture2D>("tiles/block0"), TileCollision.Impassable);
                case '1':
                    return LoadStartTile(x, y);
                case 'C':
                    return LoadCoinTile(x, y);
                case 'X':
                    return LoadExitTile(x, y);
                default:
                    return new Tile(content.Load<Texture2D>("tiles/block1"), TileCollision.Passable);
            }
        }

        private Tile LoadStartTile(int x, int y)
        {
            if (player != null)
            {
                throw new Exception("There can only be one start position. Check if you level file makes sense.");
            }

            Rectangle bounds = getBounds(x, y);
            startPos = RectangleExtras.GetBottomCenter(bounds);
            player = new Player(this, startPos);

            return new Tile(content.Load<Texture2D>("tiles/block1"), TileCollision.Passable);
        }

        private Tile LoadExitTile(int x, int y)
        {
            if (endPos != illegalPos)
                throw new NotSupportedException("A level may only have one exit.");

            endPos = getBounds(x, y).Center;

            return new Tile(content.Load<Texture2D>("tiles/exit"), TileCollision.Passable);
        }

        private Tile LoadCoinTile(int x, int y)
        {
            Point pos = getBounds(x, y).Center;
            coins.Add(new Coin(this, new Vector2(pos.X, pos.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        // Define Bounds and collisions for the map
        public Rectangle getBounds(int x, int y)
        {
            return new Rectangle(x * GlobalProps.TileSize, y * GlobalProps.TileSize, GlobalProps.TileSize, GlobalProps.TileSize);
        }

        public TileCollision getCollision(int x, int y)
        {
            if (x < 0 || x > (GlobalProps.ScreenSize.X / GlobalProps.TileSize) - 1)
            {
                return TileCollision.Impassable;
            }
            if (y < 0 || y >= (GlobalProps.ScreenSize.Y / GlobalProps.TileSize))
            {
                return TileCollision.Passable;
            }

            return tileMap[x, y].collision;
        }

        
        
        // Updating the map
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            // Time stops when player isn't controllable.
            if (!player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                player.ApplyPhysics(gameTime);
            }
            else if (exitReached)
            {
                // points
                timeRemaining = TimeSpan.Zero;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                player.Update(gameTime, keyboardState);
                UpdateCoins(gameTime);


                if (player.BoundingRectangle.Top >= Height * GlobalProps.TileSize)
                {
                    PlayerKilled();
                }

                // TODO: update enemies

                if (player.IsAlive && player.IsOnGround && player.BoundingRectangle.Contains(endPos))
                {
                    HasReachedExit();
                }
            }
            if (timeRemaining < TimeSpan.Zero)
            {
                timeRemaining = TimeSpan.Zero;
            }
        }

        public void PlayerKilled()
        {
            player.Killed();
        }

        private void UpdateCoins(GameTime gameTime)
        {
            for (int i = 0; i < coins.Count; i++)
            {
                Coin coin = coins[i];

                if (coin.LocalBounds.Intersects(player.BoundingRectangle))
                {
                    coins.RemoveAt(i--);
                    OnGemCollected(coin);
                }
            }
        }

        private void OnGemCollected(Coin coin)
        {
            score += coin.Value;
            Debug.WriteLine(score);
        }

        public void HasReachedExit()
        {
            Debug.WriteLine("Exit Reached");
            player.ReachedExit();
            exitReached = true;
        }

        public void StartNewLife()
        {
            player.Reset(startPos);
        }

        public void Dispose()
        {
            Content.Unload();
        }
        
        // Draw
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTex, new Rectangle(0, 0, GlobalProps.ScreenSize.X, GlobalProps.ScreenSize.Y), Color.White);

            DrawTiles(spriteBatch);

            foreach (Coin coin in coins)
            {
                coin.Draw(gameTime, spriteBatch);
            }

            player.Draw(gameTime, spriteBatch);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tileMap[x, y].texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * GlobalProps.TileSize;
                        spriteBatch.Draw(texture, new Rectangle((int) position.X, (int) position.Y, GlobalProps.TileSize, GlobalProps.TileSize), Color.White);
                    }
                }
            }
        }


        // Validate Tiles
        private void ValidateMap(Stream mapStream, List<string> mapRows)
        {
            using (StreamReader reader = new StreamReader(mapStream))
            {
                string line = reader.ReadLine();
                mapWidth = line.Length;

                while (line != null)
                {
                    mapRows.Add(line);
                    if (line.Length != mapWidth)
                    {
                        throw new Exception(String.Format("The lenght of the line {0} is different from the rest. This is not allowed. Check for map size != rectangle", line));
                    }
                    line = reader.ReadLine();
                }
            }
        }

        private void ValidateStart()
        {
            if (Player == null)
            {
                throw new Exception("No PlayerTile has been assigned. Check if map has start tile");
            }
        }
    }
}
