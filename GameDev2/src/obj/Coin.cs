using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDev2.src.obj
{
    public class Coin
    {
        private Texture2D texture;
        private Vector2 pos;
        private Vector2 origin;
        private Map map;
        private int value;

        private bool isAlive;

        public Vector2 Pos
        {
            get { return pos; }
        }

        public Map Map
        {
            get { return map; }
        }

        public bool IsAlive
        {
            get { return isAlive; }
        }

        public Rectangle LocalBounds
        {
            get { return new Rectangle((int) Pos.X,(int) Pos.Y, GlobalProps.TileSize, GlobalProps.TileSize); }
        }

        public int Value
        {
            get { return 10; }
        }

        public Coin(Map map, Vector2 pos)
        {
            this.map = map;
            this.pos = pos;

            LoadContent();
        }

        private void LoadContent()
        {
            texture = Map.Content.Load<Texture2D>("Sprites/coin/coin");
            origin = new Vector2(texture.Width / 2.0F, texture.Height / 2.0F);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(texture, Pos, null, Color.White, 0.0F, origin, 1.0F, SpriteEffects.None, 0.0F);
            spriteBatch.Draw(texture, destinationRectangle: new Rectangle((int) Pos.X - (GlobalProps.TileSize / 2), (int) Pos.Y - (GlobalProps.TileSize / 2), GlobalProps.TileSize, GlobalProps.TileSize), null, Color.White, 0.0F, Vector2.Zero, SpriteEffects.None, 0.0F);
        }
    }
}
