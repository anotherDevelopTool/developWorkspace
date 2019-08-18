using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopWorkspace.Base
{
    public enum Level { TRACE=-1,DEBUG=0,INFO=1,WARNING=2,ERROR=3,FATAL=4}
    public interface Ilogger
    {
        Level level { get; set; }

        void WriteLine(string logText,Level level = Level.INFO);
    }
}
