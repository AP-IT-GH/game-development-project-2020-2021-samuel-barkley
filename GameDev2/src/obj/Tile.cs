using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDev2.src.obj
{
    public enum TileCollision
    {
        Passable, Impassable, Platform
    }

    class Tile
    {
        public Texture2D texture;
        public TileCollision collision;

        public int size;
        public int width;
        public int height;

        public Tile(Texture2D texture, TileCollision collision)
        {
            this.texture = texture;
            this.collision = collision;

            size = GlobalProps.TileSize;
            width = size;
            height = size;
        }
    }
}
