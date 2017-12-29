using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VSWindowManager
{
    public class GutterService
    {
        private Window _window;
        private List<DependencyObject> allSideBars;

        public GutterService(Window pWindow)
        {
            _window = pWindow;
        }

        public void ToggleAllSideBars()
        {
            allSideBars = FindChildrenByType(_window, "AutoHideChannelControl");
            if (allSideBars == null  || allSideBars.Count == 0) return;

            foreach(FrameworkElement sideBar in allSideBars)
            {
                sideBar.Visibility = sideBar.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            }
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