using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem;
using CalradianDeserters.Extensions;

namespace CalradianDeserters.Models
{
    public class CustomPartySpeedCalculatingModel : PartySpeedModel
    {
        private PartySpeedModel _baseModel;
        public CustomPartySpeedCalculatingModel(CampaignGameStarter campaignGameStarter)
        {
            _baseModel = campaignGameStarter.GetModelByType<PartySpeedModel>();
        }

        public override float BaseSpeed => _baseModel.BaseSpeed;
        public override float MinimumSpeed => _baseModel.MinimumSpeed;


        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            var baseSpeed = _baseModel.CalculateFinalSpeed(mobileParty, finalSpeed);
            if (mobileParty.IsDeserterParty())
            {
                baseSpeed.AddFactor(Settings.Instance.DeserterPartySpeedBonus);
            }

            return baseSpeed;
        }

        public override ExplainedNumber CalculateBaseSpeed(MobileParty party, bool includeDescriptions = false, int additionalTroopOnFootCount = 0, int additionalTroopOnHorseCount = 0)
        {
            return _baseModel.CalculateBaseSpeed(party, includeDescriptions, additionalTroopOnFootCount, additionalTroopOnHorseCount);
        }
    }

}
