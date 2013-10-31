// Copyright (c) 2013 Michael Cummings <michael.cummings.97@outlook.com>
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

namespace fCraft
{
    static class Team
    {

        public static void Init()
        {
            //CommandManager.RegisterCommand(CdTeam);
        }
        static readonly CommandDescriptor CdTeam = new CommandDescriptor
        {
            Name = "Team",
            Category = CommandCategory.Game,
            Permissions = new Permission[] { Permission.Games },
            IsConsoleSafe = false,
            Usage = "/Team [red/blue]",
            Handler = TeamHandler
        };
        private static void TeamHandler(Player player, Command cmd)
        {
            string teamColor = cmd.Next();
            World world = player.World;
            if (teamColor == null)
            {
                player.Message("&cYou must choose a team color!");
                return;
            }
            if (!TeamDeathMatch.isOn)
            {
                player.Message("&cThere is currently no active game.");
                return;
            }
            if (world == TeamDeathMatch.TDMworld_)
            {
                if (!player.isOnTDMTeam)
                {
                    if (teamColor == "red")
                    {
                        if (TeamDeathMatch.redTeamCount == 0)
                        {
                            player.Message("&wYou have joined the &cRed Team&w.");
                            player.Message("&wType &H/Gun&w to begin!");
                            player.iName = Color.Red + player.Name;
                            player.Info.TDMoldname = player.Info.DisplayedName;
                            player.Info.DisplayedName = "&f(" + TeamDeathMatch.redTeam + "&f) " + Color.Red + player.Name;
                            player.Info.isOnRedTeam = true;
                            player.Info.isOnBlueTeam = false;
                            player.Info.isPlayingTD = true;
                            player.entityChanged = true;
                            player.Info.gameKills = 0;
                            player.Info.gameDeaths = 0;
                            player.isOnTDMTeam = true;
                            TeamDeathMatch.redTeamCount++;
                            RandomPosRed(player);
                            return;
                        }
                        if (TeamDeathMatch.redTeamCount < TeamDeathMatch.blueTeamCount)
                        {
                            player.Message("&wYou have joined the &cRed Team&w.");
                            player.Message("&wType &H/Gun&w to begin!");
                            player.iName = Color.Red + player.Name;
                            player.Info.TDMoldname = player.Info.DisplayedName;
                            player.Info.DisplayedName = "&f(" + TeamDeathMatch.redTeam + "&f) " + Color.Red + player.Name;
                            player.Info.isOnRedTeam = true;
                            player.Info.isOnBlueTeam = false;
                            player.Info.isPlayingTD = true;
                            player.entityChanged = true;
                            player.Info.gameKills = 0;
                            player.Info.gameDeaths = 0;
                            player.isOnTDMTeam = true;
                            TeamDeathMatch.redTeamCount++;
                            RandomPosRed(player);
                            return;
                        }
                        else if (TeamDeathMatch.redTeamCount > TeamDeathMatch.blueTeamCount)
                        {
                            player.Message("&wThe &cRed Team&w is full. Joining the &1Blue Team&w.");
                            player.Message("&wType &H/Gun&w to begin!");
                            player.iName = Color.Blue + player.Name;
                            player.Info.TDMoldname = player.Info.DisplayedName;
                            player.Info.DisplayedName = "&f(" + TeamDeathMatch.blueTeam + "&f) " + Color.Blue + player.Name;
                            player.Info.isOnRedTeam = false;
                            player.Info.isOnBlueTeam = true;
                            player.Info.isPlayingTD = true;
                            player.entityChanged = true;
                            player.Info.gameKills = 0;
                            player.Info.gameDeaths = 0;
                            player.isOnTDMTeam = true;
                            TeamDeathMatch.blueTeamCount++;
                            RandomPosRed(player);
                        }
                    }
                    if (teamColor == "blue")
                    {
                        if (TeamDeathMatch.blueTeamCount == 0)
                        {
                            player.Message("&wYou have joined the &1Blue Team&w.");
                            player.Message("&wType &H/Gun&w to begin!");
                            player.iName = Color.Blue + player.Name;
                            player.Info.TDMoldname = player.Info.DisplayedName;
                            player.Info.DisplayedName = "&f(" + TeamDeathMatch.blueTeam + "&f) " + Color.Blue + player.Name;
                            player.Info.isOnRedTeam = false;
                            player.Info.isOnBlueTeam = true;
                            player.Info.isPlayingTD = true;
                            player.entityChanged = true;
                            player.Info.gameKills = 0;
                            player.Info.gameDeaths = 0;
                            player.isOnTDMTeam = true;
                            TeamDeathMatch.blueTeamCount++;
                            RandomPosBlue(player);
                        }
                        if (TeamDeathMatch.blueTeamCount < TeamDeathMatch.redTeamCount)
                        {
                            player.Message("&wYou have joined the &1Blue Team&w.");
                            player.Message("&wType &H/Gun&w to begin!");
                            player.iName = Color.Blue + player.Name;
                            player.Info.TDMoldname = player.Info.DisplayedName;
                            player.Info.DisplayedName = "&f(" + TeamDeathMatch.blueTeam + "&f) " + Color.Blue + player.Name;
                            player.Info.isOnRedTeam = false;
                            player.Info.isOnBlueTeam = true;
                            player.Info.isPlayingTD = true;
                            player.entityChanged = true;
                            player.Info.gameKills = 0;
                            player.Info.gameDeaths = 0;
                            player.isOnTDMTeam = true;
                            TeamDeathMatch.blueTeamCount++;
                            RandomPosBlue(player);
                        }
                        else if (TeamDeathMatch.blueTeamCount > TeamDeathMatch.redTeamCount)
                        {
                            player.Message("&wThe &cRed Team&w is full. Joining the &1Blue Team&w.");
                            player.Message("&wType &H/Gun&w to begin!");
                            player.iName = Color.Red + player.Name;
                            player.Info.TDMoldname = player.Info.DisplayedName;
                            player.Info.DisplayedName = "&f(" + TeamDeathMatch.redTeam + "&f) " + Color.Red + player.Name;
                            player.Info.isOnRedTeam = true;
                            player.Info.isOnBlueTeam = false;
                            player.Info.isPlayingTD = true;
                            player.entityChanged = true;
                            player.Info.gameKills = 0;
                            player.Info.gameDeaths = 0;
                            player.isOnTDMTeam = true;
                            TeamDeathMatch.redTeamCount++;
                            RandomPosBlue(player);
                            return;
                        }
                    }
                }
                else
                {
                    player.Message("&cYou are already on a team!");
                    return;
                }
            }
            else
            {
                player.Message("&cYou are not in the Team Death Match world. Type &a/j {0}", TeamDeathMatch.TDMworld_);
                return;
            }
        }
        public static void RandomPosBlue(Player p)
        {
            Random rand = new Random();
            int x = rand.Next(2, TeamDeathMatch.TDMworld_.Map.Width);
            int y = rand.Next(2, TeamDeathMatch.TDMworld_.Map.Length);
            int z1 = 0;
            for (int z = TeamDeathMatch.TDMworld_.Map.Height - 1; z > 0; z--)
            {
                if (TeamDeathMatch.TDMworld_.Map.GetBlock(x, y, z) != Block.Air)
                {
                    z1 = z + 3;
                    break;
                }
            }
            if (p.isOnTDMTeam)
            {
                p.TeleportTo(new Position(x, y, z1 + 2).ToVector3I().ToPlayerCoords()); //teleport players to a random position
            }
        }
        public static void RandomPosRed(Player p)
        {
            Random rand = new Random();
            int x = rand.Next(2, TeamDeathMatch.TDMworld_.Map.Width);
            int y = rand.Next(2, TeamDeathMatch.TDMworld_.Map.Length);
            int z1 = 0;
            for (int z = TeamDeathMatch.TDMworld_.Map.Height - 1; z > 0; z--)
            {
                if (TeamDeathMatch.TDMworld_.Map.GetBlock(x, y, z) != Block.Air)
                {
                    z1 = z + 3;
                    break;
                }
            }
            if (p.isOnTDMTeam)
            {
                p.TeleportTo(new Position(x, y, z1 + 2).ToVector3I().ToPlayerCoords()); //teleport players to a random position
            }
        }
    }
}
