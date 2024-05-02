using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTestStopwatch
{
    public class StopwatchModel
    {
        public long Id { get; set; }
        public int minute { get; set; }
        public int second { get; set; }
        public int subsecond { get; set; }
    }
}
