using K12.BusinessLogic;
using K12.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentRecordReport
{
    public class StudentObj
    {
        public StudentRecord StudentRecord;
        private AddressRecord addressRecord;
        private ParentRecord parentRecord;
        private BeforeEnrollmentRecord beforeEnrollmentRecord;
        public Dictionary<int, int> GradeToSchoolYear;
        public Dictionary<int, int> SchoolYearToGrade;
        public Dictionary<string, SubjectObj> SubjectObjDic;
        public Dictionary<string, AvgObj> AvgObjDic;
        public Dictionary<string, AttendanceObj> AttendanceObjDic;
        public List<string> SubjectList6;
        public List<string> SubjectList12;
        public DateTime? Entrance;
        public DateTime? Leaving;
        public UpdateRecordRecord EntranceRecord;

        public StudentObj(StudentRecord sr)
        {
            this.StudentRecord = sr;

            GradeToSchoolYear = new Dictionary<int, int>() {
            {1,0},{2,0},{3,0},{4,0},{5,0},{6,0},{7,0},{8,0},{9,0},{10,0},{11,0},{12,0}};

            SchoolYearToGrade = new Dictionary<int, int>();
            SubjectObjDic = new Dictionary<string, SubjectObj>();
            AvgObjDic = new Dictionary<string, AvgObj>();
            AttendanceObjDic = new Dictionary<string, AttendanceObj>();
            SubjectList6 = new List<string>();
            SubjectList12 = new List<string>();
        }

        /// <summary>
        /// 設定入學時間
        /// </summary>
        /// <param name="urr"></param>
        public void SetEntrance(UpdateRecordRecord urr)
        {
            if (Entrance == null)
                Entrance = new DateTime();

            DateTime dt = new DateTime();
            if(DateTime.TryParse(urr.UpdateDate,out dt))
            {
                if (dt > Entrance)
                {
                    Entrance = dt;
                    EntranceRecord = urr;
                }
            }
        }

        /// <summary>
        /// 設定離校時間
        /// </summary>
        /// <param name="urr"></param>
        public void SetLeaving(UpdateRecordRecord urr)
        {
            if (Leaving == null)
                Leaving = new DateTime();

            DateTime dt = new DateTime();
            if (DateTime.TryParse(urr.UpdateDate, out dt))
            {
                if (dt > Leaving)
                    Leaving = dt;
            }
        }

        /// <summary>
        /// 地址
        /// </summary>
        public AddressRecord AddressRecord
        {
            get
            {
                if (addressRecord == null)
                    addressRecord = new AddressRecord();

                return addressRecord;
            }
            set
            {
                addressRecord = value;
            }
        }

        /// <summary>
        /// 監護人
        /// </summary>
        public ParentRecord ParentRecord
        {
            get
            {
                if (parentRecord == null)
                    parentRecord = new ParentRecord();

                return parentRecord;
            }
            set
            {
                parentRecord = value;
            }
        }

        /// <summary>
        /// 前級畢業資訊
        /// </summary>
        public BeforeEnrollmentRecord BeforeEnrollmentRecord
        {
            get
            {
                if (beforeEnrollmentRecord == null)
                    beforeEnrollmentRecord = new BeforeEnrollmentRecord();

                return beforeEnrollmentRecord;
            }
            set
            {
                beforeEnrollmentRecord = value;
            }
        }

        /// <summary>
        /// 設定學期歷程對照
        /// </summary>
        /// <param name="item"></param>
        public void SetHistory(SemesterHistoryItem item)
        {
            int grade = item.GradeYear;
            int new_schoolYear = item.SchoolYear;

            //年級對照學年度:相同年級的學年度,取學年度較高者
            if (GradeToSchoolYear.ContainsKey(grade))
            {
                int old_schoolYear = GradeToSchoolYear[grade];

                if (new_schoolYear > old_schoolYear)
                    GradeToSchoolYear[grade] = new_schoolYear;
            }

            //學年度對照年級:相同學年度的年級,取年級較高者
            if (!SchoolYearToGrade.ContainsKey(new_schoolYear))
                SchoolYearToGrade.Add(new_schoolYear, grade);
            else if(grade > SchoolYearToGrade[new_schoolYear])
                SchoolYearToGrade[new_schoolYear] = grade;

            //上課天數
            string key = item.SchoolYear + "_" + item.Semester;
            int dayCount = item.SchoolDayCount.HasValue ? item.SchoolDayCount.Value : 0;
            if (!AttendanceObjDic.ContainsKey(key))
                AttendanceObjDic.Add(key, new AttendanceObj(dayCount));
        }

        /// <summary>
        /// 設定學期成績
        /// </summary>
        /// <param name="ssr"></param>
        public void SetSubjects(SemesterScoreRecord ssr)
        {
            //各科目學期成績(obj version)
            foreach (string subj in ssr.Subjects.Keys)
            {
                string subjKey = ssr.SchoolYear + "_" + subj;

                if (!SubjectObjDic.ContainsKey(subjKey))
                    SubjectObjDic.Add(subjKey, new SubjectObj());

                SubjectObjDic[subjKey].LoadData(ssr.Subjects[subj]);

                //將科目清單分類
                if (SchoolYearToGrade.ContainsKey(ssr.SchoolYear))
                {
                    int grade = SchoolYearToGrade[ssr.SchoolYear];
                    if (grade <= 6)
                    {
                        if (!SubjectList6.Contains(subj))
                            SubjectList6.Add(subj);
                    }
                    else
                    {
                        if (!SubjectList12.Contains(subj))
                            SubjectList12.Add(subj);
                    }
                }
            }

            //排序科目清單
            SortSubjects();

            //各學期平均成績
            string key = ssr.SchoolYear + "";
            if (!AvgObjDic.ContainsKey(key))
                AvgObjDic.Add(key, new AvgObj());

            AvgObjDic[key].LoadData(ssr);
        }

        public void SetAttendance(AutoSummaryRecord asr)
        {
            string key = asr.SchoolYear + "_" + asr.Semester;

            if (!AttendanceObjDic.ContainsKey(key))
                AttendanceObjDic.Add(key, new AttendanceObj());

            AttendanceObj ao = AttendanceObjDic[key];
            foreach (AbsenceCountRecord acr in asr.AbsenceCounts)
            {
                if (Global.AbsenceSetDic["事病假"].Contains(acr.Name))
                    ao.Excused += acr.Count;
                if (Global.AbsenceSetDic["曠課"].Contains(acr.Name))
                    ao.Unexcused += acr.Count;
                if (Global.AbsenceSetDic["遲到"].Contains(acr.Name))
                    ao.Tardy += acr.Count;
            }
        }

        public void SortSubjects()
        {
            SubjectList6.Sort(delegate(string x, string y)
            {
                string xx = x.PadLeft(50, '0');
                string yy = y.PadLeft(50, '0');
                return xx.CompareTo(yy);
            });

            SubjectList12.Sort(delegate(string x, string y)
            {
                string xx = x.PadLeft(50, '0');
                string yy = y.PadLeft(50, '0');
                return xx.CompareTo(yy);
            });
        }
    }
}
