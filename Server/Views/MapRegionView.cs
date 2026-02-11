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
        private void DeleteRowButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：删除当前选中的 MapRegion 行
            // 说明（新手）：
            // - 先检查当前是否选中区域，如果没有会提示用户选择一行
            // - 询问用户确认后，调用 `region.Delete()` 从数据库集合中删除对象
            // - 删除后刷新视图。注意：删除操作在保存数据库后才会永久生效
            GridView view = MapRegionGridControl.FocusedView as GridView;
            if (view == null) return;

            MapRegion region = view.GetFocusedRow() as MapRegion;
            if (region == null)
            {
                System.Windows.Forms.MessageBox.Show(
                    "请选择要删除的区域！",
                    "删除区域",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);
                return;
            }

            //DialogResult result = System.Windows.Forms.MessageBox.Show(
            //    $"确定要删除区域 '{region.Description}' 吗？\n\n" +
            //    $"地图: {region.Map?.Description ?? "未知"}\n" +
            //    $"大小: {region.Size} 格\n\n" +
            //    $"此操作将在保存数据库后永久生效。",
            //    "删除区域",
            //    System.Windows.Forms.MessageBoxButtons.YesNo,
            //    System.Windows.Forms.MessageBoxIcon.Warning);

            //if (result != System.Windows.Forms.DialogResult.Yes) return;

            region.Delete();

            view.RefreshData();

            System.Windows.Forms.MessageBox.Show(
                "区域已删除！\n\n请点击\"保存数据库\"按钮以永久保存更改。",
                "删除成功",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
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

            // 获取当前 GridView 的焦点行（即当前选中的 MapRegion 对象）。
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

        private void MapRegionGridView_DoubleClick(object sender, EventArgs e)
        {
            // 事件：双击表格行时显示区域详细信息
            // 说明（新手）：双击区域时会弹出对话框显示该区域的边界坐标、尺寸等详细信息
            GridView view = MapRegionGridControl.FocusedView as GridView;
            if (view == null) return;

            MapRegion region = view.GetFocusedRow() as MapRegion;
            if (region == null) return;

            //string info = GetRegionBoundsInfo(region);
            //System.Windows.Forms.MessageBox.Show(
            //    info,
            //    $"区域信息 - {region.Description}",
            //    System.Windows.Forms.MessageBoxButtons.OK,
            //    System.Windows.Forms.MessageBoxIcon.Information);
        }

        private void GenerateRegionButton_Click(object sender, EventArgs e)
        {
            // 事件：点击"生成区域"按钮，根据坐标自动创建矩形选择区域
            // 说明（新手）：输入左上角和右下角坐标后，会自动在 MapViewer 中填充该矩形区域
            
            GridView view = MapRegionGridControl.FocusedView as GridView;
            if (view == null) return;

            MapRegion region = view.GetFocusedRow() as MapRegion;
            if (region == null || region.Map == null)
            {
                System.Windows.Forms.MessageBox.Show(
                    "请先选择一个区域并设置地图！",
                    "错误",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            // 解析坐标输入
            if (!int.TryParse(TopLeftXTextEdit.Text, out int x1) ||
                !int.TryParse(TopLeftYTextEdit.Text, out int y1) ||
                !int.TryParse(BottomRightXTextEdit.Text, out int x2) ||
                !int.TryParse(BottomRightYTextEdit.Text, out int y2))
            {
                System.Windows.Forms.MessageBox.Show(
                    "请输入有效的数字坐标！",
                    "错误",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            // 确保坐标顺序正确（左上到右下）
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minY = Math.Min(y1, y2);
            int maxY = Math.Max(y1, y2);

            //// 验证坐标范围
            //if (minX < 0 || minY < 0 || maxX >= region.Map.Width || maxY >= region.Map.Height)
            //{
            //    System.Windows.Forms.MessageBox.Show(
            //        $"坐标超出地图范围！\n地图大小: {region.Map.Width} x {region.Map.Height}",
            //        "错误",
            //        System.Windows.Forms.MessageBoxButtons.OK,
            //        System.Windows.Forms.MessageBoxIcon.Warning);
            //    return;
            //}

            // 打开 MapViewer 并设置区域
            if (MapViewer.CurrentViewer == null)
            {
                MapViewer.CurrentViewer = new MapViewer();
                MapViewer.CurrentViewer.Show();
            }

            MapViewer.CurrentViewer.BringToFront();
            MapViewer.CurrentViewer.Save();
            MapViewer.CurrentViewer.MapRegion = region;

            // 生成矩形区域选择
            var selection = new System.Collections.Generic.HashSet<System.Drawing.Point>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    selection.Add(new System.Drawing.Point(x, y));
                }
            }

            MapViewer.CurrentViewer.Map.Selection = selection;
            MapViewer.CurrentViewer.Map.TextureValid = false;

            // 自动保存
            MapViewer.CurrentViewer.Save();

            System.Windows.Forms.MessageBox.Show(
                $"已生成区域！\n" +
                $"起始坐标: ({minX}, {minY})\n" +
                $"结束坐标: ({maxX}, {maxY})\n" +
                $"区域大小: {selection.Count} 格",
                "成功",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
        }

        /// <summary>
        /// 获取 MapRegion 的边界范围（最小和最大坐标）
        /// </summary>
        /// <param name="region">要分析的地图区域</param>
        /// <returns>返回边界信息，如果区域为空则返回 null</returns>
        //private string GetRegionBoundsInfo(MapRegion region)
        //{
        //    if (region == null || region.Map == null || region.Size == 0)
        //        return "区域为空";

        //    // 获取区域内所有格子的坐标
        //    var points = region.GetPoints(region.Map.Width);

        //    if (points.Count == 0)
        //        return "区域为空";

        //    // 计算边界
        //    int minX = int.MaxValue;
        //    int minY = int.MaxValue;
        //    int maxX = int.MinValue;
        //    int maxY = int.MinValue;

        //    foreach (var point in points)
        //    {
        //        if (point.X < minX) minX = point.X;
        //        if (point.Y < minY) minY = point.Y;
        //        if (point.X > maxX) maxX = point.X;
        //        if (point.Y > maxY) maxY = point.Y;
        //    }

        //    // 计算矩形尺寸
        //    int width = maxX - minX + 1;
        //    int height = maxY - minY + 1;
        //    int rectangleArea = width * height;

        //    // 计算填充率（实际选中格子 / 矩形面积）
        //    double fillRate = (double)region.Size / rectangleArea * 100;

        //    return string.Format(
        //        "起始坐标: ({0}, {1})\n" +
        //        "结束坐标: ({2}, {3})\n" +
        //        "矩形尺寸: {4} x {5} ({6} 格)\n" +
        //        "实际大小: {7} 格\n" +
        //        "填充率: {8:F1}%",
        //        minX, minY, maxX, maxY,
        //        width, height, rectangleArea,
        //        region.Size, fillRate);
        //}
    }
}