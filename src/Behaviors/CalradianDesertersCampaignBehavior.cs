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
using TaleWorlds.CampaignSystem.LogEntries;
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
        //TODO: Create deserter parties after battles with a chance
        //Include minor faction troops in them
        //Raid villages
        //merge
        //save battle data to xml or sheets


        //TODO: ADD DAİLY CHANCE OF CREATING DESERTER PARTIES TO CASTLES
        private Dictionary<MobileParty, CampaignTime> _nextDecisionTimes = new Dictionary<MobileParty, CampaignTime>();
        private List<MobileParty> _deserterParties = new List<MobileParty>();
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
            CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            foreach (var kingdom in Kingdom.All)
            {
                foreach (var kingdomId in Kingdom.All)
                {
                    var deserterClan = GetDeserterClan(kingdomId);
                    if (deserterClan == null)
                    {
                        Debug.FailedAssert("deserterClan == null");
                    }
                    else if (!kingdom.IsAtWarWith(deserterClan))
                    {
                        DeclareWarAction.ApplyByDefault(kingdom, deserterClan);
                    }
                }
            }

            foreach (var clan in Clan.NonBanditFactions)
            {
                foreach (var kingdomId in Kingdom.All)
                {
                    var deserterClan = GetDeserterClan(kingdomId);
                    if (!clan.IsAtWarWith(deserterClan) && !IsDeserterClan(clan))
                    {
                        DeclareWarAction.ApplyByDefault(clan, deserterClan);
                    }
                }
            }

            foreach (var deserterParty in MobileParty.All)
            {
                if (deserterParty.IsDeserterParty())
                {
                    _deserterParties.Add(deserterParty);
                }
            }
        }

        private void HourlyTick()
        {
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
        }

        private void HourlyTickParty(MobileParty party)
        {
            if (/*DisableAi || */party.MapEvent != null || (party.CurrentSettlement != null && party.CurrentSettlement.IsUnderSiege))
            {
                return;
            }

            if (party.IsDeserterParty(out var deserterPartyComponent))
            {
                if (_nextDecisionTimes[party].IsPast)
                {
                    MakeNewDecision(party);
                }
            }
        }

        private void MobilePartyCreated(MobileParty mobileParty)
        {
        }

        private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            foreach (var kingdom in Kingdom.All)
            {
                var deserterClan = GetDeserterClan(kingdom);

                if (deserterClan == null)
                {
                    Debug.FailedAssert("deserterClan == null");
                }
                else
                {
                    var deserterLeader = HeroCreator.CreateSpecialHero(deserterClan.MinorFactionCharacterTemplates.GetRandomElementInefficiently(), null, deserterClan);
                    deserterLeader.ChangeState(Hero.CharacterStates.Dead);
                    deserterLeader.CharacterObject.HiddenInEncylopedia = true;

                    deserterClan.SetLeader(deserterLeader);


                    foreach (var kingdom2 in Kingdom.All)
                    {
                        if (!kingdom2.IsAtWarWith(deserterClan))
                        {
                            DeclareWarAction.ApplyByDefault(kingdom2, deserterClan);
                        }
                    }


                    foreach (var clan in Clan.NonBanditFactions)
                    {
                        if (!clan.IsAtWarWith(deserterClan) && !IsDeserterClan(clan))
                        {
                            DeclareWarAction.ApplyByDefault(clan, deserterClan);
                        }
                    }
                }
            }
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

                    CreateDeserterParty(randomVillage.Settlement, GetDeserterClan(kingdom), kingdom, roster, position);
                }
            }
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent.HasWinner)
            {
                var defeatedLeader = mapEvent.GetLeaderParty(mapEvent.DefeatedSide);
                var winningLeader = mapEvent.GetLeaderParty(mapEvent.WinningSide);

                var chance = 0.6f;
                if (defeatedLeader?.MobileParty?.Army != null)
                {
                    chance = 0.75f;
                }

                var condition = MBRandom.RandomFloat < chance && defeatedLeader.IsMobile && winningLeader.IsMobile &&
                    ((defeatedLeader.MobileParty.IsLordParty || defeatedLeader.MobileParty.LeaderHero?.IsMinorFactionHero == true)
                    && (winningLeader.MobileParty.IsLordParty || winningLeader.MobileParty.LeaderHero?.IsMinorFactionHero == true) && 
                    defeatedLeader.MapFaction != null &&
                    winningLeader.MapFaction != null &&
                    defeatedLeader.MapFaction.IsKingdomFaction);

                if (condition)
                {
                    var homeSettlement = mapEvent.MapEventSettlement ?? SettlementHelper.FindNearestVillage(toMapPoint: winningLeader.MobileParty);
                    var troopRoster = TroopRoster.CreateDummyTroopRoster();
                    var count = (int)(defeatedLeader.MapEventSide.Casualties / 5);

                    var finalRoster = GetRandomTroopsFromRoster(troopRoster, count);

                    CreateDeserterParty(homeSettlement, GetDeserterClan(defeatedLeader.MapFaction), defeatedLeader.MapFaction, finalRoster, mapEvent.Position);
                }
            }
        }
        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
        {
            _nextDecisionTimes.Remove(mobileParty);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_desertersDecisionTimes", ref _nextDecisionTimes);
        }

        private void MakeNewDecision(MobileParty party)
        {
            var searchData = MobileParty.StartFindingLocatablesAroundPosition(party.Position2D, party.SeeingRange);
            var mobileParty = MobileParty.FindNextLocatable(ref searchData);


            MobileParty selectedTarget = null;
            MobileParty fleeTarget = null;

            var bestAttackScore = 0f;

            while (mobileParty != null)
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
                            mobileParty = MobileParty.FindNextLocatable(ref searchData);
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

                mobileParty = MobileParty.FindNextLocatable(ref searchData);
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

                party.Ai.SetMoveGoToPoint(targetPosition);
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

        private void CreateDeserterParty(Settlement homeSettlement, Clan deserterClan, IFaction deserterOf, TroopRoster finalRoster, Vec2 position)
        {
            var party = DeserterPartyComponent.CreateDeserterParty("deserter_party_1", deserterClan, deserterOf, homeSettlement, finalRoster,position, 5, 2);
            _deserterParties.Add(party);
            ClearDecision(party);
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

        private Clan GetDeserterClan(IFaction faction)
        {
            if (faction.StringId == "empire" || faction.StringId == "empire_w" || faction.StringId == "empire_s")
            {
                return Campaign.Current.CampaignObjectManager.Find<Clan>(x => x.StringId == $"deserters_empire");
            }

            return Campaign.Current.CampaignObjectManager.Find<Clan>(x => x.StringId == $"deserters_{faction.StringId}");
        }

        private bool IsDeserterClan(Clan clan)
        {
            foreach (var kingdom in Kingdom.All)
            {
                if (clan == GetDeserterClan(kingdom))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
