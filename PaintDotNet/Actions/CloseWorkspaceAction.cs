namespace PaintDotNet.Actions
{
    using PaintDotNet;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Dialogs;
    using PaintDotNet.Drawing;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Runtime;
    using PaintDotNet.SystemLayer;
    using PaintDotNet.Tools;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Threading;
    using System.Windows.Forms;

    internal sealed class CloseWorkspaceAction : AppWorkspaceAction
    {
        private bool cancelled;
        private DocumentWorkspace closeMe;
        private static bool isExecuting;

        public CloseWorkspaceAction() : this(null)
        {
        }

        public CloseWorkspaceAction(DocumentWorkspace closeMe)
        {
            this.closeMe = closeMe;
            this.cancelled = false;
        }

        private void CloseWorkspace(AppWorkspace appWorkspace, DocumentWorkspace dw)
        {
            if (dw != null)
            {
                if (dw.Document == null)
                {
                    appWorkspace.RemoveDocumentWorkspace(dw);
                }
                else if (!dw.Document.Dirty)
                {
                    appWorkspace.RemoveDocumentWorkspace(dw);
                }
                else
                {
                    appWorkspace.ActiveDocumentWorkspace = dw;
                    TaskButton button = new TaskButton(PdnResources.GetImageResource("Icons.MenuFileSaveIcon.png").Reference, PdnResources.GetString("CloseWorkspaceAction.SaveButton.ActionText"), PdnResources.GetString("CloseWorkspaceAction.SaveButton.ExplanationText"));
                    TaskButton button2 = new TaskButton(PdnResources.GetImageResource("Icons.MenuFileCloseIcon.png").Reference, PdnResources.GetString("CloseWorkspaceAction.DontSaveButton.ActionText"), PdnResources.GetString("CloseWorkspaceAction.DontSaveButton.ExplanationText"));
                    TaskButton button3 = new TaskButton(PdnResources.GetImageResource("Icons.CancelIcon.png").Reference, PdnResources.GetString("CloseWorkspaceAction.CancelButton.ActionText"), PdnResources.GetString("CloseWorkspaceAction.CancelButton.ExplanationText"));
                    string str = PdnResources.GetString("CloseWorkspaceAction.Title");
                    string str3 = string.Format(PdnResources.GetString("CloseWorkspaceAction.IntroText.Format"), dw.GetFileFriendlyName());
                    int thumbEdgeLength = UIUtil.ScaleWidth(80);
                    SizeInt32 num = ThumbnailHelpers.ComputeThumbnailSize(dw.Document.Size(), thumbEdgeLength);
                    SizeInt32 fullThumbSize = new SizeInt32(num.Width + 4, num.Height + 4);
                    bool animating = true;
                    Image finalThumb = null;
                    Action<Image> animationEvent = null;
                    Image[] busyAnimationFrames = AnimatedResources.Working;
                    Image[] busyAnimationThumbs = new Image[busyAnimationFrames.Length];
                    int animationHz = 50;
                    Stopwatch timing = new Stopwatch();
                    timing.Start();
                    long elapsedTicks = timing.ElapsedTicks;
                    using (System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer())
                    {
                        <>c__DisplayClass6_0 class_2;
                        Icon icon;
                        timer.Interval = animationHz / 2;
                        timer.Enabled = true;
                        EventHandler handler = delegate (object <sender>, EventArgs <e>) {
                            if (!animating)
                            {
                                timer.Enabled = false;
                                animationEvent(finalThumb);
                                timer.Dispose();
                                for (int j = 0; j < busyAnimationThumbs.Length; j++)
                                {
                                    if (busyAnimationThumbs[j] != null)
                                    {
                                        busyAnimationThumbs[j].Dispose();
                                    }
                                }
                                busyAnimationThumbs = null;
                            }
                            else
                            {
                                int num2 = (int) (timing.ElapsedMilliseconds / ((long) animationHz));
                                int index = num2 % busyAnimationFrames.Length;
                                Image image = busyAnimationFrames[index];
                                if (busyAnimationThumbs[index] == null)
                                {
                                    Bitmap bitmap = new Bitmap(fullThumbSize.Width, fullThumbSize.Height, PixelFormat.Format32bppArgb);
                                    using (Graphics graphics = Graphics.FromImage(bitmap))
                                    {
                                        graphics.CompositingMode = CompositingMode.SourceCopy;
                                        graphics.Clear(Color.Transparent);
                                        graphics.DrawImage(image, new Rectangle((bitmap.Width - image.Width) / 2, (bitmap.Height - image.Height) / 2, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                                    }
                                    busyAnimationThumbs[index] = bitmap;
                                }
                                animationEvent(busyAnimationThumbs[index]);
                            }
                        };
                        timer.Tick += handler;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(class_2.<CloseWorkspace>b__1));
                        Bitmap bitmap = new Bitmap(fullThumbSize.Width, fullThumbSize.Height, PixelFormat.Format32bppArgb);
                        Form form = appWorkspace.FindForm();
                        if (form != null)
                        {
                            PdnBaseForm form2 = form as PdnBaseForm;
                            if (form2 != null)
                            {
                                form2.RestoreWindow();
                            }
                        }
                        ImageResource imageResource = PdnResources.GetImageResource("Icons.WarningIcon.png");
                        if (imageResource != null)
                        {
                            icon = imageResource.Reference.ToIcon();
                        }
                        else
                        {
                            icon = null;
                        }
                        TaskDialog dialog = new TaskDialog {
                            Icon = icon,
                            Title = str,
                            TaskImage = bitmap,
                            ScaleTaskImageWithDpi = false,
                            IntroText = str3
                        };
                        dialog.TaskButtons = new TaskButton[] { button, button2, button3 };
                        dialog.AcceptButton = button;
                        dialog.CancelButton = button3;
                        dialog.PixelWidth96Dpi = 340;
                        TaskDialog taskDialog = dialog;
                        animationEvent = (Action<Image>) Delegate.Combine(animationEvent, image => taskDialog.TaskImage = image);
                        TaskButton button4 = taskDialog.Show(appWorkspace);
                        timer.Enabled = false;
                        timer.Tick -= handler;
                        if (button4 == button)
                        {
                            if (dw.DoSave())
                            {
                                this.cancelled = false;
                                appWorkspace.RemoveDocumentWorkspace(dw);
                            }
                            else
                            {
                                this.cancelled = true;
                            }
                        }
                        else if (button4 == button2)
                        {
                            this.cancelled = false;
                            if ((dw.Tool != null) && (dw.Tool is TransactedTool))
                            {
                                ((TransactedTool) dw.Tool).ForceCancelDrawingOrEditingAndDirty();
                            }
                            appWorkspace.RemoveDocumentWorkspace(dw);
                        }
                        else
                        {
                            this.cancelled = true;
                        }
                        if (finalThumb != null)
                        {
                            finalThumb.Dispose();
                            finalThumb = null;
                        }
                        bitmap.Dispose();
                        bitmap = null;
                    }
                }
            }
        }

        public override void PerformAction(AppWorkspace appWorkspace)
        {
            DocumentWorkspace activeDocumentWorkspace;
            Validate.IsNotNull<AppWorkspace>(appWorkspace, "appWorkspace");
            if (this.closeMe == null)
            {
                activeDocumentWorkspace = appWorkspace.ActiveDocumentWorkspace;
            }
            else
            {
                activeDocumentWorkspace = this.closeMe;
            }
            if (!appWorkspace.FindForm().IsActiveMode())
            {
                this.cancelled = true;
            }
            else if (!appWorkspace.CanSetActiveWorkspace)
            {
                this.cancelled = true;
            }
            else if (!isExecuting)
            {
                isExecuting = true;
                try
                {
                    this.CloseWorkspace(appWorkspace, activeDocumentWorkspace);
                }
                finally
                {
                    isExecuting = false;
                }
            }
            CleanupManager.RequestCleanup();
        }

        public bool Cancelled =>
            this.cancelled;
    }
}

