//Copyright (C) <2011 - 2013>  <Jon Baker, Glenn Mariën and Lao Tszy>

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

// Modifications Copyright (c) 2013 Michael Cummings <michael.cummings.97@outlook.com>
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright
//      notice, this list of conditions and the following disclaimer in the
//      documentation and/or other materials provided with the distribution.
//    * Neither the name of 800Craft or the names of its
//      contributors may be used to endorse or promote products derived from this
//      software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
namespace fCraft
{
    static class GameMain
    {

        public static void Init()
        {
            //CommandManager.RegisterCommand(CdGame);
        }
        static readonly CommandDescriptor CdGame = new CommandDescriptor
        {
            Name = "Game",
            Category = CommandCategory.Game,
            Permissions = new Permission[] { Permission.ManageGame },
            IsConsoleSafe = false,
            Usage = "/Game [tdm] [start/stop]",
            Handler = GameHandler
        };
        private static void GameHandler(Player player, Command cmd)
        {
            string GameMode = cmd.Next();
            string Option = cmd.Next();
            World world = player.World;

            if (GameMode == null)
            {
                CdGame.PrintUsage(player);
                return;
            }
            /*if (GameMode.ToLower() == "zombie")
            {
                if (Option.ToLower() == "start")
                {
                    ZombieSurvival game = new ZombieSurvival(player.World);//move to world
                    game.Start();
                    return;
                }
                else if (Option.ToLower() == "stop")
                {
                    ZombieSurvival.Stop(player);
                    Server.Message("{0} &cended the game of zombie survival in the world {1}", player.ClassyName, world.ClassyName);
                    return;
                }
            }*/
            if (GameMode.ToLower() == "tdm")
            {
                if (Option == null)
                {
                    player.Message("&cYou must choose an option! &astart/stop");
                    return;
                }
                if (Option.ToLower() == "start")
                {
                    if (world == WorldManager.MainWorld)
                    {
                        player.Message("Team Death Match cannot be played on the main world.");
                        return;
                    }
                    if (TeamDeathMatch.isOn)
                    {
                        player.Message("Team Death Match is already started.");
                        return;
                    }
                    if (player.World.CountPlayers(true) < 2)
                    {
                        player.Message("There needs to be at least &W2&S players to play Team Death Match.");
                        return;
                    }
                    else
                    {
                        TeamDeathMatch.GetInstance(player.World);
                        TeamDeathMatch.Start();
                        return;
                    }
                }
                if (Option.ToLower() == "stop")
                {
                    {
                        if (TeamDeathMatch.isOn)
                        {
                            TeamDeathMatch.Stop(player);
                            return;
                        }
                        else
                        {
                            player.Message("No games of Team DeathMatch are going on");
                            return;
                        }
                    }
                }
            }
            if (Updater.CurrentRelease.IsFlagged(ReleaseFlags.Dev))
            {
                if (GameMode.ToLower() == "minefield")
                {
                    if (Option == null)
                    {
                        player.Message("&cYou must choose an option! &astart/stop");
                        return;
                    }
                    else if (Option.ToLower() == "start")
                    {
                        if (WorldManager.FindWorldExact("Minefield") != null)
                        {
                            player.Message("&WA game of Minefield is currently running and must first be stopped");
                            return;
                        }
                        else
                        {
                            MineField.GetInstance();
                            MineField.Start(player);
                            return;
                        }
                    }
                    else if (Option.ToLower() == "stop")
                    {
                        if (WorldManager.FindWorldExact("Minefield") == null)
                        {
                            player.Message("&WA game of Minefield is currently not running");
                            return;
                        }
                        MineField.Stop(player, false);
                        return;
                    }
                }
            }
            else
            {
                CdGame.PrintUsage(player);
                return;
            }
        }
    }
}