
namespace SMBMon
{
    partial class FilterEditor
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
            this.operationLabel = new System.Windows.Forms.Label();
            this.operationBox = new System.Windows.Forms.ComboBox();
            this.operandLabel = new System.Windows.Forms.Label();
            this.operandBox = new System.Windows.Forms.ComboBox();
            this.fieldLabel = new System.Windows.Forms.Label();
            this.fieldBox = new System.Windows.Forms.ComboBox();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.valueLabel = new System.Windows.Forms.Label();
            this.filtersListView = new System.Windows.Forms.ListView();
            this.operationHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fieldHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operandHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.valueHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.removeButton = new System.Windows.Forms.Button();
            this.Add = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // operationLabel
            // 
            this.operationLabel.AutoSize = true;
            this.operationLabel.Location = new System.Drawing.Point(12, 9);
            this.operationLabel.Name = "operationLabel";
            this.operationLabel.Size = new System.Drawing.Size(56, 13);
            this.operationLabel.TabIndex = 0;
            this.operationLabel.Text = "Operation:";
            // 
            // operationBox
            // 
            this.operationBox.FormattingEnabled = true;
            this.operationBox.Location = new System.Drawing.Point(12, 25);
            this.operationBox.Name = "operationBox";
            this.operationBox.Size = new System.Drawing.Size(153, 21);
            this.operationBox.TabIndex = 1;
            // 
            // operandLabel
            // 
            this.operandLabel.AutoSize = true;
            this.operandLabel.Location = new System.Drawing.Point(287, 9);
            this.operandLabel.Name = "operandLabel";
            this.operandLabel.Size = new System.Drawing.Size(51, 13);
            this.operandLabel.TabIndex = 2;
            this.operandLabel.Text = "Operand:";
            // 
            // operandBox
            // 
            this.operandBox.FormattingEnabled = true;
            this.operandBox.Location = new System.Drawing.Point(290, 25);
            this.operandBox.Name = "operandBox";
            this.operandBox.Size = new System.Drawing.Size(99, 21);
            this.operandBox.TabIndex = 3;
            // 
            // fieldLabel
            // 
            this.fieldLabel.AutoSize = true;
            this.fieldLabel.Location = new System.Drawing.Point(171, 9);
            this.fieldLabel.Name = "fieldLabel";
            this.fieldLabel.Size = new System.Drawing.Size(29, 13);
            this.fieldLabel.TabIndex = 4;
            this.fieldLabel.Text = "Field";
            // 
            // fieldBox
            // 
            this.fieldBox.FormattingEnabled = true;
            this.fieldBox.Location = new System.Drawing.Point(174, 25);
            this.fieldBox.Name = "fieldBox";
            this.fieldBox.Size = new System.Drawing.Size(110, 21);
            this.fieldBox.TabIndex = 5;
            // 
            // valueTextBox
            // 
            this.valueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueTextBox.Location = new System.Drawing.Point(395, 26);
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(287, 20);
            this.valueTextBox.TabIndex = 6;
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(392, 10);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(37, 13);
            this.valueLabel.TabIndex = 7;
            this.valueLabel.Text = "Value:";
            // 
            // filtersListView
            // 
            this.filtersListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filtersListView.CheckBoxes = true;
            this.filtersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.operationHeader,
            this.fieldHeader,
            this.operandHeader,
            this.valueHeader});
            this.filtersListView.HideSelection = false;
            this.filtersListView.Location = new System.Drawing.Point(12, 81);
            this.filtersListView.Name = "filtersListView";
            this.filtersListView.Size = new System.Drawing.Size(670, 222);
            this.filtersListView.TabIndex = 8;
            this.filtersListView.UseCompatibleStateImageBehavior = false;
            this.filtersListView.View = System.Windows.Forms.View.Details;
            // 
            // operationHeader
            // 
            this.operationHeader.Text = "Operation";
            this.operationHeader.Width = 150;
            // 
            // fieldHeader
            // 
            this.fieldHeader.Text = "Field";
            this.fieldHeader.Width = 100;
            // 
            // operandHeader
            // 
            this.operandHeader.Text = "Operand";
            this.operandHeader.Width = 100;
            // 
            // valueHeader
            // 
            this.valueHeader.Text = "Value";
            this.valueHeader.Width = 240;
            // 
            // removeButton
            // 
            this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.removeButton.Location = new System.Drawing.Point(607, 52);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(75, 23);
            this.removeButton.TabIndex = 9;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            // 
            // Add
            // 
            this.Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Add.Location = new System.Drawing.Point(526, 52);
            this.Add.Name = "Add";
            this.Add.Size = new System.Drawing.Size(75, 23);
            this.Add.TabIndex = 10;
            this.Add.Text = "Add";
            this.Add.UseVisualStyleBackColor = true;
            this.Add.Click += new System.EventHandler(this.Add_Click);
            // 
            // FilterEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 315);
            this.Controls.Add(this.Add);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.filtersListView);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.valueTextBox);
            this.Controls.Add(this.fieldBox);
            this.Controls.Add(this.fieldLabel);
            this.Controls.Add(this.operandBox);
            this.Controls.Add(this.operandLabel);
            this.Controls.Add(this.operationBox);
            this.Controls.Add(this.operationLabel);
            this.Name = "FilterEditor";
            this.Text = "FilterEditor";
            this.Load += new System.EventHandler(this.FilterEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label operationLabel;
        private System.Windows.Forms.ComboBox operationBox;
        private System.Windows.Forms.Label operandLabel;
        private System.Windows.Forms.ComboBox operandBox;
        private System.Windows.Forms.Label fieldLabel;
        private System.Windows.Forms.ComboBox fieldBox;
        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.Label valueLabel;
        private System.Windows.Forms.ListView filtersListView;
        private System.Windows.Forms.ColumnHeader operationHeader;
        private System.Windows.Forms.ColumnHeader fieldHeader;
        private System.Windows.Forms.ColumnHeader operandHeader;
        private System.Windows.Forms.ColumnHeader valueHeader;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button Add;
    }
}