using Aspose.Cells;
using FISCA.Presentation.Controls;
using K12.BusinessLogic;
using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentRecordReport
{
    public partial class Reporter : BaseForm
    {
        BackgroundWorker _BW;

        private List<string> _Students;
        private bool _IsLowGrade;

        public Reporter(List<string> students)
        {
            InitializeComponent();

            this._Students = students;
            this._IsLowGrade = true;

            cboSelectTemp.Items.Add("1~6年級");
            cboSelectTemp.Items.Add("7~12年級");

            cboSelectTemp.Text = cboSelectTemp.Items[0] + "";

            _BW = new BackgroundWorker();
            _BW.DoWork += new DoWorkEventHandler(_BW_DoWork);
            _BW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_BW_RunWorkerCompleted);
        }

        private void _BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FISCA.Presentation.MotherForm.SetStatusBarMessage("學籍表列印完成");
            FormLock(true);
            Workbook wb = e.Result as Workbook;

            if (wb == null)
                return;

            SaveFileDialog save = new SaveFileDialog();
            save.Title = "另存新檔";
            save.FileName = "學籍表.xls";
            save.Filter = "Excel檔案 (*.xls)|*.xls|所有檔案 (*.*)|*.*";

            if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    wb.Save(save.FileName, Aspose.Cells.SaveFormat.Excel97To2003);
                    System.Diagnostics.Process.Start(save.FileName);
                }
                catch
                {
                    MessageBox.Show("檔案儲存失敗");
                }
            }
        }

        private void _BW_DoWork(object sender, DoWorkEventArgs e)
        {
            //取得學生資料
            Dictionary<string, StudentObj> StudentDic = Data.Get(_Students);

            if (StudentDic == null)
                return;

            //開始列印
            Workbook wb = new Workbook(new MemoryStream(Properties.Resources.template));

            foreach (string id in StudentDic.Keys)
            {
                StudentObj student = StudentDic[id];

                wb.Worksheets.AddCopy(0);
                wb.Worksheets[wb.Worksheets.Count - 1].Name = student.StudentRecord.Name + id;

                Cells cs = wb.Worksheets[wb.Worksheets.Count - 1].Cells;

                //列印差異標題
                if (_IsLowGrade)
                {
                    cs[0, 16].PutValue("學生學籍記錄表(一~六年級)");
                    cs[3, 13].PutValue("Bilingual Department Transcript of Records(Grade 1~6)");
                }
                else
                {
                    cs[0, 16].PutValue("學生學籍記錄表(七~十二年級)");
                    cs[3, 13].PutValue("Bilingual Department Transcript of Records(Grade 7~12)");
                }

                //列印學生基本資料
                cs[0, 2].PutValue(student.StudentRecord.StudentNumber);
                cs[2, 2].PutValue(student.StudentRecord.Name);
                cs[2, 5].PutValue(student.StudentRecord.EnglishName);
                cs[4, 2].PutValue(student.StudentRecord.Birthday.HasValue ? student.StudentRecord.Birthday.Value.ToShortDateString() : "");
                cs[4, 9].PutValue(student.ParentRecord.CustodianName);
                cs[4, 14].PutValue(student.ParentRecord.CustodianRelationship);
                cs[4, 17].PutValue(student.AddressRecord.MailingAddress);
                cs[5, 2].PutValue(student.StudentRecord.Gender);
                cs[5, 11].PutValue(student.EntranceRecord == null ? string.Empty : student.EntranceRecord.ADDate);
                cs[5, 15].PutValue(student.Entrance == null ? "西元        年        月        日" : student.Entrance.Value.ToString("西元  yyyy  年   MM   月   dd   日"));
                cs[5, 22].PutValue(student.Leaving == null ? "西元        年        月        日" : student.Leaving.Value.ToString("西元  yyyy  年   MM   月   dd   日"));
                cs[6, 2].PutValue(student.StudentRecord.Nationality);
                cs[6, 11].PutValue(student.EntranceRecord == null ? string.Empty : student.EntranceRecord.ADNumber);

                int startGrade = 1;
                int endGrade = 6;
                if (!_IsLowGrade)
                {
                    startGrade = 7;
                    endGrade = 12;
                }

                int columnIndex = 3;
                //列印指定年級資料
                for (int i = startGrade; i <= endGrade; i++)
                {
                    int schoolYear = student.GradeToSchoolYear[i];
                    //學年度標題
                    cs[8, columnIndex].PutValue(string.Format("{0}年級({1}學年度)", i, schoolYear == 0 ? "  " : schoolYear + ""));
                    cs[9, columnIndex].PutValue(string.Format("Gr.{0}Sch.  Yr.{1}-{2}", i, schoolYear == 0 ? "20__" : schoolYear + 1911 + "", schoolYear == 0 ? "20__" : schoolYear + 1912 + ""));

                    //科目成績
                    int rowIndex = 11;
                    List<string> subjectList = i <= 6 ? student.SubjectList6 : student.SubjectList12;
                    foreach (string subj in subjectList)
                    {
                        cs[rowIndex, 1].PutValue(subj);
                        string key = schoolYear + "_" + subj;
                        SubjectObj so = student.SubjectObjDic.ContainsKey(key) ? student.SubjectObjDic[key] : new SubjectObj();
                        cs[rowIndex, columnIndex].PutValue(so.GetPeriod() + "");
                        cs[rowIndex, columnIndex + 1].PutValue(so.MidtemScore + "");
                        cs[rowIndex, columnIndex + 2].PutValue(so.FinalScore + "");
                        cs[rowIndex, columnIndex + 3].PutValue(so.GetAvg() + "");

                        rowIndex++;
                    }

                    //學期平均
                    string syKey = schoolYear + "";
                    if (student.AvgObjDic.ContainsKey(syKey))
                    {
                        cs[26, columnIndex + 1].PutValue(student.AvgObjDic[syKey].MidtermAvg + "");
                        cs[26, columnIndex + 2].PutValue(student.AvgObjDic[syKey].FinalAvg + "");
                        cs[26, columnIndex + 3].PutValue(student.AvgObjDic[syKey].GetAvg() + "");
                    }

                    //出缺勤
                    rowIndex = 27;
                    string key1 = schoolYear + "_1";
                    if (student.AttendanceObjDic.ContainsKey(key1))
                    {
                        AttendanceObj ao = student.AttendanceObjDic[key1];
                        cs[rowIndex, columnIndex + 1].PutValue(ao.TotalDay == 0 ? "" : ao.TotalDay + "");
                        cs[rowIndex + 1, columnIndex + 1].PutValue(ao.Excused == 0 ? "" : ao.Excused + "");
                        cs[rowIndex + 2, columnIndex + 1].PutValue(ao.Unexcused == 0 ? "" : ao.Unexcused + "");
                        int totalAbsence = ao.Excused + ao.Unexcused;
                        cs[rowIndex + 3, columnIndex + 1].PutValue(totalAbsence == 0 ? "" : totalAbsence + "");
                        cs[rowIndex + 4, columnIndex + 1].PutValue(ao.Tardy == 0 ? "" : ao.Tardy + "");
                    }
                    string key2 = schoolYear + "_2";
                    if (student.AttendanceObjDic.ContainsKey(key2))
                    {
                        AttendanceObj ao = student.AttendanceObjDic[key2];
                        cs[rowIndex, columnIndex + 2].PutValue(ao.TotalDay == 0 ? "" : ao.TotalDay + "");
                        cs[rowIndex + 1, columnIndex + 2].PutValue(ao.Excused == 0 ? "" : ao.Excused + "");
                        cs[rowIndex + 2, columnIndex + 2].PutValue(ao.Unexcused == 0 ? "" : ao.Unexcused + "");
                        int totalAbsence = ao.Excused + ao.Unexcused;
                        cs[rowIndex + 3, columnIndex + 2].PutValue(totalAbsence == 0 ? "" : totalAbsence + "");
                        cs[rowIndex + 4, columnIndex + 2].PutValue(ao.Tardy == 0 ? "" : ao.Tardy + "");
                    }

                    columnIndex += 4;
                }
            }

            wb.Worksheets.RemoveAt(0);
            wb.Worksheets.ActiveSheetIndex = 0;
            e.Result = wb;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cboSelectTemp.Text == "1~6年級")
                _IsLowGrade = true;
            else
                _IsLowGrade = false;

            FormLock(false);

            if (_BW.IsBusy)
                MessageBox.Show("系統忙碌,請稍後再試...");
            else
                _BW.RunWorkerAsync();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            new SetForm().ShowDialog();
        }

        private void FormLock(bool b)
        {
            this.link.Enabled = b;
            this.cboSelectTemp.Enabled = b;
        }
    }
}
