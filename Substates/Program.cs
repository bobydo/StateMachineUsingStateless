using System;
using System.Threading.Tasks;

namespace Substates
{
    class Program
    {
        static async Task Main()
        {
          Motoring motoring = new Motoring();
          await  motoring.StartupAsync();
          Console.ReadLine();
        }
    }
}
