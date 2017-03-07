namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.AppModel;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class HelpMenu : PdnMenuItem
    {
        private PdnMenuItem menuHelpAbout;
        private PdnMenuItem menuHelpDonate;
        private PdnMenuItem menuHelpForum;
        private PdnMenuItem menuHelpHelpTopics;
        private PdnMenuItem menuHelpPdnSearch;
        private PdnMenuItem menuHelpPdnWebsite;
        private PdnMenuItem menuHelpPlugins;
        private PdnMenuItem menuHelpSendFeedback;
        private ToolStripSeparator menuHelpSeparator1;
        private ToolStripSeparator menuHelpSeparator2;
        private PdnMenuItem menuHelpTutorials;

        public HelpMenu()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.menuHelpHelpTopics = new PdnMenuItem();
            this.menuHelpSeparator1 = new ToolStripSeparator();
            this.menuHelpPdnWebsite = new PdnMenuItem();
            this.menuHelpPdnSearch = new PdnMenuItem();
            this.menuHelpDonate = new PdnMenuItem();
            this.menuHelpForum = new PdnMenuItem();
            this.menuHelpTutorials = new PdnMenuItem();
            this.menuHelpPlugins = new PdnMenuItem();
            this.menuHelpSendFeedback = new PdnMenuItem();
            this.menuHelpSeparator2 = new ToolStripSeparator();
            this.menuHelpAbout = new PdnMenuItem();
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.menuHelpHelpTopics, this.menuHelpSeparator1, this.menuHelpPdnWebsite, this.menuHelpPdnSearch };
            base.DropDownItems.AddRange(toolStripItems);
            if (!WinAppModel.HasCurrentPackage)
            {
                base.DropDownItems.Add(this.menuHelpDonate);
            }
            ToolStripItem[] itemArray2 = new ToolStripItem[] { this.menuHelpForum, this.menuHelpTutorials, this.menuHelpPlugins, this.menuHelpSendFeedback, this.menuHelpSeparator2, this.menuHelpAbout };
            base.DropDownItems.AddRange(itemArray2);
            base.Name = "Menu.Help";
            this.Text = PdnResources.GetString("Menu.Help.Text");
            this.Image = PdnResources.GetImageResource("Icons.MenuHelpIcon.png").Reference;
            this.DisplayStyle = ToolStripItemDisplayStyle.Image;
            base.AutoToolTip = true;
            base.AutoSize = false;
            this.Size = new Size(UIUtil.ScaleWidth(0x10) + 6, UIUtil.ScaleHeight(0x10) + 6);
            this.menuHelpHelpTopics.Name = "HelpTopics";
            this.menuHelpHelpTopics.ShortcutKeys = Keys.F1;
            this.menuHelpHelpTopics.Click += new EventHandler(this.OnMenuHelpHelpTopicsClick);
            this.menuHelpPdnWebsite.Name = "PdnWebsite";
            this.menuHelpPdnWebsite.Click += new EventHandler(this.OnMenuHelpPdnWebsiteClick);
            this.menuHelpPdnSearch.Name = "PdnSearch";
            this.menuHelpPdnSearch.Click += new EventHandler(this.OnMenuHelpPdnSearchEngineClick);
            this.menuHelpPdnSearch.ShortcutKeys = Keys.Control | Keys.E;
            this.menuHelpDonate.Name = "Donate";
            this.menuHelpDonate.Click += new EventHandler(this.OnMenuHelpDonateClick);
            this.menuHelpDonate.Font = FontUtil.CreateGdipFont(this.menuHelpDonate.Font.Name, this.menuHelpDonate.Font.Size, this.menuHelpDonate.Font.Style | FontStyle.Italic);
            this.menuHelpForum.Name = "Forum";
            this.menuHelpForum.Click += new EventHandler(this.OnMenuHelpForumClick);
            this.menuHelpTutorials.Name = "Tutorials";
            this.menuHelpTutorials.Click += new EventHandler(this.OnMenuHelpTutorialsClick);
            this.menuHelpPlugins.Name = "Plugins";
            this.menuHelpPlugins.Click += new EventHandler(this.OnMenuHelpPluginsClick);
            this.menuHelpSendFeedback.Name = "SendFeedback";
            this.menuHelpSendFeedback.Click += new EventHandler(this.OnMenuHelpSendFeedbackClick);
            this.menuHelpAbout.Name = "About";
            this.menuHelpAbout.Click += new EventHandler(this.OnMenuHelpAboutClick);
        }

        private void OnMenuHelpAboutClick(object sender, EventArgs e)
        {
            AboutDialog dialog;
            using (new WaitCursorChanger(base.AppWorkspace))
            {
                dialog = new AboutDialog();
            }
            dialog.ShowDialog(base.AppWorkspace);
            DisposableUtil.Free<AboutDialog>(ref dialog);
        }

        private void OnMenuHelpDonateClick(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite(base.AppWorkspace, "/redirect/donate_hm.html");
        }

        private void OnMenuHelpForumClick(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite(base.AppWorkspace, "/redirect/forum_hm.html");
        }

        private void OnMenuHelpHelpTopicsClick(object sender, EventArgs e)
        {
            HelpService.Instance.ShowHelp(base.AppWorkspace);
        }

        private void OnMenuHelpPdnSearchEngineClick(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite(base.AppWorkspace, "/redirect/search_hm.html");
        }

        private void OnMenuHelpPdnWebsiteClick(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite(base.AppWorkspace, "/redirect/main_hm.html");
        }

        private void OnMenuHelpPluginsClick(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite(base.AppWorkspace, "/redirect/plugins_hm.html");
        }

        private void OnMenuHelpSendFeedbackClick(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new SendFeedbackAction());
        }

        private void OnMenuHelpTutorialsClick(object sender, EventArgs e)
        {
            PdnInfo.LaunchWebSite(base.AppWorkspace, "/redirect/tutorials_hm.html");
        }
    }
}

