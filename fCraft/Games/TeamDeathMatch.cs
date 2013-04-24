//This game was created by DingusBungus 2013. Permission was granted by LeChosenOne for MrBluePotato to modify and include this in AtomicCraft.

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

        private static World world_;
        public static World TDMworld_;

        public static TeamDeathMatch GetInstance(World world)
        {
            if (instance == null)
            {
                world_ = world;
                instance = new TeamDeathMatch();
                startTime = DateTime.UtcNow;
                task_ = new SchedulerTask(Interval, true).RunForever(TimeSpan.FromSeconds(1));
            }
            return instance;
        }

        public static void Start()
        {
            world_.gameMode = GameMode.TeamDeathMatch; //set the game mode
            Scheduler.NewTask(t => world_.Players.Message("&WTeam Death Match will be starting in {0} seconds. &WGet ready!", timeDelay))
            .RunRepeating(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10), 1);
            Server.Players.Except(world_.Players).Message("&WTeam Death Matchi &fwill be starting in {0} seconds in the world {1} &WGet ready!", timeDelay, world_.ClassyName);
        }

        public static void Stop(Player p) //for stopping the game early
        {
            if (p != null && world_ != null)
            {
                Server.Players.Except(world_.Players).Message("{0} ended Team Death Match on {1}.", p.ClassyName, world_.ClassyName);
                world_.Players.Message("{0}&S stopped Team Death Match.", p.ClassyName);
            }
            RevertGame();
            return;
        }

        public static Random rand = new Random();
        public static void Interval(SchedulerTask task)
        {
            //check to stop Interval
            if (world_.gameMode != GameMode.TeamDeathMatch || world_ == null)
            {
                world_ = null;
                task.Stop();
                return;
            }
            if (!started)
            {
                if (world_.Players.Count() < 2) //in case players leave the world or disconnect during the start delay
                {
                    world_.Players.Message("&WTeam Death Match&s requires at least 2 people to play.");
                    return;
                }
                if (startTime != null && (DateTime.UtcNow - startTime).TotalSeconds > timeDelay)
                {
                    world_.Players.Message("&WType &a/Team [red/blue]&w to join a team and begin!");
                    started = true;   //the game has officially started
                    isOn = true;
                    if (!world_.gunPhysics)
                    {
                        world_.EnableGunPhysics(Player.Console, true); //enables gun physics if they are not already on
                    }
                    lastChecked = DateTime.UtcNow;     //used for intervals
                    return;
                }

                //check if one of the teams have won
                if (redScore >= scoreLimit)
                {
                    world_.Players.Message("&fThe &cRed Team&f has won {1} to {0}!", blueScore, redScore);
                    Stop(null);
                    return;
                }
                if (blueScore >= scoreLimit)
                {
                    world_.Players.Message("&fThe &1Blue Team&f has won {1} to {0}!", redScore, blueScore);
                    Stop(null);
                    return;
                }
                if (blueScore == scoreLimit && redScore == scoreLimit) //if they somehow manage to tie which I am pretty sure is impossible
                {
                    world_.Players.Message("&fThe teams have tied!");
                    Stop(null);
                    return;
                }

                //check if time is up (delay time + start time)
                if (started && startTime != null && (DateTime.UtcNow - startTime).TotalSeconds >= (timeDelay + timeLimit))
                {
                    if (redScore > blueScore)
                    {
                        world_.Players.Message("&fThe &cRed Team&f has won {0} to {1}.", redScore, blueScore);
                        Stop(null);
                        return;
                    }
                    if (redScore < blueScore)
                    {
                        world_.Players.Message("&fThe &1Blue Team&f has won {0} to {1}.", blueScore, redScore);
                        Stop(null);
                        return;
                    }
                    if (redScore == blueScore)
                    {
                        world_.Players.Message("&fThe teams tied {0} to {1}!", blueScore, redScore);
                        Stop(null);
                        return;
                    }
                    if (world_.Players.Count() <= 1)
                    {
                        Stop(null);
                        return;
                    }
                }

                if (started && (DateTime.UtcNow - lastChecked).TotalSeconds > 10) //check if players left the world, forfeits if no players of that team left
                {
                    int redCount = world_.Players.Where(p => p.Info.isOnRedTeam).ToArray().Count();
                    int blueCount = world_.Players.Where(p => p.Info.isOnBlueTeam).ToArray().Count();
                    if (blueCount < 1 || redCount < 1)
                    {
                        if (blueTeamCount == 0)
                        {
                            if (world_.Players.Count() >= 1)
                            {
                                world_.Players.Message("&1Blue Team &fhas forfeited the game. &cRed Team &fwins!");
                            }
                            Stop(null);
                            return;
                        }
                        if (redTeamCount == 0)
                        {
                            if (world_.Players.Count() >= 1)
                            {
                                world_.Players.Message("&cRed Team &fhas forfeited the game. &1Blue Team &fwins!");
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
                        world_.Players.Message("&fThe &cRed Team&f is winning {0} to {1}.", redScore, blueScore);
                        world_.Players.Message("&fThere are &W{0}&f seconds left in the game.", timeLeft);
                    }
                    if (redScore < blueScore)
                    {
                        world_.Players.Message("&fThe &1Blue Team&f is winning {0} to {1}.", blueScore, redScore);
                        world_.Players.Message("&fThere are &W{0}&f seconds left in the game.", timeLeft);
                    }
                    if (redScore == blueScore)
                    {
                        world_.Players.Message("&fThe teams are tied at {0}!", blueScore);
                        world_.Players.Message("&fThere are &W{0}&f seconds left in the game.", timeLeft);
                    }
                    lastChecked = DateTime.UtcNow;
                }
                if (timeLeft == 10)
                {
                    world_.Players.Message("&WOnly 10 seconds left!");
                }
            }
        }


        public static void RevertGame() //Reset game bools/stats and stop timers
        {
            world_.gameMode = GameMode.NULL;
            if (world_.gunPhysics)
            {
                world_.DisableGunPhysics(Player.Console, true);
            }
            task_.Stop();
            isOn = false;
            instance = null;
            started = false;
            world_ = null;
            redScore = 0;
            blueScore = 0;
            redTeamCount = 0;
            blueTeamCount = 0;
            RevertPlayer();
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
                    pI.DisplayedName = pI.oldname;
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
