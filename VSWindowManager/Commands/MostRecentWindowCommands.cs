using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSWindowManager
{
    internal class MostRecentWindowCommands
    {
        // Command Guid and IDs. These match with the symbol values in WindowManagerPackage.vsct
        public static readonly Guid WindowManagerPackageCmdSetGuid = new Guid("04c55c1f-7f7d-482b-bc73-05fed05d9674");
        public const int HideRecentToolWinGroupCmdId = 0x0110;
        public const int CloseRecentToolWinCmdId = 0x0120;
        public const int OpenRecentlyClosedToolWinCmdId = 0x0130;
        public const int ToggleVisibleWindowsCmdId = 0x0140;

        // Class constants
        private const int ENUM_LOOP_SIZE = 10;
        private const string START_PAGE_CAPTION = "Start Page";

        // Instance and package initialization
        private readonly Package package;

        // List of hidden windows to restore
        private List<IVsWindowFrame> _restoreWindows;

        private IServiceProvider ServiceProvider { get { return this.package; } }
        public static MostRecentWindowCommands Instance { get; private set; }

        public static void Initialize(Package package)
        {
            Instance = new MostRecentWindowCommands(package);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MostRecentWindowCommands"/> class.
        /// Adds our command handlers for menu commands declared in the VSCT file
        /// </summary>
        private MostRecentWindowCommands(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                commandService.AddCommand(GetCommand(HideRecentToolWinGroupCmdId, QueryStatusHideRecentToolWinGroup, HideMostRecentToolWindowGroup));
                commandService.AddCommand(GetCommand(CloseRecentToolWinCmdId, QueryStatusCloseRecentToolWin, CloseMostRecentToolWindow));
                commandService.AddCommand(GetCommand(OpenRecentlyClosedToolWinCmdId, QueryStatusOpenRecentlyClosedToolWin, OpenMostRecentlyClosedToolWin));
                commandService.AddCommand(GetCommand(ToggleVisibleWindowsCmdId, QueryStatusToggleVisibleWindowsCmdId, ToggleVisibleWindows));
            }
        }

        private OleMenuCommand GetCommand(int cmdId, EventHandler queryStatusHandler, EventHandler invokeHandler)
        {
            OleMenuCommand command = new OleMenuCommand(invokeHandler, new CommandID(WindowManagerPackageCmdSetGuid, cmdId));
            command.BeforeQueryStatus += queryStatusHandler;
            return command;
        }

        private void QueryStatusCloseRecentToolWin(object sender, EventArgs e)
        {
            // Only enable the command if there is an Open window
            EnableCommandIfTrue(sender, GetMostRecentToolWindow(bFindOpenWindow: true) != null);
        }

        private void QueryStatusHideRecentToolWinGroup(object sender, EventArgs e)
        {
            // Only enable the command if there is an Open window
            EnableCommandIfTrue(sender, GetMostRecentToolWindow(bFindOpenWindow: true) != null);
        }

        private void QueryStatusOpenRecentlyClosedToolWin(object sender, EventArgs e)
        {
            // Only enable the command if there is a closed window in the history
            EnableCommandIfTrue(sender, GetMostRecentlyClosedWindow() != null);
        }

        private void QueryStatusToggleVisibleWindowsCmdId(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Visible = true;
            command.Enabled = true;
            command.Text = ShouldRestoreWindows() ? "&Restore Hidden Windows" : "Hide &All Windows";
        }

        private bool ShouldRestoreWindows()
        {
            return HasRestorableWindows() && !HasVisibleWindows();
        }

        private bool HasRestorableWindows()
        {
            return _restoreWindows != null && _restoreWindows.Count > 0;
        }

        private bool HasVisibleWindows()
        {
            return GetMostRecentToolWindow() != null;
        }

        private static void EnableCommandIfTrue(object sender, bool bEnable)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Visible = true;
            command.Enabled = bEnable;
        }

        private void ToggleVisibleWindows(object sender, EventArgs e)
        {
            bool shouldHideWindows = !ShouldRestoreWindows();
            if (shouldHideWindows)
            {
                // Populate the restore windows list - for later restoring
                _restoreWindows = GetVisibleWindows();
                // Hide all visible windows
                _restoreWindows.ForEach(x => x.Hide());
            }
            else
            {
                // Restore previously hidden windows
                _restoreWindows.ForEach(x => x.Show());
                // Clear the hidden windows stack
                _restoreWindows = null;
            }
        }

        private List<IVsWindowFrame> GetVisibleWindows()
        {
            IVsUIShell shell = (IVsUIShell)ServiceProvider.GetService(typeof(IVsUIShell));
            shell.GetToolWindowEnum(out IEnumWindowFrames windowFrames);

            List<IVsWindowFrame> visibleWindows = new List<IVsWindowFrame>();

            // Loop through the enum of tool windows. Must be fetched in groups (ie. 10 at a time)
            IVsWindowFrame[] windowFrameArray = new IVsWindowFrame[ENUM_LOOP_SIZE];
            while (windowFrames.Next(ENUM_LOOP_SIZE, windowFrameArray, out var fetchedCount) >= 0)  // TODO Check this.
            {
                for (int i = 0; i < fetchedCount; i++)
                {
                    // Get all visible windows (not Start Page)
                    IVsWindowFrame windowFrame = windowFrameArray[i];

                    // Ignore the Start Page. It's a Tool Window - but not really.
                    if (IsStartPage(windowFrame)) continue;
                    // Skip if not visible
                    if (!IsVisibleWindow(windowFrame)) continue;

                    // Found a visible window. Add it to the list.
                    visibleWindows.Add(windowFrame);
                }

                // Break if there are no more items in the ENUM
                if (fetchedCount < ENUM_LOOP_SIZE)
                {
                    break;
                }
            }

            return visibleWindows;
        }

        private void CloseMostRecentToolWindow(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame = GetMostRecentToolWindow(bFindOpenWindow: true);
            if (windowFrame == null) return;

            // Perform Close Window operation
            System.Diagnostics.Debug.WriteLine($"Closing window: {GetWindowTitle(windowFrame)}");
            windowFrame.Hide();
        }

        /// <summary>
        /// Perform Hide Window Group operation
        /// </summary>
        private void HideMostRecentToolWindowGroup(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame = GetMostRecentToolWindow(bFindOpenWindow: true);
            if (windowFrame == null) return;

            // HACK! Internal code for setting FrameMode property will set to Docked if already in AutoHide mode.
            // To avoid this, ensure that the window's current FrameMode is not AutoHide. (Set to Dock if necessary.)
            if (IsAutoHideWindow(windowFrame))
            {
                // Temporarily Dock
                windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
            }

            // Hide window (Set to auto-hide)
            System.Diagnostics.Debug.WriteLine($"Hiding window: {GetWindowTitle(windowFrame)}");
            windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE2.VSFM_AutoHide);
        }

        /// <summary>
        /// Open the most recent tool window that is not visible
        /// </summary>
        private void OpenMostRecentlyClosedToolWin(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame = GetMostRecentlyClosedWindow();
            if (windowFrame == null) return;
            
            // Show the window (whether docked or overlayed)
            System.Diagnostics.Debug.WriteLine($"Opening window: {GetWindowTitle(windowFrame)}");
            windowFrame.Show();
        }

        //-------- Helper / Utility methods --------//

        private IVsWindowFrame GetMostRecentToolWindow(bool bFindOpenWindow = true)
        {
            IVsUIShell shell = (IVsUIShell)ServiceProvider.GetService(typeof(IVsUIShell));
            shell.GetToolWindowEnum(out IEnumWindowFrames windowFrames);

            // Loop through the enum of tool windows. Must be fetched in groups (ie. 10 at a time)
            IVsWindowFrame[] windowFrameArray = new IVsWindowFrame[ENUM_LOOP_SIZE];
            while (windowFrames.Next(ENUM_LOOP_SIZE, windowFrameArray, out var fetchedCount) >= 0)  // TODO Check this.
            {
                for (int i = 0; i < fetchedCount; i++)
                {
                    // Look for the first window that is not visible and not the Start Page
                    IVsWindowFrame windowFrame = windowFrameArray[i];

                    // Ignore the Start Page. It's a Tool Window - but not really.
                    if (IsStartPage(windowFrame)) continue;

                    // Switch: Are we looking for open windows or closed windows?
                    if (bFindOpenWindow)
                    {
                        // Skip if not visible
                        if (!IsVisibleWindow(windowFrame)) continue;
                    }
                    else
                    {
                        // Skip if Visible or if AutoHide Window
                        if (IsVisibleWindow(windowFrame) || IsAutoHideWindow(windowFrame)) continue;
                    }

                    // Found one. Return it.
                    return windowFrame;
                }

                // Break if there are no more items in the ENUM
                if (fetchedCount < ENUM_LOOP_SIZE)
                {
                    break;
                }
            }

            // None found.
            return null;
        }

        private IVsWindowFrame GetMostRecentlyClosedWindow()
        {
            IVsUIShell shell = (IVsUIShell)ServiceProvider.GetService(typeof(IVsUIShell));
            shell.GetToolWindowEnum(out IEnumWindowFrames windowFrames);

            IVsWindowFrame lastClosedWindow = null;
            // Loop through the enum of tool windows. Must be fetched in groups (ie. 10 at a time)
            IVsWindowFrame[] windowFrameArray = new IVsWindowFrame[ENUM_LOOP_SIZE];
            while (windowFrames.Next(ENUM_LOOP_SIZE, windowFrameArray, out var fetchedCount) >= 0)  // TODO Check this.
            {
                for (int i = 0; i < fetchedCount; i++)
                {
                    // Look for the first window that is not visible and not the Start Page
                    IVsWindowFrame windowFrame = windowFrameArray[i];
                    // Ignore the Start Page. It's a Tool Window - but not really.
                    if (IsStartPage(windowFrame)) continue;

                    // We are looking for a window that is not visible and not in the AutoHide tray
                    bool isVisibleWindow = IsVisibleWindow(windowFrame);
                    bool isAutoHideWindow = IsAutoHideWindow(windowFrame);
                    if (!isVisibleWindow && !isAutoHideWindow)
                    {
                        // Found one. But is it the last closed? Mark it for now, but keep checking.
                        lastClosedWindow = windowFrame;
                    }
                    else
                    {
                        if (lastClosedWindow != null)
                        {
                            return lastClosedWindow;
                        }
                    }

                }

                // Break if there are no more items in the ENUM
                if (fetchedCount < ENUM_LOOP_SIZE)
                {
                    break;
                }
            }

            return lastClosedWindow;
        }

        private static bool IsVisibleWindow(IVsWindowFrame windowFrame)
        {
            windowFrame.IsOnScreen(out int bIsOnScreen);
            return bIsOnScreen == 1;
        }

        private static bool IsStartPage(IVsWindowFrame windowFrame)
        {
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ShortCaption, out var caption);
            return ((string)caption).Equals(START_PAGE_CAPTION);
        }

        private static string GetWindowTitle(IVsWindowFrame windowFrame)
        {
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ShortCaption, out var caption);
            return (string)caption;
        }

        private static bool IsAutoHideWindow(IVsWindowFrame windowFrame)
        {
            windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, out var existingAutoHideMode);
            return (int)existingAutoHideMode == (int)VSFRAMEMODE2.VSFM_AutoHide;
        }

    }
}