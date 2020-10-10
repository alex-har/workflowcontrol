using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace South.Hosse.Controls
{
    /// <summary>
    /// 可视化工作流控件
    /// </summary>
    public partial class WorkFlow : UserControl
    {
        /// <summary>
        /// 只读
        /// </summary>
        private bool isReadOnly = false;
        /// <summary>
        /// 当前选中的对象
        /// </summary>
        private Control selectedObj;

        /// <summary>
        /// 环节连接的线
        /// </summary>
        private FlowLine tempLine;

        /// <summary>
        /// 当前鼠标按下的坐标
        /// </summary>
        private Point mouseDownCursor;

        public WorkFlow()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
        }

        /// <summary>
        /// 工作流信息
        /// </summary>
        public WorkFlowInfo FlowInfo;

        /// <summary>
        /// 选中的环节信息
        /// </summary>
        public WorkTacheInfo SelectedTacheInfo
        {
            get
            {
                if (selectedObj != null && selectedObj is WorkTache && (selectedObj as WorkTache).TacheType == WorkTacheType.Normal)
                {
                    return (selectedObj as WorkTache).TacheInfo;
                }
                else
                {
                    return null;
                }
            }
        }



        #region Public Method
        /// <summary>
        /// 以只读模式显示流程图画板，不显示工具栏菜单
        /// </summary>
        public void SetReadOnly()
        {
            isReadOnly = true;
        }

        public void Clear()
        {
            this.FlowInfo = new WorkFlowInfo();
            panelWorkFlow.Controls.Clear();
            selectedObj = null;
            tempLine = null;
        }

        /// <summary>
        ///加载工作流数据
        /// </summary>
        /// <param name="workTaches"></param>
        public void LoadFlowInfo(WorkFlowInfo flowInfo, string currentTacheId = "")
        {
            Clear();

            this.FlowInfo.Id = FlowInfo.Id;

            List<WorkTacheInfo> workTacheInfos = flowInfo.TacheInfos;

            WorkTache wtStart = CreateTacheToView();
            wtStart.TacheType = WorkTacheType.Begin;
            wtStart.Text = "开始";

            SetTacheProperty(wtStart);

            List<WorkTache> taches = new List<WorkTache>();
            taches.Add(wtStart);
            bool left = false;
            bool disConnected = false;
            for (int i = 0; i < workTacheInfos.Count; i++)
            {
                WorkTacheInfo item = workTacheInfos[i];
                WorkTache wt = CreateTacheToView();
                wt.TacheInfo = item;

                SetTacheProperty(wt);

                SetTachePosition(taches[i], wt, ref left);
                if (!disConnected)
                {
                    ConnectionLineToTache(taches[i], wt, currentTacheId);
                    disConnected = false;
                }

                if (workTacheInfos[i].IsDisConnectedFromNext)
                {
                    disConnected = true;
                }
                else
                {
                    disConnected = false;
                }
                taches.Add(wt);
            }

            //关联结束节点
            if (!disConnected)
            {
                WorkTache wtEnd = CreateTacheToView();
                wtEnd.TacheType = WorkTacheType.End;
                wtEnd.Text = "结束";

                SetTacheProperty(wtEnd);

                SetTachePosition(taches.LastOrDefault(), wtEnd, ref left);
                //必须有环节数据 开始和结束节点才连线
                if (workTacheInfos.Count > 0)
                {
                    ConnectionLineToTache(taches.LastOrDefault(), wtEnd);
                }
            }
        }

        /// <summary>
        ///  添加新环节
        /// </summary>
        /// <param name="workTacheInfo"></param>
        public void AddTache(WorkTacheInfo workTacheInfo)
        {
            WorkTache wt = CreateTacheToView();
            wt.TacheType = WorkTacheType.Normal;
            wt.Location = new Point(panelWorkFlow.Width / 2, panelWorkFlow.Height / 2);
            wt.Text = wt.TacheInfo.Name;
            wt.TacheInfo = workTacheInfo;

        }

        /// <summary>
        /// 更新环节
        /// </summary>
        /// <param name="workTacheInfo"></param>
        public void UpdateSelectedTache(WorkTacheInfo workTacheInfo)
        {
            if (selectedObj is WorkTache && (selectedObj as WorkTache).TacheType == WorkTacheType.Normal)
            {
                WorkTache tache = selectedObj as WorkTache;
                tache.Text = tache.TacheInfo.Name;
                tache.TacheInfo = workTacheInfo;
            }
        }

        /// <summary>
        /// 删除选中对象
        /// </summary>
        public void RemoveSelectedObject()
        {
            RomveObject();
        }

        /// <summary>
        /// 获取工作流信息
        /// </summary>
        /// <returns></returns>
        public WorkFlowInfo GetFlowInfo()
        {
            List<WorkTacheInfo> infos = GetWorkFlow();
            if (infos != null)
            {
                this.FlowInfo.TacheInfos = infos;
                return this.FlowInfo;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region 工作流 Panle 事件交互

        private void panelWorkFlow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isReadOnly)
            {
                return;
            }

            if (mouseDownCursor.X == e.X && mouseDownCursor.Y == e.Y)
            {
                return;
            }

            if (this.panelWorkFlow.Capture)
            {
                if (selectedObj is WorkTache)
                {
                    //画线
                    if (tempLine != null)
                    {
                        int x = selectedObj.Left + selectedObj.Width / 2;
                        int y = selectedObj.Top + selectedObj.Height / 2;
                        tempLine.SetPoint(x, y, e.X, e.Y);
                    }
                    //移动环节
                    else
                    {
                        if (TestHit(selectedObj, e.X, e.Y))
                        {
                            return;
                        }
                        if (selectedObj is WorkTache)
                        {
                            SetPositon(selectedObj, e.X, e.Y);  //移动环节
                            //移动环节关联的线
                            WorkTache wt = selectedObj as WorkTache;
                            if (wt.FlowLine1 != null)
                            {
                                wt.FlowLine1.SetPoint();
                            }
                            if (wt.FlowLine2 != null)
                            {
                                wt.FlowLine2.SetPoint();
                            }
                        }
                    }
                }
            }
            mouseDownCursor.X = e.X;
            mouseDownCursor.Y = e.Y;
        }

        private void panelWorkFlow_MouseUp(object sender, MouseEventArgs e)
        {
            if (isReadOnly)
            {
                return;
            }

            this.panelWorkFlow.Capture = false;

            if (tempLine != null && selectedObj is WorkTache)
            {
                WorkTache endTache = GetWorkTacheControl(e.X, e.Y);
                if (endTache == null || endTache.Equals(selectedObj))
                {

                }
                else
                {
                    ConnectionLineToTache(selectedObj as WorkTache, endTache); // 线关联到环节
                }
                this.panelWorkFlow.Controls.Remove(tempLine);
                tempLine = null;
            }
        }


        /// <summary>
        /// 将线关联到环节
        /// </summary>
        /// <param name="endTache"></param>
        private FlowLine ConnectionLineToTache(WorkTache startTache, WorkTache endTache, string currentTacheId = "")
        {
            if (startTache == null || endTache == null)
            {
                return null;
            }
            if (!(startTache is WorkTache))
            {
                return null;
            }

            if (startTache.TacheType == WorkTacheType.End)
            {
                this.ShowWarning("结束环节不能够作为流程起点！");
                return null;
            }
            else if (startTache.FlowLine2 != null)
            {
                this.ShowWarning("该环节已经流出！");
                return null;
            }

            if (endTache.TacheType == WorkTacheType.Begin)
            {
                this.ShowWarning("开始环节不能作为流程终点！");
                return null;
            }
            else if (endTache.FlowLine1 != null)
            {
                this.ShowWarning("该环节已经流入！");
                return null;
            }

            if (startTache.IsRelatedTo(endTache))
            {
                //this.ShowWarning("环节已经存在关联，请重新选择环节！");
                return null;
            }

            if (IsClosedLoop(startTache, endTache))
            {
                this.ShowWarning("流程不能形成闭环！");
                return null;
            }

            FlowLine line = new FlowLine();
            line.KeyDown += new KeyEventHandler(control_KeyDown);
            line.MouseClick += new MouseEventHandler(flowLine_MouseClick);

            line.SetPoint(startTache, endTache);
            if (startTache.TacheInfo.IsNextLineDashed)
            {
                //未达到的环节虚线、灰色图标表示
                line.LineStyle = DashStyle.Dash;
                endTache.TacheType = WorkTacheType.Gray;

                if (startTache.TacheInfo.Id == currentTacheId)
                {
                    startTache.TacheType = WorkTacheType.Current;
                }
            }

            //设置环节关联的线
            startTache.FlowLine2 = line;
            endTache.FlowLine1 = line;

            this.panelWorkFlow.Controls.Add(line);

            return line;
        }

        private void panelWorkFlow_MouseDown(object sender, MouseEventArgs e)
        {
            this.panelWorkFlow.Capture = false;
        }

        #endregion

        #region 工作流 Panle 内对象事件交互

        private DateTime preClickTime;
        void wt_MouseClick(object sender, MouseEventArgs e)
        {
            if (isReadOnly)
            {
                return;
            }

            //双击
            if ((DateTime.Now - preClickTime) < TimeSpan.FromSeconds(0.3))
            {
                wt_MouseDoubleClick(sender, e);
            }
            //单击
            else
            {

            }
            preClickTime = DateTime.Now;
        }

        /// <summary>
        /// 双击弹出环节信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wt_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            
        }

        /// <summary>
        /// 选中环节中点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wt_RadioMouseDown(object sender, MouseEventArgs e)
        {
            if (isReadOnly)
            {
                return;
            }

            if (sender is WorkTache)
            {
                WorkTache w = sender as WorkTache;
                selectedObj = w;
                w.Selected = true;

                tempLine = new FlowLine();
                tempLine.SetPoint(w.Left + w.Width / 2, w.Top + w.Height / 2, w.Left + w.Width / 2 + 1, w.Top + w.Height / 2 + 1);
                this.panelWorkFlow.Controls.Add(tempLine);

                Refresh();

                this.panelWorkFlow.Capture = true;
            }
        }

        /// <summary>
        /// 选择表页
        /// </summary>
        public EventHandler MouseDownEvt;

        /// <summary>
        /// 选中环节
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wt_MouseDown(object sender, MouseEventArgs e)
        {
            if (isReadOnly)
            {
                WorkTache w = sender as WorkTache;
                selectedObj = w;
                w.Selected = true;
                Refresh();

                if (MouseDownEvt != null)
                {
                    MouseDownEvt.Invoke(w.TacheInfo as object, new EventArgs());
                }
                return;
            }

            if (sender is WorkTache)
            {
                WorkTache w = sender as WorkTache;
                selectedObj = w;
                w.Selected = true;
                Refresh();
                this.panelWorkFlow.Capture = true;
            }
            mouseDownCursor = e.Location;
        }

        public void SetWorkTacheSelected(int index)
        {
            foreach (Control item in this.panelWorkFlow.Controls)
            {
                if (item is FlowLine)
                {
                    (item as FlowLine).Selected = false;
                }
                else if (item is WorkTache)
                {
                    bool selected = false;
                    WorkTache w = item as WorkTache;
                    if (w.TacheInfo.Index == index && w.TacheInfo.Name != "开始")
                    {
                        selected = true;
                    }
                    (item as WorkTache).Selected = selected;
                }
            }
            base.Refresh();
        }

        /// <summary>
        /// 选中线
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void flowLine_MouseClick(object sender, MouseEventArgs e)
        {
            if (isReadOnly)
            {
                return;
            }

            if (sender is FlowLine)
            {
                selectedObj = sender as FlowLine;
                (sender as FlowLine).Selected = true;
            }
            Refresh();
        }

        /// <summary>
        /// 删除控件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void control_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is Control)
            {
                if ((Keys)e.KeyCode == Keys.Delete)
                {
                    RomveObject();
                }
            }
        }

        #endregion

        #region 公共函数

        /// <summary>
        /// 创建新环节到视图
        /// </summary>
        /// <returns></returns>
        private WorkTache CreateTacheToView()
        {
            WorkTache wt = new WorkTache();
            panelWorkFlow.Controls.Add(wt);
            wt.MouseDown += new MouseEventHandler(wt_MouseDown);
            wt.RadioMouseDown += new MouseEventHandler(wt_RadioMouseDown);
            wt.KeyDown += new KeyEventHandler(control_KeyDown);
            wt.MouseClick += new MouseEventHandler(wt_MouseClick); //双击
            return wt;
        }

        private void SetTacheProperty(WorkTache wt)
        {
            ToolTip tip = new ToolTip();
            tip.SetToolTip(wt, wt.TacheInfo.Tips);
        }


        /// <summary>
        /// 设置环节相对位置
        /// </summary>
        /// <param name="leftTache"></param>
        /// <param name="workTache"></param>
        /// <param name="left"></param>
        private void SetTachePosition(WorkTache leftTache, WorkTache workTache, ref bool left)
        {
            int x = left ? leftTache.Left - leftTache.Width * 2 : leftTache.Left + leftTache.Width * 2;
            int y = leftTache.Top;
            if ((left && x < 0) || (!left && x > panelWorkFlow.Width - leftTache.Width))
            {
                left = !left;
                x = left ? leftTache.Left : 0;
                y = y + (int)(leftTache.Height * 1.5);
            }
            workTache.Left = x;
            workTache.Top = y;
        }

        /// <summary>
        /// 获取工作流
        /// 如果工作流信息不完整，返回null
        /// </summary>
        private List<WorkTacheInfo> GetWorkFlow()
        {
            List<WorkTacheInfo> infos = new List<WorkTacheInfo>();

            //检查开始结束环节
            bool hasStart = false;
            bool hasEnd = false;
            WorkTache startTache = null;
            int normalTache = 0;
            foreach (Control item in panelWorkFlow.Controls)
            {
                if (item is WorkTache)
                {
                    WorkTache wt = item as WorkTache;
                    if (wt.TacheType == WorkTacheType.Begin)
                    {
                        startTache = wt;
                        hasStart = true; continue;
                    }
                    else if (wt.TacheType == WorkTacheType.End)
                    {
                        hasEnd = true; continue;
                    }
                    else if (wt.TacheType == WorkTacheType.Normal)
                    {
                        normalTache++;
                    }
                }
            }

            if (normalTache == 0)
            {
                this.ShowWarning("请至少添加一个环节！");
                return null;
            }

            if (!(hasStart && hasEnd))
            {
                this.ShowWarning("请添加开始和结束环节！");

                return null;
            }

            ConnectTacheToInfo(startTache, ref infos);

            if (normalTache != infos.Count)
            {
                this.ShowWarning("环节未形成完整流程，请检查是否有断开的环节！");

                return null;
            }

            return infos;
        }

        /// <summary>
        /// 连接视图环节
        /// 只连接开始到任何终结的地方
        /// </summary>
        /// <param name="workTache"></param>
        /// <returns></returns>
        private void ConnectTacheToInfo(WorkTache workTache, ref List<WorkTacheInfo> infos)
        {
            if (workTache.TacheType == WorkTacheType.End || workTache.FlowLine2 == null)
            {
                return;
            }
            else
            {
                if (workTache.TacheType != WorkTacheType.Begin)
                {
                    WorkTacheInfo info = new WorkTacheInfo();
                    info.Name = workTache.TacheInfo.Name;
                    info.Id = workTache.TacheInfo.Id;
                    info.Tag = workTache.TacheInfo.Tag;
                    infos.Add(info);
                }
                ConnectTacheToInfo(workTache.FlowLine2.EndControl as WorkTache, ref infos);
            }
        }

        /// <summary>
        /// 设置控件位置(中心点)
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void SetPositon(Control control, int x, int y)
        {
            int w = control.Width;
            int h = control.Height;
            if (x < w / 2)
            {
                x = w / 2;
            }
            else if (x > this.panelWorkFlow.Width - w / 2)
            {
                x = this.panelWorkFlow.Width - w / 2;
            }
            if (y < h / 2)
            {
                y = h / 2;
            }
            else if (y > this.panelWorkFlow.Height - h / 2)
            {
                y = this.panelWorkFlow.Height - h / 2;
            }

            control.Left = x - w / 2;
            control.Top = y - h / 2;
        }

        /// <summary>
        /// 刷新工作流Panle
        /// 1、将没选中的对象设置为 unSelected
        /// </summary>
        public override void Refresh()
        {
            foreach (Control item in this.panelWorkFlow.Controls)
            {
                if (item.Equals(selectedObj))
                {
                    continue;
                }
                if (item is FlowLine)
                {
                    (item as FlowLine).Selected = false;
                }
                else if (item is WorkTache)
                {
                    (item as WorkTache).Selected = false;
                }
            }
            base.Refresh();
        }


        /// <summary>
        /// 点击的位置是否在控件可视范围内
        /// </summary>
        /// <param name="control"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool TestHit(Control control, int x, int y)
        {
            if (control.Left > x || control.Top > y || control.Left + control.Width < x || control.Top + control.Height < y)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取指定范围内的环节
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private WorkTache GetWorkTacheControl(int x, int y)
        {
            foreach (Control item in panelWorkFlow.Controls)
            {
                if (item is WorkTache)
                {
                    if (TestHit(item, x, y))
                    {
                        return item as WorkTache;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="control"></param>
        private void RemoveControl(Control control)
        {
            //如果移除的是环节，则环节关联的线也要删除
            if (control is WorkTache)
            {
                WorkTache workTache = control as WorkTache;
                if (workTache.FlowLine1 != null)
                {
                    RemoveControl(workTache.FlowLine1);
                }
                if (workTache.FlowLine2 != null)
                {
                    RemoveControl(workTache.FlowLine2);
                }
            }
            else if (control is FlowLine)
            {
                DisconnectTache(control as FlowLine);
            }

            this.panelWorkFlow.Controls.Remove(control);
        }


        /// <summary>
        /// 断开环节关联的线
        /// </summary>
        /// <param name="flowLine"></param>
        private void DisconnectTache(FlowLine flowLine)
        {
            if (flowLine.StartControl != null)
            {
                WorkTache workTache = flowLine.StartControl as WorkTache;
                workTache.FlowLine2 = null;
            }

            if (flowLine.EndControl != null)
            {
                WorkTache workTache = flowLine.EndControl as WorkTache;
                workTache.FlowLine1 = null;
            }

        }

        /// <summary>
        /// 是否形成闭环
        /// </summary>
        /// <returns></returns>
        private bool IsClosedLoop(WorkTache startTache, WorkTache endTache)
        {
            if (endTache.FlowLine2 != null)
            {
                if (endTache.FlowLine2.EndControl != null)
                {
                    if (endTache.FlowLine2.EndControl.Equals(startTache))
                    {
                        return true;
                    }
                    else
                    {
                        return IsClosedLoop(startTache, endTache.FlowLine2.EndControl as WorkTache);
                    }

                }
            }
            return false;
        }

        /// <summary>
        /// 获取指定类型的环节
        /// </summary>
        /// <param name="tacheType">环节类型</param>
        /// <returns>环节</returns>
        private WorkTache FirstOrDefault(WorkTacheType tacheType)
        {
            foreach (Control item in panelWorkFlow.Controls)
            {
                if (item is WorkTache)
                {
                    WorkTache workTache = item as WorkTache;
                    if (workTache.TacheType == tacheType)
                    {
                        return workTache;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 获取开始环节
        /// 如果不存在则返回 null
        /// </summary>
        /// <returns></returns>
        private WorkTache GetStartTache()
        {
            foreach (Control item in panelWorkFlow.Controls)
            {
                if (item is WorkTache)
                {
                    WorkTache workTache = item as WorkTache;
                    if (workTache.TacheType == WorkTacheType.Begin)
                    {
                        return workTache;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 自动排列所有节点
        /// </summary>
        public void AutoSortTache()
        {
            sortedTache.Clear();

            WorkTache start = FirstOrDefault(WorkTacheType.Begin);
            bool left = false;
            if (start != null)
            {
                start.Left = 0;
                start.Top = 0;
                SortTache(start, ref left);
            }

            WorkTache end = FirstOrDefault(WorkTacheType.End);
            if (end != null && end.FlowLine1 == null)
            {
                SetTachePosition(lastSortedTache, end, ref left);
            }
        }

        //保存已排列环节的临时变量
        private List<WorkTache> sortedTache = new List<WorkTache>();

        //保存最后一个排列环节的临时变量 用于定位结束环节
        private WorkTache lastSortedTache = new WorkTache();

        /// <summary>
        /// 排列关联的环节
        /// </summary>
        /// <param name="tache">第一个环节</param>
        /// <param name="left">排列方向</param>
        private void SortTache(WorkTache tache, ref bool left)
        {
            WorkTache nextTache = null;
            if (tache.FlowLine2 != null)
            {
                nextTache = tache.FlowLine2.EndControl as WorkTache;
            }
            else
            {
                foreach (Control item in this.panelWorkFlow.Controls)
                {
                    if (item is WorkTache)
                    {
                        WorkTache t = item as WorkTache;
                        if (t.TacheType == WorkTacheType.Normal
                            && !sortedTache.Contains(t)
                            && t.FlowLine1 == null)
                        {
                            nextTache = t;
                            break;
                        }
                    }
                }
            }

            if (nextTache != null)
            {
                sortedTache.Add(nextTache);

                SetTachePosition(tache, nextTache, ref left);
                lastSortedTache = nextTache;
                if (nextTache.FlowLine1 != null)
                {
                    nextTache.FlowLine1.SetPoint();  //定位环节关联的流入的线
                }

                SortTache(nextTache, ref left);
            }
        }

        /// <summary>
        /// 移除控件
        /// </summary>
        private void RomveObject()
        {
            //不准删除开始和结束环节
            if (selectedObj is Control && selectedObj is WorkTache)
            {
                WorkTache wt = selectedObj as WorkTache;
                if (wt.TacheType == WorkTacheType.Begin || wt.TacheType == WorkTacheType.End)
                {
                    this.ShowWarning("不能删除开始或结束环节！");
                    return;
                }

            }

            RemoveControl(selectedObj as Control);
            selectedObj = null;
        }

        #endregion
    }

    /// <summary>
    /// 工作流信息
    /// </summary>
    public class WorkFlowInfo
    {
        public string Id { get; set; }
        public List<WorkTacheInfo> TacheInfos { get; set; }
    }
}
