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
            // Refresh the side bar list
            List<FrameworkElement> allSideBars = GetAllSideBars();

            // If any gutters are visible, they should all be Collapsed
            bool visibleGutters = allSideBars.Exists(x => x.Visibility == Visibility.Visible);

            // Set the visibility of each side bar
            allSideBars.ForEach(sideBar => sideBar.Visibility = visibleGutters ? Visibility.Collapsed : Visibility.Visible);
        }

        private List<FrameworkElement> GetAllSideBars()
        {
            var sideBarObjects = FindChildrenByType(_window, "AutoHideChannelControl");
            if (sideBarObjects == null || sideBarObjects.Count == 0) return null;

            List<FrameworkElement> allSideBars = new List<FrameworkElement>();
            foreach (DependencyObject sideBarObject in sideBarObjects)
            {
                allSideBars.Add((FrameworkElement)sideBarObject);
            }

            return allSideBars;
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