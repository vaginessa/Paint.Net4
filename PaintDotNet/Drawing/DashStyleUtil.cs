namespace PaintDotNet.Drawing
{
    using PaintDotNet;
    using PaintDotNet.UI.Media;
    using System;
    using System.Drawing.Drawing2D;

    internal static class DashStyleUtil
    {
        public static PaintDotNet.UI.Media.DashStyle ToMedia(System.Drawing.Drawing2D.DashStyle gdipDashStyle)
        {
            switch (gdipDashStyle)
            {
                case System.Drawing.Drawing2D.DashStyle.Solid:
                    return DashStyles.Solid;

                case System.Drawing.Drawing2D.DashStyle.Dash:
                    return DashStyles.Dash;

                case System.Drawing.Drawing2D.DashStyle.Dot:
                    return DashStyles.Dot;

                case System.Drawing.Drawing2D.DashStyle.DashDot:
                    return DashStyles.DashDot;

                case System.Drawing.Drawing2D.DashStyle.DashDotDot:
                    return DashStyles.DashDotDot;
            }
            throw ExceptionUtil.InvalidEnumArgumentException<System.Drawing.Drawing2D.DashStyle>(gdipDashStyle, "gdipDashStyle");
        }
    }
}

