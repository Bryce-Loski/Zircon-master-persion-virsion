using Library;
using Library.SystemModels;
using Server.Envir;
using Server.Extensions;
using Server.Views.DirectX;
using SharpDX.Direct3D9;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Blend = SharpDX.Direct3D9.Blend;
using Color4 = SharpDX.Color4;
using D3DResultCode = SharpDX.Direct3D9.ResultCode;
using DataRectangle = SharpDX.DataRectangle;
using Matrix = SharpDX.Matrix;
using Result = SharpDX.Result;

namespace Server.Views
{
    public partial class MapViewer : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        // 新手说明：
        // - 本类负责在一个独立窗口中以 DirectX 渲染并编辑地图区域（MapRegion）或显示整张地图（MapPath）。
        // - 结构上：上半部分是 MapViewer 窗体逻辑（事件处理、UI 交互、保存/加载等）；在文件后半部分还有 DirectX 管理器和 MapControl 的实现。
        // - 重要注意：DirectX 渲染和资源管理（如 Device、Surface）对平台/驱动敏感，建议在 Windows 环境下运行并确保显卡驱动正常。
        // - 若要理解地图数据如何保存/加载，关注 `MapRegion` 的 Setter、`Save()` 方法以及 `SMain.Session` 的使用。

        // 常见交互：
        // - 在其它视图中点击“编辑”会把 `MapRegion` 对象传给此窗口以编辑该区域。
        // - 改动只存在于内存（MapRegion 对象）直到调用 `Save()` 或上层调用 `SMain.Session.Save(true)`。
        public static MapViewer CurrentViewer;
        public DXManager Manager;
        public MapControl Map;

        public DateTime AnimationTime;

        #region MapRegion

        public MapRegion MapRegion
        {
            get { return _MapRegion; }
            set
            {
                if (_MapRegion == value) return;

                MapRegion oldValue = _MapRegion;
                _MapRegion = value;

                OnMapRegionChanged(oldValue, value);
            }
        }
        private MapRegion _MapRegion;
        public event EventHandler<EventArgs> MapRegionChanged;
        public virtual void OnMapRegionChanged(MapRegion oValue, MapRegion nValue)
        {
            // 当 MapRegion 被设置/改变时触发。
            // 说明（新手）：
            // - 清理旧的选择并根据新的 MapRegion 加载地图纹理或区域点（Selection）。
            // - 启用/禁用与区域编辑相关的按钮（例如 Save/Cancel/Attributes）。
            // - 仅修改显示/选择状态，不自动保存到数据库；需要调用 Save() 或外部保存操作。
            Map.Selection.Clear();
            Map.TextureValid = false;

            if (MapRegion == null)
            {
                Map.Width = 0;
                Map.Height = 0;
                Map.Cells = null;
                UpdateScrollBars();
                return;
            }

            if (oValue == null || MapRegion.Map != oValue.Map)
            {
                Map.Load(MapRegion.Map.FileName);
                UpdateScrollBars();
            }

            Map.Selection = MapRegion.GetPoints(Map.Width);

            // 默认启用显示功能，让用户能看到选择区域
            Map.DrawSelection = true;
            Map.DrawAttributes = false;
            
            AttributesButton.Enabled = true;
            BlockedOnlyButton.Enabled = true;
            SelectionButton.Enabled = true;
            SaveButton.Enabled = true;
            CancelButton1.Enabled = true;

            MapRegionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Map Path

        public string MapPath
        {
            get { return _MapPath; }
            set
            {
                if (_MapPath == value) return;

                string oldValue = _MapPath;
                _MapPath = value;

                OnMapPathChanged(oldValue, value);
            }
        }
        private string _MapPath;
        public event EventHandler<EventArgs> MapPathChanged;
        public virtual void OnMapPathChanged(string oValue, string nValue)
        {
            // 当仅指定地图文件路径（MapPath）而不是某个区域时触发。
            // 说明（新手）：
            // - 该方法用于在只查看地图时重置区域编辑状态（MapRegion = null），并加载地图文件。
            // - 与 MapRegion 不同：此时不会有可编辑的区域（Selection 为空），多数编辑按钮会被禁用。
            Map.Selection.Clear();
            Map.TextureValid = false;
            MapRegion = null;

            if (oValue != nValue)
            {
                Map.Load(nValue);
                UpdateScrollBars();
            }

            Map.Selection = new HashSet<Point>();

            AttributesButton.Enabled = false;
            BlockedOnlyButton.Enabled = false;
            SelectionButton.Enabled = false;
            SaveButton.Enabled = false;
            CancelButton1.Enabled = false;

            MapPathChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public MapViewer()
        {
            InitializeComponent();

            // 构造函数：窗体初始化
            // 说明（新手）：InitializeComponent() 是由 Designer 生成的方法，会创建控件与布局；
            // 在此处设置 `CurrentViewer = this` 使得外部窗口可以访问当前打开的 MapViewer 实例。
            CurrentViewer = this;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // 窗体关闭时清理资源
            // 说明（新手）：需要释放 DirectX 相关资源以免占用显存或导致驱动问题。
            if (CurrentViewer == this)
                CurrentViewer = null;

            Manager.Dispose();
            Manager = null;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 窗体加载时初始化 DirectX 管理器和 MapControl
            // 说明（新手）：
            // - `DXManager` 封装了 DirectX 设备/表面/精灵等资源的创建与管理。
            // - `MapControl` 负责地图的绘制、鼠标交互、选择等逻辑。
            // - 注册鼠标滚轮用于调整选取半径（或缩放），并更新滚动条范围以匹配渲染面板大小。
            Manager = new DXManager(DXPanel);
            Manager.Create();
            Map = new MapControl(Manager)
            {
                Size = DXPanel.ClientSize,
            };

            DXPanel.MouseWheel += DXPanel_MouseWheel;

            UpdateScrollBars();
        }


        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            // 窗口大小改变时需要重置 DirectX 设备并调整 MapControl 大小
            // 说明（新手）：
            // - ResetDevice() 会重建或调整 DirectX 设备以适配新的渲染尺寸，这是必须的以避免渲染异常。
            // - 紧接着更新滚动条，让用户可以通过滚动查看较大地图的其他区域。
            if (Manager == null) return;

            Manager.ResetDevice();
            Map.Size = DXPanel.ClientSize;


            UpdateScrollBars();
        }

        public void Process()
        {
            // 主循环调用点：更新环境状态并进行渲染。
            // 说明（新手）：通常由外部定时器或主循环周期性调用此方法以持续刷新地图显示。
            UpdateEnvironment();
            RenderEnvironment();
        }

        private void UpdateEnvironment()
        {
            // 更新显示信息（动画帧、状态栏信息等）
            // 说明（新手）：此方法会更新动画计时器并把状态显示到工具栏（如地图大小、鼠标位置、选中格数）。
            if (SEnvir.Now > AnimationTime && Map != null)
            {
                AnimationTime = SEnvir.Now.AddMilliseconds(100);
                Map.Animation++;
            }

            MapSizeLabel.Caption = string.Format(@"Map Size: {0},{1}", Map.Width, Map.Height);
            PositionLabel.Caption = string.Format(@"Position: {0},{1}", Map.MouseLocation.X, Map.MouseLocation.Y);
            SelectedCellsLabel.Caption = string.Format(@"Selected Cells: {0}", Map.Selection.Count);
        }

        private void RenderEnvironment()
        {
            // 渲染流程：清屏 -> 绘制 -> 呈现
            // 说明（新手）：
            // - 该方法封装了对 DirectX 设备的常规渲染步骤，并处理设备丢失/恢复场景。
            // - 设备丢失（DeviceLost）在显卡切换、睡眠或分辨率变化时可能发生，需要 AttemptReset/AttemptRecovery 来恢复。
            try
            {
                if (Manager.DeviceLost)
                {
                    Manager.AttemptReset();
                    return;
                }

                Manager.Device.Clear(ClearFlags.Target, Color.Black, 1, 0);
                Manager.Device.BeginScene();
                Manager.Sprite.Begin(SpriteFlags.AlphaBlend);
                Manager.SetSurface(Manager.MainSurface);

                Map.Draw();

                Manager.Sprite.End();
                Manager.Device.EndScene();
                Manager.Device.Present();

            }
            catch (SharpDX.SharpDXException ex)
            {
                if (ex.ResultCode == D3DResultCode.DeviceLost)
                {
                    Manager.DeviceLost = true;
                }
                else
                {
                    Manager.AttemptRecovery();
                }
            }
            catch (Exception ex)
            {
                SEnvir.Log(ex.ToString());

                Manager.AttemptRecovery();
            }
        }


        public void UpdateScrollBars()
        {
            if (Map.Width == 0 || Map.Height == 0)
            {
                MapVScroll.Enabled = false;
                MapHScroll.Enabled = false;
                return;
            }

            MapVScroll.Enabled = true;
            MapHScroll.Enabled = true;

            int wCount = (int)(DXPanel.ClientSize.Width / (Map.CellWidth));
            int hCount = (int)(DXPanel.ClientSize.Height / (Map.CellHeight));


            MapVScroll.Maximum = Math.Max(0, Map.Height - hCount + 20);
            MapHScroll.Maximum = Math.Max(0, Map.Width - wCount + 20);

            // 确保滚动条的值在合法范围内，避免越界。
            if (MapVScroll.Value >= MapVScroll.Maximum)
                MapVScroll.Value = MapVScroll.Maximum - 1;

            if (MapHScroll.Value >= MapHScroll.Maximum)
                MapHScroll.Value = MapHScroll.Maximum - 1;
        }

        private void MapVScroll_ValueChanged(object sender, EventArgs e)
        {
            // 垂直滚动条改变：设置地图的起始 Y（顶端格子）以滚动显示内容
            Map.StartY = MapVScroll.Value;
        }
        private void MapHScroll_ValueChanged(object sender, EventArgs e)
        {
            // 水平滚动条改变：设置地图的起始 X（左端格子）以滚动显示内容
            Map.StartX = MapHScroll.Value;
        }

        private void ZoomResetButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 缩放重置：恢复到 1x 缩放并刷新滚动条范围
            Map.Zoom = 1;
            UpdateScrollBars();
        }

        private void ZoomInButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 放大视图：每次乘 2，最大限制 4x
            Map.Zoom *= 2F;
            if (Map.Zoom > 4F)
                Map.Zoom = 4F;

            UpdateScrollBars();
        }

        private void ZoomOutButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 缩小视图：每次除以 2，最小限制 0.01x（防止除以 0 或过小导致异常）
            Map.Zoom /= 2;
            if (Map.Zoom < 0.01F)
                Map.Zoom = 0.01F;

            UpdateScrollBars();
        }

        private void AttributesButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 切换显示地图属性（例如阻挡/属性格），对编辑时观察不同图层有帮助
            Map.DrawAttributes = !Map.DrawAttributes;
        }

        private void DXPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // 鼠标滚轮（在 DXPanel 上）：通常用来调整当前工具的半径（例如选择/刷子大小），不是缩放
            // 说明（新手）：缩放由 ZoomIn/ZoomOut/Reset 控件控制；鼠标滚轮在此处被用作半径调整。
            if (Map == null) return;
            
            Map.Radius = Math.Max(0, Map.Radius - e.Delta / SystemInformation.MouseWheelScrollDelta);
        }
        private void DXPanel_MouseDown(object sender, MouseEventArgs e)
        {
            // 鼠标按下事件转到 MapControl，MapControl 负责处理选择/绘制起始点等行为
            if (Map == null) return;
            
            Map.MouseDown(e);
        }

        private void DXPanel_MouseMove(object sender, MouseEventArgs e)
        {
            // 鼠标移动事件转到 MapControl，通常用于更新鼠标位置、显示悬浮信息或动态绘制选择
            if (Map == null) return;
            
            Map.MouseMove(e);
        }

        private void DXPanel_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void DXPanel_MouseEnter(object sender, EventArgs e)
        {
            // 鼠标进入渲染面板：通知 MapControl 进入状态（例如显示光标）
            if (Map == null) return;
            
            Map.MouseEnter();
        }

        private void DXPanel_MouseLeave(object sender, EventArgs e)
        {
            // 鼠标离开渲染面板：通知 MapControl 退出状态（例如隐藏光标）
            if (Map == null) return;
            
            Map.MouseLeave();
        }

        private void SelectionButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 切换显示/隐藏选区（Selection）的绘制，便于查看或隐藏当前选择的格子
            Map.DrawSelection = !Map.DrawSelection;
        }

        private void SaveButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 保存当前 MapRegion 的选择到 MapRegion 对象（但不自动写入数据库）
            // 说明（新手）：调用此方法会把 Selection 转换为 BitRegion 或 PointRegion 并写回 MapRegion 对象，
            // 仍需在上层调用 `SMain.Session.Save(true)` 才能将更改持久化到数据库（某些视图会自动调用）。
            Save();
        }

        public void Save()
        {
            // 将当前 Map.Selection 写回到 MapRegion（内存对象）中
            // 说明（新手）：
            // - 为了节省空间，代码会在两种存储格式之间切换：当选择点占地图很大比例时，用 BitArray（位图）表示，
            //   否则使用 Point[] 列表表示具体坐标。
            // - 这里只是把数据写回 MapRegion 对象，若要持久化到数据库需额外调用 `SMain.Session.Save(true)`。
            if (MapRegion == null) return;

            BitArray bitRegion = null;
            Point[] pointRegion = null;

            if (Map.Selection.Count * 8 * 8 > Map.Width * Map.Height)
            {
                bitRegion = new BitArray(Map.Width * Map.Height);

                foreach (Point point in Map.Selection)
                    bitRegion[point.Y * Map.Width + point.X] = true;
            }
            else
            {
                pointRegion = Map.Selection.ToArray();
            }

            MapRegion.BitRegion = bitRegion;
            MapRegion.PointRegion = pointRegion;

            MapRegion.Size = Map.Selection.Count;
        }

        private void CancelButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 取消更改：恢复 Selection 为 MapRegion 中原先保存的点集合
            // 说明（新手）：如果你误改了选择区域，点击取消会从 MapRegion 的保存数据中恢复选择状态。
            if (MapRegion == null) return;

            Map.Selection = MapRegion.GetPoints(Map.Width);

            Map.TextureValid = false;
        }

        private void BlockedOnlyButton_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // 切换仅显示具有某些属性（如阻挡）的单元格，用于快速选取带有指定属性的区域
            Map.AttributeSelection = !Map.AttributeSelection;
        }
    }


}


namespace Server.Views.DirectX
{
    // DXManager: 封装 DirectX 设备与渲染资源的生命周期管理
    // 说明（新手）：
    // - 负责创建和维护 Direct3D 设备、主渲染表面（MainSurface）、Sprite、Line 等对象。
    // - 提供纹理加载、设置渲染表面、透明度与混合模式的辅助函数。
    // - 当窗口大小或设备状态发生变化时需要 ResetDevice / AttemptReset / AttemptRecovery 来恢复设备。
    // - 大部分方法直接操作底层 DirectX 资源，修改时需小心资源释放顺序以避免内存或 GPU 资源泄露。
    public class DXManager : IDisposable
    {
        public Graphics Graphics;

        public readonly Control Target;

        public Dictionary<LibraryFile, MirLibrary> LibraryList = new Dictionary<LibraryFile, MirLibrary>();

        public PresentParameters Parameters { get; private set; }
        public Device Device { get; private set; }
        public Sprite Sprite { get; private set; }
        public Line Line { get; private set; }

        public Surface CurrentSurface { get; private set; }
        public Surface MainSurface { get; private set; }

        public float Opacity { get; private set; } = 1F;

        public bool Blending { get; private set; }
        public float BlendRate { get; private set; } = 1F;

        public bool DeviceLost { get; set; }

        public List<MirImage> TextureList { get; } = new List<MirImage>();

        public Texture AttributeTexture;

        public MapControl Map;

        public DXManager(Control target)
        {
            Target = target;

            Graphics = Graphics.FromHwnd(IntPtr.Zero);
            ConfigureGraphics(Graphics);


            foreach (KeyValuePair<LibraryFile, string> pair in Libraries.LibraryList)
            {
                if (!File.Exists(Path.Combine(Config.ClientPath, pair.Value))) continue;

                LibraryList[pair.Key] = new MirLibrary(Path.Combine(Config.ClientPath, pair.Value), this);
            }
        }

        public void Create()
        {
            // 创建 Direct3D 设备并加载资源（在窗体 OnLoad 时调用）
            // 说明（新手）：Device 的创建依赖窗口句柄和当前窗口大小，创建失败通常与驱动或权限有关。
            Parameters = new PresentParameters
            {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                BackBufferFormat = Format.X8R8G8B8,
                PresentationInterval = PresentInterval.Default,
                BackBufferWidth = Target.ClientSize.Width,
                BackBufferHeight = Target.ClientSize.Height,
                PresentFlags = PresentFlags.LockableBackBuffer,
            };

            Direct3D direct3D = new Direct3D();

            Device = new Device(direct3D, 0, DeviceType.Hardware, Target.Handle, CreateFlags.HardwareVertexProcessing, Parameters);

            LoadTextures();
        }

        private unsafe void LoadTextures()
        {
            // 初始化 Sprite/Line，以及为渲染准备主表面和属性纹理(AttributeTexture)
            // 说明（新手）：AttributeTexture 用于绘制选区/属性覆盖（如黄色/红色高亮），它以 32x48 大小初始化并填充默认像素。
            Sprite = new Sprite(Device);
            Line = new Line(Device) { Width = 1F };

            MainSurface = Device.GetBackBuffer(0, 0);
            CurrentSurface = MainSurface;
            Device.SetRenderTarget(0, MainSurface);

            AttributeTexture = new Texture(Device, 48, 32, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

            DataRectangle rect = AttributeTexture.LockRectangle(0, LockFlags.Discard);

            int* data = (int*)rect.DataPointer;

            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 48; x++)
                    data[y * 48 + x] = -1;

            AttributeTexture.UnlockRectangle(0);

        }
        private void CleanUp()
        {
            // 释放/清理 DirectX 资源（在 ResetDevice 或 Dispose 时调用）
            // 说明（新手）：释放顺序重要，先释放依赖对象（Sprite/Line/Surface/Texture），最后释放库中的纹理。
            if (Sprite != null)
            {
                if (!Sprite.IsDisposed)
                    Sprite.Dispose();

                Sprite = null;
            }

            if (Line != null)
            {
                if (!Line.IsDisposed)
                    Line.Dispose();

                Line = null;
            }

            if (CurrentSurface != null)
            {
                if (!CurrentSurface.IsDisposed)
                    CurrentSurface.Dispose();

                CurrentSurface = null;
            }

            if (AttributeTexture != null)
            {
                if (!AttributeTexture.IsDisposed)
                    AttributeTexture.Dispose();

                AttributeTexture = null;
            }


            Map?.DisposeTexture();

            for (int i = TextureList.Count - 1; i >= 0; i--)
                TextureList[i].DisposeTexture();
        }

        public void SetSurface(Surface surface)
        {
            if (CurrentSurface == surface) return;

            Sprite.Flush();
            CurrentSurface = surface;
            Device.SetRenderTarget(0, surface);
        }
        // 切换渲染目标表面：用于将绘制操作重定向到 ControlTexture（离屏渲染）或主表面
        public void SetOpacity(float opacity)
        {
            Device.SetSamplerState(0, SamplerState.MagFilter, 0);

            if (Opacity == opacity)
                return;

            Sprite.Flush();
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);

            if (opacity >= 1 || opacity < 0)
            {
                Device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
                Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.One);
                Device.SetRenderState(RenderState.BlendFactor, System.Drawing.Color.FromArgb(255, 255, 255, 255).ToArgb());
            }
            else
            {
                Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseBlendFactor);
                Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.SourceAlpha);
                Device.SetRenderState(RenderState.BlendFactor, System.Drawing.Color.FromArgb((byte)(255 * opacity), (byte)(255 * opacity),
                    (byte)(255 * opacity), (byte)(255 * opacity)).ToArgb());
            }

            Opacity = opacity;
            Sprite.Flush();
        }
        // 设置全局不透明度：影响后续 Sprite 绘制的透明度
        public void SetBlend(bool value, float rate = 1F)
        {
            if (value == Blending) return;

            Blending = value;
            BlendRate = 1F;
            Sprite.Flush();

            Sprite.End();
            if (Blending)
            {
                Sprite.Begin(SpriteFlags.DoNotSaveState);
                Device.SetRenderState(RenderState.AlphaBlendEnable, true);

                Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.One);
                Device.SetRenderState(RenderState.BlendFactor, System.Drawing.Color.FromArgb((byte)(255 * rate), (byte)(255 * rate), (byte)(255 * rate), (byte)(255 * rate)).ToArgb());
            }
            else
            {
                Sprite.Begin(SpriteFlags.AlphaBlend);
            }


            Device.SetRenderTarget(0, CurrentSurface);
        }
        // 开启/关闭混合模式，rate 控制混合因子（用于特殊效果，例如半透明叠加）
        public void SetColour(int colour)
        {
            Sprite.Flush();

            if (colour == 0)
            {
                Device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.Modulate);
                Device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Texture);
            }
            else
            {

                Device.SetTextureStageState(0, TextureStage.ColorOperation, TextureOperation.SelectArg1);
                Device.SetTextureStageState(0, TextureStage.ColorArg1, TextureArgument.Current);
            }

            Sprite.Flush();
        }

        public void ResetDevice()
        {
            CleanUp();

            DeviceLost = true;

            if (Target.ClientSize.Width == 0 || Target.ClientSize.Height == 0) return;

            PresentParameters parameters = Parameters;

            if (parameters.BackBufferWidth == 0 || parameters.BackBufferHeight == 0)
                return;

            parameters.BackBufferWidth = Target.ClientSize.Width;
            parameters.BackBufferHeight = Target.ClientSize.Height;

            Device.Reset(parameters);
            Parameters = parameters;
            LoadTextures();
        }
        // ResetDevice: 在设备需重建时调用（例如窗口大小改变或设备被重置）
        public void AttemptReset()
        {
            try
            {
                Result result = Device.TestCooperativeLevel();

                if (result.Code == D3DResultCode.DeviceLost.Code) return;

                if (result.Code == D3DResultCode.DeviceNotReset.Code)
                {
                    ResetDevice();
                    return;
                }

                if (result.Code != D3DResultCode.Success.Code) return;

                DeviceLost = false;
            }
            catch (Exception ex)
            {
                SEnvir.SaveError(ex.ToString());
            }
        }
        // AttemptReset / AttemptRecovery: 处理设备丢失情况的恢复逻辑（常见于显卡驱动切换、睡眠等）
        public void AttemptRecovery()
        {
            try
            {
                Sprite.End();
            }
            catch
            {
            }

            try
            {
                Device.EndScene();
            }
            catch
            {
            }

            try
            {
                MainSurface = Device.GetBackBuffer(0, 0);
                CurrentSurface = MainSurface;
                Device.SetRenderTarget(0, MainSurface);
            }
            catch
            {
            }
        }

        public static void ConfigureGraphics(Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            graphics.TextContrast = 0;
        }

        // ConfigureGraphics: 为 System.Drawing.Graphics 设置高质量渲染参数（用于非 DirectX 的文本/调试绘制）

        #region IDisposable Support

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;

                Parameters = default;
                if (Sprite != null)
                {
                    if (!Sprite.IsDisposed)
                        Sprite.Dispose();

                    Sprite = null;
                }

                if (Line != null)
                {
                    if (!Line.IsDisposed)
                        Line.Dispose();

                    Line = null;
                }

                if (CurrentSurface != null)
                {
                    if (!CurrentSurface.IsDisposed)
                        CurrentSurface.Dispose();

                    CurrentSurface = null;
                }

                if (MainSurface != null)
                {
                    if (!MainSurface.IsDisposed)
                        MainSurface.Dispose();

                    MainSurface = null;
                }

                if (Device != null)
                {
                    if (!Device.IsDisposed)
                        Device.Dispose();

                    Device = null;
                }
                if (AttributeTexture != null)
                {
                    if (!AttributeTexture.IsDisposed)
                        AttributeTexture.Dispose();

                    AttributeTexture = null;
                }

                Map?.DisposeTexture();

                if (Graphics != null)
                {
                    Graphics.Dispose();
                    Graphics = null;
                }

                foreach (KeyValuePair<LibraryFile, MirLibrary> library in LibraryList)
                    library.Value.Dispose();

                Opacity = 0;
                Blending = false;
                BlendRate = 0;
                DeviceLost = false;


            }

        }

        ~DXManager()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

    public sealed class MirLibrary : IDisposable
    {
        // MirLibrary: 代表一个资源库文件（.mir 或类似格式），延迟读取并为图像提供 MirImage 实例
        // 说明（新手）：
        // - ReadLibrary() 会解析二进制资源并构造 MirImage 数组；读取是延迟的（首次使用时加载），通过 CheckImage 保证加载完成。
        // - CreateImage/CreateShadow/CreateOverlay 方法会把原始二进制数据转换成 DirectX 的 Texture 并缓存，避免每帧重复创建。
        // - 使用后请调用 Dispose/DisposeTexture 来释放 Texture 以避免显存泄露。
        public readonly object LoadLocker = new object();

        public string FileName;

        private FileStream _FStream;
        private BinaryReader _BReader;

        public bool Loaded, Loading;

        public MirImage[] Images;

        public readonly DXManager Manager;

        public MirLibrary(string fileName, DXManager manager)
        {
            _FStream = File.OpenRead(fileName);
            _BReader = new BinaryReader(_FStream);

            Manager = manager;
        }
        public void ReadLibrary()
        {
            lock (LoadLocker)
            {
                if (Loading) return;
                Loading = true;
            }

            if (_BReader == null)
            {
                Loaded = true;
                return;
            }

            // 读取嵌入的库数据流并解析 MirImage 条目
            // 实现细节（新手注意）：
            // - 第一部分读取一个长度（int），然后把该长度的数据拷贝到 MemoryStream 中作为库的实际内容。
            // - 接着读取一个 header int：低 25 位包含条目数量（count），高位包含版本信息（version）。
            // - 根据 version 决定解析方式（不同版本可能改变了数据布局），这里只用 version==0 做兼容处理。
            // - 循环检查每个条目，先读取一个布尔值表示该索引是否存在，存在则用 MirImage 解析并存入数组。
            using (MemoryStream mstream = new MemoryStream(_BReader.ReadBytes(_BReader.ReadInt32())))
            using (BinaryReader reader = new BinaryReader(mstream))
            {
                int value = reader.ReadInt32();

                int count = value & 0x1FFFFFF;
                var version = (value >> 25) & 0x7F;

                if (version == 0)
                {
                    // 旧版处理：整个 value 即为 count
                    count = value;
                }

                Images = new MirImage[count];

                for (int i = 0; i < Images.Length; i++)
                {
                    // 每个条目以一个布尔标记开始，标记为 false 则表示该索引为空（跳过）
                    if (!reader.ReadBoolean()) continue;

                    // 构造 MirImage 时会读取该条目的元数据位置与尺寸信息，但不会立即创建 GPU 纹理（延迟加载）
                    Images[i] = new MirImage(reader, Manager, version);
                }
            }


            Loaded = true;
        }


        public Size GetSize(int index)
        {
            if (!CheckImage(index)) return Size.Empty;

            return new Size(Images[index].Width, Images[index].Height);
        }
        public Point GetOffSet(int index)
        {
            if (!CheckImage(index)) return Point.Empty;

            return new Point(Images[index].OffSetX, Images[index].OffSetY);
        }
        public MirImage GetImage(int index)
        {
            if (!CheckImage(index)) return null;

            return Images[index];
        }
        public MirImage CreateImage(int index, ImageType type)
        {
            if (!CheckImage(index)) return null;

            MirImage image = Images[index];

            Texture texture;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    break;
                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;
                    break;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    break;
                default:
                    return null;
            }

            if (texture == null) return null;

            return image;
        }

        private bool CheckImage(int index)
        {
            if (!Loaded) ReadLibrary();

            while (!Loaded)
                Thread.Sleep(1);

            return index >= 0 && index < Images.Length && Images[index] != null;
        }

        public bool VisiblePixel(int index, Point location, bool accurate = true, bool offSet = false)
        {
            if (!CheckImage(index)) return false;

            MirImage image = Images[index];

            if (offSet)
                location = new Point(location.X - image.OffSetX, location.Y - image.OffSetY);

            return image.VisiblePixel(location, accurate);
        }

        public void Draw(int index, float x, float y, Color colour, Rectangle area, float opacity, ImageType type, byte shadow = 0)
        {
            Draw(index, x, y, colour.ToColor4(), area, opacity, type, shadow);
        }

        public void Draw(int index, float x, float y, Color4 colour, Rectangle area, float opacity, ImageType type, byte shadow = 0)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];

            Texture texture;

            float oldOpacity = Manager.Opacity;
            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    break;
                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;

                    if (texture == null)
                    {
                        if (!image.ImageValid) image.CreateImage(_BReader);
                        texture = image.Image;

                        switch (image.ShadowType)
                        {
                            case 177:
                            case 176:
                            case 49:
                                Matrix m = Matrix.Scaling(1F, 0.5f, 0);

                                m.M21 = -0.50F;
                                Manager.Sprite.Transform = m * Matrix.Translation(x + image.Height / 2, y, 0);

                                Manager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
                                if (oldOpacity != 0.5F) Manager.SetOpacity(0.5F);

                                Manager.Sprite.Draw(texture, Vector3.Zero, Vector3.Zero, Color.Black);

                                Manager.SetOpacity(oldOpacity);
                                Manager.Sprite.Transform = Matrix.Identity;
                                Manager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

                                image.ExpireTime = SEnvir.Now.AddMinutes(10);
                                break;
                            case 50:
                                if (oldOpacity != 0.5F) Manager.SetOpacity(0.5F);

                                Manager.Sprite.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), Color.Black);

                                Manager.SetOpacity(oldOpacity);

                                image.ExpireTime = SEnvir.Now.AddMinutes(10);
                                break;
                        }



                        return;
                    }
                    break;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;
                    break;
                default:
                    return;
            }

            if (texture == null) return;

            Manager.SetOpacity(opacity);

            Manager.Sprite.Draw(texture, area, Vector3.Zero, new Vector3(x, y, 0), colour);

            Manager.SetOpacity(oldOpacity);

            image.ExpireTime = SEnvir.Now.AddMinutes(10);
        }
        public void Draw(int index, float x, float y, Color colour, bool useOffSet, float opacity, ImageType type)
        {
            Draw(index, x, y, colour.ToColor4(), useOffSet, opacity, type);
        }

        public void Draw(int index, float x, float y, Color4 colour, bool useOffSet, float opacity, ImageType type)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];

            Texture texture;

            float oldOpacity = Manager.Opacity;
            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                case ImageType.Shadow:
                    if (!image.ShadowValid) image.CreateShadow(_BReader);
                    texture = image.Shadow;

                    if (useOffSet)
                    {
                        x += image.ShadowOffSetX;
                        y += image.ShadowOffSetY;
                    }


                    if (texture == null)
                    {
                        if (!image.ImageValid) image.CreateImage(_BReader);
                        texture = image.Image;

                        switch (image.ShadowType)
                        {
                            case 177:
                            case 176:
                            case 49:
                                Matrix m = Matrix.Scaling(1F, 0.5f, 0);

                                m.M21 = -0.50F;
                                Manager.Sprite.Transform = m * Matrix.Translation(x + image.Height / 2, y, 0);

                                Manager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
                                if (oldOpacity != 0.5F) Manager.SetOpacity(0.5F);

                                Manager.Sprite.Draw(texture, Vector3.Zero, Vector3.Zero, Color.Black);

                                Manager.SetOpacity(oldOpacity);
                                Manager.Sprite.Transform = Matrix.Identity;
                                Manager.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);

                                image.ExpireTime = SEnvir.Now.AddMinutes(10);
                                break;
                            case 50:
                                if (oldOpacity != 0.5F) Manager.SetOpacity(0.5F);

                                Manager.Sprite.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), Color.Black);

                                Manager.SetOpacity(oldOpacity);

                                image.ExpireTime = SEnvir.Now.AddMinutes(10);
                                break;
                        }



                        return;
                    }

                    break;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                default:
                    return;
            }

            if (texture == null) return;

            Manager.SetOpacity(opacity);

            Manager.Sprite.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), colour);

            Manager.SetOpacity(oldOpacity);

            image.ExpireTime = SEnvir.Now.AddMinutes(10);
        }
        public void DrawBlend(int index, float x, float y, Color colour, bool useOffSet, float rate, ImageType type, byte shadow = 0)
        {
            DrawBlend(index, x, y, colour.ToColor4(), useOffSet, rate, type, shadow);
        }

        public void DrawBlend(int index, float x, float y, Color4 colour, bool useOffSet, float rate, ImageType type, byte shadow = 0)
        {
            if (!CheckImage(index)) return;

            MirImage image = Images[index];

            Texture texture;

            switch (type)
            {
                case ImageType.Image:
                    if (!image.ImageValid) image.CreateImage(_BReader);
                    texture = image.Image;
                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                case ImageType.Shadow:
                    return;
                case ImageType.Overlay:
                    if (!image.OverlayValid) image.CreateOverlay(_BReader);
                    texture = image.Overlay;

                    if (useOffSet)
                    {
                        x += image.OffSetX;
                        y += image.OffSetY;
                    }
                    break;
                default:
                    return;
            }
            if (texture == null) return;


            bool oldBlend = Manager.Blending;
            float oldRate = Manager.BlendRate;

            Manager.SetBlend(true, rate);

            Manager.Sprite.Draw(texture, Vector3.Zero, new Vector3(x, y, 0), colour);

            Manager.SetBlend(oldBlend, oldRate);

            image.ExpireTime = SEnvir.Now.AddMinutes(10);
        }


        #region IDisposable Support

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;

                if (Images != null)
                {
                    foreach (MirImage image in Images)
                        image?.Dispose();
                }


                Images = null;


                _FStream?.Dispose();
                _FStream = null;

                _BReader?.Dispose();
                _BReader = null;

                Loading = false;
                Loaded = false;
            }

        }

        ~MirLibrary()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

    public sealed class MirImage : IDisposable
    {
        // MirImage: 表示库中单个图片条目，负责持有 Image/Shadow/Overlay 的二进制数据和对应的 DirectX Texture
        // 说明（新手）：
        // - ImageData/ShadowData/OverlayData 保存原始像素/压缩数据，CreateImage 等方法会将其写入到 DirectX Texture。
        // - VisiblePixel 用于像素级点击检测（判断某个像素是否透明），供精确选取/碰撞判断使用。
        // - ExpireTime 用于缓存失效策略，Manager.TextureList 管理活跃纹理，确保定期回收不再使用的纹理。
        public int Version;
        public int Position;

        public DXManager Manager;

        #region Texture

        public short Width;
        public short Height;
        public short OffSetX;
        public short OffSetY;
        public byte ShadowType;
        public Texture Image;
        public bool ImageValid { get; private set; }
        public unsafe byte* ImageData;
        public int ImageDataSize
        {
            get
            {
                int w = Width + (4 - Width % 4) % 4;
                int h = Height + (4 - Height % 4) % 4;

                if (Version > 0)
                {
                    return w * h;
                }
                else
                {
                    return w * h / 2;
                }
            }
        }
        #endregion

        #region Shadow
        public short ShadowWidth;
        public short ShadowHeight;

        public short ShadowOffSetX;
        public short ShadowOffSetY;

        public Texture Shadow;
        public bool ShadowValid { get; private set; }
        public unsafe byte* ShadowData;
        public int ShadowDataSize
        {
            get
            {
                int w = ShadowWidth + (4 - ShadowWidth % 4) % 4;
                int h = ShadowHeight + (4 - ShadowHeight % 4) % 4;

                if (Version > 0)
                {
                    return w * h;
                }
                else
                {
                    return w * h / 2;
                }
            }
        }
        #endregion

        #region Overlay
        public short OverlayWidth;
        public short OverlayHeight;

        public Texture Overlay;
        public bool OverlayValid { get; private set; }
        public unsafe byte* OverlayData;
        public int OverlayDataSize
        {
            get
            {
                int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
                int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;

                if (Version > 0)
                {
                    return w * h;
                }
                else
                {
                    return w * h / 2;
                }
            }
        }
        #endregion

        private Format DrawFormat
        {
            get
            {
                return Version switch
                {
                    0 => Format.Dxt1,
                    _ => Format.Dxt5,
                };
            }
        }

        public DateTime ExpireTime;

        public MirImage(BinaryReader reader, DXManager manager, int version)
        {
            Version = version;
            Position = reader.ReadInt32();

            Width = reader.ReadInt16();
            Height = reader.ReadInt16();
            OffSetX = reader.ReadInt16();
            OffSetY = reader.ReadInt16();

            ShadowType = reader.ReadByte();
            ShadowWidth = reader.ReadInt16();
            ShadowHeight = reader.ReadInt16();
            ShadowOffSetX = reader.ReadInt16();
            ShadowOffSetY = reader.ReadInt16();

            OverlayWidth = reader.ReadInt16();
            OverlayHeight = reader.ReadInt16();

            Manager = manager;
        }

        public unsafe bool VisiblePixel(Point p, bool acurrate)
        {
            // VisiblePixel: 像素级可见性检测
            // 说明（新手）：
            // - 图片数据在二进制里以 4x4 的块为单位压缩/存放（每个块 8 字节），所以需要先根据坐标找到对应块。
            // - col0/col1 存放该块是否存在不透明像素的概览信息，若两者均为 0 则该块完全透明，可快速返回 false。
            // - 当 `accurate` 为 false 时，函数可以使用块级概览（col1 < col0）来粗略判断是否可见，从而加快检测。
            // - 精确检测会进一步检查具体像素位（通过位运算从 ImageData 的后续字节获取具体像素掩码）。
            if (p.X < 0 || p.Y < 0 || !ImageValid || ImageData == null) return false;

            int w = Width + (4 - Width % 4) % 4;
            int h = Height + (4 - Height % 4) % 4;

            if (p.X >= w || p.Y >= h)
                return false;

            // 计算包含该像素的 4x4 块在数据中的块索引
            int x = (p.X - p.X % 4) / 4;
            int y = (p.Y - p.Y % 4) / 4;
            int index = (y * (w / 4) + x) * 8;

            // 前两个 short 值（合并为 int）是对该块的快速掩码统计
            int col0 = ImageData[index + 1] << 8 | ImageData[index], col1 = ImageData[index + 3] << 8 | ImageData[index + 2];

            // 如果概览信息显示完全透明，则直接返回 false
            if (col0 == 0 && col1 == 0) return false;

            // 非精确模式：可通过 col1 < col0 的简单比较来判断是否可能有可见像素
            if (!acurrate || col1 < col0) return true;

            // 精确模式：检查具体像素位
            x = p.X % 4;
            y = p.Y % 4;
            x *= 2;

            // 通过按位与检查两位掩码，若任一位不是 1（表示可见），则返回 true
            return (ImageData[index + 4 + y] & 1 << x) >> x != 1 || (ImageData[index + 4 + y] & 1 << x + 1) >> x + 1 != 1;
        }


        public unsafe void DisposeTexture()
        {
            if (Image != null && !Image.IsDisposed)
                Image.Dispose();

            if (Shadow != null && !Shadow.IsDisposed)
                Shadow.Dispose();

            if (Overlay != null && !Overlay.IsDisposed)
                Overlay.Dispose();

            ImageData = null;
            ShadowData = null;
            OverlayData = null;

            Image = null;
            Shadow = null;
            Overlay = null;

            ImageValid = false;
            ShadowValid = false;
            OverlayValid = false;

            ExpireTime = DateTime.MinValue;

            Manager.TextureList.Remove(this);
        }

        public unsafe void CreateImage(BinaryReader reader)
        {
            // CreateImage: 从二进制流创建 DirectX Texture 并把原始像素数据写入 GPU 纹理
            // 步骤说明（新手）：
            // 1) 根据 Width/Height 计算对齐后的像素宽高（通常向上对齐到 4 的倍数），以满足存储布局。
            // 2) 创建一个 Texture 并 LockRectangle 获取 GPU 内存的指针（DataRectangle）。
            // 3) 在 reader 上 Seek 到本条目真实数据位置（Position），读取 ImageDataSize 字节并直接写入 GPU 指针。
            // 4) UnlockRectangle 完成写入，标记 ImageValid 并把该 MirImage 加入 Manager.TextureList 以便管理生命周期。
            if (Position == 0) return;

            int w = Width + (4 - Width % 4) % 4;
            int h = Height + (4 - Height % 4) % 4;

            if (w == 0 || h == 0) return;

            Image = new Texture(Manager.Device, w, h, 1, Usage.None, DrawFormat, Pool.Managed);
            DataRectangle rect = Image.LockRectangle(0, LockFlags.Discard);
            ImageData = (byte*)rect.DataPointer;

            lock (reader)
            {
                // 注意：这里对同一个 BinaryReader 加锁，保证多线程加载纹理时不会并发读写同一流
                reader.BaseStream.Seek(Position, SeekOrigin.Begin);
                byte[] buffer = reader.ReadBytes(ImageDataSize);
                SharpDX.Utilities.Write(rect.DataPointer, buffer, 0, buffer.Length);
            }

            Image.UnlockRectangle(0);

            ImageValid = true;
            ExpireTime = SEnvir.Now.AddMinutes(30);
            Manager.TextureList.Add(this);
        }
        public unsafe void CreateShadow(BinaryReader reader)
        {
            // CreateShadow: shadow 数据通常紧跟在 image 数据之后，位置为 Position + ImageDataSize
            // 说明（新手）：
            // - 如果 Image 尚未创建，会先创建 Image，确保原始像素数据已被读入。
            // - Shadow 大小计算同样需要做 4 的对齐；读取位置是 Position + ImageDataSize。
            // - 创建完毕后将 Shadow 标记为有效（ShadowValid = true）。
            if (Position == 0) return;

            if (!ImageValid)
                CreateImage(reader);

            int w = ShadowWidth + (4 - ShadowWidth % 4) % 4;
            int h = ShadowHeight + (4 - ShadowHeight % 4) % 4;

            if (w == 0 || h == 0) return;

            Shadow = new Texture(Manager.Device, w, h, 1, Usage.None, DrawFormat, Pool.Managed);
            DataRectangle rect = Shadow.LockRectangle(0, LockFlags.Discard);
            ShadowData = (byte*)rect.DataPointer;

            lock (reader)
            {
                reader.BaseStream.Seek(Position + ImageDataSize, SeekOrigin.Begin);
                byte[] buffer = reader.ReadBytes(ShadowDataSize);
                SharpDX.Utilities.Write(rect.DataPointer, buffer, 0, buffer.Length);
            }

            Shadow.UnlockRectangle(0);

            ShadowValid = true;
        }
        public unsafe void CreateOverlay(BinaryReader reader)
        {
            // CreateOverlay: overlay 紧跟在 image + shadow 数据之后，位置为 Position + ImageDataSize + ShadowDataSize
            // 说明（新手）：Overlay 常用于绘制额外特效或覆盖层，创建流程与 CreateImage/CreateShadow 类似。
            if (Position == 0) return;

            if (!ImageValid)
                CreateImage(reader);

            int w = OverlayWidth + (4 - OverlayWidth % 4) % 4;
            int h = OverlayHeight + (4 - OverlayHeight % 4) % 4;

            if (w == 0 || h == 0) return;

            Overlay = new Texture(Manager.Device, w, h, 1, Usage.None, DrawFormat, Pool.Managed);
            DataRectangle rect = Overlay.LockRectangle(0, LockFlags.Discard);
            OverlayData = (byte*)rect.DataPointer;

            lock (reader)
            {
                reader.BaseStream.Seek(Position + ImageDataSize + ShadowDataSize, SeekOrigin.Begin);
                byte[] buffer = reader.ReadBytes(OverlayDataSize);
                SharpDX.Utilities.Write(rect.DataPointer, buffer, 0, buffer.Length);
            }

            Overlay.UnlockRectangle(0);

            OverlayValid = true;
        }


        #region IDisposable Support

        public bool IsDisposed { get; private set; }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;

                Position = 0;

                Width = 0;
                Height = 0;
                OffSetX = 0;
                OffSetY = 0;

                ShadowWidth = 0;
                ShadowHeight = 0;
                ShadowOffSetX = 0;
                ShadowOffSetY = 0;

                OverlayWidth = 0;
                OverlayHeight = 0;
            }

        }

        public void Dispose()
        {
            Dispose(!IsDisposed);
            GC.SuppressFinalize(this);
        }
        ~MirImage()
        {
            Dispose(false);
        }

        #endregion

    }

    public enum ImageType
    {
        Image,
        Shadow,
        Overlay,
    }


    public class MapControl : IDisposable
    {
        public DXManager Manager;

        // MapControl: 负责地图的离屏渲染、格子绘制、选择逻辑与鼠标交互
        // 说明（新手）：
        // - MapControl 使用 DXManager 提供的 Device/Sprite/Line 来绘制地图到一个离屏 `ControlTexture`，
        //   然后由 MapViewer 将该纹理绘制到窗口上。这种模式可以避免每帧都重绘所有格子。
        // - 重要职责包括：CreateTexture/DisposeTexture（管理离屏纹理），DrawFloor（按层次绘制地图格子），
        //   Load（从 .map 文件解析地图数据），以及处理鼠标交互（MouseDown/MouseMove/MouseUp）。
        public MapControl(DXManager manager)
        {
            Manager = manager;
            Zoom = 1;
        }

        #region Size

        public Size Size
        {
            get { return _Size; }
            set
            {
                if (_Size == value) return;

                Size oldValue = _Size;
                _Size = value;

                OnSizeChanged(oldValue, value);
            }
        }
        private Size _Size;
        public event EventHandler<EventArgs> SizeChanged;
        public virtual void OnSizeChanged(Size oValue, Size nValue)
        {
            TextureValid = false;

            SizeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public Cell[,] Cells;
        public int Width, Height;

        #region StartX

        public int StartX
        {
            get { return _StartX; }
            set
            {
                if (_StartX == value) return;

                int oldValue = _StartX;
                _StartX = value;

                OnStartXChanged(oldValue, value);
            }
        }
        private int _StartX;
        public event EventHandler<EventArgs> StartXChanged;
        public virtual void OnStartXChanged(int oValue, int nValue)
        {
            StartXChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion

        #region StartY

        public int StartY
        {
            get { return _StartY; }
            set
            {
                if (_StartY == value) return;

                int oldValue = _StartY;
                _StartY = value;

                OnStartYChanged(oldValue, value);
            }
        }
        private int _StartY;
        public event EventHandler<EventArgs> StartYChanged;
        public virtual void OnStartYChanged(int oValue, int nValue)
        {
            StartYChanged?.Invoke(this, EventArgs.Empty);

            TextureValid = false;
        }

        #endregion

        #region DrawAttributes

        public bool DrawAttributes
        {
            get { return _DrawAttributes; }
            set
            {
                if (_DrawAttributes == value) return;

                bool oldValue = _DrawAttributes;
                _DrawAttributes = value;

                OnDrawAttributesChanged(oldValue, value);
            }
        }
        private bool _DrawAttributes;
        public event EventHandler<EventArgs> DrawAttributesChanged;
        public virtual void OnDrawAttributesChanged(bool oValue, bool nValue)
        {
            DrawAttributesChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion

        #region DrawSelection

        public bool DrawSelection
        {
            get { return _DrawSelection; }
            set
            {
                if (_DrawSelection == value) return;

                bool oldValue = _DrawSelection;
                _DrawSelection = value;

                OnDrawSelectionChanged(oldValue, value);
            }
        }
        private bool _DrawSelection;
        public event EventHandler<EventArgs> DrawSelectionChanged;
        public virtual void OnDrawSelectionChanged(bool oValue, bool nValue)
        {
            DrawSelectionChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion


        #region AttributeSelection

        public bool AttributeSelection
        {
            get { return _AttributeSelection; }
            set
            {
                if (_AttributeSelection == value) return;

                bool oldValue = _AttributeSelection;
                _AttributeSelection = value;

                OnAttributeSelectionChanged(oldValue, value);
            }
        }
        private bool _AttributeSelection;
        public event EventHandler<EventArgs> AttributeSelectionChanged;
        public virtual void OnAttributeSelectionChanged(bool oValue, bool nValue)
        {
            AttributeSelectionChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion

        public HashSet<Point> Selection = new HashSet<Point>();


        //Zoom to handle
        public const int BaseCellWidth = 48;
        public const int BaseCellHeight = 32;

        public float CellWidth => BaseCellWidth * Zoom;
        public float CellHeight => BaseCellHeight * Zoom;


        #region Zoom

        public float Zoom
        {
            get { return _Zoom; }
            set
            {
                if (_Zoom == value) return;

                float oldValue = _Zoom;
                _Zoom = value;

                OnZoomChanged(oldValue, value);
            }
        }
        private float _Zoom;
        public event EventHandler<EventArgs> ZoomChanged;
        public virtual void OnZoomChanged(float oValue, float nValue)
        {
            ZoomChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion

        #region Animation

        public int Animation
        {
            get { return _Animation; }
            set
            {
                if (_Animation == value) return;

                int oldValue = _Animation;
                _Animation = value;

                OnAnimationChanged(oldValue, value);
            }
        }
        private int _Animation;
        public event EventHandler<EventArgs> AnimationChanged;
        public virtual void OnAnimationChanged(int oValue, int nValue)
        {
            AnimationChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion

        #region Border

        public bool Border
        {
            get { return _Border; }
            set
            {
                if (_Border == value) return;

                bool oldValue = _Border;
                _Border = value;

                OnBorderChanged(oldValue, value);
            }
        }
        private bool _Border;
        public event EventHandler<EventArgs> BorderChanged;
        public virtual void OnBorderChanged(bool oValue, bool nValue)
        {
            BorderChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion


        #region MouseLocation

        public Point MouseLocation
        {
            get { return _MouseLocation; }
            set
            {
                if (_MouseLocation == value) return;

                Point oldValue = _MouseLocation;
                _MouseLocation = value;

                OnMouseLocationChanged(oldValue, value);
            }
        }
        private Point _MouseLocation;
        public event EventHandler<EventArgs> MouseLocationChanged;
        public virtual void OnMouseLocationChanged(Point oValue, Point nValue)
        {
            MouseLocationChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion

        #region Radius

        public int Radius
        {
            get { return _Radius; }
            set
            {
                if (_Radius == value) return;

                int oldValue = _Radius;
                _Radius = value;

                OnRadiusChanged(oldValue, value);
            }
        }
        private int _Radius;
        public event EventHandler<EventArgs> RadiusChanged;
        public virtual void OnRadiusChanged(int oValue, int nValue)
        {
            RadiusChanged?.Invoke(this, EventArgs.Empty);
            TextureValid = false;
        }

        #endregion




        #region Texture
        public bool TextureValid { get; set; }
        public Texture ControlTexture { get; set; }
        public Size TextureSize { get; set; }
        public Surface ControlSurface { get; set; }
        public DateTime ExpireTime { get; protected set; }

        protected virtual void CreateTexture()
        {
            // CreateTexture: 创建或更新离屏渲染纹理（ControlTexture）并把绘制操作重定向到该纹理
            // 说明（新手）：
            // - 如果画布尺寸改变（Size != TextureSize）会重新创建纹理并清空旧纹理。
            // - 在创建完成后会调用 OnClearTexture 来实际绘制地图的底图（DrawFloor 等）。
            if (ControlTexture == null || Size != TextureSize)
            {
                DisposeTexture();
                TextureSize = Size;
                ControlTexture = new Texture(Manager.Device, TextureSize.Width, TextureSize.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default); ;
                ControlSurface = ControlTexture.GetSurfaceLevel(0);
                Manager.Map = this;
            }

            Surface previous = Manager.CurrentSurface;
            Manager.SetSurface(ControlSurface);

            Manager.Device.Clear(ClearFlags.Target, Color.Black, 0, 0);

            OnClearTexture();

            Manager.SetSurface(previous);
            TextureValid = true;
        }
        protected virtual void OnClearTexture()
        {
            // OnClearTexture: 在新的离屏纹理上清理并绘制地图底层
            // 说明（新手）：默认实现调用 DrawFloor() 绘制地面与图块；这里也可以扩展绘制对象层/放置点等。
            DrawFloor();

            //DrawObjects();

            //DrawPlacements();
        }
        public virtual void DisposeTexture()
        {
            if (ControlTexture != null)
            {
                if (!ControlTexture.IsDisposed)
                    ControlTexture.Dispose();

                ControlTexture = null;
            }

            if (ControlSurface != null)
            {
                if (!ControlSurface.IsDisposed)
                    ControlSurface.Dispose();

                ControlSurface = null;
            }

            TextureSize = Size.Empty;
            ExpireTime = DateTime.MinValue;
            TextureValid = false;

            if (Manager.Map == this)
                Manager.Map = null;
        }

        #endregion


        public void Draw()
        {
            if (Size.Width <= 0 || Size.Height <= 0) return;

            DrawControl();
        }
        protected virtual void DrawControl()
        {
            if (!TextureValid)
            {
                CreateTexture();

                if (!TextureValid) return;
            }

            float oldOpacity = Manager.Opacity;

            Manager.SetOpacity(1F);

            Manager.Sprite.Draw(ControlTexture, Vector3.Zero, Vector3.Zero, Color.White);

            Manager.SetOpacity(oldOpacity);
        }

        public void DrawFloor()
        {
            // DrawFloor: 分层绘制地图格子（底层、中层、前景），并处理动画帧与混合绘制
            // 说明（新手）：
            // - 该方法按照行列遍历当前可见区域（由 StartX/StartY 与 Size 决定），通过 MirLibrary 绘制对应的图像索引。
            // - 绘制顺序遵循后底-中-前的顺序，处理不同尺寸（1x / 2x）以及动画与混合（Blend）场景。
            // - 选择/属性高亮使用 `Manager.AttributeTexture` 绘制叠加颜色（如黄色/红色）。
            int minX = Math.Max(0, StartX - 1);
            int maxX = Math.Min(Width - 1, StartX + (int)Math.Ceiling(Size.Width / CellWidth));

            int minY = Math.Max(0, StartY - 1);
            int maxY = Math.Min(Height - 1, StartY + (int)Math.Ceiling(Size.Height / CellHeight));

            Matrix scale = Matrix.Scaling(Zoom, Zoom, 1);

            for (int y = minY; y <= maxY; y++)
            {
                if (y % 2 != 0) continue;

                float drawY = (y - StartY) * BaseCellHeight;

                for (int x = minX; x <= maxX; x++)
                {
                    if (x % 2 != 0) continue;

                    float drawX = (x - StartX) * BaseCellWidth;

                    Cell tile = Cells[x, y];

                    MirLibrary library;
                    LibraryFile file;

                    if (!Libraries.KROrder.TryGetValue(tile.BackFile, out file)) continue;

                    if (!Manager.LibraryList.TryGetValue(file, out library)) continue;

                    Manager.Sprite.Transform = Matrix.Multiply(Matrix.Translation(drawX, drawY, 0), scale);

                    library.Draw(tile.BackImage, 0, 0, Color.White, false, 1F, ImageType.Image);
                }
            }

            for (int y = minY; y <= maxY; y++)
            {
                float drawY = (y - StartY + 1) * BaseCellHeight;

                for (int x = minX; x <= maxX; x++)
                {
                    float drawX = (x - StartX) * BaseCellWidth;

                    Cell cell = Cells[x, y];

                    MirLibrary library;
                    LibraryFile file;

                    if (Libraries.KROrder.TryGetValue(cell.MiddleFile, out file) && file != LibraryFile.Tilesc && Manager.LibraryList.TryGetValue(file, out library))
                    {
                        int index = cell.MiddleImage - 1;

                        if (cell.MiddleAnimationFrame > 1 && cell.MiddleAnimationFrame < 255)
                            continue; //   index += GameScene.Game.MapControl.Animation % cell.MiddleAnimationFrame;

                        Size s = library.GetSize(index);

                        if ((s.Width == CellWidth && s.Height == CellHeight) || (s.Width == CellWidth * 2 && s.Height == CellHeight * 2))
                        {
                            Manager.Sprite.Transform = Matrix.Multiply(Matrix.Translation(drawX, drawY - BaseCellHeight, 0), scale);

                            library.Draw(index, 0, 0, Color.White, false, 1F, ImageType.Image);
                        }
                    }


                    if (Libraries.KROrder.TryGetValue(cell.FrontFile, out file) && file != LibraryFile.Tilesc && Manager.LibraryList.TryGetValue(file, out library))
                    {
                        int index = cell.FrontImage - 1;

                        if (cell.FrontAnimationFrame > 1 && cell.FrontAnimationFrame < 255)
                            continue; //  index += GameScene.Game.MapControl.Animation % cell.FrontAnimationFrame;

                        Size s = library.GetSize(index);

                        if ((s.Width == CellWidth && s.Height == CellHeight) || (s.Width == CellWidth * 2 && s.Height == CellHeight * 2))
                        {
                            Manager.Sprite.Transform = Matrix.Multiply(Matrix.Translation(drawX, drawY - BaseCellHeight, 0), scale);

                            library.Draw(index, 0, 0, Color.White, false, 1F, ImageType.Image);
                        }
                    }
                }
            }

            maxY = Math.Min(Height - 1, StartY + 20 + (int)Math.Ceiling(Size.Height / CellHeight));
            for (int y = minY; y <= maxY; y++)
            {
                float drawY = (y - StartY + 1) * BaseCellHeight;

                for (int x = minX; x <= maxX; x++)
                {
                    float drawX = (x - StartX) * BaseCellWidth;

                    Cell cell = Cells[x, y];

                    MirLibrary library;
                    LibraryFile file;

                    if (Libraries.KROrder.TryGetValue(cell.MiddleFile, out file) && file != LibraryFile.Tilesc && Manager.LibraryList.TryGetValue(file, out library))
                    {
                        int index = cell.MiddleImage - 1;

                        bool blend = false;
                        if (cell.MiddleAnimationFrame > 1 && cell.MiddleAnimationFrame < 255)
                        {
                            index += Animation % (cell.MiddleAnimationFrame & 0x4F);
                            blend = (cell.MiddleAnimationFrame & 0x50) > 0;
                        }

                        Size s = library.GetSize(index);

                        if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != CellWidth * 2 || s.Height != CellHeight * 2))
                        {
                            Manager.Sprite.Transform = Matrix.Multiply(Matrix.Translation(drawX, drawY - s.Height, 0), scale);

                            if (!blend)
                                library.Draw(index, 0, 0, Color.White, false, 1F, ImageType.Image);
                            else
                                library.DrawBlend(index, 0, 0, Color.White, false, 0.5F, ImageType.Image);
                        }
                    }


                    if (Libraries.KROrder.TryGetValue(cell.FrontFile, out file) && file != LibraryFile.Tilesc && Manager.LibraryList.TryGetValue(file, out library))
                    {
                        int index = cell.FrontImage - 1;

                        bool blend = false;
                        if (cell.FrontAnimationFrame > 1 && cell.FrontAnimationFrame < 255)
                        {
                            index += Animation % (cell.FrontAnimationFrame & 0x4F);
                            blend = (cell.MiddleAnimationFrame & 0x50) > 0;
                        }

                        Size s = library.GetSize(index);


                        if ((s.Width != CellWidth || s.Height != CellHeight) && (s.Width != CellWidth * 2 || s.Height != CellHeight * 2))
                        {
                            Manager.Sprite.Transform = Matrix.Multiply(Matrix.Translation(drawX, drawY - s.Height, 0), scale);

                            if (!blend)
                                library.Draw(index, 0, 0, Color.White, false, 1F, ImageType.Image);
                            else
                                library.DrawBlend(index, 0, 0, Color.White, false, 0.5F, ImageType.Image);
                        }
                    }
                }
            }

            //Invalid Tile = 59
            //Selected Tile = 58


            maxY = Math.Min(Height - 1, StartY + (int)Math.Ceiling(Size.Height / CellHeight));


            Manager.SetOpacity(0.35F);
            for (int y = minY; y <= maxY; y++)
            {
                float drawY = (y - StartY) * BaseCellHeight;

                for (int x = minX; x <= maxX; x++)
                {
                    float drawX = (x - StartX) * BaseCellWidth;

                    Cell tile = Cells[x, y];

                    if (tile.Flag != AttributeSelection)
                    {
                        if (!DrawAttributes) continue;

                        Manager.Sprite.Transform = Matrix.Multiply(Matrix.Translation(drawX, drawY, 0), scale);

                        //markLibrary.Draw(59, 0, 0, Color.White, false, 1F, ImageType.Image);
                        Manager.Sprite.Draw(Manager.AttributeTexture, Vector3.Zero, Vector3.Zero, Color.Red);
                    }
                    else
                    {
                        if (!DrawSelection) continue;
                        if (!Selection.Contains(new Point(x, y))) continue;

                        Manager.Sprite.Transform = Matrix.Multiply(Matrix.Translation(drawX, drawY, 0), scale);

                        Manager.Sprite.Draw(Manager.AttributeTexture, Vector3.Zero, Vector3.Zero, Color.Yellow);

                        //markLibrary.Draw(58, 0, 0, Color.Lime, false, 1F, ImageType.Image);
                        //If Selected.
                    }
                }
            }
            Manager.Sprite.Flush();

            Manager.SetOpacity(1F);
            if (Border)
            {
                Manager.Line.Draw(new[]
                {
                    new Vector2((MouseLocation.X - StartX)*CellWidth, (MouseLocation.Y - StartY)*CellHeight),
                    new Vector2((MouseLocation.X - StartX)*CellWidth + CellWidth, (MouseLocation.Y - StartY)*CellHeight),
                    new Vector2((MouseLocation.X - StartX)*CellWidth + CellWidth, (MouseLocation.Y - StartY)*CellHeight + CellHeight),
                    new Vector2((MouseLocation.X - StartX)*CellWidth, (MouseLocation.Y - StartY)*CellHeight + CellHeight),
                    new Vector2((MouseLocation.X - StartX)*CellWidth, (MouseLocation.Y - StartY)*CellHeight),
                }, Color.Lime);


                if (Radius > 0)
                    Manager.Line.Draw(new[]
                    {
                        new Vector2((MouseLocation.X - StartX - Radius)*CellWidth, (MouseLocation.Y - StartY - Radius)*CellHeight),
                        new Vector2((MouseLocation.X - StartX + Radius)*CellWidth + CellWidth, (MouseLocation.Y - StartY- Radius)*CellHeight),
                        new Vector2((MouseLocation.X - StartX + Radius)*CellWidth + CellWidth, (MouseLocation.Y - StartY + Radius)*CellHeight + CellHeight),
                        new Vector2((MouseLocation.X - StartX - Radius)*CellWidth, (MouseLocation.Y - StartY + Radius)*CellHeight + CellHeight),
                        new Vector2((MouseLocation.X - StartX - Radius)*CellWidth, (MouseLocation.Y - StartY - Radius)*CellHeight),
                    }, Color.Lime);
            }





            Manager.Sprite.Transform = Matrix.Identity;
        }

        public void Load(string fileName)
        {
            // Load: 从磁盘读取 .map 文件并解析到 Cells 数组
            // 说明（新手）：
            // - 文件既可以是完整路径也可以只给文件名（会在 Config.MapPath 下查找带 .map 后缀的文件）。
            // - 解析会读取地图宽高并填充每个格子的图层索引、动画帧、光照与标志位（Flag）。
            // - 读取完成后会把 TextureValid 设为 false，触发在下一次绘制时重建离屏纹理。
            try
            {
                string path = null;

                if (Path.IsPathRooted(fileName))
                {
                    path = fileName;
                }
                else
                {
                    path = Path.Combine(Config.MapPath, fileName + ".map");
                }

                if (!File.Exists(path)) return;

                using (MemoryStream mStream = new MemoryStream(File.ReadAllBytes(path)))
                using (BinaryReader reader = new BinaryReader(mStream))
                {
                    mStream.Seek(22, SeekOrigin.Begin);
                    Width = reader.ReadInt16();
                    Height = reader.ReadInt16();

                    mStream.Seek(28, SeekOrigin.Begin);

                    Cells = new Cell[Width, Height];
                    for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                            Cells[x, y] = new Cell();

                    for (int x = 0; x < Width / 2; x++)
                        for (int y = 0; y < Height / 2; y++)
                        {
                            Cells[(x * 2), (y * 2)].BackFile = reader.ReadByte();
                            Cells[(x * 2), (y * 2)].BackImage = reader.ReadUInt16();
                        }

                    for (int x = 0; x < Width; x++)
                        for (int y = 0; y < Height; y++)
                        {
                            byte flag = reader.ReadByte();
                            Cells[x, y].MiddleAnimationFrame = reader.ReadByte();

                            byte value = reader.ReadByte();
                            Cells[x, y].FrontAnimationFrame = value == 255 ? 0 : value;

                            Cells[x, y].FrontFile = reader.ReadByte();
                            Cells[x, y].MiddleFile = reader.ReadByte();

                            Cells[x, y].MiddleImage = reader.ReadUInt16() + 1;
                            Cells[x, y].FrontImage = reader.ReadUInt16() + 1;

                            mStream.Seek(3, SeekOrigin.Current);

                            Cells[x, y].Light = (byte)(reader.ReadByte() & 0x0F) * 2;

                            mStream.Seek(1, SeekOrigin.Current);

                            Cells[x, y].Flag = ((flag & 0x01) != 1) || ((flag & 0x02) != 2);
                        }
                }
            }
            catch (Exception ex)
            {
                SEnvir.Log(ex.ToString());
            }
            TextureValid = false;
        }


        #region IDisposable Support

        public bool IsDisposed { get; private set; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;

                if (ControlTexture != null)
                {
                    if (!ControlTexture.IsDisposed)
                        ControlTexture.Dispose();

                    ControlTexture = null;
                }

                if (ControlSurface != null)
                {
                    if (!ControlSurface.IsDisposed)
                        ControlSurface.Dispose();

                    ControlSurface = null;
                }

                _Size = Size.Empty;

                TextureValid = false;
                TextureSize = Size.Empty;
                ExpireTime = DateTime.MinValue;

                if (Manager?.Map == this)
                    Manager.Map = null;
            }

        }

        ~MapControl()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void MouseDown(MouseEventArgs e)
        {


            switch (e.Button)
            {
                case MouseButtons.Left:

                    for (int y = MouseLocation.Y - Radius; y <= MouseLocation.Y + Radius; y++)
                        for (int x = MouseLocation.X - Radius; x <= MouseLocation.X + Radius; x++)
                        {
                            if (x < 0 || x >= Width || y < 0 || y >= Height || Cells[x, y].Flag != AttributeSelection) continue;

                            Selection.Add(new Point(x, y));
                        }


                    break;
                case MouseButtons.Right:

                    for (int y = MouseLocation.Y - Radius; y <= MouseLocation.Y + Radius; y++)
                        for (int x = MouseLocation.X - Radius; x <= MouseLocation.X + Radius; x++)
                        {
                            if (x < 0 || x >= Width || y < 0 || y >= Height || Cells[x, y].Flag != AttributeSelection) continue;

                            Selection.Remove(new Point(x, y));
                        }
                    break;
                case MouseButtons.Middle:
                    if (MouseLocation.X < 0 || MouseLocation.X >= Width || MouseLocation.Y < 0 || MouseLocation.Y >= Height) return;
                    if (Cells[MouseLocation.X, MouseLocation.Y].Flag != AttributeSelection) return;

                    HashSet<Point> doneList = new HashSet<Point> { MouseLocation };
                    Queue<Point> todoList = new Queue<Point>();

                    todoList.Enqueue(MouseLocation);

                    if (Selection.Contains(MouseLocation)) //removing
                    {
                        while (todoList.Count > 0)
                        {
                            Point p = todoList.Dequeue();

                            for (int i = 0; i < 8; i++)
                            {
                                Point nPoint = Functions.Move(p, (MirDirection)i);

                                if (nPoint.X < 0 || nPoint.X >= Width || nPoint.Y < 0 || nPoint.Y >= Height) continue;

                                if (Cells[nPoint.X, nPoint.Y].Flag != AttributeSelection) continue;

                                if (doneList.Contains(nPoint)) continue;

                                if (!Selection.Contains(nPoint)) continue;

                                doneList.Add(nPoint);
                                todoList.Enqueue(nPoint);
                            }

                            Selection.Remove(p);
                        }

                    }
                    else
                    {
                        while (todoList.Count > 0)
                        {
                            Point p = todoList.Dequeue();

                            for (int i = 0; i < 8; i++)
                            {
                                Point nPoint = Functions.Move(p, (MirDirection)i);

                                if (nPoint.X < 0 || nPoint.X >= Width || nPoint.Y < 0 || nPoint.Y >= Height) continue;

                                if (Cells[nPoint.X, nPoint.Y].Flag != AttributeSelection) continue;

                                if (doneList.Contains(nPoint)) continue;

                                if (Selection.Contains(nPoint)) continue;

                                doneList.Add(nPoint);
                                todoList.Enqueue(nPoint);
                            }

                            Selection.Add(p);
                        }
                    }

                    break;
            }
            TextureValid = false;
        }
        public void MouseMove(MouseEventArgs e)
        {
            MouseLocation = new Point(Math.Min(Width, Math.Max(0, (int)(e.X / CellWidth) + StartX)), Math.Min(Height, Math.Max(0, (int)(e.Y / CellHeight) + StartY)));

            switch (e.Button)
            {
                case MouseButtons.Left:
                    for (int y = MouseLocation.Y - Radius; y <= MouseLocation.Y + Radius; y++)
                        for (int x = MouseLocation.X - Radius; x <= MouseLocation.X + Radius; x++)
                        {
                            if (x < 0 || x >= Width || y < 0 || y >= Height || Cells[x, y].Flag != AttributeSelection) continue;

                            Selection.Add(new Point(x, y));
                        }
                    break;
                case MouseButtons.Right:

                    for (int y = MouseLocation.Y - Radius; y <= MouseLocation.Y + Radius; y++)
                        for (int x = MouseLocation.X - Radius; x <= MouseLocation.X + Radius; x++)
                        {
                            if (x < 0 || x >= Width || y < 0 || y >= Height || Cells[x, y].Flag != AttributeSelection) continue;

                            Selection.Remove(new Point(x, y));
                        }
                    break;
            }
        }

        public void MouseEnter()
        {
            Border = true;
        }
        public void MouseLeave()
        {
            Border = false;
        }



        public sealed class Cell
        {
            public int BackFile;
            public int BackImage;

            public int MiddleFile;
            public int MiddleImage;

            public int FrontFile;
            public int FrontImage;

            public int FrontAnimationFrame;
            public int FrontAnimationTick;

            public int MiddleAnimationFrame;
            public int MiddleAnimationTick;

            public int Light;

            public bool Flag;
        }

    }


}