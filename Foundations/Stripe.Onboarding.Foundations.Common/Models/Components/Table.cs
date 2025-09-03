using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accelerate.Foundations.Common.Models.Components
{
    public enum AclTableHeaderType
    {
        Text, Link, Number, Date, Buttons
    }
    public class TableHeader
    {
        public string? HeaderType
        {
            get
            {
                return Enum.GetName<AclTableHeaderType>(this.Type);
            }
        }
        public AclTableHeaderType Type { get; set; }
        public dynamic Data { get; set; }
        public string Class { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class Table
    {
        public List<TableHeader> Headers { get; set; }
        public List<TableRow> Items { get; set; }
    }
    public class Table<T>
    {
        public List<TableHeader> Headers { get; set; }
        public List<T> Items { get; set; }
    }

    public class AjaxTable<T> : Table<T>
    {
        public int CurrentPage { get; set; } = 0;
        public int ItemsPerPage { get; set; }
        public int Pages { get; set; }
        public T? Query { get; set; }
        public string Url { get; set; }
    }
}
