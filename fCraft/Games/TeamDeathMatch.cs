//This game was created by DingusBungus 2013. Permission was granted by LeChosenOne for MrBluePotato to modify and include this in AtomicCraft.

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
using fCraft.Events;

namespace fCraft
{
    class TeamDeathMatch
    {
        //Team Tags
        public const string redTeam = "&C[Red]";
        public const string blueTeam = "&1[Blue]";

        //TDM stats
        public static int blueScore = 0;
        public static int redScore = 0;
        public static int redTeamCount = 0;
        public static int blueTeamCount = 0;

        //Timing
        public static int timeLeft = 0;
        private static SchedulerTask task_;
        public static TeamDeathMatch instance;
        public static DateTime startTime;
        public static DateTime lastChecked;

        //customization (initialized as defaults)
        public static int timeLimit = 300;
        public static int scoreLimit = 50;
        public static int timeDelay = 20;

        //TDM Game Bools
        public static bool isOn = false;
        private static bool started = false;

        public static World TDMworld_;

        public static TeamDeathMatch GetInstance(World world)
        {
            if (instance == null)
            {
                TDMworld_ = world;
                instance = new TeamDeathMatch();
                startTime = DateTime.UtcNow;
                task_ = new SchedulerTask(Interval, true).RunForever(TimeSpan.FromSeconds(1));
            }
            return instance;
        }

        public static void Start()
        {
            TDMworld_.gameMode = GameMode.TeamDeathMatch; //set the game mode
            Player.Console.ParseMessage(String.Format("/WSave {0} TDMbackup", TDMworld_.ToString()), true);
            Player.Console.ParseMessage(String.Format("/ok"), true);
            Scheduler.NewTask(t => TDMworld_.Players.Message("&WTeam Death Match will be starting in {0} seconds. &WGet ready!", timeDelay))
            .RunRepeating(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10), 1);
            Server.Players.Except(TDMworld_.Players).Message("&WTeam Death Matchi &fwill be starting in {0} seconds in the world {1} &WGet ready!", timeDelay, TDMworld_.ClassyName);
            Player.Disconnected += PlayerLeftServer;
            Player.Disconnected += PlayerLeftWorld;
        }

        public static void Stop(Player p) //for stopping the game early
        {
            if (p != null && TDMworld_ != null)
            {
                Player.Console.ParseMessage(String.Format("/WLoad TDMbackup {0}", TDMworld_.ToString()), true);
                Player.Console.ParseMessage(String.Format("/ok"), true);
                Server.Players.Except(TDMworld_.Players).Message("{0} ended Team Death Match on {1}.", p.ClassyName, TDMworld_.ClassyName);
                TDMworld_.Players.Message("{0}&S stopped Team Death Match.", p.ClassyName);
            }
            RevertGame();
            return;
        }

        public static Random rand = new Random();
        public static void Interval(SchedulerTask task)
        {
            //check to stop Interval
            if (TDMworld_.gameMode != GameMode.TeamDeathMatch || TDMworld_ == null)
            {
                TDMworld_ = null;
                task.Stop();
                return;
            }
            if (!started)
            {
                if (TDMworld_.Players.Count() < 2) //in case players leave the world or disconnect during the start delay
                {
                    TDMworld_.Players.Message("&WTeam Death Match&s requires at least 2 people to play.");
                    return;
                }
                if (startTime != null && (DateTime.UtcNow - startTime).TotalSeconds > timeDelay)
                {
                    TDMworld_.Players.Message("&WType &a/Team [red/blue]&w to join a team and begin!");
                    started = true;   //the game has officially started
                    isOn = true;
                    if (!TDMworld_.gunPhysics)
                    {
                        TDMworld_.EnableGunPhysics(Player.Console, true); //enables gun physics if they are not already on
                    }
                    lastChecked = DateTime.UtcNow;     //used for intervals
                    return;
                }

                //check if one of the teams have won
                if (redScore >= scoreLimit)
                {
                    TDMworld_.Players.Message("&fThe &cRed Team&f has won {1} to {0}!", blueScore, redScore);
                    Stop(null);
                    return;
                }
                if (blueScore >= scoreLimit)
                {
                    TDMworld_.Players.Message("&fThe &1Blue Team&f has won {1} to {0}!", redScore, blueScore);
                    Stop(null);
                    return;
                }
                if (blueScore == scoreLimit && redScore == scoreLimit) //if they somehow manage to tie which I am pretty sure is impossible
                {
                    TDMworld_.Players.Message("&fThe teams have tied!");
                    Stop(null);
                    return;
                }

                //check if time is up (delay time + start time)
                if (started && startTime != null && (DateTime.UtcNow - startTime).TotalSeconds >= (timeDelay + timeLimit))
                {
                    if (redScore > blueScore)
                    {
                        TDMworld_.Players.Message("&fThe &cRed Team&f has won {0} to {1}.", redScore, blueScore);
                        Stop(null);
                        return;
                    }
                    if (redScore < blueScore)
                    {
                        TDMworld_.Players.Message("&fThe &1Blue Team&f has won {0} to {1}.", blueScore, redScore);
                        Stop(null);
                        return;
                    }
                    if (redScore == blueScore)
                    {
                        TDMworld_.Players.Message("&fThe teams tied {0} to {1}!", blueScore, redScore);
                        Stop(null);
                        return;
                    }
                    if (TDMworld_.Players.Count() <= 1)
                    {
                        Stop(null);
                        return;
                    }
                }

                if (started && (DateTime.UtcNow - lastChecked).TotalSeconds > 10) //check if players left the world, forfeits if no players of that team left
                {
                    int redCount = TDMworld_.Players.Where(p => p.Info.isOnRedTeam).ToArray().Count();
                    int blueCount = TDMworld_.Players.Where(p => p.Info.isOnBlueTeam).ToArray().Count();
                    if (blueCount < 1 || redCount < 1)
                    {
                        if (blueTeamCount == 0)
                        {
                            if (TDMworld_.Players.Count() >= 1)
                            {
                                TDMworld_.Players.Message("&1Blue Team &fhas forfeited the game. &cRed Team &fwins!");
                            }
                            Stop(null);
                            return;
                        }
                        if (redTeamCount == 0)
                        {
                            if (TDMworld_.Players.Count() >= 1)
                            {
                                TDMworld_.Players.Message("&cRed Team &fhas forfeited the game. &1Blue Team &fwins!");
                            }
                            Stop(null);
                            return;
                        }
                        else
                        {
                            Stop(null);
                            return;
                        }
                    }
                }
                timeLeft = Convert.ToInt16(((timeDelay + timeLimit) - (DateTime.UtcNow - startTime).TotalSeconds));
                //Keep the players updated about the score
                if (lastChecked != null && (DateTime.UtcNow - lastChecked).TotalSeconds > 29.9 && timeLeft <= timeLimit)
                {
                    if (redScore > blueScore)
                    {
                        TDMworld_.Players.Message("&fThe &cRed Team&f is winning {0} to {1}.", redScore, blueScore);
                        TDMworld_.Players.Message("&fThere are &W{0}&f seconds left in the game.", timeLeft);
                    }
                    if (redScore < blueScore)
                    {
                        TDMworld_.Players.Message("&fThe &1Blue Team&f is winning {0} to {1}.", blueScore, redScore);
                        TDMworld_.Players.Message("&fThere are &W{0}&f seconds left in the game.", timeLeft);
                    }
                    if (redScore == blueScore)
                    {
                        TDMworld_.Players.Message("&fThe teams are tied at {0}!", blueScore);
                        TDMworld_.Players.Message("&fThere are &W{0}&f seconds left in the game.", timeLeft);
                    }
                    lastChecked = DateTime.UtcNow;
                }
                if (timeLeft == 10)
                {
                    TDMworld_.Players.Message("&WOnly 10 seconds left!");
                }
            }
        }

        

        public static void RevertGame() //Reset game bools/stats and stop timers
        {
            Player.Disconnected += PlayerLeftServer;
            Player.Disconnected += PlayerLeftWorld;
            TDMworld_.gameMode = GameMode.NULL;
            if (TDMworld_.gunPhysics)
            {
                TDMworld_.DisableGunPhysics(Player.Console, true);
            }
            task_.Stop();
            isOn = false;
            instance = null;
            started = false;
            TDMworld_ = null;
            redScore = 0;
            blueScore = 0;
            redTeamCount = 0;
            blueTeamCount = 0;
            RevertPlayer();
        }

        static void PlayerLeftServer(object sender, Events.PlayerDisconnectedEventArgs e)
        {
            if ((TeamDeathMatch.isOn) && (e.Player.isOnTDMTeam))
            {
                e.Player.iName = null;
                e.Player.Info.DisplayedName = e.Player.Info.TDMoldname;
                e.Player.Info.isOnRedTeam = false;
                e.Player.Info.isOnBlueTeam = false;
                e.Player.Info.isPlayingTD = false;
                e.Player.entityChanged = true;
                e.Player.isOnTDMTeam = false;
                return;
            }
        }
        static void PlayerLeftWorld(object sender, Events.PlayerDisconnectedEventArgs e)
        {
            if ((TeamDeathMatch.isOn) && (e.Player.isOnTDMTeam))
            {
                if (e.Player.World.Name != TDMworld_.ToString())
                {
                    e.Player.iName = null;
                    e.Player.Info.DisplayedName = e.Player.Info.TDMoldname;
                    e.Player.Info.isOnRedTeam = false;
                    e.Player.Info.isOnBlueTeam = false;
                    e.Player.Info.isPlayingTD = false;
                    e.Player.entityChanged = true;
                    e.Player.isOnTDMTeam = false;
                    return;
                }
            }
        }
        public static void RevertPlayer()    //reverts names for online players. offline players get reverted upon leaving the game
        {
            List<PlayerInfo> TDPlayers = new List<PlayerInfo>(PlayerDB.PlayerInfoList.Where(r => (r.isOnBlueTeam || r.isOnRedTeam) && r.IsOnline).ToArray());
            for (int i = 0; i < TDPlayers.Count(); i++)
            {
                string p1 = TDPlayers[i].Name.ToString();
                PlayerInfo pI = PlayerDB.FindPlayerInfoExact(p1);
                Player p = pI.PlayerObject;

                if (p != null)
                {
                    p.iName = null;
                    pI.DisplayedName = pI.TDMoldname;
                    pI.isOnRedTeam = false;
                    pI.isOnBlueTeam = false;
                    pI.isPlayingTD = false;
                    p.entityChanged = true;
                    p.isOnTDMTeam = false;
                }
            }
        }
    }
}
