namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Data;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Imaging;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Threading;
    using PaintDotNet.UI.Media;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class TextTool : PaintDotNet.Tools.Tool
    {
        private PaintDotNet.TextAlignment alignment;
        private static readonly TextMeasurement badFontTextMeasurement = new TextMeasurement(0f, 10f, 0f, 0f, 0f, 0f);
        private PointInt32 clickPoint;
        private bool controlKeyDown;
        private readonly TimeSpan controlKeyDownThreshold;
        private DateTime controlKeyDownTime;
        private CompoundHistoryMemento currentHA;
        private const int cursorInterval = 300;
        private bool enableNub;
        private IGdiFontMap fontMap;
        private int ignoreRedraw;
        private bool lastPulseCursorState;
        private int linePos;
        private List<string> lines;
        private int managedThreadId;
        private EditingMode mode;
        private MoveHandleCanvasLayer moveHandle;
        private SizedFontProperties sizedFontProps;
        private PointInt32 startClickPoint;
        private PointInt32 startMouseXY;
        private DateTime startTime;
        private string statusBarTextFormat;
        private TextCursorCanvasLayer textCursor;
        private TextLayerOverlay textLayerOverlay;
        private TextLayoutAlgorithm? textLayoutAlgorithm;
        private PointInt32[] textPoints;
        private int textPos;
        private TextMeasurement[] textSizes;
        private Cursor textToolCursor;
        private bool tracking;

        public TextTool(DocumentWorkspace documentWorkspace) : base(documentWorkspace, PdnResources.GetImageResource("Icons.TextToolIcon.png"), PdnResources.GetString("TextTool.Name"), PdnResources.GetString("TextTool.HelpText"), 't', false, ToolBarConfigItems.Antialiasing | ToolBarConfigItems.BlendMode | ToolBarConfigItems.Text)
        {
            this.statusBarTextFormat = PdnResources.GetString("TextTool.StatusText.TextInfo.Format");
            this.enableNub = true;
            this.controlKeyDownTime = DateTime.MinValue;
            this.controlKeyDownThreshold = new TimeSpan(0, 0, 0, 0, SystemInformation.CaretBlinkTime);
            this.managedThreadId = Thread.CurrentThread.ManagedThreadId;
            base.IsCommitSupported = true;
        }

        private int FindOffsetPosition(double offset, string line, int lno)
        {
            for (int i = 0; i < line.Length; i++)
            {
                double num3 = this.TextPositionToPoint(new Position(lno, i)).X - this.clickPoint.X;
                if (num3 >= offset)
                {
                    return i;
                }
            }
            return line.Length;
        }

        private static void FixTextLayout(IDrawingContext dc, string text, ITextLayout textLayout)
        {
            Validate.IsNotNull<ITextLayout>(textLayout, "textLayout");
            if (text.Length != 0)
            {
                string familyName = textLayout.GetFontFamilyName(0).Value;
                IFontCollection fonts = textLayout.GetFontCollection(0).Value;
                int index = fonts.IndexOfFamilyName(familyName);
                if (index != -1)
                {
                    int num3;
                    IFontFamily fontFamily = fonts.GetFontFamily(index);
                    FontWeight weight = textLayout.GetFontWeight(0).Value;
                    FontStretch stretch = textLayout.GetFontStretch(0).Value;
                    PaintDotNet.DirectWrite.FontStyle style = textLayout.GetFontStyle(0).Value;
                    PaintDotNet.DirectWrite.IFont font = fontFamily.GetFirstMatchingFont(weight, stretch, style);
                    for (int i = 0; i < text.Length; i = num3)
                    {
                        if ((i >= text.Length) || !font.HasCharacter(text[i]))
                        {
                            break;
                        }
                        i++;
                        num3 = i;
                        while (true)
                        {
                            if ((num3 >= text.Length) || font.HasCharacter(text[num3]))
                            {
                                break;
                            }
                            num3++;
                        }
                        if (((i < text.Length) && (num3 <= text.Length)) && ((num3 - i) > 0))
                        {
                            TextRange textRange = new TextRange(i, num3 - i);
                            textLayout.SetFontFamilyName(textRange, "Segoe UI");
                        }
                    }
                    try
                    {
                        TextMetrics metrics = textLayout.Metrics;
                    }
                    catch (Exception exception)
                    {
                        if (exception is FontFileFormatException)
                        {
                            textLayout.SetFontFamilyName(TextRange.All, "Segoe UI");
                        }
                        else
                        {
                            ITypography typography = DirectWriteFactory.Instance.CreateTypography();
                            TextRange range2 = new TextRange(0, text.Length);
                            textLayout.SetTypography(range2, typography);
                        }
                        TextMetrics metrics2 = textLayout.Metrics;
                    }
                }
            }
        }

        private TextAntialiasMode GetDirect2DTextAntialiasMode()
        {
            if (!base.ToolSettings.Antialiasing.Value)
            {
                return TextAntialiasMode.Aliased;
            }
            return TextAntialiasMode.Grayscale;
        }

        private TextRenderingMode GetDirectWriteTextRenderingMode()
        {
            bool flag = base.ToolSettings.Antialiasing.Value;
            switch (base.ToolSettings.Text.RenderingMode.Value)
            {
                case TextToolRenderingMode.Smooth:
                    if (flag)
                    {
                        return TextRenderingMode.Outline;
                    }
                    return TextRenderingMode.Default;

                case TextToolRenderingMode.Classic:
                    if (flag)
                    {
                        return TextRenderingMode.ClearTypeGdiClassic;
                    }
                    return TextRenderingMode.Default;
            }
            if (!flag)
            {
                return TextRenderingMode.Default;
            }
            return TextRenderingMode.ClearTypeNaturalSymmetric;
        }

        private string GetStatusBarXYText()
        {
            string str;
            string str2;
            string str3;
            base.Document.CoordinatesToStrings(base.AppWorkspace.Units, this.textPoints[0].X, this.textPoints[0].Y, out str2, out str3, out str);
            return string.Format(this.statusBarTextFormat, new object[] { str2, str, str3, str });
        }

        private TextMeasurement GetStringSize(string text)
        {
            TextMeasurement measurement3;
            if (text.Length == 0)
            {
                TextMeasurement stringSize = this.GetStringSize(" ");
                return new TextMeasurement(0f, stringSize.Height, 0f, 0f, 0f, 0f);
            }
            TextRenderingMode directWriteTextRenderingMode = this.GetDirectWriteTextRenderingMode();
            TextAntialiasMode mode2 = this.GetDirect2DTextAntialiasMode();
            using (IDrawingContext context = DrawingContext.CreateNull(FactorySource.PerThread))
            {
                using (context.UseTextRenderingMode(this.GetDirectWriteTextRenderingMode()))
                {
                    using (context.UseTextAntialiasMode(this.GetDirect2DTextAntialiasMode()))
                    {
                        TextLayout resourceSource = UIText.CreateLayout(context, text, this.sizedFontProps, this.textLayoutAlgorithm, HotkeyRenderMode.Ignore, 65535.0, 65535.0);
                        ITextLayout cachedOrCreateResource = context.GetCachedOrCreateResource<ITextLayout>(resourceSource);
                        FixTextLayout(context, text, cachedOrCreateResource);
                        measurement3 = cachedOrCreateResource.Measure();
                    }
                }
            }
            return measurement3;
        }

        private void InsertCharIntoString(char c)
        {
            this.lines[this.linePos] = this.lines[this.linePos].Insert(this.textPos, c.ToString());
            this.textSizes = null;
        }

        protected override void OnActivate()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Back, new Func<Keys, bool>(this.OnBackspaceTyped));
            base.OnActivate();
            this.textToolCursor = PdnResources.GetCursor("Cursors.TextToolCursor.cur");
            base.Cursor = this.textToolCursor;
            this.Mode = EditingMode.NotEditing;
            this.fontMap = DirectWriteFactory.Instance.GetGdiFontMapRef(false);
            this.sizedFontProps = base.ToolSettings.CreateSizedFontProperties(this.fontMap);
            this.alignment = base.ToolSettings.Text.Alignment.Value;
            base.ToolSettings.Text.Alignment.ValueChanged += new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.FontFamilyName.ValueChanged += new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.FontSize.ValueChanged += new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.FontStyle.ValueChanged += new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.Alignment.ValueChanged += new ValueChangedEventHandler<object>(this.OnAlignmentChanged);
            base.ToolSettings.Text.RenderingMode.ValueChanged += new ValueChangedEventHandler<object>(this.OnRenderingModeChanged);
            base.ToolSettings.Antialiasing.ValueChanged += new ValueChangedEventHandler<object>(this.OnAntiAliasChanged);
            base.ToolSettings.PrimaryColor.ValueChanged += new ValueChangedEventHandler<object>(this.OnPrimaryColorChanged);
            base.ToolSettings.SecondaryColor.ValueChanged += new ValueChangedEventHandler<object>(this.OnSecondaryColorChanged);
            base.ToolSettings.BlendMode.ValueChanged += new ValueChangedEventHandler<object>(this.OnBlendModeChanged);
            base.ToolSettings.Selection.RenderingQuality.ValueChanged += new ValueChangedEventHandler<object>(this.OnSelectionRenderingQualityChanged);
            this.moveHandle = new MoveHandleCanvasLayer();
            this.moveHandle.HandleDiameter = 10.5;
            this.moveHandle.HandleShape = MoveHandleShape.Compass;
            this.moveHandle.IsVisible = false;
            base.DocumentCanvas.CanvasLayers.Add(this.moveHandle);
            this.textCursor = new TextCursorCanvasLayer();
            this.textCursor.IsVisible = false;
            base.DocumentCanvas.CanvasLayers.Add(this.textCursor);
            this.UpdateCanCommit();
        }

        private void OnAlignmentChanged(object sender, EventArgs a)
        {
            this.alignment = base.ToolSettings.Text.Alignment.Value;
            if (this.Mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        private void OnAntiAliasChanged(object sender, EventArgs a)
        {
            if (this.Mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        private bool OnBackspaceTyped(Keys keys)
        {
            if (base.DocumentWorkspace.Visible && (this.Mode != EditingMode.NotEditing))
            {
                this.OnKeyPress(Keys.Back);
                return true;
            }
            return false;
        }

        private void OnBlendModeChanged(object sender, EventArgs e)
        {
            if (this.Mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        protected override void OnCommit()
        {
            if (this.Mode == EditingMode.Editing)
            {
                this.SaveHistoryMemento();
                this.StopEditing();
            }
            base.OnCommit();
        }

        protected override void OnDeactivate()
        {
            PdnBaseForm.UnregisterFormHotKey(Keys.Back, new Func<Keys, bool>(this.OnBackspaceTyped));
            base.OnDeactivate();
            this.UpdateCanCommit();
            switch (this.Mode)
            {
                case EditingMode.NotEditing:
                    break;

                case EditingMode.EmptyEdit:
                    this.RedrawText(false);
                    break;

                case EditingMode.Editing:
                    this.SaveHistoryMemento();
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<EditingMode>(this.Mode, "this.Mode");
            }
            base.ToolSettings.Text.Alignment.ValueChanged -= new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.FontFamilyName.ValueChanged -= new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.FontSize.ValueChanged -= new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.FontStyle.ValueChanged -= new ValueChangedEventHandler<object>(this.OnFontChanged);
            base.ToolSettings.Text.Alignment.ValueChanged -= new ValueChangedEventHandler<object>(this.OnAlignmentChanged);
            base.ToolSettings.Text.RenderingMode.ValueChanged -= new ValueChangedEventHandler<object>(this.OnRenderingModeChanged);
            base.ToolSettings.Antialiasing.ValueChanged -= new ValueChangedEventHandler<object>(this.OnAntiAliasChanged);
            base.ToolSettings.PrimaryColor.ValueChanged -= new ValueChangedEventHandler<object>(this.OnPrimaryColorChanged);
            base.ToolSettings.SecondaryColor.ValueChanged -= new ValueChangedEventHandler<object>(this.OnSecondaryColorChanged);
            base.ToolSettings.BlendMode.ValueChanged -= new ValueChangedEventHandler<object>(this.OnBlendModeChanged);
            base.ToolSettings.Selection.RenderingQuality.ValueChanged -= new ValueChangedEventHandler<object>(this.OnSelectionRenderingQualityChanged);
            this.StopEditing();
            if (this.moveHandle != null)
            {
                base.DocumentCanvas.CanvasLayers.Remove(this.moveHandle);
                DisposableUtil.Free<MoveHandleCanvasLayer>(ref this.moveHandle);
            }
            if (this.textCursor != null)
            {
                base.DocumentCanvas.CanvasLayers.Remove(this.textCursor);
                DisposableUtil.Free<TextCursorCanvasLayer>(ref this.textCursor);
            }
            this.sizedFontProps = null;
            DisposableUtil.Free<IGdiFontMap>(ref this.fontMap);
            DisposableUtil.Free<Cursor>(ref this.textToolCursor);
        }

        private void OnFontChanged(object sender, EventArgs a)
        {
            this.sizedFontProps = base.ToolSettings.CreateSizedFontProperties(this.fontMap);
            if (this.Mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Back:
                case Keys.Delete:
                    if (this.Mode != EditingMode.NotEditing)
                    {
                        this.OnKeyPress(e.KeyCode);
                        e.Handled = true;
                    }
                    break;

                case Keys.Tab:
                    if (((e.Modifiers & Keys.Control) == Keys.None) && (this.Mode != EditingMode.NotEditing))
                    {
                        this.OnKeyPress(e.KeyCode);
                        e.Handled = true;
                    }
                    break;

                case Keys.ControlKey:
                    if (!this.controlKeyDown)
                    {
                        this.controlKeyDown = true;
                        this.controlKeyDownTime = DateTime.Now;
                    }
                    break;

                case Keys.Space:
                    if (this.Mode != EditingMode.NotEditing)
                    {
                        e.Handled = true;
                    }
                    break;

                case Keys.PageUp:
                case Keys.Next:
                case Keys.End:
                case Keys.Home:
                case (Keys.Shift | Keys.PageUp):
                case (Keys.Shift | Keys.Next):
                case (Keys.Shift | Keys.End):
                case (Keys.Shift | Keys.Home):
                    if (this.Mode != EditingMode.NotEditing)
                    {
                        this.OnKeyPress(e.KeyCode);
                        e.Handled = true;
                    }
                    break;
            }
            if (this.Mode != EditingMode.NotEditing)
            {
                PointInt32 location = PointDouble.Truncate(this.TextPositionToPoint(new Position(this.linePos, this.textPos)));
                int width = (int) Math.Round((double) this.textSizes[this.linePos].Height, MidpointRounding.AwayFromZero);
                RectInt32 b = new RectInt32(location, new SizeInt32(width, width));
                RectInt32 a = base.DocumentWorkspace.VisibleDocumentRect.Int32Bound;
                if (RectInt32.Intersect(a, b) != b)
                {
                    RectDouble num6 = a;
                    PointDouble center = num6.Center;
                    if ((location.X > a.Right) || (location.X < a.Left))
                    {
                        center.X = location.X;
                    }
                    if ((location.Y > a.Bottom) || (location.Y < a.Top))
                    {
                        center.Y = location.Y;
                    }
                    base.DocumentWorkspace.DocumentCenterPoint = center;
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '\r':
                    if (this.tracking)
                    {
                        e.Handled = true;
                    }
                    break;

                case '\x001b':
                    if (this.tracking)
                    {
                        e.Handled = true;
                    }
                    else
                    {
                        if (this.Mode == EditingMode.Editing)
                        {
                            this.SaveHistoryMemento();
                        }
                        else if (this.Mode == EditingMode.EmptyEdit)
                        {
                            this.RedrawText(false);
                        }
                        if (this.Mode != EditingMode.NotEditing)
                        {
                            e.Handled = true;
                            this.StopEditing();
                        }
                    }
                    break;
            }
            if ((!e.Handled && (this.Mode != EditingMode.NotEditing)) && !this.tracking)
            {
                e.Handled = true;
                if (this.Mode == EditingMode.EmptyEdit)
                {
                    this.Mode = EditingMode.Editing;
                    CompoundHistoryMemento memento = new CompoundHistoryMemento(base.Name, base.Image, new List<HistoryMemento>());
                    this.currentHA = memento;
                    base.HistoryStack.PushNewMemento(memento);
                }
                if (!char.IsControl(e.KeyChar))
                {
                    this.InsertCharIntoString(e.KeyChar);
                    this.textPos++;
                    this.RedrawText(true);
                }
            }
            base.OnKeyPress(e);
        }

        protected override void OnKeyPress(Keys keyData)
        {
            bool flag = true;
            Keys keys = keyData & Keys.KeyCode;
            Keys keys2 = keyData & ~Keys.KeyCode;
            if (this.tracking)
            {
                flag = false;
                goto Label_0190;
            }
            if (((keys2 == Keys.Alt) || ((base.ModifierKeys & Keys.Alt) == Keys.Alt)) || (this.Mode == EditingMode.NotEditing))
            {
                goto Label_0190;
            }
            if (keys != Keys.Back)
            {
                switch (keys)
                {
                    case Keys.End:
                        if (keys2 == Keys.Control)
                        {
                            this.linePos = this.lines.Count - 1;
                        }
                        this.textPos = this.lines[this.linePos].Length;
                        goto Label_0171;

                    case Keys.Home:
                        if (keys2 == Keys.Control)
                        {
                            this.linePos = 0;
                        }
                        this.textPos = 0;
                        goto Label_0171;

                    case Keys.Left:
                        if (keys2 != Keys.Control)
                        {
                            this.PerformLeft();
                        }
                        else
                        {
                            this.PerformControlLeft();
                        }
                        goto Label_0171;

                    case Keys.Up:
                        this.PerformUp();
                        goto Label_0171;

                    case Keys.Right:
                        if (keys2 != Keys.Control)
                        {
                            this.PerformRight();
                        }
                        else
                        {
                            this.PerformControlRight();
                        }
                        goto Label_0171;

                    case Keys.Down:
                        this.PerformDown();
                        goto Label_0171;

                    case Keys.Delete:
                        if (keys2 != Keys.Control)
                        {
                            this.PerformDelete();
                        }
                        else
                        {
                            this.PerformControlDelete();
                        }
                        goto Label_0171;

                    case Keys.Enter:
                        this.PerformEnterKey();
                        goto Label_0171;
                }
            }
            else
            {
                if (keys2 == Keys.Control)
                {
                    this.PerformControlBackspace();
                }
                else
                {
                    this.PerformBackspace();
                }
                goto Label_0171;
            }
            flag = false;
        Label_0171:
            this.startTime = DateTime.Now;
            if ((this.Mode > EditingMode.NotEditing) & flag)
            {
                this.RedrawText(true);
            }
        Label_0190:
            if (!flag)
            {
                base.OnKeyPress(keyData);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.ControlKey) && this.controlKeyDown)
            {
                TimeSpan span = (TimeSpan) (DateTime.Now - this.controlKeyDownTime);
                if (span < this.controlKeyDownThreshold)
                {
                    this.enableNub = !this.enableNub;
                }
                this.controlKeyDown = false;
            }
            base.OnKeyUp(e);
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            PointDouble point = e.Point;
            bool flag = this.moveHandle.IsPointTouchingHandle(base.CanvasView, point);
            if ((this.Mode != EditingMode.NotEditing) && ((e.Button == MouseButtons.Right) | flag))
            {
                this.tracking = true;
                this.startMouseXY = new PointInt32(e.X, e.Y);
                this.startClickPoint = this.clickPoint;
                base.Cursor = base.HandCursorMouseDown;
                this.UpdateStatusText();
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (this.textLayerOverlay != null)
                {
                    RectInt32 affectedBounds = this.textLayerOverlay.AffectedBounds;
                    float height = this.GetStringSize(string.Empty).Height;
                    int dx = (int) Math.Ceiling((double) height);
                    affectedBounds.Inflate(dx, dx);
                    if ((this.lines != null) && affectedBounds.Contains(e.X, e.Y))
                    {
                        Position position = this.PointToTextPosition(new PointDouble((double) e.X, e.Y + (((double) height) / 2.0)));
                        this.linePos = position.Line;
                        this.textPos = position.Offset;
                        this.RedrawText(true);
                        return;
                    }
                }
                switch (this.Mode)
                {
                    case EditingMode.EmptyEdit:
                        this.RedrawText(false);
                        this.StopEditing();
                        break;

                    case EditingMode.Editing:
                        this.SaveHistoryMemento();
                        this.StopEditing();
                        break;
                }
                this.clickPoint = new PointInt32(e.X, e.Y);
                this.StartEditing();
                this.RedrawText(true);
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            if (this.tracking)
            {
                Point point = new Point(e.X, e.Y);
                Size size = new Size(point.X - this.startMouseXY.X, point.Y - this.startMouseXY.Y);
                this.clickPoint = new PointInt32(this.startClickPoint.X + size.Width, this.startClickPoint.Y + size.Height);
                this.RedrawText(false);
                this.UpdateStatusText();
            }
            else
            {
                PointDouble canvasPt = e.Point;
                if (this.moveHandle.IsPointTouchingHandle(base.CanvasView, canvasPt) && this.moveHandle.IsVisible)
                {
                    base.Cursor = base.HandCursor;
                }
                else
                {
                    base.Cursor = this.textToolCursor;
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            if (this.tracking)
            {
                this.OnMouseMove(e);
                this.tracking = false;
                this.UpdateStatusText();
            }
            base.OnMouseUp(e);
        }

        protected override void OnPaste(IPdnDataObject data, out bool handled)
        {
            base.OnPaste(data, out handled);
            if ((data.GetDataPresent(PdnDataObjectFormats.StringFormat, true) && base.Active) && (this.Mode != EditingMode.NotEditing))
            {
                string str = (string) data.GetData(PdnDataObjectFormats.StringFormat, true);
                if (str.Length > 0)
                {
                    this.ignoreRedraw++;
                    foreach (char ch in str)
                    {
                        if (ch == '\n')
                        {
                            this.PerformEnterKey();
                        }
                        else
                        {
                            base.PerformKeyPress(new KeyPressEventArgs(ch));
                        }
                    }
                    handled = true;
                    this.ignoreRedraw--;
                    this.moveHandle.IsVisible = true;
                    base.IsPulseEnabled = true;
                    this.enableNub = true;
                    this.controlKeyDown = false;
                    this.RedrawText(false);
                }
            }
        }

        protected override void OnPasteQuery(IPdnDataObject data, out bool canHandle)
        {
            base.OnPasteQuery(data, out canHandle);
            if ((data.GetDataPresent(PdnDataObjectFormats.StringFormat, true) && base.Active) && (this.Mode != EditingMode.NotEditing))
            {
                canHandle = true;
            }
        }

        private void OnPrimaryColorChanged(object sender, EventArgs e)
        {
            if (this.Mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        protected override void OnPulse()
        {
            bool flag;
            base.OnPulse();
            TimeSpan span = (TimeSpan) (DateTime.Now - this.startTime);
            long totalMilliseconds = (long) span.TotalMilliseconds;
            if (((totalMilliseconds / 300L) % 2L) == 0)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }
            flag &= base.Focused;
            if (base.IsFormActive)
            {
                flag &= (base.ModifierKeys & Keys.Control) == Keys.None;
            }
            if (flag != this.lastPulseCursorState)
            {
                this.textCursor.AnimationCursorOpacityTo(flag ? 1.0 : 0.0);
                this.lastPulseCursorState = flag;
            }
            if (base.IsFormActive && ((base.ModifierKeys & Keys.Control) != Keys.None))
            {
                this.moveHandle.IsVisible = false;
            }
            else
            {
                this.moveHandle.IsVisible = true;
            }
            this.moveHandle.IsVisible &= !this.tracking;
            this.moveHandle.IsVisible &= this.enableNub;
            long num2 = span.Ticks % 0x1312d00L;
            double num3 = Math.Sin((((double) num2) / 20000000.0) * 6.2831853071795862);
            num3 = Math.Min(0.5, num3) + 1.0;
            num3 /= 2.0;
            num3 += 0.25;
            if (this.moveHandle != null)
            {
                int x = (int) (num3 * 255.0);
                byte num5 = Int32Util.ClampToByte(x);
                this.moveHandle.HandleAlpha = num5;
            }
        }

        private void OnRenderingModeChanged(object sender, ValueChangedEventArgs<object> e)
        {
            if (this.Mode != EditingMode.NotEditing)
            {
                this.textSizes = null;
                this.RedrawText(true);
            }
        }

        private void OnSecondaryColorChanged(object sender, EventArgs e)
        {
            if (this.Mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        private void OnSelectionRenderingQualityChanged(object sender, EventArgs e)
        {
            if (this.Mode != EditingMode.NotEditing)
            {
                this.RedrawText(true);
            }
        }

        private void PerformBackspace()
        {
            if ((this.textPos == 0) && (this.linePos > 0))
            {
                int length = this.lines[this.linePos - 1].Length;
                this.lines[this.linePos - 1] = this.lines[this.linePos - 1] + this.lines[this.linePos];
                this.lines.RemoveAt(this.linePos);
                this.linePos--;
                this.textPos = length;
                this.textSizes = null;
            }
            else if (this.textPos > 0)
            {
                string str = this.lines[this.linePos];
                if (this.textPos == str.Length)
                {
                    this.lines[this.linePos] = str.Substring(0, str.Length - 1);
                }
                else
                {
                    this.lines[this.linePos] = str.Substring(0, this.textPos - 1) + str.Substring(this.textPos);
                }
                this.textPos--;
                this.textSizes = null;
            }
        }

        private void PerformControlBackspace()
        {
            if ((this.textPos == 0) && (this.linePos > 0))
            {
                this.PerformBackspace();
            }
            else if (this.textPos > 0)
            {
                string str = this.lines[this.linePos];
                int textPos = this.textPos;
                if (char.IsLetterOrDigit(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsLetterOrDigit(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if (char.IsWhiteSpace(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsWhiteSpace(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if (char.IsPunctuation(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsPunctuation(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else
                {
                    textPos--;
                }
                this.lines[this.linePos] = str.Substring(0, textPos) + str.Substring(this.textPos);
                this.textPos = textPos;
                this.textSizes = null;
            }
        }

        private void PerformControlDelete()
        {
            if ((this.linePos != (this.lines.Count - 1)) || (this.textPos != this.lines[this.lines.Count - 1].Length))
            {
                if (this.textPos == this.lines[this.linePos].Length)
                {
                    this.lines[this.linePos] = this.lines[this.linePos] + this.lines[this.linePos + 1];
                    this.lines.RemoveAt(this.linePos + 1);
                }
                else
                {
                    int textPos = this.textPos;
                    string str = this.lines[this.linePos];
                    if (char.IsLetterOrDigit(str[textPos]))
                    {
                        while ((textPos < str.Length) && char.IsLetterOrDigit(str[textPos]))
                        {
                            str = str.Remove(textPos, 1);
                        }
                    }
                    else if (char.IsWhiteSpace(str[textPos]))
                    {
                        while ((textPos < str.Length) && char.IsWhiteSpace(str[textPos]))
                        {
                            str = str.Remove(textPos, 1);
                        }
                    }
                    else if (char.IsPunctuation(str[textPos]))
                    {
                        while ((textPos < str.Length) && char.IsPunctuation(str[textPos]))
                        {
                            str = str.Remove(textPos, 1);
                        }
                    }
                    else
                    {
                        textPos--;
                    }
                    this.lines[this.linePos] = str;
                }
                if ((this.lines.Count == 1) && (this.lines[0].Length == 0))
                {
                    this.Mode = EditingMode.EmptyEdit;
                }
                this.textSizes = null;
            }
        }

        private void PerformControlLeft()
        {
            if (this.textPos > 0)
            {
                int textPos = this.textPos;
                string str = this.lines[this.linePos];
                if (char.IsLetterOrDigit(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsLetterOrDigit(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if (char.IsWhiteSpace(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsWhiteSpace(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else if ((textPos > 0) && char.IsPunctuation(str[textPos - 1]))
                {
                    while ((textPos > 0) && char.IsPunctuation(str[textPos - 1]))
                    {
                        textPos--;
                    }
                }
                else
                {
                    textPos--;
                }
                this.textPos = textPos;
            }
            else if ((this.textPos == 0) && (this.linePos > 0))
            {
                this.linePos--;
                this.textPos = this.lines[this.linePos].Length;
            }
        }

        private void PerformControlRight()
        {
            if (this.textPos < this.lines[this.linePos].Length)
            {
                int textPos = this.textPos;
                string str = this.lines[this.linePos];
                if (char.IsLetterOrDigit(str[textPos]))
                {
                    while ((textPos < str.Length) && char.IsLetterOrDigit(str[textPos]))
                    {
                        textPos++;
                    }
                }
                else if (char.IsWhiteSpace(str[textPos]))
                {
                    while ((textPos < str.Length) && char.IsWhiteSpace(str[textPos]))
                    {
                        textPos++;
                    }
                }
                else if ((textPos > 0) && char.IsPunctuation(str[textPos]))
                {
                    while ((textPos < str.Length) && char.IsPunctuation(str[textPos]))
                    {
                        textPos++;
                    }
                }
                else
                {
                    textPos++;
                }
                this.textPos = textPos;
            }
            else if ((this.textPos == this.lines[this.linePos].Length) && (this.linePos < (this.lines.Count - 1)))
            {
                this.linePos++;
                this.textPos = 0;
            }
        }

        private void PerformDelete()
        {
            if ((this.linePos != (this.lines.Count - 1)) || (this.textPos != this.lines[this.lines.Count - 1].Length))
            {
                if (this.textPos == this.lines[this.linePos].Length)
                {
                    this.lines[this.linePos] = this.lines[this.linePos] + this.lines[this.linePos + 1];
                    this.lines.RemoveAt(this.linePos + 1);
                }
                else
                {
                    this.lines[this.linePos] = this.lines[this.linePos].Substring(0, this.textPos) + this.lines[this.linePos].Substring(this.textPos + 1);
                }
                if ((this.lines.Count == 1) && (this.lines[0] == ""))
                {
                    this.Mode = EditingMode.EmptyEdit;
                }
                this.textSizes = null;
            }
        }

        private void PerformDown()
        {
            if (this.linePos != (this.lines.Count - 1))
            {
                PointDouble pf = this.TextPositionToPoint(new Position(this.linePos, this.textPos));
                pf.Y += this.textSizes[0].Height;
                Position position = this.PointToTextPosition(pf);
                this.linePos = position.Line;
                this.textPos = position.Offset;
            }
        }

        private void PerformEnterKey()
        {
            if (this.lines != null)
            {
                string str = this.lines[this.linePos];
                if (this.textPos == str.Length)
                {
                    this.lines.Insert(this.linePos + 1, string.Empty);
                }
                else
                {
                    this.lines.Insert(this.linePos + 1, str.Substring(this.textPos, str.Length - this.textPos));
                    this.lines[this.linePos] = this.lines[this.linePos].Substring(0, this.textPos);
                }
                this.linePos++;
                this.textPos = 0;
                this.textSizes = null;
            }
        }

        private void PerformLeft()
        {
            if (this.textPos > 0)
            {
                this.textPos--;
            }
            else if ((this.textPos == 0) && (this.linePos > 0))
            {
                this.linePos--;
                this.textPos = this.lines[this.linePos].Length;
            }
        }

        private void PerformRight()
        {
            if (this.textPos < this.lines[this.linePos].Length)
            {
                this.textPos++;
            }
            else if ((this.textPos == this.lines[this.linePos].Length) && (this.linePos < (this.lines.Count - 1)))
            {
                this.linePos++;
                this.textPos = 0;
            }
        }

        private void PerformUp()
        {
            PointDouble pf = this.TextPositionToPoint(new Position(this.linePos, this.textPos));
            pf.Y -= this.textSizes[0].Height;
            Position position = this.PointToTextPosition(pf);
            this.linePos = position.Line;
            this.textPos = position.Offset;
        }

        private void PlaceMoveNub()
        {
            if ((this.textPoints != null) && (this.textPoints.Length != 0))
            {
                PointInt32 num = this.textPoints[this.textPoints.Length - 1];
                num.X += (int) Math.Ceiling((double) this.textSizes[this.textPoints.Length - 1].Width);
                num.Y += (int) Math.Ceiling((double) this.textSizes[this.textPoints.Length - 1].Height);
                num.X += (int) (10.0 / base.DocumentWorkspace.ScaleFactor.Ratio);
                num.Y += (int) (10.0 / base.DocumentWorkspace.ScaleFactor.Ratio);
                num.X = (int) Math.Round(Math.Min(base.ActiveLayer.Width - this.moveHandle.HandleDiameter, (double) num.X), MidpointRounding.AwayFromZero);
                num.X = (int) Math.Round(Math.Max(this.moveHandle.HandleDiameter, (double) num.X), MidpointRounding.AwayFromZero);
                num.Y = (int) Math.Round(Math.Min(base.ActiveLayer.Height - this.moveHandle.HandleDiameter, (double) num.Y), MidpointRounding.AwayFromZero);
                num.Y = (int) Math.Round(Math.Max(this.moveHandle.HandleDiameter, (double) num.Y), MidpointRounding.AwayFromZero);
                this.moveHandle.HandleLocation = num;
            }
        }

        private Position PointToTextPosition(PointDouble pf)
        {
            double offset = pf.X - this.clickPoint.X;
            double num2 = pf.Y - this.clickPoint.Y;
            int lno = (int) Math.Floor((double) (num2 / ((double) this.textSizes[0].Height)));
            if (lno < 0)
            {
                lno = 0;
            }
            else if (lno >= this.lines.Count)
            {
                lno = this.lines.Count - 1;
            }
            int num4 = this.FindOffsetPosition(offset, this.lines[lno], lno);
            Position position = new Position(lno, num4);
            if (position.Offset >= this.lines[position.Line].Length)
            {
                position.Offset = this.lines[position.Line].Length;
            }
            return position;
        }

        private void RedrawText(bool cursorOn)
        {
            if (this.ignoreRedraw <= 0)
            {
                this.RedrawTextImpl(cursorOn);
            }
        }

        private void RedrawTextImpl(bool cursorOn)
        {
            if (this.ignoreRedraw <= 0)
            {
                BitmapLayer activeLayer = (BitmapLayer) base.ActiveLayer;
                Surface surface = activeLayer.Surface;
                SizeInt32 num = surface.Size<ColorBgra>();
                ContentBlendMode blendMode = base.ToolSettings.BlendMode.Value;
                bool flag = base.ToolSettings.Antialiasing.Value;
                double num2 = Math.Floor((double) (this.clickPoint.Y - (((double) this.GetStringSize(string.Empty).Height) / 2.0)));
                double num3 = 0.0;
                if (this.textSizes == null)
                {
                    this.textSizes = new TextMeasurement[this.lines.Count];
                    for (int k = 0; k < this.lines.Count; k++)
                    {
                        this.textSizes[k] = this.GetStringSize(this.lines[k]);
                    }
                }
                this.textPoints = new PointInt32[this.lines.Count];
                for (int i = 0; i < this.lines.Count; i++)
                {
                    double num9;
                    this.textSizes[i] = this.GetStringSize(this.lines[i]);
                    switch (this.alignment)
                    {
                        case PaintDotNet.TextAlignment.Left:
                            num9 = 0.0;
                            break;

                        case PaintDotNet.TextAlignment.Center:
                            num9 = ((double) -this.textSizes[i].Width) / 2.0;
                            break;

                        case PaintDotNet.TextAlignment.Right:
                            num9 = -this.textSizes[i].Width;
                            break;

                        default:
                            throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.TextAlignment>(this.alignment, "this.alignment");
                    }
                    double x = this.clickPoint.X + num9;
                    this.textPoints[i] = PointDouble.Round(new PointDouble(x, num2 + num3), MidpointRounding.AwayFromZero);
                    num3 += this.textSizes[i].Height;
                }
                string text = this.lines[this.linePos].Substring(0, this.textPos);
                TextMeasurement stringSize = this.GetStringSize(text);
                RectDouble rect = new RectDouble((double) (this.textPoints[this.linePos].X + stringSize.Width), (double) this.textPoints[this.linePos].Y, 2.0, (double) this.textSizes[this.linePos].Height);
                RectInt32 num4 = RectDouble.Truncate(rect);
                RectInt32[] rects = new RectInt32[this.lines.Count + 1];
                for (int j = 0; j < this.lines.Count; j++)
                {
                    rects[j] = RectDouble.Offset(this.textSizes[j].SafeClipBounds, (double) this.textPoints[j].X, (double) this.textPoints[j].Y).Int32Bound;
                }
                rects[rects.Length - 1] = num4;
                RectInt32 affectedBounds = RectInt32.Intersect(rects.Bounds(), surface.Bounds<ColorBgra>()).CoalesceCopy();
                IRenderer<ColorAlpha8> cachedClippingMaskRenderer = base.Selection.GetCachedClippingMaskRenderer(base.ToolSettings.Selection.RenderingQuality.Value);
                int count = this.lines.Count;
                ITextLayout[] textLayouts = new ITextLayout[count];
                TextRenderingMode directWriteTextRenderingMode = this.GetDirectWriteTextRenderingMode();
                TextAntialiasMode aaMode = this.GetDirect2DTextAntialiasMode();
                using (IDrawingContext context = DrawingContext.CreateNull(FactorySource.PerThread))
                {
                    using (context.UseTextAntialiasMode(aaMode))
                    {
                        using (context.UseTextRenderingMode(directWriteTextRenderingMode))
                        {
                            for (int m = 0; m < count; m++)
                            {
                                string str2 = this.lines[m];
                                TextLayout resourceSource = UIText.CreateLayout(context, str2, this.sizedFontProps, this.textLayoutAlgorithm, HotkeyRenderMode.Ignore, 65535.0, 65535.0);
                                ITextLayout cachedOrCreateResource = context.GetCachedOrCreateResource<ITextLayout>(resourceSource);
                                FixTextLayout(context, str2, cachedOrCreateResource);
                                textLayouts[m] = cachedOrCreateResource.CreateRef();
                            }
                        }
                    }
                }
                TextContentRenderer renderer2 = new TextContentRenderer(num.Width, num.Height, base.ToolSettings.PrimaryColor.Value, aaMode, directWriteTextRenderingMode, textLayouts, this.textPoints, rects);
                IMaskedRenderer<ColorBgra, ColorAlpha8>[] contentRenderers = new IMaskedRenderer<ColorBgra, ColorAlpha8>[] { renderer2 };
                ContentRendererBgra renderer = new ContentRendererBgra(surface, blendMode, contentRenderers, cachedClippingMaskRenderer);
                this.textCursor.IsVisible = true;
                this.textCursor.CursorBounds = num4;
                TextLayerOverlay newOverlay = new TextLayerOverlay(activeLayer, affectedBounds, renderer);
                base.DocumentCanvas.ReplaceLayerOverlay(base.ActiveLayer, this.textLayerOverlay, newOverlay, null);
                this.textLayerOverlay = newOverlay;
                this.PlaceMoveNub();
                this.UpdateStatusText();
                base.Update();
            }
        }

        private void SaveHistoryMemento()
        {
            base.IsPulseEnabled = false;
            this.RedrawText(false);
            if (this.textLayerOverlay.AffectedBounds.HasPositiveArea)
            {
                ApplyRendererToBitmapLayerHistoryFunction function = new ApplyRendererToBitmapLayerHistoryFunction(base.Name, base.Image, base.ActiveLayerIndex, this.textLayerOverlay.Renderer, this.textLayerOverlay.AffectedBounds, 4, 0x7d0, ActionFlags.KeepToolActive);
                HistoryMemento memento2 = function.Execute(base.DocumentWorkspace) ?? new EmptyHistoryMemento(base.Name, base.Image);
                if (this.currentHA == null)
                {
                    base.HistoryStack.PushNewMemento(memento2);
                }
                else
                {
                    this.currentHA.AddMemento(memento2);
                    this.currentHA = null;
                }
            }
            base.DocumentCanvas.RemoveLayerOverlay(base.ActiveLayer, this.textLayerOverlay, null);
            this.textLayerOverlay = null;
        }

        private void StartEditing()
        {
            this.linePos = 0;
            this.textPos = 0;
            this.lines = new List<string>();
            this.textSizes = null;
            this.lines.Add(string.Empty);
            this.startTime = DateTime.Now;
            this.Mode = EditingMode.EmptyEdit;
            base.IsPulseEnabled = true;
            this.UpdateStatusText();
        }

        private void StopEditing()
        {
            if (this.textLayerOverlay != null)
            {
                base.DocumentCanvas.RemoveLayerOverlay(base.ActiveLayer, this.textLayerOverlay, null);
                this.textLayerOverlay = null;
            }
            this.Mode = EditingMode.NotEditing;
            this.lines = null;
            this.moveHandle.IsVisible = false;
            this.textCursor.IsVisible = false;
            base.IsPulseEnabled = false;
        }

        private PointDouble TextPositionToPoint(Position p)
        {
            PointDouble num = new PointDouble(0.0, 0.0);
            TextMeasurement stringSize = this.GetStringSize(this.lines[p.Line].Substring(0, p.Offset));
            TextMeasurement measurement2 = this.GetStringSize(this.lines[p.Line]);
            switch (this.alignment)
            {
                case PaintDotNet.TextAlignment.Left:
                    return new PointDouble((double) (this.clickPoint.X + stringSize.Width), (double) (this.clickPoint.Y + (stringSize.Height * p.Line)));

                case PaintDotNet.TextAlignment.Center:
                    return new PointDouble((double) (this.clickPoint.X + (stringSize.Width - (measurement2.Width / 2f))), (double) (this.clickPoint.Y + (stringSize.Height * p.Line)));

                case PaintDotNet.TextAlignment.Right:
                    return new PointDouble((double) (this.clickPoint.X + (stringSize.Width - measurement2.Width)), (double) (this.clickPoint.Y + (stringSize.Height * p.Line)));
            }
            throw ExceptionUtil.InvalidEnumArgumentException<PaintDotNet.TextAlignment>(this.alignment, "this.alignment");
        }

        private void UpdateCanCommit()
        {
            if (!base.Active)
            {
                base.CanCommit = false;
            }
            else
            {
                switch (this.mode)
                {
                    case EditingMode.Editing:
                        base.CanCommit = true;
                        return;
                }
                base.CanCommit = false;
            }
        }

        private void UpdateStatusText()
        {
            string statusBarXYText;
            ImageResource image;
            if (this.tracking)
            {
                statusBarXYText = this.GetStatusBarXYText();
                image = base.Image;
            }
            else
            {
                statusBarXYText = PdnResources.GetString("TextTool.StatusText.StartTyping");
                image = null;
            }
            base.SetStatus(image, statusBarXYText);
        }

        private EditingMode Mode
        {
            get => 
                this.mode;
            set
            {
                this.VerifyAccess<TextTool>();
                if (value != this.mode)
                {
                    this.mode = value;
                    this.UpdateCanCommit();
                }
            }
        }

        private enum EditingMode
        {
            NotEditing,
            EmptyEdit,
            Editing
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Position : IEquatable<TextTool.Position>
        {
            private int line;
            private int offset;
            public int Line
            {
                get => 
                    this.line;
                set
                {
                    if (value >= 0)
                    {
                        this.line = value;
                    }
                    else
                    {
                        this.line = 0;
                    }
                }
            }
            public int Offset
            {
                get => 
                    this.offset;
                set
                {
                    if (value >= 0)
                    {
                        this.offset = value;
                    }
                    else
                    {
                        this.offset = 0;
                    }
                }
            }
            public Position(int line, int offset)
            {
                this.line = line;
                this.offset = offset;
            }

            public bool Equals(TextTool.Position other) => 
                ((this.line == other.line) && (this.offset == other.offset));

            public override bool Equals(object obj) => 
                EquatableUtil.Equals<TextTool.Position, object>(this, obj);

            public override int GetHashCode() => 
                HashCodeUtil.CombineHashCodes(this.line, this.offset);
        }

        private sealed class TextContentRenderer : IMaskedRenderer<ColorBgra, ColorAlpha8>
        {
            private ColorBgra color;
            private SolidColorBrush drawTextBrush;
            private static readonly DrawTextOptions drawTextOptions = (OS.IsWin81OrLater ? DrawTextOptions.EnableColorFont : DrawTextOptions.None);
            private int height;
            private byte opacity;
            private TextAntialiasMode textAntialiasMode;
            private RectInt32[] textClipRects;
            private ITextLayout[] textLayouts;
            private PointInt32[] textPositions;
            private TextRenderingMode textRenderingMode;
            private int width;

            public TextContentRenderer(int width, int height, ColorBgra color, TextAntialiasMode textAntialiasMode, TextRenderingMode textRenderingMode, IEnumerable<ITextLayout> textLayouts, IEnumerable<PointInt32> textPositions, IEnumerable<RectInt32> textClipRects)
            {
                Validate.Begin().IsNotNull<IEnumerable<ITextLayout>>(textLayouts, "textLayouts").IsNotNull<IEnumerable<PointInt32>>(textPositions, "textPositions").IsNotNull<IEnumerable<RectInt32>>(textClipRects, "textClipRects").Check();
                this.width = width;
                this.height = height;
                this.color = color;
                this.opacity = color.A;
                ColorBgra bgra = ColorBgra.FromBgra(color.B, color.G, color.R, 0xff);
                this.drawTextBrush = SolidColorBrushCache.Get((ColorRgba128Float) bgra);
                this.textAntialiasMode = textAntialiasMode;
                this.textRenderingMode = textRenderingMode;
                this.textLayouts = textLayouts.ToArrayEx<ITextLayout>();
                this.textPositions = textPositions.ToArrayEx<PointInt32>();
                this.textClipRects = textClipRects.ToArrayEx<RectInt32>();
            }

            public unsafe void Render(ISurface<ColorBgra> dstContent, ISurface<ColorAlpha8> dstMask, PointInt32 renderOffset)
            {
                SizeInt32 size = dstContent.Size<ColorBgra>();
                RectInt32 num2 = new RectInt32(renderOffset, size);
                using (IDrawingContext context = DrawingContext.FromSurface(dstContent, AlphaMode.Premultiplied, FactorySource.PerThread))
                {
                    using (context.UseTranslateTransform((float) -renderOffset.X, (float) -renderOffset.Y, MatrixMultiplyOrder.Prepend))
                    {
                        using (context.UseTextAntialiasMode(this.textAntialiasMode))
                        {
                            using (context.UseTextRenderingMode(this.textRenderingMode))
                            {
                                context.Clear(null);
                                for (int j = 0; j < this.textLayouts.Length; j++)
                                {
                                    if (num2.IntersectsWith(this.textClipRects[j]))
                                    {
                                        using (context.UseAxisAlignedClip(this.textClipRects[j], AntialiasMode.Aliased))
                                        {
                                            context.Flush();
                                            ITextLayout layout = this.textLayouts[j];
                                            lock (layout)
                                            {
                                                context.DrawTextLayout(this.textPositions[j], this.textLayouts[j], this.drawTextBrush, drawTextOptions);
                                            }
                                            try
                                            {
                                                context.Flush();
                                            }
                                            catch (FontFileFormatException)
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < size.Height; i++)
                {
                    byte* rowPointer = (byte*) dstMask.GetRowPointer<ColorAlpha8>(i);
                    ColorBgra* bgraPtr = (ColorBgra*) dstContent.GetRowPointer<ColorBgra>(i);
                    for (int k = 0; k < size.Width; k++)
                    {
                        ColorBgra bgra = bgraPtr[0];
                        rowPointer[0] = bgra.A;
                        bgraPtr->B = ByteUtil.FastUnscale(bgra.B, bgra.A);
                        bgraPtr->G = ByteUtil.FastUnscale(bgra.G, bgra.A);
                        bgraPtr->R = ByteUtil.FastUnscale(bgra.R, bgra.A);
                        bgraPtr->A = this.opacity;
                        rowPointer++;
                        bgraPtr++;
                    }
                }
            }

            public bool HasContentMask =>
                true;

            public int Height =>
                this.height;

            public int Width =>
                this.width;
        }

        private sealed class TextLayerOverlay : DocumentBitmapLayerOverlay
        {
            private IRenderer<ColorBgra> renderer;

            public TextLayerOverlay(BitmapLayer layer, RectInt32 affectedBounds, IRenderer<ColorBgra> renderer) : base(layer, affectedBounds)
            {
                Validate.IsNotNull<IRenderer<ColorBgra>>(renderer, "renderer");
                this.renderer = renderer;
            }

            protected override void OnCancelled()
            {
            }

            protected override void OnRender(ISurface<ColorBgra> dst, PointInt32 renderOffset)
            {
                this.renderer.Render(dst, renderOffset);
            }

            public IRenderer<ColorBgra> Renderer =>
                this.renderer;
        }
    }
}

