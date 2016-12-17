using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using FrEle = System.Windows.FrameworkElement;
using ha = System.Windows.HorizontalAlignment;
using va = System.Windows.VerticalAlignment;

namespace MorningPod.Ultilities
{

    public static partial class Ext
    {

        public static DateTime ConvertMinsToNextDateTime(this double time)
        {
            DateTime now = vals.now;
            DateTime dt = new DateTime(now.Year , now.Month , now.Day).AddMinutes(time);

            if (dt < now) dt = dt.AddDays(1);

            return dt;
        }



        public static string PathSafe(this string str)
        {
            foreach (char c in Path.GetInvalidPathChars())
            {
                str = str.Replace(c.ToString() , "");
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                str = str.Replace(c.ToString() , "");
            }

            return str;
        }


        public static string FriendlyName(this Type type)
        {
            if (type == typeof(int))
                return "int";
            else if (type == typeof(short))
                return "short";
            else if (type == typeof(byte))
                return "byte";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(long))
                return "long";
            else if (type == typeof(float))
                return "float";
            else if (type == typeof(double))
                return "double";
            else if (type == typeof(decimal))
                return "decimal";
            else if (type == typeof(string))
                return "string";
            else if (type.IsGenericType)
                return type.FullName.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(x => FriendlyName(x)).ToArray()) + ">";
            else
                return type.FullName;
        }

        public static string GetMaxLength(this string name, int max)
        {
            return name.Length > max ? name.Substring(0, max) : name;
        }
        /// <summary>  Add "..." after  </summary>
        public static string GetMaxLengthDots(this string name, int max)
        {
            return name.Length > max ? (name.Substring(0, max) + "...") : name;
        }

        public static string ToAcronym(this string org)
        {
            return new string(org.Where((c, i) => c != ' ' && (i == 0 || org[i - 1] == ' ')).ToArray());
        }

        public static string DoReverse(this string str)
        {
            char[] charArray = str.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static bool IsBlank(this string str) { return string.IsNullOrEmpty(str); }
        public static bool NotBlank(this string str) { return !string.IsNullOrEmpty(str); }

        public static string Sep(this string sep, params object[] objs)
        {
            string RS = ""; int count = objs.Length; int count2 = objs.Length-1;
            for (int i = 0; i < count; i++)
            {
                if (i < count2) RS += objs[i] + sep;
                else RS += objs[i];
            }
            return RS;
        }

        //public static bool EqualAny(this object target, params object[] others)
        //{
            
        //}

        public static double Rnd(this double org) { return (Math.Round(org * 100d)) / 100d; }

        public static string ToUnsign(this string str)
        {
            string[] signs = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };
            for (int i = 1; i < signs.Length; i++)
            {
                for (int j = 0; j < signs[i].Length; j++)
                {
                    str = str.Replace(signs[i][j], signs[0][i - 1]);
                }
            }
            return str;
        }


        public static bool EqualAny(this string org, params string[] pars) { for (int i = 0; i < pars.Length; i++) { if (org.Equals(pars[i])) return true; } return false; }
        public static bool EqualEvery(this string org, params string[] pars) { for (int i = 0; i < pars.Length; i++) { if (!org.Equals(pars[i])) return false; } return true; }

        public static bool EqualAny(this object org, params object[] pars) { for (int i = 0; i < pars.Length; i++) { if (org.Equals(pars[i])) return true; } return false; }
        public static bool EqualEvery(this object org, params object[] pars) { for (int i = 0; i < pars.Length; i++) { if (!org.Equals(pars[i])) return false; } return true; }

        public static bool EqualAny(this char org, params char[] pars) { for (int i = 0; i < pars.Length; i++) { if (org.Equals(pars[i])) return true; } return false; }
        public static bool EqualEvery(this char org, params char[] pars) { for (int i = 0; i < pars.Length; i++) { if (!org.Equals(pars[i])) return false; } return true; }


        public static T AnyFirst<T>(this T[] objs) 
        {
            if (objs.Length > 0) return objs[0];
            return default(T);
        }


        
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count(); i++) collection.Move(collection.IndexOf(sorted[i]), i);
        }
    }






    public static partial class GUIExt
    {

        /// TEXTBLOCK
        public static Run GetRunAtIndex(this TextBlock txt, int idx, ref int idxRelativeToRun)
        {
            int count = 0;
            foreach (Inline il in txt.Inlines)
            {
                Run run = (Run) il; int ilLth = run.Text.Length;
                if (count + ilLth < idx) { count += ilLth; continue; }

                idxRelativeToRun = idx - count;
                return run;
            }

            //idxRelativeToRun = 0;
            return null;
        }


        /// RICH TEXT BOX
        public static string GetText(this RichTextBox txBx)
        {
            return new TextRange(txBx.Document.ContentStart, txBx.Document.ContentEnd).Text;
        }
        public static void SetText(this RichTextBox txBx, string str)
        {
            txBx.Document.Blocks.Clear();
            txBx.Document.Blocks.Add(new Paragraph(new Run(str)));
        }



        
        /// Dock
        /*public static void DockL(this FrEle subject, DockPanel panel = null)
        {
            panel?.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Left);
        }
        public static void DockT(this FrEle subject, DockPanel panel = null)
        {
            panel?.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Top);
        }
        public static void DockR(this FrEle subject, DockPanel panel = null)
        {
            panel?.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Right);
        }
        public static void DockB(this FrEle subject, DockPanel panel = null)
        {
            panel?.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Bottom);
        }*/

        public static void DockL(this DockPanel dpn, FrEle subject)
        {
            if (subject == null) return;
            dpn.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Left);
        }
        public static void DockT(this DockPanel dpn, FrEle subject)
        {
            if (subject == null) return;
            dpn.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Top);
        }
        public static void DockR(this DockPanel dpn, FrEle subject)
        {
            if (subject == null) return;
            dpn.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Right);
        }
        public static void DockB(this DockPanel dpn, FrEle subject)
        {
            if (subject == null) return;
            dpn.Children.Add(subject);
            DockPanel.SetDock(subject, Dock.Bottom);
        }


        public static void InGrid(this FrEle fe, int? col, int? row = null, int? colSp = null, int? rowSp = null)
        {
            if (col != null) Grid.SetColumn(fe, col.Value);
            if (row != null) Grid.SetRow(fe, row.Value);
            if (colSp != null) Grid.SetColumnSpan(fe, colSp.Value);
            if (rowSp != null) Grid.SetRowSpan(fe, rowSp.Value);
        }


        /// Align
        public static void Align(this FrEle fe, int? hori, int? vert = null)
        {
            if (hori != null)
            {
                if (hori == -1) fe.HorizontalAlignment = ha.Left;
                else if (hori == 0) fe.HorizontalAlignment = ha.Center;
                else if (hori == 1) fe.HorizontalAlignment = ha.Right;
                else if (hori == 2) fe.HorizontalAlignment = ha.Stretch;
            }

            if (vert != null)
            {
                if (vert == -1) fe.VerticalAlignment = va.Bottom;
                else if (vert == 0) fe.VerticalAlignment = va.Center;
                else if (vert == 1) fe.VerticalAlignment = va.Top;
                else if (vert == 2) fe.VerticalAlignment = va.Stretch;
            }
        }


        /// Visibility
        public static void VisShow(this FrEle fre) { fre.Visibility = Visibility.Visible; }
        public static void VisHide(this FrEle fre) { fre.Visibility = Visibility.Hidden; }
        public static void VisCollapse(this FrEle fre) { fre.Visibility = Visibility.Collapsed; }
        public static void VisToggleH(this FrEle fre, bool condition) { fre.Visibility = condition ? Visibility.Visible : Visibility.Hidden; }
        public static void VisToggleC(this FrEle fre, bool condition) { fre.Visibility = condition ? Visibility.Visible : Visibility.Collapsed; }


        public static void SetTopMost(this Window win, bool topMost)
        {
            win.Topmost = topMost;
        }


        /// Try Find Parent
        public static FrEle Clone(this FrEle subject)
        {
            return (FrEle)XamlReader.Load(XmlReader.Create(new StringReader(XamlWriter.Save(subject))));
        }

        /// Try Find From Point
        public static T TryFindFromPoint<T>(this UIElement iReference, Point iPoint) where T : DependencyObject
        {
            DependencyObject element = iReference.InputHitTest(iPoint) as DependencyObject;
            if (element == null) return null;
            if (element is T) return (T)element;
            return TryFindThisParent<T>(element);
        }
        
        /// Try Find Parent
        public static T TryFindThisParent<T>(this DependencyObject iChild) where T : DependencyObject
        {
            // Get parent item.
            DependencyObject parentObject = TryGetParentObject(iChild);

            // We've reached the end of the tree.
            if (parentObject == null) return null;

            // Check if the parent matches the type we're looking for. // Else use recursion to proceed with next level.
            T parent = parentObject as T;
            return parent ?? TryFindThisParent<T>(parentObject);
        }

        /// Try Find Parent
        public static DependencyObject TryGetParentObject(this DependencyObject iChild)
        {
            if (iChild == null) return null;

            // Handle content elements separately.
            ContentElement contentElement = iChild as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement frameworkContentElement = contentElement as FrameworkContentElement;
                return frameworkContentElement != null ? frameworkContentElement.Parent : null;
            }

            // Also try searching for parent in framework elements (such as DockPanel, etc).
            FrEle frameworkElement = iChild as FrEle;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            // If it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper.
            return VisualTreeHelper.GetParent(iChild);
        }

        /// ObservableCollection AddRange
        public static void AddRangeX<T>(this ObservableCollection<T> obsCol, List<T> list) { for (int i = 0; i < list.Count; i++) { obsCol.Add(list[i]); } }

        /// DateTime compare same day
        public static bool SameDay(this DateTime thisTime, DateTime other) { return thisTime.Year == other.Year && thisTime.Month == other.Month && thisTime.Day == other.Day; }


        /// Get Children Of Type
        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        public static List<T> GetChildrenOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            List<T> RS = new List<T>();
            if (depObj == null) return RS;

            int childCount = VisualTreeHelper.GetChildrenCount(depObj);

            if(childCount == 0 && (depObj as T != null)) { RS.Add(depObj as T); return RS; } 

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetChildrenOfType<T>(child);
                if (result.Count > 0) RS.AddRange(result);
            }
            return RS;
        }
        public static T FindChildWithName<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            // confirm parent is valid.
            if (parent == null) return null;
            if (parent is T) return parent as T;

            bool byName = childName.NotBlank();

            DependencyObject foundChild = null;

            if (parent is FrEle) (parent as FrEle).ApplyTemplate();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                foundChild = FindChildWithName<T>(child, childName);
                if (foundChild != null)
                {
                    if (!byName || Equals(foundChild.GetValue(FrEle.NameProperty), childName)) break;
                }
            }

            return foundChild as T;
        }


        /// Get Clicked Element
        public static object[] GetClickedElement(this FrEle subject, object sender, MouseButtonEventArgs args)
        {
            Point pt = args.GetPosition((UIElement)sender);

            List<object> hitResultsList = new List<object>();

            VisualTreeHelper.HitTest((UIElement)sender, null, new HitTestResultCallback(MyHitTestResult), new PointHitTestParameters(pt));

            object[] result = hitResultsList.ToArray();
            hitResultsList.Clear(); hitResultsList = null;
            return result;
        }
        public static List<object> hitResultsList = new List<object>();
        public static HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitResultsList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }



        //  ------------------------------------------------    DateTime -> String Day Month Year
        public static string StringDMY(this DateTime dateTime)
        {
            return "not done lol";

            //if (dateTime.Year == 1) return "...";
            ////dateTime = _TO.LocalTime(dateTime);
            //return dateTime.ToLocal().ToString("d/M/yyyy");
        }
        public static string StringDMYShort(this DateTime dateTime)
        {
            return "not done lol";

            //if (dateTime.Year == 1) return "...";
            ////dateTime = _TO.LocalTime(dateTime);

            //if (dateTime.Year == _VALS.CurYear) return dateTime.ToLocal().ToString("d/M");
            //return dateTime.ToLocal().ToString("d/M/yy");
        }
    }
}
