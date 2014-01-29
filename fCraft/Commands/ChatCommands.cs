using System;
using System.Collections.Generic;
using System.Linq;

namespace fCraft
{
    internal static class ChatCommands
    {
        #region GlobalChat

        private static readonly CommandDescriptor CdGlobal = new CommandDescriptor
        {
            Name = "Global",
            Category = CommandCategory.Chat,
            Aliases = new[] {"gl", "gc"},
            IsConsoleSafe = true,
            Permissions = new[] {Permission.Chat},
            Usage = "&a/Global [<message to send>/help/rules/accept/ignore/reconnect]",
            Help = "Sends a global message to other AtomicCraft servers",
            Handler = GlobalHandler
        };

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

            CommandManager.RegisterCommand(cdReview);
            CommandManager.RegisterCommand(CdAdminChat);
            CommandManager.RegisterCommand(CdCustomChat);
            CommandManager.RegisterCommand(CdAway);
            CommandManager.RegisterCommand(CdHigh5);
            CommandManager.RegisterCommand(CdPoke);
            CommandManager.RegisterCommand(CdVote);
            CommandManager.RegisterCommand(CdBroMode);
            CommandManager.RegisterCommand(CdRageQuit);
            CommandManager.RegisterCommand(CdQuit);
            CommandManager.RegisterCommand(CdGlobal);
            //CommandManager.RegisterCommand(CdOmegle);

            Player.Moved += new EventHandler<Events.PlayerMovedEventArgs>(Player_IsBack);
        }

        private static void GlobalHandler(Player player, Command cmd)
        {
            string Msg = cmd.NextAll();
            if (!ConfigKey.GCKey.Enabled())
            {
                player.Message("Global Chat is disabled on this server.");
                return;
            }
            if (!GlobalChat.GlobalThread.GCReady)
            {
                player.Message("Global Chat is not connected.");
                return;
            }
            if (Msg == "reconnect")
            {
                if (player.Can(Permission.ManageGlobalChat))
                {
                    if (GlobalChat.GlobalThread.GCReady)
                    {
                        player.Message("&cThis server is currently connected to global chat.");
                        return;
                    }
                    else
                    {
                        GlobalChat.GlobalThread.GCReady = true;
                        Server.Message(
                            "&eAttempting to connect to AtomicCraft Global Chat Network. This may take up to two minutes.");
                        GlobalChat.Init();
                        GlobalChat.Start();
                        return;
                    }
                }
            }

            var SendList = Server.Players.Where(p => p.GlobalChatAllowed && !p.IsDeaf);

            if ((!player.GlobalChatAllowed) && (Msg == "rules"))
            {
                player.Message(
                    " &cRules: No spamming and no advertising. All chat rules that apply to your server apply here.\n" +
                    "&eServer staff have the right to kick you.\n" +
                    "&9By using the Global Chat, you accept these conditions.\n" +
                    "&eType &a/Global accept &eto connect");
                return;
            }

            else if ((player.GlobalChatAllowed) && (Msg == "rules"))
            {
                player.Message(
                    " &cRules: No spamming and no advertising. All chat rules that apply to your server apply here.\n" +
                    "&eServer staff have the right to kick you.\n" +
                    "&9By using the Global Chat, you accept these conditions.");
                return;
            }

            else if ((!player.GlobalChatAllowed) && (Msg == "accept"))
            {
                player.GlobalChatAllowed = true;
                player.Message("&wThank you for accepting the global chat rules.\n" +
                               "&cYou now have global chat enabled.");
                GlobalChat.GlobalThread.SendChannelMessage(player.ClassyName + " &Sjoined global chat.");
                SendList.Message(player.ClassyName + " &Sjoined global chat.");
                return;
            }

            else if ((player.GlobalChatAllowed) && (Msg == "accept"))
            {
                player.Message("&You have already accepted the global chat rules.");
                return;
            }

            else if ((!player.GlobalChatIgnore) && (Msg == "ignore"))
            {
                player.GlobalChatIgnore = true;
                player.Message("&cYou have disconnected from global chat.");
                SendList.Message(player.ClassyName + " &Sdisconnected from global chat.");
                GlobalChat.GlobalThread.SendChannelMessage(player.ClassyName + " &Sdisconnected from global chat.");
                return;
            }

            else if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if ((!player.GlobalChatAllowed) && ((Msg.Length < 1) || (Msg.Length > 1)))
            {
                player.Message("&WYou must read and accept the global chat rules. Type &a/Global rules.");
                return;
            }

            else if ((player.GlobalChatAllowed) && ((Msg == "") || (Msg == null)))
            {
                player.Message("You must enter a message!");
                return;
            }
            else if (Msg == "help")
            {
                CdGlobal.PrintUsage(player);
                return;
            }

            else if (player.GlobalChatAllowed)
            {
                string pMsg = player.ClassyName + Color.White + ": " + Msg;
                Msg = player.ClassyName + Color.Black + ": " + Msg;
                SendList.Message("&i[Global] " + pMsg); //send the white message to Server
                Msg = Color.MinecraftToIrcColors(Msg);
                Msg = Color.ReplacePercentCodes(Msg);
                GlobalChat.GlobalThread.SendChannelMessage(Msg); //send the black message to GC
            }
        }

        #region QuitMsg

        private static readonly CommandDescriptor CdQuit = new CommandDescriptor
        {
            Name = "Quitmsg",
            Aliases = new[] {"quit", "quitmessage"},
            Category = CommandCategory.Chat,
            IsConsoleSafe = false,
            Permissions = new[] {Permission.Chat},
            Usage = "/Quitmsg [message]",
            Help = "Adds a farewell message which is displayed when you leave the server.",
            Handler = QuitHandler
        };

        private static void QuitHandler(Player player, Command cmd)
        {
            string Msg = cmd.NextAll();

            if (Msg.Length < 1)
            {
                CdQuit.PrintUsage(player);
                return;
            }
            else
            {
                player.Info.LeaveMsg = "left the server: &C" + Msg;
                player.Message("Your quit message is now set to: {0}", Msg);
            }
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
            Usage = "/Ragequit [reason]",
            Help = "An anger-quenching way to leave the server.",
            Handler = RageHandler
        };

        private static void RageHandler(Player player, Command cmd)
        {
            string reason = cmd.NextAll();
            if (reason.Length < 1)
            {
                Server.Players.Message("{0} &4Ragequit from the server", player.ClassyName);
                player.Kick(Player.Console, "&4Ragequit", LeaveReason.RageQuit, false, false, false);
                IRC.SendAction(player.ClassyName + " &4Ragequit from the server");
                return;
            }
            else
            {
                Server.Players.Message("{0} &4Ragequit from the server: &C{1}",
                    player.ClassyName, reason);
                IRC.SendAction(player.ClassyName + " &WRagequit from the server: " + reason);
                player.Kick(Player.Console, reason, LeaveReason.RageQuit, false, false, false);
            }
        }

        #endregion

        #region Bromode

        private static readonly CommandDescriptor CdBroMode = new CommandDescriptor
        {
            Name = "Bromode",
            Aliases = new string[] {"bm"},
            Category = CommandCategory.Chat | CommandCategory.Fun,
            Permissions = new[] {Permission.BroMode},
            IsConsoleSafe = true,
            Usage = "/Bromode",
            Help = "Toggles bromode.",
            Handler = BroMode
        };

        private static void BroMode(Player player, Command command)
        {
            if (!fCraft.Utils.BroMode.Active)
            {
                foreach (Player p in Server.Players)
                {
                    fCraft.Utils.BroMode.GetInstance().RegisterPlayer(p);
                }
                fCraft.Utils.BroMode.Active = true;
                Server.Players.Message("{0}&S turned Bro mode on.", player.Info.Rank.Color + player.Name);

                IRC.SendAction(player.Name + " turned Bro mode on.");
            }
            else
            {
                foreach (Player p in Server.Players)
                {
                    fCraft.Utils.BroMode.GetInstance().UnregisterPlayer(p);
                }

                fCraft.Utils.BroMode.Active = false;
                Server.Players.Message("{0}&S turned Bro Mode off.", player.Info.Rank.Color + player.Name);
                IRC.SendAction(player.Name + " turned Bro mode off");
            }
        }

        public static void Player_IsBack(object sender, Events.PlayerMovedEventArgs e)
        {
            if (e.Player.IsAway)
            {
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
            Usage = "/Vote | Ask | Kick | Yes | No | Abort",
            Help = "&HCreates a server-wide vote.",
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
            Usage = "/Customname Message",
            Help = "Broadcasts your message to all players allowed to read the CustomChatChannel.",
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
            if (message.Length > 0)
            {
                if (player.Can(Permission.UseColorCodes) && message.Contains("%"))
                {
                    message = Color.ReplacePercentCodes(message);
                }
                Chat.SendCustom(player, message);
            }
        }

        #endregion

        #region Away

        private static readonly CommandDescriptor CdAway = new CommandDescriptor
        {
            Name = "Away",
            Category = CommandCategory.Chat,
            Aliases = new[] {"afk"},
            IsConsoleSafe = true,
            Usage = "/away [optional message]",
            Help = "Shows an away message.",
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
                Server.Message("{0}&S &Eis away &9({1})",
                    player.ClassyName, msg);
                player.IsAway = true;
                return;
            }
            else
            {
                Server.Players.Message("&S{0} &Eis away &9(Away From Keyboard)", player.ClassyName);
                player.IsAway = true;
            }
        }

        #endregion

        #region HighFive

        private static readonly CommandDescriptor CdHigh5 = new CommandDescriptor
        {
            Name = "High5",
            Aliases = new string[] {"h5", "highfive"},
            Category = CommandCategory.Chat | CommandCategory.Fun,
            Permissions = new Permission[] {Permission.HighFive},
            IsConsoleSafe = true,
            Usage = "/High5 playername",
            Help = "High fives a given player.",
            NotRepeatable = true,
            Handler = High5Handler,
        };

        internal static void High5Handler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdHigh5.PrintUsage(player);
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
                .Message("{0}&S was just &chigh fived &Sby {1}&S", target.ClassyName, player.ClassyName);
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
            Usage = "/poke playername",
            Help = "&HPokes a Player.",
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
                player.Message("&SYou failed to poke {0}&S, they are immortal", target.ClassyName);
                return;
            }
            if (target == player)
            {
                player.Message("You cannot poke yourself.");
                return;
            }
            if (!Player.IsValidName(targetName))
            {
                return;
            }
            else
            {
                target.Message("&8You were just poked by {0}",
                    player.ClassyName);
                player.Message("&8Successfully poked {0}", target.ClassyName);
            }
        }

        #endregion

        #region Review

        private static readonly CommandDescriptor cdReview = new CommandDescriptor
        {
            Name = "Review",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "/review",
            NotRepeatable = true,
            Help = "&HRequest an Op to review your build.",
            Handler = Review
        };

        internal static void Review(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            var recepientList = Server.Players.Can(Permission.ReadStaffChat)
                .NotIgnoring(player)
                .Union(player);
            string message = String.Format("{0}&6 would like staff to check their build", player.ClassyName);
            recepientList.Message(message);
            var ReviewerNames = Server.Players
                .CanBeSeen(player)
                .Where(r => r.Can(Permission.Promote, player.Info.Rank));
            if (ReviewerNames.Count() > 0)
            {
                player.Message("&WOnline players who can review you: {0}",
                    ReviewerNames.JoinToString(r => String.Format("{0}&S", r.ClassyName)));
                return;
            }
            else
                player.Message("&WThere are no players online who can review you. A member of staff needs to be online.");
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
            Usage = "/Adminchat Message",
            Help = "Broadcasts your message to admins/owners on the server.",
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
            Usage = "/Say Message",
            Help = "&HShows a message in special color, without the player name prefix. " +
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
            Usage = "/Staff Message",
            Help = "Broadcasts your message to all operators/moderators on the server at once.",
            Handler = StaffHandler
        };

        private static void StaffHandler(Player player, Command cmd)
        {
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
            Usage = "/Ignore [PlayerName]",
            Help = "&HTemporarily blocks the other player from messaging you. " +
                   "If no player name is given, lists all ignored players.",
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
                    player.MessageNow("You are now ignoring {0}", targetInfo.ClassyName);
                }
                else
                {
                    player.MessageNow("You are already ignoring {0}", targetInfo.ClassyName);
                }
            }
            else
            {
                PlayerInfo[] ignoreList = player.IgnoreList;
                if (ignoreList.Length > 0)
                {
                    player.MessageNow("Ignored players: {0}", ignoreList.JoinToClassyString());
                }
                else
                {
                    player.MessageNow("You are not currently ignoring anyone.");
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
            Usage = "/Unignore PlayerName",
            Help = "Unblocks the other player from messaging you.",
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
                    player.MessageNow("You are no longer ignoring {0}", targetInfo.ClassyName);
                }
                else
                {
                    player.MessageNow("You are not currently ignoring {0}", targetInfo.ClassyName);
                }
            }
            else
            {
                PlayerInfo[] ignoreList = player.IgnoreList;
                if (ignoreList.Length > 0)
                {
                    player.MessageNow("Ignored players: {0}", ignoreList.JoinToClassyString());
                }
                else
                {
                    player.MessageNow("You are not currently ignoring anyone.");
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
            Usage = "/Me Message",
            Help = "&HSends IRC-style action message prefixed with your name.",
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
            Help = "Gives random number between 1 and 100.\n" +
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
                "{0}{1} rolled {2} ({3}...{4})",
                player.ClassyName, Color.Silver, num, min, max);
            player.Message("{0}You rolled {1} ({2}...{3})",
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
            Help = "Blocks all chat messages from being sent to you.",
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
                player.MessageNow("Deafened mode: ON");
                player.MessageNow("You will not see ANY messages until you type &H/Deafen&S again.");
                player.IsDeaf = true;
            }
            else
            {
                player.IsDeaf = false;
                player.MessageNow("Deafened mode: OFF");
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
            Help = "&HClears the chat screen.",
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
            Usage = "/Timer <Duration> <Message>",
            Help = "&HStarts a timer with a given duration and message. " +
                   "As the timer counts down, announcements are shown globally. See also: &H/Help Timer Abort",
            HelpSections = new Dictionary<string, string>
            {
                {
                    "abort", "&H/Timer Abort <TimerID>\n&S" +
                             "Aborts a timer with the given ID number. " +
                             "To see a list of timers and their IDs, type &H/Timer&S (without any parameters)."
                }
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
                    player.Message("No timers running.");
                }
                else
                {
                    player.Message("There are {0} timers running:", list.Length);
                    foreach (ChatTimer timer in list)
                    {
                        player.Message("  #{0} \"{1}&S\" (started by {2}, {3} left)",
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
                        player.Message("Given timer (#{0}) does not exist.", timerId);
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
                player.Message("Timer: Must be at least 1 second.");
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
    }
}

#endregion