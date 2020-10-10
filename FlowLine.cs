using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace South.Hosse.Controls
{
    public partial class FlowLine : UserControl
    {
        /// <summary>
        /// 线容器框偏移中心线距离
        /// </summary>
        const int offset = 5;

        public FlowLine()
        {
            StartPoint = new Point(0, 0);
            EndPoint = new Point(this.Width, this.Height);

            this.BackColor = Color.Transparent;
            InitializeComponent();
        }

        private bool selected;

        /// <summary>
        /// 选中
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
            }
        }

        private DashStyle lineStyle = DashStyle.Solid;

        /// <summary>
        /// 线的样式
        /// </summary>
        public DashStyle LineStyle
        {
            get
            {
                return lineStyle;
            }
            set
            {
                lineStyle = value;
            }
        }

        /// <summary>
        /// 根据线的起点和终点设置线的位置
        /// </summary>
        /// <param name="control"></param>
        public void SetPoint()
        {
            if (StartControl != null && EndControl != null)
            {
                SetPoint(StartControl, EndControl);
            }
        }

        /// <summary>
        /// 设置线的起点和终点
        /// </summary>
        /// <param name="startPoint">父容器的起点坐标</param>
        /// <param name="endPoint">父容器的终点坐标</param>
        public void SetPoint(Control startControl, Control endControl)
        {
            double x1 = (double)startControl.Left + (double)startControl.Width / 2.0;
            double y1 = (double)startControl.Top + (double)startControl.Height / 2.0;
            double x2 = (double)endControl.Left + (double)endControl.Width / 2.0;
            double y2 = (double)endControl.Top + (double)endControl.Height / 2.0;

            StartControl = startControl;
            EndControl = endControl;

            Point point = GetIntersectEndPoint(x1, y1, x2, y2);

            SetPoint((int)x1, (int)y1, point.X, point.Y);
        }

        public void SetPoint(int x1, int y1, int x2, int y2)
        {
            this.Left = Math.Min(x1, x2) - offset;
            this.Top = Math.Min(y1, y2) - offset;
            this.Width = Math.Abs(x1 - x2) + offset + offset;
            this.Height = Math.Abs(y1 - y2) + offset + offset;

            StartPoint.X = x1 - this.Left;
            StartPoint.Y = y1 - this.Top;
            EndPoint.X = x2 - this.Left;
            EndPoint.Y = y2 - this.Top;

            Invalidate();  //重画窗口
        }


        /// <summary>
        /// 线的起点(线矩形框为坐标系)
        /// </summary>
        private Point StartPoint;

        /// <summary>
        ///线的终点(线矩形框为坐标系)
        /// </summary>
        private Point EndPoint;

        /// <summary>
        /// 起始控件
        /// </summary>
        public Control StartControl { get; set; }

        /// <summary>
        /// 结束控件
        /// </summary>
        public Control EndControl { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.Region = GetWindowRegion();  //根据起点 终点获得闭合区域

            this.BackColor = Color.Transparent;

            Pen pen = new Pen(Color.DodgerBlue, (float)1.5);
            if (selected)
            {
                pen.Width = 2;
                pen.Color = Color.Blue;
            }

            //  抗锯齿
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            pen.DashStyle = this.lineStyle;
            if (pen.DashStyle == DashStyle.Dash)
            {
                pen.Color = Color.Gray;
            }
            pen.CustomEndCap = new AdjustableArrowCap(5, 8);
            e.Graphics.DrawLine(pen, StartPoint, EndPoint);


            base.OnPaint(e);
        }

        /// <summary>
        /// 环节图形中心连线和结束环节边框的交点
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private Point GetIntersectEndPoint(double x1, double y1, double x2, double y2)
        {
            double x;
            double y;
            y = y2 - (double)EndControl.Height / 2.0; //EndControl.Top
            if (y >= Math.Min(y1, y2) && y <= Math.Max(y1, y2))
            {
                if (GetX(x1, y1, x2, y2, y, out x))
                {
                    if (IsIntersectEndPoint(x, y))
                    {
                        return new Point((int)x, (int)y);
                    }
                }
            }

            y = y2 + (double)EndControl.Height / 2.0;
            if (y >= Math.Min(y1, y2) && y <= Math.Max(y1, y2))
            {
                if (GetX(x1, y1, x2, y2, y, out x))
                {
                    if (IsIntersectEndPoint(x, y))
                    {
                        return new Point((int)x, (int)y);
                    }
                }
            }

            x = x2 - (double)EndControl.Width / 2.0;
            if (x >= Math.Min(x1, x2) && x <= Math.Max(x1, x2))
            {
                if (GetY(x1, y1, x2, y2, x, out y))
                {
                    if (IsIntersectEndPoint(x, y))
                    {
                        return new Point((int)x, (int)y);
                    }
                }
            }

            x = x2 + (double)EndControl.Width / 2.0;
            if (x >= Math.Min(x1, x2) && x <= Math.Max(x1, x2))
            {
                if (GetY(x1, y1, x2, y2, x, out y))
                {
                    if (IsIntersectEndPoint(x, y))
                    {
                        return new Point((int)x, (int)y);
                    }
                }
            }

            return new Point((int)x2, (int)y2);
        }

        private bool IsIntersectEndPoint(double x, double y)
        {
            double x1 = EndControl.Left, y1 = EndControl.Top, x2 = EndControl.Right, y2 = EndControl.Bottom;
            return x >= Math.Min(x1, x2) && x <= Math.Max(x1, x2) && y >= Math.Min(y1, y2) && y <= Math.Max(y1, y2);
        }

        /// <summary>
        /// 根据起点和终点 获得闭合区域
        /// </summary>
        /// <returns></returns>
        private Region GetWindowRegion()
        {
            Point[] pts = new Point[5];
            GetParallelPoint(out pts[0], out pts[1], offset);  //起点终点 向上平行点
            GetParallelPoint(out pts[3], out pts[2], -offset);  //起点终点 向下平行点
            pts[4] = pts[0];  //闭合区域

            GraphicsPath path = new GraphicsPath();
            path.AddLines(pts);
            return new Region(path);
        }

        /// <summary>
        /// 获取线的平行线的点
        /// </summary>
        /// <param name="ptout1"></param>
        /// <param name="ptout2"></param>
        /// <param name="direction"></param>
        private void GetParallelPoint(out Point ptout1, out Point ptout2, int direction)
        {
            ptout1 = new Point(StartPoint.X, StartPoint.Y);
            ptout2 = new Point(EndPoint.X, EndPoint.Y);
            int inc = 1;
            if (StartPoint.Y == EndPoint.Y)
            {
                inc = (StartPoint.X < EndPoint.X ? 1 : -1);
                ptout1.Y = StartPoint.Y - direction * inc;
                ptout2.Y = EndPoint.Y - direction * inc;
            }
            else if (StartPoint.X == EndPoint.X)
            {
                inc = (StartPoint.Y < EndPoint.Y ? -1 : 1);
                ptout1.X = StartPoint.X - direction * inc;
                ptout2.X = EndPoint.X - direction * inc;
            }
            else
            {
                inc = StartPoint.X < EndPoint.X ? 1 : -1;
                double theta = -Convert.ToDouble(EndPoint.Y - StartPoint.Y) / Convert.ToDouble((EndPoint.X - StartPoint.X));
                theta = Math.Atan(theta);
                ptout1.X = StartPoint.X - Convert.ToInt32(direction * Math.Sin(theta)) * inc;
                ptout1.Y = StartPoint.Y - Convert.ToInt32(direction * Math.Cos(theta)) * inc;
                ptout2.X = EndPoint.X - Convert.ToInt32(direction * Math.Sin(theta)) * inc;
                ptout2.Y = EndPoint.Y - Convert.ToInt32(direction * Math.Cos(theta)) * inc;
            }
        }

        /// <summary>
        /// 知道直线两点 和第三点x坐标，求y坐标
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <returns></returns>
        private bool GetY(double x1, double y1, double x2, double y2, double x3, out double y)
        {
            if (x1 == x2)
            {
                y = 0;
                return false;
            }
            else if (y1 == y2)
            {
                y = y1;
                return true;
            }
            double k = (y2 - y1) / (x2 - x1);
            double b = y1 - k * x1;
            y = (k * x3 + b);
            return true;
        }
        /// <summary>
        /// 知道直线两点 和第三点y坐标，求x坐标
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <returns></returns>
        private bool GetX(double x1, double y1, double x2, double y2, double y3, out double x)
        {
            if (x1 == x2)
            {
                x = x1;
                return true;
            }
            if (y1 == y2 )
            {
                x = 0;
                return false;
            }

            double k = (y2 - y1) / (x2 - x1);
            double b = y1 - k * x1;
            x = ((y3 - b) / k);
            return true;
        }
    }
}
