// Copyright 2009-2014 Matvei Stefarov <me@matvei.org>

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

namespace fCraft
{
    internal static class ChatCommands
    {
        public static void Init()
        {
            CommandManager.RegisterCommand(CdSay);
            CommandManager.RegisterCommand(CdStaff);
            CommandManager.RegisterCommand(CdIgnore);
            CommandManager.RegisterCommand(CdUnignore);
            CommandManager.RegisterCommand(CdMe);
            CommandManager.RegisterCommand(CdRoll);
            CommandManager.RegisterCommand(CdDeafen);
            CommandManager.RegisterCommand(CdClear);
            CommandManager.RegisterCommand(CdTimer);
            CommandManager.RegisterCommand(CdReview);
            CommandManager.RegisterCommand(CdAdminChat);
            CommandManager.RegisterCommand(CdCustomChat);
            CommandManager.RegisterCommand(CdAway);
            CommandManager.RegisterCommand(CdHighFive);
            CommandManager.RegisterCommand(CdPoke);
            CommandManager.RegisterCommand(CdVote);
            CommandManager.RegisterCommand(CdBroMode);
            CommandManager.RegisterCommand(CdRageQuit);
            CommandManager.RegisterCommand(CdGlobal);

            Player.Moved += Player_IsBack;
        }

        #region GlobalChat

        private static readonly CommandDescriptor CdGlobal = new GlobalChatDescriptor
        {
            Name = "Global",
            Category = CommandCategory.Chat,
            Aliases = new[] {"gl", "gc"},
            IsConsoleSafe = true,
            Permissions = new[] {Permission.Chat},
            Usage = "&H/Global Message | Help | Accept | Rules | Ignore",
            Help = "&SSends a global message to other AtomicCraft servers.",
            Handler = GlobalHandler
        };

        private static void GlobalHandler(Player player, Command cmd)
        {
            var sendList = Server.Players.Where(p => p.GlobalChatAllowed && !p.IsDeaf);
            string msg = cmd.NextAll();
            if (!ConfigKey.GlobalChat.Enabled())
            {
                player.Message("&WGlobal Chat is disabled on this server.");
                return;
            }
            if (!GlobalChat.GlobalThread.GCReady)
            {
                player.Message("&WGlobal Chat is not connected.");
                return;
            }
            switch (msg)
            {
                case "reconnect":
                    if (player.Can(Permission.ManageGlobalChat))
                    {
                        if (GlobalChat.GlobalThread.GCReady)
                        {
                            player.Message("&WThis server is currently connected to global chat.");
                            return;
                        }
                        GlobalChat.GlobalThread.GCReady = true;
                        Server.Message(
                            "&WAttempting to connect to AtomicCraft Global Chat Network. This may take a few seconds.");
                        GlobalChat.Init();
                        GlobalChat.Start();
                        return;
                    }
                    break;
                case "rules":
                    if (!player.GlobalChatAllowed)
                    {
                        player.Message(
                            "&RRules: No spamming and no advertising. All chat rules that apply to your server apply here.\n" +
                            "&WServer staff have the right to kick you.\n" +
                            "&SBy using the Global Chat, you accept these conditions.\n" +
                            "&SType &H/global accept &Sto connect");
                        return;
                    }

                    if (player.GlobalChatAllowed)
                    {
                        player.Message(
                            "&RRules: No spamming and no advertising. All chat rules that apply to your server apply here.\n" +
                            "&WServer staff have the right to kick you.\n" +
                            "&SBy using the Global Chat, you accept these conditions.");
                        return;
                    }
                    break;
                case "accept":

                    if (!player.GlobalChatAllowed)
                    {
                        player.GlobalChatAllowed = true;
                        player.Message("&SThank you for accepting the global chat rules.\n" +
                                       "&WYou now have global chat enabled.");
                        GlobalChat.GlobalThread.SendChannelMessage(player.ClassyName + " &Sjoined global chat.");
                        sendList.Message(player.ClassyName + " &Sjoined global chat.");
                        return;
                    }

                    if (player.GlobalChatAllowed)
                    {
                        player.Message("&WYou have already accepted the global chat rules.");
                        return;
                    }
                    break;
                case "ignore":
                    if (!player.GlobalChatIgnore)
                    {
                        player.GlobalChatIgnore = true;
                        player.Message("&WYou have disconnected from global chat.");
                        sendList.Message(player.ClassyName + " &Sdisconnected from global chat.");
                        GlobalChat.GlobalThread.SendChannelMessage(player.ClassyName +
                                                                   " &Sdisconnected from global chat.");
                        return;
                    }
                    break;
                case "help":
                    CdGlobal.PrintUsage(player);
                    break;
            }
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if ((!player.GlobalChatAllowed) && ((msg.Length < 1) || (msg.Length > 1)))
            {
                player.Message("&WYou must read and accept the global chat rules. Type &H/global rules");
                return;
            }

            if ((player.GlobalChatAllowed) && string.IsNullOrEmpty(msg))
            {
                player.Message("&WYou must enter a message!");
                return;
            }
            if (!player.GlobalChatAllowed) return;
            string pMsg = player.ClassyName + Color.White + ": " + msg;
            msg = player.ClassyName + Color.Black + ": " + msg;
            sendList.Message("&g[Global] " + pMsg); //send the white message to Server
            msg = Color.MinecraftToIrcColors(msg);
            msg = Color.ReplacePercentCodes(msg);
            GlobalChat.GlobalThread.SendChannelMessage(msg); //send the black message to GC
        }

        #endregion

        #region Ragequit

        private static readonly CommandDescriptor CdRageQuit = new CommandDescriptor
        {
            Name = "Ragequit",
            Aliases = new[] {"rq"},
            Category = CommandCategory.Chat | CommandCategory.Fun,
            IsConsoleSafe = false,
            Permissions = new[] {Permission.RageQuit},
            Usage = "&H/Ragequit reason",
            Help = "&SAn anger-quenching way to leave the server.",
            Handler = RageHandler
        };

        private static void RageHandler(Player player, Command cmd)
        {
            string reason = cmd.NextAll();
            if (reason.Length < 1)
            {
                Server.Players.Message("{0} &WRagequit from the server.", player.ClassyName);
                player.Kick(Player.Console, "&WRagequit", LeaveReason.RageQuit, false, false, false);
                IRC.SendAction(player.ClassyName + " &WRagequit from the server.");
                return;
            }
            Server.Players.Message("{0} &WRagequit from the server: {1}",
                player.ClassyName, reason);
            IRC.SendAction(player.ClassyName + " &WRagequit from the server: " + reason);
            player.Kick(Player.Console, reason, LeaveReason.RageQuit, false, false, false);
        }

        #endregion

        #region Bromode

        private static readonly CommandDescriptor CdBroMode = new CommandDescriptor
        {
            Name = "Bromode",
            Aliases = new[] {"bm"},
            Category = CommandCategory.Chat | CommandCategory.Fun,
            Permissions = new[] {Permission.BroMode},
            IsConsoleSafe = true,
            Usage = "&H/Bromode",
            Help = "&SToggles bromode, a fun nick-changing command.",
            Handler = BroMode
        };

        private static void BroMode(Player player, Command command)
        {
            if (!Utils.BroMode.Active)
            {
                foreach (Player p in Server.Players)
                {
                    Utils.BroMode.GetInstance().RegisterPlayer(p);
                }
                Utils.BroMode.Active = true;
                Server.Players.Message("{0}&S turned Bro mode on.", player.Info.Rank.Color + player.Name);

                IRC.SendAction(player.Name + " &Sturned Bro mode on.");
            }
            else
            {
                foreach (Player p in Server.Players)
                {
                    Utils.BroMode.GetInstance().UnregisterPlayer(p);
                }

                Utils.BroMode.Active = false;
                Server.Players.Message("{0}&S turned Bro Mode off.", player.Info.Rank.Color + player.Name);
                IRC.SendAction(player.Name + " &Sturned Bro mode off");
            }
        }

        public static void Player_IsBack(object sender, Events.PlayerMovedEventArgs e)
        {
            if (!e.Player.IsAway) return;
            // We need to have block positions, so we divide by 32
            Vector3I oldPos = new Vector3I(e.OldPosition.X/32, e.OldPosition.Y/32, e.OldPosition.Z/32);
            Vector3I newPos = new Vector3I(e.NewPosition.X/32, e.NewPosition.Y/32, e.NewPosition.Z/32);

            // Check if the player actually moved and not just rotated
            if ((oldPos.X != newPos.X) || (oldPos.Y != newPos.Y) || (oldPos.Z != newPos.Z))
            {
                Server.Players.Message("{0} &Eis back", e.Player.ClassyName);
                e.Player.IsAway = false;
            }
        }

        #endregion

        #region Vote

        private static readonly CommandDescriptor CdVote = new CommandDescriptor
        {
            Name = "Vote",
            Category = CommandCategory.Chat | CommandCategory.Fun,
            Permissions = new[] {Permission.Chat},
            IsConsoleSafe = false,
            NotRepeatable = true,
            Usage = "&H/Vote | Ask | Kick | Yes | No | Abort",
            Help = "&SCreates a server-wide vote.",
            Handler = VoteHandler
        };

        public static void VoteHandler(Player player, Command cmd)
        {
            fCraft.VoteHandler.VoteParams(player, cmd);
        }

        #endregion

        #region Customchat

        private static readonly CommandDescriptor CdCustomChat = new CommandDescriptor
        {
            Name = ConfigKey.CustomChatName.GetString(),
            Category = CommandCategory.Chat,
            Aliases = new[] {ConfigKey.CustomAliasName.GetString()},
            Permissions = new[] {Permission.Chat},
            IsConsoleSafe = true,
            NotRepeatable = true,
            Usage = "&H/" + ConfigKey.CustomChatName.GetString() + " Message",
            Help =
                "&SBroadcasts your message to all players allowed to read " + ConfigKey.CustomChatName.GetString() + ".",
            Handler = CustomChatHandler
        };

        private static void CustomChatHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            string message = cmd.NextAll().Trim();
            if (message.Length <= 0) return;
            if (player.Can(Permission.UseColorCodes) && message.Contains("%"))
            {
                message = Color.ReplacePercentCodes(message);
            }
            Chat.SendCustom(player, message);
        }

        #endregion

        #region Away

        private static readonly CommandDescriptor CdAway = new CommandDescriptor
        {
            Name = "Away",
            Category = CommandCategory.Chat,
            Aliases = new[] {"afk"},
            IsConsoleSafe = true,
            Usage = "&H/Away message",
            Help = "&SAlerts players that you are away.",
            NotRepeatable = true,
            Handler = Away
        };

        internal static void Away(Player player, Command cmd)
        {
            string msg = cmd.NextAll().Trim();
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            if (msg.Length > 0)
            {
                Server.Message("{0}&S is away &9({1})",
                    player.ClassyName, msg);
                player.IsAway = true;
                return;
            }
            Server.Players.Message("&S{0} is away &9(Away From Keyboard)", player.ClassyName);
            player.IsAway = true;
        }

        #endregion

        #region HighFive

        private static readonly CommandDescriptor CdHighFive = new CommandDescriptor
        {
            Name = "HighFive",
            Aliases = new string[] {"h5", "high5"},
            Category = CommandCategory.Chat | CommandCategory.Fun,
            Permissions = new Permission[] {Permission.HighFive},
            IsConsoleSafe = true,
            Usage = "&H/Highfive player",
            Help = "&SHigh fives a player.",
            NotRepeatable = true,
            Handler = High5Handler,
        };

        internal static void High5Handler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdHighFive.PrintUsage(player);
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null)
                return;
            if (target == player)
            {
                player.Message("&WYou cannot high five yourself.");
                return;
            }
            Server.Players.CanSee(target)
                .Except(target)
                .Message("{0}&S was just &chigh fived &Sby {1}&S.", target.ClassyName, player.ClassyName);
            IRC.PlayerSomethingMessage(player, "high fived", target, null);
            target.Message("{0}&S high fived you.", player.ClassyName);
        }

        #endregion

        #region Poke

        private static readonly CommandDescriptor CdPoke = new CommandDescriptor
        {
            Name = "Poke",
            Category = CommandCategory.Chat | CommandCategory.Fun,
            IsConsoleSafe = true,
            Usage = "&H/Poke player",
            Help = "&SPokes a player.",
            NotRepeatable = true,
            Handler = PokeHandler
        };

        internal static void PokeHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdPoke.PrintUsage(player);
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null)
            {
                return;
            }
            if (target.Immortal)
            {
                player.Message("&SYou cannot poke {0}&S because they are immortal.", target.ClassyName);
                return;
            }
            if (target == player)
            {
                player.Message("&SYou cannot poke yourself.");
                return;
            }
            if (!Player.IsValidName(targetName))
            {
                return;
            }
            target.Message("&8You were just poked by {0}&8.",
                player.ClassyName);
            player.Message("&8You poked {0}&8.", target.ClassyName);
        }

        #endregion

        #region Review

        private static readonly CommandDescriptor CdReview = new CommandDescriptor
        {
            Name = "Review",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "&H/Review",
            NotRepeatable = true,
            Help = "&SRequest a staff member to review your build.",
            Handler = Review
        };

        internal static void Review(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            var recepientList = Server.Players.Can(Permission.StaffChat)
                .NotIgnoring(player)
                .Union(player);
            string message = String.Format("{0}&S would like a staff member to review their build.", player.ClassyName);
            recepientList.Message(message);
            var reviewerNames = Server.Players
                .CanBeSeen(player)
                .Where(r => r.Can(Permission.Promote, player.Info.Rank));
            var enumerable = reviewerNames as Player[] ?? reviewerNames.ToArray();
            bool any = enumerable.Any();
            if (any)
            {
                player.Message("&WStaff members who can review you: {0}",
                    enumerable.JoinToString(r => String.Format("{0}&S", r.ClassyName)));
                return;
            }
            player.Message(
                "&WThere currently are no staff members online who can review you. A member of staff needs to be online.");
        }

        #endregion

        #region AdminChat

        private static readonly CommandDescriptor CdAdminChat = new CommandDescriptor
        {
            Name = "Adminchat",
            Aliases = new[] {"ac"},
            Category = CommandCategory.Chat | CommandCategory.Moderation,
            Permissions = new[] {Permission.Chat},
            IsConsoleSafe = true,
            NotRepeatable = true,
            Usage = "&H/Adminchat message",
            Help = "&SA special chat channel only for admins.",
            Handler = AdminChat
        };

        internal static void AdminChat(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            if (DateTime.UtcNow < player.Info.MutedUntil)
            {
                player.Message("You are muted for another {0:0} seconds.",
                    player.Info.MutedUntil.Subtract(DateTime.UtcNow).TotalSeconds);
                return;
            }
            string message = cmd.NextAll().Trim();
            if (message.Length > 0)
            {
                if (player.Can(Permission.UseColorCodes) && message.Contains("%"))
                {
                    message = Color.ReplacePercentCodes(message);
                }
                Chat.SendAdmin(player, message);
            }
        }

        #endregion

        #region Say

        private static readonly CommandDescriptor CdSay = new CommandDescriptor
        {
            Name = "Say",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            NotRepeatable = true,
            DisableLogging = true,
            Permissions = new[] {Permission.Chat, Permission.Say},
            Usage = "&H/Say message",
            Help = "&SShows a message in special color, without the player name prefix. " +
                   "Can be used for making announcements.",
            Handler = SayHandler
        };

        private static void SayHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            if (player.Can(Permission.Say))
            {
                string msg = cmd.NextAll().Trim();
                if (msg.Length > 0)
                {
                    Chat.SendSay(player, msg);
                }
                else
                {
                    CdSay.PrintUsage(player);
                }
            }
            else
            {
                player.MessageNoAccess(Permission.Say);
            }
        }

        #endregion

        #region Staff

        private static readonly CommandDescriptor CdStaff = new CommandDescriptor
        {
            Name = "Staff",
            Aliases = new[] {"st"},
            Category = CommandCategory.Chat | CommandCategory.Moderation,
            Permissions = new[] {Permission.Chat},
            NotRepeatable = true,
            IsConsoleSafe = true,
            DisableLogging = true,
            Usage = "&H/Staff message",
            Help = "&SA special chat channel only for staff members.",
            Handler = StaffHandler
        };

        private static void StaffHandler(Player player, Command cmd)
        {
            if (player.Can(Permission.StaffChat))
                if (player.Info.IsMuted)
                {
                    player.MessageMuted();
                    return;
                }

            if (player.DetectChatSpam()) return;

            string message = cmd.NextAll().Trim();
            if (message.Length > 0)
            {
                Chat.SendStaff(player, message);
            }
        }

        #endregion

        #region Ignore

        private static readonly CommandDescriptor CdIgnore = new CommandDescriptor
        {
            Name = "Ignore",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "&H/Ignore player",
            Help = "&SBlocks the given player from messaging you. " +
                   "If no player name is given, all players are ignored.",
            Handler = IgnoreHandler
        };

        private static void IgnoreHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name != null)
            {
                if (cmd.HasNext)
                {
                    CdIgnore.PrintUsage(player);
                    return;
                }
                PlayerInfo targetInfo = PlayerDB.FindPlayerInfoOrPrintMatches(player, name);
                if (targetInfo == null) return;

                if (player.Ignore(targetInfo))
                {
                    player.MessageNow("&WYou are now ignoring {0}&W.", targetInfo.ClassyName);
                }
                else
                {
                    player.MessageNow("&SYou are already ignoring {0}&S.", targetInfo.ClassyName);
                }
            }
            else
            {
                PlayerInfo[] ignoreList = player.IgnoreList;
                if (ignoreList.Length > 0)
                {
                    player.MessageNow("&WIgnored players: {0}&W.", ignoreList.JoinToClassyString());
                }
                else
                {
                    player.MessageNow("&SYou are not currently ignoring anyone.");
                }
                return;
            }
        }

        #endregion

        #region Unignore

        private static readonly CommandDescriptor CdUnignore = new CommandDescriptor
        {
            Name = "Unignore",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "&H/Unignore player",
            Help = "&SUnblocks the other player from messaging you.",
            Handler = UnignoreHandler
        };

        private static void UnignoreHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name != null)
            {
                if (cmd.HasNext)
                {
                    CdUnignore.PrintUsage(player);
                    return;
                }
                PlayerInfo targetInfo = PlayerDB.FindPlayerInfoOrPrintMatches(player, name);
                if (targetInfo == null) return;

                if (player.Unignore(targetInfo))
                {
                    player.MessageNow("&WYou are no longer ignoring {0}&W.", targetInfo.ClassyName);
                }
                else
                {
                    player.MessageNow("&SYou are not currently ignoring {0}&W.", targetInfo.ClassyName);
                }
            }
            else
            {
                PlayerInfo[] ignoreList = player.IgnoreList;
                if (ignoreList.Length > 0)
                {
                    player.MessageNow("&WIgnored players: {0}&W.", ignoreList.JoinToClassyString());
                }
                else
                {
                    player.MessageNow("&SYou are not currently ignoring anyone.");
                }
                return;
            }
        }

        #endregion

        #region Me

        private static readonly CommandDescriptor CdMe = new CommandDescriptor
        {
            Name = "Me",
            Category = CommandCategory.Chat,
            Permissions = new[] {Permission.Chat},
            IsConsoleSafe = true,
            NotRepeatable = true,
            DisableLogging = true,
            Usage = "&H/Me message",
            Help = "&SSends an action message prefixed with your name.",
            Handler = MeHandler
        };

        private static void MeHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            string msg = cmd.NextAll().Trim();
            if (msg.Length > 0)
            {
                Chat.SendMe(player, msg);
            }
            else
            {
                CdMe.PrintUsage(player);
            }
        }

        #endregion

        #region Roll

        private static readonly CommandDescriptor CdRoll = new CommandDescriptor
        {
            Name = "Roll",
            Category = CommandCategory.Chat,
            Permissions = new[] {Permission.Chat},
            IsConsoleSafe = true,
            Usage = "&H/Roll MinNumer MaxNumber",
            Help = "&SGives random number between 1 and 100.\n" +
                   "&H/Roll MaxNumber\n" +
                   "&S  Gives number between 1 and max.\n" +
                   "&H/Roll MinNumber MaxNumber\n" +
                   "&S  Gives number between min and max.",
            Handler = RollHandler
        };

        private static void RollHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            Random rand = new Random();
            int n1;
            int min, max;
            if (cmd.NextInt(out n1))
            {
                int n2;
                if (!cmd.NextInt(out n2))
                {
                    n2 = 1;
                }
                min = Math.Min(n1, n2);
                max = Math.Max(n1, n2);
            }
            else
            {
                min = 1;
                max = 100;
            }

            int num = rand.Next(min, max + 1);
            Server.Message(player,
                "&S{0}{1} rolled {2} ({3}...{4}).",
                player.ClassyName, Color.Silver, num, min, max);
            player.Message("&S{0}You rolled {1} ({2}...{3}).",
                Color.Silver, num, min, max);
        }

        #endregion

        #region Deafen

        private static readonly CommandDescriptor CdDeafen = new CommandDescriptor
        {
            Name = "Deafen",
            Aliases = new[] {"deaf"},
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "&H/Deafen",
            Help = "&SBlocks all chat messages from being sent to you.",
            Handler = DeafenHandler
        };

        private static void DeafenHandler(Player player, Command cmd)
        {
            if (cmd.HasNext)
            {
                CdDeafen.PrintUsage(player);
                return;
            }
            if (!player.IsDeaf)
            {
                for (int i = 0; i < LinesToClear; i++)
                {
                    player.MessageNow("");
                }
                player.MessageNow("&WDeafened mode is now on.");
                player.MessageNow("&SYou will not see any chat messages until you type &H/Deafen&S again.");
                player.IsDeaf = true;
            }
            else
            {
                player.IsDeaf = false;
                player.MessageNow("&WDeafened mode is now off.");
            }
        }

        #endregion

        #region Clear

        private const int LinesToClear = 30;

        private static readonly CommandDescriptor CdClear = new CommandDescriptor
        {
            Name = "Clear",
            UsableByFrozenPlayers = true,
            Category = CommandCategory.Chat,
            Usage = "&H/Clear",
            Help = "&SClears the chat screen.",
            Handler = ClearHandler
        };

        private static void ClearHandler(Player player, Command cmd)
        {
            if (cmd.HasNext)
            {
                CdClear.PrintUsage(player);
                return;
            }
            for (int i = 0; i < LinesToClear; i++)
            {
                player.Message("");
            }
        }

        #endregion

        #region Timer

        private static readonly CommandDescriptor CdTimer = new CommandDescriptor
        {
            Name = "Timer",
            Permissions = new[] {Permission.Say},
            IsConsoleSafe = true,
            Category = CommandCategory.Chat,
            Usage = "&H/Timer Duration Message",
            Help = "&SStarts a timer with a given duration and message. " +
                   "&SAs the timer counts down, announcements are shown globally. See also: &H/Help Timer Abort",
            HelpSections = new Dictionary<string, string>
            {
                {
                    "abort", "&H/Timer Abort <TimerID>\n&S" +
                             "Aborts a timer with the given ID number. " +
                             "To see a list of timers and their IDs, type &H/Timer&S (without any parameters)."
                },
            },
            Handler = TimerHandler
        };

        private static void TimerHandler(Player player, Command cmd)
        {
            string param = cmd.Next();

            // List timers
            if (param == null)
            {
                ChatTimer[] list = ChatTimer.TimerList.OrderBy(timer => timer.TimeLeft).ToArray();
                if (list.Length == 0)
                {
                    player.Message("&SNo timers running.");
                }
                else
                {
                    player.Message("&WThere are {0} timers running:", list.Length);
                    foreach (ChatTimer timer in list)
                    {
                        player.Message("#{0} \"{1}&S\" (started by {2}, {3} left)",
                            timer.Id, timer.Message, timer.StartedBy, timer.TimeLeft.ToMiniString());
                    }
                }
                return;
            }

            // Abort a timer
            if (param.Equals("abort", StringComparison.OrdinalIgnoreCase))
            {
                int timerId;
                if (cmd.NextInt(out timerId))
                {
                    ChatTimer timer = ChatTimer.FindTimerById(timerId);
                    if (timer == null || !timer.IsRunning)
                    {
                        player.Message("&WTimer #{0} does not exist.", timerId);
                    }
                    else
                    {
                        timer.Stop();
                        string abortMsg = String.Format("&Y(Timer) {0}&Y aborted a timer with {1} left: {2}",
                            player.ClassyName, timer.TimeLeft.ToMiniString(), timer.Message);
                        Chat.SendSay(player, abortMsg);
                    }
                }
                else
                {
                    CdTimer.PrintUsage(player);
                }
                return;
            }

            // Start a timer
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            if (player.DetectChatSpam()) return;
            TimeSpan duration;
            if (!param.TryParseMiniTimespan(out duration))
            {
                CdTimer.PrintUsage(player);
                return;
            }
            if (duration > DateTimeUtil.MaxTimeSpan)
            {
                player.MessageMaxTimeSpan();
                return;
            }
            if (duration < ChatTimer.MinDuration)
            {
                player.Message("&WTimer: Must be at least 1 second.");
                return;
            }

            string sayMessage;
            string message = cmd.NextAll();
            if (String.IsNullOrEmpty(message))
            {
                sayMessage = String.Format("&Y(Timer) {0}&Y started a {1} timer",
                    player.ClassyName,
                    duration.ToMiniString());
            }
            else
            {
                sayMessage = String.Format("&Y(Timer) {0}&Y started a {1} timer: {2}",
                    player.ClassyName,
                    duration.ToMiniString(),
                    message);
            }
            Chat.SendSay(player, sayMessage);
            ChatTimer.Start(duration, message, player.Name);
        }

        #endregion

        private class GlobalChatDescriptor : CommandDescriptor
        {
            public override void PrintUsage(Player player)
            {
                base.PrintUsage(player);
                if (player.Can(Permission.ManageGlobalChat))
                {
                    player.Message("&H/Global Message | Help | Accept | Rules | Ignore | Reconnect");
                }
            }
        }
    }
}