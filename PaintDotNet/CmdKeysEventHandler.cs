namespace PaintDotNet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    internal delegate bool CmdKeysEventHandler(object sender, ref Message msg, Keys keyData);
}

