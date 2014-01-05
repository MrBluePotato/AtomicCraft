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

namespace fCraft
{
    class PropHunt
    {
        private string[] blockId = {"1", "2", "4", "5", "7", "12", "13", "17", "19", "64", "65"};

        private Random randBlock = new Random();

        //Time stuff
        private static SchedulerTask task_;
        private DateTime startTime;
        public static DateTime lastChecked;
        public static int timeLeft = 0;
        public static int timeLimit = 300;
        public static int timeDelay = 10;

        public static bool isOn = false;
        public static PropHunt instance;
        private static World _world;
        public static List<Player> PropHuntPlayers = new List<Player>();

        public PropHunt (World world)
        {
        startTime = DateTime.UtcNow;
        _world = world;
        task_ = new SchedulerTask(Interval, true).RunForever(TimeSpan.FromSeconds(1));
        }
        public void Start()
        {
            startTime = DateTime.UtcNow;
            foreach (Player p in _world.Players)
            {
                p.Model = blockId[randBlock.Next(0, blockId.Length)];
                
            }
        }

        public void Interval(SchedulerTask task)
        {
            Server.Message("Doing stuff");
            //check to stop Interval
            if (_world.gameMode != GameMode.PropHunt || _world == null)
            {
                _world = null;
                task.Stop();
                return;
            }
            if (!isOn)
            {
                /*if (_world.Players.Count() < 2) //in case players leave the world or disconnect during the start delay
                {
                    if (!ConfigKey.IsNormal.Enabled())
                    {
                        return;
                    }
                    else
                    {
                        _world.Players.Message("&WPropHunt&s requires at least 4 people to play.");
                        return;
                    }
                }*/
                if (startTime != null && (DateTime.UtcNow - startTime).TotalSeconds > timeDelay)
                {
                    foreach (Player p in _world.Players)
                    {
                        beginGame(p);
                        chooseSeeker();
                    }
                    isOn = true;
                    lastChecked = DateTime.UtcNow;     //used for intervals
                    Server.Message("it is on and stuff");
                    return;
                }
            }
            /*if (isOn && (DateTime.UtcNow - lastChecked).TotalSeconds > 10) //check if players left the world, forfeits if no players of that team left
            {
                if (_world.Players.Count(player => player.Info.isInfected) == _world.Players.Count())
                {
                    ZombiesWin();
                    return;
                }
                if (_world.Players.Count(player => player.Info.isInfected) == 0 && _world.Players.Count() > 0)
                {
                    HumansWin();
                    return;
                }

            }*/
            timeLeft = Convert.ToInt16(((timeDelay + timeLimit) - (DateTime.Now - startTime).TotalSeconds));

            if (lastChecked != null && (DateTime.UtcNow - lastChecked).TotalSeconds > 29.9 && timeLeft <= timeLimit)
            {
                _world.Players.Message("There are currently {0} human(s) and {1} zombie(s) left on {2}", _world.Players.Count() - _world.Players.Count(player => player.Info.isInfected), _world.Players.Count(player => player.Info.isInfected), _world.ClassyName);
            }
        }

        public static void beginGame(Player player)
        {

            player.Info.isPlayingPropHunt = true;
            PropHuntPlayers.Add(player);
            Server.Message("beginning game....");
        }

        // Choose a random player as the seeker
        public static void chooseSeeker()
        {
            Random randNumber = new Random();
            int randSeeker = randNumber.Next(0, _world.Players.Length);
            Player seeker = _world.Players[randSeeker];

            if (_world.Players.Count(player => player.Info.isSeeker) == 0)
            {
                _world.Players.Message("&c{0} has been infected!", seeker.Name);
            }
        }
    }
}
