namespace PaintDotNet.IndirectUI
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Windows.Forms;

    internal static class PropertyControlUtil
    {
        public static double FromSliderValueExp(int sliderValue, double minValue, double maxValue, int scaleLog10)
        {
            if (sliderValue != 0)
            {
                if ((minValue >= 0.0) && (maxValue >= 0.0))
                {
                    return FromSliderValueExpCore(sliderValue, minValue, maxValue, scaleLog10);
                }
                if ((minValue <= 0.0) && (maxValue <= 0.0))
                {
                    return -FromSliderValueExpCore(-sliderValue, -minValue, -maxValue, scaleLog10);
                }
                if (sliderValue > 0)
                {
                    return FromSliderValueExpCore(sliderValue, 0.0, maxValue, scaleLog10);
                }
                if (sliderValue < 0)
                {
                    return -FromSliderValueExpCore(-sliderValue, 0.0, -minValue, scaleLog10);
                }
                ExceptionUtil.ThrowInvalidOperationException();
            }
            return 0.0;
        }

        public static double FromSliderValueExpCore(int sliderValue, double minValue, double maxValue, int scaleLog10)
        {
            int num = 1 + scaleLog10;
            int num2 = (int) Math.Pow(10.0, (double) num);
            double num3 = Math.Pow(10.0, (double) -num);
            double num4 = sliderValue * num3;
            double num5 = (num4 - minValue) / (maxValue - minValue);
            double num6 = num5 * num5;
            double num7 = minValue + (num6 * (maxValue - minValue));
            return num7.Clamp(minValue, maxValue);
        }

        public static int GetGoodSliderHeight(TrackBar slider)
        {
            Validate.IsNotNull<TrackBar>(slider, "slider");
            if (slider.AutoSize)
            {
                return slider.Height;
            }
            if ((slider.TickStyle == TickStyle.BottomRight) || (slider.TickStyle == TickStyle.TopLeft))
            {
                return UIUtil.ScaleHeight(0x23);
            }
            if (slider.TickStyle == TickStyle.None)
            {
                return UIUtil.ScaleHeight(0x19);
            }
            if (slider.TickStyle != TickStyle.Both)
            {
                throw ExceptionUtil.InvalidEnumArgumentException<TickStyle>(slider.TickStyle, "slider.TickStyle");
            }
            return UIUtil.ScaleHeight(0x2d);
        }

        public static int GetGoodSliderTickFrequency(PdnTrackBar slider)
        {
            int num = Math.Abs((int) (slider.Maximum - slider.Minimum));
            return Math.Max(1, num / 20);
        }

        public static int GetGoodSliderTickFrequency(TrackBar slider)
        {
            int num = Math.Abs((int) (slider.Maximum - slider.Minimum));
            return Math.Max(1, num / 20);
        }

        public static int ToSliderValue(double propertyValue, int decimalPlaces)
        {
            double num = Math.Pow(10.0, (double) decimalPlaces);
            double num2 = propertyValue * num;
            return (int) Math.Round(num2, MidpointRounding.AwayFromZero);
        }

        public static int ToSliderValueExp(double propertyValue, double minValue, double maxValue, int scaleLog10)
        {
            if (propertyValue != 0.0)
            {
                if ((minValue >= 0.0) && (maxValue >= 0.0))
                {
                    return ToSliderValueExpCore(propertyValue, minValue, maxValue, scaleLog10);
                }
                if ((minValue <= 0.0) && (maxValue <= 0.0))
                {
                    return -ToSliderValueExpCore(-propertyValue, -minValue, -maxValue, scaleLog10);
                }
                if (propertyValue > 0.0)
                {
                    return ToSliderValueExpCore(propertyValue, 0.0, maxValue, scaleLog10);
                }
                if (propertyValue < 0.0)
                {
                    return -ToSliderValueExpCore(-propertyValue, 0.0, -minValue, scaleLog10);
                }
                ExceptionUtil.ThrowInvalidOperationException();
            }
            return 0;
        }

        public static int ToSliderValueExpCore(double propertyValue, double minValue, double maxValue, int scaleLog10)
        {
            int num = 1 + scaleLog10;
            int num2 = (int) Math.Pow(10.0, (double) num);
            double num3 = Math.Pow(10.0, (double) -num);
            double num5 = Math.Sqrt(Math.Abs((double) ((propertyValue - minValue) / (maxValue - minValue))));
            double num6 = minValue + (num5 * (maxValue - minValue));
            double num8 = num6.Clamp(minValue, maxValue) * num2;
            long num9 = (long) num8;
            return (int) num9;
        }
    }
}

