// Copyright 2009-2013 Matvei Stefarov <me@matvei.org>
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using fCraft.Events;
using fCraft.GUI;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;

namespace fCraft.ServerGUI {

    public sealed partial class MainForm : Form {
        volatile bool shutdownPending, startupComplete, shutdownComplete;
        const int MaxLinesInLog = 2000,
                  LinesToTrimWhenExceeded = 50;
        SkinViewer v;

        public MainForm () {
            InitializeComponent();
            Shown += StartUp;
            console.OnCommand += console_Enter;
            logBox.LinkClicked += new LinkClickedEventHandler( Link_Clicked );
            MenuItem[] menuItems = new MenuItem[] { new MenuItem( "Copy", new EventHandler( CopyMenuOnClickHandler ) ) };
            logBox.ContextMenu = new ContextMenu( menuItems );
            logBox.ContextMenu.Popup += new EventHandler( CopyMenuPopupHandler );
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe");
        }

        #region Startup
        Thread startupThread;

        void StartUp ( object sender, EventArgs a ) {
            Logger.Logged += OnLogged;
            Heartbeat.UriChanged += OnHeartbeatUriChanged;
            Server.PlayerListChanged += OnPlayerListChanged;
            Server.ShutdownEnded += OnServerShutdownEnded;
            Text = "AtomicCraft " + Updater.CurrentRelease.VersionString + " - starting...";
            startupThread = new Thread( StartupThread );
            startupThread.Name = "AtomicCraft ServerGUI Startup";
            startupThread.Start();
        }


        void StartupThread () {
#if !DEBUG
            try {
#endif
                Server.InitLibrary( Environment.GetCommandLineArgs() );
                if ( shutdownPending ) return;

                Server.InitServer();
                if ( shutdownPending ) return;

                BeginInvoke( ( Action )OnInitSuccess );

                // check for updates
                UpdaterMode updaterMode = ConfigKey.UpdaterMode.GetEnum<UpdaterMode>();
                if ( updaterMode != UpdaterMode.Disabled ) {
                    if ( shutdownPending ) return;
                    if ( Updater.UpdateCheck() ) {
                        if ( updaterMode == UpdaterMode.Notify ) {
                            String updateMsg = String.Format( "An AtomicCraft update is available! Visit http://github.com/glennmr/AtomicCraft/downloads to download. " +
                                                              "Local version: {0}. Latest available version: {1}.",
                                                              Updater.CurrentRelease.VersionString,
                                                              Updater.WebVersionFullString );
                            Logger.LogToConsole( updateMsg );
                        } else {
                            DialogResult result = new UpdateWindow().ShowDialog();
                            if ( result == DialogResult.Cancel ) {
                                // startup aborted (restart for update)
                                return;
                            }
                        }
                    }
                }


                // set process priority
                if ( !ConfigKey.ProcessPriority.IsBlank() ) {
                    try {
                        Process.GetCurrentProcess().PriorityClass = ConfigKey.ProcessPriority.GetEnum<ProcessPriorityClass>();
                    } catch ( Exception ) {
                        Logger.Log( LogType.Warning,
                                    "MainForm.StartServer: Could not set process priority, using defaults." );
                    }
                }

                if ( shutdownPending ) return;
                if ( Server.StartServer() ) {
                    startupComplete = true;
                    BeginInvoke( ( Action )OnStartupSuccess );
                } else {
                    BeginInvoke( ( Action )OnStartupFailure );
                }
#if !DEBUG
            } catch ( Exception ex ) {
                Logger.LogAndReportCrash( "Unhandled exception in ServerGUI.StartUp", "ServerGUI", ex, true );
                Shutdown( ShutdownReason.Crashed, Server.HasArg( ArgKey.ExitOnCrash ) );
            }
#endif
        }


        void OnInitSuccess () {
            Text = "AtomicCraft " + Updater.CurrentRelease.VersionString + " - " + ConfigKey.ServerName.GetString();
        }


        void OnStartupSuccess () {
            if ( !ConfigKey.HeartbeatEnabled.Enabled() ) {
                uriDisplay.Text = "Heartbeat disabled. See externalurl.txt";
            }
            console.Enabled = true;
            console.Text = "";
        }


        void OnStartupFailure () {
            Shutdown( ShutdownReason.FailedToStart, Server.HasArg( ArgKey.ExitOnCrash ) );
        }

        #endregion


        #region Shutdown

        protected override void OnFormClosing ( FormClosingEventArgs e ) {
            if ( startupThread != null && !shutdownComplete ) {
                Shutdown( ShutdownReason.ProcessClosing, true );
                e.Cancel = true;
            } else {
                base.OnFormClosing( e );
            }
        }


        void Shutdown ( ShutdownReason reason, bool quit ) {
            if ( shutdownPending ) return;
            shutdownPending = true;
            console.Enabled = false;
            console.Text = "Shutting down...";
            Text = "AtomicCraft " + Updater.CurrentRelease.VersionString + " - shutting down...";
            uriDisplay.Enabled = false;
            if ( !startupComplete ) {
                startupThread.Join();
            }
            Server.Shutdown( new ShutdownParams( reason, TimeSpan.Zero, quit, false ), false );
        }


        void OnServerShutdownEnded ( object sender, ShutdownEventArgs e ) {
            try {
                BeginInvoke( ( Action )delegate {
                    shutdownComplete = true;
                    switch ( e.ShutdownParams.Reason ) {
                        case ShutdownReason.FailedToInitialize:
                        case ShutdownReason.FailedToStart:
                        case ShutdownReason.Crashed:
                            if ( Server.HasArg( ArgKey.ExitOnCrash ) ) {
                                Application.Exit();
                            }
                            break;
                        default:
                            Application.Exit();
                            break;
                    }
                } );
            } catch ( ObjectDisposedException ) {
            } catch ( InvalidOperationException ) { }
        }

        #endregion


        public void OnLogged ( object sender, LogEventArgs e ) {
            if ( !e.WriteToConsole ) return;
            try {
                if ( shutdownComplete ) return;
                if ( logBox.InvokeRequired ) {
                    BeginInvoke( ( EventHandler<LogEventArgs> )OnLogged, sender, e );
                } else {
                    // store user's selection
                    int userSelectionStart = logBox.SelectionStart;
                    int userSelectionLength = logBox.SelectionLength;
                    bool userSelecting = ( logBox.SelectionStart != logBox.Text.Length && logBox.Focused ||
                                          logBox.SelectionLength > 0 );

                    // insert and color a new message
                    int oldLength = logBox.Text.Length;
                    string msgToAppend = e.Message + Environment.NewLine;
                    logBox.AppendText( msgToAppend );
                    logBox.Select( oldLength, msgToAppend.Length );
                    switch ( e.MessageType ) {
                        case LogType.PrivateChat:
                            logBox.SelectionColor = System.Drawing.Color.Teal;
                            break;
                        case LogType.IRC:
                                logBox.SelectionColor = System.Drawing.Color.Blue;
                            break;

                        case LogType.ChangedWorld:
                            logBox.SelectionColor = System.Drawing.Color.Orange;
                            break;
                        case LogType.Warning:
                                logBox.SelectionColor = System.Drawing.Color.Green;
                                        break;
                        case LogType.Debug:
                            logBox.SelectionColor = System.Drawing.Color.DarkGray;
                            break;
                        case LogType.Error:
                        case LogType.SeriousError:
                            logBox.SelectionColor = System.Drawing.Color.Red;
                            break;
                        case LogType.ConsoleInput:
                        case LogType.ConsoleOutput:
                                logBox.SelectionColor = System.Drawing.Color.Black;
                            break;
                        default:
                        logBox.SelectionColor = System.Drawing.Color.Black;
                            break;
                    }

                    // cut off the log, if too long
                    if ( logBox.Lines.Length > MaxLinesInLog ) {
                        logBox.SelectionStart = 0;
                        logBox.SelectionLength = logBox.GetFirstCharIndexFromLine( LinesToTrimWhenExceeded );
                        userSelectionStart -= logBox.SelectionLength;
                        if ( userSelectionStart < 0 ) userSelecting = false;
                        string textToAdd = "----- cut off, see " + Logger.CurrentLogFileName + " for complete log -----" + Environment.NewLine;
                        logBox.SelectedText = textToAdd;
                        userSelectionStart += textToAdd.Length;
                        logBox.SelectionColor = System.Drawing.Color.DarkGray;
                    }

                    // either restore user's selection, or scroll to end
                    if ( userSelecting ) {
                        logBox.Select( userSelectionStart, userSelectionLength );
                    } else {
                        logBox.SelectionStart = logBox.Text.Length;
                        logBox.ScrollToCaret();
                    }
                }
            } catch ( ObjectDisposedException ) {
            } catch ( InvalidOperationException ) { }
        }


        public void OnHeartbeatUriChanged ( object sender, UriChangedEventArgs e ) {
            try {
                if ( shutdownPending ) return;
                if ( uriDisplay.InvokeRequired ) {
                    BeginInvoke( ( EventHandler<UriChangedEventArgs> )OnHeartbeatUriChanged,
                            sender, e );
                } else {
                    uriDisplay.Text = e.NewUri.ToString();
                    uriDisplay.Enabled = true;
                }
            } catch ( ObjectDisposedException ) {
            } catch ( InvalidOperationException ) { }
        }


        public void OnPlayerListChanged ( object sender, EventArgs e ) {
            try {
                if ( shutdownPending ) return;
                if ( playerList.InvokeRequired ) {
                    BeginInvoke( ( EventHandler )OnPlayerListChanged, null, EventArgs.Empty );
                } else {
                    playerList.Items.Clear();
                    Player[] playerListCache = Server.Players.OrderBy( p => p.Info.Rank.Index ).ToArray();
                    foreach ( Player player in playerListCache ) {
                        playerList.Items.Add( player.Info.Rank.Name + " - " + player.Name );
                    }
                }
            } catch ( ObjectDisposedException ) {
            } catch ( InvalidOperationException ) { }
        }


        private void console_Enter () {
            string[] separator = { Environment.NewLine };
            string[] lines = console.Text.Trim().Split( separator, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string line in lines ) {
#if !DEBUG
                try {
#endif
                    if ( line.Equals( "/Clear", StringComparison.OrdinalIgnoreCase ) ) {
                        logBox.Clear();
                    } else if ( line.Equals( "/credits", StringComparison.OrdinalIgnoreCase ) ) {
                        new AboutWindow().Show();
                    } else {
                        Player.Console.ParseMessage( line, true );
                    }
#if !DEBUG
                } catch ( Exception ex ) {
                    Logger.LogToConsole( "Error occured while trying to execute last console command: " );
                    Logger.LogToConsole( ex.GetType().Name + ": " + ex.Message );
                    Logger.LogAndReportCrash( "Exception executing command from console", "ServerGUI", ex, false );
                }
#endif
            }
            console.Text = "";
        }

        private void logBox_TextChanged ( object sender, EventArgs e ) {

        }

        private void Link_Clicked ( object sender, LinkClickedEventArgs e ) {
            System.Diagnostics.Process.Start( e.LinkText );
        }

        private void MainForm_Load ( object sender, EventArgs e ) {

        }


        private void CopyMenuOnClickHandler ( object sender, EventArgs e ) {
            if ( logBox.SelectedText.Length > 0 )
                Clipboard.SetText( logBox.SelectedText.ToString(), TextDataFormat.Text );
        }

        private void CopyMenuPopupHandler ( object sender, EventArgs e ) {
            ContextMenu menu = sender as ContextMenu;
            if ( menu != null ) {
                menu.MenuItems[0].Enabled = ( logBox.SelectedText.Length > 0 );
            }
        }

        private void playerList_SelectedIndexChanged ( object sender, EventArgs e ) {
            try {
                string s = ( string )playerList.Items[playerList.SelectedIndex];
                s = s.Substring( s.IndexOf( '-' ),
                    s.Length - s.IndexOf( '-' ) )
                    .Replace( "-", "" )
                    .Replace( " ", "" )
                    .Trim();
                PlayerInfo player = PlayerDB.FindPlayerInfoExact( s );
                if ( player == null ) return;
                v = new SkinViewer( player );
                v.Show();
            } catch {  } //do nothing at all
        }

        private void contextMenuStrip1_Opening ( object sender, CancelEventArgs e ) {
            
        }
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;

                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_NOCLOSE;
                return cp;
            }
        }
    }
}