using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

//Copyright (C) <2011 - 2014> <Jon Baker, Glenn Mariën and Lao Tszy>

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
    internal static class DevCommands
    {
#if DEBUG
        public static void Init()
        {
            /*
             * NOTE: These commands are unfinished, under development and non-supported.
             * If you are not using a dev build of AtomicCraft, please comment these the below out to ensure 
             * stability.
             * */

            CommandManager.RegisterCommand(CdBot);
            CommandManager.RegisterCommand(CdRandomTestCommand);
        }


        private static readonly CommandDescriptor CdBot = new CommandDescriptor
        {
            Name = "Bot",
            Category = CommandCategory.Fun,
            Permissions = new[] {Permission.Chat},
            IsConsoleSafe = false,
            NotRepeatable = true,
            Usage = "/Spell",
            Help = "Penis",
            UsableByFrozenPlayers = false,
            Handler = BotHandler,
        };

        internal static void BotHandler(Player player, Command cmd)
        {
            //Bot bot = player.Bot;
            string yes = cmd.Next();
            if (yes.ToLower() == "create")
            {
                string Name = cmd.Next();
                Position Pos = new Position(player.Position.X, player.Position.Y, player.Position.Z, player.Position.R,
                    player.Position.L);
                //player.Bot = new Bot(Name, Pos, 1, player.World);
                //player.Bot.SetBot();
            }
        }

                #region CheckBlock (Debug Only)
#if DEBUG
        private static readonly CommandDescriptor CdRandomTestCommand = new CommandDescriptor
        {
            Name = "TestCommand",
            Category = CommandCategory.Fun,
            Permissions = new[] { Permission.Chat },
            Usage = "/Yolo",
            Help = "Smd",
            Handler = TestCommandHandler
        };

        private static void TestCommandHandler(Player player, Command cmd)
        {
            //player.Message(player.HeldBlock.ToString());
            //player.Send(Packet.MakeHoldThis(49, 0));
            //string label = "label";
            //player.Send(Packet.MakeAddSelectionBox(0, label, 67, 55, 32, 75, 54, 32, 50, 100, 0, 75));
            //player.Send(Packet.PlaySound("random.explode", 71, 33, 56, 2));
            player.Send(Packet.MakeMessageType(100, ""));
        }
#endif

        #endregion
    }
#endif
    }