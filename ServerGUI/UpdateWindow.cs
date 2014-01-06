﻿// Copyright 2009-2014 Matvei Stefarov <me@matvei.org>
using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;
using System.Text;
using System.IO;

namespace fCraft.ServerGUI {
    public sealed partial class UpdateWindow : Form {
        readonly string updaterFullPath;
        readonly WebClient downloader = new WebClient();
        readonly bool autoUpdate;
        bool closeFormWhenDownloaded;

        public UpdateWindow() {
            InitializeComponent();
            updaterFullPath = Path.Combine( Paths.WorkingPath, Paths.UpdaterFileName );
            autoUpdate = (ConfigKey.UpdaterMode.GetEnum<UpdaterMode>() == UpdaterMode.Auto);
            lVersion.Text = String.Format( lVersion.Text,
                                           Updater.CurrentRelease.VersionString,
                                           Updater.WebVersionFullString);
            tChangeLog.Text = Updater.Changelog;
            Shown += Download;
        }


        void Download( object caller, EventArgs args ) {
            xShowDetails.Focus();
            downloader.DownloadProgressChanged += DownloadProgress;
            downloader.DownloadFileCompleted += DownloadComplete;
            ReleaseMode mode = ConfigKey.ReleaseMode.GetEnum<ReleaseMode>();
            if (mode == ReleaseMode.Public)
            {
                downloader.DownloadFileAsync(new Uri(Updater.PublicUpdaterLocation), updaterFullPath);
            }
            else if (mode == ReleaseMode.Dev)
            {
                downloader.DownloadFileAsync(new Uri(Updater.DevUpdaterLocation), updaterFullPath);
            }
        }


        void DownloadProgress( object sender, DownloadProgressChangedEventArgs e ) {
            Invoke( (Action)delegate {
                progress.Value = e.ProgressPercentage;
                lProgress.Text = "Downloading (" + e.ProgressPercentage + "%)";
            } );
        }


        void DownloadComplete( object sender, AsyncCompletedEventArgs e ) {
            if( closeFormWhenDownloaded ) {
                Close();
            } else {
                progress.Value = 100;
                if( e.Cancelled || e.Error != null ) {
                    MessageBox.Show( e.Error.ToString(), "Error occured while trying to download " + Paths.UpdaterFileName );
                } else if( autoUpdate ) {
                    bUpdateNow_Click( null, null );
                } else {
                    bUpdateNow.Enabled = true;
                    bUpdateLater.Enabled = true;
                }
            }
        }


        private void bCancel_Click( object sender, EventArgs e ) {
            Close();
        }

        private void bUpdateNow_Click( object sender, EventArgs e ) {
            string args = Server.GetArgString() +
                          String.Format( "--restart=\"{0}\"", MonoCompat.PrependMono( "Server (Graphical).exe" ) );
            MonoCompat.StartDotNetProcess( updaterFullPath, args, true );
            Server.Shutdown( new ShutdownParams( ShutdownReason.Restarting, TimeSpan.Zero, true, false ), false );
        }

        private void xShowDetails_CheckedChanged( object sender, EventArgs e ) {
        }

        private void bUpdateLater_Click( object sender, EventArgs e ) {
            Updater.RunAtShutdown = true;
            Logger.Log( LogType.SystemActivity,
                        "An AtomicCraft update will be applied next time the server is shut down or restarted." );
            Close();
        }

        private void UpdateWindow_FormClosing( object sender, FormClosingEventArgs e ) {
            if( !downloader.IsBusy ) return;
            downloader.CancelAsync();
            closeFormWhenDownloaded = true;
            e.Cancel = true;
        }
    }
}