using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSWindowManager
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(WindowManagerPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)]
    public sealed class WindowManagerPackage : Package
    {
        /// <summary>
        /// WindowManagerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "1cc17f39-7039-467d-a9ad-ede432cb89ea";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleGuttersCommand"/> class.
        /// </summary>
        public WindowManagerPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            ToggleGuttersCommand.Initialize(this);
            HideRecentToolWindowCommands.Initialize(this);
            base.Initialize();

            AddWindowMgmtButtonToStatusBar();
        }

        #endregion

        #region Status Bar Button Init

        public static readonly Guid WindowManagerPackageCmdSetGuid = new Guid("04c55c1f-7f7d-482b-bc73-05fed05d9674");
        public const int WindowManagerMenuCmdId = 0x1030;

        private void AddWindowMgmtButtonToStatusBar()
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
                        dockPanel.Children.Insert(dockPanel.Children.Count - 1, WindowManagementStatusBar);
                    }
                }
            }
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

        #endregion

    }
}
