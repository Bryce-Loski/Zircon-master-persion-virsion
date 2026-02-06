using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using Library;
using Library.SystemModels;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Server.Views
{
    public partial class MapInfoView : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public MapInfoView()
        {
            InitializeComponent();

            MapInfoGridControl.DataSource = SMain.Session.GetCollection<MapInfo>().Binding;
            MonsterLookUpEdit.DataSource = SMain.Session.GetCollection<MonsterInfo>().Binding;
            MapInfoLookUpEdit.DataSource = SMain.Session.GetCollection<MapInfo>().Binding;
            ItemLookUpEdit.DataSource = SMain.Session.GetCollection<ItemInfo>().Binding;
            RegionLookUpEdit.DataSource = SMain.Session.GetCollection<MapRegion>().Binding;

            LightComboBox.Items.AddEnum<LightSetting>();
            WeatherComboBox.Items.AddEnum<Weather>();
            DirectionImageComboBox.Items.AddEnum<MirDirection>();
            MapIconImageComboBox.Items.AddEnum<MapIcon>();
            StartClassImageComboBox.Items.AddEnum<RequiredClass>();
            RequiredClassImageComboBox.Items.AddEnum<RequiredClass>();
            StatImageComboBox.Items.AddEnum<Stat>();

            MiningGridView.CustomRowCellEditForEditing += MiningGridView_CustomRowCellEditForEditing;
        }

        private void MiningGridView_CustomRowCellEditForEditing(object sender, CustomRowCellEditEventArgs e)
        {
            // 事件：当在 MiningGridView 编辑某个单元格时触发，用来动态为 "Region" 列提供筛选过的下拉编辑器。
            // 说明（新手）：
            // - 该方法根据当前选中的地图（MapInfo）筛选 `MapRegion` 集合，只展示属于当前地图的区域。
            // - 返回的是一个临时创建的 `RepositoryItemLookUpEdit`，不会改变全局的 RegionLookUpEdit。
            // - 这使得在“采集/挖矿”条目里选择区域时，只能选择当前地图相关的区域。
            if (e.Column.FieldName == "Region")
            {
                var currentMapRow = MapInfoGridView.GetRow(MapInfoGridView.FocusedRowHandle) as MapInfo;

                var binding = SMain.Session.GetCollection<MapRegion>().Binding;

                var filteredDataSource = binding.Where(x => x.Map == currentMapRow).ToList();

                RepositoryItemLookUpEdit lookupEdit = new()
                {
                    DataSource = filteredDataSource,
                    DisplayMember = "Description",
                    NullText = "[Region is null]"
                };

                lookupEdit.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] { new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Index", "Index"), new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Description", "Description"), new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Size", "Size") });

                e.RepositoryItem = lookupEdit;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SMain.SetUpView(MapInfoGridView);
            SMain.SetUpView(GuardsGridView);
            SMain.SetUpView(RegionGridView);
            SMain.SetUpView(MiningGridView);

            UpdateInfoStats();
        }

        private void UpdateInfoStats()
        {
            // 非事件方法：检查旧版字段（如 MonsterHealth/MonsterDamage 等），并把它们转换为新的 `MapInfoStat` 条目。
            // 说明（新手）：
            // - 这是一次性的数据迁移/清理工具；如果发现旧字段非 0，会创建对应的 BuffStat 并清零旧字段，最后保存 Session。
            // - 由于它会调用 `SMain.Session.Save(true)`，请在确认变更前做好备份（或在测试环境运行）。
            bool needSave = false;

            foreach (var map in SMain.Session.GetCollection<MapInfo>().Binding)
            {
                if (map.MonsterHealth != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MonsterHealth;
                    stat.Amount = map.MonsterHealth;
                    map.BuffStats.Add(stat);

                    map.MonsterHealth = 0;
                    needSave = true;
                }

                if (map.MonsterDamage != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MonsterDamage;
                    stat.Amount = map.MonsterDamage;
                    map.BuffStats.Add(stat);

                    map.MonsterDamage = 0;
                    needSave = true;
                }

                if (map.DropRate != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MonsterDrop;
                    stat.Amount = map.DropRate;
                    map.BuffStats.Add(stat);

                    map.DropRate = 0;
                    needSave = true;
                }

                if (map.ExperienceRate != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MonsterExperience;
                    stat.Amount = map.ExperienceRate;
                    map.BuffStats.Add(stat);

                    map.ExperienceRate = 0;
                    needSave = true;
                }

                if (map.GoldRate != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MonsterGold;
                    stat.Amount = map.GoldRate;
                    map.BuffStats.Add(stat);

                    map.GoldRate = 0;
                    needSave = true;
                }

                if (map.MaxMonsterHealth != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MaxMonsterHealth;
                    stat.Amount = map.MaxMonsterHealth;
                    map.BuffStats.Add(stat);

                    map.MaxMonsterHealth = 0;
                    needSave = true;
                }

                if (map.MaxMonsterDamage != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MaxMonsterDamage;
                    stat.Amount = map.MaxMonsterDamage;
                    map.BuffStats.Add(stat);

                    map.MaxMonsterDamage = 0;
                    needSave = true;
                }

                if (map.MaxDropRate != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MaxMonsterDrop;
                    stat.Amount = map.MaxDropRate;
                    map.BuffStats.Add(stat);

                    map.MaxDropRate = 0;
                    needSave = true;
                }

                if (map.MaxExperienceRate != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MaxMonsterExperience;
                    stat.Amount = map.MaxExperienceRate;
                    map.BuffStats.Add(stat);

                    map.MaxExperienceRate = 0;
                    needSave = true;
                }

                if (map.MaxGoldRate != 0)
                {
                    var stat = SMain.Session.GetCollection<MapInfoStat>().CreateNewObject();
                    stat.Stat = Stat.MaxMonsterGold;
                    stat.Amount = map.MaxGoldRate;
                    map.BuffStats.Add(stat);

                    map.MaxGoldRate = 0;
                    needSave = true;
                }
            }

            if (needSave)
            {
                SMain.Session.Save(true);
            }
        }

        private void SaveButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：点击“保存数据库”按钮。
            // 说明（新手）：
            // - 触发后会调用 `SMain.Session.Save(true)` 将当前内存对象集合持久化到数据库。
            // - 保存是即时且会覆盖数据库中对应数据；在大型改动前建议先备份数据库或在测试环境操作。
            SMain.Session.Save(true);
        }

        private void EditButtonEdit_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            // 事件：点击 RegionGridView 中的“编辑”按钮（按钮列使用 EditButtonEdit）。
            // 说明（新手）：
            // - 该处理器会打开或聚焦 `MapViewer` 窗口，并将当前选中的 `MapRegion` 传给地图查看器用于编辑/显示。
            // - `MapViewer.CurrentViewer.Save()` 在这里被调用：确保 MapViewer 中的未保存更改先被保存。
            if (MapViewer.CurrentViewer == null)
            {
                MapViewer.CurrentViewer = new MapViewer();
                MapViewer.CurrentViewer.Show();
            }

            MapViewer.CurrentViewer.BringToFront();

            GridView view = MapInfoGridControl.FocusedView as GridView;

            if (view == null) return;

            // 保存 MapViewer 当前状态（如果有），再设置 MapRegion 以便在 MapViewer 中加载该区域供编辑。
            MapViewer.CurrentViewer.Save();

            MapViewer.CurrentViewer.MapRegion = view.GetFocusedRow() as MapRegion;
        }

        private void ImportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：导入 JSON 数据到 MapInfo 集合。
            // 说明（新手）：
            // - `JsonImporter.Import<T>()` 通常会弹出文件对话框，读取 JSON 并将数据导入到 Session 中的集合。
            // - 导入后如果需要保存到数据库，可能还需手动点击“保存数据库”。
            JsonImporter.Import<MapInfo>();
        }

        private void ExportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：将 `MapInfo` 集合导出为 JSON（通常基于当前 GridView 选择/全部数据）。
            // 说明（新手）：
            // - `JsonExporter.Export<T>(GridView)` 会读取提供的 GridView 数据并弹出文件保存对话框来保存 JSON 文件。
            // - 导出的是当前视图的数据（可被 `JsonImporter` 复原）。
            JsonExporter.Export<MapInfo>(MapInfoGridView);
        }

        private void InsertRowButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            // 事件：在当前选中地图之后插入一个新的 MapInfo 行。
            // 说明（新手）：
            // - 先检查当前是否选中地图（focusedMap），如果没有会提示用户选择一行。
            // - 询问用户确认后，使用 `SMain.Session.InsertObjectAfter<T>(index)` 在数据集合中插入新对象。
            // - 插入后刷新视图并将焦点移动到新插入的行，方便用户继续编辑新地图的字段。
            var mapCollection = SMain.Session.GetCollection<MapInfo>();

            if (MapInfoGridView.GetFocusedRow() is not MapInfo focusedMap)
            {
                XtraMessageBox.Show("Please select a map to insert after.", "Insert Map", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = XtraMessageBox.Show($"Do you want to insert row after {focusedMap.FileName} - {focusedMap.Description}?", "Insert Map", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            MapInfo newMap = SMain.Session.InsertObjectAfter<MapInfo>(focusedMap.Index);

            MapInfoGridView.RefreshData();

            int bindingIndex = mapCollection.Binding.IndexOf(newMap);
            int rowHandle = MapInfoGridView.GetRowHandle(bindingIndex);

            MapInfoGridView.FocusedRowHandle = rowHandle;
            MapInfoGridView.SelectRow(rowHandle);
        }
    }
}