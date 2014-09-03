using FISCA.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentRecordReport
{
    public class Program
    {
        [FISCA.MainMethod]
        public static void main()
        {
            FISCA.Presentation.RibbonBarItem item1 = FISCA.Presentation.MotherForm.RibbonBarItems["學生", "資料統計"];
            item1["報表"]["學籍相關報表"]["學籍表(雙語部)"].Enable = false;
            item1["報表"]["學籍相關報表"]["學籍表(雙語部)"].Click += delegate
            {
                new Reporter(K12.Presentation.NLDPanels.Student.SelectedSource).ShowDialog();
            };

            K12.Presentation.NLDPanels.Student.SelectedSourceChanged += delegate
            {
                item1["報表"]["學籍相關報表"]["學籍表(雙語部)"].Enable = K12.Presentation.NLDPanels.Student.SelectedSource.Count > 0 && Permissions.StudentRecordReport權限;
            };

            Catalog permission = RoleAclSource.Instance["學生"]["報表"];
            permission.Add(new RibbonFeature(Permissions.StudentRecordReport, "學籍表(雙語部)"));
        }
    }
}
