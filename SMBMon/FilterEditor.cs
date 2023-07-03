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
    }
}
