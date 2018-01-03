using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

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
            base.Initialize();

            DependencyObject dd = VisualTreeHelper.GetChild(Application.Current.MainWindow, 0);
            DependencyObject ddd = VisualTreeHelper.GetChild(dd, 0);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(ddd); i++)
            {
                object o = VisualTreeHelper.GetChild(ddd, i);
                if (o != null && o is DockPanel)
                {
                    DockPanel d = o as DockPanel;
                    if (d.Name == "StatusBarPanel")
                    {
                        d.Children.Insert(d.Children.Count - 1, WindowManagementStatusBar);
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
                    StatusBar sb = new StatusBar() { Width = 100, Background = Brushes.Transparent };
                    StatusBarItem item = new StatusBarItem() { Width = 100 };
                    item.Content = new TextBlock() { Text = "Window Management", Background = Brushes.Transparent, Foreground = Brushes.White };
                    item.MouseUp += Item_MouseUp;
                    sb.Items.Add(item);

                    _statusBar = sb;
                }

                return _statusBar;
            }
        }

        private StatusBar _statusBar;

        private void Item_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IVsUIShell shell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell != null)
            {
                POINTS[] p = new POINTS[1];
                p[0] = new POINTS();
                p[0].x = 0;
                p[0].y = 0;

                Guid g = new Guid("{04c55c1f-7f7d-482b-bc73-05fed05d9674}");
                uint id = 0x1030;
                if (ErrorHandler.Failed(shell.ShowContextMenu(0, ref g, (int)id, p, null)))
                {
                    MessageBox.Show("Failed to show context menu");
                }
            }
        }

        #endregion
    }
}
