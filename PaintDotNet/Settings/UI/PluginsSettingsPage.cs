namespace PaintDotNet.Settings.UI
{
    using PaintDotNet;
    using PaintDotNet.AppModel;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Diagnostics;
    using PaintDotNet.Resources;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Windows.Forms;

    internal sealed class PluginsSettingsPage : SettingsDialogPage
    {
        private PluginErrorInfo[] allPluginsErrorInfos;
        private TextBox detailsTextBox;
        private ListBox errorsListBox;
        private PdnLabel introText;
        private bool isInitialized;
        private readonly PluginsSettingsSection section;
        private TableLayoutPanel tableLayoutPanel;

        public PluginsSettingsPage(PluginsSettingsSection section) : base(section)
        {
            this.section = section;
            this.tableLayoutPanel = new TableLayoutPanel();
            this.introText = new PdnLabel();
            this.errorsListBox = new ListBox();
            this.detailsTextBox = new TextBox();
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel.Controls.Add(this.introText, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.errorsListBox, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.detailsTextBox, 0, 2);
            this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33333f));
            this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 66.66666f));
            this.introText.AutoSize = true;
            this.tableLayoutPanel.TabIndex = 0;
            this.introText.Text = PdnResources.GetString("SettingsDialog.Plugins.IntroText");
            this.introText.TabIndex = 1;
            this.errorsListBox.Dock = DockStyle.Fill;
            this.errorsListBox.TabIndex = 2;
            this.errorsListBox.SelectedIndexChanged += new EventHandler(this.OnErrorsListBoxSelectedIndexChanged);
            this.detailsTextBox.Dock = DockStyle.Fill;
            this.detailsTextBox.ReadOnly = true;
            this.detailsTextBox.Multiline = true;
            this.detailsTextBox.Font = new Font(FontFamily.GenericMonospace, 8f);
            this.detailsTextBox.TabIndex = 3;
            base.Controls.Add(this.tableLayoutPanel);
        }

        private static string GetDetailTextForFilePath(IPluginErrorService pluginErrorService, IEnumerable<PluginErrorInfo> allPluginErrorInfos, string filePath)
        {
            string format = PdnResources.GetString("SettingsDialog.Plugins.AssemblyDetailText.Name.Format");
            string str2 = PdnResources.GetString("SettingsDialog.Plugins.SupportInfo.DisplayName.Format");
            string str3 = PdnResources.GetString("SettingsDialog.Plugins.SupportInfo.Version.Format");
            string str4 = PdnResources.GetString("SettingsDialog.Plugins.SupportInfo.Author.Format");
            string str5 = PdnResources.GetString("SettingsDialog.Plugins.SupportInfo.Copyright.Format");
            string str6 = PdnResources.GetString("SettingsDialog.Plugins.SupportInfo.Website.Format");
            string str7 = PdnResources.GetString("SettingsDialog.Plugins.SupportInfo.Type.Format");
            if (((allPluginErrorInfos == null) || !allPluginErrorInfos.Any<PluginErrorInfo>()) || (filePath == null))
            {
                return string.Empty;
            }
            string str8 = null;
            IPluginSupportInfo pluginSupportInfo = null;
            if (str8 == null)
            {
                Assembly assembly = (from pei in allPluginErrorInfos
                    where pei.FilePath == filePath
                    where pei.HasAssembly
                    select pei.Assembly).FirstOrDefault<Assembly>();
                if (assembly != null)
                {
                    AssemblyName name = new AssemblyName(assembly.FullName);
                    pluginSupportInfo = PluginSupportInfo.GetPluginSupportInfo(assembly);
                    str8 = string.Format(format, assembly.Location, name.Version);
                }
            }
            if (str8 == null)
            {
                str8 = filePath;
                pluginSupportInfo = null;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(str8);
            builder.Append("\r\n");
            if (pluginSupportInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(pluginSupportInfo.DisplayName))
                {
                    builder.AppendFormat(str2, pluginSupportInfo.DisplayName);
                    builder.Append("\r\n");
                }
                if ((pluginSupportInfo.Version != null) && (pluginSupportInfo.Version != new Version(0, 0, 0, 0)))
                {
                    builder.AppendFormat(str3, pluginSupportInfo.Version.ToString());
                    builder.Append("\r\n");
                }
                if (!string.IsNullOrWhiteSpace(pluginSupportInfo.Author))
                {
                    builder.AppendFormat(str4, pluginSupportInfo.Author);
                    builder.Append("\r\n");
                }
                if (!string.IsNullOrWhiteSpace(pluginSupportInfo.Copyright))
                {
                    builder.AppendFormat(str5, pluginSupportInfo.Copyright);
                    builder.Append("\r\n");
                }
                if (pluginSupportInfo.WebsiteUri != null)
                {
                    builder.AppendFormat(str6, pluginSupportInfo.WebsiteUri.ToString());
                    builder.Append("\r\n");
                }
            }
            builder.Append("\r\n");
            PluginErrorInfo[] infoArray = (from pei in allPluginErrorInfos
                where pei.FilePath == filePath
                orderby pei.TypeName
                select pei).ToArrayEx<PluginErrorInfo>();
            bool flag = true;
            foreach (PluginErrorInfo info2 in infoArray)
            {
                IPluginSupportInfo info3;
                string pluginBlockReasonString;
                if (!flag)
                {
                    builder.Append("\r\n");
                }
                else
                {
                    flag = false;
                }
                if (info2.Type != null)
                {
                    info3 = PluginSupportInfo.GetPluginSupportInfo(info2.Type);
                }
                else
                {
                    info3 = null;
                }
                if ((info3 != null) && !string.IsNullOrWhiteSpace(info3.DisplayName))
                {
                    builder.AppendFormat(str2, info3.DisplayName);
                    builder.Append("\r\n");
                }
                if (info2.HasTypeName)
                {
                    builder.AppendFormat(str7, info2.TypeName);
                    builder.Append("\r\n");
                }
                if (info3 != null)
                {
                    if ((info3.Version != null) && (info3.Version != new Version(0, 0, 0, 0)))
                    {
                        builder.AppendFormat(str3, info3.Version.ToString());
                        builder.Append("\r\n");
                    }
                    if (!string.IsNullOrWhiteSpace(info3.Author))
                    {
                        builder.AppendFormat(str4, info3.Author);
                        builder.Append("\r\n");
                    }
                    if (!string.IsNullOrWhiteSpace(info3.Copyright))
                    {
                        builder.AppendFormat(str5, info3.Copyright);
                        builder.Append("\r\n");
                    }
                    if (info3.WebsiteUri != null)
                    {
                        builder.AppendFormat(str6, info3.WebsiteUri.ToString());
                        builder.Append("\r\n");
                    }
                }
                if (info2.Error is BlockedPluginException)
                {
                    pluginBlockReasonString = GetPluginBlockReasonString(((BlockedPluginException) info2.Error).Reason);
                }
                else
                {
                    pluginBlockReasonString = info2.ErrorString;
                }
                builder.Append(pluginBlockReasonString);
                builder.Append("\r\n");
            }
            return builder.ToString();
        }

        private static string GetPluginBlockReasonString(PluginBlockReason reason)
        {
            StringBuilder builder = new StringBuilder();
            IEnumerable<PluginBlockReason> enumerable2 = from v in Enum.GetValues(typeof(PluginBlockReason)).Cast<PluginBlockReason>()
                where (reason & v) > PluginBlockReason.NotBlocked
                select v;
            EnumLocalizer localizer = EnumLocalizer.Create(typeof(PluginBlockReason));
            return (from r in enumerable2 select localizer.GetLocalizedEnumValue(r).LocalizedName).Aggregate<string, string>(string.Empty, (sa, s) => (sa + ((sa.Length > 0) ? Environment.NewLine : string.Empty) + s));
        }

        private void Initialize()
        {
            this.allPluginsErrorInfos = (from pei in this.section.PluginErrorService.GetPluginLoadErrors()
                orderby pei.FilePath
                select pei).ToArrayEx<PluginErrorInfo>();
            this.errorsListBox.DisplayMember = "FileName";
            Item[] items = (from filePath in this.allPluginsErrorInfos.Select<PluginErrorInfo, string>(pei => pei.FilePath).Distinct<string>() select new Item(this, filePath)).ToArrayEx<Item>();
            this.errorsListBox.Items.AddRange(items);
            this.errorsListBox.SelectedIndex = 0;
        }

        private void OnErrorsListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            Item selectedItem = (Item) this.errorsListBox.SelectedItem;
            if (selectedItem == null)
            {
                this.detailsTextBox.Text = string.Empty;
            }
            else
            {
                this.detailsTextBox.Text = selectedItem.DetailText;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (!this.isInitialized)
            {
                this.Initialize();
                this.isInitialized = true;
            }
            Size clientSize = base.ClientSize;
            int num = UIUtil.ScaleWidth(8);
            int num2 = UIUtil.ScaleHeight(8);
            Size size2 = new Size(base.ClientSize.Width, base.PanelHeight);
            base.ClientSize = size2;
            this.tableLayoutPanel.Location = new Point(0, 0);
            this.tableLayoutPanel.Size = size2;
            base.OnLayout(levent);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PluginsSettingsPage.<>c <>9 = new PluginsSettingsPage.<>c();
            public static Func<PluginErrorInfo, bool> <>9__12_1;
            public static Func<PluginErrorInfo, Assembly> <>9__12_2;
            public static Func<PluginErrorInfo, string> <>9__12_4;
            public static Func<string, string, string> <>9__13_2;
            public static Func<PluginErrorInfo, string> <>9__9_0;
            public static Func<PluginErrorInfo, string> <>9__9_1;

            internal bool <GetDetailTextForFilePath>b__12_1(PluginErrorInfo pei) => 
                pei.HasAssembly;

            internal Assembly <GetDetailTextForFilePath>b__12_2(PluginErrorInfo pei) => 
                pei.Assembly;

            internal string <GetDetailTextForFilePath>b__12_4(PluginErrorInfo pei) => 
                pei.TypeName;

            internal string <GetPluginBlockReasonString>b__13_2(string sa, string s) => 
                (sa + ((sa.Length > 0) ? Environment.NewLine : string.Empty) + s);

            internal string <Initialize>b__9_0(PluginErrorInfo pei) => 
                pei.FilePath;

            internal string <Initialize>b__9_1(PluginErrorInfo pei) => 
                pei.FilePath;
        }

        private sealed class Item : IEquatable<PluginsSettingsPage.Item>
        {
            private string detailText;
            private readonly string filePath;
            private readonly PluginsSettingsPage owner;

            public Item(PluginsSettingsPage owner, string filePath)
            {
                this.owner = Validate.IsNotNull<PluginsSettingsPage>(owner, "owner");
                this.filePath = filePath;
            }

            public bool Equals(PluginsSettingsPage.Item other) => 
                ((other != null) && this.filePath.Equals(other.filePath));

            public override bool Equals(object obj) => 
                EquatableUtil.Equals<PluginsSettingsPage.Item, object>(this, obj);

            public override int GetHashCode() => 
                this.filePath.GetHashCode();

            public string DetailText
            {
                get
                {
                    if (this.detailText == null)
                    {
                        this.detailText = PluginsSettingsPage.GetDetailTextForFilePath(this.owner.section.PluginErrorService, this.owner.allPluginsErrorInfos, this.filePath);
                    }
                    return this.detailText;
                }
            }

            public string FileName
            {
                get
                {
                    try
                    {
                        return Path.GetFileName(this.filePath);
                    }
                    catch (Exception)
                    {
                        return this.filePath;
                    }
                }
            }
        }
    }
}

