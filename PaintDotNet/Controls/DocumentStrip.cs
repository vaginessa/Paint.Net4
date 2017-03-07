namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Animation;
    using PaintDotNet.Collections;
    using PaintDotNet.Direct2D;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Threading;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class DocumentStrip : ImageStrip
    {
        private int? clearTopInset;
        private List<ImageStrip.Item> documentButtons = new List<ImageStrip.Item>();
        private List<DocumentWorkspace> documents = new List<DocumentWorkspace>();
        private Dictionary<DocumentWorkspace, ImageStrip.Item> dw2button = new Dictionary<DocumentWorkspace, ImageStrip.Item>();
        private bool ensureSelectedIsVisible = true;
        private const double imageFadeInDuration = 0.5;
        private HashSet<IDisposable> keepAliveTickets = new HashSet<IDisposable>();
        private DocumentWorkspace selectedDocument;
        private int suspendThumbnailUpdates;
        private ThumbnailManager thumbnailManager;
        private Dictionary<DocumentWorkspace, RenderArgs> thumbs = new Dictionary<DocumentWorkspace, RenderArgs>();
        private object thumbsLock = new object();

        [field: CompilerGenerated]
        public event ValueEventHandler<Tuple<DocumentWorkspace, DocumentClickAction>> DocumentClicked;

        [field: CompilerGenerated]
        public event EventHandler DocumentListChanged;

        public DocumentStrip()
        {
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Tab, new Func<Keys, bool>(this.OnNextTabHotKeyPressed));
            PdnBaseForm.RegisterFormHotKey(Keys.Control | Keys.Shift | Keys.Tab, new Func<Keys, bool>(this.OnPreviousTabHotKeyPressed));
            using (ISynchronizationContext context = SynchronizationContextDispatcher.CreateRef())
            {
                this.thumbnailManager = new ThumbnailManager(context);
            }
            base.Name = "DocumentStrip";
            for (int i = 1; i <= 9; i++)
            {
                Keys keys = KeysUtil.FromLetterOrDigitChar((char) (i + 0x30));
                PdnBaseForm.RegisterFormHotKey(Keys.Control | keys, new Func<Keys, bool>(this.OnDigitHotKeyPressed));
                PdnBaseForm.RegisterFormHotKey(Keys.Alt | keys, new Func<Keys, bool>(this.OnDigitHotKeyPressed));
            }
            base.ShowCloseButtons = true;
        }

        public void AddDocumentWorkspace(DocumentWorkspace addMe)
        {
            this.VerifyThreadAccess();
            this.documents.Add(addMe);
            ImageStrip.Item newItem = new ImageStrip.Item(null) {
                Image = null,
                ImageOpacity = { Value = 0.0 },
                Tag = addMe
            };
            base.AddItem(newItem);
            this.documentButtons.Add(newItem);
            addMe.CompositionUpdated += new EventHandler(this.OnWorkspaceCompositionUpdated);
            this.dw2button.Add(addMe, newItem);
            if (addMe.Document != null)
            {
                this.QueueThumbnailUpdate(addMe);
                newItem.IsDirty = addMe.Document.Dirty;
                addMe.Document.DirtyChanged += new EventHandler(this.OnDocumentDirtyChanged);
            }
            addMe.DocumentChanging += new ValueEventHandler<Document>(this.OnWorkspaceDocumentChanging);
            addMe.DocumentChanged += new EventHandler(this.OnWorkspaceDocumentChanged);
            this.OnDocumentListChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (this.documents.Count > 0)
                {
                    this.RemoveDocumentWorkspace(this.documents[this.documents.Count - 1]);
                }
                if (this.thumbnailManager != null)
                {
                    this.thumbnailManager.Dispose();
                    this.thumbnailManager = null;
                }
                foreach (DocumentWorkspace workspace in this.thumbs.Keys)
                {
                    RenderArgs args = this.thumbs[workspace];
                    args.ISurface.Dispose();
                    args.Dispose();
                }
                this.thumbs.Clear();
                this.thumbs = null;
            }
            base.Dispose(disposing);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int width;
            this.VerifyThreadAccess();
            Size size = base.ItemSize.ToGdipSize();
            if (base.ItemCount == 0)
            {
                width = 0;
            }
            else
            {
                width = base.ViewRectangle.Width;
            }
            return new Size(width, size.Height);
        }

        public void LockDocumentWorkspaceDirtyValue(DocumentWorkspace lockMe, bool forceDirtyValue)
        {
            this.VerifyThreadAccess();
            this.dw2button[lockMe].LockDirtyValue(forceDirtyValue);
        }

        public bool NextTab()
        {
            this.VerifyThreadAccess();
            bool flag = false;
            if (this.selectedDocument != null)
            {
                int num2 = (this.documents.IndexOf(this.selectedDocument) + 1) % this.documents.Count;
                this.SelectedDocument = this.documents[num2];
                flag = true;
            }
            return flag;
        }

        private bool OnDigitHotKeyPressed(Keys keys)
        {
            keys &= ~Keys.Alt;
            keys &= ~Keys.Control;
            if ((keys >= Keys.D0) && (keys <= Keys.D9))
            {
                int num2;
                int num = ((int) keys) - 0x30;
                if (num == 0)
                {
                    num2 = 9;
                }
                else
                {
                    num2 = num - 1;
                }
                if (num2 < this.documents.Count)
                {
                    base.PerformItemClick(num2, ImageStrip.ItemPart.Image, MouseButtons.Left);
                    return true;
                }
            }
            return false;
        }

        protected virtual void OnDocumentClicked(DocumentWorkspace dw, DocumentClickAction action)
        {
            this.DocumentClicked.Raise<Tuple<DocumentWorkspace, DocumentClickAction>>(this, Tuple.Create<DocumentWorkspace, DocumentClickAction>(dw, action));
        }

        private void OnDocumentDirtyChanged(object sender, EventArgs e)
        {
            if (!base.CheckAccess())
            {
                base.BeginInvoke(() => this.OnDocumentDirtyChanged(sender, e));
            }
            else
            {
                this.VerifyThreadAccess();
                for (int i = 0; i < this.documents.Count; i++)
                {
                    if (sender == this.documents[i].Document)
                    {
                        ImageStrip.Item item = this.dw2button[this.documents[i]];
                        item.IsDirty = ((Document) sender).Dirty;
                    }
                }
            }
        }

        protected virtual void OnDocumentListChanged()
        {
            this.DocumentListChanged.Raise(this);
        }

        protected override void OnItemClicked(ImageStrip.Item item, ImageStrip.ItemPart itemPart, MouseButtons mouseButtons)
        {
            DocumentWorkspace tag = item.Tag as DocumentWorkspace;
            if (tag != null)
            {
                if (mouseButtons == MouseButtons.Middle)
                {
                    this.OnDocumentClicked(tag, DocumentClickAction.Close);
                }
                else
                {
                    switch (itemPart)
                    {
                        case ImageStrip.ItemPart.None:
                            break;

                        case ImageStrip.ItemPart.Image:
                            if (mouseButtons != MouseButtons.Left)
                            {
                                if (mouseButtons == MouseButtons.Right)
                                {
                                }
                                break;
                            }
                            this.SelectedDocument = tag;
                            break;

                        case ImageStrip.ItemPart.CloseButton:
                            if (mouseButtons == MouseButtons.Left)
                            {
                                this.OnDocumentClicked(tag, DocumentClickAction.Close);
                            }
                            break;

                        default:
                            throw new InvalidEnumArgumentException();
                    }
                }
            }
            base.OnItemClicked(item, itemPart, mouseButtons);
        }

        protected override void OnItemMoved(ImageStripItemMovedEventArgs e)
        {
            DocumentWorkspace workspace = this.documents[e.OldIndex];
            ImageStrip.Item item = this.documentButtons[e.OldIndex];
            this.documents.RemoveAt(e.OldIndex);
            this.documents.Insert(e.NewIndex, workspace);
            this.documentButtons.RemoveAt(e.OldIndex);
            this.documentButtons.Insert(e.NewIndex, item);
            base.OnItemMoved(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            ImageStrip.Item item;
            if (((this.ensureSelectedIsVisible && !this.Focused) && (!base.LeftScrollButton.Focused && !base.RightScrollButton.Focused)) && ((this.selectedDocument != null) && this.dw2button.TryGetValue(this.selectedDocument, out item)))
            {
                base.EnsureItemFullyVisible(item);
            }
            base.OnLayout(levent);
        }

        private bool OnNextTabHotKeyPressed(Keys keys) => 
            ((Control.MouseButtons == MouseButtons.None) && this.NextTab());

        private bool OnPreviousTabHotKeyPressed(Keys keys) => 
            ((Control.MouseButtons == MouseButtons.None) && this.PreviousTab());

        protected override void OnRender(IDrawingContext dc, RectFloat clipRect)
        {
            if (this.clearTopInset.HasValue)
            {
                RectFloat num = new RectFloat(0f, 0f, (float) base.Width, (float) this.clearTopInset.Value);
                using (dc.UseAxisAlignedClip(num, AntialiasMode.Aliased))
                {
                    dc.Clear(null);
                }
            }
            base.OnRender(dc, clipRect);
        }

        protected override void OnScrollArrowClicked(ArrowDirection arrowDirection)
        {
            int num = 0;
            if (arrowDirection != ArrowDirection.Left)
            {
                if (arrowDirection == ArrowDirection.Right)
                {
                    num = 1;
                }
            }
            else
            {
                num = -1;
            }
            int width = base.ItemSize.Width;
            int deltaScrollOffset = num * width;
            base.SmoothScrollByOffset(deltaScrollOffset);
            base.OnScrollArrowClicked(arrowDirection);
        }

        private void OnThumbnailRendered(object sender, ValueEventArgs<Tuple<IThumbnailProvider, ISurface<ColorBgra>>> e)
        {
            this.VerifyThreadAccess();
            RenderArgs args = null;
            DocumentWorkspace item = (DocumentWorkspace) e.Value.Item1;
            ISurface<ColorBgra> renderer = e.Value.Item2;
            if (this.documents.Contains(item))
            {
                SizeInt32 num = renderer.Size<ColorBgra>();
                if (this.thumbs.ContainsKey(item))
                {
                    args = this.thumbs[item];
                    if (args.Size.ToSizeInt32() != num)
                    {
                        args.ISurface.Dispose();
                        args.Dispose();
                        args = null;
                        this.thumbs.Remove(item);
                    }
                }
                if (args == null)
                {
                    args = new RenderArgs(e.Value.Item2.ToSurface());
                    this.thumbs.Add(item, args);
                }
                e.Value.Item2.Render<ColorBgra>(args.ISurface);
                e.Value.Item2.Dispose();
                this.OnThumbnailUpdated(item);
            }
        }

        private void OnThumbnailUpdated(DocumentWorkspace dw)
        {
            if (this.dw2button.ContainsKey(dw))
            {
                ImageStrip.Item item = this.dw2button[dw];
                RenderArgs args = this.thumbs[dw];
                item.Image = args.Bitmap;
                if (item.ImageOpacity.FinalValue != 1.0)
                {
                    item.ImageOpacity.AnimateValueTo(1.0, 0.5, AnimationTransitionType.SmoothStop);
                }
                item.Update();
            }
        }

        private void OnWorkspaceCompositionUpdated(object sender, EventArgs e)
        {
            this.VerifyThreadAccess();
            DocumentWorkspace dw = (DocumentWorkspace) sender;
            if (this.SelectedDocument == dw)
            {
                this.QueueThumbnailUpdate(dw);
            }
        }

        private void OnWorkspaceDocumentChanged(object sender, EventArgs e)
        {
            this.VerifyThreadAccess();
            DocumentWorkspace workspace = (DocumentWorkspace) sender;
            ImageStrip.Item item = this.dw2button[workspace];
            if (workspace.Document != null)
            {
                item.IsDirty = workspace.Document.Dirty;
                workspace.Document.DirtyChanged += new EventHandler(this.OnDocumentDirtyChanged);
            }
            else
            {
                item.IsDirty = false;
            }
        }

        private void OnWorkspaceDocumentChanging(object sender, ValueEventArgs<Document> e)
        {
            this.VerifyThreadAccess();
            if (e.Value != null)
            {
                e.Value.DirtyChanged -= new EventHandler(this.OnDocumentDirtyChanged);
            }
        }

        public bool PreviousTab()
        {
            this.VerifyThreadAccess();
            bool flag = false;
            if (this.selectedDocument != null)
            {
                int num2 = (this.documents.IndexOf(this.selectedDocument) + (this.documents.Count - 1)) % this.documents.Count;
                this.SelectedDocument = this.documents[num2];
                flag = true;
            }
            return flag;
        }

        public void QueueThumbnailUpdate(DocumentWorkspace dw)
        {
            this.VerifyThreadAccess();
            if (this.suspendThumbnailUpdates <= 0)
            {
                this.thumbnailManager.QueueThumbnailUpdate(dw, base.PreferredImageSize.Width - 2, new ValueEventHandler<Tuple<IThumbnailProvider, ISurface<ColorBgra>>>(this.OnThumbnailRendered));
            }
        }

        public void RefreshAllThumbnails()
        {
            this.VerifyThreadAccess();
            foreach (DocumentWorkspace workspace in this.documents)
            {
                this.QueueThumbnailUpdate(workspace);
            }
        }

        public void RefreshThumbnail(DocumentWorkspace dw)
        {
            this.VerifyThreadAccess();
            if (this.documents.Contains(dw))
            {
                this.QueueThumbnailUpdate(dw);
            }
        }

        public void RemoveDocumentWorkspace(DocumentWorkspace removeMe)
        {
            this.VerifyThreadAccess();
            removeMe.CompositionUpdated -= new EventHandler(this.OnWorkspaceCompositionUpdated);
            if (this.selectedDocument == removeMe)
            {
                this.selectedDocument = null;
            }
            removeMe.DocumentChanging -= new ValueEventHandler<Document>(this.OnWorkspaceDocumentChanging);
            removeMe.DocumentChanged -= new EventHandler(this.OnWorkspaceDocumentChanged);
            if (removeMe.Document != null)
            {
                removeMe.Document.DirtyChanged -= new EventHandler(this.OnDocumentDirtyChanged);
            }
            this.documents.Remove(removeMe);
            this.thumbnailManager.RemoveFromQueue(removeMe);
            ImageStrip.Item item = this.dw2button[removeMe];
            base.RemoveItem(item);
            this.dw2button.Remove(removeMe);
            this.documentButtons.Remove(item);
            if (this.thumbs.ContainsKey(removeMe))
            {
                RenderArgs args = this.thumbs[removeMe];
                ISurface<ColorBgra> iSurface = args.ISurface;
                args.Dispose();
                this.thumbs.Remove(removeMe);
                iSurface.Dispose();
            }
            this.OnDocumentListChanged();
        }

        public void ResumeThumbnailUpdates()
        {
            this.VerifyThreadAccess();
            this.suspendThumbnailUpdates--;
        }

        public void SelectDocumentWorkspace(DocumentWorkspace selectMe)
        {
            this.VerifyThreadAccess();
            bool flag = false;
            if (base.IsHandleCreated)
            {
                UIUtil.SuspendControlPainting(this);
                flag = true;
            }
            this.selectedDocument = selectMe;
            if (this.thumbs.ContainsKey(selectMe))
            {
                RenderArgs args = this.thumbs[selectMe];
                Bitmap bitmap = args.Bitmap;
            }
            else
            {
                this.QueueThumbnailUpdate(selectMe);
            }
            foreach (ImageStrip.Item item in this.documentButtons)
            {
                if ((item.Tag as DocumentWorkspace) == selectMe)
                {
                    base.EnsureItemFullyVisible(item);
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }
            if (flag)
            {
                UIUtil.ResumeControlPainting(this);
                base.Invalidate(true);
            }
        }

        public void SuspendThumbnailUpdates()
        {
            this.VerifyThreadAccess();
            this.suspendThumbnailUpdates++;
        }

        public void UnlockDocumentWorkspaceDirtyValue(DocumentWorkspace unlockMe)
        {
            this.VerifyThreadAccess();
            this.dw2button[unlockMe].UnlockDirtyValue();
        }

        public int? ClearTopInset
        {
            get => 
                this.clearTopInset;
            set
            {
                int? nullable = value;
                int? clearTopInset = this.clearTopInset;
                if ((nullable.GetValueOrDefault() == clearTopInset.GetValueOrDefault()) ? (nullable.HasValue != clearTopInset.HasValue) : true)
                {
                    this.clearTopInset = value;
                    base.Invalidate();
                }
            }
        }

        public int DocumentCount
        {
            get
            {
                this.VerifyThreadAccess();
                return this.documents.Count;
            }
        }

        public DocumentWorkspace[] DocumentList
        {
            get
            {
                this.VerifyThreadAccess();
                return this.documents.ToArrayEx<DocumentWorkspace>();
            }
        }

        public Image[] DocumentThumbnails
        {
            get
            {
                this.VerifyThreadAccess();
                Image[] imageArray = new Image[this.documents.Count];
                for (int i = 0; i < imageArray.Length; i++)
                {
                    RenderArgs args;
                    DocumentWorkspace key = this.documents[i];
                    if (!this.thumbs.TryGetValue(key, out args))
                    {
                        imageArray[i] = null;
                    }
                    else if (args == null)
                    {
                        imageArray[i] = null;
                    }
                    else
                    {
                        imageArray[i] = args.Bitmap;
                    }
                }
                return imageArray;
            }
        }

        public bool EnsureSelectedIsVisible
        {
            get => 
                this.ensureSelectedIsVisible;
            set
            {
                this.VerifyThreadAccess();
                if (this.ensureSelectedIsVisible != value)
                {
                    this.ensureSelectedIsVisible = value;
                    base.PerformLayout();
                }
            }
        }

        public DocumentWorkspace SelectedDocument
        {
            get
            {
                this.VerifyThreadAccess();
                return this.selectedDocument;
            }
            set
            {
                this.VerifyThreadAccess();
                if (!this.documents.Contains(value))
                {
                    throw new ArgumentException("DocumentWorkspace isn't being tracked by this instance of DocumentStrip");
                }
                if (this.selectedDocument != value)
                {
                    this.SelectDocumentWorkspace(value);
                    this.OnDocumentClicked(value, DocumentClickAction.Select);
                    this.Refresh();
                }
            }
        }

        public int SelectedDocumentIndex
        {
            get
            {
                this.VerifyThreadAccess();
                return this.documents.IndexOf(this.selectedDocument);
            }
        }

        public int ThumbnailUpdateLatency
        {
            get => 
                this.thumbnailManager.UpdateLatency;
            set
            {
                this.VerifyThreadAccess();
                this.thumbnailManager.UpdateLatency = value;
            }
        }
    }
}

