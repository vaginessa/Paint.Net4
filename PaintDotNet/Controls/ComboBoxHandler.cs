namespace PaintDotNet.Controls
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Threading;
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal abstract class ComboBoxHandler : ThreadAffinitizedObjectBase, IIsDisposed, IDisposable
    {
        private System.Windows.Forms.ComboBox comboBox;
        private bool isDisposed;

        public ComboBoxHandler(System.Windows.Forms.ComboBox comboBox, DrawMode drawMode = 0)
        {
            Validate.IsNotNull<System.Windows.Forms.ComboBox>(comboBox, "comboBox");
            comboBox.VerifyThreadAccess();
            this.comboBox = comboBox;
            this.comboBox.DrawMode = drawMode;
            this.comboBox.DrawItem += new DrawItemEventHandler(this.OnComboBoxDrawItem);
            this.comboBox.HandleCreated += new EventHandler(this.OnComboBoxHandleCreated);
            this.comboBox.MeasureItem += new MeasureItemEventHandler(this.OnComboBoxMeasureItem);
            this.comboBox.DropDown += new EventHandler(this.OnComboBoxDropDown);
            this.comboBox.DropDownClosed += new EventHandler(this.OnComboBoxDropDownClosed);
            this.comboBox.GotFocus += new EventHandler(this.OnComboBoxGotFocus);
            this.comboBox.SelectedIndexChanged += new EventHandler(this.OnComboBoxSelectedIndexChanged);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.isDisposed = true;
            if (disposing && (this.comboBox != null))
            {
                this.comboBox.DrawItem -= new DrawItemEventHandler(this.OnComboBoxDrawItem);
                this.comboBox.HandleCreated -= new EventHandler(this.OnComboBoxHandleCreated);
                this.comboBox.MeasureItem -= new MeasureItemEventHandler(this.OnComboBoxMeasureItem);
                this.comboBox.DropDown -= new EventHandler(this.OnComboBoxDropDown);
                this.comboBox.GotFocus -= new EventHandler(this.OnComboBoxGotFocus);
                this.comboBox.SelectedIndexChanged -= new EventHandler(this.OnComboBoxSelectedIndexChanged);
                this.comboBox = null;
            }
        }

        ~ComboBoxHandler()
        {
            this.Dispose(false);
        }

        protected virtual void OnComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
        }

        protected virtual void OnComboBoxDropDown(object sender, EventArgs e)
        {
        }

        protected virtual void OnComboBoxDropDownClosed(object sender, EventArgs e)
        {
        }

        protected virtual void OnComboBoxGotFocus(object sender, EventArgs e)
        {
        }

        protected virtual void OnComboBoxHandleCreated(object sender, EventArgs e)
        {
        }

        protected virtual void OnComboBoxMeasureItem(object sender, MeasureItemEventArgs e)
        {
        }

        protected virtual void OnComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
        }

        public System.Windows.Forms.ComboBox ComboBox =>
            this.comboBox;

        public bool IsDisposed =>
            this.isDisposed;
    }
}

