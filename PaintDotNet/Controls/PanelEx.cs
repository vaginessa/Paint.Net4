namespace PaintDotNet.Controls
{
    using PaintDotNet.SystemLayer;
    using System;

    internal class PanelEx : ScrollPanel
    {
        private bool hideHScroll;

        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.hideHScroll)
            {
                UIUtil.SuspendControlPainting(this);
            }
            base.OnSizeChanged(e);
            if (this.hideHScroll)
            {
                UIUtil.HideHorizontalScrollBar(this);
                UIUtil.ResumeControlPainting(this);
                base.Invalidate(true);
            }
        }

        public bool HideHScroll
        {
            get => 
                this.hideHScroll;
            set
            {
                this.hideHScroll = value;
            }
        }
    }
}

