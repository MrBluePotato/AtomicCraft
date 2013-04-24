//Copyright (c) 2013 MrBluePotato [License coming soon :D]
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
            CommandManager.RegisterCommand(CdTeam);
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
            if (teamColor == "red")
            {
                if (TeamDeathMatch.blueTeamCount > TeamDeathMatch.redTeamCount)
                {
                    player.Message("&wYou have joined the &cRed Team&w.");
                    player.iName = Color.Red + player.Name;
                    player.Info.oldname = player.Info.DisplayedName;
                    player.Info.DisplayedName = "&f(" + TeamDeathMatch.redTeam + "&f) " + Color.Red + player.Name;
                    player.Info.isOnRedTeam = true;
                    player.Info.isOnBlueTeam = false;
                    player.Info.isPlayingTD = true;
                    player.entityChanged = true;
                    player.Info.gameKills = 0;
                    player.Info.gameDeaths = 0;
                    player.isOnTDMTeam = true;
                    TeamDeathMatch.redTeamCount++;
                    return;
                }
                else
                {
                    player.Message("&wThe &cRed Team&w is full. Joining the &1Blue Team&w.");
                    player.iName = Color.Blue + player.Name;
                    player.Info.oldname = player.Info.DisplayedName;
                    player.Info.DisplayedName = "&f(" + TeamDeathMatch.blueTeam + "&f) " + Color.Blue + player.Name;
                    player.Info.isOnRedTeam = false;
                    player.Info.isOnBlueTeam = true;
                    player.Info.isPlayingTD = true;
                    player.entityChanged = true;
                    player.Info.gameKills = 0;
                    player.Info.gameDeaths = 0;
                    player.isOnTDMTeam = true;
                    TeamDeathMatch.blueTeamCount++;
                }
            }
            if (teamColor == "blue")
            {
                player.Message("&wYou have joined the &1Blue Team&w.");
                player.Message("Let the games Begin! Type &H/Gun");
                player.iName = Color.Blue + player.Name;
                player.Info.oldname = player.Info.DisplayedName;
                player.Info.DisplayedName = "&f(" + TeamDeathMatch.blueTeam + "&f) " + Color.Blue + player.Name;
                player.Info.isOnRedTeam = false;
                player.Info.isOnBlueTeam = true;
                player.Info.isPlayingTD = true;
                player.entityChanged = true;
                player.Info.gameKills = 0;
                player.Info.gameDeaths = 0;
                player.isOnTDMTeam = true;
                TeamDeathMatch.blueTeamCount++;
            }
            else
            {
                player.Message("&wThe &cRed Team&w is full. Joining the &1Blue Team&w.");
                player.iName = Color.Red + player.Name;
                player.Info.oldname = player.Info.DisplayedName;
                player.Info.DisplayedName = "&f(" + TeamDeathMatch.redTeam + "&f) " + Color.Red + player.Name;
                player.Info.isOnRedTeam = true;
                player.Info.isOnBlueTeam = false;
                player.Info.isPlayingTD = true;
                player.entityChanged = true;
                player.Info.gameKills = 0;
                player.Info.gameDeaths = 0;
                player.isOnTDMTeam = true;
                TeamDeathMatch.redTeamCount++;
                return;
            }
            foreach (Player p in TeamDeathMatch.TDMworld_.Players)
            {
                if (p.Info.isPlayingTD)
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
            else
            {
                player.Message("&cYou are not in the Team Death Match world. Type &a/j {0}", TeamDeathMatch.TDMworld_);
                return;
            }
        }
    }
}
