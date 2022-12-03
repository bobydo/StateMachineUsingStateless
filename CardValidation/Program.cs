using System;

namespace CardValidation
{
  
    class Program
    {
        static void Main()
        {
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
