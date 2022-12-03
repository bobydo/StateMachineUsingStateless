using Stateless;
using Stateless.Graph;
using System;
using System.Linq;

namespace CardValidation
{
    public enum EmailState
    {
        Local,
        Domain,
        Accepted,
        Rejected,
        Start
    }
    public class EmailValidator : IValidator
    {
        private EmailState currentState = EmailState.Start;
        private readonly StateMachine<EmailState, char> machine;
        public EmailValidator()
        {
            //provide a getter and setter so the current state can be reset to the Start State
            //after each attempt at validation
            machine = new StateMachine<EmailState, char>(() => currentState, s => currentState = s);
            // ignore unconfigured Trigger exception
            machine.OnUnhandledTrigger((state, trigger) => { });
            ConfigureMachine();
        }
        private void ConfigureMachine()
        {
            machine.Configure(EmailState.Start)
            .Permit('@', EmailState.Rejected)
            .Permit('.', EmailState.Rejected)
            .Permit('x', EmailState.Local);

            machine.Configure(EmailState.Local)
            .Permit('@', EmailState.Domain);

            machine.Configure(EmailState.Domain)
            .Permit('@', EmailState.Rejected)
            .Permit('.', EmailState.Rejected)
            .Permit('-', EmailState.Rejected)
            .Permit('x', EmailState.Accepted);

            machine.Configure(EmailState.Accepted)
            .Permit('-', EmailState.Rejected)
            .Permit('@', EmailState.Rejected);

            machine.Configure(EmailState.Rejected);

            //Console.WriteLine( UmlDotGraph.Format(machine.GetInfo()));

        }

        public bool Validate(string dataString)
        {
            char[] acceptable = new char[] { '@', '.', '-' };
            //rinse out all illegal chars
            if (dataString.Any(c => !char.IsLetterOrDigit(c) && !acceptable.Contains(c))) return false;

            foreach (var c in dataString)
            {
                //use the trigger 'x' for all alphanumeric chars
                char trigger = char.IsLetterOrDigit(c) ? 'x' : c;
                //The Fire method initiates the state transition.
                machine.Fire(trigger);
            }
            var isValid = machine.IsInState(EmailState.Accepted);
            //reset to Start
            currentState = EmailState.Start;
            return isValid;
        }

    }
}
