using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accelerate.Foundations.Common.Models.Components
{
    public class TableRow<T> : object
    {
        public List<T> Values { get; set; }
    }
    public class TableRow : object
    {
    }
}
