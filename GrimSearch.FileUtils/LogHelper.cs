using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimSearch.Utils
{
    public static class LogHelper
    {
        static LogHelper()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static ILog GetLog()
        {
            var loggers = LogManager.GetCurrentLoggers();
            return LogManager.GetLogger("GDISLog");
        }
    }
}
