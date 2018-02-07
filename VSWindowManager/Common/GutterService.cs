using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VSWindowManager
{
    public class GutterService
    {
        private Window _window;

        public GutterService(Window pWindow)
        {
            _window = pWindow;
        }

        public void ToggleAllSideBars()
        {
            // Fetch an up-to-date list of gutters
            List<FrameworkElement> allGutters = GetAllTabGutters();
            if (allGutters == null) return; // There are no tab gutters to toggle.

            // To toggle margins, check if any area already hidden.
            bool haveHiddenGutters = allGutters.Exists(x => x.Visibility == Visibility.Collapsed);

            // Set the visibility of each tab bar
            Visibility newVisibility = (haveHiddenGutters ? Visibility.Visible : Visibility.Collapsed);
            allGutters.ForEach(x => x.Visibility = newVisibility);
        }

        private List<FrameworkElement> GetAllTabGutters()
        {
            List<DependencyObject> autohideChannelControls = FindChildrenByType(_window, "AutoHideChannelControl");
            if (autohideChannelControls == null || autohideChannelControls.Count == 0) return null;

            // Get the "ItemsPresenter" from the Grid inside the AutoHideChannelControl object
            List<FrameworkElement> allTabGutters = new List<FrameworkElement>();
            foreach (DependencyObject autoHideChannelControl in autohideChannelControls)
            {
                // Note: Not doing null checking here. Expecting all AutoHideChannelControls to have an "ItemsPresenter" inside a (Grid).
                foreach (DependencyObject grid in FindChildrenByType(autoHideChannelControl, "Grid"))
                {
                    allTabGutters.Add((FrameworkElement)((FrameworkElement)grid).FindName("ItemsPresenter"));
                }
            }

            return allTabGutters;
        }

        private static List<DependencyObject> FindChildrenByType(DependencyObject parent, string typeName)
        {
            if (parent == null)
            {
                return null;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            List<DependencyObject> matches = null;
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child.GetType().Name.Contains(typeName))
                {
                    // Add it to a list
                    if (matches == null) matches = new List<DependencyObject>() { child };
                    else matches.Add(child);
                    continue;
                }

                // If no matches found at this level, go deeper.
                if (matches == null)
                {
                    matches = FindChildrenByType(child, typeName);
                }
                else
                {
                    List<DependencyObject> childMatches = FindChildrenByType(child, typeName);
                    if (childMatches != null) matches.AddRange(childMatches);
                }

            }

            return matches;
        }

    }
}