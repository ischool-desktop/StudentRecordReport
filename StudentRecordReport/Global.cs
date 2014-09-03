using FISCA.UDT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentRecordReport
{
    public class Global
    {
        public static AccessHelper _A = new AccessHelper();
        public static Dictionary<string, List<string>> AbsenceSetDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// 假別設定初始化
        /// </summary>
        /// <returns></returns>
        public static void AbsenceSetDicInit()
        {
            AbsenceSetDic.Clear();

            AbsenceSetDic.Add("事病假", new List<string>());
            AbsenceSetDic.Add("曠課", new List<string>());
            AbsenceSetDic.Add("遲到", new List<string>());

            foreach (AbsenceUDT au in _A.Select<AbsenceUDT>())
            {
                if (AbsenceSetDic.ContainsKey(au.Target) && !AbsenceSetDic[au.Target].Contains(au.Source))
                    AbsenceSetDic[au.Target].Add(au.Source);
            }
        }
    }
}
