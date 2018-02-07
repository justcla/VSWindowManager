using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

        public static void Initialize()
        {
            // Find the status bar dock panel
            DockPanel statusBarDockPanel = GetStatusBarDockPanel();
            if (statusBarDockPanel == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: Could not find status bar dock panel, therefore cannot add new button to status bar.");
                return;
            }

            // Create the new status bar button in a new status bar
            StatusBar windowManagementStatusBar = CreateStatusBar();

            // Add the Window Management status bar to the Status Bar dock panel at position 0 (far left)
            statusBarDockPanel.Children.Insert(0, windowManagementStatusBar);

        }

        private static DockPanel GetStatusBarDockPanel()
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

        private static StatusBar CreateStatusBar()
        {
            // Create the status bar button and bind the content and mouse events
            StatusBarItem statusBarItem = new StatusBarItem() { Width = 100 };
            statusBarItem.Content = GetItemContent();
            statusBarItem.MouseUp += ShowContextMenu;

            // Add the status bar button to a new status bar
            StatusBar sb = new StatusBar() { Width = 100, Background = Brushes.Transparent };
            sb.Items.Add(statusBarItem);
            return sb;
        }

        private static object GetItemContent()
        {
            // TODO: Have this return an image moniker for an icon
            return new TextBlock() { Text = "Window Management", Background = Brushes.Transparent, Foreground = Brushes.White };
        }

        private static void ShowContextMenu(object sender, MouseButtonEventArgs mouseButtonEvent)
        {
            FrameworkElement frameworkElement = (FrameworkElement)sender;
            IVsUIShell uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

            Guid cmdSetGuid = WindowManagerPackageCmdSetGuid;
            POINTS[] points = GetPointsFromMouseEvent(frameworkElement, mouseButtonEvent);

            if (ErrorHandler.Failed(uiShell.ShowContextMenu(0, ref cmdSetGuid, WindowManagerMenuCmdId, points, null)))
            {
                MessageBox.Show("Failed to show context menu");
            }
        }

        private static POINTS[] GetPointsFromMouseEvent(FrameworkElement frameworkElement, MouseButtonEventArgs mouseButtonEvent)
        {
            Point relativePoint = mouseButtonEvent.GetPosition(frameworkElement);
            Point screenPoint = frameworkElement.PointToScreen(relativePoint);
            POINTS point = new POINTS();
            point.x = (short)screenPoint.X;
            point.y = (short)screenPoint.Y;
            return new[] { point };
        }

    }
}
