/* Copyright 2009-2012 Matvei Stefarov <me@matvei.org>
 * 
 * Based, in part, on SmartIrc4net code. Original license is reproduced below.
 * 
 *
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2005 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
 *
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using System.Text;

namespace fCraft
{
    public static class GlobalChat
    {
        private const int Timeout = 10000; // socket timeout (ms)
        private const int ReconnectDelay = 15000;
        private static GlobalThread[] _threads;
        internal static int SendDelay = 750; //default

        private static string _hostName;
        private static int _port;
        private static string _channelName;
        private static string _botNick;

        private static readonly ConcurrentQueue<string> OutputQueue = new ConcurrentQueue<string>();


        // includes IRC color codes and non-printable ASCII
        public static readonly Regex NonPrintableChars = new Regex("\x03\\d{1,2}(,\\d{1,2})?|[\x00-\x1F\x7E-\xFF]",
            RegexOptions.Compiled);

        private static void AssignBotForInputParsing()
        {
            bool needReassignment = false;
            foreach (GlobalThread t in _threads.Where(t => t.ResponsibleForInputParsing && !t.IsReady))
            {
                t.ResponsibleForInputParsing = false;
                needReassignment = true;
            }
            if (!needReassignment) return;
            foreach (GlobalThread t in _threads.Where(t => t.IsReady))
            {
                t.ResponsibleForInputParsing = true;
                Logger.Log(LogType.SystemActivity,
                    "Bot \"{0}\" is now responsible for parsing input.",
                    t.ActualBotNick);
                return;
            }
            Logger.Log(LogType.SystemActivity, "All Global Chat bots have disconnected.");
        }

        public static void Init()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string data = wc.DownloadString("http://error.atomiccraft.net/gcdata.txt");
                    _hostName = data.Split('&')[0];
                    _channelName = data.Split('&')[1];
                }
            }
            catch
            {
                _hostName = "irc.geekshed.net";
                _channelName = "#atomiccraft.sex";
            }
            _port = 6667;
            _botNick = "[" + RemoveTroublesomeCharacters(ConfigKey.ServerName.GetString()) + "]";
        }

        public static string RemoveTroublesomeCharacters(string inString)
        {
            if (inString == null) return null;
            StringBuilder newString = new StringBuilder();
            char ch;
            foreach (char t in inString)
            {
                ch = t;
                if ((ch <= 0x007A && ch >= 0x0061) || (ch <= 0x005A && ch >= 0x0041) || (ch <= 0x0039 && ch >= 0x0030) ||
                    ch == ']')
                {
                    newString.Append(ch);
                }
            }
            return newString.ToString();
        }


        public static bool Start()
        {
            int threadCount = 1;

            if (threadCount == 1)
            {
                GlobalThread thread = new GlobalThread();
                if (thread.Start(_botNick, true))
                {
                    _threads = new[] {thread};
                }
            }
            else
            {
                List<GlobalThread> threadTemp = new List<GlobalThread>();
                for (int i = 0; i < threadCount; i++)
                {
                    GlobalThread temp = new GlobalThread();
                    if (temp.Start(_botNick + (i + 1), (threadTemp.Count == 0)))
                    {
                        threadTemp.Add(temp);
                    }
                }
                _threads = threadTemp.ToArray();
            }

            if (_threads.Length > 0)
            {
                //HookUpHandlers();
                return true;
            }
            Logger.Log(LogType.SystemActivity, "GlobalChat functionality disabled.");
            return false;
        }

        public sealed class GlobalThread : IDisposable
        {
            public static bool GcReady = false;
            public string ActualBotNick;
            public bool IsConnected;
            public bool IsReady;
            public bool ResponsibleForInputParsing;
            private TcpClient _client;
            private string _desiredBotNick;
            private DateTime _lastMessageSent;
            private StreamReader _reader;
            private bool _reconnect;
            private Thread _thread;
            private StreamWriter _writer;
            private ConcurrentQueue<string> localQueue = new ConcurrentQueue<string>();


            public bool Start([NotNull] string botNick, bool parseInput)
            {
                if (botNick == null) throw new ArgumentNullException("botNick");
                if (botNick.Length > 55)
                {
                    Logger.Log(LogType.Error, "Unable to start Global Chat (Server name exceeds 55 in length)");
                    return false;
                }


                _desiredBotNick = botNick;
                ResponsibleForInputParsing = parseInput;
                try
                {
                    // start the machinery!
                    _thread = new Thread(IoThread)
                    {
                        Name = "AtomicCraft.GlobalChat",
                        IsBackground = true
                    };
                    _thread.Start();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogType.Error,
                        "GlobalChat: Could not start the bot: {0}", ex);
                    return false;
                }
            }


            private void Connect()
            {
                // initialize the client
                IPAddress ipToBindTo = IPAddress.Parse(ConfigKey.IP.GetString());
                IPEndPoint localEndPoint = new IPEndPoint(ipToBindTo, 0);
                _client = new TcpClient(localEndPoint)
                {
                    NoDelay = true,
                    ReceiveTimeout = Timeout,
                    SendTimeout = Timeout
                };
                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);

                // connect
                _client.Connect(_hostName, _port);

                // prepare to read/write
                _reader = new StreamReader(_client.GetStream());
                _writer = new StreamWriter(_client.GetStream());
                IsConnected = true;
                GcReady = true;
            }


            public void Send([NotNull] string msg)
            {
                if (msg == null) throw new ArgumentNullException("msg");
                localQueue.Enqueue(msg);
            }

            public static void SendChannelMessage([NotNull] string line)
            {
                if (line == null) throw new ArgumentNullException("line");
                line = Color.MinecraftToIrcColors(line);
                if (_channelName == null || !GcReady)
                    return; // in case IRC bot is disabled.
                SendRawMessage(IRCCommands.Privmsg(_channelName, line));
            }

            public static void SendRawMessage([NotNull] string line)
            {
                if (line == null) throw new ArgumentNullException("line");

                OutputQueue.Enqueue(line);
            }


            // runs in its own thread, started from Connect()
            private void IoThread()
            {
                string outputLine = "";
                _lastMessageSent = DateTime.UtcNow;

                do
                {
                    try
                    {
                        ActualBotNick = _desiredBotNick;
                        _reconnect = false;
                        Logger.Log(LogType.SystemActivity,
                            "Connecting to AtomicCraft Global Chat as {2}",
                            _hostName, _port, ActualBotNick);
                        if (ActualBotNick == "[AtomicCraftDefaultServer]")
                        {
                            try
                            {
                                Logger.Log(LogType.Error, "You must set a server name to connect to global chat.");
                                _reconnect = false;
                                DisconnectThread();
                            }
                            catch (Exception)
                            {
                                return;
                            }
                        }
                        else
                        {
                            Connect();
                        }


                        // register
                        Send(IRCCommands.User(ActualBotNick, 8, ConfigKey.ServerName.GetString()));
                        Send(IRCCommands.Nick(ActualBotNick));

                        while (IsConnected && !_reconnect)
                        {
                            Thread.Sleep(10);

                            if (localQueue.Count > 0 &&
                                DateTime.UtcNow.Subtract(_lastMessageSent).TotalMilliseconds >= SendDelay &&
                                localQueue.TryDequeue(out outputLine))
                            {
                                _writer.Write(outputLine + "\r\n");
                                _lastMessageSent = DateTime.UtcNow;
                                _writer.Flush();
                            }

                            if (OutputQueue.Count > 0 &&
                                DateTime.UtcNow.Subtract(_lastMessageSent).TotalMilliseconds >= SendDelay &&
                                OutputQueue.TryDequeue(out outputLine))
                            {
                                _writer.Write(outputLine + "\r\n");
                                _lastMessageSent = DateTime.UtcNow;
                                _writer.Flush();
                            }

                            if (_client.Client.Available > 0)
                            {
                                string line = _reader.ReadLine();
                                if (line == null) break;
                                HandleMessage(line);
                            }
                        }
                    }
                    catch (SocketException)
                    {
                        Logger.Log(LogType.Warning, "GlobalChat: Disconnected. Will retry in {0} seconds.",
                            ReconnectDelay/1000);
                        _reconnect = true;
                    }
                    catch (IOException)
                    {
                        Logger.Log(LogType.Warning, "GlobalChat: Disconnected. Will retry in {0} seconds.",
                            ReconnectDelay/1000);
                        _reconnect = true;
#if !DEBUG
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogType.Error, "GlobalChat: {0}", ex);
                        reconnect = true;
#endif
                    }

                    if (_reconnect) Thread.Sleep(ReconnectDelay);
                } while (_reconnect);
            }

            public void SendMessage(string message)
            {
                HandleMessage("PRIVMSG " + message);
            }

            private void HandleMessage([NotNull] string message)
            {
                if (message == null) throw new ArgumentNullException("message");

                IRCMessage msg = IRC.MessageParser(message, ActualBotNick);
                var sendList = Server.Players.Where(p => !p.IsDeaf && !p.GlobalChatIgnore);
#if DEBUG_IRC
                Logger.Log( LogType.IRC,
                            "[{0}]: {1}",
                            msg.Type, msg.RawMessage );
#endif

                switch (msg.Type)
                {
                    case IRCMessageType.Login:
                        Send(IRCCommands.Join(_channelName));
                        IsReady = true;
                        AssignBotForInputParsing(); // bot should be ready to receive input after joining
                        return;


                    case IRCMessageType.Ping:
                        // ping-pong
                        Send(IRCCommands.Pong(msg.RawMessageArray[1].Substring(1)));
                        return;


                    case IRCMessageType.ChannelAction:
                    case IRCMessageType.ChannelMessage:
                        // channel chat
                        if (!ResponsibleForInputParsing) return;
                        string processedMessage = msg.Message;
                        if (msg.Type == IRCMessageType.ChannelAction)
                        {
                            if (processedMessage.StartsWith("\u0001ACTION"))
                            {
                                processedMessage = processedMessage.Substring(8);
                            }
                            else
                            {
                                return;
                            }
                        }

                        processedMessage = IRC.NonPrintableChars.Replace(processedMessage, "");
                        if (processedMessage.Length > 0)
                        {
                            if (msg.Type == IRCMessageType.ChannelAction)
                            {
                                sendList.Message("&g[Global] * {0} {1}",
                                    msg.Nick, processedMessage);
                            }
                            else
                            {
                                sendList.Message("&g[Global] {0}{1}: {2}",
                                    msg.Nick, Color.White, processedMessage);
                            }
                        }

                        else if (msg.Message.StartsWith("#"))
                        {
                            sendList.Message("&g[Global] {0}{1}: {2}",
                                msg.Nick, Color.White, processedMessage.Substring(1));
                        }
                        return;


                    case IRCMessageType.Join:
                        if (!ResponsibleForInputParsing) return;
                        if (msg.Nick.StartsWith("("))
                        {
                            sendList.Message("&g[Global] Server {0} joined Global Chat",
                                msg.Nick);
                        }
                        else
                        {
                            sendList.Message("&g[Global] {0} joined Global Chat",
                                msg.Nick);
                        }
                        return;


                    case IRCMessageType.Kick:
                        string kicked = msg.RawMessageArray[3];
                        if (kicked == ActualBotNick)
                        {
                            Logger.Log(LogType.SystemActivity,
                                "Bot was kicked from {0} by {1} ({2}), rejoining.",
                                msg.Channel, msg.Nick, msg.Message);
                            Thread.Sleep(ReconnectDelay);
                            Send(IRCCommands.Join(msg.Channel));
                        }
                        else
                        {
                            if (!ResponsibleForInputParsing) return;
                            sendList.Message("&g[Global] {0} kicked {1} ({2})",
                                msg.Nick, kicked, msg.Message);
                        }
                        return;


                    case IRCMessageType.Part:
                    case IRCMessageType.Quit:
                        if (!ResponsibleForInputParsing) return;
                        sendList.Message("&g[Global] Server {0} left Global Chat",
                            msg.Nick);
                        return;


                    case IRCMessageType.NickChange:
                        if (!ResponsibleForInputParsing) return;
                        sendList.Message("&g[Global] {0} is now known as {1}",
                            msg.Nick, msg.Message);
                        return;


                    case IRCMessageType.ErrorMessage:
                    case IRCMessageType.Error:
                        bool die = false;
                        switch (msg.ReplyCode)
                        {
                            case IRCReplyCode.ErrorNicknameInUse:
                            case IRCReplyCode.ErrorNicknameCollision:
                                ActualBotNick = ActualBotNick.Remove(ActualBotNick.Length - 4) + "_";
                                Logger.Log(LogType.SystemActivity,
                                    "Error: Global Chat Nickname is already in use. Trying \"{0}\"",
                                    ActualBotNick);
                                Send(IRCCommands.Nick(ActualBotNick));
                                break;

                            case IRCReplyCode.ErrorBannedFromChannel:
                            case IRCReplyCode.ErrorNoSuchChannel:
                                Logger.Log(LogType.SystemActivity,
                                    "Error: {0} ({1})",
                                    msg.ReplyCode, msg.Channel);
                                GcReady = false;
                                die = true;
                                break;
                                //wont happen
                            case IRCReplyCode.ErrorBadChannelKey:
                                Logger.Log(LogType.SystemActivity,
                                    "Error: Channel password required for {0}. AtomicCraft does not currently support passworded channels.",
                                    msg.Channel);
                                die = true;
                                GcReady = false;
                                break;

                            default:
                                Logger.Log(LogType.SystemActivity,
                                    "Error ({0}): {1}",
                                    msg.ReplyCode, msg.RawMessage);
                                GcReady = false;
                                break;
                        }

                        if (die)
                        {
                            Logger.Log(LogType.SystemActivity, "Error: Disconnecting from Global Chat.");
                            _reconnect = false;
                            DisconnectThread();
                        }

                        return;


                    case IRCMessageType.QueryAction:
                        // TODO: PMs
                        Logger.Log(LogType.SystemActivity,
                            "Query: {0}", msg.RawMessage);
                        break;


                    case IRCMessageType.Kill:
                        Logger.Log(LogType.SystemActivity,
                            "Bot was killed from {0} by {1} ({2}), reconnecting.",
                            _hostName, msg.Nick, msg.Message);
                        _reconnect = true;
                        IsConnected = false;
                        return;
                }
            }


            public void DisconnectThread()
            {
                IsReady = false;
                AssignBotForInputParsing();
                IsConnected = false;
                GcReady = false;
                if (_thread != null && _thread.IsAlive)
                {
                    _thread.Join(1000);
                    if (_thread.IsAlive)
                    {
                        _thread.Abort();
                    }
                }
                try
                {
                    if (_reader != null) _reader.Close();
                }
                catch (ObjectDisposedException)
                {
                }
                try
                {
                    if (_writer != null) _writer.Close();
                }
                catch (ObjectDisposedException)
                {
                }
                try
                {
                    if (_client != null) _client.Close();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception)
                {
                }
            }

            #region IDisposable members

            public void Dispose()
            {
                try
                {
                    if (_reader != null) _reader.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }

                try
                {
                    if (_reader != null) _writer.Dispose();
                }
                catch (ObjectDisposedException)
                {
                }

                try
                {
                    if (_client != null && _client.Connected)
                    {
                        _client.Close();
                    }
                }
                catch (ObjectDisposedException)
                {
                }
            }

            #endregion
        }
    }
}