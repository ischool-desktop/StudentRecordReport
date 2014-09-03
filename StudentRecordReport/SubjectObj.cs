using K12.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentRecordReport
{
    public class SubjectObj
    {
        public decimal? MidtemScore;
        public decimal? FinalScore;
        public decimal? MidtemPeriod;
        public decimal? FinalPeriod;

        /// <summary>
        /// 讀取學期科目成績資料
        /// </summary>
        /// <param name="ss"></param>
        public void LoadData(SubjectScore ss)
        {
            if (ss.Semester == 1)
            {
                MidtemScore = ss.Score;
                MidtemPeriod = ss.Period;
            }
            else if (ss.Semester == 2)
            {
                FinalScore = ss.Score;
                FinalPeriod = ss.Period;
            }
        }

        /// <summary>
        /// 取得科目時數
        /// </summary>
        /// <returns></returns>
        public decimal? GetPeriod()
        {
            //先取後者
            if (FinalPeriod.HasValue)
                return FinalPeriod.Value;
            else if (MidtemPeriod.HasValue)
                return MidtemPeriod.Value;
            else
                return null;
        }

        /// <summary>
        /// 取得該科目的學期平均
        /// </summary>
        /// <returns></returns>
        public decimal? GetAvg()
        {
            decimal? avg = null;
            decimal sum = 0;
            int count = 0;

            if (MidtemScore.HasValue)
            {
                sum += MidtemScore.Value;
                count++;
            }

            if (FinalScore.HasValue)
            {
                sum += FinalScore.Value;
                count++;
            }

            if (count > 0)
                avg = Math.Round(sum / count, 2, MidpointRounding.AwayFromZero);

            return avg;
        }
    }
}
