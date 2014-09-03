using FISCA.Presentation.Controls;
using FISCA.UDT;
using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StudentRecordReport
{
    public partial class SetForm : BaseForm
    {
        AccessHelper _A;
        List<AbsenceUDT> _auList;

        public SetForm()
        {
            InitializeComponent();
            _A = new AccessHelper();

            colTarget.Items.Add("事病假");
            colTarget.Items.Add("曠課");
            colTarget.Items.Add("遲到");

            //取得全部假別
            foreach (AbsenceMappingInfo info in AbsenceMapping.SelectAll())
                colSource.Items.Add(info.Name);

            //排序
            _auList = _A.Select<AbsenceUDT>();
            _auList.Sort(delegate(AbsenceUDT x, AbsenceUDT y)
            {
                string xx = x.Target.PadLeft(20, '0');
                xx += x.Source.PadLeft(20, '0');
                string yy = y.Target.PadLeft(20, '0');
                yy += y.Source.PadLeft(20, '0');
                return xx.CompareTo(yy);
            });

            foreach (AbsenceUDT au in _auList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dgv, au.Target, au.Source);
                dgv.Rows.Add(row);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Dictionary<string, AbsenceUDT> auDic = new Dictionary<string, AbsenceUDT>();

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow)
                    continue;

                string target = row.Cells[colTarget.Index].Value + "";
                string source = row.Cells[colSource.Index].Value + "";
                string key = target + "_" + source;

                if (!auDic.ContainsKey(key))
                {
                    AbsenceUDT au = new AbsenceUDT();
                    au.Target = target;
                    au.Source = source;
                    auDic.Add(key, au);
                }
            }

            if (auDic.Count > 0)
            {
                _A.DeletedValues(_auList);

                _A.InsertValues(auDic.Values);
            }

            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
