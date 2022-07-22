using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;

namespace CalradianDeserters.Behaviors
{
    public class CalradianDesertersCampaignBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreated);
        }

        private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
        }

        public override void SyncData(IDataStore dataStore)
        {

        }
    }
}
