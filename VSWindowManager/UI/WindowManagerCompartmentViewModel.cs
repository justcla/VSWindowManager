using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Imaging.Interop;
using System;
using System.Windows;

namespace VSWindowManager
{
    /// <summary>
    /// View model for the WindowManager compartment
    /// </summary>
    public class WindowManagerCompartmentViewModel : ObservableObject
    {
        /// <summary>
        /// The icon to be displayed on the SCC compartment
        /// </summary>
        public ImageMoniker Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        private ImageMoniker _icon;

        /// <summary>
        /// The tooltip to be displayed on the SCC compartment
        /// </summary>
        public string ToolTip
        {
            get { return _toolTip; }
            set { SetProperty(ref _toolTip, value); }
        }

        private string _toolTip;

        /// <summary>
        /// Controls the visibility of the SCC compartment
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
            set { SetProperty(ref _visible, value); }
        }

        private bool _visible;

        /// <summary>
        /// Current absolute position of the control
        /// </summary>
        public Rect Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        private Rect _position;

        /// <summary>
        /// Event raised when a compartment is clicked
        /// </summary>
        public event EventHandler<WindowManagerCompartmentClickedEventArgs> CompartmentClicked;

        internal void OnCompartmentClicked(WindowManagerCompartmentClickedEventArgs e)
        {
            CompartmentClicked.RaiseEvent(this, e);
        }
    }
}
