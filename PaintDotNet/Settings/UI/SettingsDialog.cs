namespace PaintDotNet.Settings.UI
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Direct2D;
    using PaintDotNet.DirectWrite;
    using PaintDotNet.Drawing;
    using PaintDotNet.Imaging;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.UI.Media;
    using PaintDotNet.VisualStyling;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal sealed class SettingsDialog : PdnBaseFormInternal
    {
        private AppSettings appSettings;
        private PdnPushButton closeButton;
        private Container components;
        private SettingsDialogPage currentPage;
        private SettingsDialogSection currentSection;
        private int hotTrackIndex = -1;
        private static System.Type lastCurrentSection;
        private PanelEx sectionPanel;
        private ListBox sectionsListBox;
        private SelectionHighlightRenderer selectionHighlightRenderer = new SelectionHighlightRenderer();
        private PaintDotNet.Controls.SeparatorLine separator;
        private IServiceProvider services;
        private SettingsDialogPage[] settingsPages;
        private SettingsDialogSection[] settingsSections;
        private AppSettings.ToolsSection toolBarSettings;

        public SettingsDialog(IServiceProvider services, AppSettings appSettings, AppSettings.ToolsSection toolBarSettings)
        {
            Validate.Begin().IsNotNull<IServiceProvider>(services, "services").IsNotNull<AppSettings>(appSettings, "appSettings").IsNotNull<AppSettings.ToolsSection>(toolBarSettings, "toolBarSettings").Check();
            this.services = services;
            this.appSettings = appSettings;
            this.toolBarSettings = toolBarSettings;
            this.components = new Container();
            List<SettingsDialogSection> items = new List<SettingsDialogSection> {
                new UISettingsSection(this.appSettings),
                new ToolsSettingsSection(this.appSettings, toolBarSettings)
            };
            if (!WinAppModel.HasCurrentPackage)
            {
                items.Add(new UpdatesSettingsSection(this, this.appSettings));
            }
            items.Add(new DiagnosticsSettingsSection(this.appSettings));
            IPluginErrorService pluginErrorService = services.GetService<IPluginErrorService>();
            if (pluginErrorService.GetPluginLoadErrors().Any<PluginErrorInfo>())
            {
                items.Add(new PluginsSettingsSection(this.appSettings, pluginErrorService));
            }
            this.settingsSections = items.ToArrayEx<SettingsDialogSection>();
            this.settingsPages = this.settingsSections.Select<SettingsDialogSection, SettingsDialogPage>(ss => ss.CreateUI()).ToArrayEx<SettingsDialogPage>();
            if (lastCurrentSection == null)
            {
                this.currentSection = this.settingsSections[0];
            }
            else
            {
                this.currentSection = this.settingsSections.FirstOrDefault<SettingsDialogSection>(ss => (ss.GetType() == lastCurrentSection)) ?? this.settingsSections[0];
            }
            base.SuspendLayout();
            this.DoubleBuffered = true;
            base.ResizeRedraw = true;
            base.AutoHandleGlassRelatedOptimizations = true;
            base.IsGlassDesired = !OS.IsWin10OrLater;
            this.Font = System.Drawing.SystemFonts.MenuFont;
            this.sectionsListBox = new FlickerFreeOwnerDrawListBox();
            this.sectionPanel = new PanelEx();
            this.separator = new PaintDotNet.Controls.SeparatorLine();
            this.closeButton = new PdnPushButton();
            this.closeButton.Name = "closeButton";
            this.closeButton.AutoSize = true;
            this.closeButton.Text = PdnResources.GetString("Form.CloseButton.Text");
            this.sectionPanel.Name = "sectionPanel";
            this.sectionPanel.AutoScroll = true;
            this.sectionPanel.HideHScroll = true;
            this.sectionPanel.HorizontalScroll.Enabled = false;
            this.sectionPanel.HorizontalScroll.Visible = false;
            this.sectionPanel.VerticalScroll.Enabled = true;
            this.sectionsListBox.Name = "sectionsListBox";
            this.sectionsListBox.Items.AddRange(this.settingsSections.ToArrayEx<SettingsDialogSection>());
            this.sectionsListBox.DrawMode = DrawMode.OwnerDrawFixed;
            this.sectionsListBox.ItemHeight = UIUtil.ScaleHeight(0x20);
            this.sectionsListBox.DrawItem += new DrawItemEventHandler(this.OnSectionsListBoxDrawItem);
            this.sectionsListBox.IntegralHeight = false;
            this.sectionsListBox.BorderStyle = BorderStyle.FixedSingle;
            this.sectionsListBox.MouseEnter += (s, e) => this.UpdateHotTrackIndex(new Point(-1, -1));
            this.sectionsListBox.MouseMove += (s, e) => this.UpdateHotTrackIndex(e.Location);
            this.sectionsListBox.MouseLeave += (s, e) => this.UpdateHotTrackIndex(new Point(-1, -1));
            this.sectionsListBox.SelectedIndex = this.settingsSections.IndexOf<SettingsDialogSection>(this.currentSection);
            this.sectionsListBox.SelectedIndexChanged += new EventHandler(this.OnSectionsListBoxSelectedIndexChanged);
            this.separator.Name = "separator";
            base.Controls.Add(this.sectionsListBox);
            base.Controls.Add(this.sectionPanel);
            base.Controls.Add(this.closeButton);
            base.Controls.Add(this.separator);
            base.Icon = PdnResources.GetImageResource("Icons.MenuUtilitiesSettingsIcon.png").Reference.ToIcon();
            base.AcceptButton = this.closeButton;
            base.CancelButton = this.closeButton;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.FormBorderStyle = FormBorderStyle.Sizable;
            base.ShowInTaskbar = false;
            this.Text = PdnResources.GetString("SettingsDialog.Text");
            base.ClientSize = UIUtil.ScaleSize(600, 450);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                    this.components = null;
                }
                DisposableUtil.Free<SelectionHighlightRenderer>(ref this.selectionHighlightRenderer);
            }
            base.Dispose(disposing);
        }

        protected override void OnClosed(EventArgs e)
        {
            lastCurrentSection = this.currentSection.GetType();
            base.OnClosed(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int y = UIUtil.ScaleHeight(7);
            int x = UIUtil.ScaleWidth(7);
            int width = base.ClientSize.Width - (x * 2);
            int num4 = base.IsGlassEffectivelyEnabled ? 0 : y;
            int num5 = base.IsGlassEffectivelyEnabled ? -1 : x;
            Size size = UIUtil.ScaleSize(0x55, 0x18);
            this.closeButton.Size = size;
            this.closeButton.PerformLayout();
            this.closeButton.Location = new Point((base.ClientSize.Width - this.closeButton.Width) - num5, (base.ClientSize.Height - num4) - this.closeButton.Height);
            this.separator.Size = this.separator.GetPreferredSize(width, 1);
            this.separator.Location = new Point(x, this.closeButton.Top - y);
            this.sectionsListBox.Location = new Point(x, y);
            this.sectionsListBox.Width = UIUtil.ScaleWidth(150);
            this.sectionsListBox.Height = (this.separator.Top - this.sectionsListBox.Top) - y;
            this.sectionPanel.SuspendLayout();
            this.sectionPanel.Location = new Point(this.sectionsListBox.Right + x, y);
            this.sectionPanel.Width = (base.ClientSize.Width - this.sectionPanel.Left) - x;
            this.sectionPanel.Height = (this.separator.Top - this.sectionPanel.Top) - y;
            if (this.currentPage == null)
            {
                this.currentPage = this.settingsPages[0];
            }
            if (this.currentPage.Section != this.currentSection)
            {
                this.sectionPanel.Controls.Remove(this.currentPage);
                this.currentPage.Visible = false;
                this.currentPage = this.settingsPages.First<SettingsDialogPage>(sp => sp.Section == this.currentSection);
                this.currentPage.Visible = true;
            }
            if (this.currentPage.Parent == null)
            {
                this.sectionPanel.Controls.Add(this.currentPage);
            }
            this.currentPage.SuspendLayout();
            this.currentPage.PanelHeight = 0;
            this.currentPage.Location = this.sectionPanel.AutoScrollPosition;
            this.currentPage.Width = this.sectionPanel.ClientSize.Width;
            this.currentPage.ResumeLayout(false);
            this.currentPage.PerformLayout();
            this.currentPage.SuspendLayout();
            this.sectionPanel.ResumeLayout(true);
            this.currentPage.PanelHeight = this.sectionPanel.Height;
            this.currentPage.Width = this.sectionPanel.ClientSize.Width;
            this.currentPage.ResumeLayout(false);
            this.currentPage.PerformLayout();
            if (base.IsGlassEffectivelyEnabled)
            {
                this.separator.Visible = false;
                base.GlassInset = new Padding(0, 0, 0, base.ClientSize.Height - this.separator.Top);
                base.SizeGripStyle = SizeGripStyle.Hide;
            }
            else
            {
                this.separator.Visible = true;
                base.GlassInset = new Padding(0);
                base.SizeGripStyle = SizeGripStyle.Show;
            }
            this.MinimumSize = new Size(this.sectionsListBox.Right * 3, 200);
            base.OnLayout(levent);
        }

        private void OnSectionsListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            SettingsDialogSection section = this.sectionsListBox.Items[e.Index] as SettingsDialogSection;
            int x = UIUtil.ScaleWidth(4);
            using (IDrawingContext context = DrawingContextUtil.FromGraphics(e.Graphics, e.Bounds, false, FactorySource.PerThread))
            {
                HighlightState disabled;
                context.Clear(new ColorRgba128Float?(this.BackColor));
                if (e.State.HasFlag(DrawItemState.Disabled) || e.State.HasFlag(DrawItemState.Grayed))
                {
                    disabled = HighlightState.Disabled;
                }
                else if (e.State.HasFlag(DrawItemState.Selected))
                {
                    disabled = HighlightState.Checked;
                }
                else if (e.State.HasFlag(DrawItemState.HotLight) || (e.Index == this.hotTrackIndex))
                {
                    disabled = HighlightState.Hover;
                }
                else
                {
                    disabled = HighlightState.Default;
                }
                RectInt32 bounds = e.Bounds.ToRectInt32();
                bounds.Inflate(-1, -1);
                this.selectionHighlightRenderer.HighlightState = disabled;
                this.selectionHighlightRenderer.RenderBackground(context, bounds);
                SizeInt32 num4 = UIUtil.ScaleSize(section.DeviceIcon.PixelSize);
                RectInt32 num5 = new RectInt32(x, e.Bounds.Top + ((e.Bounds.Height - num4.Height) / 2), num4.Width, num4.Height);
                context.DrawBitmap(section.DeviceIcon, new RectDouble?(num5), 1.0, BitmapInterpolationMode.Linear, null);
                HotkeyRenderMode hotkeyRenderMode = !e.State.HasFlag(DrawItemState.NoAccelerator) ? HotkeyRenderMode.Show : HotkeyRenderMode.Hide;
                TextLayout resourceSource = UIText.CreateLayout(context, section.DisplayName, e.Font, null, hotkeyRenderMode, 65535.0, 65535.0);
                ITextLayout cachedOrCreateResource = context.GetCachedOrCreateResource<ITextLayout>(resourceSource);
                int num6 = num5.Right + x;
                float num7 = e.Bounds.Top + ((e.Bounds.Height - cachedOrCreateResource.Metrics.Height) / 2f);
                context.DrawTextLayout((double) num6, (double) num7, resourceSource, this.selectionHighlightRenderer.EmbeddedTextBrush, DrawTextOptions.None);
                if (!e.State.HasFlag(DrawItemState.NoFocusRect))
                {
                    context.DrawFocusRectangle(e.Bounds.ToRectFloat());
                }
            }
        }

        private void OnSectionsListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.CurrentSection = this.settingsSections[this.sectionsListBox.SelectedIndex];
        }

        protected override void OnShown(EventArgs e)
        {
            base.PerformLayout();
            base.Invalidate(true);
            base.OnShown(e);
        }

        internal void PerformUpdateCheck()
        {
            this.ShouldPerformUpdateCheckAfterClose = true;
            base.Close();
        }

        private void UpdateHotTrackIndex(Point mousePt)
        {
            int num = this.sectionsListBox.IndexFromPoint(mousePt);
            if (num != this.hotTrackIndex)
            {
                this.hotTrackIndex = num;
                this.sectionsListBox.Refresh();
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                UIUtil.AddCompositedExStyleToCreateParams(createParams);
                return createParams;
            }
        }

        public SettingsDialogSection CurrentSection
        {
            get => 
                this.currentSection;
            set
            {
                this.VerifyThreadAccess();
                if ((value != this.currentSection) && this.settingsSections.Contains<SettingsDialogSection>(value))
                {
                    this.currentSection = value;
                    base.PerformLayout();
                }
            }
        }

        public bool ShouldPerformUpdateCheckAfterClose { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SettingsDialog.<>c <>9 = new SettingsDialog.<>c();
            public static Func<SettingsDialogSection, SettingsDialogPage> <>9__22_0;
            public static Func<SettingsDialogSection, bool> <>9__22_1;

            internal SettingsDialogPage <.ctor>b__22_0(SettingsDialogSection ss) => 
                ss.CreateUI();

            internal bool <.ctor>b__22_1(SettingsDialogSection ss) => 
                (ss.GetType() == SettingsDialog.lastCurrentSection);
        }

        private sealed class FlickerFreeOwnerDrawListBox : ListBox
        {
            public FlickerFreeOwnerDrawListBox()
            {
                this.DoubleBuffered = true;
                base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                base.SetStyle(ControlStyles.ResizeRedraw, true);
                base.SetStyle(ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                }
                if (base.Items.Count > 0)
                {
                    for (int i = 0; i < base.Items.Count; i++)
                    {
                        Rectangle itemRectangle = base.GetItemRectangle(i);
                        if (e.ClipRectangle.IntersectsWith(itemRectangle))
                        {
                            if ((((this.SelectionMode == SelectionMode.One) && (this.SelectedIndex == i)) || ((this.SelectionMode == SelectionMode.MultiSimple) && base.SelectedIndices.Contains(i))) || ((this.SelectionMode == SelectionMode.MultiExtended) && base.SelectedIndices.Contains(i)))
                            {
                                this.OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font, itemRectangle, i, DrawItemState.NoFocusRect | DrawItemState.NoAccelerator | DrawItemState.Selected, this.ForeColor, this.BackColor));
                            }
                            else
                            {
                                this.OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font, itemRectangle, i, DrawItemState.NoFocusRect | DrawItemState.NoAccelerator | DrawItemState.Default, this.ForeColor, this.BackColor));
                            }
                        }
                    }
                }
                base.OnPaint(e);
            }
        }
    }
}

