//Copyright (c) < LeChosenOne, DingusBingus >
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
using System.Threading;


namespace fCraft
{
    internal static class GameHandlers
    {
        internal static void Init()
        {
            ReleaseMode mode = ConfigKey.ReleaseMode.GetEnum<ReleaseMode>();
            if (mode == ReleaseMode.Dev)
            {
                CommandManager.RegisterCommand(CdTeamDeathMatch);
                CommandManager.RegisterCommand(CdGame);
                CommandManager.RegisterCommand(CdTeam);
            }
        }

        #region TeamDeathMatch
        static readonly CommandDescriptor CdTeamDeathMatch = new CommandDescriptor
                {
                    Name = "TeamDeathMatch",
                    Aliases = new[] { "tdm" },
                    Category = CommandCategory.World,
                    Permissions = new Permission[] { Permission.Games },
                    IsConsoleSafe = false,
                    Usage = "/TeamDeathMatch [time/score/scorelimit/timelimit/help]",
                    Handler = TDHandler
                };


        private static void TDHandler(Player player, Command cmd)       //For TDM Game: starting/ending game, customizing game options, viewing score, etc.
        {
            string Option = cmd.Next();
            World world = player.World;


            if (string.IsNullOrEmpty(Option))
            {
                CdTeamDeathMatch.PrintUsage(player);
                return;
            }
            if (!TeamDeathMatch.isOn && (Option.ToLower() == "timelimit" || Option.ToLower() == "scorelimit" || Option.ToLower() == "timedelay"))
            {
                if (Option.ToLower() == "timelimit")    //option to change the length of the game (5m default)
                {
                    string time = cmd.Next();
                    if (time == null)
                    {
                        player.Message("/TDM timelimit <limit> (whole number in minutes)\n&HNote: The acceptable times are from 1-20 minutes.");
                        return;
                    }
                    int timeLimit = 0;
                    bool parsed = Int32.TryParse(time, out timeLimit);
                    if (!parsed)
                    {
                        player.Message("Enter a whole number in minutes.");
                        return;
                    }
                    if (timeLimit < 1 || timeLimit > 20)
                    {
                        player.Message("Acceptable times are between 1 and 20 minutes.");
                        return;
                    }
                    else
                    {
                        TeamDeathMatch.timeLimit = (timeLimit * 60);
                        player.Message("The time limit has been changed to &W{0}&S minutes.", timeLimit);
                        Server.Message("&cThe Team Death Match time limit has been changed to &w{0}&s minutes.", timeLimit);
                        return;
                    }
                }
                if (Option.ToLower() == "scorelimit")       //changes the score limit
                {
                    string score = cmd.Next();
                    if (score == null)
                    {
                        player.Message("/TDM scorelimit <limit> \n&HNote: The acceptable scores are from 5-300 points");
                        return;
                    }
                    int scoreLimit = 0;
                    bool parsed = Int32.TryParse(score, out scoreLimit);
                    if (!parsed)
                    {
                        player.Message("Enter a whole number score.");
                        return;
                    }
                    if (scoreLimit < 5 || scoreLimit > 300)
                    {
                        player.Message("Acceptable scores range from 5-300 points.");
                        return;
                    }
                    else
                    {
                        TeamDeathMatch.scoreLimit = scoreLimit;
                        player.Message("The score limit has been changed to &W{0}&s points.", scoreLimit);
                        Server.Message("&cThe Team Death Match time limit has been changed to &w{0}&s minutes.", scoreLimit);
                        return;
                    }
                }
            }
            if (TeamDeathMatch.isOn && (Option.ToLower() == "timelimit" || Option.ToLower() == "scorelimit"))
            {
                player.Message("You cannot adjust game settings while a game is going on");
                return;
            }
            if (Option.ToLower() == "score")       //scoreboard for the matchs, different messages for when the game has ended. //td score
            {
                int red = TeamDeathMatch.redScore;
                int blue = TeamDeathMatch.blueScore;


                if (red > blue)
                {
                    if (player.Info.isOnRedTeam)
                    {
                        player.Message("&sYour team is winning {0} to {1}", red, blue);
                        return;
                    }
                    if (player.Info.isOnBlueTeam)
                    {
                        player.Message("&sYour team is losing {0} to {1}", red, blue);
                        return;
                    }
                    else
                    {
                        player.Message("&sThe &cRed Team&s won {0} to {1}", red, blue);
                        return;
                    }
                }
                if (red < blue)
                {
                    if (player.Info.isOnBlueTeam)
                    {
                        player.Message("&sYour team is winning {0} to {1}", blue, red);
                        return;
                    }
                    if (player.Info.isOnRedTeam)
                    {
                        player.Message("&sYour team is losing {0} to {1}", blue, red);
                        return;
                    }
                    else
                    {
                        player.Message("&sThe &1Blue Team&s won {0} to {1}", blue, red);
                        return;
                    }
                }
                if (red == blue)
                {
                    if (player.Info.isPlayingTD)
                    {
                        player.Message("&sThe teams are tied at {0}!", blue);
                        return;
                    }
                    else
                    {
                        player.Message("&sThe teams tied at {0}!", blue);
                        return;
                    }
                }
            }
            if (Option.ToLower() == "settings") //shows the current settings for the game (time limit, time delay, score limit)
            {
                player.Message("The Current Settings For TDM: Time Delay: &c{0}&ss | Time Limit: &c{1}&sm | Score Limit: &c{2}&s points",
                    TeamDeathMatch.timeDelay, (TeamDeathMatch.timeLimit / 60), TeamDeathMatch.scoreLimit);
                return;
            }
            if (Option.ToLower() == "help") //detailed help for the cmd
            {
                player.Message("Showing Option Descriptions for /TD (Option):\n&HTime &f- Tells how much time left in the game"
                + "\n&HScore &f- Tells the score of the current game(or last game played)"
                + "\n&HScoreLimit [number(5-300)] &f- Sets the score at which the game will end (Enter Whole Numbers from 5-300)"
                + "\n&HTimeLimit [time(m)] &f- Sets the time at which the game will end (Enter whole minutes from 1-15)"
                + "\n&HTimeDelay [time(s)] &f- Sets the time delay at the beginning of the match (Enter 10 second incriments from 10-60)"
                + "\n&HSettings&f - Shows the current TDM settings"
                + "\n&HAbout &f- General Game Description and Credits"
                + "\n&HDefaults&f: TimeDelay: 20s, TimeLimit: 5m, ScoreLimit 50");
                return;
            }
            if (Option.ToLower() == "time" || Option.ToLower() == "timeleft")
            {
                if (player.Info.isPlayingTD)
                {
                    player.Message("&fThere are &W{0}&f seconds left in the game.", TeamDeathMatch.timeLeft);
                    return;
                }
                else
                {
                    player.Message("&fThere are no games of Team DeathMatch going on.");
                    return;
                }
            }
            else
            {
                CdTeamDeathMatch.PrintUsage(player);
                return;
            }
        }
        #endregion


        #region GameMainHandler
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
        #endregion


        #region TeamHandler
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
        #endregion
    }
}
