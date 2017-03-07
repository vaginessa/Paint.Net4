namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Collections;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Media;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class PdnStatusBar : StatusStripEx, IStatusBarProgress
    {
        private AppWorkspace appWorkspace;
        private ImageResource contextStatusImage;
        private PdnToolStripStatusLabel contextStatusLabel;
        private ToolStripStatusLabel cursorInfoStatusLabel;
        private ToolStripStatusLabel imageInfoStatusLabel;
        private LocalizedMeasurementUnit[] localizedUnits;
        private PaintDotNet.ScaleFactor maxDocScaleFactor;
        private PaintDotNet.ScaleFactor minDocScaleFactor;
        private string percentageFormat;
        private ToolStripProgressBar progressStatusBar;
        private ToolStripSeparator progressStatusSeparator;
        private string progressTextFormat = PdnResources.GetString("StatusBar.Progress.Percentage.Format");
        private PaintDotNet.ScaleFactor scaleFactor;
        private int suspendEvents;
        private MeasurementUnit unit;
        private static readonly char[] unitAbreviationTrimChars = new char[] { ' ', '.', '(', ')' };
        private PdnToolStripSplitButton unitsButton;
        private string windowText;
        private Image zoomActualSizeImage;
        private ToolStripButton zoomAmountButton;
        private PaintDotNet.ZoomBasis zoomBasis;
        private ToolStripButton zoomInButton;
        private ToolStripButton zoomOutButton;
        private ToolStripSeparator zoomSeparator;
        private ZoomSliderControl zoomSlider;
        private Image zoomToWindowImage;
        private ToolStripButton zoomToWindowToggleButton;

        [field: CompilerGenerated]
        public event EventHandler UnitsChanged;

        [field: CompilerGenerated]
        public event EventHandler ZoomBasisChanged;

        [field: CompilerGenerated]
        public event EventHandler ZoomIn;

        [field: CompilerGenerated]
        public event EventHandler ZoomOut;

        [field: CompilerGenerated]
        public event EventHandler ZoomScaleChanged;

        public PdnStatusBar(AppWorkspace appWorkspace)
        {
            this.appWorkspace = appWorkspace;
            this.PopulateLocalizedUnitsArray();
            this.zoomToWindowImage = UIUtil.GetScaledImage(PdnResources.GetImageResource("Icons.MenuViewZoomToWindowIcon.png").Reference);
            this.zoomActualSizeImage = UIUtil.GetScaledImage(PdnResources.GetImageResource("Icons.MenuViewActualSizeIcon.png").Reference);
            this.windowText = EnumLocalizer.GetLocalizedEnumValue(typeof(PaintDotNet.ZoomBasis), PaintDotNet.ZoomBasis.FitToWindow).LocalizedName;
            this.percentageFormat = PdnResources.GetString("ZoomConfigWidget.Percentage.Format");
            this.InitializeComponent();
            this.imageInfoStatusLabel.Image = UIUtil.GetScaledImage(PdnResources.GetImageResource("Icons.ImageSizeIcon.png").Reference);
            this.cursorInfoStatusLabel.Image = UIUtil.GetScaledImage(PdnResources.GetImageResource("Icons.CursorXYIcon.png").Reference);
            this.zoomToWindowToggleButton.Image = this.zoomToWindowImage;
            this.zoomOutButton.Image = UIUtil.GetScaledImage(PdnResources.GetImageResource("Icons.MenuViewZoomOutIcon.png").Reference);
            this.zoomInButton.Image = UIUtil.GetScaledImage(PdnResources.GetImageResource("Icons.MenuViewZoomInIcon.png").Reference);
            this.zoomOutButton.ToolTipText = PdnResources.GetString("ZoomConfigWidget.ZoomOutButton.ToolTipText");
            this.zoomInButton.ToolTipText = PdnResources.GetString("ZoomConfigWidget.ZoomInButton.ToolTipText");
            this.zoomBasis = PaintDotNet.ZoomBasis.FitToWindow;
            this.scaleFactor = PaintDotNet.ScaleFactor.OneToOne;
            this.OnZoomBasisChanged();
        }

        public void EraseProgressStatusBar()
        {
            try
            {
                this.progressStatusSeparator.Visible = false;
                this.progressStatusBar.Visible = false;
                this.progressStatusBar.Value = 0;
            }
            catch (NullReferenceException)
            {
            }
        }

        public void EraseProgressStatusBarAsync()
        {
            base.BeginInvoke(new Action(this.EraseProgressStatusBar));
        }

        public double GetProgressStatusBarValue()
        {
            ToolStripProgressBar progressStatusBar = this.progressStatusBar;
            lock (progressStatusBar)
            {
                return this.progressStatusBar.Value;
            }
        }

        private void InitializeComponent()
        {
            this.contextStatusLabel = new PdnToolStripStatusLabel();
            this.progressStatusSeparator = new ToolStripSeparator();
            this.progressStatusBar = new ToolStripProgressBar();
            ToolStripSeparator separator = new ToolStripSeparator();
            this.imageInfoStatusLabel = new ToolStripStatusLabel();
            ToolStripSeparator separator2 = new ToolStripSeparator();
            this.cursorInfoStatusLabel = new ToolStripStatusLabel();
            this.unitsButton = new PdnToolStripSplitButton();
            this.zoomToWindowToggleButton = new ToolStripButton();
            this.zoomAmountButton = new ToolStripButton();
            this.zoomSeparator = new ToolStripSeparator();
            this.zoomOutButton = new ToolStripButton();
            this.zoomSlider = new ZoomSliderControl();
            this.zoomInButton = new ToolStripButton();
            base.SuspendLayout();
            this.contextStatusLabel.Name = "contextStatusLabel";
            this.contextStatusLabel.Spring = true;
            this.contextStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.contextStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.contextStatusLabel.ImageScaling = ToolStripItemImageScaling.None;
            this.progressStatusSeparator.DisplayStyle = ToolStripItemDisplayStyle.None;
            this.progressStatusSeparator.Visible = false;
            this.progressStatusBar.Name = "progressStatusBar";
            this.progressStatusBar.AutoSize = false;
            this.progressStatusBar.Width = 130;
            this.progressStatusBar.Visible = false;
            this.progressStatusBar.ProgressBar.Style = ProgressBarStyle.Continuous;
            separator.DisplayStyle = ToolStripItemDisplayStyle.None;
            this.imageInfoStatusLabel.Name = "imageInfoStatusLabel";
            this.imageInfoStatusLabel.AutoSize = false;
            this.imageInfoStatusLabel.Width = UIUtil.ScaleWidth(90);
            this.imageInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.imageInfoStatusLabel.ImageScaling = ToolStripItemImageScaling.None;
            separator2.DisplayStyle = ToolStripItemDisplayStyle.None;
            this.cursorInfoStatusLabel.Name = "cursorInfoStatusLabel";
            this.cursorInfoStatusLabel.AutoSize = false;
            this.cursorInfoStatusLabel.Width = UIUtil.ScaleWidth(90);
            this.cursorInfoStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.ImageAlign = ContentAlignment.MiddleLeft;
            this.cursorInfoStatusLabel.ImageScaling = ToolStripItemImageScaling.None;
            this.unitsButton.AutoSize = true;
            this.unitsButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.unitsButton.TextAlign = ContentAlignment.MiddleCenter;
            this.unitsButton.ToolTipText = string.Empty;
            this.unitsButton.AutoToolTip = false;
            this.unitsButton.DropDownDirection = ToolStripDropDownDirection.AboveRight;
            this.unitsButton.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.OnUnitsButtonDropDownItemClicked);
            this.PopulateUnitsButtonDropDown();
            this.SetUnitsButtonLabelAndDropDownItemChecks();
            this.zoomToWindowToggleButton.Click += new EventHandler(this.OnZoomToggleButtonClicked);
            this.zoomToWindowToggleButton.ImageScaling = ToolStripItemImageScaling.None;
            this.zoomToWindowToggleButton.ToolTipText = PdnResources.GetString("Menu.View.ZoomToWindow.Text").Replace("&", "");
            this.zoomAmountButton.AutoSize = true;
            this.zoomAmountButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.zoomAmountButton.TextAlign = ContentAlignment.MiddleCenter;
            this.zoomAmountButton.AutoToolTip = false;
            this.zoomAmountButton.Click += new EventHandler(this.OnZoomAmountButtonClick);
            this.zoomSeparator.DisplayStyle = ToolStripItemDisplayStyle.None;
            this.zoomOutButton.ImageScaling = ToolStripItemImageScaling.None;
            this.zoomSlider.AutoSize = false;
            this.zoomSlider.Width = UIUtil.ScaleWidth(0x63);
            this.zoomSlider.BackColor = Color.Transparent;
            this.zoomSlider.ScaleFactorChanged += new EventHandler(this.OnSliderScaleFactorChanged);
            this.zoomInButton.ImageScaling = ToolStripItemImageScaling.None;
            base.Name = "PdnStatusBar";
            this.Items.Add(this.contextStatusLabel);
            this.Items.Add(this.progressStatusSeparator);
            this.Items.Add(this.progressStatusBar);
            this.Items.Add(separator);
            this.Items.Add(this.imageInfoStatusLabel);
            this.Items.Add(separator2);
            this.Items.Add(this.cursorInfoStatusLabel);
            this.Items.Add(this.unitsButton);
            this.Items.Add(this.zoomAmountButton);
            ToolStripSeparator separator1 = new ToolStripSeparator {
                DisplayStyle = ToolStripItemDisplayStyle.None
            };
            this.Items.Add(separator1);
            this.Items.Add(this.zoomToWindowToggleButton);
            this.Items.Add(this.zoomSeparator);
            this.Items.Add(this.zoomOutButton);
            this.Items.Add(this.zoomSlider);
            this.Items.Add(this.zoomInButton);
            base.ResumeLayout(false);
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == this.zoomInButton)
            {
                this.OnZoomIn();
            }
            else if (e.ClickedItem == this.zoomOutButton)
            {
                this.OnZoomOut();
            }
            base.OnItemClicked(e);
        }

        private void OnSliderScaleFactorChanged(object sender, EventArgs e)
        {
            this.ScaleFactor = this.zoomSlider.ScaleFactor;
        }

        private void OnUnitsButtonDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            LocalizedMeasurementUnit tag = e.ClickedItem.Tag as LocalizedMeasurementUnit;
            this.unit = tag.Unit;
            this.OnUnitsChanged();
        }

        private void OnUnitsChanged()
        {
            this.UnitsChanged.Raise(this);
        }

        private void OnZoomAmountButtonClick(object sender, EventArgs e)
        {
            PdnBaseForm typeZoomTextForm = new PdnBaseForm {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false
            };
            TextBox zoomTextBox = new TextBox {
                Text = this.scaleFactor.ToString(),
                Width = UIUtil.ScaleWidth(0x30)
            };
            typeZoomTextForm.Controls.Add(zoomTextBox);
            zoomTextBox.PerformLayout();
            typeZoomTextForm.Size = zoomTextBox.Size;
            zoomTextBox.KeyPress += delegate (object sender2, KeyPressEventArgs e2) {
                if ((e2.KeyChar == '\r') || (e2.KeyChar == '\n'))
                {
                    PaintDotNet.ScaleFactor factor;
                    if (PaintDotNet.ScaleFactor.TryParse(zoomTextBox.Text, out factor))
                    {
                        this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
                        this.ScaleFactor = factor;
                        typeZoomTextForm.Close();
                    }
                    else
                    {
                        SystemSounds.Beep.Play();
                    }
                }
                else if (e2.KeyChar == '\x001b')
                {
                    typeZoomTextForm.Close();
                }
            };
            Rectangle rectangle = base.RectangleToScreen(this.zoomAmountButton.Bounds);
            Rectangle rectangle2 = new Rectangle(rectangle.X + ((rectangle.Width - typeZoomTextForm.Width) / 2), rectangle.Y - typeZoomTextForm.Height, typeZoomTextForm.Width, typeZoomTextForm.Height);
            typeZoomTextForm.Bounds = rectangle2;
            typeZoomTextForm.Load += delegate (object sender2, EventArgs e2) {
                typeZoomTextForm.Size = zoomTextBox.Size;
                zoomTextBox.Select();
                zoomTextBox.SelectAll();
            };
            typeZoomTextForm.Deactivate += delegate (object sender2, EventArgs e2) {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(delegate {
                        try
                        {
                            typeZoomTextForm.Close();
                        }
                        catch (Exception)
                        {
                        }
                    });
                }
                else
                {
                    typeZoomTextForm.Close();
                }
            };
            typeZoomTextForm.FormClosed += (sender2, e2) => typeZoomTextForm.Dispose();
            typeZoomTextForm.Show();
        }

        private void OnZoomBasisChanged()
        {
            this.SetZoomText();
            if (this.suspendEvents == 0)
            {
                this.ZoomBasisChanged.Raise(this);
            }
            if (this.ZoomBasis == PaintDotNet.ZoomBasis.FitToWindow)
            {
                this.zoomToWindowToggleButton.Image = this.zoomActualSizeImage;
            }
            else
            {
                this.zoomToWindowToggleButton.Image = this.zoomToWindowImage;
            }
        }

        private void OnZoomIn()
        {
            this.ZoomIn.Raise(this);
        }

        private void OnZoomOut()
        {
            this.ZoomOut.Raise(this);
        }

        private void OnZoomScaleChanged()
        {
            if (this.suspendEvents == 0)
            {
                this.ZoomScaleChanged.Raise(this);
            }
        }

        private void OnZoomToggleButtonClicked(object sender, EventArgs e)
        {
            if (this.ZoomBasis == PaintDotNet.ZoomBasis.FitToWindow)
            {
                this.ZoomBasis = PaintDotNet.ZoomBasis.ScaleFactor;
                this.ScaleFactor = PaintDotNet.ScaleFactor.OneToOne;
            }
            else
            {
                this.ZoomBasis = PaintDotNet.ZoomBasis.FitToWindow;
            }
        }

        public void PerformZoomBasisChanged()
        {
            this.OnZoomBasisChanged();
        }

        private void PopulateLocalizedUnitsArray()
        {
            MeasurementUnit[] unitArray = Enum.GetValues(typeof(MeasurementUnit)).Cast<MeasurementUnit>().ToArrayEx<MeasurementUnit>();
            this.localizedUnits = new LocalizedMeasurementUnit[unitArray.Length];
            for (int i = 0; i < unitArray.Length; i++)
            {
                MeasurementUnit unit = unitArray[i];
                string str = "MeasurementUnit." + unit.ToString();
                string abbreviation = PdnResources.GetString(str + ".Abbreviation").Trim(unitAbreviationTrimChars);
                string description = PdnResources.GetString(str + ".Plural");
                this.localizedUnits[i] = new LocalizedMeasurementUnit(unit, description, abbreviation);
            }
        }

        private void PopulateUnitsButtonDropDown()
        {
            foreach (MeasurementUnit unit in Enum.GetValues(typeof(MeasurementUnit)))
            {
                string str = "MeasurementUnit." + unit.ToString();
                string abbreviation = PdnResources.GetString(str + ".Abbreviation").Trim(unitAbreviationTrimChars);
                string description = PdnResources.GetString(str + ".Plural");
                ToolStripMenuItem item = new ToolStripMenuItem {
                    Text = description,
                    Tag = new LocalizedMeasurementUnit(unit, description, abbreviation)
                };
                this.unitsButton.DropDownItems.Add(item);
            }
        }

        public void RedirectItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.OnItemClicked(e);
        }

        public void ResetProgressStatusBar()
        {
            try
            {
                this.progressStatusBar.Value = 0;
                this.progressStatusSeparator.Visible = true;
                this.progressStatusBar.Visible = true;
            }
            catch (NullReferenceException)
            {
            }
        }

        public void ResumeEvents()
        {
            this.suspendEvents--;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (((specified & BoundsSpecified.Height) == BoundsSpecified.Height) && ((height & 1) == 0))
            {
                height++;
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        public void SetProgressStatusBar(double? percent)
        {
            ToolStripProgressBar progressStatusBar = this.progressStatusBar;
            lock (progressStatusBar)
            {
                bool flag2;
                if (percent.HasValue)
                {
                    int num = (int) percent.Value;
                    flag2 = num != 100;
                    if (num != this.progressStatusBar.Value)
                    {
                        this.progressStatusBar.Value = (int) percent.Value;
                    }
                }
                else
                {
                    this.progressStatusBar.Style = ProgressBarStyle.Marquee;
                    flag2 = true;
                }
                this.progressStatusBar.Visible = flag2;
                this.progressStatusSeparator.Visible = flag2;
            }
        }

        private void SetUnitsButtonLabelAndDropDownItemChecks()
        {
            string abbreviation = string.Empty;
            ToolStripMenuItem[] array = new ToolStripMenuItem[this.unitsButton.DropDownItems.Count];
            this.unitsButton.DropDownItems.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                ToolStripMenuItem item = array[i];
                LocalizedMeasurementUnit tag = (LocalizedMeasurementUnit) item.Tag;
                if (tag.Unit == this.unit)
                {
                    abbreviation = tag.Abbreviation;
                    item.Checked = true;
                    int index = (i + 1) % array.Length;
                    this.unitsButton.DefaultItem = array[index];
                }
                else
                {
                    item.Checked = false;
                }
            }
            this.unitsButton.Text = abbreviation;
        }

        private void SetZoomText()
        {
            this.zoomAmountButton.Text = this.scaleFactor.ToString();
            this.zoomSlider.ScaleFactor = this.scaleFactor;
        }

        public void SuspendEvents()
        {
            this.suspendEvents++;
        }

        public ImageResource ContextStatusImage
        {
            get => 
                this.contextStatusImage;
            set
            {
                if (this.contextStatusImage != value)
                {
                    this.contextStatusImage = value;
                    if (this.contextStatusImage == null)
                    {
                        this.contextStatusLabel.Image = null;
                    }
                    else
                    {
                        this.contextStatusLabel.Image = UIUtil.GetScaledImage(this.contextStatusImage.Reference);
                    }
                    this.QueueUpdate();
                }
            }
        }

        public string ContextStatusText
        {
            get => 
                this.contextStatusLabel.Text;
            set
            {
                this.contextStatusLabel.Text = value;
                this.QueueUpdate();
            }
        }

        public string CursorInfoText
        {
            get => 
                this.cursorInfoStatusLabel.Text;
            set
            {
                this.cursorInfoStatusLabel.Text = value;
                this.QueueUpdate();
            }
        }

        protected override Padding DefaultPadding =>
            (base.DefaultPadding + new Padding(0, 0, 0, 1));

        protected override Size DefaultSize =>
            new Size(200, 0x19);

        public string ImageInfoStatusText
        {
            get => 
                this.imageInfoStatusLabel.Text;
            set
            {
                this.imageInfoStatusLabel.Text = value;
            }
        }

        public PaintDotNet.ScaleFactor MaxDocScaleFactor
        {
            get => 
                this.maxDocScaleFactor;
            set
            {
                this.maxDocScaleFactor = value;
                this.zoomSlider.SetMaxDocScaleFactor(value);
            }
        }

        public PaintDotNet.ScaleFactor MinDocScaleFactor
        {
            get => 
                this.minDocScaleFactor;
            set
            {
                this.minDocScaleFactor = value;
                this.zoomSlider.SetMinDocScaleFactor(value);
            }
        }

        public PaintDotNet.ScaleFactor ScaleFactor
        {
            get => 
                this.scaleFactor;
            set
            {
                if (this.scaleFactor != value)
                {
                    this.scaleFactor = value;
                    this.OnZoomScaleChanged();
                }
                this.SetZoomText();
                base.Invalidate();
            }
        }

        public MeasurementUnit Units
        {
            get => 
                this.unit;
            set
            {
                if (this.unit != value)
                {
                    this.unit = value;
                    this.OnUnitsChanged();
                }
                this.SetUnitsButtonLabelAndDropDownItemChecks();
            }
        }

        public PaintDotNet.ZoomBasis ZoomBasis
        {
            get => 
                this.zoomBasis;
            set
            {
                if (this.zoomBasis != value)
                {
                    this.zoomBasis = value;
                    this.OnZoomBasisChanged();
                }
            }
        }

        private sealed class LocalizedMeasurementUnit
        {
            public LocalizedMeasurementUnit(MeasurementUnit unit, string description, string abbreviation)
            {
                this.Unit = unit;
                this.Description = description;
                this.Abbreviation = abbreviation;
            }

            public string Abbreviation { get; private set; }

            public string Description { get; private set; }

            public MeasurementUnit Unit { get; private set; }
        }
    }
}

