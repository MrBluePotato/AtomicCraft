// Copyright (c) <2013 - 2014> Michael Cummings <michael.cummings.97@outlook.com>
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright
//      notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright
//      notice, this list of conditions and the following disclaimer in the
//      documentation and/or other materials provided with the distribution.
//    * Neither the name of AtomicCraft or the names of its
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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JetBrains.Annotations;
using System.Threading;

namespace fCraft
{
    internal class PropHunt
    {
        //Randoms
        private static Random randBlock = new Random();
        public static Random RandWorld = new Random();

        //Time stuff
        private static SchedulerTask _task;
        public static DateTime LastChecked;
        public static DateTime RoundEnd;
        public static int TimeLeft = 0;
        public static int TimeLimit = 45;
        public static int TimeDelay = 10;

        //Bools
        public static bool IsOn = false;
        public static bool VoteIsOn = false;
        public static bool RoundStarted = false;
        private static bool _votingRestarting;
        private static bool restartGame = false;

        //Stuff
        public static PropHunt Instance;
        public static Game.StartMode StartMode = ConfigKey.StartMode.GetEnum<Game.StartMode>();

        //Worlds and stufff
        private static World _world;
        private static World _winningWorld;
        public static World World1;
        public static World World2;
        public static World World3;

        //Lists
        private static string[] blockId = {"1", "2", "4", "5", "7", "12", "13", "17", "19", "64", "65"};

        public static List<Player> PropHuntPlayers = new List<Player>();
        public static List<World> PropHuntWorlds = new List<World>();
        public static List<Player> Voted = new List<Player>();

        //Voting
        public static int Voted1;
        public static int Voted2;
        public static int Voted3;

        private static Block[] clickableBlocks =
        {
            Block.Stone, Block.Grass, Block.Dirt, Block.Cobblestone,
            Block.Plank, Block.Bedrock, Block.Sand, Block.Gravel,
            Block.Log, Block.Sponge, Block.Crate, Block.StoneBrick
        };

        private static SchedulerTask _checkIdlesTask;
        private static TimeSpan _checkIdlesInterval = TimeSpan.FromSeconds(1);
        private DateTime _startTime;

        //For on-demand instances of PropHunt
        public PropHunt(World world)
        {
            _world = world;
            _startTime = DateTime.Now;
        }

        //Used if the server starts in prophunt
        public PropHunt()
        {
            if (PropHuntWorlds.Count <= 3)
            {
                Logger.Log(LogType.Error,
                    "You must have at least 3 PropHunt maps. Please add some with /PropHunt add [mapname].");
                return;
            }
            _world = PropHuntWorlds[RandWorld.Next(0, PropHuntWorlds.Count)];
        }

        public static TimeSpan CheckIdlesInterval
        {
            get { return _checkIdlesInterval; }
            set
            {
                if (value.Ticks < 0) throw new ArgumentException("CheckIdlesInterval may not be negative.");
                _checkIdlesInterval = value;
                if (_checkIdlesTask != null) _checkIdlesTask.Interval = _checkIdlesInterval;
            }
        }

        public void Start()
        {
#if !(DEBUG)
            if (_world.Players.Count() < 4) //in case players leave the world or disconnect during the start delay
            {
                _world.Players.Message("&WPropHunt&s requires at least 4 people to play.");
                return;
            }
#endif
            _world.gameMode = GameMode.PropHunt;
            _startTime = DateTime.Now;
            _task = new SchedulerTask(Interval, true).RunForever(TimeSpan.FromMilliseconds(250));
            Logger.Log(LogType.SystemActivity, "&WPropHunt &S is starting in {0} seconds in world {1}", TimeDelay,
                _world.ClassyName);
            _world.Players.Message("&WPropHunt &S is starting in {0} seconds in world {1}", TimeDelay, _world.ClassyName);
            foreach (Player p in _world.Players)
                PropHuntPlayers.Add(p);
        }

        public void Interval(SchedulerTask task)
        {
            //check to stop Interval
            if (_world == null)
            {
                _world = null;
                task.Stop();
                return;
            }
            if (_world.gameMode != GameMode.PropHunt)
            {
                _world = null;
                task.Stop();
                return;
            }
            if (!IsOn)
            {
                //Time delay if it is the on-demand instance of the game
                if ((DateTime.Now - _startTime).TotalSeconds > TimeDelay && StartMode != Game.StartMode.PropHunt)
                {
                    foreach (Player p in PropHuntPlayers)
                    {
                        BeginGame(p);
                        ChooseSeeker();
                        if (!p.IsPropHuntSeeker)
                        {
                            p.Model = blockId[randBlock.Next(0, blockId.Length)];
                            string blockName = Map.GetBlockByName(p.Model).ToString();
                            p.Message("You are disgused as {0}", blockName);
                            p.iName = " ";
                        }
                        p.IsPlayingPropHunt = true;
                    }
                }
                //Handlers for various things
                Player.Moving += PlayerMovingHandler;
                Player.Clicking += PlayerClickingHandler;
                Player.Connected += PlayerConnectedHandler;

                _checkIdlesTask = Scheduler.NewTask(CheckIdles).RunForever(CheckIdlesInterval);

                IsOn = true;
                LastChecked = DateTime.Now; //used for intervals

#if DEBUG
                Server.Message("It is on and stuff...");
#endif
            }

            TimeLeft = Convert.ToInt16(((TimeDelay + TimeLimit) - (DateTime.Now - _startTime).TotalSeconds));
            if (IsOn && TimeLeft == 0 && StartMode == Game.StartMode.PropHunt)
            {
                if (PropHuntPlayers.Count(player => player.IsPropHuntSeeker) >
                    PropHuntPlayers.Count(player => (!(player.IsPropHuntSeeker))))
                {
                    Server.Message("&cThe seekers won!");
                    RoundEnd = DateTime.Now;
                    RoundStarted = false;
                    TakeVote();
                }
                if (PropHuntPlayers.Count(player => player.IsPropHuntSeeker) <
                    PropHuntPlayers.Count(player => (!(player.IsPropHuntSeeker))))
                {
                    Server.Message("&cThe blocks won!");
                    RoundEnd = DateTime.Now;
                    RoundStarted = false;
                    TakeVote();
                }
                if (PropHuntPlayers.Count(player => player.IsPropHuntSeeker) ==
                    PropHuntPlayers.Count(player => (!(player.IsPropHuntSeeker))))
                {
                    Server.Message("&cIt's a tie!");
                    RoundEnd = DateTime.Now;
                    RoundStarted = false;
                    TakeVote();
                }
                //Ondemand prophunt
                if (TimeLeft == 0 && StartMode == Game.StartMode.None)
                {
                    Server.Message("&cThe seeker was unable to find all the blocks!");
                    return;
                }
            }
            if (LastChecked == null || !((DateTime.Now - LastChecked).TotalSeconds > 30) || TimeLeft > TimeLimit)
                return;
            _world.Players.Message("There are currently {0} block(s) and {1} seeker(s) left on {2}",
                _world.Players.Count() - _world.Players.Count(player => !player.IsPropHuntSeeker),
                _world.Players.Count(player => player.IsPropHuntSeeker), _world.ClassyName);
            _world.Players.Message("There are {0} seconds left!", TimeLeft);
            LastChecked = DateTime.Now;
        }


        public static void BeginGame(Player player)
        {
            player.IsPlayingPropHunt = true;
            PropHuntPlayers.Add(player);
            Server.Message("&WPropHunt is starting!");
            RoundStarted = true;
#if DEBUG
            Server.Message("Beginning game....");
#endif
        }

        // Choose a random player as the seeker
        public static void ChooseSeeker()
        {
            if (PropHuntPlayers.Count(player => player.IsPropHuntSeeker) != 0) return;
            Random randNumber = new Random();
            int randSeeker = randNumber.Next(0, PropHuntPlayers.Count);
            Player seeker = PropHuntPlayers[randSeeker];

            seeker.Message("&cYou were chosen as the seeker!");
            seeker.IsPropHuntSeeker = true;
        }

        //Called when the seeker tags a player (turns player into seeker)
        public static void MakeSeeker(Player p)
        {
            p.IsPropHuntTagged = false;
            p.Message("&cYou were tagged! You are now a seeker!");
            p.Model = "steve";
            p.IsPropHuntSeeker = true;
        }

        //Resets player and map settings from the game
        public static void RevertGame()
        {
            lock (PropHuntPlayers)
            {
                foreach (Player p in PropHuntPlayers)
                {
                    p.IsPlayingPropHunt = false;
                    if (!p.IsPropHuntSeeker)
                    {
                        p.IsSolidBlock = false;
                        p.Model = "steve";
                    }
                    if (p.IsPropHuntTagged)
                    {
                        p.IsPropHuntTagged = false;
                    }
                    p.IsPropHuntSeeker = false;
                }
            }
            IsOn = false;

            _world.gameMode = GameMode.NULL;
            _world = null;

            Instance = null;
            _task.Stop();
        }

        public static void RevertPlayers()
        {
            lock (PropHuntPlayers)
            {
                foreach (Player p in PropHuntPlayers)
                {
                    if (!p.IsPropHuntSeeker)
                    {
                        p.IsSolidBlock = false;
                        p.Model = "steve";
                    }
                    if (p.IsPropHuntTagged)
                    {
                        p.IsPropHuntTagged = false;
                    }
                    p.IsPropHuntSeeker = false;
                }
            }
        }

        //Voting
        public void TakeVote()
        {
            //Stop the game before voting
            _task.Stop();
            _checkIdlesTask.Stop();
            Player.Moving -= PlayerMovingHandler;
            RevertPlayers();


            //Actual voting
            if (!_votingRestarting)
            {
                World[] voteWorlds = PropHuntWorlds.OrderBy(x => RandWorld.Next())
                    .Take(3)
                    .ToArray();
                World1 = voteWorlds[0];
                World2 = voteWorlds[1];
                World3 = voteWorlds[2];
            }
            Server.Players.Message("&S--------------------------------------------------------------");
            Server.Players.Message("&SVote for the next map!");
            Server.Players.Message("&S/Vote &c1&S for {0}&S, &c2&S for {1}&S, and &c3&S for {2}",
                World1.ClassyName, World2.ClassyName, World3.ClassyName);
            Server.Players.Message("&S--------------------------------------------------------------");
            VoteIsOn = true;

            Scheduler.NewTask((task) => VoteCheck())
                .RunOnce(TimeSpan.FromSeconds(60));
        }

        public void VoteCheck()
        {
            if (!VoteIsOn) return;
            bool any = Voted.Any();
            if (!any)
            {
                Logger.Log(LogType.Warning, "There we no votes. Voting will restart.");
                Server.Message("&cThere were no votes. Voting will restart.");
                _votingRestarting = true;
                TakeVote();
                return;
            }
            if (Voted1 > Voted2 || Voted1 > Voted3)
            {
                _winningWorld = World1;
            }
            else if (Voted2 > Voted1 || Voted2 > Voted3)
            {
                _winningWorld = World2;
            }
            else if (Voted3 > Voted1 || Voted3 > Voted2)
            {
                _winningWorld = World3;
            }
            else
            {
                _winningWorld = World1;
            }
            Server.Players.Message("&S--------------------------------------------------------------");
            Server.Players.Message("&SVoting results are in! &A{0}&S:&C {1}, &A{2}&S:&C {3}, &A{4}&S:&C {5}",
                World1.ClassyName,
                Voted1, World2.ClassyName, Voted2, World3.ClassyName, Voted3);
            Server.Players.Message("&SThe next map is: {0}", _winningWorld.ClassyName);
            Server.Players.Message("&S--------------------------------------------------------------");
            VoteIsOn = false;
            foreach (Player p in Voted)
            {
                p.HasVoted = false;
            }
            foreach (Player p in PropHuntPlayers)
            {
#if DEBUG
                Server.Message("Joining new world.");
#endif
                p.JoinWorld(_winningWorld);
            }
            var game = new PropHunt(_winningWorld);
            Voted1 = 0;
            Voted2 = 0;
            Voted3 = 0;
            game.Start();
        }

        // Avoid re-defining the list every time your handler is called. Make it static!

        public static void PlayerClickingHandler(object sender, fCraft.Events.PlayerClickingEventArgs e)
        {
            // if player clicked a non-air block
            if (e.Action != ClickAction.Delete) return;
            Block currentBlock = e.Player.WorldMap.GetBlock(e.Coords); // Gets the blocks coords
            // Check if currentBlock is on the list
            if (!clickableBlocks.Contains(currentBlock)) return;
            foreach (Player p in _world.Players)
            {
                if (p.prophuntSolidPos != e.Coords || !p.IsSolidBlock) continue;
                //Remove the players block
                Block airBlock = Block.Air;
                var blockUpdate = new BlockUpdate(null, p.prophuntSolidPos, airBlock);
                if (p.World != null) if (p.World.Map != null) p.World.Map.QueueUpdate(blockUpdate);

                //Do the other stuff
                p.Message("&cA seeker has found you! Run away!");
                p.IsPropHuntTagged = true;
                p.ResetIdleTimer();
                p.IsSolidBlock = false;
                p.Info.IsHidden = false;
                Player.RaisePlayerHideChangedEvent(p);
            }
            e.Cancel = true;
        }

        // Checks if the seeker tagged a player, after they broke the block form
        public static void PlayerMovingHandler(object sender, fCraft.Events.PlayerMovingEventArgs e)
        {
            if (e.Player.IsPropHuntSeeker)
            {
                Vector3I oldPos = new Vector3I(e.OldPosition.X/32, e.OldPosition.Y/32, e.OldPosition.Z/32);
                    // Get the position of the player
                Vector3I newPos = new Vector3I(e.NewPosition.X/32, e.NewPosition.Y/32, e.NewPosition.Z/32);

                if (oldPos.X != newPos.X || oldPos.Y != newPos.Y || oldPos.Z != newPos.Z)
                    // Check if the positions are not the same (player moved)
                {
                    foreach (Player p in _world.Players)
                    {
                        Vector3I pos = p.Position.ToBlockCoords(); // Converts to block coords
                        if (e.NewPosition.DistanceSquaredTo(pos.ToPlayerCoords()) <= 48*48)
                        {
                            if (!p.IsPropHuntSeeker && !p.IsSolidBlock)
                            {
                                MakeSeeker(p);
                            }
                        }
                    }
                }
            }
        }

        //Used if the server starts prophunt on launch. This brings the player to the world that the game is in.
        public static void PlayerConnectedHandler(object sender, fCraft.Events.PlayerConnectedEventArgs e)
        {
            PropHuntPlayers.Add(e.Player);
            e.StartingWorld = _world;
            if (PropHunt.StartMode != Game.StartMode.PropHunt) return;
            BeginGame(e.Player);
            if (PropHuntPlayers.TakeWhile(p => !p.IsPropHuntSeeker).Any())
            {
                if (PropHuntPlayers.Count() >= 2)
                {
                    ChooseSeeker();
                    return;
                }
                Server.Message("&cThere are not enough players online to being PropHunt. Please try again later.");
                return;
            }
            if (RoundStarted && TimeDelay == 0)
            {
                e.Player.Message("&cYou connected while a round was in progress. You have been made a seeker.");
                e.Player.IsPropHuntSeeker = true;
                return;
            }
            e.Player.Model = blockId[randBlock.Next(0, blockId.Length)];
            string blockName = Map.GetBlockByName(e.Player.Model).ToString();
            e.Player.Message("You are disgused as {0}", blockName);
        }

        // checks for idle players

        public static void CheckIdles(SchedulerTask task)
        {
            foreach (Player p in PropHuntPlayers)
            {
                if (p.IdleTime.TotalSeconds < 7) continue;

                if (p.IdleTime.TotalSeconds >= 7)
                {
                    if (!p.IsSolidBlock && !p.IsPropHuntSeeker)
                    {
                        p.Info.IsHidden = true;
                        p.IsSolidBlock = true;

                        //Gets the coords of the player
                        short x = (short) (p.Position.X/32*32 + 16);
                        short y = (short) (p.Position.Y/32*32 + 16);
                        short z = (short) (p.Position.Z/32*32);
                        Vector3I Pos = new Vector3I(p.Position.X/32, p.Position.Y/32, (p.Position.Z - 32)/32);

                        //Saves the player pos when they were solid for later removing the block
                        p.prophuntSolidPos = Pos;

                        //Converts player's model block into Block.*blockname*
                        Block playerBlock = Map.GetBlockByName(p.Model);

                        //Places the block at the players current location
                        var blockUpdate = new BlockUpdate(null, Pos, playerBlock);
                        if (p.World != null) if (p.World.Map != null) p.World.Map.QueueUpdate(blockUpdate);
                        p.WorldMap.SetBlock(Pos, playerBlock);

                        p.Message("&cYou are now a solid block. Don't move!");
                    }
                }
            }
        }
    }
}