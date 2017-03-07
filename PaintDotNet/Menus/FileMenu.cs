namespace PaintDotNet.Menus
{
    using PaintDotNet;
    using PaintDotNet.Actions;
    using PaintDotNet.AppModel;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    internal sealed class FileMenu : PdnMenuItem
    {
        private PdnMenuItem menuFileAcquire;
        private PdnMenuItem menuFileAcquireFromScannerOrCamera;
        private PdnMenuItem menuFileClose;
        private PdnMenuItem menuFileExit;
        private PdnMenuItem menuFileNew;
        private PdnMenuItem menuFileOpen;
        private PdnMenuItem menuFileOpenRecent;
        private PdnMenuItem menuFileOpenRecentSentinel;
        private PdnMenuItem menuFilePrint;
        private PdnMenuItem menuFileSave;
        private PdnMenuItem menuFileSaveAs;
        private ToolStripSeparator menuFileSeparator1;
        private ToolStripSeparator menuFileSeparator2;
        private ToolStripSeparator menuFileSeparator3;

        public FileMenu()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.F4, new Func<Keys, bool>(this.OnCtrlF4Typed));
            this.InitializeComponent();
        }

        private void DoExit()
        {
            Startup.CloseApplication();
        }

        private ToolStripItem[] GetMenuItemsToAdd() => 
            new ToolStripItem[] { this.menuFileNew, this.menuFileOpen, this.menuFileOpenRecent, this.menuFileAcquire, this.menuFileClose, this.menuFileSeparator1, this.menuFileSave, this.menuFileSaveAs, this.menuFileSeparator2, this.menuFilePrint, this.menuFileSeparator3, this.menuFileExit };

        private void InitializeComponent()
        {
            this.menuFileNew = new PdnMenuItem();
            this.menuFileOpen = new PdnMenuItem();
            this.menuFileOpenRecent = new PdnMenuItem();
            this.menuFileOpenRecentSentinel = new PdnMenuItem();
            this.menuFileAcquire = new PdnMenuItem();
            this.menuFileAcquireFromScannerOrCamera = new PdnMenuItem();
            this.menuFileClose = new PdnMenuItem();
            this.menuFileSeparator1 = new ToolStripSeparator();
            this.menuFileSave = new PdnMenuItem();
            this.menuFileSaveAs = new PdnMenuItem();
            this.menuFileSeparator2 = new ToolStripSeparator();
            this.menuFilePrint = new PdnMenuItem();
            this.menuFileSeparator3 = new ToolStripSeparator();
            this.menuFileExit = new PdnMenuItem();
            base.DropDownItems.AddRange(this.GetMenuItemsToAdd());
            base.Name = "Menu.File";
            this.Text = PdnResources.GetString("Menu.File.Text");
            this.menuFileNew.Name = "New";
            this.menuFileNew.ShortcutKeys = Keys.Control | Keys.N;
            this.menuFileNew.Click += new EventHandler(this.OnMenuFileNewClick);
            this.menuFileOpen.Name = "Open";
            this.menuFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            this.menuFileOpen.Click += new EventHandler(this.OnMenuFileOpenClick);
            this.menuFileOpenRecent.Name = "OpenRecent";
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.menuFileOpenRecentSentinel };
            this.menuFileOpenRecent.DropDownItems.AddRange(toolStripItems);
            this.menuFileOpenRecent.DropDownOpening += new EventHandler(this.OnMenuFileOpenRecentDropDownOpening);
            this.menuFileOpenRecentSentinel.Text = "sentinel";
            this.menuFileAcquire.Name = "Acquire";
            ToolStripItem[] itemArray2 = new ToolStripItem[] { this.menuFileAcquireFromScannerOrCamera };
            this.menuFileAcquire.DropDownItems.AddRange(itemArray2);
            this.menuFileAcquire.DropDownOpening += new EventHandler(this.OnMenuFileAcquireDropDownOpening);
            this.menuFileAcquireFromScannerOrCamera.Name = "FromScannerOrCamera";
            this.menuFileAcquireFromScannerOrCamera.Click += new EventHandler(this.OnMenuFileAcquireFromScannerOrCameraClick);
            this.menuFileClose.Name = "Close";
            this.menuFileClose.Click += new EventHandler(this.OnMenuFileCloseClick);
            this.menuFileClose.ShortcutKeys = Keys.Control | Keys.W;
            this.menuFileSave.Name = "Save";
            this.menuFileSave.ShortcutKeys = Keys.Control | Keys.S;
            this.menuFileSave.Click += new EventHandler(this.OnMenuFileSaveClick);
            this.menuFileSaveAs.Name = "SaveAs";
            this.menuFileSaveAs.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            this.menuFileSaveAs.Click += new EventHandler(this.OnMenuFileSaveAsClick);
            this.menuFilePrint.Name = "Print";
            this.menuFilePrint.ShortcutKeys = Keys.Control | Keys.P;
            this.menuFilePrint.Click += new EventHandler(this.OnMenuFilePrintClick);
            this.menuFileExit.Name = "Exit";
            this.menuFileExit.Click += new EventHandler(this.OnMenuFileExitClick);
        }

        private void OnClearListClick(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new ClearMruListAction());
        }

        private bool OnCtrlF4Typed(Keys keys)
        {
            this.menuFileClose.PerformClick();
            return true;
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            this.VerifyDropDownItems();
            base.OnDropDownClosed(e);
        }

        protected override void OnDropDownItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (!this.GetMenuItemsToAdd().Contains<ToolStripItem>(e.ClickedItem))
            {
                throw new InvalidOperationException("Invalid menu item detected");
            }
            base.OnDropDownItemClicked(e);
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            this.VerifyDropDownItems();
            base.OnDropDownOpened(e);
            this.VerifyDropDownItems();
        }

        protected override void OnDropDownOpening(EventArgs e)
        {
            base.DropDownItems.Clear();
            ToolStripItem[] menuItemsToAdd = this.GetMenuItemsToAdd();
            base.DropDownItems.AddRange(menuItemsToAdd);
            this.menuFileNew.Enabled = true;
            this.menuFileOpen.Enabled = true;
            this.menuFileOpenRecent.Enabled = true;
            this.menuFileOpenRecentSentinel.Enabled = true;
            this.menuFileAcquire.Enabled = true;
            this.menuFileAcquireFromScannerOrCamera.Enabled = true;
            this.menuFileExit.Enabled = true;
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                this.menuFileSave.Enabled = true;
                this.menuFileSaveAs.Enabled = true;
                this.menuFileClose.Enabled = true;
                this.menuFilePrint.Enabled = true;
            }
            else
            {
                this.menuFileSave.Enabled = false;
                this.menuFileSaveAs.Enabled = false;
                this.menuFileClose.Enabled = false;
                this.menuFilePrint.Enabled = false;
            }
            base.OnDropDownOpening(e);
        }

        private void OnMenuFileAcquireDropDownOpening(object sender, EventArgs e)
        {
            bool flag = true;
            if (ScanningAndPrinting.IsComponentAvailable && !ScanningAndPrinting.CanScan)
            {
                flag = false;
            }
            this.menuFileAcquireFromScannerOrCamera.Enabled = flag;
        }

        private void OnMenuFileAcquireFromScannerOrCameraClick(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new AcquireFromScannerOrCameraAction());
        }

        private void OnMenuFileCloseClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.DocumentWorkspaces.Length != 0)
            {
                base.AppWorkspace.PerformAction(new CloseWorkspaceAction());
            }
        }

        private void OnMenuFileExitClick(object sender, EventArgs e)
        {
            this.DoExit();
        }

        private void OnMenuFileNewClick(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new NewImageAction());
        }

        private void OnMenuFileOpenClick(object sender, EventArgs e)
        {
            base.AppWorkspace.PerformAction(new OpenFileAction());
        }

        private void OnMenuFileOpenRecentDropDownOpening(object sender, EventArgs e)
        {
            int num;
            MostRecentFilesService.Instance.LoadMruList();
            MostRecentFile[] fileList = MostRecentFilesService.Instance.GetFileList();
            MostRecentFile[] fileArray2 = new MostRecentFile[fileList.Length];
            for (num = 0; num < fileList.Length; num++)
            {
                fileArray2[(fileArray2.Length - num) - 1] = fileList[num];
            }
            foreach (ToolStripItem item in this.menuFileOpenRecent.DropDownItems)
            {
                item.Click -= new EventHandler(this.OnMenuFileOpenRecentFileClick);
            }
            this.menuFileOpenRecent.DropDownItems.Clear();
            num = 0;
            foreach (MostRecentFile file in fileArray2)
            {
                string str;
                if (num < 9)
                {
                    str = "&";
                }
                else
                {
                    str = "";
                }
                int num3 = 1 + num;
                ToolStripMenuItem item2 = new ToolStripMenuItem(str + num3.ToString() + " " + Path.GetFileName(file.Path));
                item2.Click += new EventHandler(this.OnMenuFileOpenRecentFileClick);
                item2.ImageScaling = ToolStripItemImageScaling.None;
                item2.Image = (Image) file.Thumb.Clone();
                this.menuFileOpenRecent.DropDownItems.Add(item2);
                num++;
            }
            if (this.menuFileOpenRecent.DropDownItems.Count == 0)
            {
                ToolStripMenuItem item3 = new ToolStripMenuItem(PdnResources.GetString("Menu.File.OpenRecent.None")) {
                    Enabled = false
                };
                this.menuFileOpenRecent.DropDownItems.Add(item3);
            }
            else
            {
                ToolStripSeparator separator = new ToolStripSeparator();
                this.menuFileOpenRecent.DropDownItems.Add(separator);
                ToolStripMenuItem item4 = new ToolStripMenuItem {
                    Text = PdnResources.GetString("Menu.File.OpenRecent.ClearThisList")
                };
                this.menuFileOpenRecent.DropDownItems.Add(item4);
                Image reference = PdnResources.GetImageResource("Icons.MenuEditEraseSelectionIcon.png").Reference;
                item4.ImageAlign = ContentAlignment.MiddleCenter;
                item4.ImageScaling = ToolStripItemImageScaling.None;
                int iconSize = MostRecentFilesService.Instance.IconSize;
                Bitmap image = new Bitmap(iconSize + 2, iconSize + 2, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.Clear(Color.Transparent);
                    Point point = new Point((image.Width - reference.Width) / 2, (image.Height - reference.Height) / 2);
                    graphics.DrawImage(reference, point.X, point.Y, reference.Width, reference.Height);
                }
                item4.Image = image;
                item4.Click += new EventHandler(this.OnClearListClick);
            }
        }

        private void OnMenuFileOpenRecentFileClick(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = (ToolStripMenuItem) sender;
                int index = item.Text.IndexOf(" ");
                int num2 = int.Parse(item.Text.Substring(1, index - 1)) - 1;
                MostRecentFile[] fileList = MostRecentFilesService.Instance.GetFileList();
                string path = fileList[(fileList.Length - num2) - 1].Path;
                base.AppWorkspace.OpenFileInNewWorkspace(path);
            }
            catch (Exception)
            {
            }
        }

        private void OnMenuFilePrintClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.PerformAction(new PrintAction());
            }
        }

        private void OnMenuFileSaveAsClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.DoSaveAs();
            }
        }

        private void OnMenuFileSaveClick(object sender, EventArgs e)
        {
            if (base.AppWorkspace.ActiveDocumentWorkspace != null)
            {
                base.AppWorkspace.ActiveDocumentWorkspace.DoSave();
            }
        }

        private void VerifyDropDownItems()
        {
            ToolStripItem[] menuItemsToAdd = this.GetMenuItemsToAdd();
            ToolStripItem[] second = base.DropDownItems.Cast<ToolStripItem>().ToArray<ToolStripItem>();
            if (!menuItemsToAdd.SequenceEqual<ToolStripItem>(second))
            {
                throw new InvalidOperationException("Invalid menu items detected");
            }
        }
    }
}

