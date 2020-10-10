using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace South.Hosse.Controls
{
    /// <summary>
    /// 环节类型
    /// </summary>
    public enum WorkTacheType
    {
        /// <summary>
        /// 开始
        /// </summary>
        Begin,
        /// <summary>
        /// 结束
        /// </summary>
        End,
        /// <summary>
        /// 一般
        /// </summary>
        Normal,

        /// <summary>
        /// 未到达灰色
        /// </summary>
        Gray,

        /// <summary>
        /// 未到达灰色
        /// </summary>
        Current
    }

    public partial class WorkTache : Button
    {

        private Button radio = new Button();  // 触发画线的控件
        public MouseEventHandler RadioMouseDown;
        private WorkTacheType tacheType;
        private FlowLine flowLine1;
        private FlowLine flowLine2;
        private bool selected;

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                TacheInfo.Name = value;
                base.Text = value;
            }
        }

        /// <summary>
        /// 环节信息
        /// </summary>
        private WorkTacheInfo workTacheInfo = new WorkTacheInfo();

        public WorkTacheInfo TacheInfo
        {
            get { return workTacheInfo; }
            set
            {
                if (value != null)
                {
                    workTacheInfo = value;
                    this.Text = workTacheInfo.Name;
                }
            }
        }

        /// <summary>
        /// 流入环节连接的线
        /// </summary>
        public FlowLine FlowLine1 { get { return flowLine1; } set { flowLine1 = value; } }

        /// <summary>
        /// 流出环节连接的线
        /// 开始和结束环节仅有一条线可以连接
        /// </summary>
        public FlowLine FlowLine2
        {
            get
            {
                return flowLine2;
            }
            set
            {
                flowLine2 = value;
            }
        }

        /// <summary>
        /// 环节类型
        /// </summary>
        public WorkTacheType TacheType
        {
            get
            {
                return tacheType;
            }
            set
            {
                tacheType = value;
                ChangeBg();
            }
        }

        /// <summary>
        /// 控件是否被选中
        /// </summary>
        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                if (selected)
                {
                    this.BringToFront(); //置顶
                    BackColor = Color.LightBlue;
                }
                else
                {
                    BackColor = Color.Transparent;
                }
            }
        }

        /// <summary>
        /// 控件相对父容器中心点的位置
        /// </summary>
        public Point CentrePosition
        {
            get
            {
                return new Point(this.Left + this.Width / 2, this.Top + this.Height / 2);
            }
            set
            {
                this.Left = value.X + this.Width / 2;
                this.Top = value.Y + this.Height / 2;
            }
        }


        public WorkTache()
        {
            InitializeComponent();

            this.Cursor = Cursors.Hand;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.Height = 72;
            this.Width = 73;
            this.TextAlign = ContentAlignment.BottomCenter;

            this.FlatAppearance.BorderSize = 0;
            this.FlatStyle = FlatStyle.Flat;
            this.UseVisualStyleBackColor = false;
            this.AutoSize = false;

            this.radio.FlatAppearance.BorderSize = 0;
            this.radio.FlatStyle = FlatStyle.Flat;
            this.radio.UseVisualStyleBackColor = false;
            this.radio.BackColor = Color.Transparent;
            this.radio.AutoSize = false;
            this.radio.Left = this.Width / 2 - 5;
            this.radio.Top = this.Height / 2 - 5;
            this.radio.Width = 10;
            this.radio.Height = 10;
            this.radio.Text = string.Empty;
            GraphicsPath circlePath = new GraphicsPath();
            circlePath.AddEllipse(0, 0, this.radio.Width, this.radio.Height);
            this.radio.Region = new Region(circlePath);

            this.radio.MouseDown += new MouseEventHandler(radio_MouseDown);
            this.radio.MouseEnter += new EventHandler(radio_MouseEnter);
            this.radio.MouseLeave += new EventHandler(radio_MouseLeave);

            tacheType = WorkTacheType.Normal;

            this.Controls.Add(radio);

            this.LoadBackgroudImage("Tache.png");
        }

        /// <summary>
        /// 是否已经和环节具有联系
        /// </summary>
        /// <param name="workTache"></param>
        /// <returns></returns>
        public bool IsRelatedTo(WorkTache workTache)
        {
            bool res = false;
            if (flowLine1 != null)
            {
                if (flowLine1.StartControl == workTache || flowLine1.EndControl == workTache)
                {
                    res = true;
                }
            }
            if (flowLine2 != null)
            {
                if (flowLine2.StartControl == workTache || flowLine2.EndControl == workTache)
                {
                    res = true;
                }
            }
            return res;
        }

        void radio_MouseLeave(object sender, EventArgs e)
        {
            this.radio.BackColor = Color.Transparent;
        }

        void radio_MouseEnter(object sender, EventArgs e)
        {
            this.radio.BackColor = Color.Red;
        }

        void radio_MouseDown(object sender, MouseEventArgs e)
        {
            if (RadioMouseDown != null)
            {
                this.RadioMouseDown.Invoke(this, e);
            }
        }

        /// <summary>
        /// 选中时切换控件背景 
        /// </summary>
        private void ChangeBg()
        {
            switch (this.TacheType)
            {
                case WorkTacheType.Begin:
                    this.BackgroundImage = Properties.Resources.TacheStart;
                    this.LoadBackgroudImage("TacheStart.png");
                    break;
                case WorkTacheType.End:
                    this.BackgroundImage = Properties.Resources.TacheEnd;
                    this.LoadBackgroudImage("TacheEnd.png");
                    radio.Visible = false;
                    break;
                case WorkTacheType.Normal:
                    this.BackgroundImage = Properties.Resources.Tache;
                    this.LoadBackgroudImage("Tache.png");
                    break;
                case WorkTacheType.Gray:
                    this.BackgroundImage = Properties.Resources.TacheGray;
                    this.LoadBackgroudImage("TacheGray.png");
                    break;
                case WorkTacheType.Current:
                    this.BackgroundImage = Properties.Resources.TacheCurrent;
                    this.LoadBackgroudImage("TacheCurrent.png");
                    break;
                default:
                    break;
            }
        }

    }

    /// <summary>
    /// 定义环节信息的类
    /// </summary>
    public class WorkTacheInfo
    {
        /// <summary>
        /// 环节所在索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 环节id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 环节实际数据
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 环节名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 提示
        /// </summary>
        public string Tips { get; set; }

        /// <summary>
        /// 与下一个节点不相连
        /// </summary>
        public bool IsDisConnectedFromNext { get; set; }

        /// <summary>
        /// 下一条线是虚线
        /// </summary>
        public bool IsNextLineDashed { get; set; }
    }
}
