namespace PaintDotNet.Menus
{
    using PaintDotNet.Controls;
    using PaintDotNet.SystemLayer;
    using System;
    using System.Windows.Forms;

    internal sealed class PdnMainMenu : MenuStripEx
    {
        private AdjustmentsMenu adjustmentsMenu;
        private PaintDotNet.Controls.AppWorkspace appWorkspace;
        private EditMenu editMenu;
        private EffectsMenu effectsMenu;
        private FileMenu fileMenu;
        private ImageMenu imageMenu;
        private LayersMenu layersMenu;
        private ViewMenu viewMenu;

        public PdnMainMenu()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.fileMenu = new FileMenu();
            this.editMenu = new EditMenu();
            this.viewMenu = new ViewMenu();
            this.imageMenu = new ImageMenu();
            this.adjustmentsMenu = new AdjustmentsMenu();
            this.effectsMenu = new EffectsMenu();
            this.layersMenu = new LayersMenu();
            base.SuspendLayout();
            base.Name = "PdnMainMenu";
            base.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            ToolStripItem[] toolStripItems = new ToolStripItem[] { this.fileMenu, this.editMenu, this.viewMenu, this.imageMenu, this.layersMenu, this.adjustmentsMenu, this.effectsMenu };
            this.Items.AddRange(toolStripItems);
            base.ResumeLayout();
        }

        public void PopulateEffects()
        {
            this.adjustmentsMenu.PopulateEffects();
            this.effectsMenu.PopulateEffects();
        }

        public void RunEffect(System.Type effectType)
        {
            this.adjustmentsMenu.RunEffect(effectType);
        }

        public PaintDotNet.Controls.AppWorkspace AppWorkspace
        {
            get => 
                this.appWorkspace;
            set
            {
                this.appWorkspace = value;
                this.fileMenu.AppWorkspace = value;
                this.editMenu.AppWorkspace = value;
                this.viewMenu.AppWorkspace = value;
                this.imageMenu.AppWorkspace = value;
                this.layersMenu.AppWorkspace = value;
                this.adjustmentsMenu.AppWorkspace = value;
                this.effectsMenu.AppWorkspace = value;
            }
        }
    }
}

