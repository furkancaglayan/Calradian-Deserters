using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using CalradianDeserters.Extensions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using CalradianDeserters.Components;

namespace CalradianDeserters.Models
{
    public class CustomFoodConsumptionModel : MobilePartyFoodConsumptionModel
    {
        private MobilePartyFoodConsumptionModel _baseModel;

        public CustomFoodConsumptionModel(CampaignGameStarter gameStarter)
        {
            _baseModel = CalradianDesertersExtensions.GetModelByType<MobilePartyFoodConsumptionModel>(gameStarter);
        }

        public override int NumberOfMenOnMapToEatOneFood => _baseModel.NumberOfMenOnMapToEatOneFood;


        public override ExplainedNumber CalculateDailyBaseFoodConsumptionf(MobileParty party, bool includeDescription = false)
        {
            return _baseModel.CalculateDailyBaseFoodConsumptionf(party, includeDescription);
        }

        public override ExplainedNumber CalculateDailyFoodConsumptionf(MobileParty party, ExplainedNumber baseConsumption)
        {
            return _baseModel.CalculateDailyFoodConsumptionf(party, baseConsumption);
        }

        public override bool DoesPartyConsumeFood(MobileParty mobileParty)
        {
            if (mobileParty.PartyComponent is DeserterPartyComponent)
            {
                return false;
            }

            return _baseModel.DoesPartyConsumeFood(mobileParty);

        }
    }
}
