using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
//Copyright (C) <2011 - 2013> <Jon Baker, Glenn Mariën and Lao Tszy>

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
namespace fCraft
{
    static class DevCommands
    {

        public static void Init()
        {
            /*
             * NOTE: These commands are unfinished, under development and non-supported.
             * If you are not using a dev build of 800Craft, please comment these the below out to ensure 
             * stability.
             * */

            //CommandManager.RegisterCommand(CdBot);
        }


        /*static readonly CommandDescriptor CdBot = new CommandDescriptor {
            Name = "Bot",
            Category = CommandCategory.Fun,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = false,
            NotRepeatable = true,
            Usage = "/Spell",
            Help = "Penis",
            UsableByFrozenPlayers = false,
            Handler = BotHandler,
        };
        internal static void BotHandler ( Player player, Command cmd ) {
            Bot bot = player.Bot;
            string yes = cmd.Next();
            if ( yes.ToLower() == "create" ) {
                string Name = cmd.Next();
                Position Pos = new Position( player.Position.X, player.Position.Y, player.Position.Z, player.Position.R, player.Position.L );
                player.Bot = new Bot( Name, Pos, 1, player.World );
                //player.Bot.SetBot();
    }*/
    }
}
