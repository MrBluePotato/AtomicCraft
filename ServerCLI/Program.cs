using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.ComponentModel;
using fCraft.Events;
using System.Reflection;

namespace fCraft.ServerCLI
{
    internal static class Program
    {
        private static bool useColor = true;

        private static void Main(string[] args)
        {
            Logger.Logged += OnLogged;
            Heartbeat.UriChanged += OnHeartbeatUriChanged;

            Console.Title = "AtomicCraft " + Updater.CurrentRelease.VersionString + " - starting...";

#if !DEBUG
            try {
#endif
            Server.InitLibrary(args);
            useColor = !Server.HasArg(ArgKey.NoConsoleColor);

            Server.InitServer();

            Updater.UpdateCheck();
            Console.Title = "AtomicCraft " + Updater.CurrentRelease.VersionString + " - " +
                            ConfigKey.ServerName.GetString();

            if (!ConfigKey.ProcessPriority.IsBlank())
            {
                try
                {
                    Process.GetCurrentProcess().PriorityClass =
                        ConfigKey.ProcessPriority.GetEnum<ProcessPriorityClass>();
                }
                catch (Exception)
                {
                    Logger.Log(LogType.Warning, "Program.Main: Could not set process priority, using defaults.");
                }
            }

            if (Server.StartServer())
            {
                Console.WriteLine("** Running AtomicCraft version {0}. **", Updater.CurrentRelease.VersionString);
                Console.WriteLine("** Server is now ready. Type /Shutdown to exit safely. **");

                while (!Server.IsShuttingDown)
                {
                    string cmd = Console.ReadLine();
                    if (cmd.Equals("/Clear", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Clear();
                    }
                    else
                    {
                        try
                        {
                            Player.Console.ParseMessage(cmd, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogAndReportCrash("Error while executing a command from console", "ServerCLI", ex,
                                false);
                        }
                    }
                }
            }
            else
            {
                ReportFailure(ShutdownReason.FailedToStart);
            }
#if !DEBUG
            } catch( Exception ex ) {
                Logger.LogAndReportCrash( "Unhandled exception in ServerCLI", "ServerCLI", ex, true );
                ReportFailure( ShutdownReason.Crashed );
            } finally {
                Console.ResetColor();
            }
#endif
        }


        private static void ReportFailure(ShutdownReason reason)
        {
            Console.Title = String.Format("AtomicCraft {0} {1}", Updater.CurrentRelease.VersionString, reason);
            if (useColor) Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("** {0} **", reason);
            if (useColor) Console.ResetColor();
            Server.Shutdown(new ShutdownParams(reason, TimeSpan.Zero, false, false), true);
            if (!Server.HasArg(ArgKey.ExitOnCrash))
            {
                Console.ReadLine();
            }
        }


        [DebuggerStepThrough]
        private static void OnLogged(object sender, LogEventArgs e)
        {
            if (!e.WriteToConsole) return;
            switch (e.MessageType)
            {
                case LogType.Error:
                    if (useColor) Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.Message);
                    if (useColor) Console.ResetColor();
                    return;

                case LogType.SeriousError:
                    if (useColor) Console.ForegroundColor = ConsoleColor.White;
                    if (useColor) Console.BackgroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(e.Message);
                    if (useColor) Console.ResetColor();
                    return;

                case LogType.Warning:
                    if (useColor) Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(e.Message);
                    if (useColor) Console.ResetColor();
                    return;

                case LogType.Debug:
                case LogType.Trace:
                    if (useColor) Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(e.Message);
                    if (useColor) Console.ResetColor();
                    return;

                default:
                    Console.WriteLine(e.Message);
                    return;
            }
        }


        private static void OnHeartbeatUriChanged(object sender, UriChangedEventArgs e)
        {
            File.WriteAllText("externalurl.txt", e.NewUri.ToString(), Encoding.ASCII);
            Console.WriteLine("** URL: {0} **", e.NewUri);
            Console.WriteLine("URL is also saved to file externalurl.txt");
        }
    }
}