using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSG_TaskReport_Service.Model
{
    public class TableFieldInfo
    {
        public string ColumnName { get; set; }
        public string Type { get; set; }
        public string ColumnType { get; set; }
        public string ColumnKey { get; set; }
        public string ColumnExtra { get; set; }
        public int? Position { get; set; }
        public long? MaxLength { get; set; }
        public long? NumLength { get; set; }
        public long? NumScale { get; set; }
    }
}
