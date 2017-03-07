namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.Resources;
    using System;
    using System.Windows.Forms;

    internal sealed class ViewMenu : PdnMenuItem
    {
        private PdnMenuItem menuViewActualSize;
        private PdnMenuItem menuViewCentimeters;
        private PdnMenuItem menuViewGrid;
        private PdnMenuItem menuViewInches;
        private PdnMenuItem menuViewPixels;
        private PdnMenuItem menuViewRulers;
        private ToolStripSeparator menuViewSeparator1;
        private ToolStripSeparator menuViewSeparator2;
        private PdnMenuItem menuViewZoomIn;
        private PdnMenuItem menuViewZoomOut;
        private PdnMenuItem menuViewZoomToSelection;
        private PdnMenuItem menuViewZoomToWindow;

        public ViewMenu()
        {
            this.InitializeComponent();
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.OemMinus, new Func<Keys, bool>(this.OnCtrlOemMinusShortcut));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Oemplus, new Func<Keys, bool>(this.OnCtrlOemPlusShortcut));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.D0, new Func<Keys, bool>(this.OnCtrlZero));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.NumPad0, new Func<Keys, bool>(this.OnCtrlNumPad0));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.A, new Func<Keys, bool>(this.OnCtrlShiftA));
        }

        private void InitializeComponent()
        {
            this.menuViewZoomIn = new PdnMenuItem();
            this.menuViewZoomOut = new PdnMenuItem();
            this.menuViewZoomToWindow = new PdnMenuItem();
            this.menuViewZoomToSelection = new PdnMenuItem();
            this.menuViewActualSize = new PdnMenuItem();
            this.menuViewSeparator1 = new ToolStripSeparator();
            this.menuViewGrid = new PdnMenuItem();
            this.menuViewRulers = new PdnMenuItem();
            this.menuViewSeparator2 = new ToolStripSeparator();
            this.menuViewPixels = new PdnMenuItem();
            this.menuViewInches = new PdnMenuItem();
            this.menuViewCentimeters = new PdnMenuItem();
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.menuViewZoomIn, this.menuViewZoomOut, this.menuViewZoomToWindow, this.menuViewZoomToSelection, this.menuViewActualSize, this.menuViewSeparator1, this.menuViewGrid, this.menuViewRulers, this.menuViewSeparator2, this.menuViewPixels, this.menuViewInches, this.menuViewCentimeters };
            base.DropDownItems.AddRange(toolStripItems);
            base.Name = "Menu.View";
            this.Text = PdnResources.GetString("Menu.View.Text");
            this.menuViewZoomIn.Name = "ZoomIn";
            this.menuViewZoomIn.ShortcutKeys = Keys.Control | Keys.Add;
            this.menuViewZoomIn.ShortcutKeyDisplayString = PdnResources.GetString("Menu.View.ZoomIn.ShortcutKeyDisplayString");
            this.menuViewZoomIn.Click += new EventHandler(this.OnMenuViewZoomInClick);
            this.menuViewZoomOut.Name = "ZoomOut";
            this.menuViewZoomOut.ShortcutKeys = Keys.Control | Keys.Subtract;
            this.menuViewZoomOut.ShortcutKeyDisplayString = PdnResources.GetString("Menu.View.ZoomOut.ShortcutKeyDisplayString");
            this.menuViewZoomOut.Click += new EventHandler(this.OnMenuViewZoomOutClick);
            this.menuViewZoomToWindow.Name = "ZoomToWindow";
            this.menuViewZoomToWindow.ShortcutKeys = Keys.Control | Keys.B;
            this.menuViewZoomToWindow.Click += new EventHandler(this.OnMenuViewZoomToWindowClick);
            this.menuViewZoomToSelection.Name = "ZoomToSelection";
            this.menuViewZoomToSelection.ShortcutKeys = Keys.Control | Keys.Shift | Keys.B;
            this.menuViewZoomToSelection.Click += new EventHandler(this.OnMenuViewZoomToSelectionClick);
            this.menuViewActualSize.Name = "ActualSize";
            this.menuViewActualSize.ShortcutKeys = Keys.Control | Keys.D0;
            this.menuViewActualSize.Click += new EventHandler(this.OnMenuViewActualSizeClick);
            this.menuViewGrid.Name = "Grid";
            this.menuViewGrid.Click += new EventHandler(this.OnMenuViewGridClick);
            this.menuViewRulers.Name = "Rulers";
            this.menuViewRulers.Click += new EventHandler(this.OnMenuViewRulersClick);
            this.menuViewPixels.Name = "Pixels";
            this.menuViewPixels.Click += new EventHandler(this.OnMenuViewPixelsClick);
            this.menuViewPixels.Text = PdnResources.GetString("MeasurementUnit.Pixel.Plural");
            this.menuViewInches.Name = "Inches";
            this.menuViewInches.Text = PdnResources.GetString("MeasurementUnit.Inch.Plural");
            this.menuViewInches.Click += new EventHandler(this.OnMenuViewInchesClick);
            this.menuViewCentimeters.Name = "Centimeters";
            this.menuViewCentimeters.Click += new EventHandler(this.OnMenuViewCentimetersClick);
            this.menuViewCentimeters.Text = PdnResources.GetString("MeasurementUnit.Centimeter.Plural");
        }

        private bool OnCtrlNumPad0(Keys keys)
        {
            this.menuViewActualSize.PerformClick();
            return true;
        }

        private bool OnCtrlOemMinusShortcut(Keys keys)
        {
            this.menuViewZoomOut.PerformClick();
            return true;
        }

        private bool OnCtrlOemPlusShortcut(Keys keys)
        {
            this.menuViewZoomIn.PerformClick();
            return true;
        }

        private bool OnCtrlShiftA(Keys keys)
        {
            this.menuViewActualSize.PerformClick();
            return true;
        }

        private bool OnCtrlZero(Keys keys)
        {
            this.menuViewActualSize.PerformClick();
            return true;
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            this.menuViewPixels.Checked = false;
            this.menuViewInches.Checked = false;
            this.menuViewCentimeters.Checked = false;
            switch (base.AppWorkspace.Units)
            {
                case MeasurementUnit.Pixel:
                    this.menuViewPixels.Checked = true;
                    break;

                case MeasurementUnit.Inch:
                    this.menuViewInches.Checked = true;
                    break;

                case MeasurementUnit.Centimeter:
                    this.menuViewCentimeters.Checked = true;
                    break;

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException<MeasurementUnit>(base.AppWorkspace.Units, "this.AppWorkspace.Units");
            }
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.menuViewZoomIn.Enabled = true;
                this.menuViewZoomOut.Enabled = true;
                this.menuViewZoomToWindow.Enabled = true;
                this.menuViewZoomToSelection.Enabled = !base.AppWorkspace.ActiveDocumentWorkspace.Selection.IsEmpty;
                this.menuViewActualSize.Enabled = true;
                this.menuViewGrid.Enabled = true;
                this.menuViewRulers.Enabled = true;
                this.menuViewPixels.Enabled = true;
                this.menuViewInches.Enabled = true;
                this.menuViewCentimeters.Enabled = true;
                this.menuViewZoomToWindow.Checked = base.AppWorkspace.ActiveDocumentWorkspace.ZoomBasis == ZoomBasis.FitToWindow;
                this.menuViewGrid.Checked = base.AppWorkspace.ActiveDocumentWorkspace.DrawGrid;
                this.menuViewRulers.Checked = base.AppWorkspace.ActiveDocumentWorkspace.RulersEnabled;
            }
            else
            {
                this.menuViewZoomIn.Enabled = false;
                this.menuViewZoomOut.Enabled = false;
                this.menuViewZoomToWindow.Enabled = false;
                this.menuViewZoomToSelection.Enabled = false;
                this.menuViewActualSize.Enabled = false;
                this.menuViewGrid.Enabled = false;
                this.menuViewRulers.Enabled = false;
                this.menuViewPixels.Enabled = false;
                this.menuViewInches.Enabled = false;
                this.menuViewCentimeters.Enabled = false;
            }
            base.OnDropDownOpening(e);
        }

        private void OnMenuViewActualSizeClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.CanvasView.ScaleRatio = base.AppWorkspace.ActiveDocumentWorkspace.CanvasView.ScaleRatio;
                base.AppWorkspace.ActiveDocumentWorkspace.ZoomBasis = ZoomBasis.ScaleFactor;
                base.AppWorkspace.ActiveDocumentWorkspace.ZoomTo(ScaleFactor.OneToOne);
            }
        }

        private void OnMenuViewCentimetersClick(object sender, EventArgs e)
        {
            base.AppWorkspace.Units = MeasurementUnit.Centimeter;
        }

        private void OnMenuViewGridClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.DrawGrid = !base.AppWorkspace.ActiveDocumentWorkspace.DrawGrid;
            }
        }

        private void OnMenuViewInchesClick(object sender, EventArgs e)
        {
            base.AppWorkspace.Units = MeasurementUnit.Inch;
        }

        private void OnMenuViewPixelsClick(object sender, EventArgs e)
        {
            base.AppWorkspace.Units = MeasurementUnit.Pixel;
        }

        private void OnMenuViewRulersClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.RulersEnabled = !base.AppWorkspace.ActiveDocumentWorkspace.RulersEnabled;
            }
        }

        private void OnMenuViewZoomInClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomInAction());
            }
        }

        private void OnMenuViewZoomOutClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomOutAction());
            }
        }

        private void OnMenuViewZoomToSelectionClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomToSelectionAction());
            }
        }

        private void OnMenuViewZoomToWindowClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new ZoomToWindowAction());
            }
        }
    }
}

