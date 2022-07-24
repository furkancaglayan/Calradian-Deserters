using CalradianDeserters.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace CalradianDeserters.Behaviors
{
    public class CalradianDesertersCampaignBehavior : CampaignBehaviorBase
    {
        private Clan Deserters => Clan.FindFirst(x => x.StringId == "deserters");
        public override void RegisterEvents()
        {
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.MobilePartyCreated.AddNonSerializedListener(this, MobilePartyCreated);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreated);
        }

        private void MobilePartyCreated(MobileParty mobileParty)
        {
            if (mobileParty.ActualClan == Deserters && !IsDeserter(mobileParty))
            {
                DestroyPartyAction.Apply(null, mobileParty);
            }
        }

        private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            foreach (var mobileParty in MobileParty.All.ToList())
            {
                if (mobileParty.ActualClan == Deserters && !IsDeserter(mobileParty))
                {
                    DestroyPartyAction.Apply(null, mobileParty);
                }
            }
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent.HasWinner)
            {
                var defeatedLeader = mapEvent.GetLeaderParty(mapEvent.DefeatedSide);
                var winningLeader = mapEvent.GetLeaderParty(mapEvent.WinningSide);

                if (defeatedLeader.IsMobile && winningLeader.IsMobile && defeatedLeader.MobileParty.Army != null && winningLeader.MobileParty.Army != null)
                {
                    var troops = defeatedLeader.MapEventSide.Parties.First().Troops;
                    var casualties = defeatedLeader.MapEventSide.Casualties;
                }
            }
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private bool IsDeserter(MobileParty mobileParty)
        {
            return mobileParty.PartyComponent is DeserterPartyComponent;
        }
    }
}
