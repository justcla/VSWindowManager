using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSWindowManager
{
    class StatusBarButton
    {
        public static readonly Guid WindowManagerPackageCmdSetGuid = new Guid("04c55c1f-7f7d-482b-bc73-05fed05d9674");
        public const int WindowManagerMenuCmdId = 0x1030;

        public void Initialize()
        {
            DockPanel statusBarDockPanel = GetStatusBarDockPanel();
            if (statusBarDockPanel == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: Could not find status bar dock panel, therefore cannot add new button to status bar.");
                return;
            }

            // Add the Window Management status bar to the Status Bar dock panel at position 0 (far left)
            statusBarDockPanel.Children.Insert(0, WindowManagementStatusBar);

        }

        private DockPanel GetStatusBarDockPanel()
        {
            DependencyObject dd = VisualTreeHelper.GetChild(Application.Current.MainWindow, 0);
            DependencyObject ddd = VisualTreeHelper.GetChild(dd, 0);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(ddd); i++)
            {
                object o = VisualTreeHelper.GetChild(ddd, i);
                if (o != null && o is DockPanel)
                {
                    DockPanel dockPanel = o as DockPanel;
                    if (dockPanel.Name == "StatusBarPanel")
                    {
                        return dockPanel;
                    }
                }
            }
            return null;
        }

        private StatusBar WindowManagementStatusBar
        {
            get
            {
                if (_statusBar == null)
                {
                    // Create the status bar button and bind the content and mouse events
                    StatusBarItem statusBarItem = new StatusBarItem() { Width = 100 };
                    statusBarItem.Content = GetItemContent();
                    statusBarItem.MouseUp += ShowWindowManagementContextMenu;

                    // Add the status bar button to a new status bar
                    StatusBar sb = new StatusBar() { Width = 100, Background = Brushes.Transparent };
                    sb.Items.Add(statusBarItem);

                    _statusBar = sb;
                }

                return _statusBar;
            }
        }

        private StatusBar _statusBar;

        private static object GetItemContent()
        {
            // TODO: Have this return an image moniker for an icon
            return new TextBlock() { Text = "Window Management", Background = Brushes.Transparent, Foreground = Brushes.White };
        }

        private void ShowWindowManagementContextMenu(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IVsUIShell shell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell != null)
            {
                POINTS[] p = new POINTS[1];
                p[0] = new POINTS();
                p[0].x = 0;
                p[0].y = 0;

                Guid CmdSetGuid = WindowManagerPackageCmdSetGuid;
                uint cmdId = WindowManagerMenuCmdId;
                if (ErrorHandler.Failed(shell.ShowContextMenu(0, ref CmdSetGuid, (int)cmdId, p, null)))
                {
                    MessageBox.Show("Failed to show context menu");
                }
            }
        }

    }
}
