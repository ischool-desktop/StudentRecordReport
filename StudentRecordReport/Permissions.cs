using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentRecordReport
{
    class Permissions
    {
        public static string StudentRecordReport { get { return "StudentRecordReport.BE575725-8BE4-4EA2-9BF6-7AE22CF080A0"; } }

        public static bool StudentRecordReport權限
        {
            get { return FISCA.Permission.UserAcl.Current[StudentRecordReport].Executable; }
        }
    }
}
