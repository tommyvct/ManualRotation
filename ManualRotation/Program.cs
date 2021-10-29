// Credit: https://docs.microsoft.com/en-us/uwp/api/windows.devices.sensors.simpleorientationsensor.orientationchanged?view=winrt-insider

using Windows.Devices.Sensors;
using Windows.Foundation;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace ManualRotation
{
    public class Program
    {
        private static SimpleOrientation previousOrientation;
        private static DateTime lastToast = DateTime.Now.AddSeconds(10);   
        private static bool toastsCleard = false;

        public const string ManualRotation = "ManualRotation"; 


        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "install":
                        Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)?
                          .SetValue(ManualRotation, "\"" + Environment.ProcessPath + "\"");
                        MessageBox.Show("Installed.", ManualRotation);
                        break;
                    case "remove":
                    case "uninstall":
                        Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)?
                            .DeleteValue(ManualRotation);
                        ToastNotificationManagerCompat.Uninstall();
                        MessageBox.Show("Uninstalled.", ManualRotation);
                        Environment.Exit(0);
                        break;
                    case "-h":
                    case "help":
                    case "--help":
                        var usage = new StringBuilder();                        
                        usage.AppendLine("Usage:");
                        usage.AppendLine("ManualRotation install");
                        usage.AppendLine("    Register for autostart for current user.");
                        usage.AppendLine("ManualRotation remove");
                        usage.AppendLine("    Deregister for autostart for current user.");
                        MessageBox.Show(usage.ToString(), ManualRotation);
                        Environment.Exit(0);
                        break;
                    default:
                        MessageBox.Show("Parameter can't be recognized.", ManualRotation);
                        Environment.Exit(0);
                        break;
                }
            }

            SimpleOrientationSensor? sensor = SimpleOrientationSensor.GetDefault();
            previousOrientation = sensor.GetCurrentOrientation();

            if (sensor == null)
            {
                Environment.Exit(1);
            }

            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                var args = ToastArguments.Parse(toastArgs.Argument);

                try
                {
                    switch (args["rotateTo"])
                    {
                        case "0":
                            RotationDriver.Rotate(1, RotationDriver.Orientations.DEGREES_CW_0);
                            previousOrientation = SimpleOrientation.NotRotated;
                            break;
                        case "1":
                            RotationDriver.Rotate(1, RotationDriver.Orientations.DEGREES_CW_90);
                            previousOrientation = SimpleOrientation.Rotated90DegreesCounterclockwise;
                            break;
                        case "2":
                            RotationDriver.Rotate(1, RotationDriver.Orientations.DEGREES_CW_180);
                            previousOrientation = SimpleOrientation.Rotated180DegreesCounterclockwise;
                            break;
                        case "3":
                            RotationDriver.Rotate(1, RotationDriver.Orientations.DEGREES_CW_270);
                            previousOrientation = SimpleOrientation.Rotated270DegreesCounterclockwise;
                            break;
                        default:
                            break;
                    }
                }
                catch (KeyNotFoundException) { }
            };

            sensor.OrientationChanged += new TypedEventHandler<
                SimpleOrientationSensor,
                SimpleOrientationSensorOrientationChangedEventArgs>(
                    (sender, e) =>
                    {
                        var orientation = e.Orientation;

                        var rawRegistry = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AutoRotation", "Enable", null);

                        if (rawRegistry == null)
                        {
                            Environment.Exit(2);
                        }

                        bool autoRotate = ((int)rawRegistry == 0) ? false : true;

                        if (orientation == previousOrientation || autoRotate)
                        {
                            return;
                        }

                        switch (orientation)
                        {
                            case SimpleOrientation.Facedown:
                            case SimpleOrientation.Faceup:
                                break;
                            default:
                                ShowToast(orientation);
                                break;
                        }
                    });

            while (true)
            {
                Thread.Sleep(2000);

                if (!toastsCleard)
                {
                    if ((DateTime.Now - lastToast).TotalSeconds > 5) // toasts older than 5 seconds are useless
                    {
                        ClearToast();
                    }
                }
            }
        }

        private static void ShowToast(SimpleOrientation rotateTo)
        {
            if (!toastsCleard)
            {
                ClearToast();
            }

            new ToastContentBuilder()
                .AddText("Rotation Lock is Engaged.")
                .AddText("Do you still want to rotate you screen?")
                .AddButton(new ToastButton()
                    .SetContent("Rotate")
                    .AddArgument("rotateTo", rotateTo)
                    .SetBackgroundActivation())
                .Show(toast =>
                {
                    toast.Priority = Windows.UI.Notifications.ToastNotificationPriority.High;
                    toast.ExpiresOnReboot = true;
                    toast.ExpirationTime = DateTime.Now.AddSeconds(5);
                });

            lastToast = DateTime.Now;
            toastsCleard = false;
        }

        private static void ClearToast()
        {
            ToastNotificationManagerCompat.History.Clear();
            toastsCleard = true;
        }
    }
}