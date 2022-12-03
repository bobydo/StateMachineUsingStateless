using Stateless;
using Stateless.Graph;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardValidation
{
    public enum State
    {
        Accepted,
        Validating,
        Rejected,
        Failed,
        Start,
        Cancelled
    }
    public enum Trigger
    {
        Cancel,
        Fail,
        Retry,
        Process,
        Accept,
        Reject,
        Exit
    }
    public class CardMachine
    {
        private State currentState = State.Start;
        private readonly StateMachine<State, Trigger> machine;
        private int attempts = 0;
        private readonly IValidator validator = new EmailValidator();
        private bool IsRejected => attempts > 1;
        private readonly Dictionary<char, Trigger> lookupTrigger = new Dictionary<char, Trigger> {
            {'s',Trigger.Process },
            {'c',Trigger.Cancel },
            {'r',Trigger.Retry },
            {'e',Trigger.Exit }
        };
        public CardMachine()
        {
            //provide a getter and setter so the current state can be reset to the Start State
            machine = new StateMachine<State, Trigger>(() => currentState, s => currentState = s);
            //ignore unconfigured Trigger exception
            // machine.OnUnhandledTrigger((state, trigger) => { });
            ConfigureMachine();
        }
        private void ConfigureMachine()
        {
            machine.Configure(State.Start)
            .Permit(Trigger.Process, State.Validating);

            machine.Configure(State.Validating)
            .OnEntry(() =>
            {
                //prompt for an input
                Log(Constants.StartValidating);
                var address = Console.ReadLine();
                if (validator.Validate(address))
                {
                    machine.Fire(Trigger.Accept);
                    return;
                }
                Trigger trigger = IsRejected ? Trigger.Reject : Trigger.Fail;
                machine.Fire(trigger);
            })
            .Permit(Trigger.Accept, State.Accepted)
            .Permit(Trigger.Fail, State.Failed)
            .Permit(Trigger.Reject, State.Rejected);

            machine.Configure(State.Accepted)
            .OnEntry(() =>
              {
                  Log(Constants.Accepted);
               });

            machine.Configure(State.Failed)
             .OnEntry(() =>
             {
                 var trigger = GetResponseTrigger(Constants.Failed, Constants.FailedChoices);
                 attempts += 1;
                 machine.Fire(trigger);
             })
             .Permit(Trigger.Cancel, State.Cancelled)
             .Permit(Trigger.Retry, State.Validating);


            machine.Configure(State.Rejected)
            .OnEntry(() =>
             {
                 Log(Constants.Reject);
             });

            machine.Configure(State.Cancelled)
            .OnEntry(() =>
             {
                 Log(Constants.Cancelled);
 
             });
          //output the DotGraph string here while the current state is the Start state
         // Console.WriteLine(ToDotGraph()); ;
        }
        public void Run()
        {
            currentState = State.Start;
            attempts = 0;
            machine.Fire(Trigger.Process);
        }
        public static void Log(string msg, ConsoleColor colour = ConsoleColor.Yellow)
        {
            var temp = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(msg);
            Console.ForegroundColor = temp;
        }

        public Trigger GetResponseTrigger(string message, string choices)
        {
            char response;
            Log(message);
            do
            {
                response = char.ToLower(Console.ReadKey(true).KeyChar);
            }
            while (choices.All(c => c != response));
            return lookupTrigger[response];
        }

        public string ToDotGraph()
        {
            return UmlDotGraph.Format(machine.GetInfo());
        }

    }
}
