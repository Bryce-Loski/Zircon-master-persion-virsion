using DevExpress.XtraBars;
using Server.Envir;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;


namespace Server.Views
{
    public partial class ChatLogView : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public static BindingList<string> Logs = new BindingList<string>();

        public ChatLogView()
        {
            InitializeComponent();

            // 绑定日志数据源
            LogListBoxControl.DataSource = Logs;

            // 支持多选（一次复制多条聊天）
            LogListBoxControl.SelectionMode = SelectionMode.MultiExtended;

            // 右键菜单：复制选中 & 复制全部
            var menu = new ContextMenuStrip();

            var copySelectedItem = new ToolStripMenuItem("复制选中聊天");
            copySelectedItem.Click += (s, e) => CopySelectedLogsToClipboard();

            var copyAllItem = new ToolStripMenuItem("复制全部聊天");
            copyAllItem.Click += (s, e) => CopyAllLogsToClipboard();

            menu.Items.Add(copySelectedItem);
            menu.Items.Add(copyAllItem);

            LogListBoxControl.ContextMenuStrip = menu;

            // 支持 Ctrl + C 复制选中聊天
            LogListBoxControl.KeyDown += LogListBoxControl_KeyDown;
        }


        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            while (!SEnvir.ChatLogs.IsEmpty)
            {
                string log;

                if (!SEnvir.DisplayChatLogs.TryDequeue(out log)) continue;

                Logs.Add(log);
            }

            if (Logs.Count > 0)
                ClearLogsButton.Enabled = true;
        }

        private void ClearLogsButton_ItemClick(object sender, ItemClickEventArgs e)
        {
            Logs.Clear();
            ClearLogsButton.Enabled = false;
        }

        private void CopySelectedLogsToClipboard()
        {
            if (LogListBoxControl.SelectedIndices == null ||
                LogListBoxControl.SelectedIndices.Count == 0 ||
                Logs == null || Logs.Count == 0)
                return;

            var list = new List<string>();

            foreach (int index in LogListBoxControl.SelectedIndices)
            {
                if (index >= 0 && index < Logs.Count)
                    list.Add(Logs[index]);
            }

            if (list.Count > 0)
                Clipboard.SetText(string.Join(Environment.NewLine, list));
        }

        private void CopyAllLogsToClipboard()
        {
            if (Logs == null || Logs.Count == 0)
                return;

            Clipboard.SetText(string.Join(Environment.NewLine, Logs));
        }

        private void LogListBoxControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + C 复制选中聊天记录
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedLogsToClipboard();
                e.Handled = true;
            }
        }

    }
}