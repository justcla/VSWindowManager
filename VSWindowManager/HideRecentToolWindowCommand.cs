using System;
using System.ComponentModel.Design;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSWindowManager
{
    internal class HideRecentToolWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0110;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("04c55c1f-7f7d-482b-bc73-05fed05d9674");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="HideRecentToolWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private HideRecentToolWindowCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }

        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HideRecentToolWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new HideRecentToolWindowCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            IVsUIShell shell = (IVsUIShell)ServiceProvider.GetService(typeof(IVsUIShell));
            shell.GetToolWindowEnum(out IEnumWindowFrames windowFrames);
            IVsWindowFrame[] windowFrameArray = new IVsWindowFrame[10];
            while (windowFrames.Next(10, windowFrameArray, out var fetchedCount) >= 0)  // TODO Check this.
            {
                for (int i = 0; i < fetchedCount; i++)
                {
                    IVsWindowFrame windowFrame = windowFrameArray[i];
                    windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ShortCaption, out var caption);
                    //windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Type, out var windowType);
                    //System.Diagnostics.Debug.WriteLine($"Caption: {caption} Type: {windowType}");

                    // Skip over the Start Page. It's a Tool Window - but not really.
                    if (((string)caption).Equals("Start Page")) {
                        continue;
                    }

                    // Only pay attention to visible windows
                    windowFrame.IsOnScreen(out int bIsOnScreen);
                    if (bIsOnScreen == 1)
                    {
                        // Found an active window. AutoHide it.
                        System.Diagnostics.Debug.WriteLine($"Hiding window: {caption}");

                        // HACK! Internal code for setting FrameMode property will set to Docked if already in AutoHide mode.
                        // To avoid this, ensure that the window's current FrameMode is not AutoHide. (Set to Dock if necessary.)
                        windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, out var existingAutoHideMode);
                        if ((int)existingAutoHideMode == (int)VSFRAMEMODE2.VSFM_AutoHide)
                        {
                            // Temporarily Dock
                            windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
                        }

                        windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE2.VSFM_AutoHide);
                        break;
                    }
                }

                if (fetchedCount < 10)
                {
                    break;
                }
            }
        }
    }
}