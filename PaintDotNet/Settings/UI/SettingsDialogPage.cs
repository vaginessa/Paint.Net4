namespace PaintDotNet.Settings.UI
{
    using PaintDotNet.Diagnostics;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal class SettingsDialogPage : Control
    {
        private SettingsDialogSection section;

        public SettingsDialogPage(SettingsDialogSection section)
        {
            Validate.IsNotNull<SettingsDialogSection>(section, "section");
            this.section = section;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int num = UIUtil.ScaleHeight(8);
            if (base.Controls.Count > 0)
            {
                int num2 = base.Controls.Cast<Control>().Max<Control>((Func<Control, int>) (c => c.Bottom));
                base.ClientSize = new Size(base.ClientSize.Width, num2 + num);
            }
            else
            {
                base.ClientSize = new Size(base.ClientSize.Width, 0x10);
            }
            base.OnLayout(levent);
        }

        public int PanelHeight { get; set; }

        public SettingsDialogSection Section =>
            this.section;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SettingsDialogPage.<>c <>9 = new SettingsDialogPage.<>c();
            public static Func<Control, int> <>9__7_0;

            internal int <OnLayout>b__7_0(Control c) => 
                c.Bottom;
        }
    }
}

