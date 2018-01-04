using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSWindowManager
{
    internal class HideRecentToolWindowCommands
    {
        /// <summary>
        /// Command IDs.
        /// </summary>
        public const int HideRecentToolWinGroupCmdId = 0x0110;
        public const int CloseRecentToolWinCmdId = 0x0120;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("04c55c1f-7f7d-482b-bc73-05fed05d9674");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="HideRecentToolWindowCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private HideRecentToolWindowCommands(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                commandService.AddCommand(CreateOleMenuCommand(HideRecentToolWinGroupCmdId, HideMostRecentToolWindowGroup));
                commandService.AddCommand(CreateOleMenuCommand(CloseRecentToolWinCmdId, CloseMostRecentToolWindow));
            }

        }

        private OleMenuCommand CreateOleMenuCommand(int cmdId, EventHandler invokeHandler)
        {
            return new OleMenuCommand(invokeHandler, new CommandID(CommandSet, cmdId));
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HideRecentToolWindowCommands Instance
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
            Instance = new HideRecentToolWindowCommands(package);
        }

        private void CloseMostRecentToolWindow(object sender, EventArgs e)
        {
            CloseOrHideWindow(bCloseWindow: true);
        }

        private void HideMostRecentToolWindowGroup(object sender, EventArgs e)
        {
            CloseOrHideWindow(bCloseWindow: false);
        }

        private void CloseOrHideWindow(bool bCloseWindow)
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
                    if (((string)caption).Equals("Start Page"))
                    {
                        continue;
                    }

                    // Only pay attention to visible windows
                    windowFrame.IsOnScreen(out int bIsOnScreen);
                    if (bIsOnScreen != 1)
                    {
                        continue;
                    }

                    // Found an active window.
                    System.Diagnostics.Debug.WriteLine($"Hiding window: {caption}. Operation: {(bCloseWindow ? "Close" : "Hide Group")}");

                    // Hide Group or Close Window?
                    if (bCloseWindow)
                    {
                        // Perform Close Window operation
                        windowFrame.Hide();
                        break;
                    }
                    else // Perform Hide Window Group operation
                    {
                        // HACK! Internal code for setting FrameMode property will set to Docked if already in AutoHide mode.
                        // To avoid this, ensure that the window's current FrameMode is not AutoHide. (Set to Dock if necessary.)
                        windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, out var existingAutoHideMode);
                        if ((int)existingAutoHideMode == (int)VSFRAMEMODE2.VSFM_AutoHide)
                        {
                            // Temporarily Dock
                            windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
                        }

                        // Hide window (Set to auto-hide)
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