using System.IO;
using System.Reflection;

namespace CardValidation
{
  
    class Program
    {
        static void Main()
        {
            LogConfig.LoadLogConfig();
            var _log4net = log4net.LogManager.GetLogger(typeof(Program));

            _log4net.Info("Hello Logging World");

            var cardMachine = new CardMachine();
            Trigger trigger;
            do
            {
                cardMachine.Run();
                trigger = cardMachine.GetResponseTrigger(Constants.ValidationEnded, Constants.EndedChoices);
            }
            while (trigger != Trigger.Exit);
        }
    }
}
