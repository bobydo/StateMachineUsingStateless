using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;

public class LogConfig
{
    public static void LoadLogConfig()
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
    }
}
