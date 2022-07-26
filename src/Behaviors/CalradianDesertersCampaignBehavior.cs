using CalradianDeserters.Components;
using CalradianDeserters.Extensions;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CalradianDeserters.Behaviors
{
    public class CalradianDesertersCampaignBehavior : CampaignBehaviorBase
    {
        private Clan Deserters => Clan.FindFirst(x => x.StringId == "deserters");
        private Dictionary<MobileParty, CampaignTime> _nextDecisionTimes = new Dictionary<MobileParty, CampaignTime>();
        private List<MobileParty> _deserterParties = new List<MobileParty>();
        private List<MobileParty> _obsoleteDeserterParties = new List<MobileParty>();
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunchedEvent);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, Tick);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.MobilePartyCreated.AddNonSerializedListener(this, MobilePartyCreated);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreated);
        }

        private void HourlyTick()
        {
            for (int i = _obsoleteDeserterParties.Count - 1; i >= 0 ; i--)
            {
                _obsoleteDeserterParties[i].ActualClan = Deserters;
                DestroyPartyAction.Apply(null, _obsoleteDeserterParties[i]);
            }

            _obsoleteDeserterParties.Clear();
        }

        private void OnSessionLaunchedEvent(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
            AddGameMenus(campaignGameStarter);
        }

        private void AddDialogs(CampaignGameStarter campaignGameStarter)
        {
        }

        private void AddGameMenus(CampaignGameStarter campaignGameStarter)
        {

        }

        private void Tick(float dt)
        {
#if DEBUG
            foreach (var party in _deserterParties)
            {
                party.IsVisible = true;
            }
#endif
        }

        private void HourlyTickParty(MobileParty party)
        {
            if (/*DisableAi || */party.MapEvent != null || (party.CurrentSettlement != null && party.CurrentSettlement.IsUnderSiege))
            {
                return;
            }

            if (party.PartyComponent is DeserterPartyComponent deserterPartyComponent)
            {
                if (_nextDecisionTimes[party].IsPast)
                {
                    MakeNewDecision(party);
                }
            }
        }

        private void MobilePartyCreated(MobileParty mobileParty)
        {
            if (mobileParty.ActualClan == Deserters && !IsDeserter(mobileParty))
            {
                _obsoleteDeserterParties.Add(mobileParty);
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

            GenerateRandomParties();
        }

        private void GenerateRandomParties(int n = 4)
        {
            var x = 25;
            var y = 55;

            foreach (var kingdom in Kingdom.All)
            {
                for (int i = 0; i < n; i++)
                {
                    var roster = TroopRoster.CreateDummyTroopRoster();
                    const string troopId = "deserter";
                    var character = Campaign.Current.ObjectManager.GetObject<CharacterObject>(troopId);
                    roster.AddToCounts(character, MBRandom.RandomInt(x, y));
                    var randomVillage = kingdom.Villages.GetRandomElement();
                    var position = Helpers.MobilePartyHelper.FindReachablePointAroundPosition(randomVillage.Settlement.Position2D, 10, 5);

                    CreateDeserterParty(randomVillage.Settlement, roster, position);
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
                    var homeSettlement = mapEvent.MapEventSettlement ?? SettlementHelper.FindNearestVillage(toMapPoint: winningLeader.MobileParty);
                    var troopRoster = TroopRoster.CreateDummyTroopRoster();
                    var diedInBattleProperty = GetPropertyInfo(typeof(MapEventParty), "DiedInBattle");
                    foreach (var mapEventParty in defeatedLeader.MapEventSide.Parties)
                    {
                        var diedInBattle = diedInBattleProperty.GetMethod.Invoke(mapEventParty, new object[] { });
                        troopRoster.Add((TroopRoster)diedInBattle);
                    }

                    var count = (int)(defeatedLeader.MapEventSide.Casualties / 10);
                    var finalRoster = GetRandomTroopsFromRoster(troopRoster, count);
                    CreateDeserterParty(homeSettlement, finalRoster, mapEvent.Position);
                }
            }
        }
        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
        {
            _nextDecisionTimes.Remove(mobileParty);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_obsoleteDeserterParties", ref _obsoleteDeserterParties);
            dataStore.SyncData("_deserterParties", ref _deserterParties);
            dataStore.SyncData("_desertersDecisionTimes", ref _nextDecisionTimes);
        }

        private void MakeNewDecision(MobileParty party)
        {
            var partiesAround = new MobilePartiesAroundPositionList(32).GetPartiesAroundPosition(party.Position2D, party.SeeingRange);

            MobileParty selectedTarget = null;
            MobileParty fleeTarget = null;

            var bestAttackScore = 0f;

            foreach (var mobileParty in partiesAround)
            {
                if (mobileParty.IsActive && (mobileParty.IsVillager || mobileParty.IsCaravan || mobileParty.IsLordParty || 
                    (mobileParty.IsMilitia && mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.IsVillage)))
                {
                    var enemyStrength = mobileParty.GetTotalStrengthWithFollowers();
                    var partyStrength = party.Party.TotalStrength;

                    if (enemyStrength > partyStrength)
                    {
                        if (mobileParty.IsLordParty)
                        {
                            if (enemyStrength > partyStrength * 1.33f &&
                                (fleeTarget == null || Campaign.Current.Models.MapDistanceModel.GetDistance(fleeTarget, party) > Campaign.Current.Models.MapDistanceModel.GetDistance(mobileParty, party)))
                            {
                                fleeTarget = mobileParty;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                    var attackScore = GetAttackScoreOfParty(mobileParty, party) + MBRandom.RandomFloatRanged(-0.3f, 0.3f);
                    if (attackScore > bestAttackScore)
                    {
                        selectedTarget = mobileParty;
                        bestAttackScore = attackScore;
                    }
                }
            }

            if (fleeTarget != null)
            {
                var targetDirectionNormalized = (-fleeTarget.Position2D + party.Position2D).Normalized();
                var tryCount = 10;
                var targetPosition = targetDirectionNormalized * 5 + party.Position2D;

                for (int i = 0; i < tryCount; i++)
                {
                    targetPosition = targetDirectionNormalized * 5+ party.Position2D;
                    targetPosition.RotateCCW((i * 360 / tryCount) * MathF.PI / 180);
                    
                    var faceIndex = Campaign.Current.MapSceneWrapper.GetFaceIndex(targetPosition);

                    if (faceIndex.IsValid())
                    {
                        break;
                    }
                }

                party.SetMoveGoToPoint(targetPosition);
                DisableThinkForHours(party, 2);
            }
            else if (selectedTarget != null && !selectedTarget.IsMilitia)
            {
                SetPartyAiAction.GetActionForEngagingParty(party, selectedTarget);
                DisableThinkForHours(party, 12);
            }
            else if (selectedTarget != null && selectedTarget.IsMilitia && !selectedTarget.CurrentSettlement.IsRaided 
                    && !selectedTarget.CurrentSettlement.IsUnderRaid)
            {
                SetPartyAiAction.GetActionForRaidingSettlement(party, selectedTarget.CurrentSettlement);
                DisableThinkForHours(party, 24);
            }
            else/* if (MBRandom.RandomFloat < 0.7f)*/
            {
                var settlement = SettlementHelper.FindNearestVillage(toMapPoint: party);
                SetPartyAiAction.GetActionForPatrollingAroundSettlement(party, settlement);
                DisableThinkForHours(party, 6);
            }
        }

        private void ClearDecision(MobileParty party)
        {
            _nextDecisionTimes.Set(party, CampaignTime.Now);
        }

        private void DisableThinkForHours(MobileParty party, float hours)
        {
            _nextDecisionTimes[party] = CampaignTime.HoursFromNow(hours);
        }

        private void CreateDeserterParty(Settlement homeSettlement, TroopRoster finalRoster, Vec2 position)
        {
            var party = DeserterPartyComponent.CreateDeserterParty("deserter_party_1", Deserters, homeSettlement, finalRoster,position, 5, 2);
            _deserterParties.Add(party);
            ClearDecision(party);
        }
        private bool IsDeserter(MobileParty mobileParty)
        {
            return mobileParty.PartyComponent is DeserterPartyComponent;
        }

        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propInfo = null;
            do
            {
                propInfo = type.GetProperty(propertyName,
                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (propInfo == null && type != null);
            return propInfo;
        }

        private static TroopRoster GetRandomTroopsFromRoster(TroopRoster troopRoster, int t)
        {
            var roster = TroopRoster.CreateDummyTroopRoster();

            for (int i = 0; i < t && troopRoster.Count > 0; i++)
            {
                var rand = MBRandom.RandomInt(0, troopRoster.Count);
                var troop = troopRoster.GetCharacterAtIndex(rand);
                troopRoster.AddToCountsAtIndex(rand, -1);
                roster.AddToCounts(troop, 1);
            }

            return roster;
        }

        private float GetAttackScoreOfParty(MobileParty enemyParty, MobileParty party)
        {
            var strengthFactor = Math.Max(enemyParty.GetTotalStrengthWithFollowers() - party.Party.TotalStrength, 0f);

            var multiplier = enemyParty.IsLordParty ? 1.2f : (enemyParty.IsVillager ? 0.9f : 0.6f);

            var distance = Campaign.Current.Models.MapDistanceModel.GetDistance(party, enemyParty);

            return multiplier * (strengthFactor - 0.5f) + (1 / (distance * distance));
        }
    }
}
