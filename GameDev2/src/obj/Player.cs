using GameDev2.src.util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameDev2.src.obj
{
    public class Player
    {
        // Player state vars
        private bool isAlive;
        private bool isOnGround;

        // Jumping vars
        private bool isJump;
        private bool wasJump;
        private float jumpTime;

        // Movement vars
        private Vector2 pos;
        private Vector2 velocity;
        private float movement;
        // Hor
        private float accelaration = 13000.0F;
        private float maxSpeed = 1800.0F;
        private float groundDrag = 0.60F;
        private float airDrag = 0.70F;
        // Vert
        private float jumpTimeMax = 0.4F;
        private float launchVelocity = -6000.0F;
        private float g = 3400.0F;
        private float fallSpeedMax = 550.0F;
        private float airControl = 0.15F;
        

        private Rectangle localBounds;
        private float prevBot;

        private Map map;

        // Vars for animation
        private AnimationPlayer animPlayer;
        private Animation idleAnim;
        private SpriteEffects facingDir;

        // Getters
        public bool IsAlive
        {
            get { return isAlive; }
        }

        public bool IsOnGround
        {
            get { return isOnGround; }
        }

        public Map Map
        {
            get { return map; }
        }

        public Vector2 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Pos.X - animPlayer.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Pos.Y - animPlayer.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }
        
        public Player(Map map, Vector2 pos)
        {
            this.map = map;

            Load();
            Reset(pos);
        }

        public void Load()
        {
            idleAnim = new Animation(map.Content.Load<Texture2D>("sprites/player/idle/elemental_Idle"), 0.1F, true);
            

            double width = idleAnim.FrameWidth / 2.4;
            int height = /*(int) (idleAnim.FrameHeight * 0.9F)*/ (int) (GlobalProps.TileSize * 0.9F);
            int left = (int) (idleAnim.FrameWidth - width) / 2;
            int top = idleAnim.FrameHeight - height;

            localBounds = new Rectangle(left, top, (int) width, height);
            GlobalProps.PlayerWidth = (int) width;
            GlobalProps.PlayerHeight = height;
        }

        public void Reset(Vector2 startPos)
        {
            Vector2 tempPos = new Vector2(startPos.X, startPos.Y);
            Pos = tempPos;
            Velocity = Vector2.Zero;
            isAlive = true;
            animPlayer.PlayAnimation(idleAnim);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            GetInput(keyboardState);

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    animPlayer.PlayAnimation(idleAnim);
                }
                else
                {
                    animPlayer.PlayAnimation(idleAnim);
                }
            }

            movement = 0.0f;
            isJump = false;
        }

        private void GetInput(KeyboardState keyboardState)
        {

            if (keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0F;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0F;
            }

            isJump = keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Space) || keyboardState.IsKeyDown(Keys.LeftShift);
        }

        public void ApplyPhysics(GameTime gameTime)
        {
            double timeElapsed = gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 prevPos = Pos;

            velocity.X += movement * accelaration * (float) timeElapsed;
            velocity.Y = MathHelper.Clamp(Velocity.Y + g * (float) timeElapsed, -fallSpeedMax, fallSpeedMax);
            velocity.Y = Jump(Velocity.Y, gameTime);

            if (isOnGround)
            {
                velocity.X *= groundDrag;
            }
            else
            {
                velocity.X *= airDrag;
            }

            velocity.X = MathHelper.Clamp(Velocity.X, -maxSpeed, maxSpeed);

            Pos += velocity * (float) timeElapsed;
            Pos = new Vector2((float) Math.Round(Pos.X), (float) Math.Round(Pos.Y));

            HandleCollision();

            // If colliding, reset velocity
            if (Pos.X == prevPos.X)
            {
                velocity.X = 0;
            }

            if (Pos.Y == prevPos.Y)
            {
                velocity.Y = 0;
            }

        }

        private float Jump(float velY, GameTime gameTime)
        {
            if (isJump)
            {
                if (jumpTime > 0.0F || (!wasJump && isOnGround))
                {
                    jumpTime += (float) gameTime.ElapsedGameTime.TotalSeconds;
                    animPlayer.PlayAnimation(idleAnim);
                }

                // Going up
                if (jumpTime > 0.0F && jumpTime <= jumpTimeMax)
                {
                    velY = (float) (launchVelocity * (1.0F - Math.Pow(jumpTime / jumpTimeMax, airControl)));
                }
                // Reached top
                else
                {
                    jumpTime = 0.0F;
                }
            }
            else
            {
                jumpTime = 0.0F;
            }
            wasJump = isJump;

            return velY;
        }

        private void HandleCollision()
        {
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / GlobalProps.TileSize);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / GlobalProps.TileSize));
            int topTile = (int)Math.Floor((float)bounds.Top / GlobalProps.TileSize);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / GlobalProps.TileSize));

            

            isOnGround = false;

            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = map.getCollision(x, y);

                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = map.getBounds(x, y);
                        Vector2 depth = RectangleExtras.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (prevBot <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Pos = new Vector2(Pos.X, Pos.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Pos = new Vector2(Pos.X + depth.X, Pos.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            prevBot = bounds.Bottom;
        }

        public void Killed()
        {
            isAlive = false;
        }

        public void ReachedExit()
        {
            // Do exit anim if I do one
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (velocity.X < 0)
            {
                facingDir = SpriteEffects.FlipHorizontally;
            }
            else if (velocity.X > 0)
            {
                facingDir = SpriteEffects.None;
            }

            Vector2 adjustedPos = new Vector2(BoundingRectangle.X, BoundingRectangle.Y);
            
            animPlayer.Draw(gameTime, spriteBatch, adjustedPos , facingDir);


            Texture2D rect = new Texture2D(GlobalProps.GetGameInstance.GraphicsDevice, GlobalProps.TileSize, GlobalProps.TileSize);
            Color[] data = new Color[GlobalProps.TileSize * GlobalProps.TileSize];
            for (int i = 0; i < data.Length; ++i) data[i] = new Color(255, 0, 0, 10);
            rect.SetData(data);

            //spriteBatch.Draw(rect, BoundingRectangle, Color.White);
        }
    }
}
