using H.NotifyIcon.Core;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;
using Windows.Win32;
using WinRT.Interop;
using WinUIEx.Messaging;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.UI;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Input;
using Windows.Devices.Input;

namespace spooWASDK.Classes
{
    public sealed partial class TrayIcon
    {
        public static void LaunchTrayIcon(WindowEx window)
        {
            var icon = new TrayIconWithContextMenu();
            icon.MessageWindow.MouseEventReceived += (sender, e) =>
            {
                if (e.MouseEvent == MouseEvent.IconLeftMouseDown)
                {

                }
            };
            icon.Create();
        }
    }
}
