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
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using System.Threading;

namespace fCraft
{
    class PropHunt
    {
        //Randoms
        private static Random randBlock = new Random();
        public static Random randWorld = new Random();

        //Time stuff
        private static SchedulerTask task_;
        private DateTime startTime;
        public static DateTime lastChecked;
        public static DateTime roundEnd;
        public static int timeLeft = 0;
        public static int timeLimit = 45;
        public static int timeDelay = 10;

        //Bools
        public static bool isOn = false;
        public static bool VoteIsOn = false;
        public static bool roundStarted = false;
        private static bool votingRestarting = false;
        private static bool restartGame = false;

        //Stuff
        public static PropHunt instance;
        public static Game.StartMode startMode = ConfigKey.StartMode.GetEnum<Game.StartMode>();

        //Worlds and stufff
        private static World _world;
        private static World _winningWorld;
        public static World world1;
        public static World world2;
        public static World world3;

        //Lists
        private static string[] blockId = { "1", "2", "4", "5", "7", "12", "13", "17", "19", "64", "65" };

        public static List<Player> PropHuntPlayers = new List<Player>();
        public static List<World> PropHuntWorlds = new List<World>();
        public static List<Player> Voted = new List<Player>();

        //Voting
        public static int Voted1;
        public static int Voted2;
        public static int Voted3;

        //For on-demand instances of PropHunt
        public PropHunt(World world)
        {
            _world = world;
            startTime = DateTime.Now;
            task_ = new SchedulerTask(Interval, true).RunForever(TimeSpan.FromMilliseconds(250));
        }

        //Used if the server starts in prophunt
        public PropHunt()
        {
            lock (WorldManager.SyncRoot)
            {
                foreach (World w in WorldManager.Worlds)
                {
                    if (w.IsPropHunt)
                    {
                        PropHunt.PropHuntWorlds.Add(w);
                    }
                }
            }
            if (PropHuntWorlds.Count() <= 3)
            {
                Logger.Log(LogType.Error, "You must have at least 3 PropHunt maps. Please add some with /PropHunt add [mapname].");
                return;
            }
            _world = PropHuntWorlds[randWorld.Next(0, PropHuntWorlds.Count)];
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
            startTime = DateTime.Now;
            task_ = new SchedulerTask(Interval, true).RunForever(TimeSpan.FromMilliseconds(250));
            Logger.Log(LogType.SystemActivity, "&WPropHunt &S is starting in {0} seconds in world {0}", timeDelay, _world.ClassyName);
            _world.Players.Message("&WPropHunt &S is starting in {0} seconds in world {0}", timeDelay, _world.ClassyName);
#if DEBUG
            Server.Message("PropHunt is starting");
#endif
        }

        public void Interval(SchedulerTask task)
        {
            //check to stop Interval
            if (_world == null)
            {
                _world = null;
                task.Stop();
                return;
#if DEBUG
                Server.Message("World was null");
#endif
            }
            if (_world.gameMode != GameMode.PropHunt)
            {
                _world = null;
                task.Stop();
                return;
#if DEBUG
                Server.Message("Gamemode was not prophunt");
#endif
            }
            if (!isOn)
            {
                //Time delay if it is the on-demand instance of the game
                if (startTime != null && (DateTime.Now - startTime).TotalSeconds > timeDelay)
                {
                    timeDelay = 0;
                    foreach (Player p in _world.Players)
                    {
                        beginGame(p);
                        chooseSeeker();
                        p.isPlayingPropHunt = true;
                        if (!p.isPropHuntSeeker)
                        {
                            p.Model = blockId[randBlock.Next(0, blockId.Length)];
                            string blockName = Map.GetBlockByName(p.Model).ToString();
                            p.Message("You are disgused as {0}", blockName);
                        }
                    }
                }

                //Handlers for various things
                Player.Moving += PlayerMovingHandler;
                Player.Clicking += PlayerClickingHandler;
                Player.Connected += PlayerConnectedHandler;

                checkIdlesTask = Scheduler.NewTask(CheckIdles).RunForever(CheckIdlesInterval);

                isOn = true;
                lastChecked = DateTime.Now;     //used for intervals

#if DEBUG
                Server.Message("It is on and stuff...");
#endif
            }

            timeLeft = Convert.ToInt16(((timeDelay + timeLimit) - (DateTime.Now - startTime).TotalSeconds));
            if (isOn)
            {
#if !(DEBUG)
            if (_world.Players.Count(player => player.isPropHuntSeeker) == _world.Players.Count())
            {
                //Many things will happen here.
            }
            if (_world.Players.Count(player => player.isPropHuntSeeker) == 0 && _world.Players.Count() > 0)
            {
                //Lots of things will happen here.
            }
#endif
                //Ondemand prophunt
                if (timeLeft == 0 && startMode == Game.StartMode.None)
                {
                    Server.Message("The seeker was unable to find all the blocks!69");
                    return;
                }

                //Startup prophunt
                if (timeLeft == 0 && startMode == Game.StartMode.PropHunt)
                {
                    _world.Players.Message("The seeker was unable to find all the blocks!");
                    roundEnd = DateTime.Now;
                    roundStarted = false;
                    takeVote();
                    return;

                }
            }
            if (lastChecked != null && (DateTime.Now - lastChecked).TotalSeconds > 30 && timeLeft <= timeLimit)
            {
                _world.Players.Message("There are currently {0} block(s) and {1} seeker(s) left on {2}", _world.Players.Count() - _world.Players.Count(player => !player.isPropHuntSeeker), _world.Players.Count(player => player.isPropHuntSeeker), _world.ClassyName);
                _world.Players.Message("There are {0} seconds left!", timeLeft);
                lastChecked = DateTime.Now;
            }
        }


        public static void beginGame(Player player)
        {
            player.isPlayingPropHunt = true;
            PropHuntPlayers.Add(player);
            Server.Message("&WPropHunt is starting!");
            roundStarted = true;
#if DEBUG
            Server.Message("Beginning game....");
#endif
        }

        // Choose a random player as the seeker
        public static void chooseSeeker()
        {
            Random randNumber = new Random();
            int randSeeker = randNumber.Next(0, _world.Players.Length);
            Player seeker = _world.Players[randSeeker];

            if (_world.Players.Count(player => player.isPropHuntSeeker) == 0)
            {
                seeker.Message("&cYou were chosen as the seeker!");
                seeker.isPropHuntSeeker = true;
            }
        }

        //Called when the seeker tags a player (turns player into seeker)
        public static void makeSeeker(Player p)
        {
            p.isPropHuntTagged = false;
            p.Message("&cYou were tagged! You are now a seeker!");
            p.Model = "steve";
            p.isPropHuntSeeker = true;
        }

        //Resets player and map settings from the game
        public static void revertGame()
        {
            lock (_world.Players)
            {
                foreach (Player p in _world.Players)
                {
                    p.isPlayingPropHunt = false;
                    if (!p.isPropHuntSeeker)
                    {
                        p.isSolidBlock = false;
                        p.Model = "steve";
                    }
                    if (p.isPropHuntTagged)
                    {
                        p.isPropHuntTagged = false;
                    }
                    p.isPropHuntSeeker = false;
                }
            }
            isOn = false;

            _world.gameMode = GameMode.NULL;
            _world = null;

            instance = null;
            task_.Stop();
        }

        //Voting
        public void takeVote()
        {
            //Stop the game before voting
            task_.Stop();
            checkIdlesTask.Stop();
            Player.Moving -= PlayerMovingHandler;

            //Actual voting
            if (!votingRestarting)
            {
                World[] voteWorlds = PropHuntWorlds.OrderBy(x => randWorld.Next())
                                                   .Take(3)
                                                   .ToArray();
                world1 = voteWorlds[0];
                world2 = voteWorlds[1];
                world3 = voteWorlds[2];
            }
            Server.Players.Message("&S--------------------------------------------------------------");
            Server.Players.Message("Vote for the next map!");
            Server.Players.Message("&S/Vote &c1&S for {0}&S, &c2&S for {1}&S, and &c3&S for {2}",
                                   world1.ClassyName, world2.ClassyName, world3.ClassyName);
            Server.Players.Message("&S--------------------------------------------------------------");
            VoteIsOn = true;

            Scheduler.NewTask((task) => VoteCheck())
                     .RunOnce(TimeSpan.FromSeconds(60));

        }

        public void VoteCheck()
        {
            if (VoteIsOn)
            {
                if (PropHunt.Voted.Count() == 0)
                {
                    Logger.Log(LogType.Warning, "There we no votes. Voting will restart.");
                    Server.Message("&cThere were no votes. Voting will restart.");
                    votingRestarting = true;
                    takeVote();
                    return;
                }
                if (Voted1 < Voted2 || Voted1 < Voted3)
                {
                    _winningWorld = world1;
                }
                else if (Voted2 < Voted1 || Voted2 < Voted3)
                {
                    _winningWorld = world2;
                }
                else if (Voted3 < Voted1 || Voted3 < Voted2)
                {
                    _winningWorld = world3;
                }
                Server.Players.Message("&S--------------------------------------------------------------");
                Server.Players.Message("&SVoting results are in! &A{0}&S:&C {1}, &A{2}&S:&C {3}, &A{4}&S:&C {5}", world1.ClassyName,
                                       Voted1, world2.ClassyName, Voted2, world3.ClassyName, Voted3);
                Server.Players.Message("&SThe next map is: {0}", _winningWorld.ClassyName);
                Server.Players.Message("&S--------------------------------------------------------------");
                VoteIsOn = false;
                foreach (Player p in Voted)
                {
                    p.HasVoted = false;
                }
                foreach (Player p in _world.Players)
                {
                    if (p.isPlayingPropHunt)
                    {
#if DEBUG
                        Server.Message("Rejoin world");
#endif
                        _world = _winningWorld;
                        p.JoinWorld(_world);
                    }
                }
                PropHunt game = new PropHunt();
                game.Start();
            }
        }

        // Avoid re-defining the list every time your handler is called. Make it static!
        static Block[] clickableBlocks = {
            Block.Stone, Block.Grass, Block.Dirt, Block.Cobblestone,
            Block.Plank, Block.Bedrock, Block.Sand, Block.Gravel,
            Block.Log, Block.Sponge, Block.Crate, Block.StoneBrick };

        public static void PlayerClickingHandler(object sender, fCraft.Events.PlayerClickingEventArgs e)
        {
            // if player clicked a non-air block
            if (e.Action == ClickAction.Delete)
            {
                Block currentBlock = e.Player.WorldMap.GetBlock(e.Coords); // Gets the blocks coords
                // Check if currentBlock is on the list
                if (clickableBlocks.Contains(currentBlock))
                {
                    foreach (Player p in _world.Players)
                    {
                        if (p.prophuntSolidPos == e.Coords && p.isSolidBlock)
                        {
                            //Remove the players block
                            Block airBlock = Block.Air;
                            BlockUpdate blockUpdate = new BlockUpdate(null, p.prophuntSolidPos, airBlock);
                            p.World.Map.QueueUpdate(blockUpdate);

                            //Do the other stuff
                            p.Message("&cA seeker has found you! Run away!");
                            p.isPropHuntTagged = true;
                            p.ResetIdleTimer();
                            p.isSolidBlock = false;
                            p.Info.IsHidden = false;
                            Player.RaisePlayerHideChangedEvent(p);
                        }
                    }
                    e.Cancel = true;
                }
            }
        }

        // Checks if the seeker tagged a player, after they broke the block form
        public static void PlayerMovingHandler(object sender, fCraft.Events.PlayerMovingEventArgs e)
        {
            if (e.Player.isPropHuntSeeker)
            {
                Vector3I oldPos = new Vector3I(e.OldPosition.X / 32, e.OldPosition.Y / 32, e.OldPosition.Z / 32); // Get the position of the player
                Vector3I newPos = new Vector3I(e.NewPosition.X / 32, e.NewPosition.Y / 32, e.NewPosition.Z / 32);

                if (oldPos.X != newPos.X || oldPos.Y != newPos.Y || oldPos.Z != newPos.Z) // Check if the positions are not the same (player moved)
                {
                    foreach (Player p in _world.Players)
                    {
                        Vector3I pos = p.Position.ToBlockCoords(); // Converts to block coords
                        if (e.NewPosition.DistanceSquaredTo(pos.ToPlayerCoords()) <= 48 * 48)
                        {
                            if (!p.isPropHuntSeeker && !p.isSolidBlock)
                            {
                                makeSeeker(p);
                            }
                        }
                    }
                }
            }
        }

        //Used if the server starts prophunt on launch. This brings the player to the world that the game is in.
        public static void PlayerConnectedHandler(object sender, fCraft.Events.PlayerConnectedEventArgs e)
        {
            e.StartingWorld = _world;
            if (PropHunt.startMode == Game.StartMode.PropHunt && !e.Player.isPlayingPropHunt && timeDelay == 0)
            {
                beginGame(e.Player);
                e.Player.Model = blockId[randBlock.Next(0, blockId.Length)];
                foreach (Player p in Server.Players)
                {
                    if (p.isPropHuntSeeker == true)
                    {
                        break;
                    }
                    else
                    {
                        if (Server.Players.Count() < 2)
                        {
                            chooseSeeker();
                            return;
                        }
                        else
                        {
                            Server.Message("&cThere are not enough players online to being PropHunt. Please try again later.");
                            return;
                        }
                    }
                }
                if (roundStarted && timeDelay != 0)
                {
                    e.Player.Message("You connected while a round was in progress. You have been made a seeker.");
                    e.Player.isPropHuntSeeker = true;
                }
            }
        }

        // checks for idle players
        static SchedulerTask checkIdlesTask;
        static TimeSpan checkIdlesInterval = TimeSpan.FromSeconds(1);
        public static TimeSpan CheckIdlesInterval
        {
            get { return checkIdlesInterval; }
            set
            {
                if (value.Ticks < 0) throw new ArgumentException("CheckIdlesInterval may not be negative.");
                checkIdlesInterval = value;
                if (checkIdlesTask != null) checkIdlesTask.Interval = checkIdlesInterval;
            }
        }

        public static void CheckIdles(SchedulerTask task)
        {
            Player[] tempPlayerList = _world.Players;
            for (int i = 0; i < tempPlayerList.Length; i++)
            {
                Player player = tempPlayerList[i];
                if (player.IdleTime.TotalSeconds < 7) continue;

                if (player.IdleTime.TotalSeconds >= 7)
                {
                    if (!player.isSolidBlock && !player.isPropHuntSeeker)
                    {
                        //Debug message to easily alert when player is idle
#if DEBUG
                        Server.Message("{0} is idle!", player.ClassyName);
#endif
                        player.Info.IsHidden = true;
                        player.isSolidBlock = true;

                        //Gets the coords of the player
                        short x = (short)(player.Position.X / 32 * 32 + 16);
                        short y = (short)(player.Position.Y / 32 * 32 + 16);
                        short z = (short)(player.Position.Z / 32 * 32);
                        Vector3I Pos = new Vector3I(player.Position.X / 32, player.Position.Y / 32, (player.Position.Z - 32) / 32);

                        //Saves the player pos when they were solid for later removing the block
                        player.prophuntSolidPos = Pos;

                        //Converts player's model block into Block.*blockname*
                        Block playerBlock = Map.GetBlockByName(player.Model);

                        //Places the block at the players current location
                        BlockUpdate blockUpdate = new BlockUpdate(null, Pos, playerBlock);
                        player.World.Map.QueueUpdate(blockUpdate);
                        player.WorldMap.SetBlock(Pos, playerBlock);

                        player.Message("&cYou are now a solid block. Don't move!");
                    }
                }
            }
        }
    }
}
