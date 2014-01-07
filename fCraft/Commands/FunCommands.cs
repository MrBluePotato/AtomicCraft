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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomMaze;
using System.Threading;

namespace fCraft
{
    internal static class FunCommands
    {
        internal static void Init()
        {
            CommandManager.RegisterCommand(CdRandomMaze);
            CommandManager.RegisterCommand(CdMazeCuboid);
            CommandManager.RegisterCommand(CdFirework);
            CommandManager.RegisterCommand(CdLife);
            CommandManager.RegisterCommand(CdPossess);
            CommandManager.RegisterCommand(CdUnpossess);
            CommandManager.RegisterCommand(CdChangeModel);
            CommandManager.RegisterCommand(CdChangeWeather);
        }

        #region Possess

        static readonly CommandDescriptor CdPossess = new CommandDescriptor
        {
            Name = "Possess",
            Category = CommandCategory.Fun,
            Permissions = new[] { Permission.Possess },
            Usage = "/Possess PlayerName",
            Handler = PossessHandler
        };

        static void PossessHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdPossess.PrintUsage(player);
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null) return;
            if (target.Immortal)
            {
                player.Message("You cannot possess {0}&S, they are immortal", target.ClassyName);
                return;
            }
            if (target == player)
            {
                player.Message("You cannot possess yourself.");
                return;
            }

            if (!player.Can(Permission.Possess, target.Info.Rank))
            {
                player.Message("You may only possess players ranked {0}&S or lower.",
                player.Info.Rank.GetLimit(Permission.Possess).ClassyName);
                player.Message("{0}&S is ranked {1}",
                                target.ClassyName, target.Info.Rank.ClassyName);
                return;
            }

            if (!player.Possess(target))
            {
                player.Message("Already possessing {0}", target.ClassyName);
            }
        }
        #endregion


        #region Unpossess
        static readonly CommandDescriptor CdUnpossess = new CommandDescriptor
        {
            Name = "unpossess",
            Category = CommandCategory.Fun,
            Permissions = new[] { Permission.Possess },
            NotRepeatable = true,
            Usage = "/Unpossess target",
            Handler = UnpossessHandler
        };

        static void UnpossessHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdUnpossess.PrintUsage(player);
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, true, true);
            if (target == null) return;

            if (!player.StopPossessing(target))
            {
                player.Message("You are not currently possessing anyone.");
            }
        }

        #endregion


        #region Life
        static readonly CommandDescriptor CdLife = new CommandDescriptor
        {
            Name = "Life",
            Category = CommandCategory.Fun,
            Permissions = new[] { Permission.DrawAdvanced },
            IsConsoleSafe = false,
            NotRepeatable = true,
            Usage = "/Life <command> [params]",
            Help = "&HGoogle \"Conwey's Game of Life\"\n'&H/Life help'&S for more usage info\n(c) 2012 LaoTszy",
            UsableByFrozenPlayers = false,
            Handler = LifeHandlerFunc,
        };
        static void LifeHandlerFunc(Player p, Command cmd)
        {
            try
            {
                if (!cmd.HasNext)
                {
                    p.Message("&H/Life <command> <params>. Commands are Help, Create, Delete, Start, Stop, Set, List, Print");
                    p.Message("Type /Life help <command> for more information");
                    return;
                }
                LifeHandler.ProcessCommand(p, cmd);
            }
            catch (Exception e)
            {
                p.Message("Error: " + e.Message);
            }
        }
        #endregion


        #region Firework
        static readonly CommandDescriptor CdFirework = new CommandDescriptor
        {
            Name = "Firework",
            Category = CommandCategory.Fun,
            Permissions = new[] { Permission.Fireworks },
            IsConsoleSafe = false,
            NotRepeatable = false,
            Usage = "/Firework",
            Help = "&HToggles Firework Mode on/off for yourself. " +
            "All Gold blocks will be replaced with fireworks if " +
            "firework physics are enabled for the current world.",
            UsableByFrozenPlayers = false,
            Handler = FireworkHandler
        };

        static void FireworkHandler(Player player, Command cmd)
        {
            if (player.fireworkMode)
            {
                player.fireworkMode = false;
                player.Message("Firework Mode has been turned off.");
                return;
            }
            else
            {
                player.fireworkMode = true;
                player.Message("Firework Mode has been turned on. " +
                    "All Gold blocks are now being replaced with Fireworks.");
            }
        }
        #endregion


        #region RandomMaze
        static readonly CommandDescriptor CdRandomMaze = new CommandDescriptor
        {
            Name = "RandomMaze",
            Aliases = new string[] { "3dmaze" },
            Category = CommandCategory.Fun,
            Permissions = new Permission[] { Permission.DrawAdvanced },
            RepeatableSelection = true,
            Help =
                "Choose the size (width, length and height) and it will draw a random maze at the chosen point. " +
                "Optional parameters tell if the lifts are to be drawn and if hint blocks (log) are to be added. \n(C) 2012 Lao Tszy",
            Usage = "/randommaze <width> <length> <height> [nolifts] [hints]",
            Handler = MazeHandler
        };
        static void MazeHandler(Player p, Command cmd)
        {
            try
            {
                RandomMazeDrawOperation op = new RandomMazeDrawOperation(p, cmd);
                BuildingCommands.DrawOperationBegin(p, cmd, op);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Error: " + e.Message);
            }
        }
        #endregion


        #region MazeCuboid
        static readonly CommandDescriptor CdMazeCuboid = new CommandDescriptor
        {
            Name = "MazeCuboid",
            Aliases = new string[] { "Mc", "Mz", "Maze" },
            Category = CommandCategory.Fun,
            Permissions = new Permission[] { Permission.DrawAdvanced },
            RepeatableSelection = true,
            Help =
                "Draws a cuboid with the current brush and with a random maze inside.(C) 2012 Lao Tszy",
            Usage = "/MazeCuboid [block type]",
            Handler = MazeCuboidHandler,
        };
        static void MazeCuboidHandler(Player p, Command cmd)
        {
            try
            {
                MazeCuboidDrawOperation op = new MazeCuboidDrawOperation(p);
                BuildingCommands.DrawOperationBegin(p, cmd, op);
            }
            catch (Exception e)
            {
                Logger.Log(LogType.Error, "Error: " + e.Message);
            }
        }
        #endregion


        #region ChangeModel
        static readonly CommandDescriptor CdChangeModel = new CommandDescriptor
        {
            Name = "ChangeModel",
            Aliases = new string[] { "model", "disguise" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Bring },
            Usage = "/Model [Model] [Player]",
            Help = "Change the Model of [Player]!\n" +
            "Valid models: &echicken, creeper, croc, steve, pig, sheep, skeleton, spider, zombie.",
            Handler = ModelHandler
        };
        static void ModelHandler(Player player, Command cmd)
        {
            string modelName = cmd.Next();
            string targetName = cmd.Next();
            // make sure that both parameters are given (and no extra ones)
            if (String.IsNullOrEmpty(modelName))
            {
                CdChangeModel.PrintUsage(player);
                return;
            }
            if (targetName == null)
            {
                targetName = player.Name;
            }
            PlayerInfo p = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);
            if (p == null)
            {
                return;
            }
            Block block;
            if (GetBlockName(modelName, false, out block))
            {
                // block name is given, send its numeric ID in the ChangeModel packet
                string newModel = ((int)block).ToString();
                if (targetName == player.Name)
                {
                    player.Message("Your model has changed from {0} &S to {1}", p.PlayerObject.Model, newModel);
                    p.PlayerObject.Model = newModel;
                    return;
                }
                player.Message(p.ClassyName + "&S's model has changed from {0} &S to {1}", p.PlayerObject.Model, newModel);
                p.PlayerObject.Model = newModel;
                return;
            }
            acceptedModels model;
            if (EnumUtil.TryParse(modelName, out model, true))
            {
                string newModel = model.ToString().ToLower();
                if (targetName == player.Name)
                {
                    player.Message("Your model has changed from {0} &S to {1}", p.PlayerObject.Model, newModel);
                    p.PlayerObject.Model = newModel;
                    return;
                }
                player.Message(p.ClassyName + "&S's model has changed from {0} &S to {1}", p.PlayerObject.Model, newModel);
                p.PlayerObject.Model = newModel;
                return;
            }
            else
            {
                CdChangeModel.PrintUsage(player);

            }
        }
        enum acceptedModels
        {
            chicken,
            creeper,
            croc,
            humanoid,
            pig,
            printer,
            sheep,
            skeleton,
            spider,
            zombie
        };

        static bool GetBlockName(string blockName, bool allowNoneBlock, out Block block)
        {
            if (blockName == null) throw new ArgumentNullException("blockName");
            if (Map.BlockNames.TryGetValue(blockName.ToLower(), out block))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


        #region ChangeWeather
        static readonly CommandDescriptor CdChangeWeather = new CommandDescriptor
        {
            Name = "ChangeWeather",
            Aliases = new string[] { "weather", "setweather" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Bring },
            Usage = "&1/Weather [Type] [World]",
            Help = "&1Change the weather of a world.\n" +
            "&1Valid weather: &esun, rain, snow",
            Handler = ChangeWeatherHandler
        };
        static void ChangeWeatherHandler(Player player, Command cmd)
        {
            string worldName = cmd.Next();
            string weatherName = cmd.Next();

            // make sure that both parameters are given (and no extra ones)
            if (String.IsNullOrEmpty(worldName) || String.IsNullOrEmpty(weatherName) || cmd.HasNext)
            {
                CdChangeWeather.PrintUsage(player);
                return;
            }

            // parse weather name
            WeatherType weather;
            if (!EnumUtil.TryParse(weatherName, out weather, true))
            {
                player.Message("Unrecognized weather type: {0}", weatherName);
                CdChangeWeather.PrintUsage(player);
                return;
            }

            // find world by name
            World world = WorldManager.FindWorldOrPrintMatches(player, worldName);
            if (world == null) return;

            player.Message("Changed weather of {0}&S to {1}", world.ClassyName, weather);
            world.Players.Send(Packet.EnvWeatherType((int)weather));
        }

        enum WeatherType
        {
            Sun = 0,
            Rain = 1,
            Snow = 2
        }
        #endregion



    }
}
