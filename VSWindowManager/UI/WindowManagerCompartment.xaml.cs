using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;

namespace VSWindowManager
{
    /// <summary>
    /// Interaction logic for WindowManagerCompartment.xaml
    /// </summary>
    internal partial class WindowManagerCompartment : UserControl
    {
        private static readonly DependencyProperty PositionProperty;

        private Window _window;

        private bool _isInitialized;

        static WindowManagerCompartment()
        {
            WindowManagerCompartment.PositionProperty = DependencyProperty.Register("Position", typeof(Rect), typeof(WindowManagerCompartment),
                                                                          new FrameworkPropertyMetadata(Rect.Empty));
        }

        public WindowManagerCompartment()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the position of the compartment in absolute coordinates
        /// </summary>
        public Rect Position
        {
            get { return (Rect)GetValue(WindowManagerCompartment.PositionProperty); }
            private set { SetValue(WindowManagerCompartment.PositionProperty, value); }
        }

        private void WindowManagerCompartment_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                return;
            }

            _window = Window.GetWindow(this);

            // If either the window location changes or layout is updated, get the new position of the compartment
            if (_window != null)
            {
                _window.LocationChanged += WindowManagerCompartment_PositionChanged;
                LayoutUpdated += WindowManagerCompartment_PositionChanged;

                Binding binding = new Binding();
                binding.Source = this;
                binding.Mode = BindingMode.OneWayToSource;
                binding.Path = new PropertyPath("DataContext.Position");

                SetBinding(PositionProperty, binding);

                _isInitialized = true;
            }

        }

        private void WindowManagerCompartment_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_window != null)
            {
                _window.LocationChanged -= WindowManagerCompartment_PositionChanged;
                LayoutUpdated -= WindowManagerCompartment_PositionChanged;
            }
            
            _isInitialized = false;
        }

        private void WindowManagerCompartment_PositionChanged(object sender, EventArgs e)
        {
            // Get the absolute position of the compartment if it is connected to
            // an HwndSource
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                Point topLeft = this.PointToScreen(new Point(0, 0));
                Point bottomRight = this.PointToScreen(new Point(ActualWidth, ActualHeight));

                Position = new Rect(topLeft, bottomRight);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            WindowManagerCompartmentViewModel viewModel = this.DataContext as WindowManagerCompartmentViewModel;

            if (viewModel != null)
            {
                if (viewModel.Position.Top == 0 && viewModel.Position.Left == 0)
                {
                    viewModel.Position = Position;
                }

                viewModel.OnCompartmentClicked(new WindowManagerCompartmentClickedEventArgs(Position));
            }

            e.Handled = true;
        }
    }
}
