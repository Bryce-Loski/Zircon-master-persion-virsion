using DevExpress.XtraBars;
using Library;
using Library.SystemModels;
using System;
using System.Linq;

namespace Server.Views
{
    public partial class MovementInfoView : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public MovementInfoView()
        {
            InitializeComponent();

            MovementGridControl.DataSource = SMain.Session.GetCollection<MovementInfo>().Binding;

            MapLookUpEdit.DataSource = SMain.Session.GetCollection<MapRegion>().Binding.Where(x => x.RegionType == RegionType.None || x.RegionType == RegionType.Connection || x.RegionType == RegionType.SpawnConnection);
            ItemLookUpEdit.DataSource = SMain.Session.GetCollection<ItemInfo>().Binding;
            SpawnLookUpEdit.DataSource = SMain.Session.GetCollection<RespawnInfo>().Binding;
            InstanceLookUpEdit.DataSource = SMain.Session.GetCollection<InstanceInfo>().Binding;

            // 手动添加地图图标枚举（中文描述）
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("无", MapIcon.None));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("洞穴", MapIcon.Cave));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("出口", MapIcon.Exit));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("向下", MapIcon.Down));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("向上", MapIcon.Up));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("省份", MapIcon.Province));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("建筑", MapIcon.Building));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("比奇城", MapIcon.BichonCity));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("城堡", MapIcon.Castle));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("虫穴", MapIcon.BugCaves));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("洞穴上下", MapIcon.CaveUpDown));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("三角小人", MapIcon.SmallManInTriangle));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("沙丘", MapIcon.Dunes));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("门道", MapIcon.Doorway));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("银杏树", MapIcon.GinkoTree));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("森林", MapIcon.Forest));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("虫洞气泡", MapIcon.InsectCaveBubble));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("蚂蚁洞", MapIcon.AntCave));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("金川寺", MapIcon.JinchonTemple));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("矿洞", MapIcon.MiningCave));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("泥墙", MapIcon.Mudwall));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("边境城镇", MapIcon.BorderTown));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("绿洲", MapIcon.Oasis));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("神秘宫殿", MapIcon.UnknownPalace));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("指针", MapIcon.Pointer));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("蛇", MapIcon.Serpent));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("神殿", MapIcon.Shrine));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("骷髅洞", MapIcon.SkullCave));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("骷髅骨洞", MapIcon.SkullBonesCave));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("向下楼梯", MapIcon.StairDown));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("向上楼梯", MapIcon.StairUp));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("神秘寺庙", MapIcon.UnknownTemple));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("走道", MapIcon.Walkway));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("石头寺庙", MapIcon.StoneTemple));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("沃玛寺庙", MapIcon.WoomaTemple));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("祖玛寺庙", MapIcon.ZumaTemple));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("岛屿海岸", MapIcon.IslandShores));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("沙丘走道", MapIcon.DuneWalkway));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("沙丘走道2", MapIcon.DuneWalkway2));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("森林走道", MapIcon.ForestWalkway));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("森林走道2", MapIcon.ForestWalkway2));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("森林走道3", MapIcon.ForestWalkway3));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("星星", MapIcon.Star));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("锁", MapIcon.Lock));
            MapIconImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("船", MapIcon.Boat));
        }

        private void SaveButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            SMain.Session.Save(true);
        }

        private void DeleteRowButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (MovementGridView.SelectedRowsCount == 0)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("请先选择要删除的移动连接行。", "提示", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return;
            }

            MovementInfo movement = MovementGridView.GetFocusedRow() as MovementInfo;
            if (movement == null) return;

            string sourceDesc = movement.SourceRegion?.ServerDescription ?? "未知";
            string destDesc = movement.DestinationRegion?.ServerDescription ?? "未知";

            if (DevExpress.XtraEditors.XtraMessageBox.Show(
                $"确定要删除此移动连接吗？\n\n起始区域：{sourceDesc}\n目标区域：{destDesc}",
                "确认删除",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                return;

            movement.Delete();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SMain.SetUpView(MovementGridView);
        }

        private void ImportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            JsonImporter.Import<MovementInfo>();
        }

        private void ExportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            JsonExporter.Export<MovementInfo>(MovementGridView);
        }
    }
}