namespace Server.Views
{
    partial class MapRegionView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapRegionView));
            DevExpress.XtraEditors.Controls.EditorButtonImageOptions editorButtonImageOptions2 = new DevExpress.XtraEditors.Controls.EditorButtonImageOptions();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject5 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject6 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject7 = new DevExpress.Utils.SerializableAppearanceObject();
            DevExpress.Utils.SerializableAppearanceObject serializableAppearanceObject8 = new DevExpress.Utils.SerializableAppearanceObject();
            ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            SaveButton = new DevExpress.XtraBars.BarButtonItem();
            DeleteRowButton = new DevExpress.XtraBars.BarButtonItem();
            ImportButton = new DevExpress.XtraBars.BarButtonItem();
            ExportButton = new DevExpress.XtraBars.BarButtonItem();
            ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            JsonImportExport = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            CoordinatePanel = new DevExpress.XtraEditors.GroupControl();
            TopLeftXTextEdit = new DevExpress.XtraEditors.TextEdit();
            TopLeftYTextEdit = new DevExpress.XtraEditors.TextEdit();
            BottomRightXTextEdit = new DevExpress.XtraEditors.TextEdit();
            BottomRightYTextEdit = new DevExpress.XtraEditors.TextEdit();
            GenerateRegionButton = new DevExpress.XtraEditors.SimpleButton();
            labelControl1 = new DevExpress.XtraEditors.LabelControl();
            labelControl2 = new DevExpress.XtraEditors.LabelControl();
            labelControl3 = new DevExpress.XtraEditors.LabelControl();
            labelControl4 = new DevExpress.XtraEditors.LabelControl();
            MapRegionGridControl = new DevExpress.XtraGrid.GridControl();
            MapRegionGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            MapLookUpEdit = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            EditButtonEdit = new DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit();
            gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            RegionTypeImageComboBox = new DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox();
            ((System.ComponentModel.ISupportInitialize)ribbon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CoordinatePanel).BeginInit();
            CoordinatePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TopLeftXTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TopLeftYTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BottomRightXTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BottomRightYTextEdit.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MapRegionGridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MapRegionGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MapLookUpEdit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)EditButtonEdit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RegionTypeImageComboBox).BeginInit();
            SuspendLayout();
            // 
            // ribbon
            // 
            ribbon.ExpandCollapseItem.Id = 0;
            ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] { ribbon.ExpandCollapseItem, ribbon.SearchEditItem, SaveButton, DeleteRowButton, ImportButton, ExportButton });
            ribbon.Location = new System.Drawing.Point(0, 0);
            ribbon.MaxItemId = 5;
            ribbon.Name = "ribbon";
            ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] { ribbonPage1 });
            ribbon.Size = new System.Drawing.Size(728, 144);
            // 
            // SaveButton
            // 
            SaveButton.Caption = "保存数据库";
            SaveButton.Id = 1;
            SaveButton.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("SaveButton.ImageOptions.Image");
            SaveButton.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("SaveButton.ImageOptions.LargeImage");
            SaveButton.LargeWidth = 60;
            SaveButton.Name = "SaveButton";
            SaveButton.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            SaveButton.ItemClick += SaveButton_ItemClick;
            // 
            // DeleteRowButton
            // 
            DeleteRowButton.Caption = "删除行";
            DeleteRowButton.Id = 2;
            DeleteRowButton.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("DeleteRowButton.ImageOptions.Image");
            DeleteRowButton.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("DeleteRowButton.ImageOptions.LargeImage");
            DeleteRowButton.Name = "DeleteRowButton";
            DeleteRowButton.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            DeleteRowButton.ItemClick += DeleteRowButton_ItemClick;
            // 
            // ImportButton
            // 
            ImportButton.Caption = "导入";
            ImportButton.Id = 3;
            ImportButton.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("ImportButton.ImageOptions.Image");
            ImportButton.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("ImportButton.ImageOptions.LargeImage");
            ImportButton.Name = "ImportButton";
            ImportButton.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            ImportButton.ItemClick += ImportButton_ItemClick;
            // 
            // ExportButton
            // 
            ExportButton.Caption = "导出";
            ExportButton.Id = 4;
            ExportButton.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("ExportButton.ImageOptions.Image");
            ExportButton.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("ExportButton.ImageOptions.LargeImage");
            ExportButton.Name = "ExportButton";
            ExportButton.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.Large;
            ExportButton.ItemClick += ExportButton_ItemClick;
            // 
            // ribbonPage1
            // 
            ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] { ribbonPageGroup1, JsonImportExport });
            ribbonPage1.Name = "ribbonPage1";
            ribbonPage1.Text = "主页";
            // 
            // ribbonPageGroup1
            // 
            ribbonPageGroup1.AllowTextClipping = false;
            ribbonPageGroup1.CaptionButtonVisible = DevExpress.Utils.DefaultBoolean.False;
            ribbonPageGroup1.ItemLinks.Add(SaveButton);
            ribbonPageGroup1.ItemLinks.Add(DeleteRowButton);
            ribbonPageGroup1.Name = "ribbonPageGroup1";
            ribbonPageGroup1.Text = "数据操作";
            // 
            // JsonImportExport
            // 
            JsonImportExport.ItemLinks.Add(ImportButton);
            JsonImportExport.ItemLinks.Add(ExportButton);
            JsonImportExport.Name = "JsonImportExport";
            JsonImportExport.Text = "Json 导入导出";
            // 
            // CoordinatePanel
            // 
            CoordinatePanel.Controls.Add(GenerateRegionButton);
            CoordinatePanel.Controls.Add(labelControl4);
            CoordinatePanel.Controls.Add(BottomRightYTextEdit);
            CoordinatePanel.Controls.Add(labelControl3);
            CoordinatePanel.Controls.Add(BottomRightXTextEdit);
            CoordinatePanel.Controls.Add(labelControl2);
            CoordinatePanel.Controls.Add(TopLeftYTextEdit);
            CoordinatePanel.Controls.Add(labelControl1);
            CoordinatePanel.Controls.Add(TopLeftXTextEdit);
            CoordinatePanel.Dock = System.Windows.Forms.DockStyle.Top;
            CoordinatePanel.Location = new System.Drawing.Point(0, 144);
            CoordinatePanel.Name = "CoordinatePanel";
            CoordinatePanel.Size = new System.Drawing.Size(728, 80);
            CoordinatePanel.TabIndex = 3;
            CoordinatePanel.Text = "坐标生成区域";
            // 
            // TopLeftXTextEdit
            // 
            TopLeftXTextEdit.EditValue = "0";
            TopLeftXTextEdit.Location = new System.Drawing.Point(60, 30);
            TopLeftXTextEdit.MenuManager = ribbon;
            TopLeftXTextEdit.Name = "TopLeftXTextEdit";
            TopLeftXTextEdit.Size = new System.Drawing.Size(60, 20);
            TopLeftXTextEdit.TabIndex = 0;
            // 
            // TopLeftYTextEdit
            // 
            TopLeftYTextEdit.EditValue = "0";
            TopLeftYTextEdit.Location = new System.Drawing.Point(60, 55);
            TopLeftYTextEdit.MenuManager = ribbon;
            TopLeftYTextEdit.Name = "TopLeftYTextEdit";
            TopLeftYTextEdit.Size = new System.Drawing.Size(60, 20);
            TopLeftYTextEdit.TabIndex = 1;
            // 
            // BottomRightXTextEdit
            // 
            BottomRightXTextEdit.EditValue = "0";
            BottomRightXTextEdit.Location = new System.Drawing.Point(200, 30);
            BottomRightXTextEdit.MenuManager = ribbon;
            BottomRightXTextEdit.Name = "BottomRightXTextEdit";
            BottomRightXTextEdit.Size = new System.Drawing.Size(60, 20);
            BottomRightXTextEdit.TabIndex = 2;
            // 
            // BottomRightYTextEdit
            // 
            BottomRightYTextEdit.EditValue = "0";
            BottomRightYTextEdit.Location = new System.Drawing.Point(200, 55);
            BottomRightYTextEdit.MenuManager = ribbon;
            BottomRightYTextEdit.Name = "BottomRightYTextEdit";
            BottomRightYTextEdit.Size = new System.Drawing.Size(60, 20);
            BottomRightYTextEdit.TabIndex = 3;
            // 
            // GenerateRegionButton
            // 
            GenerateRegionButton.Location = new System.Drawing.Point(280, 35);
            GenerateRegionButton.Name = "GenerateRegionButton";
            GenerateRegionButton.Size = new System.Drawing.Size(100, 35);
            GenerateRegionButton.TabIndex = 4;
            GenerateRegionButton.Text = "生成区域";
            GenerateRegionButton.Click += GenerateRegionButton_Click;
            // 
            // labelControl1
            // 
            labelControl1.Location = new System.Drawing.Point(10, 33);
            labelControl1.Name = "labelControl1";
            labelControl1.Size = new System.Drawing.Size(48, 14);
            labelControl1.TabIndex = 5;
            labelControl1.Text = "左上 X:";
            // 
            // labelControl2
            // 
            labelControl2.Location = new System.Drawing.Point(10, 58);
            labelControl2.Name = "labelControl2";
            labelControl2.Size = new System.Drawing.Size(48, 14);
            labelControl2.TabIndex = 6;
            labelControl2.Text = "左上 Y:";
            // 
            // labelControl3
            // 
            labelControl3.Location = new System.Drawing.Point(150, 33);
            labelControl3.Name = "labelControl3";
            labelControl3.Size = new System.Drawing.Size(48, 14);
            labelControl3.TabIndex = 7;
            labelControl3.Text = "右下 X:";
            // 
            // labelControl4
            // 
            labelControl4.Location = new System.Drawing.Point(150, 58);
            labelControl4.Name = "labelControl4";
            labelControl4.Size = new System.Drawing.Size(48, 14);
            labelControl4.TabIndex = 8;
            labelControl4.Text = "右下 Y:";
            // 
            // MapRegionGridControl
            // 
            MapRegionGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            MapRegionGridControl.Location = new System.Drawing.Point(0, 224);
            MapRegionGridControl.MainView = MapRegionGridView;
            MapRegionGridControl.MenuManager = ribbon;
            MapRegionGridControl.Name = "MapRegionGridControl";
            MapRegionGridControl.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] { MapLookUpEdit, EditButtonEdit, RegionTypeImageComboBox });
            MapRegionGridControl.Size = new System.Drawing.Size(728, 324);
            MapRegionGridControl.TabIndex = 2;
            MapRegionGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] { MapRegionGridView });
            // 
            // MapRegionGridView
            // 
            MapRegionGridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] { gridColumn1, gridColumn2, gridColumn3, gridColumn4, gridColumn5 });
            MapRegionGridView.GridControl = MapRegionGridControl;
            MapRegionGridView.Name = "MapRegionGridView";
            MapRegionGridView.OptionsView.EnableAppearanceEvenRow = true;
            MapRegionGridView.OptionsView.EnableAppearanceOddRow = true;
            MapRegionGridView.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Top;
            MapRegionGridView.OptionsView.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
            MapRegionGridView.OptionsView.ShowGroupPanel = false;
            MapRegionGridView.DoubleClick += MapRegionGridView_DoubleClick;
            // 
            // gridColumn1
            // 
            gridColumn1.Caption = "地图";
            gridColumn1.ColumnEdit = MapLookUpEdit;
            gridColumn1.FieldName = "Map";
            gridColumn1.Name = "gridColumn1";
            gridColumn1.Visible = true;
            gridColumn1.VisibleIndex = 0;
            // 
            // MapLookUpEdit
            // 
            MapLookUpEdit.AutoHeight = false;
            MapLookUpEdit.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            MapLookUpEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
        new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
    });
            MapLookUpEdit.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Index", "编号"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("FileName", "文件名"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Description", "描述")
    });
            MapLookUpEdit.DisplayMember = "Description";
            MapLookUpEdit.Name = "MapLookUpEdit";
            MapLookUpEdit.NullText = "[未选择地图]";
            // 
            // gridColumn2
            // 
            gridColumn2.Caption = "区域描述";
            gridColumn2.FieldName = "Description";
            gridColumn2.Name = "gridColumn2";
            gridColumn2.Visible = true;
            gridColumn2.VisibleIndex = 1;
            // 
            // gridColumn3
            // 
            gridColumn3.Caption = "大小";
            gridColumn3.FieldName = "Size";
            gridColumn3.Name = "gridColumn3";
            gridColumn3.OptionsColumn.AllowEdit = false;
            gridColumn3.OptionsColumn.ReadOnly = true;
            gridColumn3.OptionsColumn.AllowFocus = false;
            gridColumn3.OptionsColumn.TabStop = false;
            gridColumn3.Visible = true;
            gridColumn3.VisibleIndex = 3;
            // 
            // gridColumn4
            // 
            gridColumn4.Caption = "编辑";
            gridColumn4.ColumnEdit = EditButtonEdit;
            gridColumn4.Name = "gridColumn4";
            gridColumn4.Visible = true;
            gridColumn4.VisibleIndex = 4;
            // 
            // EditButtonEdit
            // 
            EditButtonEdit.AutoHeight = false;
            editorButtonImageOptions2.Image = (System.Drawing.Image)resources.GetObject("editorButtonImageOptions2.Image");
            EditButtonEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
        new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph, "", -1, true, true, false, editorButtonImageOptions2, new DevExpress.Utils.KeyShortcut(System.Windows.Forms.Keys.None), serializableAppearanceObject5, serializableAppearanceObject6, serializableAppearanceObject7, serializableAppearanceObject8, "", null, null, DevExpress.Utils.ToolTipAnchor.Default)
    });
            EditButtonEdit.Name = "EditButtonEdit";
            EditButtonEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            EditButtonEdit.ButtonClick += EditButtonEdit_ButtonClick;
            // 
            // gridColumn5
            // 
            gridColumn5.Caption = "区域类型";
            gridColumn5.ColumnEdit = RegionTypeImageComboBox;
            gridColumn5.FieldName = "RegionType";
            gridColumn5.Name = "gridColumn5";
            gridColumn5.Visible = true;
            gridColumn5.VisibleIndex = 2;
            // 
            // RegionTypeImageComboBox
            // 
            RegionTypeImageComboBox.AutoHeight = false;
            RegionTypeImageComboBox.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
        new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
    });
            RegionTypeImageComboBox.Name = "RegionTypeImageComboBox";
            // 
            // MapRegionView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(728, 548);
            Controls.Add(MapRegionGridControl);
            Controls.Add(CoordinatePanel);
            Controls.Add(ribbon);
            Name = "MapRegionView";
            Ribbon = ribbon;
            Text = "区域信息";
            ((System.ComponentModel.ISupportInitialize)ribbon).EndInit();
            ((System.ComponentModel.ISupportInitialize)CoordinatePanel).EndInit();
            CoordinatePanel.ResumeLayout(false);
            CoordinatePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TopLeftXTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)TopLeftYTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)BottomRightXTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)BottomRightYTextEdit.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)MapRegionGridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)MapRegionGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)MapLookUpEdit).EndInit();
            ((System.ComponentModel.ISupportInitialize)EditButtonEdit).EndInit();
            ((System.ComponentModel.ISupportInitialize)RegionTypeImageComboBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion


        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.BarButtonItem SaveButton;
        private DevExpress.XtraGrid.GridControl MapRegionGridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView MapRegionGridView;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit MapLookUpEdit;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        public DevExpress.XtraEditors.Repository.RepositoryItemButtonEdit EditButtonEdit;
        private DevExpress.XtraBars.BarButtonItem DeleteRowButton;
        private DevExpress.XtraBars.BarButtonItem ImportButton;
        private DevExpress.XtraBars.BarButtonItem ExportButton;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup JsonImportExport;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraEditors.Repository.RepositoryItemImageComboBox RegionTypeImageComboBox;
        private DevExpress.XtraEditors.GroupControl CoordinatePanel;
        private DevExpress.XtraEditors.TextEdit TopLeftXTextEdit;
        private DevExpress.XtraEditors.TextEdit TopLeftYTextEdit;
        private DevExpress.XtraEditors.TextEdit BottomRightXTextEdit;
        private DevExpress.XtraEditors.TextEdit BottomRightYTextEdit;
        private DevExpress.XtraEditors.SimpleButton GenerateRegionButton;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl4;
    }
}