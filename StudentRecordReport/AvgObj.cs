using K12.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudentRecordReport
{
    public class AvgObj
    {
        public decimal? MidtermAvg;
        public decimal? FinalAvg;

        /// <summary>
        /// 讀取學期成績AvgScore
        /// </summary>
        /// <param name="ssr"></param>
        public void LoadData(SemesterScoreRecord ssr)
        {
            if (ssr.Semester == 1)
            {
                MidtermAvg = ssr.AvgScore;
            } 
            else if(ssr.Semester == 2)
            {
                FinalAvg = ssr.AvgScore;
            }
        }

        /// <summary>
        /// 取得上下學期的學期平均總成績
        /// </summary>
        /// <returns></returns>
        public decimal? GetAvg()
        {
            decimal? avg = null;
            decimal sum = 0;
            int count = 0;

            if (MidtermAvg.HasValue)
            {
                sum += MidtermAvg.Value;
                count++;
            }

            if (FinalAvg.HasValue)
            {
                sum += FinalAvg.Value;
                count++;
            }

            if (count > 0)
                avg = Math.Round(sum / count, 2, MidpointRounding.AwayFromZero);

            return avg;
        }
    }
}
