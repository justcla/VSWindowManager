using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VSWindowManager
{
    public class GutterService
    {
        private const double MARGIN_HIDE_WIDTH = -23;

        private Window _window;

        public GutterService(Window pWindow)
        {
            _window = pWindow;
        }

        public void ToggleAllSideBars()
        {
            // Refresh the side bar list
            List<FrameworkElement> allSideBars = GetAllSideBars();

            // If any margins contain a negative margin, then we have hidden some hidden gutters.
            bool haveHiddenGutters = allSideBars.Exists(x => x.Margin.ToString().Contains("-"));
            bool showGutters = !haveHiddenGutters;

            // Set the visibility of each side bar
            foreach(FrameworkElement sideBar in allSideBars)
            {
                Thickness newMargin = sideBar.Margin;
                switch (GetDockPosition(sideBar))
                {
                    case Dock.Left:
                        newMargin.Left = (showGutters ? 0 : MARGIN_HIDE_WIDTH);
                        break;
                    case Dock.Right:
                        newMargin.Right = (showGutters ? 0 : MARGIN_HIDE_WIDTH - 4);    // Set Right margin slightly skinnier
                        break;
                    case Dock.Top:
                        newMargin.Top = (showGutters ? 0 : MARGIN_HIDE_WIDTH);
                        break;
                    case Dock.Bottom:
                        newMargin.Bottom = (showGutters ? 0 : MARGIN_HIDE_WIDTH);
                        break;
                    default:
                        break;
                }
                sideBar.Margin = newMargin;
            }
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

        private static Dock GetDockPosition(FrameworkElement sideBar)
        {
            // Use reflection to get the value of ChannelDock from the sideBar (AutoHideChannelControl)
            // ChannelDock is a DependencyProperty. Must dig into DataContext.
            object dataContextValue = GetPropertyValue(sideBar, "DataContext");
            return (Dock)GetPropertyValue(dataContextValue, "Dock");
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            Type type = obj.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            return propertyInfo.GetValue(obj);
        }

    }
}