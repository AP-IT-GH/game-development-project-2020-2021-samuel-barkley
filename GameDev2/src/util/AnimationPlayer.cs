using System;
using GameDev2.src;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDev2
{
    // Handles playing of animation (timing of frames)
    struct AnimationPlayer
    {
        public Animation Animation
        {
            get { return animation; }
        }
        Animation animation;

        // Which frame is anim on
        public int FrameIndex
        {
            get { return frameIndex; }
        }
        int frameIndex;

        // How long has the frame been shown
        private float time;

        public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }

        public void PlayAnimation(Animation animation)
        {
            // If this animation is already running, don't restart it.
            if (Animation == animation)
                return;

            // (re)start the new animation.
            this.animation = animation;
            this.frameIndex = 0;
            this.time = 0.0f;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            if (Animation == null)
                throw new NotSupportedException("No animation is currently playing.");

            // Process passing time.
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (time > Animation.FrameTime)
            {
                time -= Animation.FrameTime;

                // Advance the frame index; looping or clamping as appropriate.
                if (Animation.IsLooping)
                {
                    frameIndex = (frameIndex + 1) % Animation.FrameCount;
                }
                else
                {
                    frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);
                }
            }

            // Calculate the source rectangle of the current frame.
            Rectangle source = new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);
            
            // Draw the current frame.
            spriteBatch.Draw(Animation.Texture, new Rectangle((int)position.X, (int)position.Y, GlobalProps.TileSize, GlobalProps.TileSize), source, Color.White, 0.0f, Vector2.Zero, spriteEffects, 0.0F);
        }
    }
}