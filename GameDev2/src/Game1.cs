using GameDev2.src;
using GameDev2.src.obj;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;

namespace GameDev2
{
    public class Game1 : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        Vector2 baseScreenSize = new Vector2(960, 600);
        private Matrix globalTransformation;
        int backbufferWidth, backbufferHeight;

        private int tileSize;

        // Global content.
        private SpriteFont hudFont;

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        private int levelIndex = -1;
        private Map map;
        private bool wasContinuePressed;

        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        private KeyboardState keyboardState;

        private const int numberOfLevels = 3;

        public int TileSize
        {
            get { return tileSize; }
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.IsFullScreen = false;

            _graphics.PreferredBackBufferWidth = (int) baseScreenSize.X;
            _graphics.PreferredBackBufferHeight = (int) baseScreenSize.Y;
            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            GlobalProps.ScreenSize = new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        }

        public Game1 getInstance()
        {
            return this;
        }

        // Load all content that should be used
        protected override void LoadContent()
        {
            this.Content.RootDirectory = "Content";

            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load font
            hudFont = Content.Load<SpriteFont>("fonts/batmanForever");

            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("overlays/win");
            loseOverlay = Content.Load<Texture2D>("overlays/lose");
            diedOverlay = Content.Load<Texture2D>("overlays/died");

            //playerTex = new Texture2D(GraphicsDevice, 1, 1);
            
            ScalePresentationArea();

            LoadNextLevel();
        }

        

        public void ScalePresentationArea()
        {
            // Scaling
            backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float horScaling = backbufferWidth / baseScreenSize.X;
            float verScaling = backbufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
            System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
        }


        // Update classes (runs game logic)
        protected override void Update(GameTime gameTime)
        {
            if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
            {
                ScalePresentationArea();
            }

            // Poll input from user
            HandleInput(gameTime);

            // Update the map
            map.Update(gameTime, keyboardState);

            base.Update(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            keyboardState = Keyboard.GetState();

            bool continuePressed = keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.Enter);

            if (!wasContinuePressed && continuePressed)
            {
                if (!map.Player.IsAlive)
                {
                    map.StartNewLife();
                }
                else if (map.TimeRemaining == TimeSpan.Zero)
                {
                    if (map.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }

        private void LoadNextLevel()
        {
            // move to the next level
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unloads the content of the map
            if (map != null)
                map.Dispose();
            

            // Load the level.
            string mapPath = string.Format("Content/levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(mapPath))
                map = new Map(Services, fileStream, levelIndex);
        }

        private void ReloadCurrentLevel()
        {
            levelIndex--;
            LoadNextLevel();
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, globalTransformation);

            map.Draw(gameTime, spriteBatch);

            DrawHud();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);

            Vector2 center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);

            string timeString = "TIME: " + map.TimeRemaining.Minutes.ToString("00") + ":" + map.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (map.TimeRemaining > WarningTime ||
                map.ReachedExit ||
                (int) map.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(hudFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            DrawShadowedString(hudFont, "SCORE: " + map.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (map.TimeRemaining == TimeSpan.Zero)
            {
                if (map.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!map.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }
        }

        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }
    }
}
