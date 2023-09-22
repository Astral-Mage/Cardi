using System;

namespace ChatBot.Bot.Plugins.LostRPG.ControllerSystem.Controllers
{
    public enum EnslavementType
    {
        Null,
        Combat,
        Love,
        Breeding,
        Display,
    }

    public class EnslavementController : BaseController
    {
        private readonly EnslavementType enslavementType;
        private readonly string ownerName;
        private readonly DateTime enslavementDate;

        private int totalDebtOwed;
        private int totalDebtPaid;



        public void PayDebt(int valueToPay)
        {
            totalDebtPaid += valueToPay;
        }

        public void Update()
        {
            if (totalDebtPaid >= totalDebtOwed)
            {
                // slavery ends

            }
        }

        public EnslavementController(EnslavementType slavetype, string ownerId, int startingDebt)
        {
            enslavementType = slavetype;
            ownerName = ownerId;
            enslavementDate = DateTime.Now;

            totalDebtOwed = startingDebt;
            totalDebtPaid = 0;


        }

        // ability to pay off debt?
        // status effect(s)
        // how to enslave
        // base effect of enslavement
        // ability to release slave
        // owners slave display card
        // ability to enslave
        // ability to change slave type
        // ability to see slave stats
        // ability to transfer ownership of a slave
        // force slaves to do non-specified action at a cost

        // ensure that card is user-only. public-called cards should contain very little information
    }
}
