using System.Drawing;
using System.Windows.Forms;

namespace PerfMonitor.Library
{
    class CustomMessageBox : Form
    {
        private uint btnid = 0;
        private readonly DialogResult dr = DialogResult.Cancel;

        public DialogResult ShowResult() { return btnid == 1 ? DialogResult.OK : DialogResult.Cancel; }

        public CustomMessageBox (Form parent, string message, string title, Point location, MessageBoxButtons buttons)
        {
            // 设置窗体的属性
            Text = title;
            ControlBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Size = new (400, 150);

            // 创建标签来显示消息文本
            Label messageLabel = new ();
            messageLabel.Text = message;
            messageLabel.AutoSize = true;
            messageLabel.Location = new (20, 20);
            ShowInTaskbar = false;
            Owner = parent;

            switch ( buttons )
            {
                case MessageBoxButtons.OK:
                    {
                        Button closeButton = new()
                        {
                            Text = "确认",
                            Size = new(80, 30),
                            Location = new(160, 80)
                        };
                        closeButton.Click += (sender, e) => OK_Pressed();
                        Controls.Add(closeButton);
                    }
                    break;
                case MessageBoxButtons.OKCancel:
                    {
                        Button OKBtn = new()
                        {
                            Text = "OK",
                            Size = new(80, 30),
                            Location = new(80, 80)
                        };
                        OKBtn.Click += (sender, e) => OK_Pressed();
                        Button CanBtn = new()
                        {
                            Text = "Canel",
                            Size = new(80, 30),
                            Location = new(240, 80),
                        };
                        CanBtn.Click += (sender, e) => Cancel_Pressed();
                        Controls.Add(OKBtn);
                        Controls.Add(CanBtn);
                    }
                    break;
                case MessageBoxButtons.YesNo:
                    {
                        Button OKBtn = new()
                        {
                            Text = "Yes",
                            Size = new(80, 30),
                            Location = new(80, 80)
                        };
                        OKBtn.Click += (sender, e) => OK_Pressed();
                        Button CanBtn = new()
                        {
                            Text = "No",
                            Size = new(80, 30),
                            Location = new(240, 80)
                        };
                        CanBtn.Click += (sender, e) => Cancel_Pressed();
                        Controls.Add(OKBtn);
                        Controls.Add(CanBtn);
                    }
                    break;
            }
            // 将控件添加到窗体中
            Controls.Add(messageLabel);

            // 设置窗体的位置
            StartPosition = FormStartPosition.Manual;
            Location = location;
        }

        internal void OK_Pressed()
        {
            btnid = 1;
            Dispose();
        }
        internal void Cancel_Pressed ()
        {
            btnid = 0;
            Dispose();
        }
    }

}
