using Stateless;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Substates
{
    public enum State
    {
        Off,
        Start,
        Motoring,
        Seatbelt,
        Engine,
        Brake,
        Parked
    }
    public enum Trigger
    {
        Start,
        Motor,
        Fasten,
        Engage,
        Release,
        Park
    }
    public class Motoring
    {
        private const string engineNoise = "chug ";
        private Task engineTask;
        private readonly StateMachine<State, Trigger> machine;
        public State CurrentState = State.Off;
        public int Count { get; set; }
        public Motoring()
        {
            Count = 0;
            machine = new StateMachine<State, Trigger>(() => CurrentState, NewState =>
            {
                CurrentState = NewState;
                Count++;
            });
            ConfigureMachine();
        }

        private void ConfigureMachine()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            machine.Configure(State.Start)
            .Permit(Trigger.Motor, State.Motoring)
            .OnExit(() => Log("Leaving Start"));

            machine.Configure(State.Motoring)
            .Permit(Trigger.Fasten, State.Seatbelt)
            .OnEntry((transition) => Log("Started Motoring"))
            .OnExitAsync(() =>
             {
                 Log("Finished Motoring");
                 return Task.CompletedTask;
             });

            machine.Configure(State.Seatbelt)
            .SubstateOf(State.Motoring)
            .Permit(Trigger.Engage, State.Engine)
            .OnEntry(() => Log("Seatbelt Fastened"))
            .OnExitAsync(() =>
             {
                 Log("Seatbelt UnFastened");
                 return Task.CompletedTask;
             });

            machine.Configure(State.Engine)
           .SubstateOf(State.Seatbelt)
           .Permit(Trigger.Release, State.Brake)
           .OnEntry(() =>
          {
               //start the task but don't await it here
               engineTask = Task.Run(() => ChugChug(cts.Token));
              Log($"Engine Started {engineNoise}");
          })
           .OnExitAsync(async () =>
           {
               cts.Cancel();
               await engineTask;
               Log("Engine Stopped");
           });

            machine.Configure(State.Brake)
            .SubstateOf(State.Engine)
            .Permit(Trigger.Park, State.Parked)
            .OnEntry(() => Log("Brake Released"))
            .OnExitAsync(() =>
             {
                 Log("BreakApplied");
                 return Task.CompletedTask;
             });

            machine.Configure(State.Parked)
           .OnEntry(() => Log("Parked"));
        }
        public async Task StartupAsync()
        {
            machine.Fire(Trigger.Motor);
            machine.Fire(Trigger.Fasten);
            machine.Fire(Trigger.Engage);
            machine.Fire(Trigger.Release);
            string msg = machine.IsInState(State.Motoring) ? "is in " : "is not in ";
            Log($"The current state is {machine.State} it {msg}state Motoring", ConsoleColor.Yellow);
            await Task.Delay(50);
            Log("\r\nFiring Trigger Park", ConsoleColor.Yellow);
            //FireAsync calls the OnExitAsync action of the current state
            //The call bubbles up through the substates to State.Motoring
            await machine.FireAsync(Trigger.Park);
        }

        //Best not to append Async to the method name
        // as the method is not intrinsically async
        private void ChugChug(CancellationToken token)
        {
            while (true)
            {
                //simulate long-running method
                Thread.Sleep(5);
                //check for cancellation
                if (token.IsCancellationRequested) break;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(engineNoise);
            }
        }

        public static void Log(string msg, ConsoleColor colour = ConsoleColor.White)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(msg);
            Console.ForegroundColor = temp;
        }

    }
}
