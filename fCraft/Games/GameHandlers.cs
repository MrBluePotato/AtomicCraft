//Copyright (c) < LeChosenOne, DingusBingus >
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//Modified by MrBluePotato
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
            CommandManager.RegisterCommand(CdTeamDeathMatch);
        }

        static readonly CommandDescriptor CdTeamDeathMatch = new CommandDescriptor
                {
                    Name = "TeamDeathMatch",
                    Aliases = new[] { "tdm" },
                    Category = CommandCategory.World,
                    Permissions = new Permission[] { Permission.Games },
                    IsConsoleSafe = false,
                    Usage = "/TeamDeathMatch [time/score/scorelimit/timelimit/about/help]",
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
            if (Option.ToLower() == "about")    //td about
            {
                player.Message("&cTeam Deathmatch&S is a team game where all players are assigned to a red or blue team. Players cannot shoot players on their own team. The game will start the gun physics for you. The game keeps score and notifications come up about the score and time left every 30 seconds. The Score Limit, Time Delay and Time Limit are customizable. Detailed help is on &H/TD Help"
                + "\n&SDeveloped for &5Legend&WCraft&S by &fDingus&0Bungus&S 2013 - Based on the template of ZombieGame.cs written by Jonty800.");
                return;
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
    }
}
