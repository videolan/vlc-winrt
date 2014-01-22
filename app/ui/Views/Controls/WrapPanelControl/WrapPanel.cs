using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT.Views.Controls.WrapPanelControl
{
    public class WrapPanel : Panel
    {
        private bool _ignorePropertyChange;

        #region public double ItemHeight
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                "ItemHeight",
                typeof(double),
                typeof(WrapPanel),
                new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));
        #endregion public double ItemHeight

        #region public double ItemWidth
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(
                "ItemWidth",
                typeof(double),
                typeof(WrapPanel),
                new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));
        #endregion public double ItemWidth

        #region public Orientation Orientation
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(WrapPanel),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Almost always set from the CLR property.")]
        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            Orientation value = (Orientation)e.NewValue;

            // Ignore the change if requested
            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            // Validate the Orientation
            if ((value != Orientation.Horizontal) &&
                (value != Orientation.Vertical))
            {
                // Reset the property to its original state before throwing
                source._ignorePropertyChange = true;
                source.SetValue(OrientationProperty, (Orientation)e.OldValue);

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Properties.Resources.WrapPanel_OnOrientationPropertyChanged_InvalidValue",
                    value);
                throw new ArgumentException(message, "value");
            }

            // Orientation affects measuring.
            source.InvalidateMeasure();
        }
        #endregion public Orientation Orientation

        public WrapPanel()
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Almost always set from the CLR property.")]
        private static void OnItemHeightOrWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            double value = (double)e.NewValue;

            // Ignore the change if requested
            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            // Validate the length (which must either be NaN or a positive,
            // finite number)
            if (!double.IsNaN(value) && ((value <= 0.0) || double.IsPositiveInfinity(value)))
            {
                // Reset the property to its original state before throwing
                source._ignorePropertyChange = true;
                source.SetValue(e.Property, (double)e.OldValue);

                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Properties.Resources.WrapPanel_OnItemHeightOrWidthPropertyChanged_InvalidValue",
                    value);
                throw new ArgumentException(message, "value");
            }

            // The length properties affect measuring.
            source.InvalidateMeasure();
        }

        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#", Justification = "Compat with WPF.")]
        protected override Size MeasureOverride(Size constraint)
        {
            // Variables tracking the size of the current line, the total size
            // measured so far, and the maximum size available to fill.  Note
            // that the line might represent a row or a column depending on the
            // orientation.
            Orientation o = Orientation;
            OrientedSize lineSize = new OrientedSize(o);
            OrientedSize totalSize = new OrientedSize(o);
            OrientedSize maximumSize = new OrientedSize(o, constraint.Width, constraint.Height);

            // Determine the constraints for individual items
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            bool hasFixedWidth = !double.IsNaN(itemWidth);
            bool hasFixedHeight = !double.IsNaN(itemHeight);
            Size itemSize = new Size(
                hasFixedWidth ? itemWidth : constraint.Width,
                hasFixedHeight ? itemHeight : constraint.Height);

            // Measure each of the Children
            foreach (UIElement element in Children)
            {
                // Determine the size of the element
                element.Measure(itemSize);
                OrientedSize elementSize = new OrientedSize(
                    o,
                    hasFixedWidth ? itemWidth : element.DesiredSize.Width,
                    hasFixedHeight ? itemHeight : element.DesiredSize.Height);

                // If this element falls of the edge of the line
                if (NumericExtensions.IsGreaterThan(lineSize.Direct + elementSize.Direct, maximumSize.Direct))
                {
                    // Update the total size with the direct and indirect growth
                    // for the current line
                    totalSize.Direct = Math.Max(lineSize.Direct, totalSize.Direct);
                    totalSize.Indirect += lineSize.Indirect;

                    // Move the element to a new line
                    lineSize = elementSize;

                    // If the current element is larger than the maximum size,
                    // place it on a line by itself
                    if (NumericExtensions.IsGreaterThan(elementSize.Direct, maximumSize.Direct))
                    {
                        // Update the total size for the line occupied by this
                        // single element
                        totalSize.Direct = Math.Max(elementSize.Direct, totalSize.Direct);
                        totalSize.Indirect += elementSize.Indirect;

                        // Move to a new line
                        lineSize = new OrientedSize(o);
                    }
                }
                else
                {
                    // Otherwise just add the element to the end of the line
                    lineSize.Direct += elementSize.Direct;
                    lineSize.Indirect = Math.Max(lineSize.Indirect, elementSize.Indirect);
                }
            }

            // Update the total size with the elements on the last line
            totalSize.Direct = Math.Max(lineSize.Direct, totalSize.Direct);
            totalSize.Indirect += lineSize.Indirect;

            // Return the total size required as an un-oriented quantity
            return new Size(totalSize.Width, totalSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // Variables tracking the size of the current line, and the maximum
            // size available to fill.  Note that the line might represent a row
            // or a column depending on the orientation.
            Orientation o = Orientation;
            OrientedSize lineSize = new OrientedSize(o);
            OrientedSize maximumSize = new OrientedSize(o, finalSize.Width, finalSize.Height);

            // Determine the constraints for individual items
            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            bool hasFixedWidth = !itemWidth.IsNaN();
            bool hasFixedHeight = !itemHeight.IsNaN();
            double indirectOffset = 0;
            double? directDelta = (o == Orientation.Horizontal) ?
                (hasFixedWidth ? (double?)itemWidth : null) :
                (hasFixedHeight ? (double?)itemHeight : null);

            // Measure each of the Children.  We will process the elements one
            // line at a time, just like during measure, but we will wait until
            // we've completed an entire line of elements before arranging them.
            // The lineStart and lineEnd variables track the size of the
            // currently arranged line.
            UIElementCollection children = Children;
            int count = children.Count;
            int lineStart = 0;
            for (int lineEnd = 0; lineEnd < count; lineEnd++)
            {
                UIElement element = children[lineEnd];

                // Get the size of the element
                OrientedSize elementSize = new OrientedSize(
                    o,
                    hasFixedWidth ? itemWidth : element.DesiredSize.Width,
                    hasFixedHeight ? itemHeight : element.DesiredSize.Height);

                // If this element falls of the edge of the line
                if (NumericExtensions.IsGreaterThan(lineSize.Direct + elementSize.Direct, maximumSize.Direct))
                {
                    // Then we just completed a line and we should arrange it
                    ArrangeLine(lineStart, lineEnd, directDelta, indirectOffset, lineSize.Indirect);

                    // Move the current element to a new line
                    indirectOffset += lineSize.Indirect;
                    lineSize = elementSize;

                    // If the current element is larger than the maximum size
                    if (NumericExtensions.IsGreaterThan(elementSize.Direct, maximumSize.Direct))
                    {
                        // Arrange the element as a single line
                        ArrangeLine(lineEnd, ++lineEnd, directDelta, indirectOffset, elementSize.Indirect);

                        // Move to a new line
                        indirectOffset += lineSize.Indirect;
                        lineSize = new OrientedSize(o);
                    }

                    // Advance the start index to a new line after arranging
                    lineStart = lineEnd;
                }
                else
                {
                    // Otherwise just add the element to the end of the line
                    lineSize.Direct += elementSize.Direct;
                    lineSize.Indirect = Math.Max(lineSize.Indirect, elementSize.Indirect);
                }
            }

            // Arrange any elements on the last line
            if (lineStart < count)
            {
                ArrangeLine(lineStart, count, directDelta, indirectOffset, lineSize.Indirect);
            }

            return finalSize;
        }

        private void ArrangeLine(int lineStart, int lineEnd, double? directDelta, double indirectOffset, double indirectGrowth)
        {
            double directOffset = 0.0;

            Orientation o = Orientation;
            bool isHorizontal = o == Orientation.Horizontal;

            UIElementCollection children = Children;
            for (int index = lineStart; index < lineEnd; index++)
            {
                // Get the size of the element
                UIElement element = children[index];
                OrientedSize elementSize = new OrientedSize(o, element.DesiredSize.Width, element.DesiredSize.Height);

                // Determine if we should use the element's desired size or the
                // fixed item width or height
                double directGrowth = directDelta != null ?
                    directDelta.Value :
                    elementSize.Direct;

                // Arrange the element
                Rect bounds = isHorizontal ?
                    new Rect(directOffset, indirectOffset, directGrowth, indirectGrowth) :
                    new Rect(indirectOffset, directOffset, indirectGrowth, directGrowth);
                element.Arrange(bounds);

                directOffset += directGrowth;
            }
        }
    }

    internal static class NumericExtensions
    {
        /// <summary>
        /// NanUnion is a C++ style type union used for efficiently converting
        /// a double into an unsigned long, whose bits can be easily
        /// manipulated.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct NanUnion
        {
            /// <summary>
            /// Floating point representation of the union.
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "It is accessed through the other member of the union")]
            [FieldOffset(0)]
            internal double FloatingValue;

            /// <summary>
            /// Integer representation of the union.
            /// </summary>
            [FieldOffset(0)]
            internal ulong IntegerValue;
        }

#if !WINDOWS_PHONE
        public static bool IsZero(this double value)
        {
            // We actually consider anything within an order of magnitude of
            // epsilon to be zero
            return Math.Abs(value) < 2.2204460492503131E-15;
        }
#endif

        public static bool IsNaN(this double value)
        {
            // Get the double as an unsigned long
            NanUnion union = new NanUnion { FloatingValue = value };

            // An IEEE 754 double precision floating point number is NaN if its
            // exponent equals 2047 and it has a non-zero mantissa.
            ulong exponent = union.IntegerValue & 0xfff0000000000000L;
            if ((exponent != 0x7ff0000000000000L) && (exponent != 0xfff0000000000000L))
            {
                return false;
            }
            ulong mantissa = union.IntegerValue & 0x000fffffffffffffL;
            return mantissa != 0L;
        }

        public static bool IsGreaterThan(double left, double right)
        {
            return (left > right) && !AreClose(left, right);
        }

        public static bool AreClose(double left, double right)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (left == right)
            // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                return true;
            }

            double a = (Math.Abs(left) + Math.Abs(right) + 10.0) * 2.2204460492503131E-16;
            double b = left - right;
            return (-a < b) && (a > b);
        }

#if !WINDOWS_PHONE
        public static bool IsLessThanOrClose(double left, double right)
        {
            return (left < right) || AreClose(left, right);
        }
#endif
    }
}