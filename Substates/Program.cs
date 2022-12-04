using System;
using System.Threading.Tasks;

namespace Substates
{
    class Program
    {
        static async Task Main()
        {
            LogConfig.LoadLogConfig();
            Motoring motoring = new Motoring();
            await  motoring.StartupAsync();
            Console.ReadLine();
        }
    }
}
