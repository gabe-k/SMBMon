
namespace SMBMon
{
    partial class MainForm
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
            this.startServerButton = new System.Windows.Forms.Button();
            this.eventsListView = new System.Windows.Forms.ListView();
            this.timeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.handleColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operationColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pathColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.resultColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.detailColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.addFilterButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startServerButton
            // 
            this.startServerButton.Location = new System.Drawing.Point(713, 12);
            this.startServerButton.Name = "startServerButton";
            this.startServerButton.Size = new System.Drawing.Size(75, 23);
            this.startServerButton.TabIndex = 0;
            this.startServerButton.Text = "Start Server";
            this.startServerButton.UseVisualStyleBackColor = true;
            this.startServerButton.Click += new System.EventHandler(this.startServerButton_Click);
            // 
            // eventsListView
            // 
            this.eventsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.timeColumnHeader,
            this.handleColumnHeader,
            this.operationColumnHeader,
            this.pathColumnHeader,
            this.resultColumnHeader,
            this.detailColumnHeader});
            this.eventsListView.FullRowSelect = true;
            this.eventsListView.HideSelection = false;
            this.eventsListView.Location = new System.Drawing.Point(12, 41);
            this.eventsListView.Name = "eventsListView";
            this.eventsListView.Size = new System.Drawing.Size(776, 383);
            this.eventsListView.TabIndex = 1;
            this.eventsListView.UseCompatibleStateImageBehavior = false;
            this.eventsListView.View = System.Windows.Forms.View.Details;
            // 
            // timeColumnHeader
            // 
            this.timeColumnHeader.Text = "Time";
            // 
            // handleColumnHeader
            // 
            this.handleColumnHeader.Text = "Handle";
            // 
            // operationColumnHeader
            // 
            this.operationColumnHeader.Text = "Operation";
            this.operationColumnHeader.Width = 78;
            // 
            // pathColumnHeader
            // 
            this.pathColumnHeader.Text = "Path";
            this.pathColumnHeader.Width = 301;
            // 
            // resultColumnHeader
            // 
            this.resultColumnHeader.Text = "Result";
            this.resultColumnHeader.Width = 92;
            // 
            // detailColumnHeader
            // 
            this.detailColumnHeader.Text = "Detail";
            // 
            // addFilterButton
            // 
            this.addFilterButton.Location = new System.Drawing.Point(632, 12);
            this.addFilterButton.Name = "addFilterButton";
            this.addFilterButton.Size = new System.Drawing.Size(75, 23);
            this.addFilterButton.TabIndex = 2;
            this.addFilterButton.Text = "AddFilter";
            this.addFilterButton.UseVisualStyleBackColor = true;
            this.addFilterButton.Click += new System.EventHandler(this.addFilterButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 436);
            this.Controls.Add(this.addFilterButton);
            this.Controls.Add(this.eventsListView);
            this.Controls.Add(this.startServerButton);
            this.Name = "MainForm";
            this.Text = "SMBMon";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button startServerButton;
        private System.Windows.Forms.ListView eventsListView;
        private System.Windows.Forms.ColumnHeader timeColumnHeader;
        private System.Windows.Forms.ColumnHeader handleColumnHeader;
        private System.Windows.Forms.ColumnHeader operationColumnHeader;
        private System.Windows.Forms.ColumnHeader pathColumnHeader;
        private System.Windows.Forms.ColumnHeader resultColumnHeader;
        private System.Windows.Forms.ColumnHeader detailColumnHeader;
        private System.Windows.Forms.Button addFilterButton;
    }
}

