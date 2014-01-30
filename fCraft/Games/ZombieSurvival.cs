//Copyright (C) <2011 - 2014>  <Jon Baker, Glenn Mariën and Lao Tszy>

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

namespace fCraft
{
    internal class ZombieSurvival
    {
        //Timing
        private const string _zomb = "&8_Infected_";
        private static SchedulerTask task_;
        public static DateTime startTime;
        public static DateTime lastChecked;
        public static int timeLeft = 0;
        public static int timeLimit = 300;
        public static int timeDelay = 10;

        //Values
        public static bool isOn = false;
        public static ZombieSurvival instance;
        private static World _world;
        public static Random rand = new Random();
        public static List<Player> ZombiePlayers = new List<Player>();

        public ZombieSurvival(World world)
        {
            startTime = DateTime.UtcNow;
            _world = world;
            task_ = new SchedulerTask(Interval, true).RunForever(TimeSpan.FromSeconds(1));
        }

        public void Start()
        {
            startTime = DateTime.UtcNow;
            /*if (!ConfigKey.IsNormal.Enabled())
            {
                var files = Directory.GetFiles("maps", "*.*").Where(name => !name.EndsWith(".lvlqonly")).ToArray();
                string zombieLevel = Path.GetFileNameWithoutExtension(files[rand.Next(files.Length - 1)]);
                Player.Console.ParseMessage(String.Format("/WLoad {0} {0}", zombieLevel), true);
                Player.Console.ParseMessage(String.Format("/Ok"), true);
                WorldManager.MainWorld = WorldManager.FindWorldExact(zombieLevel);
                WorldManager.MainWorld.BlockDB.Clear();
                WorldManager.MainWorld.gameMode = GameMode.ZombieSurvival; //set the game mode
                Scheduler.NewTask(t => Server.Message("&WZombie survival &fwill be starting in {0} seconds: &WGet ready!", timeDelay))
                .RunRepeating(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10), 1);
            }
            else
            {
                _world.gameMode = GameMode.ZombieSurvival; //set the game mode
                Scheduler.NewTask(t => _world.Players.Message("&WZombie survival &fwill be starting in {0} seconds: &WGet ready!", timeDelay))
                .RunRepeating(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10), 1);
            }*/
        }

        public static void Stop(Player p) //for stopping the game early
        {
            if (p != null && _world != null)
            {
                _world.Players.Message("{0}&S stopped the Zombie Survival on world {1}",
                    p.ClassyName, _world.ClassyName);
            }
            RevertGame();
            return;
        }

        public static void HumansWin()
        {
            _world.Players.Message("&cThe humans have won the game!");
            RevertGame();
            return;
        }

        public static void ZombiesWin() //
        {
            _world.Players.Message("&cThe zombies have won the game!");
            RevertGame();
            return;
        }

        public static void PlayerMoved(Player p, fCraft.Events.PlayerMovingEventArgs e)
        {
            if (p.IsInfected)
            {
                foreach (Player target in ZombiePlayers)
                {
                    if (p.Position == target.Position && !target.IsInfected)
                    {
                        _world.Players.Message("&c{0} infected {1}!", p.Name, target.Name);
                        target.Message("&cYou were infected by {0}", p.Name);
                        target.IsInfected = true;
                    }
                }
            }
        }

        public void Interval(SchedulerTask task)
        {
            //check to stop Interval
            if (_world.gameMode != GameMode.ZombieSurvival || _world == null)
            {
                _world = null;
                task.Stop();
                return;
            }
            if (!isOn)
            {
                if (_world.Players.Count() < 2) //in case players leave the world or disconnect during the start delay
                {
                    /*if (!ConfigKey.IsNormal.Enabled())
                    {
                        return;
                    }
                    else
                    {
                        _world.Players.Message("&WZombie Survival&s requires at least 4 people to play.");
                        return;
                    }*/
                }
                if (startTime != null && (DateTime.UtcNow - startTime).TotalSeconds > timeDelay)
                {
                    foreach (Player p in _world.Players)
                    {
                        int x = rand.Next(2, _world.Map.Width);
                        int y = rand.Next(2, _world.Map.Length);
                        int z1 = 0;
                        for (int z = _world.Map.Height - 1; z > 0; z--)
                        {
                            if (_world.Map.GetBlock(x, y, z) != Block.Air)
                            {
                                z1 = z + 3;
                                break;
                            }
                        }
                        p.TeleportTo(new Position(x, y, z1 + 2).ToVector3I().ToPlayerCoords());
                            //teleport players to a random position
                        beginGame(p);
                        chooseInfected();
                    }
                    isOn = true;
                    lastChecked = DateTime.UtcNow; //used for intervals
                    return;
                }
            }
            if (isOn && (DateTime.UtcNow - lastChecked).TotalSeconds > 10)
                //check if players left the world, forfeits if no players of that team left
            {
                if (_world.Players.Count(player => player.IsInfected) == _world.Players.Count())
                {
                    ZombiesWin();
                    return;
                }
                if (_world.Players.Count(player => player.IsInfected) == 0 && _world.Players.Count() > 0)
                {
                    HumansWin();
                    return;
                }
            }
            timeLeft = Convert.ToInt16(((timeDelay + timeLimit) - (DateTime.Now - startTime).TotalSeconds));

            if (lastChecked != null && (DateTime.UtcNow - lastChecked).TotalSeconds > 29.9 && timeLeft <= timeLimit)
            {
                _world.Players.Message("There are currently {0} human(s) and {1} zombie(s) left on {2}",
                    _world.Players.Count() - _world.Players.Count(player => player.IsInfected),
                    _world.Players.Count(player => player.IsInfected), _world.ClassyName);
            }
        }

        public static void beginGame(Player player)
        {
            player.IsPlayingZombieSurvival = true;
            ZombiePlayers.Add(player);
        }

        //Choose a random player as the infected player
        /*public static void chooseInfected()
        {
            infected.Info.isInfected = true;
            infected.Info.oldname = infected.Info.DisplayedName;
            infected.Info.DisplayedName = "&c[INFECTED]";
        }*/

        public void chooseInfected()
        {
            /*if ((!ConfigKey.IsNormal.Enabled()) && (WorldManager.MainWorld.Players.Count() < 1))
            {
                Server.Message("&cStill gotta figure out what to do here...");
                return;
            }
            else
            {
                Random rand = new Random();
                int min = 0;
                int max = 1;
                int num = rand.Next(min, max);
                Player p = _world.Players[num];
                ToZombie(null, p);
            }*/
        }

        public void ToZombie(Player infector, Player target)
        {
            if (infector == null)
            {
                _world.Players.Message("{0}&S has been the first to get &cInfected. &9Panic!", target.ClassyName);
                target.iName = _zomb;
                target.Model = "zombie";
                target.entityChanged = true;
                return;
            }
            if (infector.iName == _zomb && target.iName != _zomb)
            {
                target.iName = _zomb;
                target.Model = "zombie";
                target.entityChanged = true;
                _world.Players.Message("{0}&S was &cInfected&S by {1}",
                    target.ClassyName, infector.ClassyName);
                target.Message("&WYou are now a Zombie!");
                return;
            }
            else if (infector.iName != _zomb && target.iName == _zomb)
            {
                infector.iName = _zomb;
                target.Model = "zombie";
                infector.entityChanged = true;
                _world.Players.Message("{0}&S was &cInfected&S by {1}",
                    infector.ClassyName, target.ClassyName);
                infector.Message("&WYou are now a Zombie!");
            }
        }

        public static void RevertGame() //Reset game bools/stats and stop timers
        {
            _world.gameMode = GameMode.NULL;
            task_.Stop();
            isOn = false;
            instance = null;
            _world = null;

            foreach (Player p in ZombiePlayers)
            {
                p.iName = p.Name;
                p.Model = "steve";
                p.IsInfected = false;
                p.Info.DisplayedName = p.Oldname;
                p.IsPlayingZombieSurvival = false;
                p.Message("You are no longer playing zombie survival.");
            }
        }
    }
}