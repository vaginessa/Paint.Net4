namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.ComponentModel;
    using PaintDotNet.Controls;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Resources;
    using PaintDotNet.Settings.App;
    using PaintDotNet.Settings.UI;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Updates;
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal sealed class PdnAuxMenu : MenuStripEx
    {
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private ToolStripButton colorsWindowToggleButton;
        private HelpMenu helpMenu;
        private ToolStripButton historyWindowToggleButton;
        private ToolStripButton layersWindowToggleButton;
        private ToolStripButton showSettingsButton;
        private ToolStripButton toolsWindowToggleButton;

        public PdnAuxMenu()
        {
            this.InitializeComponent();
            PdnBaseForm.RegisterFormHotKey(Keys.F5, new Func<Keys, bool>(this.OnToolsHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.F5, new Func<Keys, bool>(this.OnToolsResetHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.F6, new Func<Keys, bool>(this.OnHistoryHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.F6, new Func<Keys, bool>(this.OnHistoryResetHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.F7, new Func<Keys, bool>(this.OnLayersHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.F7, new Func<Keys, bool>(this.OnLayersResetHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.F8, new Func<Keys, bool>(this.OnColorsHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.F8, new Func<Keys, bool>(this.OnColorsResetHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Alt | Keys.X, new Func<Keys, bool>(this.OnSettingsHotKeyPressed));
        }

        private void InitializeComponent()
        {
            Size size = new Size(UIUtil.ScaleWidth(0x10) + 6, UIUtil.ScaleHeight(0x10) + 6);
            this.toolsWindowToggleButton = new ToolStripButton();
            this.toolsWindowToggleButton.Image = PdnResources.GetImageResource("Icons.Settings.Tools.16.png").Reference;
            this.toolsWindowToggleButton.Checked = true;
            this.toolsWindowToggleButton.Click += new EventHandler(this.OnToggleButtonClick);
            this.toolsWindowToggleButton.Text = PdnResources.GetString("Menu.Window.Tools.Text");
            this.toolsWindowToggleButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolsWindowToggleButton.AutoSize = false;
            this.toolsWindowToggleButton.Size = size;
            this.toolsWindowToggleButton.Margin = new Padding(0, 0, 1, 0);
            this.historyWindowToggleButton = new ToolStripButton();
            this.historyWindowToggleButton.Image = PdnResources.GetImageResource("Icons.MenuWindowHistoryIcon.16.png").Reference;
            this.historyWindowToggleButton.Checked = true;
            this.historyWindowToggleButton.Click += new EventHandler(this.OnToggleButtonClick);
            this.historyWindowToggleButton.Text = PdnResources.GetString("Menu.Window.History.Text");
            this.historyWindowToggleButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.historyWindowToggleButton.AutoSize = false;
            this.historyWindowToggleButton.Size = size;
            this.historyWindowToggleButton.Margin = new Padding(0, 0, 1, 0);
            this.layersWindowToggleButton = new ToolStripButton();
            this.layersWindowToggleButton.Image = PdnResources.GetImageResource("Icons.MenuWindowLayersIcon.16.png").Reference;
            this.layersWindowToggleButton.Checked = true;
            this.layersWindowToggleButton.Click += new EventHandler(this.OnToggleButtonClick);
            this.layersWindowToggleButton.Text = PdnResources.GetString("Menu.Window.Layers.Text");
            this.layersWindowToggleButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.layersWindowToggleButton.AutoSize = false;
            this.layersWindowToggleButton.Size = size;
            this.layersWindowToggleButton.Margin = new Padding(0, 0, 1, 0);
            this.colorsWindowToggleButton = new ToolStripButton();
            this.colorsWindowToggleButton.Image = PdnResources.GetImageResource("Icons.MenuWindowColorsIcon.16.png").Reference;
            this.colorsWindowToggleButton.Checked = true;
            this.colorsWindowToggleButton.Click += new EventHandler(this.OnToggleButtonClick);
            this.colorsWindowToggleButton.Text = PdnResources.GetString("Menu.Window.Colors.Text");
            this.colorsWindowToggleButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.colorsWindowToggleButton.AutoSize = false;
            this.colorsWindowToggleButton.Size = size;
            this.showSettingsButton = new ToolStripButton();
            this.showSettingsButton.Image = PdnResources.GetImageResource("Icons.MenuUtilitiesSettingsIcon.16.png").Reference;
            this.showSettingsButton.Checked = false;
            this.showSettingsButton.Click += new EventHandler(this.OnShowSettingsClick);
            this.showSettingsButton.Text = PdnResources.GetString("Menu.Settings.Text");
            this.showSettingsButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.showSettingsButton.AutoSize = false;
            this.showSettingsButton.Size = size;
            this.showSettingsButton.Margin = new Padding(1, 0, 0, 0);
            this.helpMenu = new HelpMenu();
            this.helpMenu.Margin = new Padding(1, 0, 0, 0);
            base.SuspendLayout();
            base.Name = "PdnAuxMenu";
            base.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            base.ShowItemToolTips = true;
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.toolsWindowToggleButton, this.historyWindowToggleButton, this.layersWindowToggleButton, this.colorsWindowToggleButton, new ToolStripSeparator(), this.showSettingsButton, this.helpMenu };
            this.Items.AddRange(toolStripItems);
            base.ResumeLayout();
        }

        private bool OnColorsHotKeyPressed(Keys keys)
        {
            this.OnToggleButtonClick(this.colorsWindowToggleButton, EventArgs.Empty);
            return true;
        }

        private bool OnColorsResetHotKeyPressed(Keys keys)
        {
            this.ResetFloatingForm(this.appWorkspace.Widgets.ColorsForm);
            return true;
        }

        private void OnFloatingToolFormVisibleChanged(object sender, EventArgs e)
        {
            FloatingToolForm form = (FloatingToolForm) sender;
            if (form == this.appWorkspace.Widgets.ToolsForm)
            {
                this.toolsWindowToggleButton.Checked = form.Visible;
            }
            else if (form == this.appWorkspace.Widgets.HistoryForm)
            {
                this.historyWindowToggleButton.Checked = form.Visible;
            }
            else if (form == this.appWorkspace.Widgets.LayersForm)
            {
                this.layersWindowToggleButton.Checked = form.Visible;
            }
            else if (form == this.appWorkspace.Widgets.ColorsForm)
            {
                this.colorsWindowToggleButton.Checked = form.Visible;
            }
        }

        private bool OnHistoryHotKeyPressed(Keys keys)
        {
            this.OnToggleButtonClick(this.historyWindowToggleButton, EventArgs.Empty);
            return true;
        }

        private bool OnHistoryResetHotKeyPressed(Keys keys)
        {
            this.ResetFloatingForm(this.appWorkspace.Widgets.HistoryForm);
            return true;
        }

        private void OnLanguageChangedFromSettingsDialog(CultureInfo oldLanguage)
        {
            CultureInfo parent = AppSettings.Instance.UI.Language.Value;
            Icon icon = PdnResources.GetImageResource("Icons.MenuUtilitiesLanguageIcon.png").Reference.ToIcon();
            string str = PdnResources.GetString("ConfirmLanguageDialog.Title");
            Image image = null;
            string str2 = PdnResources.GetString("ConfirmLanguageDialog.IntroText");
            Image reference = PdnResources.GetImageResource("Icons.RightArrowBlue.png").Reference;
            string format = PdnResources.GetString("ConfirmLanguageDialog.RestartTB.ExplanationText.Format");
            CultureInfo info2 = new CultureInfo("en-US");
            if (parent.Equals(info2))
            {
                parent = parent.Parent;
            }
            string nativeName = parent.NativeName;
            string explanationText = string.Format(format, nativeName);
            TaskButton button = new TaskButton(reference, PdnResources.GetString("ConfirmLanguageDialog.RestartTB.ActionText"), explanationText);
            TaskButton button2 = new TaskButton(PdnResources.GetImageResource("Icons.CancelIcon.png").Reference, PdnResources.GetString("ConfirmLanguageDialog.CancelTB.ActionText"), PdnResources.GetString("ConfirmLanguageDialog.CancelTB.ExplanationText"));
            int num = (TaskDialog.DefaultPixelWidth96Dpi * 5) / 4;
            TaskDialog dialog2 = new TaskDialog {
                Icon = icon,
                Title = str,
                TaskImage = image,
                ScaleTaskImageWithDpi = true,
                IntroText = str2
            };
            dialog2.TaskButtons = new TaskButton[] { button, button2 };
            dialog2.AcceptButton = button;
            dialog2.CancelButton = button2;
            dialog2.PixelWidth96Dpi = num;
            TaskDialog dialog = dialog2;
            if (dialog.Show(this.AppWorkspace) == button)
            {
                if (ShellUtil.IsActivityQueuedForRestart)
                {
                    MessageBoxUtil.ErrorBox(this.AppWorkspace, PdnResources.GetString("Effect.PluginErrorDialog.CantQueue2ndRestart"));
                }
                else
                {
                    CloseAllWorkspacesAction action = new CloseAllWorkspacesAction();
                    action.PerformAction(this.AppWorkspace);
                    if (!action.Cancelled)
                    {
                        ShellUtil.RestartApplication();
                        Startup.CloseApplication();
                    }
                }
            }
            else
            {
                AppSettings.Instance.UI.Language.Value = oldLanguage;
            }
        }

        private bool OnLayersHotKeyPressed(Keys keys)
        {
            this.OnToggleButtonClick(this.layersWindowToggleButton, EventArgs.Empty);
            return true;
        }

        private bool OnLayersResetHotKeyPressed(Keys keys)
        {
            this.ResetFloatingForm(this.appWorkspace.Widgets.LayersForm);
            return true;
        }

        private bool OnSettingsHotKeyPressed(Keys keys)
        {
            this.ShowSettings();
            return true;
        }

        private void OnShowSettingsClick(object sender, EventArgs e)
        {
            this.ShowSettings();
        }

        private void OnToggleButtonClick(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            FloatingToolForm tag = (FloatingToolForm) button.Tag;
            if ((Control.ModifierKeys & (Keys.Control | Keys.Shift)) == (Keys.Control | Keys.Shift))
            {
                this.ResetFloatingForm(tag);
            }
            else
            {
                tag.Visible = !tag.Visible;
            }
            if (this.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.AppWorkspace.ActiveDocumentWorkspace.Focus();
            }
        }

        private bool OnToolsHotKeyPressed(Keys keys)
        {
            this.OnToggleButtonClick(this.toolsWindowToggleButton, EventArgs.Empty);
            return true;
        }

        private bool OnToolsResetHotKeyPressed(Keys keys)
        {
            this.ResetFloatingForm(this.appWorkspace.Widgets.ToolsForm);
            return true;
        }

        private void ResetFloatingForm(FloatingToolForm form)
        {
            this.AppWorkspace.ResetFloatingForm(form);
            form.Visible = false;
            form.Visible = true;
        }

        private void ShowSettings()
        {
            bool shouldPerformUpdateCheckAfterClose;
        Label_0000:
            if (base.IsDisposed || this.AppWorkspace.IsDisposed)
            {
                return;
            }
            CultureInfo oldLanguage = AppSettings.Instance.UI.Language.Value;
            using (SettingsDialog dialog = new SettingsDialog(new ServiceProviderProxy(this.appWorkspace), AppSettings.Instance, this.AppWorkspace.ToolSettings))
            {
                dialog.ShowDialog(this.AppWorkspace);
                shouldPerformUpdateCheckAfterClose = dialog.ShouldPerformUpdateCheckAfterClose;
            }
            if (shouldPerformUpdateCheckAfterClose)
            {
                UpdatesService.Instance.PerformUpdateCheck();
                goto Label_0000;
            }
            if (!oldLanguage.Equals(AppSettings.Instance.UI.Language.Value))
            {
                this.OnLanguageChangedFromSettingsDialog(oldLanguage);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 260)
            {
                m.Result = new IntPtr(1);
            }
            base.WndProc(ref m);
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                if (this.appWorkspace != null)
                {
                    throw new InvalidOperationException();
                }
                this.appWorkspace = value;
                this.toolsWindowToggleButton.Tag = this.appWorkspace.Widgets.ToolsForm;
                this.toolsWindowToggleButton.Checked = this.appWorkspace.Widgets.ToolsForm.Visible;
                this.appWorkspace.Widgets.ToolsForm.VisibleChanged += new EventHandler(this.OnFloatingToolFormVisibleChanged);
                this.historyWindowToggleButton.Tag = this.appWorkspace.Widgets.HistoryForm;
                this.historyWindowToggleButton.Checked = this.appWorkspace.Widgets.HistoryForm.Visible;
                this.appWorkspace.Widgets.HistoryForm.VisibleChanged += new EventHandler(this.OnFloatingToolFormVisibleChanged);
                this.layersWindowToggleButton.Tag = this.appWorkspace.Widgets.LayersForm;
                this.layersWindowToggleButton.Checked = this.appWorkspace.Widgets.LayersForm.Visible;
                this.appWorkspace.Widgets.LayersForm.VisibleChanged += new EventHandler(this.OnFloatingToolFormVisibleChanged);
                this.colorsWindowToggleButton.Tag = this.appWorkspace.Widgets.ColorsForm;
                this.colorsWindowToggleButton.Checked = this.appWorkspace.Widgets.ColorsForm.Visible;
                this.appWorkspace.Widgets.ColorsForm.VisibleChanged += new EventHandler(this.OnFloatingToolFormVisibleChanged);
                this.helpMenu.AppWorkspace = value;
            }
        }
    }
}

