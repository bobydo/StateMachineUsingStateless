using System;
using Stateless;
namespace PureState4
{
    enum State
    {
        A, B, C, D
    }
  
 // Context is a wrapper class. It encapsulates the state machine and handles data I/O   
    class Context
    {
        // the StateMachine class is Generic 
        //It requires a finite set of States and a finite set of  Triggers
        //In this case the states are the members of the enum 'State'
        //The triggers are the two possible types of the type 'bool' ( true and false)
        //The starting State is passed into the constructor
        private StateMachine<State, bool> machine = new StateMachine<State, bool>(State.A);

        public Context()
        {
            //All the states have to be configured
            //And all the required transitions from a state have to be permitted.
            machine.Configure(State.A)
                   .Permit(true, State.B)//permit the transition using trigger 'true' to State.B
                   .PermitReentry(false);//permit transition to this state for 'false'

            machine.Configure(State.B)
                   .Permit(true, State.C)
                   .Permit(false, State.A);

            machine.Configure(State.C)
                  .Permit(true, State.D)
                  .Permit(false, State.A);

            machine.Configure(State.D)
                 .PermitReentry(true)
                 .PermitReentry(false);

        }
        public string Validate(string dataString)
        {
            foreach (var c in dataString)
            {
                bool trigger = c == '1' ? true : false;
                //The Fire method initiates the state transition.
                machine.Fire(trigger);
            }
            return machine.IsInState(State.D) ? "Accepted" : "Rejected";
        }

    }

    class Program
    {
        static void Main()
        {
            LogConfig.LoadLogConfig();
            var context = new Context();
            string input = "10011010111011011";
            var result = context.Validate(input);
            Console.WriteLine($"Inputting string: {input}");
            Console.WriteLine($"The result is {result}");
            Console.ReadLine();
        }
    }
}
