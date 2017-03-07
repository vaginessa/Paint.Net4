namespace PaintDotNet.Controls
{
    using System;
    using System.Windows.Forms;

    internal class PdnNumericUpDown : PdnNumericSpin
    {
        public PdnNumericUpDown()
        {
            base.TextAlign = HorizontalAlignment.Right;
        }

        protected override void OnEnter(EventArgs e)
        {
            base.Select(0, this.Text.Length);
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            decimal num;
            if (base.Value < base.Minimum)
            {
                base.Value = base.Minimum;
            }
            else if (base.Value > base.Maximum)
            {
                base.Value = base.Maximum;
            }
            if (decimal.TryParse(this.Text, out num))
            {
                base.Value = num;
            }
            base.OnLeave(e);
        }
    }
}

