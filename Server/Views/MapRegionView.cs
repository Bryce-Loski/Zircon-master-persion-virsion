using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using Library;
using Library.SystemModels;
using System;

namespace Server.Views
{
    public partial class MapRegionView : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        // 说明（给新手）：
        // - 本类是地图区域编辑窗口的代码文件（与 Designer 生成的 `MapRegionView.Designer.cs` 配合）。
        // - Designer 文件只创建控件和布局；本文件负责数据绑定、事件处理和业务行为（例如保存、导入/导出、打开地图查看器）。
        // - 常见的交互点：
        //   * 构造函数：将 RepositoryItem/LookUpEdit 的 DataSource 绑定到 `SMain.Session` 中的集合（运行时注入数据）。
        //   * `OnLoad`：调用 `SMain.SetUpView` 做统一的视图初始化（样式/行为）。
        //   * 按钮事件：Save/Import/Export/编辑按钮的实现都在此文件中。
        public MapRegionView()
        {
            InitializeComponent();

            MapRegionGridControl.DataSource = SMain.Session.GetCollection<MapRegion>().Binding;
            MapLookUpEdit.DataSource = SMain.Session.GetCollection<MapInfo>().Binding;

            RegionTypeImageComboBox.Items.AddEnum<RegionType>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 在窗体加载时对 GridView 做通用初始化（样式、行为、快捷键等）。
            // 说明（新手）：`SMain.SetUpView` 是项目中定义的一个工具方法，用于统一配置 GridView，
            // 例如启用奇偶行样式、新建行位置、按钮显示等，便于所有视图保持一致的 UI/UX。
            SMain.SetUpView(MapRegionGridView);
        }

        private void SaveButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：点击“保存数据库”按钮。
            // 说明（新手）：调用 `SMain.Session.Save(true)` 会把当前 Session 中所有集合的更改持久化到数据库。
            // 在批量修改或重要更改前建议先备份数据库或在测试环境中运行该操作。
            SMain.Session.Save(true);
        }

        private void EditButtonEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            // 事件：编辑按钮被点击（通常出现在某一列的按钮列中，用于在地图查看器中编辑/查看区域）。
            // 说明（新手）：
            // - 该方法会确保 `MapViewer` 窗口存在并被聚焦，然后把当前选中的 `MapRegion` 传给 `MapViewer` 以便编辑。
            // - 在把对象传给 MapViewer 之前，会调用 `MapViewer.CurrentViewer.Save()` 来保存 MapViewer 的当前状态（如果需要）。
            if (MapViewer.CurrentViewer == null)
            {
                MapViewer.CurrentViewer = new MapViewer();
                MapViewer.CurrentViewer.Show();
            }

            MapViewer.CurrentViewer.BringToFront();

            GridView view = MapRegionGridControl.FocusedView as GridView;

            if (view == null) return;

            MapViewer.CurrentViewer.Save();

            // 将当前选中的 MapRegion 对象传递给 MapViewer，MapViewer 将使用该对象来显示或编辑该区域。
            MapViewer.CurrentViewer.MapRegion = view.GetFocusedRow() as MapRegion;
        }

        private void ImportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：导入 MapRegion 的 JSON 文件到当前 Session。
            // 说明（新手）：`JsonImporter.Import<T>()` 通常会弹出文件选择对话框并把 JSON 数据解析为对象后加入到集合中，
            // 导入后若要永久保存，请点击保存按钮或手动调用 `SMain.Session.Save(true)`。
            JsonImporter.Import<MapRegion>();
        }

        private void ExportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：将 MapRegion 数据从 GridView 导出为 JSON。
            // 说明（新手）：`JsonExporter.Export<T>(GridView)` 会基于提供的 GridView 导出其当前展示的数据并弹出保存对话框。
            // 导出的 JSON 可被 `JsonImporter` 重新导入到另一个实例或用于备份。
            JsonExporter.Export<MapRegion>(MapRegionGridView);
        }
    }
}