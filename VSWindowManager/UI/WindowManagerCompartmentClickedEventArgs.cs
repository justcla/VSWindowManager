using System;
using System.Windows;

namespace VSWindowManager
{
    /// <summary>
    /// EventArgs for when a Window manager compartment is clicked
    /// </summary>
    public class WindowManagerCompartmentClickedEventArgs : EventArgs
    {
        public WindowManagerCompartmentClickedEventArgs(Rect clickedElementPosition)
        {
            this.ClickedElementPosition = clickedElementPosition;
        }

        /// <summary>
        /// The screen position of the rectangle that was clicked in absolute coordinates
        /// </summary>
        public Rect ClickedElementPosition { get; }
    }
}
