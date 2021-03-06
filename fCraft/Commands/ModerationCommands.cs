﻿// Copyright 2009-2014 Matvei Stefarov <me@matvei.org>

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
using System.Net;
using fCraft.Events;
using JetBrains.Annotations;

namespace fCraft
{
    /// <summary>
    ///     Most commands for server moderation - kick, ban, rank change, etc - are here.
    /// </summary>
    internal static class ModerationCommands
    {
        internal static void Init()
        {
            CdBan.Help += BanCommonHelp;
            CdBanIp.Help += BanCommonHelp;
            CdBanAll.Help += BanCommonHelp;
            CdUnban.Help += BanCommonHelp;
            CdUnbanIp.Help += BanCommonHelp;
            CdUnbanAll.Help += BanCommonHelp;

            CommandManager.RegisterCommand(CdBan);
            CommandManager.RegisterCommand(CdBanIp);
            CommandManager.RegisterCommand(CdBanAll);
            CommandManager.RegisterCommand(CdUnban);
            CommandManager.RegisterCommand(CdUnbanIp);
            CommandManager.RegisterCommand(CdUnbanAll);

            CommandManager.RegisterCommand(CdBanEx);

            CommandManager.RegisterCommand(CdKick);

            CommandManager.RegisterCommand(CdRank);

            CommandManager.RegisterCommand(CdHide);
            CommandManager.RegisterCommand(CdUnhide);

            CommandManager.RegisterCommand(CdSetSpawn);

            CommandManager.RegisterCommand(CdFreeze);
            CommandManager.RegisterCommand(CdUnfreeze);

            CommandManager.RegisterCommand(CdTp);
            CommandManager.RegisterCommand(CdBring);
            CommandManager.RegisterCommand(CdWorldBring);
            CommandManager.RegisterCommand(CdBringAll);

            CommandManager.RegisterCommand(CdPatrol);
            CommandManager.RegisterCommand(CdSpecPatrol);

            CommandManager.RegisterCommand(CdMute);
            CommandManager.RegisterCommand(CdUnmute);

            CommandManager.RegisterCommand(CdSpectate);
            CommandManager.RegisterCommand(CdUnspectate);

            CommandManager.RegisterCommand(CdSlap);
            CommandManager.RegisterCommand(CdTpZone);
            CommandManager.RegisterCommand(CdBasscannon);
            CommandManager.RegisterCommand(CdKill);
            CommandManager.RegisterCommand(CdTempBan);
            CommandManager.RegisterCommand(CdWarn);
            CommandManager.RegisterCommand(CdUnWarn);
            CommandManager.RegisterCommand(CdDisconnect);
            CommandManager.RegisterCommand(CdModerate);
            CommandManager.RegisterCommand(CdImpersonate);
            CommandManager.RegisterCommand(CdImmortal);
            CommandManager.RegisterCommand(CdTitle);

            CommandManager.RegisterCommand(CdPay);
            CommandManager.RegisterCommand(CdEconomy);
            CommandManager.RegisterCommand(CdStore);
            CommandManager.RegisterCommand(CdBalance);
            CommandManager.RegisterCommand(CdReport);
            CommandManager.RegisterCommand(CdReports);
        }

        private const string BanCommonHelp = "Ban information can be viewed with &H/BanInfo";

        public static List<string> BassText = new List<string>();

        #region Title

        private static readonly CommandDescriptor CdTitle = new CommandDescriptor
        {
            Name = "Title",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.CustomName },
            Usage = "/Title <Playername> <Title>",
            Help = "&HChanges or sets a player's title.",
            Handler = TitleHandler
        };

        private static void TitleHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            string titleName = cmd.NextAll();

            if (string.IsNullOrEmpty(targetName))
            {
                CdTitle.PrintUsage(player);
                return;
            }

            PlayerInfo info = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);
            if (info == null) return;
            string oldTitle = info.TitleName;
            if (titleName.Length == 0) titleName = null;
            if (titleName == info.TitleName)
            {
                if (titleName == null)
                {
                    player.Message("Title: Title for {0} is not set.",
                        info.Name);
                }
                else
                {
                    player.Message("Title: Title for {0} is already set to \"{1}&S\"",
                        info.Name,
                        titleName);
                }
                return;
            }
            //check the title, is it a title?
            if (titleName != null)
            {
                string stripT = Color.StripColors(titleName);
                if (!stripT.StartsWith("[") && !stripT.EndsWith("]"))
                {
                    //notify player, confirm with /ok TODO
                    //titleName = info.Rank.Color + "[" + titleName + info.Rank.Color + "]";
                }
            }
            info.TitleName = titleName;

            if (oldTitle == null)
            {
                player.Message("Title: Title for {0} set to \"{1}&S\"",
                    info.Name,
                    titleName);
                player.RefreshEntity();
            }
            else if (titleName == null)
            {
                player.Message("Title: Title for {0} was reset (was \"{1}&S\")",
                    info.Name,
                    oldTitle);
                player.RefreshEntity();
            }
            else
            {
                player.Message("Title: Title for {0} changed from \"{1}&S\" to \"{2}&S\"",
                    info.Name,
                    oldTitle,
                    titleName);
                player.RefreshEntity();
            }
        }

        #endregion

        #region Immortal

        private static readonly CommandDescriptor CdImmortal = new CommandDescriptor
        {
            Name = "Immortal",
            Aliases = new[] { "Invincible", "God" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Immortal },
            Help = "Stops death by all things.",
            NotRepeatable = true,
            Usage = "/Immortal",
            Handler = ImmortalHandler
        };

        internal static void ImmortalHandler(Player player, Command cmd)
        {
            if (player.Immortal)
            {
                player.Immortal = false;
                Server.Players.Message("{0}&S is no longer Immortal", player.ClassyName);
                return;
            }
            player.Immortal = true;
            Server.Players.Message("{0}&S is now Immortal", player.ClassyName);
        }

        #endregion

        #region Moderate

        private static readonly CommandDescriptor CdModerate = new CommandDescriptor
        {
            Name = "Moderate",
            Aliases = new[] { "MuteAll", "Moderation" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Moderation },
            Help = "Create a server-wide silence, muting all players until called again.",
            NotRepeatable = true,
            Usage = "/Moderate [Voice / Devoice] [PlayerName]",
            Handler = ModerateHandler
        };

        internal static void ModerateHandler(Player player, Command cmd)
        {
            string option = cmd.Next();
            if (option == null)
            {
                if (Server.Moderation)
                {
                    Server.Moderation = false;
                    Server.Message("{0}&W deactivated server moderation, the chat feed is enabled", player.ClassyName);
                    IRC.SendAction(player.ClassyName + " &Sdeactivated server moderation, the chat feed is enabled");
                    Server.VoicedPlayers.Clear();
                }
                else
                {
                    Server.Moderation = true;
                    Server.Message("{0}&W activated server moderation, the chat feed is disabled", player.ClassyName);
                    IRC.SendAction(player.ClassyName + " &Sactivated server moderation, the chat feed is disabled");
                    if (player.World != null)
                    {
                        //console safe
                        Server.VoicedPlayers.Add(player);
                    }
                }
            }
            else
            {
                string name = cmd.Next();
                if (option.ToLower() == "voice" && Server.Moderation)
                {
                    if (name == null)
                    {
                        player.Message("Please enter a player to Voice");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
                    if (target == null) return;
                    if (Server.VoicedPlayers.Contains(target))
                    {
                        player.Message("{0}&S is already voiced", target.ClassyName);
                        return;
                    }
                    Server.VoicedPlayers.Add(target);
                    Server.Message("{0}&S was given Voiced status by {1}", target.ClassyName, player.ClassyName);
                }
                else if (option.ToLower() == "devoice" && Server.Moderation)
                {
                    if (name == null)
                    {
                        player.Message("Please enter a player to Devoice");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
                    if (target == null) return;
                    if (!Server.VoicedPlayers.Contains(target))
                    {
                        player.Message("&WError: {0}&S does not have voiced status", target.ClassyName);
                        return;
                    }
                    Server.VoicedPlayers.Remove(target);
                    player.Message("{0}&S is no longer voiced", target.ClassyName);
                    target.Message("You are no longer voiced");
                }
                else
                {
                    player.Message("&WError: Server moderation is not activated");
                }
            }
        }

        #endregion

        #region Kill

        private static readonly CommandDescriptor CdKill = new CommandDescriptor
        {
            Name = "Kill",
            Category = CommandCategory.Moderation | CommandCategory.Fun,
            Aliases = new[] { "Slay" },
            IsConsoleSafe = false,
            Permissions = new[] { Permission.Kill },
            Help = "Kills a player.",
            NotRepeatable = true,
            Usage = "/Kill playername",
            Handler = KillHandler
        };

        internal static void KillHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            string oReason = cmd.NextAll();
            if (name == null)
            {
                player.Message("Please enter a name");
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;
            if (target.Immortal)
            {
                player.Message("&SYou failed to kill {0}&S, they are immortal", target.ClassyName);
                return;
            }

            if (target == player)
            {
                player.Message("You suicidal bro?");
                return;
            }
            double time = (DateTime.UtcNow - player.LastUsedKill).TotalSeconds;
            if (time < 10)
            {
                player.Message("&WYou can use /Kill again in " + Math.Round(10 - time) + " seconds.");
                return;
            }
            if (player.Can(Permission.Kill, target.Info.Rank))
            {
                if (player.World != null) if (player.World.Map != null) target.TeleportTo(player.World.Map.Spawn);
                player.LastUsedKill = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(oReason))
                {
                    Server.Players.CanSee(target)
                        .Union(target)
                        .Message("{0}&C was &4Killed&C by {1}&W: {2}", target.ClassyName, player.ClassyName, oReason);
                }
                else
                {
                    Server.Players.CanSee(target)
                        .Union(target)
                        .Message("{0}&C was &4Killed&C by {1}", target.ClassyName, player.ClassyName);
                }
            }
            else
            {
                player.Message("You can only Kill players ranked {0}&S or lower",
                    player.Info.Rank.GetLimit(Permission.Kill).ClassyName);
                player.Message("{0}&S is ranked {1}", target.ClassyName, target.Info.Rank.ClassyName);
            }
        }

        #endregion

        #region Slap

        private static readonly CommandDescriptor CdSlap = new CommandDescriptor
        {
            Name = "Slap",
            IsConsoleSafe = true,
            NotRepeatable = true,
            Aliases = new[] { "Sky" },
            Category = CommandCategory.Moderation | CommandCategory.Fun,
            Permissions = new[] { Permission.Slap },
            Help = "Slaps a player to the sky. " +
                   "Available items are: bakingtray, fish, bitchslap, and shoe.",
            Usage = "/Slap <playername> [item]",
            Handler = Slap
        };

        private static void Slap(Player player, Command cmd)
        {
            string name = cmd.Next();
            string item = cmd.Next();
            if (name == null)
            {
                player.Message("Please enter a name");
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;
            if (target.Immortal)
            {
                player.Message("&SYou failed to slap {0}&S, they are immortal", target.ClassyName);
                return;
            }
            if (target == player)
            {
                player.Message("&sYou can't slap yourself.... What's wrong with you???");
                return;
            }
            double time = (DateTime.UtcNow - player.LastUsedSlap).TotalSeconds;
            if (time < 10)
            {
                player.Message("&WYou can use /Slap again in " + Math.Round(10 - time) + " seconds.");
                return;
            }
            if (player.Can(Permission.Slap, target.Info.Rank))
            {
                Position slap = new Position(target.Position.X, target.Position.Y, (target.World.Map.Bounds.ZMax) * 32);
                target.TeleportTo(slap);
                if (string.IsNullOrEmpty(item))
                {
                    Server.Players.CanSee(target)
                        .Union(target)
                        .Message("{0} &Swas slapped sky high by {1}", target.ClassyName, player.ClassyName);
                    IRC.PlayerSomethingMessage(player, "slapped", target, null);
                    player.LastUsedSlap = DateTime.UtcNow;
                    return;
                }
                string aMessage;
                switch (item.ToLower())
                {
                    case "bakingtray":
                        aMessage = String.Format("{0} &Swas slapped by {1}&S with a Baking Tray", target.ClassyName,
                            player.ClassyName);
                        break;
                    case "fish":
                        aMessage = String.Format("{0} &Swas slapped by {1}&S with a Giant Fish", target.ClassyName,
                            player.ClassyName);
                        break;
                    case "bitchslap":
                        aMessage = String.Format("{0} &Swas bitch-slapped by {1}", target.ClassyName, player.ClassyName);
                        break;
                    case "shoe":
                        aMessage = String.Format("{0} &Swas slapped by {1}&S with a Shoe", target.ClassyName,
                            player.ClassyName);
                        break;
                    default:
                        Server.Players.CanSee(target)
                            .Union(target)
                            .Message("{0} &Swas slapped sky high by {1}", target.ClassyName, player.ClassyName);
                        IRC.PlayerSomethingMessage(player, "slapped", target, null);
                        player.LastUsedSlap = DateTime.UtcNow;
                        return;
                }
                Server.Players.CanSee(target).Union(target).Message(aMessage);
                IRC.PlayerSomethingMessage(player, "slapped", target, null);
                player.LastUsedSlap = DateTime.UtcNow;
            }
            else
            {
                player.Message("&sYou can only Slap players ranked {0}&S or lower",
                    player.Info.Rank.GetLimit(Permission.Slap).ClassyName);
                player.Message("{0}&S is ranked {1}", target.ClassyName, target.Info.Rank.ClassyName);
            }
        }

        #endregion

        #region TPZone

        private static readonly CommandDescriptor CdTpZone = new CommandDescriptor
        {
            Name = "Tpzone",
            IsConsoleSafe = false,
            Aliases = new[] { "tpz", "zonetp" },
            Category = CommandCategory.World | CommandCategory.Zone,
            Permissions = new[] { Permission.Teleport },
            Help = "Teleports you to the centre of a Zone listed in /Zones.",
            Usage = "/tpzone ZoneName",
            Handler = TpZone
        };

        private static void TpZone(Player player, Command cmd)
        {
            string zoneName = cmd.Next();
            if (zoneName == null)
            {
                player.Message("No zone name specified. See &W/Help tpzone");
            }
            else
            {
                if (player.World == null) return; if (player.World.Map == null) return; Zone zone = player.World.Map.Zones.Find(zoneName);
                if (zone == null)
                {
                    player.MessageNoZone(zoneName);
                    return;
                }
                Position zPos = new Position((((zone.Bounds.XMin + zone.Bounds.XMax) / 2) * 32),
                    (((zone.Bounds.YMin + zone.Bounds.YMax) / 2) * 32),
                    (((zone.Bounds.ZMin + zone.Bounds.ZMax) / 2) + 2) * 32);
                player.TeleportTo((zPos));
                player.Message("&WTeleporting you to zone " + zone.ClassyName);
            }
        }

        #endregion

        #region Impersonate

        private static readonly CommandDescriptor CdImpersonate = new CommandDescriptor
        {
            Name = "Impersonate",
            Category = CommandCategory.Moderation | CommandCategory.Fun,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.EditPlayerDB },
            Help = "&HChanges to players skin to a desired name. " +
                   "If no playername is given, all changes are reverted. " +
                   "Note: The name above your head changes too",
            Usage = "/Impersonate PlayerName",
            Handler = ImpersonateHandler
        };

        private static void ImpersonateHandler(Player player, Command cmd)
        {
            //entityChanged should be set to true for the skin update to happen in real time
            string iName = cmd.Next();
            if (iName == null && player.iName == null)
            {
                CdImpersonate.PrintUsage(player);
                return;
            }
            if (iName == null)
            {
                player.iName = null;
                player.entityChanged = true;
                player.Message("&SAll changes have been removed and your skin has been updated");
                return;
            }
            //ignore isvalidname for percent codes to work
            if (player.iName == null)
            {
                player.Message("&SYour name has changed from '" + player.Info.Rank.Color + player.Name + "&S' to '" +
                               iName);
            }
            if (player.iName != null)
            {
                player.Message("&SYour name has changed from '" + player.iName + "&S' to '" + iName);
            }
            player.iName = iName;
            player.entityChanged = true;
        }

        #endregion

        #region TempBan

        private static readonly CommandDescriptor CdTempBan = new CommandDescriptor
        {
            Name = "Tempban",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Aliases = new[] { "tban" },
            Permissions = new[] { Permission.TempBan },
            Help = "Bans a player for a selected amount of time. Example: 10s | 10 m | 10h ",
            Usage = "/Tempban Player Duration",
            Handler = Tempban
        };

        private static void Tempban(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            string timeString = cmd.Next();
            TimeSpan duration;
            try
            {
                if (String.IsNullOrEmpty(targetName) || String.IsNullOrEmpty(timeString) ||
                    !timeString.TryParseMiniTimespan(out duration) || duration <= TimeSpan.Zero)
                {
                    CdTempBan.PrintUsage(player);
                    return;
                }
            }
            catch (OverflowException)
            {
                player.Message("TempBan: Given duration is too long.");
                return;
            }

            // find the target
            PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);

            if (target == null)
                return;

            if (target.Name == player.Name)
            {
                player.Message("&WYou cannot tempban yourself");
                return;
            }
            if (target.IsBanned)
            {
                player.Message("&WPlayer is already banned");
                return;
            }
            // check permissions
            if (!player.Can(Permission.BanIP, target.Rank))
            {
                player.Message("You can only Temp-Ban players ranked {0}&S or lower.",
                    player.Info.Rank.GetLimit(Permission.TempBan).ClassyName);
                player.Message("{0}&S is ranked {1}", target.ClassyName, target.Rank.ClassyName);
                return;
            }

            // do the banning
            if (target.Tempban(player.Name, duration))
            {
                string reason = cmd.NextAll();
                try
                {
                    target.Ban(player, "You were Banned for " + timeString, false, true);
                    DateTime unbanTime = DateTime.UtcNow;
                    unbanTime = unbanTime.AddSeconds(duration.ToSeconds());
                    target.BannedUntil = unbanTime;
                    target.IsTempbanned = true;

                    Server.Message("&SPlayer {0}&S was Banned by {1}&S for {2}",
                        target.ClassyName, player.ClassyName, duration.ToMiniString());

                    if (reason.Length > 0) Server.Message("&Wreason: {0}", reason);
                    Logger.Log(LogType.UserActivity, "Player {0} was Banned by {1} for {2}",
                        target.Name, player.Name, duration.ToMiniString());
                }
                catch (PlayerOpException ex)
                {
                    player.Message(ex.MessageColored);
                }
            }
            else
            {
                player.Message("Player {0}&S is already Banned by {1}&S for {2:0} more.",
                    target.ClassyName,
                    target.BannedBy,
                    target.BannedUntil.Subtract(DateTime.UtcNow).ToMiniString());
            }
        }

        #endregion

        #region Basscannon

        private static readonly CommandDescriptor CdBasscannon = new CommandDescriptor
        {
            Name = "Basscannon",
            Category = CommandCategory.Moderation | CommandCategory.Fun,
            IsConsoleSafe = true,
            Aliases = new[] { "bc" },
            IsHidden = false,
            Permissions = new[] { Permission.Basscannon },
            Usage = "Let the Basscannon 'Kick' it!",
            Help = "A classy way to kick players from the server",
            Handler = Basscannon
        };

        internal static void Basscannon(Player player, Command cmd)
        {
            string name = cmd.Next();
            string reason = cmd.NextAll();

            if (name == null)
            {
                player.Message("Please enter a player name to use the basscannon on.");
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;

            if (ConfigKey.RequireKickReason.Enabled() && String.IsNullOrEmpty(reason))
            {
                player.Message("&WPlease specify a reason: &W/Basscannon PlayerName Reason");
                // freeze the target player to prevent further damage
                return;
            }

            if (player.Can(Permission.Kick, target.Info.Rank))
            {
                target.Info.IsHidden = false;
                try
                {
                    target.BassKick(player, reason, LeaveReason.Kick, true, true, true);
                    if (BassText.Count < 1)
                    {
                        BassText.Add("Flux Pavillion does not approve of your behavior");
                        BassText.Add("Let the Basscannon KICK IT!");
                        BassText.Add("WUB WUB WUB WUB WUB WUB!");
                        BassText.Add("Basscannon, Basscannon, Basscannon, Basscannon!");
                        BassText.Add("Pow pow POW!!!");
                    }
                    string line = BassText[new Random().Next(0, BassText.Count)].Trim();
                    if (line.Length == 0) return;
                    Server.Message("&9{0}", line);
                }
                catch (PlayerOpException ex)
                {
                    player.Message(ex.MessageColored);
                    if (ex.ErrorCode == PlayerOpExceptionCode.ReasonRequired)
                        return;
                }
            }
            else
            {
                player.Message("You can only use /Basscannon on players ranked {0}&S or lower",
                    player.Info.Rank.GetLimit(Permission.Kick).ClassyName);
                player.Message("{0}&S is ranked {1}", target.ClassyName, target.Info.Rank.ClassyName);
            }
        }

        #endregion

        #region Warn

        private static readonly CommandDescriptor CdWarn = new CommandDescriptor
        {
            Name = "Warn",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            NotRepeatable = true,
            Permissions = new[] { Permission.Warn },
            Help =
                "&HWarns a player and puts a black star next to their name for 20 minutes. During the 20 minutes, if they are warned again, they will get kicked.",
            Usage = "/Warn playername",
            Handler = Warn
        };

        internal static void Warn(Player player, Command cmd)
        {
            string name = cmd.Next();

            if (name == null)
            {
                player.Message("No player specified.");
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null)
                return;

            if (player.Can(Permission.Warn, target.Info.Rank))
            {
                target.Info.IsHidden = false;
                if (target.Info.Warn(player.Name))
                {
                    Server.Message("{0}&S has been warned by {1}",
                        target.ClassyName, player.ClassyName);
                    Scheduler.NewTask(t => target.Info.UnWarn()).RunOnce(TimeSpan.FromMinutes(15));
                }
                else
                {
                    try
                    {
                        target.Kick(player, "Auto Kick (2 warnings or more)", LeaveReason.Kick, true, true, true);
                    }
                    catch (PlayerOpException ex)
                    {
                        player.Message(ex.MessageColored);
                        if (ex.ErrorCode == PlayerOpExceptionCode.ReasonRequired)
                            return;
                    }
                }
            }
            else
            {
                player.Message("You can only warn players ranked {0}&S or lower",
                    player.Info.Rank.GetLimit(Permission.Warn).ClassyName);
                player.Message("{0}&S is ranked {1}", target.ClassyName, target.Info.Rank.ClassyName);
            }
        }

        #endregion

        #region Unwarn

        private static readonly CommandDescriptor CdUnWarn = new CommandDescriptor
        {
            Name = "Unwarn",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Warn },
            Usage = "/Unwarn PlayerName",
            Help = "&HUnwarns a player",
            Handler = UnWarn
        };

        internal static void UnWarn(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name == null)
            {
                player.Message("No player specified.");
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);

            if (target == null)
                return;

            if (player.Can(Permission.Warn, target.Info.Rank))
            {
                if (target.Info.UnWarn())
                {
                    Server.Message("{0}&S had their warning removed by {1}.", target.ClassyName, player.ClassyName);
                }
                else
                {
                    player.Message("{0}&S does not have a warning.", target.ClassyName);
                }
            }
            else
            {
                player.Message("You can only unwarn players ranked {0}&S or lower",
                    player.Info.Rank.GetLimit(Permission.Warn).ClassyName);
                player.Message("{0}&S is ranked {1}", target.ClassyName, target.Info.Rank.ClassyName);
            }
        }

        #endregion

        #region Disconnect

        private static readonly CommandDescriptor CdDisconnect = new CommandDescriptor
        {
            Name = "Disconnect",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Aliases = new[] { "gtfo" },
            IsHidden = false,
            Permissions = new[] { Permission.Gtfo },
            Usage = "/disconnect playername",
            Help = "Get rid of those annoying people without saving to PlayerDB",
            Handler = DisconnectHandler
        };

        internal static void DisconnectHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name == null)
            {
                player.Message("Please enter a name");
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;

            if (player.Can(Permission.Gtfo, target.Info.Rank))
            {
                try
                {
                    target.Kick(player, "Manually disconnected by " + player.Name, LeaveReason.Kick, false, true, false);
                    Server.Players.Message("{0} &Swas manually disconnected by {1}", target.ClassyName,
                        player.ClassyName);
                }
                catch (PlayerOpException ex)
                {
                    player.Message(ex.MessageColored);
                    if (ex.ErrorCode == PlayerOpExceptionCode.ReasonRequired)
                        return;
                }
            }
            else
            {
                player.Message("You can only Disconnect players ranked {0}&S or lower",
                    player.Info.Rank.GetLimit(Permission.Gtfo).ClassyName);
                player.Message("{0}&S is ranked {1}", target.ClassyName, target.Info.Rank.ClassyName);
            }
        }

        #endregion

        #region Ban / Unban

        private static readonly CommandDescriptor CdBan = new CommandDescriptor
        {
            Name = "Ban",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Ban },
            Usage = "/Ban PlayerName [Reason]",
            Help = "&HBans a specified player by name. Note: Does NOT ban IP. " +
                   "Any text after the player name will be saved as a memo. ",
            Handler = BanHandler
        };


        private static readonly CommandDescriptor CdBanIp = new CommandDescriptor
        {
            Name = "BanIP",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Ban, Permission.BanIP },
            Usage = "/BanIP PlayerName|IPAddress [Reason]",
            Help =
                "&HBans the player's name and IP. If player is not online, last known IP associated with the name is used. " +
                "You can also type in the IP address directly. " +
                "Any text after PlayerName/IP will be saved as a memo. ",
            Handler = BanIPHandler
        };


        private static readonly CommandDescriptor CdBanAll = new CommandDescriptor
        {
            Name = "BanAll",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Ban, Permission.BanIP, Permission.BanAll },
            Usage = "/BanAll PlayerName|IPAddress [Reason]",
            Help = "&HBans the player's name, IP, and all other names associated with the IP. " +
                   "If player is not online, last known IP associated with the name is used. " +
                   "You can also type in the IP address directly. " +
                   "Any text after PlayerName/IP will be saved as a memo. ",
            Handler = BanAllHandler
        };


        private static readonly CommandDescriptor CdUnban = new CommandDescriptor
        {
            Name = "Unban",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Ban },
            Usage = "/Unban PlayerName [Reason]",
            Help = "&HRemoves ban for a specified player. Does NOT remove associated IP bans. " +
                   "Any text after the player name will be saved as a memo. ",
            Handler = UnbanHandler
        };


        private static readonly CommandDescriptor CdUnbanIp = new CommandDescriptor
        {
            Name = "UnbanIP",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Ban, Permission.BanIP },
            Usage = "/UnbanIP PlayerName|IPaddress [Reason]",
            Help = "&HRemoves ban for a specified player's name and last known IP. " +
                   "You can also type in the IP address directly. " +
                   "Any text after the player name will be saved as a memo. ",
            Handler = UnbanIPHandler
        };


        private static readonly CommandDescriptor CdUnbanAll = new CommandDescriptor
        {
            Name = "UnbanAll",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Ban, Permission.BanIP, Permission.BanAll },
            Usage = "/UnbanAll PlayerName|IPaddress [Reason]",
            Help =
                "&HRemoves ban for a specified player's name, last known IP, and all other names associated with the IP. " +
                "You can also type in the IP address directly. " +
                "Any text after the player name will be saved as a memo. ",
            Handler = UnbanAllHandler
        };


        private static readonly CommandDescriptor CdBanEx = new CommandDescriptor
        {
            Name = "BanEx",
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Ban, Permission.BanIP },
            Usage = "/BanEx +PlayerName&S or &H/BanEx -PlayerName",
            Help = "&HAdds or removes an IP-ban exemption for an account. " +
                   "Exempt accounts can log in from any IP, including banned ones.",
            Handler = BanExHandler
        };

        private static void BanHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdBan.PrintUsage(player);
                return;
            }
            PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);
            Player reportTarget = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null) return;
            string reason = cmd.NextAll();
            try
            {
                if (Reports.Contains(reportTarget))
                {
                    Player targetPlayer = target.PlayerObject;
                    target.Ban(player, reason, true, true);
                    WarnIfOtherPlayersOnIp(player, target, targetPlayer);
                    Reports.Remove(reportTarget);
                }
                else
                {
                    Player targetPlayer = target.PlayerObject;
                    target.Ban(player, reason, true, true);
                    WarnIfOtherPlayersOnIp(player, target, targetPlayer);
                }
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
                if (ex.ErrorCode == PlayerOpExceptionCode.ReasonRequired)
                {
                    FreezeIfAllowed(player, target);
                }
            }
        }

        private static void BanIPHandler(Player player, Command cmd)
        {
            string targetNameOrIp = cmd.Next();
            if (targetNameOrIp == null)
            {
                CdBanIp.PrintUsage(player);
                return;
            }
            string reason = cmd.NextAll();

            IPAddress targetAddress;
            if (Server.IsIP(targetNameOrIp) && IPAddress.TryParse(targetNameOrIp, out targetAddress))
            {
                try
                {
                    targetAddress.BanIP(player, reason, true, true);
                }
                catch (PlayerOpException ex)
                {
                    player.Message(ex.MessageColored);
                }
            }
            else
            {
                PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetNameOrIp);
                if (target == null) return;
                try
                {
                    if (target.LastIP.Equals(IPAddress.Any) || target.LastIP.Equals(IPAddress.None))
                    {
                        target.Ban(player, reason, true, true);
                    }
                    else
                    {
                        target.BanIP(player, reason, true, true);
                    }
                }
                catch (PlayerOpException ex)
                {
                    player.Message(ex.MessageColored);
                    if (ex.ErrorCode == PlayerOpExceptionCode.ReasonRequired)
                    {
                        FreezeIfAllowed(player, target);
                    }
                }
            }
        }

        private static void BanAllHandler(Player player, Command cmd)
        {
            string targetNameOrIp = cmd.Next();
            if (targetNameOrIp == null)
            {
                CdBanAll.PrintUsage(player);
                return;
            }
            string reason = cmd.NextAll();

            IPAddress targetAddress;
            if (Server.IsIP(targetNameOrIp) && IPAddress.TryParse(targetNameOrIp, out targetAddress))
            {
                try
                {
                    targetAddress.BanAll(player, reason, true, true);
                }
                catch (PlayerOpException ex)
                {
                    player.Message(ex.MessageColored);
                }
            }
            else
            {
                PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetNameOrIp);
                if (target == null) return;
                try
                {
                    if (target.LastIP.Equals(IPAddress.Any) || target.LastIP.Equals(IPAddress.None))
                    {
                        target.Ban(player, reason, true, true);
                    }
                    else
                    {
                        target.BanAll(player, reason, true, true);
                    }
                }
                catch (PlayerOpException ex)
                {
                    player.Message(ex.MessageColored);
                    if (ex.ErrorCode == PlayerOpExceptionCode.ReasonRequired)
                    {
                        FreezeIfAllowed(player, target);
                    }
                }
            }
        }

        private static void UnbanHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdUnban.PrintUsage(player);
                return;
            }
            PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);
            if (target == null) return;
            string reason = cmd.NextAll();
            try
            {
                target.Unban(player, reason, true, true);
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        private static void UnbanIPHandler(Player player, Command cmd)
        {
            string targetNameOrIp = cmd.Next();
            if (targetNameOrIp == null)
            {
                CdUnbanIp.PrintUsage(player);
                return;
            }
            string reason = cmd.NextAll();

            try
            {
                IPAddress targetAddress;
                if (Server.IsIP(targetNameOrIp) && IPAddress.TryParse(targetNameOrIp, out targetAddress))
                {
                    targetAddress.UnbanIP(player, reason, true, true);
                }
                else
                {
                    PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetNameOrIp);
                    if (target == null) return;
                    if (target.LastIP.Equals(IPAddress.Any) || target.LastIP.Equals(IPAddress.None))
                    {
                        target.Unban(player, reason, true, true);
                    }
                    else
                    {
                        target.UnbanIP(player, reason, true, true);
                    }
                }
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        private static void UnbanAllHandler(Player player, Command cmd)
        {
            string targetNameOrIp = cmd.Next();
            if (targetNameOrIp == null)
            {
                CdUnbanAll.PrintUsage(player);
                return;
            }
            string reason = cmd.NextAll();

            try
            {
                IPAddress targetAddress;
                if (Server.IsIP(targetNameOrIp) && IPAddress.TryParse(targetNameOrIp, out targetAddress))
                {
                    targetAddress.UnbanAll(player, reason, true, true);
                }
                else
                {
                    PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetNameOrIp);
                    if (target == null) return;
                    if (target.LastIP.Equals(IPAddress.Any) || target.LastIP.Equals(IPAddress.None))
                    {
                        target.Unban(player, reason, true, true);
                    }
                    else
                    {
                        target.UnbanAll(player, reason, true, true);
                    }
                }
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        private static void BanExHandler(Player player, Command cmd)
        {
            string playerName = cmd.Next();
            if (playerName == null || playerName.Length < 2 || (playerName[0] != '-' && playerName[0] != '+'))
            {
                CdBanEx.PrintUsage(player);
                return;
            }
            bool addExemption = (playerName[0] == '+');
            string targetName = playerName.Substring(1);
            PlayerInfo target = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);
            if (target == null) return;

            switch (target.BanStatus)
            {
                case BanStatus.Banned:
                    player.Message(
                        addExemption
                            ? "Player {0}&S is currently banned. Unban before adding an exemption."
                            : "Player {0}&S is already banned. There is no exemption to remove.",
                        target.ClassyName);
                    break;
                case BanStatus.IPBanExempt:
                    if (addExemption)
                    {
                        player.Message("IP-Ban exemption already exists for player {0}", target.ClassyName);
                    }
                    else
                    {
                        player.Message("IP-Ban exemption removed for player {0}",
                            target.ClassyName);
                        target.BanStatus = BanStatus.NotBanned;
                    }
                    break;
                case BanStatus.NotBanned:
                    if (addExemption)
                    {
                        player.Message("IP-Ban exemption added for player {0}",
                            target.ClassyName);
                        target.BanStatus = BanStatus.IPBanExempt;
                    }
                    else
                    {
                        player.Message("No IP-Ban exemption exists for player {0}",
                            target.ClassyName);
                    }
                    break;
            }
        }

        #endregion

        #region Kick

        private static readonly CommandDescriptor CdKick = new CommandDescriptor
        {
            Name = "Kick",
            Aliases = new[] { "k" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Kick },
            Usage = "/Kick PlayerName [Reason]",
            Help = "Kicks the specified player from the server. " +
                   "Optional kick reason/message is shown to the kicked player and logged.",
            Handler = KickHandler
        };

        private static void KickHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name == null)
            {
                player.Message("Usage: &H/Kick PlayerName [Message]");
                return;
            }

            // find the target
            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;

            string reason = cmd.NextAll();
            DateTime previousKickDate = target.Info.LastKickDate;
            string previousKickedBy = target.Info.LastKickByClassy;
            string previousKickReason = target.Info.LastKickReason;

            // do the kick
            try
            {
                Player targetPlayer = target;
                target.Kick(player, reason, LeaveReason.Kick, true, true, true);
                WarnIfOtherPlayersOnIp(player, target.Info, targetPlayer);
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
                if (ex.ErrorCode == PlayerOpExceptionCode.ReasonRequired)
                {
                    FreezeIfAllowed(player, target.Info);
                }
                return;
            }

            // warn player if target has been kicked before
            if (target.Info.TimesKicked > 1)
            {
                player.Message("Warning: {0}&S has been kicked {1} times before.",
                    target.ClassyName, target.Info.TimesKicked - 1);
                if (previousKickDate != DateTime.MinValue)
                {
                    player.Message("Most recent kick was {0} ago, by {1}",
                        DateTime.UtcNow.Subtract(previousKickDate).ToMiniString(),
                        previousKickedBy);
                }
                if (!String.IsNullOrEmpty(previousKickReason))
                {
                    player.Message("Most recent kick reason was: {0}",
                        previousKickReason);
                }
            }
        }

        #endregion

        #region Changing Rank (Promotion / Demotion)

        private static readonly CommandDescriptor CdRank = new CommandDescriptor
        {
            Name = "Rank",
            Aliases = new[] { "user", "promote", "demote" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Promote, Permission.Demote },
            AnyPermission = true,
            IsConsoleSafe = true,
            Usage = "/Rank PlayerName RankName [Reason]",
            Help = "Changes the rank of a player to a specified rank. " +
                   "Any text specified after the RankName will be saved as a memo.",
            Handler = RankHandler
        };

        public static void RankHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            string newRankName = cmd.Next();

            // Check arguments
            if (name == null || newRankName == null)
            {
                CdRank.PrintUsage(player);
                player.Message("See &H/Ranks&S for list of ranks.");
                return;
            }

            // Parse rank name
            Rank newRank = RankManager.FindRank(newRankName);
            if (newRank == null)
            {
                player.MessageNoRank(newRankName);
                return;
            }

            // Parse player name
            if (name == "-")
            {
                if (player.LastUsedPlayerName != null)
                {
                    name = player.LastUsedPlayerName;
                }
                else
                {
                    player.Message("Cannot repeat player name: you haven't used any names yet.");
                    return;
                }
            }
            PlayerInfo targetInfo = PlayerDB.FindPlayerInfoExact(name);

            if (targetInfo == null)
            {
                if (!player.Can(Permission.EditPlayerDB))
                {
                    player.MessageNoPlayer(name);
                    return;
                }
                if (!Player.IsValidName(name))
                {
                    player.MessageInvalidPlayerName(name);
                    CdRank.PrintUsage(player);
                    return;
                }
                if (cmd.IsConfirmed)
                {
                    targetInfo = PlayerDB.AddFakeEntry(name, newRank > RankManager.DefaultRank ? RankChangeType.Promoted : RankChangeType.Demoted);
                }
                else
                {
                    player.Confirm(cmd,
                        "Warning: Player \"{0}\" is not in the database (possible typo). Type the full name or",
                        name);
                    return;
                }
            }

            try
            {
                player.LastUsedPlayerName = targetInfo.Name;
                targetInfo.ChangeRank(player, newRank, cmd.NextAll(), true, true, false);
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        #endregion

        #region Hide

        private static readonly CommandDescriptor CdHide = new CommandDescriptor
        {
            Name = "Hide",
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Hide },
            Usage = "/Hide [silent]",
            Help = "&HEnables invisible mode. It looks to other players like you left the server, " +
                   "but you can still do anything - chat, build, delete, type commands - as usual. " +
                   "Great way to spy on griefers and scare newbies. " +
                   "Call &H/Unhide&S to reveal yourself.",
            Handler = HideHandler
        };


        private static readonly CommandDescriptor CdUnhide = new CommandDescriptor
        {
            Name = "Unhide",
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Hide },
            Usage = "/Unhide [silent]",
            Help = "&HDisables the &H/Hide&S invisible mode. " +
                   "It looks to other players like you just joined the server.",
            Handler = UnhideHandler
        };

        private static void HideHandler(Player player, Command cmd)
        {
            if (player.Info.IsHidden)
            {
                player.Message("You are already hidden.");
                return;
            }

            string silentString = cmd.Next();
            bool silent = false;
            if (silentString != null)
            {
                silent = silentString.Equals("silent", StringComparison.OrdinalIgnoreCase);
            }

            player.Info.IsHidden = true;
            player.Message("&8You are now hidden.");

            // to make it look like player just logged out in /Info
            player.Info.LastSeen = DateTime.UtcNow;

            if (!silent)
            {
                if (ConfigKey.ShowConnectionMessages.Enabled())
                {
                    Server.Players.CantSee(player).Message("&SPlayer {0}&S left the server.", player.ClassyName);
                }
                if (ConfigKey.IRCBotAnnounceServerJoins.Enabled())
                {
                    IRC.PlayerDisconnectedHandler(null,
                        new PlayerDisconnectedEventArgs(player, LeaveReason.ClientQuit, true));
                }
            }

            // for aware players: notify
            Server.Players.CanSee(player).Message("&SPlayer {0}&S is now hidden.", player.ClassyName);

            Player.RaisePlayerHideChangedEvent(player);
        }

        private static void UnhideHandler(Player player, Command cmd)
        {
            if (player.World == null) PlayerOpException.ThrowNoWorld(player);

            if (!player.Info.IsHidden)
            {
                player.Message("You are not currently hidden.");
                return;
            }

            bool silent = cmd.HasNext;

            // for aware players: notify
            Server.Players.CanSee(player).Message("&SPlayer {0}&S is no longer hidden.",
                player.ClassyName);
            player.Message("&8You are no longer hidden.");
            player.Info.IsHidden = false;
            if (!silent)
            {
                if (ConfigKey.ShowConnectionMessages.Enabled())
                {
                    // ReSharper disable AssignNullToNotNullAttribute
                    string msg = Server.MakePlayerConnectedMessage(player, false, player.World);
                    // ReSharper restore AssignNullToNotNullAttribute
                    Server.Players.CantSee(player).Message(msg);
                }
                if (ConfigKey.IRCBotAnnounceServerJoins.Enabled())
                {
                    IRC.PlayerReadyHandler(null, new PlayerConnectedEventArgs(player, player.World));
                }
            }

            Player.RaisePlayerHideChangedEvent(player);
        }

        #endregion

        #region Set Spawn

        private static readonly CommandDescriptor CdSetSpawn = new CommandDescriptor
        {
            Name = "SetSpawn",
            Category = CommandCategory.Moderation | CommandCategory.World,
            Permissions = new[] { Permission.SetSpawn },
            Help = "&HAssigns your current location to be the spawn point of the map/world. " +
                   "If an optional PlayerName param is given, the spawn point of only that player is changed instead.",
            Usage = "/SetSpawn [PlayerName]",
            Handler = SetSpawnHandler
        };

        public static void SetSpawnHandler(Player player, Command cmd)
        {
            World playerWorld = player.World;
            if (playerWorld == null) PlayerOpException.ThrowNoWorld(player);


            string playerName = cmd.Next();
            if (playerName == null)
            {
                Map map = player.WorldMap;
                map.Spawn = player.Position;
                player.TeleportTo(map.Spawn);
                player.Send(PacketWriter.MakeAddEntity(255, player.ListName, player.Position));
                player.Message("New spawn point saved.");
                Logger.Log(LogType.UserActivity,
                    "{0} changed the spawned point.",
                    player.Name);
            }
            else if (player.Can(Permission.Bring))
            {
                if (playerWorld == null) return;
                Player[] infos = playerWorld.FindPlayers(player, playerName);
                if (infos.Length == 1)
                {
                    Player target = infos[0];
                    player.LastUsedPlayerName = target.Name;
                    if (player.Can(Permission.Bring, target.Info.Rank))
                    {
                        target.Send(PacketWriter.MakeAddEntity(255, target.ListName, player.Position));
                    }
                    else
                    {
                        player.Message("You may only set spawn of players ranked {0}&S or lower.",
                            player.Info.Rank.GetLimit(Permission.Bring).ClassyName);
                        player.Message("{0}&S is ranked {1}", target.ClassyName, target.Info.Rank.ClassyName);
                    }
                }
                else if (infos.Length > 0)
                {
                    player.MessageManyMatches("player", infos);
                }
                else
                {
                    infos = Server.FindPlayers(player, playerName, true);
                    if (infos.Length > 0)
                    {
                        player.Message("You may only set spawn of players on the same world as you.");
                    }
                    else
                    {
                        player.MessageNoPlayer(playerName);
                    }
                }
            }
            else
            {
                player.MessageNoAccess(CdSetSpawn);
            }
        }

        #endregion

        #region Freeze

        private static readonly CommandDescriptor CdFreeze = new CommandDescriptor
        {
            Name = "Freeze",
            Aliases = new[] { "f" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Freeze },
            Usage = "/Freeze PlayerName",
            Help = "Freezes the specified player in place. " +
                   "This is usually effective, but not hacking-proof. " +
                   "To release the player, use &H/unfreeze PlayerName",
            Handler = FreezeHandler
        };


        private static readonly CommandDescriptor CdUnfreeze = new CommandDescriptor
        {
            Name = "Unfreeze",
            Aliases = new[] { "uf" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Freeze },
            Usage = "/Unfreeze PlayerName",
            Help = "Releases the player from a frozen state. See &H/Help Freeze&S for more information.",
            Handler = UnfreezeHandler
        };

        private static void FreezeHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name == null)
            {
                CdFreeze.PrintUsage(player);
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;

            try
            {
                target.Info.Freeze(player, true, true);
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        private static void UnfreezeHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name == null)
            {
                CdFreeze.PrintUsage(player);
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;

            try
            {
                target.Info.Unfreeze(player, true, true);
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        #endregion

        #region TP

        private static readonly CommandDescriptor CdTp = new CommandDescriptor
        {
            Name = "TP",
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Teleport },
            Usage = "/TP PlayerName&S or &H/TP X Y Z",
            Help = "&HTeleports you to a specified player's location. " +
                   "If coordinates are given, teleports to that location.",
            Handler = TPHandler
        };

        private static void TPHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name == null)
            {
                CdTp.PrintUsage(player);
                return;
            }

            if (cmd.Next() != null)
            {
                cmd.Rewind();
                int x, y, z;
                if (cmd.NextInt(out x) && cmd.NextInt(out y) && cmd.NextInt(out z))
                {
                    if (x <= -1024 || x >= 1024 || y <= -1024 || y >= 1024 || z <= -1024 || z >= 1024)
                    {
                        player.Message("Coordinates are outside the valid range!");
                    }
                    else
                    {
                        player.TeleportTo(new Position
                        {
                            X = (short)(x * 32 + 16),
                            Y = (short)(y * 32 + 16),
                            Z = (short)(z * 32 + 16),
                            R = player.Position.R,
                            L = player.Position.L
                        });
                    }
                }
                else
                {
                    CdTp.PrintUsage(player);
                }
            }
            else
            {
                if (name == "-")
                {
                    if (player.LastUsedPlayerName != null)
                    {
                        name = player.LastUsedPlayerName;
                    }
                    else
                    {
                        player.Message("Cannot repeat player name: you haven't used any names yet.");
                        return;
                    }
                }
                Player[] matches = Server.FindPlayers(player, name, true);
                if (matches.Length == 1)
                {
                    Player target = matches[0];
                    World targetWorld = target.World;
                    if (targetWorld == null) PlayerOpException.ThrowNoWorld(target);

                    if (targetWorld == player.World)
                    {
                        player.TeleportTo(target.Position);
                    }
                    else
                    {
                        if (targetWorld == null) return;
                        switch (targetWorld.AccessSecurity.CheckDetailed(player.Info))
                        {
                            case SecurityCheckResult.Allowed:
                            case SecurityCheckResult.WhiteListed:
                                if (targetWorld.IsFull)
                                {
                                    player.Message("Cannot teleport to {0}&S because world {1}&S is full.",
                                        target.ClassyName,
                                        targetWorld.ClassyName);
                                    return;
                                }
                                player.StopSpectating();
                                player.JoinWorld(targetWorld, WorldChangeReason.Tp, target.Position);
                                break;
                            case SecurityCheckResult.BlackListed:
                                player.Message("Cannot teleport to {0}&S because you are blacklisted on world {1}",
                                    target.ClassyName,
                                    targetWorld.ClassyName);
                                break;
                            case SecurityCheckResult.RankTooLow:
                                player.Message("Cannot teleport to {0}&S because world {1}&S requires {2}+&S to join.",
                                    target.ClassyName,
                                    targetWorld.ClassyName,
                                    targetWorld.AccessSecurity.MinRank.ClassyName);
                                break;
                            // TODO: case PermissionType.RankTooHigh:
                        }
                    }
                }
                else if (matches.Length > 1)
                {
                    player.MessageManyMatches("player", matches);
                }
                else
                {
                    // Try to guess if player typed "/TP" instead of "/Join"
                    World[] worlds = WorldManager.FindWorlds(player, name);

                    if (worlds.Length == 1)
                    {
                        player.LastUsedWorldName = worlds[0].Name;
                        player.StopSpectating();
                        player.ParseMessage("/Join " + worlds[0].Name, false);
                    }
                    else
                    {
                        player.MessageNoPlayer(name);
                    }
                }
            }
        }

        #endregion

        #region Bring / WorldBring / BringAll

        private static readonly CommandDescriptor CdBring = new CommandDescriptor
        {
            Name = "Bring",
            IsConsoleSafe = true,
            Aliases = new[] { "summon", "fetch" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Bring },
            Usage = "/Bring PlayerName [ToPlayer]",
            Help = "Teleports another player to your location. " +
                   "If the optional second parameter is given, teleports player to another player.",
            Handler = BringHandler
        };


        private static readonly CommandDescriptor CdWorldBring = new CommandDescriptor
        {
            Name = "WBring",
            IsConsoleSafe = true,
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Bring },
            Usage = "/WBring PlayerName WorldName",
            Help = "&HTeleports a player to the given world's spawn.",
            Handler = WorldBringHandler
        };


        private static readonly CommandDescriptor CdBringAll = new CommandDescriptor
        {
            Name = "BringAll",
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Bring, Permission.BringAll },
            Usage = "/BringAll [@Rank [@AnotherRank]] [*|World [AnotherWorld]]",
            Help = "&HTeleports all players from your world to you. " +
                   "If any world names are given, only teleports players from those worlds. " +
                   "If any rank names are given, only teleports players of those ranks.",
            Handler = BringAllHandler
        };

        private static void BringHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name == null)
            {
                CdBring.PrintUsage(player);
                return;
            }

            // bringing someone to another player (instead of to self)
            string toName = cmd.Next();
            Player toPlayer = player;
            if (toName != null)
            {
                toPlayer = Server.FindPlayerOrPrintMatches(player, toName, false, true);
                if (toPlayer == null) return;
            }
            else if (toPlayer.World == null)
            {
                player.Message("When used from console, /Bring requires both names to be given.");
                return;
            }

            World world = toPlayer.World;
            if (world == null) PlayerOpException.ThrowNoWorld(toPlayer);

            Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
            if (target == null) return;

            if (target == player)
            {
                player.Message("&WYou cannot bring yourself!");
                return;
            }


            if (!player.Can(Permission.Bring, target.Info.Rank))
            {
                player.Message("You may only bring players ranked {0}&S or lower.",
                    player.Info.Rank.GetLimit(Permission.Bring).ClassyName);
                player.Message("{0}&S is ranked {1}",
                    target.ClassyName, target.Info.Rank.ClassyName);
                return;
            }

            if (target.World == world)
            {
                // teleport within the same world
                target.TeleportTo(toPlayer.Position);
                target.Message("&8You were summoned by {0}", player.ClassyName);
            }
            else
            {
                // teleport to a different world
                if (world == null) return;
                SecurityCheckResult check = world.AccessSecurity.CheckDetailed(target.Info);
                if (check == SecurityCheckResult.RankTooHigh || check == SecurityCheckResult.RankTooLow)
                {
                    if (player.CanJoin(world))
                    {
                        if (cmd.IsConfirmed)
                        {
                            BringPlayerToWorld(player, target, world, true, true);
                        }
                        else
                        {
                            player.Confirm(cmd,
                                "Player {0}&S is ranked too low to join {1}&S. Override world permissions?",
                                target.ClassyName,
                                world.ClassyName);
                        }
                    }
                    else
                    {
                        player.Message("Neither you nor {0}&S are allowed to join world {1}",
                            target.ClassyName, world.ClassyName);
                    }
                }
                else
                {
                    BringPlayerToWorld(player, target, world, false, true);
                    target.Message("&8You were summoned by {0}", player.ClassyName);
                }
            }
        }

        private static void WorldBringHandler(Player player, Command cmd)
        {
            string playerName = cmd.Next();
            string worldName = cmd.Next();
            if (playerName == null || worldName == null)
            {
                CdWorldBring.PrintUsage(player);
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, playerName, false, true);
            World world = WorldManager.FindWorldOrPrintMatches(player, worldName);

            if (target == null || world == null) return;

            if (target == player)
            {
                player.Message("&WYou cannot &H/WBring&W yourself.");
                return;
            }

            if (!player.Can(Permission.Bring, target.Info.Rank))
            {
                player.Message("You may only bring players ranked {0}&S or lower.",
                    player.Info.Rank.GetLimit(Permission.Bring).ClassyName);
                player.Message("{0}&S is ranked {1}",
                    target.ClassyName, target.Info.Rank.ClassyName);
                return;
            }

            if (world == target.World)
            {
                player.Message("Player {0}&S is already in world {1}&S. They were brought to spawn.",
                    target.ClassyName, world.ClassyName);
                target.TeleportTo(target.WorldMap.Spawn);
                return;
            }

            SecurityCheckResult check = world.AccessSecurity.CheckDetailed(target.Info);
            if (check == SecurityCheckResult.RankTooHigh || check == SecurityCheckResult.RankTooLow)
            {
                if (player.CanJoin(world))
                {
                    if (cmd.IsConfirmed)
                    {
                        BringPlayerToWorld(player, target, world, true, false);
                    }
                    else
                    {
                        player.Confirm(cmd,
                            "Player {0}&S is ranked too low to join {1}&S. Override world permissions?",
                            target.ClassyName,
                            world.ClassyName);
                    }
                }
                else
                {
                    player.Message("Neither you nor {0}&S are allowed to join world {1}",
                        target.ClassyName, world.ClassyName);
                }
            }
            else
            {
                BringPlayerToWorld(player, target, world, false, false);
            }
        }

        private static void BringAllHandler(Player player, Command cmd)
        {
            if (player.World == null) PlayerOpException.ThrowNoWorld(player);

            List<World> targetWorlds = new List<World>();
            List<Rank> targetRanks = new List<Rank>();
            bool allWorlds = false;
            bool allRanks = true;

            // Parse the list of worlds and ranks
            string arg;
            while ((arg = cmd.Next()) != null)
            {
                if (arg.StartsWith("@"))
                {
                    Rank rank = RankManager.FindRank(arg.Substring(1));
                    if (rank == null)
                    {
                        player.Message("Unknown rank: {0}", arg.Substring(1));
                        return;
                    }
                    if (player.Can(Permission.Bring, rank))
                    {
                        targetRanks.Add(rank);
                    }
                    else
                    {
                        player.Message("&WYou are not allowed to bring players of rank {0}",
                            rank.ClassyName);
                    }
                    allRanks = false;
                }
                else if (arg == "*")
                {
                    allWorlds = true;
                }
                else
                {
                    World world = WorldManager.FindWorldOrPrintMatches(player, arg);
                    if (world == null) return;
                    targetWorlds.Add(world);
                }
            }

            // If no worlds were specified, use player's current world
            if (!allWorlds && targetWorlds.Count == 0)
            {
                targetWorlds.Add(player.World);
            }

            // Apply all the rank and world options
            HashSet<Player> targetPlayers;
            if (allRanks && allWorlds)
            {
                targetPlayers = new HashSet<Player>(Server.Players);
            }
            else if (allWorlds)
            {
                targetPlayers = new HashSet<Player>();
                foreach (Rank rank in targetRanks)
                {
                    foreach (Player rankPlayer in Server.Players.Ranked(rank))
                    {
                        targetPlayers.Add(rankPlayer);
                    }
                }
            }
            else if (allRanks)
            {
                targetPlayers = new HashSet<Player>();
                foreach (World world in targetWorlds)
                {
                    foreach (Player worldPlayer in world.Players)
                    {
                        targetPlayers.Add(worldPlayer);
                    }
                }
            }
            else
            {
                targetPlayers = new HashSet<Player>();
                foreach (Rank rank in targetRanks)
                {
                    foreach (World world in targetWorlds)
                    {
                        foreach (Player rankWorldPlayer in world.Players.Ranked(rank))
                        {
                            targetPlayers.Add(rankWorldPlayer);
                        }
                    }
                }
            }

            Rank bringLimit = player.Info.Rank.GetLimit(Permission.Bring);

            // Remove the player him/herself
            targetPlayers.Remove(player);

            int count = 0;


            // Actually bring all the players
            foreach (Player targetPlayer in targetPlayers.CanBeSeen(player)
                .RankedAtMost(bringLimit))
            {
                if (targetPlayer.World == player.World)
                {
                    // teleport within the same world
                    targetPlayer.TeleportTo(player.Position);
                    targetPlayer.Position = player.Position;
                    if (targetPlayer.Info.IsFrozen)
                    {
                        targetPlayer.Position = player.Position;
                    }
                }
                else
                {
                    // teleport to a different world
                    if (player.World != null) BringPlayerToWorld(player, targetPlayer, player.World, false, true);
                }
                count++;
            }

            // Check if there's anyone to bring
            if (count == 0)
            {
                player.Message("No players to bring!");
            }
            else
            {
                player.Message("Bringing {0} players...", count);
            }
        }


        private static void BringPlayerToWorld([NotNull] Player player, [NotNull] Player target, [NotNull] World world,
            bool overridePermissions, bool usePlayerPosition)
        {
            if (player == null) throw new ArgumentNullException("player");
            if (target == null) throw new ArgumentNullException("target");
            if (world == null) throw new ArgumentNullException("world");
            switch (world.AccessSecurity.CheckDetailed(target.Info))
            {
                case SecurityCheckResult.Allowed:
                case SecurityCheckResult.WhiteListed:
                    if (world.IsFull)
                    {
                        player.Message("Cannot bring {0}&S because world {1}&S is full.",
                            target.ClassyName,
                            world.ClassyName);
                        return;
                    }
                    target.StopSpectating();
                    if (usePlayerPosition)
                    {
                        target.JoinWorld(world, WorldChangeReason.Bring, player.Position);
                    }
                    else
                    {
                        target.JoinWorld(world, WorldChangeReason.Bring);
                    }
                    break;

                case SecurityCheckResult.BlackListed:
                    player.Message("Cannot bring {0}&S because he/she is blacklisted on world {1}",
                        target.ClassyName,
                        world.ClassyName);
                    break;

                case SecurityCheckResult.RankTooLow:
                    if (overridePermissions)
                    {
                        target.StopSpectating();
                        if (usePlayerPosition)
                        {
                            target.JoinWorld(world, WorldChangeReason.Bring, player.Position);
                        }
                        else
                        {
                            target.JoinWorld(world, WorldChangeReason.Bring);
                        }
                    }
                    else
                    {
                        player.Message("Cannot bring {0}&S because world {1}&S requires {2}+&S to join.",
                            target.ClassyName,
                            world.ClassyName,
                            world.AccessSecurity.MinRank.ClassyName);
                    }
                    break;
                // TODO: case PermissionType.RankTooHigh:
            }
        }

        #endregion

        #region Patrol & SpecPatrol

        private static readonly CommandDescriptor CdPatrol = new CommandDescriptor
        {
            Name = "Patrol",
            Aliases = new[] { "pat" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Patrol },
            Help = "Teleports you to the next player in need of checking.",
            Handler = PatrolHandler
        };


        private static readonly CommandDescriptor CdSpecPatrol = new CommandDescriptor
        {
            Name = "SpecPatrol",
            Aliases = new[] { "spat" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Patrol, Permission.Spectate },
            Help = "Teleports you to the next player in need of checking.",
            Handler = SpecPatrolHandler
        };

        private static void PatrolHandler(Player player, Command cmd)
        {
            World playerWorld = player.World;
            if (playerWorld == null) PlayerOpException.ThrowNoWorld(player);
            Player target = playerWorld.GetNextPatrolTarget(player);
            if (target == null)
            {
                player.Message("Patrol: No one to patrol in this world.");
                return;
            }

            player.TeleportTo(target.Position);
            player.Message("Patrol: Teleporting to {0}", target.ClassyName);
        }

        private static void SpecPatrolHandler(Player player, Command cmd)
        {
            World playerWorld = player.World;
            if (playerWorld == null) PlayerOpException.ThrowNoWorld(player);
            Player target = playerWorld.GetNextPatrolTarget(player,
                p => player.Can(Permission.Spectate, p.Info.Rank),
                true);
            if (target == null)
            {
                player.Message("Patrol: No one to spec-patrol in this world.");
                return;
            }

            target.LastPatrolTime = DateTime.UtcNow;
            player.Spectate(target);
        }

        #endregion

        #region Mute / Unmute

        private static readonly TimeSpan MaxMuteDuration = TimeSpan.FromDays(700); // 100w0d

        private static readonly CommandDescriptor CdMute = new CommandDescriptor
        {
            Name = "Mute",
            Category = CommandCategory.Moderation | CommandCategory.Chat,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Mute },
            Help = "&HMutes a player for a specified length of time.",
            Usage = "/Mute PlayerName Duration",
            Handler = MuteHandler
        };


        private static readonly CommandDescriptor CdUnmute = new CommandDescriptor
        {
            Name = "Unmute",
            Category = CommandCategory.Moderation | CommandCategory.Chat,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Mute },
            Help = "&HUnmutes a player.",
            Usage = "/Unmute PlayerName",
            Handler = UnmuteHandler
        };

        private static void MuteHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            string timeString = cmd.Next();
            TimeSpan duration;

            // validate command parameters
            if (String.IsNullOrEmpty(targetName) || String.IsNullOrEmpty(timeString) ||
                !timeString.TryParseMiniTimespan(out duration) || duration <= TimeSpan.Zero)
            {
                CdMute.PrintUsage(player);
                return;
            }

            // check if given time exceeds maximum (700 days)
            if (duration > MaxMuteDuration)
            {
                player.Message("Maximum mute duration is {0}.", MaxMuteDuration.ToMiniString());
                duration = MaxMuteDuration;
            }

            // find the target
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null) return;

            // actually mute
            try
            {
                target.Info.Mute(player, duration, true, true);
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        private static void UnmuteHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (String.IsNullOrEmpty(targetName))
            {
                CdUnmute.PrintUsage(player);
                return;
            }

            // find target
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null) return;

            try
            {
                target.Info.Unmute(player, true, true);
            }
            catch (PlayerOpException ex)
            {
                player.Message(ex.MessageColored);
            }
        }

        #endregion

        #region Spectate / Unspectate

        private static readonly CommandDescriptor CdSpectate = new CommandDescriptor
        {
            Name = "Spectate",
            Aliases = new[] { "follow", "spec" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Spectate },
            Usage = "/Spectate PlayerName",
            Handler = SpectateHandler
        };


        private static readonly CommandDescriptor CdUnspectate = new CommandDescriptor
        {
            Name = "Unspectate",
            Aliases = new[] { "unfollow", "unspec" },
            Category = CommandCategory.Moderation,
            Permissions = new[] { Permission.Spectate },
            NotRepeatable = true,
            Handler = UnspectateHandler
        };

        private static void SpectateHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                PlayerInfo lastSpec = player.LastSpectatedPlayer;
                if (lastSpec != null)
                {
                    Player spec = player.SpectatedPlayer;
                    if (spec != null)
                    {
                        player.Message("Now spectating {0}", spec.ClassyName);
                    }
                    else
                    {
                        player.Message("Last spectated {0}", lastSpec.ClassyName);
                    }
                }
                else
                {
                    CdSpectate.PrintUsage(player);
                }
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null) return;

            if (target == player)
            {
                player.Message("You cannot spectate yourself.");
                return;
            }

            if (!player.Can(Permission.Spectate, target.Info.Rank))
            {
                player.Message("You may only spectate players ranked {0}&S or lower.",
                    player.Info.Rank.GetLimit(Permission.Spectate).ClassyName);
                player.Message("{0}&S is ranked {1}",
                    target.ClassyName, target.Info.Rank.ClassyName);
                return;
            }

            if (!player.Spectate(target))
            {
                player.Message("Already spectating {0}", target.ClassyName);
            }
        }

        private static void UnspectateHandler(Player player, Command cmd)
        {
            if (!player.StopSpectating())
            {
                player.Message("You are not currently spectating anyone.");
            }
        }

        #endregion

        #region Pay

        private static readonly CommandDescriptor CdPay = new CommandDescriptor
        {
            Name = "Pay",
            Aliases = new[] { "send" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = false,
            Permissions = new[] { Permission.Economy },
            Usage = "/pay player amount",
            Help = "&SUsed to pay a certain player an amount of money.",
            Handler = PayHandler
        };

        private static void PayHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            string money = cmd.Next();

            if (money == null)
            {
                player.Message("&ePlease select the amount of bits you wish to send.");
                return;
            }

            if (targetName != null)
            {
                Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
                if (target == null)
                {
                    player.Message("&ePlease select a player to pay bits towards.");
                    return;
                }

                int amount;
                if (!int.TryParse(money, out amount))
                {
                    player.Message("&ePlease select from a whole number.");
                    return;
                }

                PayHandler(player, new Command("/economy pay " + target + " " + money));
            }
        }

        #endregion

        #region Balance

        private static readonly CommandDescriptor CdBalance = new CommandDescriptor
        {
            Name = "Balance",
            Aliases = new[] { "bal" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Economy },
            Usage = "/Balance <username>",
            Help = "&SCheck what a player's balance is.",
            Handler = BalanceHandler
        };

        private static void BalanceHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();

            try
            {
                if (targetName == null)
                {
                    player.Message("&cYou &ehave &c{0}&e " + ConfigKey.CurrencyPl.GetString().ToLower() + ".",
                        player.Info.Money);
                    return;
                }
                Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
                if (target == null)
                {
                }
                else
                {
                    player.Message("&e{0}&e has &C{1} &e" + ConfigKey.CurrencyPl.GetString().ToLower() + ".", target.ClassyName,
                        target.Info.Money);
                }
            }
            catch (ArgumentNullException)
            {
                CdBalance.PrintUsage(player);
            }
        }

        #endregion

        #region Economy

        private static readonly CommandDescriptor CdEconomy = new EconomyDescriptor
        {
            Name = "Economy",
            Aliases = new[] { "Money", "Econ" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Economy },
            Usage = "&H/Economy Pay Player Amount",
            Help = "&S" +
                   " &a/Pay &ewill pay that player an amount of bits," +
                   "and &a/Give &e+ &a/Take &ewill give/take coins from or to a player.",
            HelpSections = new Dictionary<string, string>
            {
                {
                    "Pay", "&H/Economy Pay Player Amount\n&S" +
                             "Pays the player the given amount of " + ConfigKey.CurrencyPl.GetString().ToLower() +"."
                },
                {
                    "Take", "&H/Economy Take Player Amount\n&S" +
                    "Takes the given amount of " + ConfigKey.CurrencyPl.GetString().ToLower() +" from the player."
                },
                {
                    "Give", "&H/Economy Give Player Amount\n&S" +
                    "Gives the player the given amount of " + ConfigKey.CurrencyPl.GetString().ToLower() +"."
                }
            },
            Handler = EconomyHandler
        };


        private static void EconomyHandler(Player player, Command cmd)
        {
            try
            {
                string option = cmd.Next();
                string targetName = cmd.Next();
                string amount = cmd.Next();
                if (option == null || amount == null || targetName == null) CdEconomy.PrintUsage(player);
                if (targetName == null) return;
                Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
                if (target == null) return;
                int amountnum;
                if (option != null)
                    switch (option.ToLower())
                    {
                        case "give":
                            if (!player.Can(Permission.ManageEconomy))
                            {
                                return;
                            }
                            if (player.Can(Permission.GiveSelf) && target == player) //Giving yourself da monai
                            {
                                if (!int.TryParse(amount, out amountnum))
                                {
                                    player.Message("&eThe amount must be a number without any decimals!");
                                    return;
                                }
                                if (cmd.IsConfirmed)
                                {
                                    GiveMoney(target, player, amountnum);
                                    int tNewMoney = target.Info.Money + amountnum;
                                    target.Info.Money = tNewMoney;
                                    return;
                                }
                                if (amountnum == 1)
                                {
                                    player.Confirm(cmd, "&SAre you sure you want to give {0} &W{1} &S {2}?", target.ClassyName, amountnum, ConfigKey.CurrencySl.GetString().ToLower());
                                    return;
                                }
                                player.Confirm(cmd, "&SAre you sure you want to give {0} &W{1} &S {2}?", target.ClassyName, amountnum, ConfigKey.CurrencyPl.GetString().ToLower());
                                return;
                            }


                            if ((!player.Can(Permission.GiveSelf)) && (target == player))
                            {
                                player.Message("&WYou cannot give yourself {0}.", ConfigKey.CurrencyPl.GetString().ToLower());
                                return;
                            }

                            if (!int.TryParse(amount, out amountnum))
                            {
                                player.Message("&WThe amount must be a number without any decimals.");
                                return;
                            }
                            if (cmd.IsConfirmed)
                            {
                                GiveMoney(target, player, amountnum);
                                int tNewMoney = target.Info.Money + amountnum;
                                target.Info.Money = tNewMoney;
                            }
                            else
                            {
                                if (amountnum == 1)
                                {
                                    player.Confirm(cmd, "&SAre you sure you want to give {0} &W{1} &S {2}?", target.ClassyName, amountnum, ConfigKey.CurrencySl.GetString().ToLower());
                                    return;
                                }
                                player.Confirm(cmd, "&SAre you sure you want to give {0} &W{1} &S {2}?", target.ClassyName, amountnum, ConfigKey.CurrencyPl.GetString().ToLower());
                                return;
                            }
                            break;
                        case "take":
                            if (!player.Can(Permission.ManageEconomy))
                            {
                                return;
                            }

                            if (target == player)
                            {
                                player.Message("&eYou cannot take take {0} from yourself.", ConfigKey.CurrencyPl.GetString().ToLower());
                                return;
                            }
                            if (!int.TryParse(amount, out amountnum))
                            {
                                player.Message("&WThe amount must be a number.");
                                return;
                            }

                            if (cmd.IsConfirmed)
                            {
                                if (amountnum > target.Info.Money)
                                {
                                    player.Message(
                                        "{0}&W doesn't have that many {1}", target.ClassyName, ConfigKey.CurrencyPl.GetString().ToLower());
                                }
                                else
                                {
                                    GiveMoney(target, player, amountnum);
                                    int tNewMoney = target.Info.Money - amountnum;
                                    target.Info.Money = tNewMoney;
                                }
                            }
                            else
                            {
                                if (amountnum == 1)
                                {
                                    player.Confirm(cmd, "&SAre you sure you want to take &W{0} &S{1} from {2}&S?", amountnum, ConfigKey.CurrencySl.GetString().ToLower(), target.ClassyName);
                                    return;
                                }
                                player.Confirm(cmd, "&SAre you sure you want to take &W{0} &S{1} from {2}&S?", amountnum, ConfigKey.CurrencyPl.GetString().ToLower(), target.ClassyName);
                            }
                            break;
                        case "pay":
                            if (target == player)
                            {
                                player.Message("&WYou cannot pay youself.");
                                return;
                            }
                            if (!int.TryParse(amount, out amountnum))
                            {
                                player.Message("&WThe amount must be a number without any decimals.");
                                return;
                            }

                            if (cmd.IsConfirmed)
                            {
                                if (amountnum > player.Info.Money)
                                {
                                    player.Message("&WYou don't have enough {0}.", ConfigKey.CurrencyPl.GetString().ToLower());
                                }
                                else
                                {
                                    PayMoney(target, player, amountnum);
                                    int pNewMoney = player.Info.Money - amountnum;
                                    int tNewMoney = target.Info.Money + amountnum;
                                    player.Info.Money = pNewMoney;
                                    target.Info.Money = tNewMoney;
                                }
                            }
                            else
                            {
                                if (amountnum == 1)
                                {
                                    player.Confirm(cmd, "&SAre you sure you want to pay &W{0} &S{1} from {2}&S?", amountnum, ConfigKey.CurrencySl.GetString().ToLower(), target.ClassyName);
                                    return;
                                }
                                player.Confirm(cmd, "&SAre you sure you want to pay &W{0} &S{1} from {2}&S?", amountnum, ConfigKey.CurrencyPl.GetString().ToLower(), target.ClassyName);
                            }
                            break;
                    }
            }
            catch (ArgumentNullException)
            {
                CdEconomy.PrintUsage(player);
            }
        }

        static void GiveMoney(Player player, Player sender, int amount)
        {
            if (amount == 1)
            {
                sender.Message(
    "&SYou have given {0} &S{1} {2}.",
    player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
                player.Message(
                    "{0} &Shas given you {1} {2}.",
                    sender.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
                Server.Players.Except(player)
                    .Except(sender)
                    .Message(
                        "{0} &Swas given {1} {2} by {3}&S.", player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower(), sender.ClassyName);
            }
            sender.Message(
    "&SYou have given {0} &S{1} {2}.",
    player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
            player.Message(
                "{0} &Shas given you {1} {2}.",
                sender.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
            Server.Players.Except(player)
                .Except(sender)
                .Message(
                    "{0} &Swas given {1} {2} by {3}&S.", player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower(), sender.ClassyName);
        }

        static void PayMoney(Player player, Player sender, int amount)
        {
            if (amount == 1)
            {
                sender.Message(
    "&SYou have paid {0} &S{1} {2}.",
    player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
                player.Message(
                    "{0} &Shas paid you {1} {2}.",
                    sender.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
                Server.Players.Except(player)
                    .Except(sender)
                    .Message(
                        "{0} &Swas paid {1} {2} by {3}&S.", player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower(), sender.ClassyName);
            }
            sender.Message(
    "&SYou have paid {0} &S{1} {2}.",
    player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
            player.Message(
                "{0} &Shas paid you {1} {2}.",
                sender.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower());
            Server.Players.Except(player)
                .Except(sender)
                .Message(
                    "{0} &Swas paid {1} {2} by {3}&S.", player.ClassyName, amount, ConfigKey.CurrencySl.GetString().ToLower(), sender.ClassyName);
        }

        #endregion

        #region Store

        private static readonly CommandDescriptor CdStore = new CommandDescriptor
        {
            Name = "Store",
            Aliases = new[] { "str" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = false,
            Permissions = new[] { Permission.Economy },
            Usage = "/Store [items/buy] [item] [field]",
            Help = "Buy special commands, like insult and hug.",
            Handler = StoreHandler
        };

        private static void StoreHandler(Player player, Command cmd)
        {
            try
            {
                string option = cmd.Next();
                string item = cmd.Next();
                string field = cmd.Next();
                if (option == null)
                {
                    CdStore.PrintUsage(player);
                }
                switch (option)
                {
                    case "items":
                        player.Message("&aHug&7 - &eHug Somebody! &c(&7" + ConfigKey.NickPrice.GetInt() + " " +
                                       ConfigKey.CurrencyPl.GetString().ToLower() + "&c)");
                        player.Message("&aInsult&7 - &eInsult Somebody! &c(&7" + ConfigKey.TitlePrice.GetInt() + " " +
                                       ConfigKey.CurrencyPl.GetString().ToLower() + "&c)");
                        player.Message("&aLottery&7 - &ePlay the Lottery! &c(&7" + ConfigKey.LotteryPrice.GetInt() + " " +
                                       ConfigKey.CurrencyPl.GetString().ToLower() + "&c)");
                        break;


                    case "buy":
                        if (item == null)
                        {
                            player.Message("&ePlease select an option [&aitems/buy&e]");
                        }
                        double time;
                        int pNewMoney;
                        switch (item)
                        {
                            case "hug":
                                if (field == null)
                                {
                                    player.Message("&SPlease enter a player name.");
                                }

                                else
                                {
                                    //Economy Stuff
                                    if (ConfigKey.NickPrice.GetInt() > player.Info.Money)
                                    {
                                        player.Message("You dont have enough " + ConfigKey.CurrencyPl.GetString().ToLower() +
                                                       "!");
                                        return;
                                    }
                                    Player target = Server.FindPlayerOrPrintMatches(player, field, false, true);
                                    //Taking the money...
                                    pNewMoney = player.Info.Money - ConfigKey.NickPrice.GetInt();

                                    //Actually Hug the Player
                                    if (target == player)
                                    {
                                        player.Message("&sAre you feeling lonley...");
                                        return;
                                    }
                                    time = (DateTime.UtcNow - player.LastUsedHug).TotalSeconds;
                                    if (time < 10)
                                    {
                                        player.Message("&WYou can use /Hug again in " + Math.Round(10 - time) +
                                                       " seconds.");
                                        return;
                                    }
                                    if (target == null)
                                    {
                                        return;
                                    }
                                    if (player.Can(Permission.Economy, target.Info.Rank))
                                    {
                                        Server.Players.CanSee(target)
                                            .Union(target)
                                            .Message("{0} &Swas hugged by {1}", target.ClassyName,
                                                player.ClassyName);
                                        IRC.PlayerSomethingMessage(player, "hugged", target, null);
                                        player.LastUsedHug = DateTime.UtcNow;
                                        player.Info.Money = pNewMoney;
                                    }
                                }
                                break;

                            case "insult":
                                if (field == null)
                                {
                                    player.Message("&ePlease enter a player's username");
                                }

                                else
                                {
                                    //Economy Stuff
                                    if (ConfigKey.TitlePrice.GetInt() > player.Info.Money)
                                    {
                                        player.Message("You dont have enough " +
                                                       ConfigKey.CurrencyPl.GetString().ToLower() + "!");
                                        return;
                                    }
                                    //Taking the money...
                                    pNewMoney = player.Info.Money - ConfigKey.LotteryPrice.GetInt();
                                    //Insult Dat Bitch
                                    Player target = Server.FindPlayerOrPrintMatches(player, field, false,
                                        true);
                                    var randomizer = new Random();
                                    var insults = new List<String>
                                    {
                                        "{0}&s pooped on {1}&s's mom's face.",
                                        "{0}&s spit in {1}&s's drink.",
                                        "{0}&s threw a chair at {1}&s.",
                                        "{0}&s rubbed their ass on {1}&s",
                                        "{0}&s flicked off {1}&s.",
                                        "{0}&s pulled down their pants and flashed {1}&s.",
                                        "{0}&s went into {1}&s's house on their birthday, locked them in the closet, and ate their birthday dinner.",
                                        "{0}&s went up to {1}&s and said 'mama, mama, mama, mama, mommy, mommy, mommy, mommy, ma, ma, ma, ma, mum, mum, mum, mum. Hi! hehehehe'",
                                        "{0}&s asked {1}&s if they were single, just to hear them say a painful 'yes'...",
                                        "{0}&s shoved a pineapple up {1}&s's ass",
                                        "{0}&s beat {1}&s with a cane.",
                                        "{0}&s put {1}&s in a boiling pot and started chanting.",
                                        "{0}&s ate cheetos then wiped their hands all over {1}&s's white clothes",
                                        "{0}&s sprayed {1}&s's crotch with water, then pointed and laughed.",
                                        "{0}&s tied up {1}&s and ate their last candy bar right in front of them.",
                                        "{0}&s gave {1}&s a wet willy.",
                                        "{0}&s gave {1}&s a wedgie.",
                                        "{0}&s gave {1}&s counterfeit money and then called the Secret Service on them.",
                                        "{0}&s shot {1}&s in the knee with an arrow.",
                                        "{0}&s called {1}&s a disfigured, bearded clam.",
                                        "{0}&s flipped a table onto {1}&s.",
                                        "{0}&s smashed {1}&s over the head with their vintage record.",
                                        "{0}&s dropped a piano on {1}&s.",
                                        "{0}&s burned {1}&s with a cigarette.",
                                        "{0}&s incinerated {1}&s with a Kamehameha!",
                                        "{0}&s had sex with {1}&s's mother.",
                                        "{0}&s shredded {1}&s's mother."
                                    };

                                    int index = randomizer.Next(0, insults.Count); // (0, 18)
                                    time = (DateTime.Now - player.LastUsedInsult).TotalSeconds;
                                    if (target == null)
                                        return;
                                    if (target == player)
                                    {
                                        player.Message("Are you mad at yourself...");
                                        return;
                                    }
                                    double timeLeft = Math.Round(20 - time);
                                    if (time < 20)
                                    {
                                        player.Message("You cannot use this command for another " +
                                                       timeLeft +
                                                       " second(s).");
                                        return;
                                    }
                                    Server.Message(insults[index], player.ClassyName,
                                        target.ClassyName);
                                    IRC.PlayerSomethingMessage(player, "insulted", target, null);
                                    player.LastUsedInsult = DateTime.Now;
                                    //Taking the money...
                                    player.Info.Money = pNewMoney;
                                }
                                break;

                            case "lottery":

                                //Economy Stuff
                                if (ConfigKey.LotteryPrice.GetInt() > player.Info.Money)
                                {
                                    player.Message("You dont have enough " + ConfigKey.CurrencyPl.GetString().ToLower() +
                                                   "!");
                                    return;
                                }
                                time = (DateTime.UtcNow - player.LastUsedLottery).TotalMinutes;
                                if (time < ConfigKey.LotteryTimeBetween.GetInt())
                                {
                                    player.Message("&eYou can play the lottery again in &c" +
                                                   Math.Round(ConfigKey.LotteryTimeBetween.GetInt() - time) +
                                                   " &eminutes.");
                                    return;
                                }
                                var rand = new Random();
                                int min, max;
                                min = ConfigKey.LotteryMin.GetInt();
                                max = ConfigKey.LotteryMax.GetInt();
                                int num = rand.Next(min, max + 1);
                                //actually give the player the money
                                pNewMoney = player.Info.Money + num - ConfigKey.LotteryPrice.GetInt();
                                player.Message(
                                    "&eYou won &C{0} &e" + ConfigKey.CurrencyPl.GetString().ToLower() +
                                    " &efrom the lottery!",
                                    num);
                                Server.Players.Except(player)
                                    .Message(
                                        "&e{0} won &C{1} &e" + ConfigKey.CurrencyPl.GetString().ToLower() +
                                        " &efrom the lottery!", player.ClassyName, num);
                                player.Info.Money = pNewMoney;
                                player.LastUsedLottery = DateTime.UtcNow;
                                break;
                        }
                        break;
                }
            }
            catch (ArgumentNullException)
            {
                CdStore.PrintUsage(player);
            }
        }

        #endregion

        #region Report

        public static
        List<Player> Reports = new List<Player>();

        private static readonly CommandDescriptor CdReport = new CommandDescriptor
        {
            Name = "Report",
            Category = CommandCategory.Chat,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = false,
            Usage = "&a/Report <player>",
            Help = "Report a player who has been griefing, etc.",
            Handler = ReportHandler
        };

        private static readonly CommandDescriptor CdReports = new CommandDescriptor
        {
            Name = "Reports",
            Category = CommandCategory.Chat,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = false,
            Usage = "&a/Reports",
            Help = "View all reports",
            Handler = ReportsHandler
        };

        private static void ReportHandler(Player player, Command cmd)
        {
            try
            {
                string targetName = cmd.Next();
                if (targetName == null) return;
                Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
                if (target == null) return;
                if (Reports.Contains(target))
                {
                    player.Message("&eThat player has already been reported.");
                    return;
                }
                Reports.Add(target);
                player.Message("&ePlayer sucesfully reported.");
            }
            catch (ArgumentNullException)
            {
                CdReport.PrintUsage(player);
            }
        }

        private static void ReportsHandler(Player player, Command cmd)
        {
            try
            {
                string option = cmd.Next();
                string playerReport = cmd.Next();
                if (option == null)
                {
                    if (!Reports.Any())
                    {
                        player.Message("&eThere are no reports.");
                    }
                    else
                    {
                        player.Message("&c----&7Reports&c----");
                        foreach (Player p in Reports)
                        {
                            player.Message(p.Name);
                        }
                    }
                }
                else if (option == "remove" && playerReport != null)
                {
                    Player target = Server.FindPlayerOrPrintMatches(player, playerReport, false, true);
                    if (target == null) return;
                    if (!Reports.Contains(target))
                    {
                        player.Message(target.Name + "&e could not be found.");
                        return;
                    }
                    if (Reports.Contains(target))
                    {
                        Reports.Remove(target);
                        player.Message(target.ClassyName + "&e was removed from the reports list.");
                        return;
                    }
                    CdReports.PrintUsage(player);
                }
            }
            catch (ArgumentNullException)
            {
                CdReports.PrintUsage(player);
            }
        }

        #endregion

        #region Freeze stuff

        // freeze target if player is allowed to do so
        private static void FreezeIfAllowed(Player player, PlayerInfo targetInfo)
        {
            if (targetInfo.IsOnline && !targetInfo.IsFrozen && player.Can(Permission.Freeze, targetInfo.Rank))
            {
                try
                {
                    targetInfo.Freeze(player, true, true);
                    player.Message("Player {0}&S has been frozen while you retry.", targetInfo.ClassyName);
                }
                catch (PlayerOpException)
                {
                }
            }
        }


        // warn player if others are still online from target's IP
        private static void WarnIfOtherPlayersOnIp(Player player, PlayerInfo targetInfo, Player except)
        {
            Player[] otherPlayers = Server.Players.FromIP(targetInfo.LastIP)
                .Except(except)
                .ToArray();
            if (otherPlayers.Length > 0)
            {
                player.Message("&WWarning: Other player(s) share IP with {0}&W: {1}",
                    targetInfo.ClassyName,
                    otherPlayers.JoinToClassyString());
            }
        }

        #endregion

        private class EconomyDescriptor : CommandDescriptor
        {
            public override void PrintUsage(Player player)
            {
                base.PrintUsage(player);
                if (player.Can(Permission.ManageEconomy))
                {
                    player.Message("&H/Economy | Pay | Take | Give Player Amount");
                }
            }
        }
    }
}