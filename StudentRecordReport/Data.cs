using K12.BusinessLogic;
using K12.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentRecordReport
{
    public class Data
    {
        public static  Dictionary<string, StudentObj> Get(List<string> studentIDs)
        {
            try
            {
                Dictionary<string, StudentObj> StudentDic = new Dictionary<string, StudentObj>();

                //學生基本資料
                foreach (StudentRecord sr in K12.Data.Student.SelectByIDs(studentIDs))
                {
                    if (!StudentDic.ContainsKey(sr.ID))
                        StudentDic.Add(sr.ID, new StudentObj(sr));
                }

                //學生地址
                foreach (AddressRecord ar in K12.Data.Address.SelectByStudentIDs(studentIDs))
                {
                    StudentDic[ar.RefStudentID].AddressRecord = ar;
                }

                //學生監護人
                foreach (ParentRecord pr in K12.Data.Parent.SelectByStudentIDs(studentIDs))
                {
                    StudentDic[pr.RefStudentID].ParentRecord = pr;
                }

                //前級畢業資訊
                foreach (BeforeEnrollmentRecord ber in K12.Data.BeforeEnrollment.SelectByStudentIDs(studentIDs))
                {
                    StudentDic[ber.RefStudentID].BeforeEnrollmentRecord = ber;
                }

                //學生學期歷程
                foreach (SemesterHistoryRecord shr in K12.Data.SemesterHistory.SelectByStudentIDs(studentIDs))
                {
                    foreach (SemesterHistoryItem item in shr.SemesterHistoryItems)
                    {
                        StudentDic[item.RefStudentID].SetHistory(item);
                    }
                }

                //取得學期成績
                foreach (SemesterScoreRecord ssr in K12.Data.SemesterScore.SelectByStudentIDs(studentIDs))
                {
                    StudentDic[ssr.RefStudentID].SetSubjects(ssr);
                }

                //假別設定初始化(在SetAttendance之前執行)
                Global.AbsenceSetDicInit();

                //取得缺曠
                foreach (AutoSummaryRecord asr in AutoSummary.Select(studentIDs, null))
                {
                    StudentDic[asr.RefStudentID].SetAttendance(asr);
                }

                return StudentDic;
            }
            catch (Exception e)
            {
                MessageBox.Show("取得資料過程發生錯誤:" + e);
                return null;
            }
        }
        
    }
}
