﻿using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynaframe3
{
    /// <summary>
    /// This class is responsible for processing commands from various sources (Webpage, get requests, mqtt, GPIO, etc)
    /// </summary>
    public static class CommandProcessor
    {
        static MainWindow handleMainWindow = null;

        public static void GetMainWindowHandle(MainWindow mainWindow)
        {
            handleMainWindow = mainWindow;
        }
        /// <summary>
        /// Turns the screen off using vcgencmd (Linux only, doesn't work on all screens)
        /// </summary>
        public static void TurnOffScreen()
        {
            Helpers.RunProcess("vcgencmd", "display_power 0");
        }

        /// <summary>
        /// Turns the screen on using vcgencmd (Linux only, doesn't work on all screens)
        /// </summary>
        public static void TurnOnScreen()
        {
            Helpers.RunProcess("vcgencmd", "display_power 1");
        }

        public static void SetInfoBar(string InfobarValue)
        {
            switch (InfobarValue)
            {
                case "INFOBAR_DATETIME":
                    {
                        AppSettings.Default.InfoBarState = AppSettings.InfoBar.DateTime;
                        break;
                    }
                case "INFOBAR_FILENAME":
                    {
                        AppSettings.Default.InfoBarState = AppSettings.InfoBar.FileInfo;
                        break;
                    }
                case "INFOBAR_HIDDEN":
                    {
                        AppSettings.Default.InfoBarState = AppSettings.InfoBar.OFF;
                        break;
                    }
                case "INFOBAR_IP":
                    {
                        AppSettings.Default.InfoBarState = AppSettings.InfoBar.IP;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            AppSettings.Default.ReloadSettings = true;
            AppSettings.Default.Save();
        }

        /// <summary>
        /// These commands are for controling the slide show such as skipping,
        /// going back, pausing, etc.
        /// </summary>
        /// <param name="ControlCommand"></param>
        public static void ControlSlideshow(string ControlCommand)
        {
            switch (ControlCommand)
            {
                case "CONTROL_FIRST":
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            handleMainWindow.GoToFirstImage();
                        });
                        break;
                    }
                case "CONTROL_BACKWARD":
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            handleMainWindow.GoToPreviousImage();
                        });
                        break;
                    }
                case "CONTROL_PAUSE":
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            handleMainWindow.Pause();
                        });
                        break;
                    }
                case "CONTROL_FORWARD":
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            handleMainWindow.GoToNextImage();
                        });
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// This method handles incoming transmissions from other frames. 
        /// </summary>
        /// <param name="filename"></param>
        public static void ProcessSetFile(string filename)
        {
            Logger.LogComment("SYNC: SetFile recieved: " + filename);

            // We have to do a bit of logic to figure out what is the 'closest' we can come to this file on this frame
            // This next call is where the magic happens for going from another frames file to matching on this frame
            string localFile = handleMainWindow.playListEngine.ConvertFileNameToLocal(filename);

            Logger.LogComment("SYNC: Converted it to: " + filename);
            handleMainWindow.PlayFile(localFile);
        }
        public static void ProcessCommand(string command)
        {
            Logger.LogComment("Command recieved: " + command);
            switch (command)
            {
                case "SCREENOFF":
                    {
                        TurnOffScreen();
                        break;
                    }
                case "SCREENON":
                    {
                        TurnOnScreen();
                        break;
                    }
                case "INFOBAR_DATETIME":
                case "INFOBAR_FILENAME":
                case "INFOBAR_HIDDEN":
                    {
                        SetInfoBar(command);
                        break;
                    }
                case "CONTROL_FIRST":
                case "CONTROL_BACKWARD":
                case "CONTROL_PAUSE":
                case "CONTROL_FORWARD":
                    {
                        ControlSlideshow(command);
                        break;
                    }
                case "REBOOT":
                    {
                        Helpers.RunProcess("reboot", "");
                        break;
                    }
                case "SHUTDOWN":
                    {
                        Helpers.RunProcess("shutdown", "");
                        break;
                    }
                case "EXITAPP":
                    {
                        // Close up shop we're going home
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            handleMainWindow.Close();
                            handleMainWindow.server.Stop();
                        });
                        throw new Exception("EXIT Called...exiting app!");
                        break;
                    }
                case "UTILITY_UPDATEFILELIST":
                    {
                        AppSettings.Default.RefreshDirctories = true;
                        AppSettings.Default.Save();
                        break;
                    }

                default:
                {
                         
                    break;
                }
            }
        } 
    }
}
