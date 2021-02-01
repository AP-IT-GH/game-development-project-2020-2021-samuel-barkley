using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDev2.src
{
    class GlobalProps
    {
        // Var definitions;
        private static Point screenSize;
        private static int tileSize;

        // Usefull tempVars
        private static int nrOfTilesX = 16;

        private static int playerWidth;
        private static int playerHeight;

        private static Game1 gameInstance;

        // Getters and setters
        public static Point ScreenSize
        {
            get { return screenSize; }
            set
            {
                if (value.X > 0 && value.Y > 0)
                {
                    screenSize = new Point(value.X, value.Y);
                    tileSize = screenSize.X / nrOfTilesX;
                }
            }
        }

        public static int TileSize
        {
            get { return tileSize; }
        }

        public static int PlayerWidth
        {
            get { return playerWidth; }
            set { playerWidth = value; }
        }

        public static int PlayerHeight
        {
            get { return playerHeight; }
            set { playerHeight = value; }
        }

        public static Game1 GetGameInstance
        {
            get { return gameInstance; }
            set
            {
                if (gameInstance == null)
                {
                    gameInstance = value;
                }
            }
        }
    }
}
