namespace South.Hosse.Controls
{
    partial class WorkFlow
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panelWorkFlow = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panelWorkFlow
            // 
            this.panelWorkFlow.AutoScroll = true;
            this.panelWorkFlow.BackColor = System.Drawing.SystemColors.Window;
            this.panelWorkFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWorkFlow.Location = new System.Drawing.Point(0, 0);
            this.panelWorkFlow.Name = "panelWorkFlow";
            this.panelWorkFlow.Size = new System.Drawing.Size(789, 425);
            this.panelWorkFlow.TabIndex = 3;
            this.panelWorkFlow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelWorkFlow_MouseDown);
            this.panelWorkFlow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelWorkFlow_MouseMove);
            this.panelWorkFlow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelWorkFlow_MouseUp);
            // 
            // WorkFlow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.panelWorkFlow);
            this.Name = "WorkFlow";
            this.Size = new System.Drawing.Size(789, 425);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelWorkFlow;
    }
}
