using DevExpress.XtraBars;
using Library;
using Library.SystemModels;
using Server.Extensions;
using System;

namespace Server.Views
{
    public partial class MagicInfoView : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public MagicInfoView()
        {
            InitializeComponent();

            MagicInfoGridControl.DataSource = SMain.Session.GetCollection<MagicInfo>().Binding;

            MagicImageComboBox.Items.AddEnumWithChineseName<MagicType>();
            PropertyImageComboBox.Items.AddEnum<MagicProperty>();

            // 手动添加职业枚举（中文描述 - 传奇3民间常用说法）
            ClassImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("战士", MirClass.Warrior));
            ClassImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("法师", MirClass.Wizard));
            ClassImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("道士", MirClass.Taoist));
            ClassImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("刺客", MirClass.Assassin));

            // 手动添加技能树枚举（中文描述 - 传奇3民间常用说法）
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("无", MagicSchool.None));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("被动技能", MagicSchool.Passive));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("主动技能", MagicSchool.Active));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("开关技能", MagicSchool.Toggle));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("火系", MagicSchool.Fire));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("冰系", MagicSchool.Ice));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("雷系", MagicSchool.Lightning));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("风系", MagicSchool.Wind));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("神圣系", MagicSchool.Holy));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("暗黑系", MagicSchool.Dark));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("幻影系", MagicSchool.Phantom));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("物理系", MagicSchool.Physical));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("暴力系", MagicSchool.Atrocity));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("杀戮系", MagicSchool.Kill));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("刺杀系", MagicSchool.Assassination));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("骑术", MagicSchool.Horse));
            SchoolImageComboBox.Items.Add(new DevExpress.XtraEditors.Controls.ImageComboBoxItem("修炼", MagicSchool.Discipline));
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SMain.SetUpView(MagicInfoGridView);
        }

        private void SaveButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            SMain.Session.Save(true);
        }

        private void InsertRowButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            MagicInfo newMagic = SMain.Session.GetCollection<MagicInfo>().CreateNewObject();
            MagicInfoGridView.FocusedRowHandle = MagicInfoGridView.LocateByValue("Index", newMagic.Index);
        }

        private void DeleteRowButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (MagicInfoGridView.SelectedRowsCount == 0)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("请先选择要删除的技能行。", "提示", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                return;
            }

            MagicInfo magic = MagicInfoGridView.GetFocusedRow() as MagicInfo;
            if (magic == null) return;

            if (DevExpress.XtraEditors.XtraMessageBox.Show(
                $"确定要删除此技能吗？\n\n技能名称：{magic.Name}\n技能代码：{magic.Magic}\n职业：{magic.Class}",
                "确认删除",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                return;

            magic.Delete();
        }

        private void MagicInfoView_Load(object sender, EventArgs e)
        {

        }

        private void ImportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            JsonImporter.Import<MagicInfo>();
        }

        private void ExportButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            JsonExporter.Export<MagicInfo>(MagicInfoGridView);
        }
    }
}