namespace controller
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.projectNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.priceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalRequireDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.finishQuantityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.remainsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.backgroundNoDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isRestrictDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.backgroundAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.downloadAddressDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.refreshDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.voteProjectBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.voteProjectBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "掉线";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "活跃";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(36, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 14);
            this.label3.TabIndex = 2;
            this.label3.Text = "无";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.label4.Location = new System.Drawing.Point(36, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 14);
            this.label4.TabIndex = 3;
            this.label4.Text = "无";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.projectNameDataGridViewTextBoxColumn,
            this.priceDataGridViewTextBoxColumn,
            this.totalRequireDataGridViewTextBoxColumn,
            this.finishQuantityDataGridViewTextBoxColumn,
            this.remainsDataGridViewTextBoxColumn,
            this.backgroundNoDataGridViewTextBoxColumn,
            this.idTypeDataGridViewTextBoxColumn,
            this.isRestrictDataGridViewCheckBoxColumn,
            this.backgroundAddressDataGridViewTextBoxColumn,
            this.downloadAddressDataGridViewTextBoxColumn,
            this.refreshDateDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.voteProjectBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(3, 60);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(546, 272);
            this.dataGridView1.TabIndex = 4;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // projectNameDataGridViewTextBoxColumn
            // 
            this.projectNameDataGridViewTextBoxColumn.DataPropertyName = "ProjectName";
            this.projectNameDataGridViewTextBoxColumn.HeaderText = "项目";
            this.projectNameDataGridViewTextBoxColumn.Name = "projectNameDataGridViewTextBoxColumn";
            this.projectNameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // priceDataGridViewTextBoxColumn
            // 
            this.priceDataGridViewTextBoxColumn.DataPropertyName = "Price";
            this.priceDataGridViewTextBoxColumn.FillWeight = 55F;
            this.priceDataGridViewTextBoxColumn.HeaderText = "价格";
            this.priceDataGridViewTextBoxColumn.Name = "priceDataGridViewTextBoxColumn";
            this.priceDataGridViewTextBoxColumn.ReadOnly = true;
            this.priceDataGridViewTextBoxColumn.Width = 55;
            // 
            // totalRequireDataGridViewTextBoxColumn
            // 
            this.totalRequireDataGridViewTextBoxColumn.DataPropertyName = "TotalRequire";
            this.totalRequireDataGridViewTextBoxColumn.HeaderText = "总数";
            this.totalRequireDataGridViewTextBoxColumn.Name = "totalRequireDataGridViewTextBoxColumn";
            this.totalRequireDataGridViewTextBoxColumn.ReadOnly = true;
            this.totalRequireDataGridViewTextBoxColumn.Visible = false;
            // 
            // finishQuantityDataGridViewTextBoxColumn
            // 
            this.finishQuantityDataGridViewTextBoxColumn.DataPropertyName = "FinishQuantity";
            this.finishQuantityDataGridViewTextBoxColumn.HeaderText = "完成";
            this.finishQuantityDataGridViewTextBoxColumn.Name = "finishQuantityDataGridViewTextBoxColumn";
            this.finishQuantityDataGridViewTextBoxColumn.ReadOnly = true;
            this.finishQuantityDataGridViewTextBoxColumn.Visible = false;
            // 
            // remainsDataGridViewTextBoxColumn
            // 
            this.remainsDataGridViewTextBoxColumn.DataPropertyName = "Remains";
            this.remainsDataGridViewTextBoxColumn.FillWeight = 80F;
            this.remainsDataGridViewTextBoxColumn.HeaderText = "剩余";
            this.remainsDataGridViewTextBoxColumn.Name = "remainsDataGridViewTextBoxColumn";
            this.remainsDataGridViewTextBoxColumn.ReadOnly = true;
            this.remainsDataGridViewTextBoxColumn.Width = 80;
            // 
            // backgroundNoDataGridViewTextBoxColumn
            // 
            this.backgroundNoDataGridViewTextBoxColumn.DataPropertyName = "BackgroundNo";
            this.backgroundNoDataGridViewTextBoxColumn.HeaderText = "后台";
            this.backgroundNoDataGridViewTextBoxColumn.Name = "backgroundNoDataGridViewTextBoxColumn";
            this.backgroundNoDataGridViewTextBoxColumn.ReadOnly = true;
            this.backgroundNoDataGridViewTextBoxColumn.Visible = false;
            // 
            // idTypeDataGridViewTextBoxColumn
            // 
            this.idTypeDataGridViewTextBoxColumn.DataPropertyName = "IdType";
            this.idTypeDataGridViewTextBoxColumn.FillWeight = 55F;
            this.idTypeDataGridViewTextBoxColumn.HeaderText = "平台";
            this.idTypeDataGridViewTextBoxColumn.Name = "idTypeDataGridViewTextBoxColumn";
            this.idTypeDataGridViewTextBoxColumn.ReadOnly = true;
            this.idTypeDataGridViewTextBoxColumn.Width = 55;
            // 
            // isRestrictDataGridViewCheckBoxColumn
            // 
            this.isRestrictDataGridViewCheckBoxColumn.DataPropertyName = "IsRestrict";
            this.isRestrictDataGridViewCheckBoxColumn.FillWeight = 55F;
            this.isRestrictDataGridViewCheckBoxColumn.HeaderText = "限人";
            this.isRestrictDataGridViewCheckBoxColumn.Name = "isRestrictDataGridViewCheckBoxColumn";
            this.isRestrictDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isRestrictDataGridViewCheckBoxColumn.Width = 55;
            // 
            // backgroundAddressDataGridViewTextBoxColumn
            // 
            this.backgroundAddressDataGridViewTextBoxColumn.DataPropertyName = "BackgroundAddress";
            this.backgroundAddressDataGridViewTextBoxColumn.HeaderText = "后台地址";
            this.backgroundAddressDataGridViewTextBoxColumn.Name = "backgroundAddressDataGridViewTextBoxColumn";
            this.backgroundAddressDataGridViewTextBoxColumn.ReadOnly = true;
            this.backgroundAddressDataGridViewTextBoxColumn.Visible = false;
            // 
            // downloadAddressDataGridViewTextBoxColumn
            // 
            this.downloadAddressDataGridViewTextBoxColumn.DataPropertyName = "DownloadAddress";
            this.downloadAddressDataGridViewTextBoxColumn.HeaderText = "下载地址";
            this.downloadAddressDataGridViewTextBoxColumn.Name = "downloadAddressDataGridViewTextBoxColumn";
            this.downloadAddressDataGridViewTextBoxColumn.ReadOnly = true;
            this.downloadAddressDataGridViewTextBoxColumn.Visible = false;
            // 
            // refreshDateDataGridViewTextBoxColumn
            // 
            this.refreshDateDataGridViewTextBoxColumn.DataPropertyName = "RefreshDate";
            this.refreshDateDataGridViewTextBoxColumn.FillWeight = 130F;
            this.refreshDateDataGridViewTextBoxColumn.HeaderText = "时间";
            this.refreshDateDataGridViewTextBoxColumn.Name = "refreshDateDataGridViewTextBoxColumn";
            this.refreshDateDataGridViewTextBoxColumn.ReadOnly = true;
            this.refreshDateDataGridViewTextBoxColumn.Width = 130;
            // 
            // voteProjectBindingSource
            // 
            this.voteProjectBindingSource.DataSource = typeof(controller.util.VoteProject);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(474, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "取消置顶";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(550, 333);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form3";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "实时监控";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.voteProjectBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource voteProjectBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn projectNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn priceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalRequireDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn finishQuantityDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn remainsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn backgroundNoDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn idTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isRestrictDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn backgroundAddressDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn downloadAddressDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn refreshDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.Button button1;
    }
}