namespace Server.Views
{
    partial class RespawnInfoView
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
        /// 设计器支持所需的方法 - 请不要使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            // 资源管理器（用于加载图标等资源）
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RespawnInfoView));

            // 顶部功能区（刷新点编辑相关工具）
            ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            SaveButton = new DevExpress.XtraBars.BarButtonItem();
            ImportButton = new DevExpress.XtraBars.BarButtonItem();
            ExportButton = new DevExpress.XtraBars.BarButtonItem();
            ribbonPage1 = new DevExpress.XtraBars.Ribbon.RibbonPage();
            ribbonPageGroup1 = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            JsonImportExport = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();

            // 刷新点信息主表格
            RespawnInfoGridControl = new DevExpress.XtraGrid.GridControl();
            RespawnInfoGridView = new DevExpress.XtraGrid.Views.Grid.GridView();

            // 列 & 下拉编辑器
            gridColumn1 = new DevExpress.XtraGrid.Columns.GridColumn();
            MonsterLookUpEdit = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            RegionLookUpEdit = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn4 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn5 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn6 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn7 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn8 = new DevExpress.XtraGrid.Columns.GridColumn();
            gridColumn9 = new DevExpress.XtraGrid.Columns.GridColumn();

            ((System.ComponentModel.ISupportInitialize)ribbon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RespawnInfoGridControl).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RespawnInfoGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MonsterLookUpEdit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RegionLookUpEdit).BeginInit();
            SuspendLayout();
            // 
            // ribbon
            // 
            ribbon.ExpandCollapseItem.Id = 0;
            ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
        ribbon.ExpandCollapseItem,
        ribbon.SearchEditItem,
        SaveButton,
        ImportButton,
        ExportButton
    });
            ribbon.Location = new System.Drawing.Point(0, 0);
            ribbon.MaxItemId = 4;
            ribbon.Name = "ribbon";
            ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] { ribbonPage1 });
            ribbon.Size = new System.Drawing.Size(879, 144);
            // 
            // SaveButton
            // 
            SaveButton.Caption = "保存数据库";
            SaveButton.Id = 1;
            SaveButton.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("SaveButton.ImageOptions.Image");
            SaveButton.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("SaveButton.ImageOptions.LargeImage");
            SaveButton.LargeWidth = 60;
            SaveButton.Name = "SaveButton";
            SaveButton.ItemClick += SaveButton_ItemClick;
            // 
            // ImportButton
            // 
            ImportButton.Caption = "导入";
            ImportButton.Id = 2;
            ImportButton.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("ImportButton.ImageOptions.Image");
            ImportButton.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("ImportButton.ImageOptions.LargeImage");
            ImportButton.Name = "ImportButton";
            ImportButton.ItemClick += ImportButton_ItemClick;
            // 
            // ExportButton
            // 
            ExportButton.Caption = "导出";
            ExportButton.Id = 3;
            ExportButton.ImageOptions.Image = (System.Drawing.Image)resources.GetObject("ExportButton.ImageOptions.Image");
            ExportButton.ImageOptions.LargeImage = (System.Drawing.Image)resources.GetObject("ExportButton.ImageOptions.LargeImage");
            ExportButton.Name = "ExportButton";
            ExportButton.ItemClick += ExportButton_ItemClick;
            // 
            // ribbonPage1
            // 
            ribbonPage1.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
        ribbonPageGroup1,
        JsonImportExport
    });
            ribbonPage1.Name = "ribbonPage1";
            ribbonPage1.Text = "主页";
            // 
            // ribbonPageGroup1
            // 
            ribbonPageGroup1.AllowTextClipping = false;
            ribbonPageGroup1.CaptionButtonVisible = DevExpress.Utils.DefaultBoolean.False;
            ribbonPageGroup1.ItemLinks.Add(SaveButton);
            ribbonPageGroup1.Name = "ribbonPageGroup1";
            ribbonPageGroup1.Text = "保存";
            // 
            // JsonImportExport
            // 
            JsonImportExport.ItemLinks.Add(ImportButton);
            JsonImportExport.ItemLinks.Add(ExportButton);
            JsonImportExport.Name = "JsonImportExport";
            JsonImportExport.Text = "Json";
            // 
            // RespawnInfoGridControl
            // 
            RespawnInfoGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            RespawnInfoGridControl.Location = new System.Drawing.Point(0, 144);
            RespawnInfoGridControl.MainView = RespawnInfoGridView;
            RespawnInfoGridControl.MenuManager = ribbon;
            RespawnInfoGridControl.Name = "RespawnInfoGridControl";
            RespawnInfoGridControl.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
        MonsterLookUpEdit,
        RegionLookUpEdit
    });
            RespawnInfoGridControl.Size = new System.Drawing.Size(879, 400);
            RespawnInfoGridControl.TabIndex = 2;
            RespawnInfoGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
        RespawnInfoGridView
    });
            // 
            // RespawnInfoGridView
            // 
            RespawnInfoGridView.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
        gridColumn1,
        gridColumn2,
        gridColumn3,
        gridColumn4,
        gridColumn5,
        gridColumn6,
        gridColumn7,
        gridColumn8,
        gridColumn9
    });
            RespawnInfoGridView.GridControl = RespawnInfoGridControl;
            RespawnInfoGridView.Name = "RespawnInfoGridView";
            RespawnInfoGridView.OptionsView.EnableAppearanceEvenRow = true;
            RespawnInfoGridView.OptionsView.EnableAppearanceOddRow = true;
            RespawnInfoGridView.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Top;
            RespawnInfoGridView.OptionsView.ShowButtonMode = DevExpress.XtraGrid.Views.Base.ShowButtonModeEnum.ShowAlways;
            RespawnInfoGridView.OptionsView.ShowGroupPanel = false;
            // 
            // gridColumn1 - 刷新怪物
            // 
            gridColumn1.Caption = "怪物";
            gridColumn1.ColumnEdit = MonsterLookUpEdit;
            gridColumn1.FieldName = "Monster";
            gridColumn1.Name = "gridColumn1";
            gridColumn1.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            gridColumn1.SortMode = DevExpress.XtraGrid.ColumnSortMode.DisplayText;
            gridColumn1.Visible = true;
            gridColumn1.VisibleIndex = 0;
            // 
            // MonsterLookUpEdit - 怪物下拉选择配置
            // 
            MonsterLookUpEdit.AutoHeight = false;
            MonsterLookUpEdit.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            MonsterLookUpEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
        new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
    });
            MonsterLookUpEdit.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Index", "序号"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("MonsterName", "怪物名称"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AI", "AI"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Level", "等级"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Experience", "经验值"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("IsBoss", "首领怪")
    });
            MonsterLookUpEdit.DisplayMember = "MonsterName";
            MonsterLookUpEdit.Name = "MonsterLookUpEdit";
            MonsterLookUpEdit.NullText = "[怪物为空]";
            // 
            // gridColumn2 - 刷新区域
            // 
            gridColumn2.Caption = "区域";
            gridColumn2.ColumnEdit = RegionLookUpEdit;
            gridColumn2.FieldName = "Region";
            gridColumn2.Name = "gridColumn2";
            gridColumn2.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            gridColumn2.SortMode = DevExpress.XtraGrid.ColumnSortMode.DisplayText;
            gridColumn2.Visible = true;
            gridColumn2.VisibleIndex = 1;
            // 
            // RegionLookUpEdit - 区域下拉选择配置
            // 
            RegionLookUpEdit.AutoHeight = false;
            RegionLookUpEdit.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
            RegionLookUpEdit.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
        new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
    });
            RegionLookUpEdit.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Index", "序号"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("ServerDescription", "服务器描述"),
        new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Size", "区域大小")
    });
            RegionLookUpEdit.DisplayMember = "ServerDescription";
            RegionLookUpEdit.Name = "RegionLookUpEdit";
            RegionLookUpEdit.NullText = "[区域为空]";
            // 
            // gridColumn3 - 刷新间隔
            // 
            gridColumn3.Caption = "刷新间隔";
            gridColumn3.FieldName = "Delay";
            gridColumn3.Name = "gridColumn3";
            gridColumn3.Visible = true;
            gridColumn3.VisibleIndex = 2;
            // 
            // gridColumn4 - 刷新数量
            // 
            gridColumn4.Caption = "数量";
            gridColumn4.FieldName = "Count";
            gridColumn4.Name = "gridColumn4";
            gridColumn4.Visible = true;
            gridColumn4.VisibleIndex = 3;
            // 
            // gridColumn5 - 掉落组
            // 
            gridColumn5.Caption = "掉落组";
            gridColumn5.FieldName = "DropSet";
            gridColumn5.Name = "gridColumn5";
            gridColumn5.Visible = true;
            gridColumn5.VisibleIndex = 4;
            // 
            // gridColumn6 - 事件刷新
            // 
            gridColumn6.Caption = "事件刷新";
            gridColumn6.FieldName = "EventSpawn";
            gridColumn6.Name = "gridColumn6";
            gridColumn6.Visible = true;
            gridColumn6.VisibleIndex = 5;
            // 
            // gridColumn7 - 公告
            // 
            gridColumn7.Caption = "公告";
            gridColumn7.FieldName = "Announce";
            gridColumn7.Name = "gridColumn7";
            gridColumn7.Visible = true;
            gridColumn7.VisibleIndex = 6;
            // 
            // gridColumn8 - 复活事件几率
            // 
            gridColumn8.Caption = "复活事件几率";
            gridColumn8.FieldName = "EasterEventChance";
            gridColumn8.Name = "gridColumn8";
            gridColumn8.Visible = true;
            gridColumn8.VisibleIndex = 7;
            // 
            // gridColumn9 - 刷新编号
            // 
            gridColumn9.Caption = "刷新编号";
            gridColumn9.FieldName = "RespawnIndex";
            gridColumn9.Name = "gridColumn9";
            gridColumn9.Visible = true;
            gridColumn9.VisibleIndex = 8;
            // 
            // RespawnInfoView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(879, 544);
            Controls.Add(RespawnInfoGridControl);
            Controls.Add(ribbon);
            Name = "RespawnInfoView";
            Ribbon = ribbon;
            Text = "刷新信息";
            ((System.ComponentModel.ISupportInitialize)ribbon).EndInit();
            ((System.ComponentModel.ISupportInitialize)RespawnInfoGridControl).EndInit();
            ((System.ComponentModel.ISupportInitialize)RespawnInfoGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)MonsterLookUpEdit).EndInit();
            ((System.ComponentModel.ISupportInitialize)RegionLookUpEdit).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion


        private DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPage1;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroup1;
        private DevExpress.XtraBars.BarButtonItem SaveButton;
        private DevExpress.XtraGrid.GridControl RespawnInfoGridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView RespawnInfoGridView;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn1;
        private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit MonsterLookUpEdit;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit RegionLookUpEdit;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn4;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn5;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn6;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn7;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn8;
        private DevExpress.XtraBars.BarButtonItem ImportButton;
        private DevExpress.XtraBars.BarButtonItem ExportButton;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup JsonImportExport;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn9;
    }
}