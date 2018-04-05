using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
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
        public const int RecentToolWindow1CmdId = 0x0310;
        public const int RecentToolWindow2CmdId = 0x0320;
        public const int RecentToolWindow3CmdId = 0x0330;
        public const int RecentToolWindow4CmdId = 0x0340;
        public const int RecentToolWindow5CmdId = 0x0350;
        public const int RecentToolWindow6CmdId = 0x0360;
        public const int RecentToolWindow7CmdId = 0x0370;
        public const int RecentToolWindow8CmdId = 0x0380;
        public const int RecentToolWindow9CmdId = 0x0390;

        // Class constants
        private const int ENUM_LOOP_SIZE = 10;
        private const string START_PAGE_CAPTION = "Start Page";

        // Instance and package initialization
        private readonly Package package;

        // List of hidden windows to restore
        private List<IVsWindowFrame> _restoreWindows;
        private List<IVsWindowFrame> _otherRecentWindows;

        private IServiceProvider ServiceProvider { get { return this.package; } }
        public static MostRecentWindowCommands Instance { get; private set; }

        public static void Initialize(Package package)
        {
            Instance = new MostRecentWindowCommands(package);
        }

        internal void PopulateOtherRecentWindowsList()
        {
            List<IVsWindowFrame> allToolWindows = GetAllToolWindows();
            _otherRecentWindows = allToolWindows.FindAll((windowFrame) => !IsPopularWindow(windowFrame));
        }

        private bool IsPopularWindow(IVsWindowFrame windowFrame)
        {
            // TODO: Compare window GUIDs, instead of titles
            List<string> popularWindows = new List<string>() { "Solution Explorer", "Error List", "Output" };
            return popularWindows.Contains(GetWindowTitle(windowFrame));
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
                // Add commands for the recent windows
                commandService.AddCommand(GetCommand(RecentToolWindow1CmdId, QueryStatusRecentToolWindow1, OpenRecentToolWindow1));
                commandService.AddCommand(GetCommand(RecentToolWindow2CmdId, QueryStatusRecentToolWindow2, OpenRecentToolWindow2));
                commandService.AddCommand(GetCommand(RecentToolWindow3CmdId, QueryStatusRecentToolWindow3, OpenRecentToolWindow3));
                commandService.AddCommand(GetCommand(RecentToolWindow4CmdId, QueryStatusRecentToolWindow4, OpenRecentToolWindow4));
                commandService.AddCommand(GetCommand(RecentToolWindow5CmdId, QueryStatusRecentToolWindow5, OpenRecentToolWindow5));
                commandService.AddCommand(GetCommand(RecentToolWindow6CmdId, QueryStatusRecentToolWindow6, OpenRecentToolWindow6));
                commandService.AddCommand(GetCommand(RecentToolWindow7CmdId, QueryStatusRecentToolWindow7, OpenRecentToolWindow7));
                commandService.AddCommand(GetCommand(RecentToolWindow8CmdId, QueryStatusRecentToolWindow8, OpenRecentToolWindow8));
                commandService.AddCommand(GetCommand(RecentToolWindow9CmdId, QueryStatusRecentToolWindow9, OpenRecentToolWindow9));
            }
        }

        private void OpenRecentToolWindow1(object sender, EventArgs e) { OpenRecentToolWindow(1); }
        private void OpenRecentToolWindow2(object sender, EventArgs e) { OpenRecentToolWindow(2); }
        private void OpenRecentToolWindow3(object sender, EventArgs e) { OpenRecentToolWindow(3); }
        private void OpenRecentToolWindow4(object sender, EventArgs e) { OpenRecentToolWindow(4); }
        private void OpenRecentToolWindow5(object sender, EventArgs e) { OpenRecentToolWindow(5); }
        private void OpenRecentToolWindow6(object sender, EventArgs e) { OpenRecentToolWindow(6); }
        private void OpenRecentToolWindow7(object sender, EventArgs e) { OpenRecentToolWindow(7); }
        private void OpenRecentToolWindow8(object sender, EventArgs e) { OpenRecentToolWindow(8); }
        private void OpenRecentToolWindow9(object sender, EventArgs e) { OpenRecentToolWindow(9); }

        private void OpenRecentToolWindow(int num)
        {
            _otherRecentWindows[num-1].Show();
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
            //EnableCommandIfTrue(sender, GetMostRecentToolWindow(bFindOpenWindow: false) != null);
            IVsWindowFrame lastClosedWindow = GetMostRecentToolWindow(bFindOpenWindow: false);
            bool hasLastClosedWindow = lastClosedWindow != null;
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Visible = hasLastClosedWindow;
            command.Enabled = hasLastClosedWindow;
            command.Text = hasLastClosedWindow ? $"&Re-open {GetWindowTitle(lastClosedWindow)}" : "(Disabled)";
        }

        private void QueryStatusToggleVisibleWindowsCmdId(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Visible = true;
            command.Enabled = true;
            // TODO: Localise for EN-US and other non-traditional EN languages.
            command.Text = ShouldRestoreWindows() ? "Undo Mi&nimise All Windows" : "Mi&nimise All Windows";
        }

        private void QueryStatusRecentToolWindow1(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 1); }
        private void QueryStatusRecentToolWindow2(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 2); }
        private void QueryStatusRecentToolWindow3(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 3); }
        private void QueryStatusRecentToolWindow4(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 4); }
        private void QueryStatusRecentToolWindow5(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 5); }
        private void QueryStatusRecentToolWindow6(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 6); }
        private void QueryStatusRecentToolWindow7(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 7); }
        private void QueryStatusRecentToolWindow8(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 8); }
        private void QueryStatusRecentToolWindow9(object sender, EventArgs e) { QueryStatusRecentToolWindow(sender, 9); }

        private void QueryStatusRecentToolWindow(object sender, int num)
        {
            bool bEnable = HasOtherRecentWindow(num);
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Visible = bEnable;
            command.Enabled = bEnable;
            command.Text = GetTextForRecentWindowCommand(num, bEnable);
        }

        private string GetTextForRecentWindowCommand(int num, bool bEnabled)
        {
            string titleText = bEnabled ? GetWindowTitle(_otherRecentWindows[num - 1]) : "(Disabled)";
            return $"&{num}: {titleText}";
        }

        private bool HasOtherRecentWindow(int num)
        {
            // Assume the OtherRecentWindows list will be populated when the context menu is opened. See StatusBarButton.ShowContextMenu
            return _otherRecentWindows.Count >= num;
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
                List<IVsWindowFrame> visibleWindows = GetVisibleWindows();
                // Only restore windows that are docked (not AutoHide)
                _restoreWindows = visibleWindows.FindAll(windowFrame => !IsAutoHideWindow(windowFrame));

                // Hide all visible windows (including any visible auto-hide windows)
                visibleWindows.ForEach((windowFrame) =>
                {
                    HideWindow(windowFrame);
                });
            }
            else
            {
                // Restore previously hidden windows
                _restoreWindows.ForEach((windowFrame) =>
                {
                    DockWindow(windowFrame);
                });
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
            HideWindow(windowFrame);
        }

        private static void HideWindow(IVsWindowFrame windowFrame)
        {
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

        private static void DockWindow(IVsWindowFrame windowFrame)
        {
            if (windowFrame == null) return;
            windowFrame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
        }

        /// <summary>
        /// Open the most recent tool window that is not visible
        /// </summary>
        private void OpenMostRecentlyClosedToolWin(object sender, EventArgs e)
        {
            IVsWindowFrame windowFrame = GetMostRecentToolWindow(bFindOpenWindow: false);
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

            IVsWindowFrame recentWindow = null;

            // Loop through the enum of tool windows. Must be fetched in groups (ie. 10 at a time)
            IVsWindowFrame[] windowFrameArray = new IVsWindowFrame[ENUM_LOOP_SIZE];
            while (windowFrames.Next(ENUM_LOOP_SIZE, windowFrameArray, out var fetchedCount) >= 0)  // TODO Check this.
            {
                for (int i = 0; i < fetchedCount; i++)
                {
                    IVsWindowFrame windowFrame = windowFrameArray[i];

                    // Ignore the Start Page. It's a Tool Window - but not really.
                    if (IsStartPage(windowFrame)) continue;

                    // Switch: Are we looking for open windows or closed windows?
                    if (bFindOpenWindow)
                    {
                        // Skip if not visible
                        if (IsVisibleWindow(windowFrame))
                        {
                            return windowFrame;
                        }
                    }
                    else
                    {
                        // We are looking for a window that is not visible and not in the AutoHide tray
                        if (IsClosedWindow(windowFrame))
                        {
                            // Found one. But is it the last closed? Mark it for now, but keep checking.
                            recentWindow = windowFrame;
                        }
                        else
                        {
                            if (recentWindow != null)
                            {
                                return recentWindow;
                            }
                        }
                    }
                }

                // Break if there are no more items in the ENUM
                if (fetchedCount < ENUM_LOOP_SIZE)
                {
                    break;
                }
            }

            return recentWindow;
        }

        private List<IVsWindowFrame> GetAllToolWindows()
        {

            IVsUIShell shell = (IVsUIShell)ServiceProvider.GetService(typeof(IVsUIShell));
            shell.GetToolWindowEnum(out IEnumWindowFrames windowFrames);

            // Loop through the enum of tool windows. Must be fetched in groups (ie. 10 at a time)
            List<IVsWindowFrame> toolWindows = new List<IVsWindowFrame>();
            IVsWindowFrame[] windowFrameArray = new IVsWindowFrame[ENUM_LOOP_SIZE];
            while (windowFrames.Next(ENUM_LOOP_SIZE, windowFrameArray, out var fetchedCount) >= 0)  // TODO Check this.
            {
                for (int i = 0; i < fetchedCount; i++)
                {
                    IVsWindowFrame windowFrame = windowFrameArray[i];
                    // Ignore the Start Page. It's a Tool Window - but not really.
                    if (IsStartPage(windowFrame)) continue;
                    toolWindows.Add(windowFrame);
                }

                // Break if there are no more items in the ENUM
                if (fetchedCount < ENUM_LOOP_SIZE)
                {
                    break;
                }
            }

            return toolWindows;
        }

        private static bool IsClosedWindow(IVsWindowFrame windowFrame)
        {
            return windowFrame.IsVisible() == VSConstants.S_FALSE;
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