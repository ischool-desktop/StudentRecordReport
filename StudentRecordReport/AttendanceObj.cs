using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentRecordReport
{
    public class AttendanceObj
    {
        public int TotalDay;
        public int Excused;
        public int Unexcused;
        public int Tardy;

        public AttendanceObj(int count)
        {
            this.TotalDay = count;
        }

        public AttendanceObj()
        {
        }
    }
}
