using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace CalradianDeserters.Components
{
    public partial class DeserterPartyComponent : PartyComponent
    {
        public override TextObject Name
        {
            get
            {
                if (TextObject.IsNullOrEmpty(_name))
                {
                    _name = GameTexts.FindText("str_deserter_name");
                    _name.SetTextVariable("FACTION_INFORMAL_NAME", _deserterOf.InformalName);
                }

                return _name;
            }
        }

        public override Hero PartyOwner => null;
        public override Settlement HomeSettlement => _homeSettlement;

        [SaveableField(0)]
        private Settlement _homeSettlement;

        [SaveableField(1)]
        private IFaction _deserterOf;

        [SaveableField(2)]
        private TextObject _name;

        [LoadInitializationCallback]
        private void OnLoad(MetaData metaData, ObjectLoadData objectLoadData)
        {
        }

        private static void SetUpParty(MobileParty party, Clan deserterClan, IFaction deserterOf, TroopRoster troopRoster, Vec2 spawnPosition, float spawnRadius, float minSpawnRadius)
        {
            party.ActualClan = deserterClan;
            party.Ai.SetDoNotMakeNewDecisions(true);
            party.SetPartyUsedByQuest(true);
            party.InitializeMobilePartyAroundPosition(troopRoster, TroopRoster.CreateDummyTroopRoster(), spawnPosition, spawnRadius, minSpawnRadius);
        }

        public static MobileParty CreateDeserterParty(string stringId, Clan deserterClan, IFaction deserterOf, Settlement homeSettlement, TroopRoster troopRoster, Vec2 spawnPosition, float spawnRadius, float minSpawnRadius)
        {
            return MobileParty.CreateParty(stringId, new DeserterPartyComponent(homeSettlement, deserterOf), (MobileParty x) => SetUpParty(x, deserterClan, deserterOf, troopRoster, spawnPosition, spawnRadius, minSpawnRadius));
        }

        public DeserterPartyComponent(Settlement homeSettlement, IFaction deserterOf)
        {
            _homeSettlement = homeSettlement;
            _deserterOf = deserterOf;
        }
    }
}
