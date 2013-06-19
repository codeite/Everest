using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Everest.Pipeline;

namespace Everest.Timeout
{
    public class TimeoutOption : PipelineOption
    {
        public TimeoutOption(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan Timeout { get; set; }
    }
}
