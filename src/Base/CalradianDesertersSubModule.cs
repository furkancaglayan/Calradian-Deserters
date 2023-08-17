#define CALRADIAN_DESERTERS

using CalradianDeserters.Behaviors;
using CalradianDeserters.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CalradianDeserters.Base
{
    public class CalradianDesertersSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
                AddModels(campaignStarter);
                AddBehaviors(campaignStarter);
            }
        }

        private void AddBehaviors(CampaignGameStarter campaignStarter)
        {
            campaignStarter.AddBehavior(new CalradianDesertersCampaignBehavior());
        }

        private void AddModels(CampaignGameStarter campaignStarter)
        {
            campaignStarter.AddModel(new CustomFoodConsumptionModel(campaignStarter));
            campaignStarter.AddModel(new CustomDiplomacyModel(campaignStarter));
            campaignStarter.AddModel(new CustomPartySpeedCalculatingModel(campaignStarter));
        }
    }
}
