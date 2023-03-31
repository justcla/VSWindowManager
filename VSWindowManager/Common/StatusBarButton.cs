using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSWindowManager
{
    class StatusBarButton
    {
        private static WindowManagerCompartmentViewModel viewModel;
        private static WindowManagerCompartment compartment = new WindowManagerCompartment();
        private static StatusBar windowManagementStatusBar;
        private static bool addedStatusBarIcon = false;

        public static readonly Guid WindowManagerPackageCmdSetGuid = new Guid("04c55c1f-7f7d-482b-bc73-05fed05d9674");
        public const int WindowManagerMenuCmdId = 0x1030;

        public static void Initialize()
        {
            InitializeStatusBar();

        }

        private static void InitializeStatusBar()
        {
            // Add the Window Management status bar to the Status Bar
            AddWindowManagementStatusBar();
        }

        private static void AddWindowManagementStatusBar()
        {
            // Find the status bar dock panel
            DockPanel statusBarDockPanel = GetStatusBarDockPanel();
            if (statusBarDockPanel == null)
            {
                System.Diagnostics.Debug.WriteLine("Error: Could not find status bar dock panel, therefore cannot add new button to status bar.");
                return;
            }

            // Create the new status bar button in a new status bar
            if (windowManagementStatusBar == null)
            {
                windowManagementStatusBar = CreateStatusBar();
            }

            // Add the Window Management status bar to the Status Bar dock panel at position 0 (far left)
            statusBarDockPanel.Children.Insert(0, windowManagementStatusBar);
            addedStatusBarIcon = true;
        }

        private static DockPanel GetStatusBarDockPanel()
        {
            // Code fix contributed by Zhang Chen (GitHub user: zc910704)
            try
            {
                DependencyObject rootGrid = VisualTreeHelper.GetChild(Application.Current.MainWindow, 0);
                DependencyObject mainChild = VisualTreeHelper.GetChild(rootGrid, 0);
                DependencyObject primaryObj = VisualTreeHelper.GetChild(mainChild, 0);
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(primaryObj); i++)
                {
                    DependencyObject o = VisualTreeHelper.GetChild(primaryObj, i);
                    if (o != null && o is DockPanel)
                    {
                        DockPanel dockPanel = o as DockPanel;
                        if (dockPanel.Name == "StatusBarPanel")
                        {
                            return dockPanel;
                        }
                    }
                }
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception occurred while attempting to locate status bar.", e);
            }
            return null;
        }

        private static StatusBar CreateStatusBar()
        {
            // Create the status bar button and bind the content and mouse events
            StatusBarItem statusBarItem = new StatusBarItem();
            statusBarItem.Content = GetItemContent();

            // Add the status bar button to a new status bar
            StatusBar sb = new StatusBar() { Background = Brushes.Transparent };
            sb.Items.Add(statusBarItem);
            return sb;
        }

        private static object GetItemContent()
        {
            viewModel = new WindowManagerCompartmentViewModel();
            compartment = new WindowManagerCompartment();

            viewModel.Visible = true;
            viewModel.Icon = KnownMonikers.DockPanel;
            viewModel.CompartmentClicked += ShowContextMenu;
            string shortcutKeys = GetWindowToolsShortcut();
            viewModel.ToolTip = @"Tool Windows" + shortcutKeys;

            compartment.Width = 20;
            compartment.HorizontalAlignment = HorizontalAlignment.Stretch;
            compartment.VerticalAlignment = VerticalAlignment.Stretch;
            compartment.DataContext = viewModel;

            return compartment;
        }

        private static string GetWindowToolsShortcut()
        {
            // TODO: Find current shortcut for Window.ShowWindowManagementTools
            return @"  (Ctrl+\, Ctrl+W)";
        }

        private static void ShowContextMenu(object sender, WindowManagerCompartmentClickedEventArgs args)
        {
            LaunchWindowToolsContextMenu();
        }

        public static void LaunchWindowToolsContextMenu()
        {
            // Initialize(refresh) the OtherRecentWindows list here so that it doesn't have to be built repeatedly during each QueryStatus
            MostRecentWindowCommands.Instance.PopulateOtherRecentWindowsList();

            // Display Window manager menu
            if (!addedStatusBarIcon)
            {
                AddWindowManagementStatusBar();
            }
            IVsUIShell uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
            if (uiShell != null)
            {
                Rect position = viewModel.Position;

                POINTS[] p = new POINTS[1];
                p[0] = new POINTS();
                p[0].x = (short)position.TopLeft.X;
                p[0].y = (short)position.TopLeft.Y;
                Guid guidSccDisplayInformationCommandSet = WindowManagerPackageCmdSetGuid;
                uiShell.ShowContextMenu(0, ref guidSccDisplayInformationCommandSet, WindowManagerMenuCmdId, p, null);
            }
        }
    }
}
