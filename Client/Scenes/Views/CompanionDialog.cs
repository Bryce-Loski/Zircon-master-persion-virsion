using Client.Controls;
using Client.Envir;
using Client.Models;
using Client.UserModels;
using Library;
using Library.SystemModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C = Library.Network.ClientPackets;

namespace Client.Scenes.Views
{
    /// <summary>
    /// 伙伴对话框 - 这是游戏中管理玩家伙伴（宠物）的主界面窗口
    /// 功能包括：查看伙伴属性、装备管理、奖励查看、过滤器设置、背包管理
    /// </summary>
    public sealed class CompanionDialog : DXImageControl
    {
        #region Properties

        #region Main
        // === 主界面区域 ===

        // Tab 标签页控制器 - 用于切换不同的功能面板
        private DXTabControl TabControl;
        // 伙伴主页签
        private DXTab CompanionTab;

        // 装备格子数组 - 存放伙伴可以装备的物品（如头部、背部、食物、背包等）
        public DXItemCell[] EquipmentGrid;
        // 伙伴等级信息 - 包含当前等级的各种数据（经验上限、饥饿度上限等）
        private CompanionLevelInfo Info;
        // 伙伴的3D显示对象 - 用于在界面上显示伙伴的外观
        public MonsterObject CompanionDisplay;
        // 伙伴显示的位置坐标
        public Point CompanionDisplayPoint;

        // 各种标题标签（"名称:"、"等级:"、"经验:"、"饥饿度:" 等固定文字）
        public DXLabel NameLabelTitle, LevelLabelTitle, ExperienceLabelTitle, HungerLabelTitle;

        // 功能按钮：关闭窗口、查看奖励、设置过滤器、打开背包
        public DXButton CloseButton, BonusButton, FilterButton, BagButton;

        // 数值显示标签 - 显示伙伴的实际数据
        public DXLabel TitleLabel, HungerLabel, HealthLabel, NameLabel, LevelLabel, ExperienceLabel;

        // 进度条控件 - 用于可视化显示生命值、经验值、饥饿度
        public DXControl HealthBar, ExperienceBar, HungerBar;

        #endregion

        #region Bonus
        // === 奖励面板区域 ===
        // 当伙伴升级到特定等级（3、5、7、10、11、13、15级）时，会获得永久属性奖励

        // 垂直滚动条 - 当奖励项目太多时可以滚动查看
        public DXVScrollBar BonusScrollBar;
        // 奖励面板的容器控件
        public DXControl BonusControl;

        // 奖励属性列表 - 存储所有等级奖励的显示组件
        public List<CompanionBonusStat> BonusStats = new();

        #endregion

        #region Filter
        // === 过滤器面板区域 ===
        // 用于设置伙伴背包自动拾取物品的过滤条件

        // 过滤器面板的容器控件
        public DXImageControl FilterControl;

        // 保存过滤器设置按钮
        public DXButton SaveFilterButton;

        // 职业过滤器 - 按职业筛选（战士、法师、刺客等）
        public Dictionary<MirClass, DXCheckBox> FilterClass = new Dictionary<MirClass, DXCheckBox>();
        // 稀有度过滤器 - 按品质筛选（普通、精英、卓越）
        public Dictionary<Rarity, DXCheckBox> FilterRarity = new Dictionary<Rarity, DXCheckBox>();
        // 物品类型过滤器 - 按类型筛选（武器、防具、饰品等）
        public Dictionary<ItemType, DXCheckBox> FilterType = new Dictionary<ItemType, DXCheckBox>();

        // 三个快捷品质过滤器复选框（当前代码中未使用）
        public DXCheckBox FilterTypeCommon;
        public DXCheckBox FilterTypeElite;
        public DXCheckBox FilterTypeSuperior;

        #endregion

        #region Bag
        // === 背包面板区域 ===
        // 显示伙伴背包中的所有物品

        // 背包面板的容器控件
        public DXImageControl BagControl;

        // 重量显示标签（例如："50 / 100"）
        public DXLabel WeightLabel;
        // 重量进度条 - 可视化显示当前负重
        public DXControl WeightBar;

        // 背包数据：当前重量、最大重量、背包格子数量
        public int BagWeight, MaxBagWeight, InventorySize;

        // 物品网格 - 5x6 的格子，用于存放和显示伙伴拾取的物品
        public DXItemGrid InventoryGrid;

        #endregion

        /// <summary>
        /// 当窗口显示/隐藏状态改变时触发
        /// </summary>
        public override void OnIsVisibleChanged(bool oValue, bool nValue)
        {
            // 如果窗口显示，将其置于最前
            if (IsVisible)
                BringToFront();

            // 同步更新窗口设置的可见状态
            if (Settings != null)
                Settings.Visible = nValue;

            base.OnIsVisibleChanged(oValue, nValue);
        }

        public override void OnLocationChanged(Point oValue, Point nValue)
        {
            base.OnLocationChanged(oValue, nValue);

            if (Settings != null && IsMoving)
                Settings.Location = nValue;
        }

        /// <summary>
        /// 处理键盘按键事件
        /// </summary>
        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    // 按 ESC 键关闭窗口
                    if (CloseButton.Visible)
                    {
                        CloseButton.InvokeMouseClick();
                        // 如果配置为不关闭所有窗口，则标记事件已处理
                        if (!Config.EscapeCloseAll)
                            e.Handled = true;
                    }
                    break;
            }
        }

        #region Settings
        // === 窗口设置区域 ===
        // 保存和恢复窗口的位置、大小、显示状态等

        // 窗口设置对象
        public WindowSetting Settings;
        // 窗口类型标识
        public WindowType Type => WindowType.CompanionBox;

        /// <summary>
        /// 加载窗口设置 - 恢复上次关闭时的窗口状态
        /// </summary>
        public void LoadSettings()
        {
            if (Type == WindowType.None || !CEnvir.Loaded) return;

            // 尝试查找当前分辨率下的窗口设置
            Settings = CEnvir.WindowSettings.Binding.FirstOrDefault(x => x.Resolution == Config.GameSize && x.Window == Type);

            if (Settings != null)
            {
                // 找到设置，应用它
                ApplySettings();
                return;
            }

            // 没有找到设置，创建新的默认设置
            Settings = CEnvir.WindowSettings.CreateNewObject();
            Settings.Resolution = Config.GameSize;
            Settings.Window = Type;
            Settings.Size = Size;
            Settings.Visible = Visible;
            Settings.Location = Location;

            ApplySettings();
        }

        /// <summary>
        /// 应用窗口设置 - 将保存的设置应用到窗口
        /// </summary>
        public void ApplySettings()
        {
            if (Settings == null) return;

            // 恢复窗口位置
            Location = Settings.Location;

            // 恢复窗口可见性
            Visible = Settings.Visible;

            // 默认显示背包面板
            ChangeView("Bag");
        }

        #endregion

        #endregion

        /// <summary>
        /// 构造函数 - 创建伙伴对话框窗口并初始化所有UI元素
        /// </summary>
        public CompanionDialog()
        {
            // 设置窗口背景图片
            LibraryFile = LibraryFile.Interface;
            Index = 141;  // 背景图片索引
            Movable = true;  // 窗口可以拖动
            Sort = true;  // 启用层级排序
            DropShadow = true;  // 显示阴影效果

            // === 创建关闭按钮 ===
            CloseButton = new DXButton
            {
                Parent = this,
                Index = 15,
                LibraryFile = LibraryFile.Interface,
                Hint = CEnvir.Language.CommonControlClose,  // 鼠标悬停提示文字
                HintPosition = HintPosition.TopLeft
            };
            // 定位到窗口右上角
            CloseButton.Location = new Point(DisplayArea.Width - CloseButton.Size.Width - 3, 3);
            // 点击关闭按钮时隐藏窗口
            CloseButton.MouseClick += (o, e) => Visible = false;

            // === 创建窗口标题 ===
            TitleLabel = new DXLabel
            {
                Text = CEnvir.Language.CompanionDialogTitle,  // 标题文字（可能是"伙伴"或"Companion"）
                Parent = this,
                Font = new Font(Config.FontName, CEnvir.FontSize(10F), FontStyle.Bold),  // 粗体字
                ForeColour = Color.FromArgb(198, 166, 99),  // 金黄色文字
                Outline = true,  // 文字描边
                OutlineColour = Color.Black,  // 黑色描边
                IsControl = false,  // 不是交互控件
            };
            // 居中显示标题
            TitleLabel.Location = new Point((DisplayArea.Width - TitleLabel.Size.Width) / 2, 8);

            #region Main
            // === 初始化主界面 ===

            // 创建标签页控制器
            TabControl = new DXTabControl
            {
                Parent = this,
                Location = new Point(0, 38),
                Size = new Size(DisplayArea.Width, DisplayArea.Height - 38),
                MarginLeft = 15
            };
            // 创建伙伴主标签页
            CompanionTab = new DXTab
            {
                Parent = TabControl,
                TabButton = { Label = { Text = CEnvir.Language.CompanionDialogCompanionTabLabel } },
                BackColour = Color.Empty,
                Location = new Point(0, 24)
            };

            // 伙伴3D模型显示的位置坐标
            CompanionDisplayPoint = new Point(90, 140);

            // === 创建三个切换面板的按钮 ===
            // 奖励按钮 - 查看等级奖励属性
            BonusButton = new DXButton
            {
                Parent = CompanionTab,
                Label = { Text = CEnvir.Language.CompanionDialogCompanionTabBonusButtonLabel },
                Location = new Point(10, 263),
                Size = new Size(70, DefaultHeight),
                Enabled = false,  // 初始禁用，点击后才启用其他按钮
            };
            BonusButton.MouseClick += (o, e) => ChangeView("Bonus");  // 点击显示奖励面板

            // 过滤器按钮 - 设置拾取过滤条件
            FilterButton = new DXButton
            {
                Parent = CompanionTab,
                Label = { Text = CEnvir.Language.CompanionDialogCompanionTabFilterButtonLabel },
                Location = new Point(90, 263),
                Size = new Size(70, DefaultHeight),
                Enabled = false,
            };
            FilterButton.MouseClick += (o, e) => ChangeView("Filter");  // 点击显示过滤器面板

            // 背包按钮 - 查看伙伴背包物品
            BagButton = new DXButton
            {
                Parent = CompanionTab,
                Label = { Text = CEnvir.Language.CompanionDialogCompanionTabBagButtonLabel },
                Location = new Point(170, 263),
                Size = new Size(70, DefaultHeight),
                Enabled = false,
            };
            BagButton.MouseClick += (o, e) => ChangeView("Bag");  // 点击显示背包面板

            // === 创建装备格子数组 ===
            EquipmentGrid = new DXItemCell[Globals.CompanionEquipmentSize];

            DXItemCell cell;
            // 创建背包格子（用于装备扩容背包道具）
            EquipmentGrid[(int)CompanionSlot.Bag] = cell = new DXItemCell
            {
                Parent = CompanionTab,
                Location = new Point(198, 17),
                FixedBorder = true,
                Border = true,
                Slot = (int)CompanionSlot.Bag,
                GridType = GridType.CompanionEquipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 99);  // 绘制格子背景图标

            // 创建头部装备格子
            EquipmentGrid[(int)CompanionSlot.Head] = cell = new DXItemCell
            {
                Parent = CompanionTab,
                Location = new Point(198, 59),
                FixedBorder = true,
                Border = true,
                Slot = (int)CompanionSlot.Head,
                GridType = GridType.CompanionEquipment,
            };
            cell.ItemChanged += (o, e) => CompanionEquipmentChanged();  // 装备改变时更新伙伴外观
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 100);

            // 创建背部装备格子（翅膀等）
            EquipmentGrid[(int)CompanionSlot.Back] = cell = new DXItemCell
            {
                Parent = CompanionTab,
                Location = new Point(198, 103),
                FixedBorder = true,
                Border = true,
                Slot = (int)CompanionSlot.Back,
                GridType = GridType.CompanionEquipment,
            };
            cell.ItemChanged += (o, e) => CompanionEquipmentChanged();
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 101);

            // 创建食物格子（用于自动喂食伙伴）
            EquipmentGrid[(int)CompanionSlot.Food] = cell = new DXItemCell
            {
                Parent = CompanionTab,
                Location = new Point(24, 17),
                FixedBorder = true,
                Border = true,
                Slot = (int)CompanionSlot.Food,
                GridType = GridType.CompanionEquipment,
            };
            cell.BeforeDraw += (o, e) => Draw((DXItemCell)o, 102);

            // 获取游戏界面图片库（用于绘制各种进度条）
            CEnvir.LibraryList.TryGetValue(LibraryFile.GameInter, out MirLibrary library);

            // === 创建生命值进度条 ===
            HealthBar = new DXControl
            {
                Parent = CompanionTab,
                Location = new Point(60, 123),
                Size = library.GetSize(4375),
                Visible = false
            };
            // 绘制生命值进度条的逻辑
            HealthBar.BeforeDraw += (o, e) =>
            {
                if (library == null) return;

                if (Info == null) return;

                // 计算生命值百分比（目前只显示存活状态：有伙伴=100%，没有=0%）
                float percent = Math.Min(1, Math.Max(0, GameScene.Game.Companion != null ? 1 : 0));

                if (percent == 0) return;

                // 加载进度条图片
                MirImage image = library.CreateImage(4375, ImageType.Image);

                if (image == null) return;

                // 根据百分比绘制进度条（宽度 = 图片宽度 × 百分比）
                PresentTexture(image.Image, this, new Rectangle(HealthBar.DisplayArea.X, HealthBar.DisplayArea.Y, (int)(image.Width * percent), image.Height), Color.White, HealthBar);
            };

            HealthLabel = new DXLabel
            {
                Parent = CompanionTab,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Outline = true,
                OutlineColour = Color.Black,
                IsControl = false,
                AutoSize = false,
                Size = new Size(128, 20),
                Location = new Point(60, 117),
                Visible = false
            };

            NameLabel = new DXLabel
            {
                Parent = CompanionTab,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Outline = true,
                OutlineColour = Color.Black,
                IsControl = false,
                AutoSize = false,
                Size = new Size(152, 17),
                Location = new Point(73, 156)
            };

            NameLabelTitle = new DXLabel
            {
                Parent = CompanionTab,
                Outline = true,
                Font = new Font(Config.FontName, CEnvir.FontSize(9F), FontStyle.Bold),
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
                ForeColour = Color.FromArgb(198, 166, 99),
                OutlineColour = Color.Black,
                IsControl = false,
                Text = CEnvir.Language.CompanionDialogCompanionTabNameLabel,
                AutoSize = false,
                Size = new Size(60, 17),
                Location = new Point(10, 156)
            };

            LevelLabel = new DXLabel
            {
                Parent = CompanionTab,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Outline = true,
                OutlineColour = Color.Black,
                IsControl = false,
                AutoSize = false,
                Size = new Size(152, 17),
                Location = new Point(73, 178)
            };

            LevelLabelTitle = new DXLabel
            {
                Parent = CompanionTab,
                Outline = true,
                Font = new Font(Config.FontName, CEnvir.FontSize(9F), FontStyle.Bold),
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
                ForeColour = Color.FromArgb(198, 166, 99),
                OutlineColour = Color.Black,
                IsControl = false,
                Text = CEnvir.Language.CompanionDialogCompanionTabLevelLabel,
                AutoSize = false,
                Size = new Size(60, 17),
                Location = new Point(10, 178)
            };

            ExperienceBar = new DXControl
            {
                Parent = CompanionTab,
                Location = new Point(73, 202),
                Size = library.GetSize(4310),
            };
            // 绘制经验值进度条的逻辑
            ExperienceBar.BeforeDraw += (o, e) =>
            {
                if (library == null) return;

                if (Info == null || GameScene.Game.Companion == null) return;

                // 计算经验百分比 = 当前经验 / 升级所需经验
                float percent = Math.Min(1, Math.Max(0, GameScene.Game.Companion.Experience / (float)Info.MaxExperience));

                if (percent == 0) return;

                MirImage image = library.CreateImage(4310, ImageType.Image);

                if (image == null) return;

                // 绘制经验进度条
                PresentTexture(image.Image, this, new Rectangle(ExperienceBar.DisplayArea.X, ExperienceBar.DisplayArea.Y, (int)(image.Width * percent), image.Height), Color.White, ExperienceBar);
            };

            ExperienceLabel = new DXLabel
            {
                Parent = CompanionTab,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Outline = true,
                OutlineColour = Color.Black,
                IsControl = false,
                AutoSize = false,
                Size = new Size(152, 17),
                Location = new Point(73, 200)
            };

            ExperienceLabelTitle = new DXLabel
            {
                Parent = CompanionTab,
                Outline = true,
                Font = new Font(Config.FontName, CEnvir.FontSize(9F), FontStyle.Bold),
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
                ForeColour = Color.FromArgb(198, 166, 99),
                OutlineColour = Color.Black,
                IsControl = false,
                Text = CEnvir.Language.CompanionDialogCompanionTabExpLabel,
                AutoSize = false,
                Size = new Size(60, 17),
                Location = new Point(10, 200)
            };

            HungerBar = new DXControl
            {
                Parent = CompanionTab,
                Location = new Point(73, 224),
                Size = library.GetSize(4311),
            };
            // 绘制饥饿度进度条的逻辑
            HungerBar.BeforeDraw += (o, e) =>
            {
                if (library == null) return;

                if (Info == null || GameScene.Game.Companion == null) return;

                // 计算饥饿度百分比 = 当前饥饿度 / 最大饥饿度
                float percent = Math.Min(1, Math.Max(0, GameScene.Game.Companion.Hunger / (float)Info.MaxHunger));

                if (percent == 0) return;

                MirImage image = library.CreateImage(4311, ImageType.Image);

                if (image == null) return;

                // 绘制饥饿度进度条
                PresentTexture(image.Image, this, new Rectangle(HungerBar.DisplayArea.X, HungerBar.DisplayArea.Y, (int)(image.Width * percent), image.Height), Color.White, HungerBar);
            };

            HungerLabel = new DXLabel
            {
                Parent = CompanionTab,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Outline = true,
                OutlineColour = Color.Black,
                IsControl = false,
                AutoSize = false,
                Size = new Size(152, 17),
                Location = new Point(73, 222)
            };

            HungerLabelTitle = new DXLabel
            {
                Parent = CompanionTab,
                Outline = true,
                Font = new Font(Config.FontName, CEnvir.FontSize(9F), FontStyle.Bold),
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
                ForeColour = Color.FromArgb(198, 166, 99),
                OutlineColour = Color.Black,
                IsControl = false,
                Text = CEnvir.Language.CompanionDialogCompanionTabHungerLabel,
                AutoSize = false,
                Size = new Size(60, 17),
                Location = new Point(10, 222)
            };

            #endregion

            #region Bonus Panel
            // === 初始化奖励面板 ===

            // 创建奖励面板容器
            BonusControl = new DXControl
            {
                Parent = CompanionTab,
                Visible = false,
                Location = new Point(252, 0),
                Size = new Size(208, 300),
            };

            // 创建滚动条（用于滚动查看全部7个等级奖励）
            BonusScrollBar = new DXVScrollBar
            {
                Parent = BonusControl,
                Size = new Size(14, BonusControl.Size.Height - 2),
                Location = new Point(BonusControl.Size.Width - 14, 1),
                VisibleSize = BonusControl.Size.Height,
                Visible = false,
                Change = 57  // 每次滚动移动57像素（一个奖励项的高度）
            };
            // 鼠标滚轮事件绑定到滚动条
            BonusControl.MouseWheel += BonusScrollBar.DoMouseWheel;

            // === 创建7个等级奖励显示项 ===
            int i = 0;
            CompanionBonusStat bonusStat;

            // 3级奖励
            BonusStats.Add(bonusStat = new()
            {
                Parent = BonusControl,
                Location = new Point(0, 5 + (i * 57)),  // 每个项间隔57像素
                Index = i,
                Level = 3
            });
            bonusStat.MouseWheel += BonusScrollBar.DoMouseWheel;

            i++;
            // 5级奖励
            BonusStats.Add(bonusStat = new()
            {
                Parent = BonusControl,
                Location = new Point(0, 5 + (i * 57)),
                Index = i,
                Level = 5
            });
            bonusStat.MouseWheel += BonusScrollBar.DoMouseWheel;

            i++;
            // 7级奖励
            BonusStats.Add(bonusStat = new()
            {
                Parent = BonusControl,
                Location = new Point(0, 5 + (i * 57)),
                Index = i,
                Level = 7
            });
            bonusStat.MouseWheel += BonusScrollBar.DoMouseWheel;

            i++;
            // 10级奖励
            BonusStats.Add(bonusStat = new()
            {
                Parent = BonusControl,
                Location = new Point(0, 5 + (i * 57)),
                Index = i,
                Level = 10
            });
            bonusStat.MouseWheel += BonusScrollBar.DoMouseWheel;

            i++;
            // 11级奖励
            BonusStats.Add(bonusStat = new()
            {
                Parent = BonusControl,
                Location = new Point(0, 5 + (i * 57)),
                Index = i,
                Level = 11
            });
            bonusStat.MouseWheel += BonusScrollBar.DoMouseWheel;

            i++;
            // 13级奖励
            BonusStats.Add(bonusStat = new()
            {
                Parent = BonusControl,
                Location = new Point(0, 5 + (i * 57)),
                Index = i,
                Level = 13
            });
            bonusStat.MouseWheel += BonusScrollBar.DoMouseWheel;

            i++;
            // 15级奖励
            BonusStats.Add(bonusStat = new()
            {
                Parent = BonusControl,
                Location = new Point(0, 5 + (i * 57)),
                Index = i,
                Level = 15
            });
            bonusStat.MouseWheel += BonusScrollBar.DoMouseWheel;

            // 设置滚动条的最大值（7个奖励项 × 57像素 + 15像素边距）
            BonusScrollBar.MaxValue = (BonusStats.Count * 57) + 15;
            // 滚动时更新奖励项的位置
            BonusScrollBar.ValueChanged += (o, e) => UpdateBonusLocations();

            #endregion

            #region Filter Panel
            // === 初始化过滤器面板 ===

            DXLabel label;

            FilterControl = new DXImageControl
            {
                Index = 143,
                LibraryFile = LibraryFile.Interface,
                Parent = CompanionTab,
                Visible = false,
                Location = new Point(252, 0),
                Size = new Size(208, 300)
            };

            // "职业" 标题
            label = new DXLabel
            {
                Parent = FilterControl,
                Outline = true,
                Font = new Font(Config.FontName, CEnvir.FontSize(10F), FontStyle.Bold),
                ForeColour = Color.FromArgb(198, 166, 99),  // 金黄色
                OutlineColour = Color.Black,
                IsControl = false,
                Text = "职业",
            };
            label.Location = new Point(10, 10);

            // 绘制职业过滤器复选框
            DrawClassFilter();

            // "稀有度" 标题
            label = new DXLabel
            {
                Parent = FilterControl,
                Outline = true,
                Font = new Font(Config.FontName, CEnvir.FontSize(10F), FontStyle.Bold),
                ForeColour = Color.FromArgb(198, 166, 99),
                OutlineColour = Color.Black,
                IsControl = false,
                Text = "稀有度类型",
            };
            label.Location = new Point(10, 70);

            // 绘制稀有度过滤器复选框
            DrawRarityFilter();

            // "物品类型" 标题
            label = new DXLabel
            {
                Parent = FilterControl,
                Outline = true,
                Font = new Font(Config.FontName, CEnvir.FontSize(10F), FontStyle.Bold),
                ForeColour = Color.FromArgb(198, 166, 99),
                OutlineColour = Color.Black,
                IsControl = false,
                Text = "物品类型",
            };
            label.Location = new Point(10, 130);

            // 绘制物品类型过滤器复选框
            DrawItemTypeFilter();

            // 保存过滤器设置按钮
            SaveFilterButton = new DXButton
            {
                Parent = this,
                Label = { Text = "保存设置", },  // "保存设置"
                ButtonType = ButtonType.SmallButton,
                Size = new Size(80, SmallButtonHeight)
            };
            SaveFilterButton.Location = new Point(370, 40);
            // 点击保存按钮时的处理
            SaveFilterButton.MouseClick += (o, e) =>
            {
                // 获取所有选中的过滤条件
                List<MirClass> fClass = GetCheckedItemsClass();
                List<Rarity> fRarity = GetCheckedItemsRarity();
                List<ItemType> fType = GetCheckedItemsType();

                // 将过滤器保存到用户设置（用逗号连接成字符串）
                GameScene.Game.User.FiltersClass = String.Join(",", fClass);
                GameScene.Game.User.FiltersRarity = String.Join(",", fRarity);
                GameScene.Game.User.FiltersItemType = String.Join(",", fType);
                // 发送到服务器保存
                CEnvir.Enqueue(new C.SendCompanionFilters { FilterClass = GetCheckedItemsClass(), FilterRarity = GetCheckedItemsRarity(), FilterItemType = GetCheckedItemsType() });
            };

            #endregion

            #region BagPanel
            // === 初始化背包面板 ===

            // 创建背包面板容器
            BagControl = new DXImageControl
            {
                Index = 142,
                LibraryFile = LibraryFile.Interface,
                Parent = CompanionTab,
                Visible = false,
                Location = new Point(252, 0),
                Size = new Size(208, 300),
            };

            // 创建背包物品网格（5列 × 6行 = 30个格子）
            InventoryGrid = new DXItemGrid
            {
                GridSize = new Size(5, 6),  // 5列6行的网格
                Parent = BagControl,
                GridType = GridType.CompanionInventory,
                Location = new Point(10, 14),
                GridPadding = 1,  // 格子之间1像素间距
                BackColour = Color.Empty,
                Border = false
            };

            WeightBar = new DXControl
            {
                Parent = BagControl,
                Location = new Point(8, 266),
                Size = library.GetSize(4312),
            };
            // 绘制重量进度条的逻辑
            WeightBar.BeforeDraw += (o, e) =>
            {
                if (library == null) return;

                if (Info == null || GameScene.Game.Companion == null) return;

                // 计算负重百分比 = 当前重量 / 最大重量
                float percent = Math.Min(1, Math.Max(0, BagWeight / (float)MaxBagWeight));

                if (percent == 0) return;

                MirImage image = library.CreateImage(4312, ImageType.Image);

                if (image == null) return;

                // 绘制重量进度条
                PresentTexture(image.Image, this, new Rectangle(WeightBar.DisplayArea.X, WeightBar.DisplayArea.Y, (int)(image.Width * percent), image.Height), Color.White, WeightBar);
            };

            WeightLabel = new DXLabel
            {
                Parent = BagControl,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                Outline = true,
                OutlineColour = Color.Black,
                IsControl = false,
                Location = new Point(7, 264),
                AutoSize = false,
                Size = new Size(80, 15)
            };

            #endregion
        }

        #region Methods
        // === 主要功能方法 ===

        /// <summary>
        /// 更新奖励项的显示位置（当滚动条滚动时调用）
        /// </summary>
        public void UpdateBonusLocations()
        {
            // 根据滚动条的值计算Y轴偏移量
            int y = -BonusScrollBar.Value;

            // 更新每个奖励项的位置
            foreach (CompanionBonusStat row in BonusStats)
                row.Location = new Point(0, 5 + (57 * row.Index) + y);
        }

        /// <summary>
        /// 更新指定等级的奖励显示
        /// </summary>
        /// <param name="level">等级（3、5、7、10、11、13、15）</param>
        /// <param name="stats">奖励属性（null表示未获得）</param>
        public void UpdateBonus(int level, Stats stats)
        {
            // 查找对应等级的奖励项
            var bonus = BonusStats.FirstOrDefault(x => x.Level == level);

            if (bonus == null) return;

            // 设置标题文字（例如："Lv. 3 奖励"）
            bonus.LevelLabel.Text = $"Lv. {level} 奖励";
            // 设置属性文字（已获得显示属性值，未获得显示"未获得"）
            bonus.StatLabel.Text = stats == null ? "未获得" : stats.GetDisplay(stats.Values.Keys.First());
            // 设置颜色（红色=未获得，绿色=已获得）
            bonus.StatLabel.ForeColour = stats == null ? Color.Red : Color.LimeGreen;
        }

        /// <summary>
        /// 切换显示的面板（奖励、过滤器、背包）
        /// </summary>
        /// <param name="view">要显示的面板名称</param>
        private void ChangeView(string view)
        {
            // 先禁用所有按钮
            BonusButton.Enabled = false;
            FilterButton.Enabled = false;
            BagButton.Enabled = false;

            // 隐藏保存按钮
            SaveFilterButton.Visible = false;

            // 隐藏所有面板
            BonusControl.Visible = false;
            FilterControl.Visible = false;
            BagControl.Visible = false;

            // 根据参数显示对应面板，并启用其他按钮
            switch (view)
            {
                case "Bonus":  // 显示奖励面板
                    BonusControl.Visible = true;
                    BonusButton.Enabled = false;  // 当前按钮保持禁用
                    FilterButton.Enabled = true;  // 启用其他按钮
                    BagButton.Enabled = true;
                    break;
                case "Filter":  // 显示过滤器面板
                    FilterControl.Visible = true;
                    BonusButton.Enabled = true;
                    FilterButton.Enabled = false;
                    BagButton.Enabled = true;
                    SaveFilterButton.Visible = true;  // 显示保存按钮
                    break;
                case "Bag":  // 显示背包面板
                    BagControl.Visible = true;
                    BonusButton.Enabled = true;
                    FilterButton.Enabled = true;
                    BagButton.Enabled = false;
                    break;
            }
        }

        /// <summary>
        /// 当伙伴数据改变时调用（例如：召唤/收回伙伴）
        /// </summary>
        public void CompanionChanged()
        {
            // 如果没有伙伴，关闭窗口
            if (GameScene.Game.Companion == null)
            {
                Visible = false;
                return;
            }

            // 绑定伙伴的背包数组到网格
            InventoryGrid.ItemGrid = GameScene.Game.Companion.InventoryArray;

            // 绑定伙伴的装备数组到装备格子
            foreach (DXItemCell cell in EquipmentGrid)
                cell.ItemGrid = GameScene.Game.Companion.EquipmentArray;

            // 创建伙伴的3D显示对象
            CompanionDisplay = new MonsterObject(GameScene.Game.Companion.CompanionInfo);
            CompanionDisplay.CompanionObject = new ClientCompanionObject
            {
                HeadShape = EquipmentGrid[(int)CompanionSlot.Head].Item?.Info.Shape ?? 0,  // 头部外观
                BackShape = EquipmentGrid[(int)CompanionSlot.Back].Item?.Info.Shape ?? 0,  // 背部外观（翅膀）
                Name = GameScene.Game.Companion.Name
            };

            // 显示伙伴名字
            NameLabel.Text = GameScene.Game.Companion.Name;

            // 刷新界面数据
            Refresh();
        }

        /// <summary>
        /// 当伙伴装备改变时调用（更新伙伴外观）
        /// </summary>
        public void CompanionEquipmentChanged()
        {
            if (CompanionDisplay == null || CompanionDisplay.CompanionObject == null)
                return;

            // 更新伙伴显示对象的外观
            CompanionDisplay.CompanionObject.HeadShape = EquipmentGrid[(int)CompanionSlot.Head].Item?.Info.Shape ?? 0;
            CompanionDisplay.CompanionObject.BackShape = EquipmentGrid[(int)CompanionSlot.Back].Item?.Info.Shape ?? 0;
            CompanionDisplay.CompanionObject.Name = GameScene.Game.Companion.Name;
        }

        /// <summary>
        /// 绘制装备格子的背景图标（当格子为空时显示）
        /// </summary>
        /// <param name="cell">装备格子</param>
        /// <param name="index">图标索引（99=背包、100=头部、101=背部、102=食物）</param>
        public void Draw(DXItemCell cell, int index)
        {
            if (InterfaceLibrary == null) return;

            // 如果格子有物品，不绘制背景图标
            if (cell.Item != null) return;

            // 计算图标居中位置
            Size s = InterfaceLibrary.GetSize(index);
            int x = (cell.Size.Width - s.Width) / 2 + cell.DisplayArea.X;
            int y = (cell.Size.Height - s.Height) / 2 + cell.DisplayArea.Y;

            // 绘制半透明的背景图标
            InterfaceLibrary.Draw(index, x, y, Color.White, false, 0.2F, ImageType.Image);
        }

        /// <summary>
        /// 每帧处理逻辑（更新动画等）
        /// </summary>
        public override void Process()
        {
            base.Process();

            // 更新伙伴3D模型的动画
            CompanionDisplay?.Process();
        }

        /// <summary>
        /// 绘制完成后的额外绘制（绘制伙伴3D模型）
        /// </summary>
        protected override void OnAfterDraw()
        {
            base.OnAfterDraw();

            if (CompanionDisplay == null) return;

            // 计算伙伴显示位置
            int x = DisplayArea.X + CompanionDisplayPoint.X;
            int y = DisplayArea.Y + CompanionDisplayPoint.Y;

            // 驴子模型需要特殊位置调整
            if (CompanionDisplay.Image == MonsterImage.Companion_Donkey)
            {
                x += 10;
                y -= 5;
            }

            // 先绘制阴影，再绘制伙伴身体
            CompanionDisplay.DrawShadow(x, y);
            CompanionDisplay.DrawBody(x, y);
        }

        /// <summary>
        /// 刷新界面所有数据显示
        /// </summary>
        public void Refresh()
        {
            // 更新等级显示
            LevelLabel.Text = "Lv. " + GameScene.Game.Companion.Level.ToString();

            // 获取当前等级的配置信息
            Info = Globals.CompanionLevelInfoList.Binding.First(x => x.Level == GameScene.Game.Companion.Level);

            // 更新生命值显示
            HealthLabel.Text = GameScene.Game.Companion != null ? "100%" : "0%";

            // 更新经验值显示（百分比格式，例如：65.50%）
            ExperienceLabel.Text = Info.MaxExperience > 0 ? $"{GameScene.Game.Companion.Experience / (decimal)Info.MaxExperience:p2}" : "100%";

            // 更新饥饿度显示（例如："50 of 100"）
            HungerLabel.Text = $"{GameScene.Game.Companion.Hunger} of {Info.MaxHunger}";

            // 更新重量显示
            WeightLabel.Text = $"{BagWeight} / {MaxBagWeight}";
            // 超重时显示红色警告
            WeightLabel.ForeColour = BagWeight >= MaxBagWeight ? Color.Red : Color.White;

            // 更新所有等级奖励的显示
            UpdateBonus(3, GameScene.Game.Companion.Level3);
            UpdateBonus(5, GameScene.Game.Companion.Level5);
            UpdateBonus(7, GameScene.Game.Companion.Level7);
            UpdateBonus(10, GameScene.Game.Companion.Level10);
            UpdateBonus(11, GameScene.Game.Companion.Level11);
            UpdateBonus(13, GameScene.Game.Companion.Level13);
            UpdateBonus(15, GameScene.Game.Companion.Level15);

            // 根据背包大小启用/禁用格子
            for (int i = 0; i < InventoryGrid.Grid.Length; i++)
                InventoryGrid.Grid[i].Enabled = i < InventorySize;
        }

        #endregion

        #region Filter Methods
        // === 过滤器相关方法 ===

        /// <summary>
        /// 获取所有选中的职业过滤器
        /// </summary>
        /// <returns>选中的职业列表</returns>
        private List<MirClass> GetCheckedItemsClass()
        {
            List<MirClass> selected = new List<MirClass>();
            foreach (KeyValuePair<MirClass, DXCheckBox> pair in FilterClass)
            {
                if (pair.Value.Checked)
                {
                    selected.Add(pair.Key);
                }
            }
            return selected;
        }
        /// <summary>
        /// 获取所有选中的稀有度过滤器
        /// </summary>
        /// <returns>选中的稀有度列表</returns>
        private List<Rarity> GetCheckedItemsRarity()
        {
            List<Rarity> selected = new List<Rarity>();
            foreach (KeyValuePair<Rarity, DXCheckBox> pair in FilterRarity)
            {
                if (pair.Value.Checked)
                {
                    selected.Add(pair.Key);
                }
            }
            return selected;
        }
        /// <summary>
        /// 获取所有选中的物品类型过滤器
        /// </summary>
        /// <returns>选中的物品类型列表</returns>
        private List<ItemType> GetCheckedItemsType()
        {
            List<ItemType> selected = new List<ItemType>();
            foreach (KeyValuePair<ItemType, DXCheckBox> pair in FilterType)
            {
                if (pair.Value.Checked)
                {
                    selected.Add(pair.Key);
                }
            }
            return selected;
        }

        /// <summary>
        /// 绘制物品类型过滤器的复选框
        /// </summary>
        private void DrawItemTypeFilter()
        {
            Array itemTypes = Enum.GetValues(typeof(ItemType));
            int index = 0;  // 列索引（0或1，两列布局）
            int row = 0;    // 行索引
            foreach (ItemType itemType in itemTypes)
            {
                string item = itemType.ToString();
                // 排除不需要显示的物品类型（消耗品、伙伴专用物品、系统物品等）
                if (item == "Nothing" || item == "Consumable" || item == "Torch" || item == "Poison" || item == "Amulet" || item == "Meat" || item == "Ore" || item == "Currency"
                || item == "DarkStone" || item == "RefineSpecial" || item == "HorseArmour" || item == "CompanionFood" || item == "System" || item == "ItemPart" || item.Contains("Companion")
                || item == "Hook" || item == "Float" || item == "Bait" || item == "Finder" || item == "Reel")
                {
                    continue;
                }

                // 创建复选框
                FilterType[itemType] = new DXCheckBox
                {
                    Parent = FilterControl,
                    Hint = "选择 " + item.ToLower() + " 类物品",  // 鼠标悬停提示
                };
                FilterType[itemType].Location = new Point(10 + (110 * index), 150 + (18 * row));

                // 创建类型名称标签
                DXLabel label = new DXLabel
                {
                    Parent = FilterControl,
                    Outline = true,
                    ForeColour = Color.AntiqueWhite,
                    OutlineColour = Color.Black,
                    IsControl = false,
                    Text = char.ToUpper(item[0]) + item.Substring(1)  // 首字母大写
                };
                label.Location = new Point(25 + (110 * index++), 150 + (18 * row));
                // 两列布局：每两个换行
                if (index % 2 == 0)
                {
                    row++;
                    index = 0;
                }
            }
        }
        /// <summary>
        /// 绘制职业过滤器的复选框（战士、法师、刺客等）
        /// </summary>
        private void DrawClassFilter()
        {
            Array classes = Enum.GetValues(typeof(MirClass));
            int index = 0;  // 列索引
            int row = 0;    // 行索引

            foreach (MirClass mirClass in classes)
            {
                // 创建职业复选框
                FilterClass[mirClass] = new DXCheckBox
                {
                    Parent = FilterControl,
                    Hint = "选择 " + mirClass.ToString().ToLower() + " 职业物品",
                };
                FilterClass[mirClass].Location = new Point(10 + (110 * index), 30 + (18 * row));

                // 创建职业名称标签
                DXLabel label = new DXLabel
                {
                    Parent = FilterControl,
                    Outline = true,
                    ForeColour = Color.AntiqueWhite,
                    OutlineColour = Color.Black,
                    IsControl = false,
                    Text = char.ToUpper(mirClass.ToString()[0]) + mirClass.ToString().Substring(1)
                };
                label.Location = new Point(25 + (110 * index++), 30 + (18 * row));

                // 两列布局
                if (index % 2 == 0)
                {
                    row++;
                    index = 0;
                }
            }
        }
        /// <summary>
        /// 绘制稀有度过滤器的复选框（普通、精英、卓越）
        /// </summary>
        private void DrawRarityFilter()
        {
            Array rarities = Enum.GetValues(typeof(Rarity));
            int index = 0;
            int row = 0;

            foreach (Rarity rarity in rarities)
            {
                // 创建稀有度复选框
                FilterRarity[rarity] = new DXCheckBox
                {
                    Parent = FilterControl,
                    Hint = "选择 " + rarity.ToString().ToLower() + " 品质物品",
                };
                FilterRarity[rarity].Location = new Point(10 + (110 * index), 90 + (18 * row));

                // 根据稀有度设置不同的颜色
                Color rarityLabelColor = Color.AntiqueWhite;  // 普通=米黄色
                switch (rarity)
                {
                    case Rarity.Elite:  // 精英=紫色
                        rarityLabelColor = Color.MediumPurple;
                        break;
                    case Rarity.Superior:  // 卓越=绿色
                        rarityLabelColor = Color.PaleGreen;
                        break;
                }
                // 创建稀有度名称标签
                DXLabel label = new DXLabel
                {
                    Parent = FilterControl,
                    Outline = true,
                    ForeColour = rarityLabelColor,
                    OutlineColour = Color.Black,
                    IsControl = false,
                    Text = char.ToUpper(rarity.ToString()[0]) + rarity.ToString().Substring(1)
                };
                label.Location = new Point(25 + (110 * index++), 90 + (18 * row));

                // 两列布局
                if (index % 2 == 0)
                {
                    row++;
                    index = 0;
                }
            }
        }

        /// <summary>
        /// 从用户设置中恢复过滤器的选中状态
        /// </summary>
        public void RefreshFilter()
        {
            // 恢复职业过滤器的选中状态
            if (GameScene.Game.User.FiltersClass != String.Empty)
            {
                // 将逗号分隔的字符串拆分成列表
                List<string> list = GameScene.Game.User.FiltersClass.Split(',').ToList();
                foreach (KeyValuePair<MirClass, DXCheckBox> pair in FilterClass)
                {
                    // 如果该职业在保存的列表中，勾选它
                    string result = list.Find(x => x == pair.Key.ToString());
                    if (result != null)
                    {
                        pair.Value.Checked = true;
                    }
                }
            }

            // 恢复稀有度过滤器的选中状态
            if (GameScene.Game.User.FiltersRarity != String.Empty)
            {
                List<string> list = GameScene.Game.User.FiltersRarity.Split(',').ToList();
                foreach (KeyValuePair<Rarity, DXCheckBox> pair in FilterRarity)
                {
                    string result = list.Find(x => x == pair.Key.ToString());
                    if (result != null)
                    {
                        pair.Value.Checked = true;
                    }
                }
            }

            // 恢复物品类型过滤器的选中状态
            if (GameScene.Game.User.FiltersItemType != String.Empty)
            {
                List<string> list = GameScene.Game.User.FiltersItemType.Split(',').ToList();
                foreach (KeyValuePair<ItemType, DXCheckBox> pair in FilterType)
                {
                    string result = list.Find(x => x == pair.Key.ToString());
                    if (result != null)
                    {
                        pair.Value.Checked = true;
                    }
                }
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                EquipmentGrid = null;
                Info = null;
                CompanionDisplay = null;
                CompanionDisplayPoint = Point.Empty;

                if (TabControl != null)
                {
                    if (!TabControl.IsDisposed)
                        TabControl.Dispose();

                    TabControl = null;
                }

                if (CompanionTab != null)
                {
                    if (!CompanionTab.IsDisposed)
                        CompanionTab.Dispose();

                    CompanionTab = null;
                }

                if (CloseButton != null)
                {
                    if (!CloseButton.IsDisposed)
                        CloseButton.Dispose();

                    CloseButton = null;
                }

                if (BonusButton != null)
                {
                    if (!BonusButton.IsDisposed)
                        BonusButton.Dispose();

                    BonusButton = null;
                }

                if (FilterButton != null)
                {
                    if (!FilterButton.IsDisposed)
                        FilterButton.Dispose();

                    FilterButton = null;
                }

                if (BagButton != null)
                {
                    if (!BagButton.IsDisposed)
                        BagButton.Dispose();

                    BagButton = null;
                }

                if (TitleLabel != null)
                {
                    if (!TitleLabel.IsDisposed)
                        TitleLabel.Dispose();

                    TitleLabel = null;
                }

                if (HealthBar != null)
                {
                    if (!HealthBar.IsDisposed)
                        HealthBar.Dispose();

                    HealthBar = null;
                }

                if (ExperienceBar != null)
                {
                    if (!ExperienceBar.IsDisposed)
                        ExperienceBar.Dispose();

                    ExperienceBar = null;
                }

                if (HungerBar != null)
                {
                    if (!HungerBar.IsDisposed)
                        HungerBar.Dispose();

                    HungerBar = null;
                }

                if (NameLabelTitle != null)
                {
                    if (!NameLabelTitle.IsDisposed)
                        NameLabelTitle.Dispose();

                    NameLabelTitle = null;
                }
                if (LevelLabelTitle != null)
                {
                    if (!LevelLabelTitle.IsDisposed)
                        LevelLabelTitle.Dispose();

                    LevelLabelTitle = null;
                }

                if (ExperienceLabelTitle != null)
                {
                    if (!ExperienceLabelTitle.IsDisposed)
                        ExperienceLabelTitle.Dispose();

                    ExperienceLabelTitle = null;
                }

                if (HungerLabelTitle != null)
                {
                    if (!HungerLabelTitle.IsDisposed)
                        HungerLabelTitle.Dispose();

                    HungerLabelTitle = null;
                }

                if (HungerLabel != null)
                {
                    if (!HungerLabel.IsDisposed)
                        HungerLabel.Dispose();

                    HungerLabel = null;
                }

                if (HealthLabel != null)
                {
                    if (!HealthLabel.IsDisposed)
                        HealthLabel.Dispose();

                    HealthLabel = null;
                }

                if (NameLabel != null)
                {
                    if (!NameLabel.IsDisposed)
                        NameLabel.Dispose();

                    NameLabel = null;
                }

                if (LevelLabel != null)
                {
                    if (!LevelLabel.IsDisposed)
                        LevelLabel.Dispose();

                    LevelLabel = null;
                }

                if (ExperienceLabel != null)
                {
                    if (!ExperienceLabel.IsDisposed)
                        ExperienceLabel.Dispose();

                    ExperienceLabel = null;
                }

                if (HealthBar != null)
                {
                    if (!HealthBar.IsDisposed)
                        HealthBar.Dispose();

                    HealthBar = null;
                }

                if (BonusScrollBar != null)
                {
                    if (!BonusScrollBar.IsDisposed)
                        BonusScrollBar.Dispose();

                    BonusScrollBar = null;
                }

                if (BonusControl != null)
                {
                    if (!BonusControl.IsDisposed)
                        BonusControl.Dispose();

                    BonusControl = null;
                }

                BonusStats = null;

                if (FilterControl != null)
                {
                    if (!FilterControl.IsDisposed)
                        FilterControl.Dispose();

                    FilterControl = null;
                }

                if (SaveFilterButton != null)
                {
                    if (!SaveFilterButton.IsDisposed)
                        SaveFilterButton.Dispose();

                    SaveFilterButton = null;
                }

                foreach (KeyValuePair<MirClass, DXCheckBox> pair in FilterClass)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                FilterClass.Clear();
                FilterClass = null;

                foreach (KeyValuePair<Rarity, DXCheckBox> pair in FilterRarity)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                FilterRarity.Clear();
                FilterRarity = null;

                foreach (KeyValuePair<ItemType, DXCheckBox> pair in FilterType)
                {
                    if (pair.Value == null) continue;
                    if (pair.Value.IsDisposed) continue;

                    pair.Value.Dispose();
                }
                FilterType.Clear();
                FilterType = null;

                if (FilterTypeCommon != null)
                {
                    if (!FilterTypeCommon.IsDisposed)
                        FilterTypeCommon.Dispose();

                    FilterTypeCommon = null;
                }

                if (FilterTypeElite != null)
                {
                    if (!FilterTypeElite.IsDisposed)
                        FilterTypeElite.Dispose();

                    FilterTypeElite = null;
                }

                if (FilterTypeSuperior != null)
                {
                    if (!FilterTypeSuperior.IsDisposed)
                        FilterTypeSuperior.Dispose();

                    FilterTypeSuperior = null;
                }

                if (BagControl != null)
                {
                    if (!BagControl.IsDisposed)
                        BagControl.Dispose();

                    BagControl = null;
                }

                if (WeightLabel != null)
                {
                    if (!WeightLabel.IsDisposed)
                        WeightLabel.Dispose();

                    WeightLabel = null;
                }

                if (WeightBar != null)
                {
                    if (!WeightBar.IsDisposed)
                        WeightBar.Dispose();

                    WeightBar = null;
                }

                BagWeight = 0;
                MaxBagWeight = 0;
                InventorySize = 0;

                if (InventoryGrid != null)
                {
                    if (!InventoryGrid.IsDisposed)
                        InventoryGrid.Dispose();

                    InventoryGrid = null;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 伙伴奖励属性显示控件 - 显示单个等级奖励的UI组件
    /// </summary>
    public sealed class CompanionBonusStat : DXControl
    {
        #region Index

        public int Index
        {
            get => _Index;
            set
            {
                if (_Index == value) return;

                int oldValue = _Index;
                _Index = value;

                OnIndexChanged(oldValue, value);
            }
        }
        private int _Index;
        public event EventHandler<EventArgs> IndexChanged;
        public void OnIndexChanged(int oValue, int nValue)
        {
            IndexChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        // 此奖励对应的等级（3、5、7、10、11、13、15）
        public int Level;

        // 等级标签和属性标签
        public DXLabel LevelLabel, StatLabel;

        /// <summary>
        /// 构造函数 - 创建一个奖励属性显示控件
        /// </summary>
        public CompanionBonusStat()
        {
            Size = new Size(215, 57);  // 固定大小

            Random r = new Random();

            var i = r.Next(10);

            // 创建等级标签（显示"Lv. X Bonus"）
            LevelLabel = new DXLabel
            {
                Parent = this,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
                Outline = true,
                OutlineColour = Color.Black,
                IsControl = false,
                AutoSize = false,
                Size = new Size(this.Size.Width - 38, 17),
                Location = new Point(20, 8)
            };

            // 创建属性标签（显示具体的属性加成或"Not Gained"）
            StatLabel = new DXLabel
            {
                Parent = this,
                Outline = true,
                ForeColour = Color.White,
                DrawFormat = TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
                OutlineColour = Color.Black,
                IsControl = false,
                AutoSize = false,
                Size = new Size(this.Size.Width - 38, 20),
                Location = new Point(20, 30)
            };
        }

        /// <summary>
        /// 更新显示内容
        /// </summary>
        /// <param name="level">等级</param>
        /// <param name="stat">属性数据</param>
        public void Update(int level, Stats stat)
        {
            LevelLabel.Text = $"Lv. {level} 技能";
            StatLabel.Text = stat.GetDisplay(stat.Values.Keys.FirstOrDefault());
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (LevelLabel != null)
                {
                    if (!LevelLabel.IsDisposed)
                        LevelLabel.Dispose();

                    LevelLabel = null;
                }

                if (StatLabel != null)
                {
                    if (!StatLabel.IsDisposed)
                        StatLabel.Dispose();

                    StatLabel = null;
                }
            }
        }

        #endregion
    }
}
