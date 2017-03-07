namespace PaintDotNet.Canvas
{
    using PaintDotNet.Direct2D;
    using PaintDotNet.Rendering;
    using PaintDotNet.UI.Media;
    using System;

    internal sealed class MoveHandleCanvasLayer : CanvasLayer
    {
        private SolidColorBrush blackBrush = new SolidColorBrush((ColorRgba128Float) ColorBgra.Black);
        private readonly CalculateInvalidRectCallback calculateInvalidRect;
        private byte handleAlpha;
        private double handleDiameter;
        private PointDouble handleLocation;
        private VectorDouble handleLocationScreenOffset;
        private MoveHandleShape handleShape;
        private Matrix3x2Double handleTransform;
        private SolidColorBrush whiteBrush = new SolidColorBrush((ColorRgba128Float) ColorBgra.White);

        public MoveHandleCanvasLayer()
        {
            this.calculateInvalidRect = new CalculateInvalidRectCallback(this.CalculateInvalidRect);
            this.handleTransform = Matrix3x2Double.Identity;
        }

        private RectDouble CalculateInvalidRect(CanvasView canvasView, RectDouble canvasRect) => 
            RectDouble.Inflate(this.GetHandleCanvasRect(canvasView), 1.0, 1.0);

        private RectDouble GetHandleCanvasRect(CanvasView canvasView)
        {
            double canvasHairWidth = canvasView.CanvasHairWidth;
            double num2 = canvasHairWidth * this.handleDiameter;
            double num3 = num2 / 2.0;
            PointDouble center = this.handleTransform.Transform(this.handleLocation) + ((PointDouble) (this.handleLocationScreenOffset * canvasHairWidth));
            return RectDouble.FromCenter(center, (double) ((num3 + canvasHairWidth) * 2.0));
        }

        private void InvalidateHandle()
        {
            if (this.handleDiameter > 0.0)
            {
                base.Invalidate(this.calculateInvalidRect, RectDouble.Zero);
            }
        }

        public bool IsPointTouchingHandle(CanvasView canvasView, PointDouble canvasPt)
        {
            base.VerifyAccess();
            double canvasHairWidth = canvasView.CanvasHairWidth;
            PointDouble center = this.handleTransform.Transform(this.handleLocation) + ((PointDouble) (this.handleLocationScreenOffset * canvasHairWidth));
            double edgeLength = this.handleDiameter * canvasHairWidth;
            return RectDouble.FromCenter(center, edgeLength).Contains(canvasPt);
        }

        protected override void OnIsVisibleChanged(bool oldValue, bool newValue)
        {
            this.InvalidateHandle();
            base.OnIsVisibleChanged(oldValue, newValue);
        }

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect, CanvasView canvasView)
        {
            double canvasHairWidth = canvasView.CanvasHairWidth;
            if (this.handleDiameter > 2.0)
            {
                double rotationAngle = this.handleTransform.GetRotationAngle();
                PointDouble location = this.handleTransform.Transform(this.handleLocation) + ((PointDouble) (this.handleLocationScreenOffset * canvasHairWidth));
                VectorDouble[] vecs = new VectorDouble[] { new VectorDouble(-1.0, -1.0), new VectorDouble(1.0, -1.0), new VectorDouble(1.0, 1.0), new VectorDouble(-1.0, 1.0), new VectorDouble(-1.0, 0.0), new VectorDouble(1.0, 0.0), new VectorDouble(0.0, -1.0), new VectorDouble(0.0, 1.0) };
                vecs.RotateInPlace(rotationAngle);
                vecs.NormalizeInPlace();
                double num5 = ((double) this.handleAlpha) / 255.0;
                this.whiteBrush.Opacity = num5;
                this.blackBrush.Opacity = num5;
                if (this.handleShape != MoveHandleShape.Circle)
                {
                    PointDouble[] numArray2 = new PointDouble[] { location + (vecs[0] * (this.handleDiameter * canvasHairWidth)), location + (vecs[1] * (this.handleDiameter * canvasHairWidth)), location + (vecs[2] * (this.handleDiameter * canvasHairWidth)), location + (vecs[3] * (this.handleDiameter * canvasHairWidth)) };
                    PointDouble[] numArray3 = new PointDouble[] { location + (vecs[0] * ((this.handleDiameter - 1.0) * canvasHairWidth)), location + (vecs[1] * ((this.handleDiameter - 1.0) * canvasHairWidth)), location + (vecs[2] * ((this.handleDiameter - 1.0) * canvasHairWidth)), location + (vecs[3] * ((this.handleDiameter - 1.0) * canvasHairWidth)) };
                    PointDouble[] numArray4 = new PointDouble[] { location + (vecs[0] * ((this.handleDiameter - 2.0) * canvasHairWidth)), location + (vecs[1] * ((this.handleDiameter - 2.0) * canvasHairWidth)), location + (vecs[2] * ((this.handleDiameter - 2.0) * canvasHairWidth)), location + (vecs[3] * ((this.handleDiameter - 2.0) * canvasHairWidth)) };
                    dc.DrawLine(numArray2[0], numArray2[1], this.whiteBrush, canvasHairWidth);
                    dc.DrawLine(numArray2[1], numArray2[2], this.whiteBrush, canvasHairWidth);
                    dc.DrawLine(numArray2[2], numArray2[3], this.whiteBrush, canvasHairWidth);
                    dc.DrawLine(numArray2[3], numArray2[0], this.whiteBrush, canvasHairWidth);
                    dc.DrawLine(numArray3[0], numArray3[1], this.blackBrush, canvasHairWidth);
                    dc.DrawLine(numArray3[1], numArray3[2], this.blackBrush, canvasHairWidth);
                    dc.DrawLine(numArray3[2], numArray3[3], this.blackBrush, canvasHairWidth);
                    dc.DrawLine(numArray3[3], numArray3[0], this.blackBrush, canvasHairWidth);
                    dc.DrawLine(numArray4[0], numArray4[1], this.whiteBrush, canvasHairWidth);
                    dc.DrawLine(numArray4[1], numArray4[2], this.whiteBrush, canvasHairWidth);
                    dc.DrawLine(numArray4[2], numArray4[3], this.whiteBrush, canvasHairWidth);
                    dc.DrawLine(numArray4[3], numArray4[0], this.whiteBrush, canvasHairWidth);
                }
                else if (this.handleShape == MoveHandleShape.Circle)
                {
                    RectDouble rect = new RectDouble(location, SizeDouble.Zero);
                    rect.Inflate((double) ((this.handleDiameter - 1.0) * canvasHairWidth), (double) ((this.handleDiameter - 1.0) * canvasHairWidth));
                    dc.DrawEllipse(EllipseDouble.FromRect(rect), this.whiteBrush, canvasHairWidth);
                    rect.Inflate(-canvasHairWidth, -canvasHairWidth);
                    dc.DrawEllipse(EllipseDouble.FromRect(rect), this.blackBrush, canvasHairWidth);
                    rect.Inflate(-canvasHairWidth, -canvasHairWidth);
                    dc.DrawEllipse(EllipseDouble.FromRect(rect), this.whiteBrush, canvasHairWidth);
                }
                if (this.handleShape == MoveHandleShape.Compass)
                {
                    PointDouble num7 = location + ((PointDouble) (vecs[0] * ((this.handleDiameter - 1.0) * canvasHairWidth)));
                    PointDouble point = location + ((PointDouble) (vecs[1] * ((this.handleDiameter - 1.0) * canvasHairWidth)));
                    PointDouble num9 = location + ((PointDouble) (vecs[2] * ((this.handleDiameter - 1.0) * canvasHairWidth)));
                    PointDouble num10 = location + ((PointDouble) (vecs[3] * ((this.handleDiameter - 1.0) * canvasHairWidth)));
                    PointDouble num11 = new PointDouble(num7.X, (num7.Y + num10.Y) / 2.0);
                    PointDouble num12 = new PointDouble((num7.X + point.X) / 2.0, num7.Y);
                    PointDouble num13 = new PointDouble(point.X, (point.Y + num9.Y) / 2.0);
                    PointDouble num14 = new PointDouble((num10.X + num9.X) / 2.0, num9.Y);
                    PointDouble num15 = new PointDouble(num12.X, num11.Y);
                    PathGeometry geometry = new PathGeometry();
                    PathFigure item = new PathFigure {
                        IsClosed = true,
                        IsFilled = true,
                        StartPoint = num7
                    };
                    item.Segments.Add(new LineSegment(point));
                    item.Segments.Add(new LineSegment(num9));
                    item.Segments.Add(new LineSegment(num10));
                    geometry.Figures.Add(item);
                    dc.FillGeometry(geometry, this.whiteBrush, null);
                    PathGeometry geometry2 = new PathGeometry();
                    double num16 = canvasHairWidth;
                    double num17 = 1.35 * canvasHairWidth;
                    double num18 = (num17 * 3.0) / 2.0;
                    double num19 = num16 / 2.0;
                    double num20 = (num17 * Math.Sqrt(27.0)) / 2.0;
                    PathFigure figure2 = new PathFigure {
                        IsFilled = true,
                        IsClosed = true,
                        StartPoint = num11
                    };
                    figure2.Segments.Add(new LineSegment(new PointDouble(num11.X + num20, num11.Y + num18)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num11.X + num20, num11.Y + num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num15.X - num19, num15.Y + num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num14.X - num19, num14.Y - num20)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num14.X - num18, num14.Y - num20)));
                    figure2.Segments.Add(new LineSegment(num14));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num14.X + num18, num14.Y - num20)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num14.X + num19, num14.Y - num20)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num15.X + num19, num15.Y + num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num13.X - num20, num13.Y + num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num13.X - num20, num13.Y + num18)));
                    figure2.Segments.Add(new LineSegment(num13));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num13.X - num20, num13.Y - num18)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num13.X - num20, num13.Y - num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num15.X + num19, num15.Y - num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num12.X + num19, num12.Y + num20)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num12.X + num18, num12.Y + num20)));
                    figure2.Segments.Add(new LineSegment(num12));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num12.X - num18, num12.Y + num20)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num12.X - num19, num12.Y + num20)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num15.X - num19, num15.Y - num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num11.X + num20, num11.Y - num19)));
                    figure2.Segments.Add(new LineSegment(new PointDouble(num11.X + num20, num11.Y - num18)));
                    geometry2.Figures.Add(figure2);
                    dc.FillGeometry(geometry2, this.blackBrush, null);
                }
            }
            base.OnRender(dc, clipRect, canvasView);
        }

        public byte HandleAlpha
        {
            get => 
                this.handleAlpha;
            set
            {
                base.VerifyAccess();
                if (value != this.handleAlpha)
                {
                    this.handleAlpha = value;
                    this.InvalidateHandle();
                }
            }
        }

        public double HandleDiameter
        {
            get => 
                this.handleDiameter;
            set
            {
                base.VerifyAccess();
                if (value != this.handleDiameter)
                {
                    this.InvalidateHandle();
                    this.handleDiameter = value;
                    this.InvalidateHandle();
                }
            }
        }

        public PointDouble HandleLocation
        {
            get => 
                this.handleLocation;
            set
            {
                base.VerifyAccess();
                if (value != this.handleLocation)
                {
                    this.InvalidateHandle();
                    this.handleLocation = value;
                    this.InvalidateHandle();
                }
            }
        }

        public VectorDouble HandleLocationScreenOffset
        {
            get => 
                this.handleLocationScreenOffset;
            set
            {
                base.VerifyAccess();
                if (value != this.handleLocationScreenOffset)
                {
                    this.InvalidateHandle();
                    this.handleLocationScreenOffset = value;
                    this.InvalidateHandle();
                }
            }
        }

        public MoveHandleShape HandleShape
        {
            get => 
                this.handleShape;
            set
            {
                base.VerifyAccess();
                if (value != this.handleShape)
                {
                    this.InvalidateHandle();
                    this.handleShape = value;
                    this.InvalidateHandle();
                }
            }
        }

        public Matrix3x2Double HandleTransform
        {
            get => 
                this.handleTransform;
            set
            {
                base.VerifyAccess();
                if (value != this.handleTransform)
                {
                    this.InvalidateHandle();
                    this.handleTransform = value;
                    this.InvalidateHandle();
                }
            }
        }
    }
}

