using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMBMon
{
    public partial class FilterEditor : Form
    {
        public FilterEditor()
        {
            InitializeComponent();
        }

        private void FilterEditor_Load(object sender, EventArgs e)
        {
            // populate the filter list
            foreach(SMBFilter filter in Program.FilteredFileSystem.GetFilters())
            {
                string[] columns = { filter.Operation.ToString(), filter.Clause.field.ToString(), filter.Clause.operand.ToString(), filter.Clause.valueString };
                filtersListView.Items.Add(new ListViewItem(columns));
                filtersListView.Items[filtersListView.Items.Count - 1].Checked = filter.Enabled;
            }

            // populate the operations box
            Dictionary<int, string> operationsMap = new Dictionary<int, string>();
            foreach (NTFileOperation operation in Enum.GetValues(typeof(NTFileOperation)))
            {
                operationsMap.Add((int)operation, operation.ToString());
            }
            operationBox.ValueMember = "Key";
            operationBox.DisplayMember = "Value";
            operationBox.DataSource = operationsMap.ToList();

            // populate the field box
            Dictionary<int, string> fieldsMap = new Dictionary<int, string>();
            foreach (FilterField field in Enum.GetValues(typeof(FilterField)))
            {
                fieldsMap.Add((int)field, field.ToString());
            }
            fieldBox.ValueMember = "Key";
            fieldBox.DisplayMember = "Value";
            fieldBox.DataSource = fieldsMap.ToList();

            // populate the operand box
            Dictionary<int, string> operandsMap = new Dictionary<int, string>();
            foreach (FilterOperand operand in Enum.GetValues(typeof(FilterOperand)))
            {
                operandsMap.Add((int)operand, operand.ToString());
            }
            operandBox.ValueMember = "Key";
            operandBox.DisplayMember = "Value";
            operandBox.DataSource = operandsMap.ToList();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            SMBFilterClause clause = new SMBFilterClause((FilterField)fieldBox.SelectedValue, (FilterOperand)operandBox.SelectedValue, valueTextBox.Text);
            SMBFilter filter = new SMBFilter((NTFileOperation)operationBox.SelectedValue, FilterAction.Log, true);
        }
    }
}
