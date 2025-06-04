using System.Data.SqlTypes;

namespace WedMockExam.Repository.Interfaces.Workplace
{
    public class WorkplaceFilter
    {
        public SqlInt32? Floor { get; set; }
        public SqlString? Zone { get; set; }
        public SqlBoolean? HasMonitor { get; set; }
        public SqlBoolean? HasDocking { get; set; }
        public SqlBoolean? HasWindow { get; set; }
        public SqlBoolean? HasPrinter { get; set; }
    }
}
