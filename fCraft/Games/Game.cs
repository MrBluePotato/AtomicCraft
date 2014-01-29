using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fCraft
{
    public static class Game
    {
        /// <summary> Server start up mode. </summary>
        public enum StartMode
        {
            /// <summary> Does not automatically start a game. </summary>
            None,

            /// <summary> Automatically starts PropHunt. </summary>
            PropHunt,
        }
    }
}