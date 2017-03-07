namespace PaintDotNet.Resources
{
    using PaintDotNet.Collections;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal static class AnimatedResources
    {
        private static Image[] GetWorkingAnimation(int frameEdge)
        {
            string str = OS.IsWin8OrLater ? "metro" : "aero";
            Image reference = PdnResources.GetImageResource($"Images.process-working{frameEdge.ToString()}.{str}.png").Reference;
            List<Image> items = new List<Image>();
            for (int i = 0; i < (reference.Height / frameEdge); i++)
            {
                for (int j = 0; j < (reference.Width / frameEdge); j++)
                {
                    if ((j != 0) || (i != 0))
                    {
                        Bitmap image = new Bitmap(frameEdge, frameEdge, reference.PixelFormat);
                        using (Graphics graphics = Graphics.FromImage(image))
                        {
                            graphics.CompositingMode = CompositingMode.SourceCopy;
                            graphics.DrawImage(reference, new Rectangle(0, 0, frameEdge, frameEdge), new Rectangle(j * frameEdge, i * frameEdge, frameEdge, frameEdge), GraphicsUnit.Pixel);
                        }
                        items.Add(image);
                    }
                }
            }
            return items.ToArrayEx<Image>();
        }

        public static Image[] Working
        {
            get
            {
                int width = 0x10;
                float num2 = UIUtil.ScaleWidth(width);
                if (num2 >= 24f)
                {
                    return Working32;
                }
                return Working16;
            }
        }

        public static Image[] Working16 =>
            GetWorkingAnimation(0x10);

        public static Image[] Working22 =>
            GetWorkingAnimation(0x16);

        public static Image[] Working32 =>
            GetWorkingAnimation(0x20);
    }
}

