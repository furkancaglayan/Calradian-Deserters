using CalradianDeserters.Base;
using CalradianDeserters.Components;
using CalradianDeserters.Extensions;
using Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;
using static System.Collections.Specialized.BitVector32;

namespace CalradianDeserters.Behaviors
{
    public class CalradianDesertersCampaignBehavior : CampaignBehaviorBase
    {
        private Dictionary<MobileParty, CampaignTime> _nextDecisionTimes = new Dictionary<MobileParty, CampaignTime>();
        private Dictionary<Settlement, CampaignTime> _nextChanceToSpawnDeserters = new Dictionary<Settlement, CampaignTime>();
        private List<MobileParty> _deserterParties = new List<MobileParty>();
        private Dictionary<string, Clan> _deserterClans = new Dictionary<string, Clan>();
        private Dictionary<(IFaction, int), List<CharacterObject>> _troopTrees = new Dictionary<(IFaction, int), List<CharacterObject>>();

#if DEBUG
        private static PlatformDirectoryPath DataDirectory => new PlatformDirectoryPath(PlatformFileType.User, "Data");
        private static PlatformFilePath DataFilePath => new PlatformFilePath(DataDirectory, $"deserter_stats_{Campaign.Current.UniqueGameId}.txt");

        private int _totalRaidAttempts;
        private int _totalSuccessfulRaids;
        private int _totalBattles;
        private int _totalWonBattles;
        private int _totalCaravanBattles;
        private int _totalCaravanWonBattles;
        private int _totalVillagerBattles;
        private int _totalVillagerWonBattles;

#if CALRADIAN_PATROLSV2
        private int _totalPatrolPartyBattles;
        private int _totalPatrolPartyWonBattles;
#endif

#endif
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunchedEvent);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, HourlyTick);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
            CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnCompanionClanCreatedEvent.AddNonSerializedListener(this, OnCompanionClanCreated);
            CampaignEvents.KingdomCreatedEvent.AddNonSerializedListener(this, OnKingdomCreated);
            CampaignEvents.RebellionFinished.AddNonSerializedListener(this, OnRebellionFinished);
#if DEBUG
            CampaignEvents.MakePeace.AddNonSerializedListener(this, OnPeace);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
#endif
        }


#if DEBUG

        private void OnRaidCompleted(BattleSideEnum winner, RaidEventComponent raidEvent)
        {
            if (winner == BattleSideEnum.Attacker && raidEvent.AttackerSide.LeaderParty?.MobileParty?.IsDeserterParty() == true)
            {
                _totalRaidAttempts++;
                _totalSuccessfulRaids++;
            }
            else if (winner == BattleSideEnum.Defender && raidEvent.DefenderSide.LeaderParty?.MobileParty?.IsDeserterParty() == true)
            {
                _totalRaidAttempts++;
            }
        }


        private void DailyTick()
        {
            var partyCount = _deserterParties.Count;
            if (partyCount == 0)
            {
                return;
            }
            Log($"Daily Tick {(int)CampaignData.CampaignStartTime.ElapsedDaysUntilNow}");
            Log($"Total Deserter Count: {partyCount}");
            Log($"Total Caravan Battles Count: {_totalCaravanBattles} ----- Total Caravan Battles Won: {_totalCaravanWonBattles}");
            Log($"Total Villager Battles Count: {_totalVillagerBattles} ----- Total Villager Battles Won: {_totalVillagerWonBattles}");
#if CALRADIAN_PATROLSV2
            Log($"Total Patrol Battles Count: {_totalPatrolPartyBattles} ----- Total Patrol Battles Won: {_totalPatrolPartyWonBattles}");
#endif
            Log($"Total Battles Count: {_totalBattles} ----- Total Battles Won: {_totalWonBattles}");
            Log($"Total Raids Count: {_totalRaidAttempts} ----- Total Raids Won: {_totalSuccessfulRaids}");
            Log($"-------------------------------------------\n");
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom kingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool arg5)
        {
            Debug.Assert(!clan.IsDeserterClan() && !clan.IsDeserterClan(), "!clan.IsDeserterClan() && !clan.IsDeserterClan()");
        }

        private void OnPeace(IFaction faction1, IFaction faction2, MakePeaceAction.MakePeaceDetail detail)
        {
            Debug.Assert(!faction1.IsDeserterClan() && !faction2.IsDeserterClan(), "!faction1.IsDeserterClan() && !faction2.IsDeserterClan()");
        }

        private void Log(string message)
        {
            try
            {
                File.AppendAllText(DataFilePath.FileFullPath, $"{message}\n");
            }
            catch
            {
            }
        }

#endif

        private void OnRebellionFinished(Settlement settlement, Clan clan)
        {
            DeclareWarWithFaction(clan);
        }

        private void OnCompanionClanCreated(Clan clan)
        {
            DeclareWarWithFaction(clan);
        }

        private void OnKingdomCreated(Kingdom kingdom)
        {
            DeclareWarWithFaction(kingdom);
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            InitializeDeserterClans(true);

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
        }

        private void AddDialogs(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddDialogLine("deserter_start_player", "start", "deserter_party_player_greeting", "{=deserters_start}You will die by my hands!", deserter_start_talk_on_condition, null);
            campaignGameStarter.AddPlayerLine("deserter_start_player1", "deserter_party_player_greeting", "close_window", "{=!}{PLAYER_LINE}", player_start_talk_on_condition, null);
        }

        #region Dialogs
        private bool deserter_start_talk_on_condition()
        {
            return MobileParty.ConversationParty != null && MobileParty.ConversationParty.IsDeserterParty();
        }

        private bool player_start_talk_on_condition()
        {
            if (MobileParty.ConversationParty != null && MobileParty.ConversationParty.IsDeserterParty(out var component))
            {
                if (component.DeserterOf == Clan.PlayerClan.Kingdom)
                {
                    MBTextManager.SetTextVariable("PLAYER_LINE", GameTexts.FindText("str_player_line_same_kingdom")); ;
                }
                else
                {
                    MBTextManager.SetTextVariable("PLAYER_LINE", GameTexts.FindText("str_player_line_generic")); ;
                }

                return true;
            }

            return false;
        }

        #endregion
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

        private void OnNewGameCreated(CampaignGameStarter campaignGameStarter)
        {
            InitializeDeserterClans();
        }

        private void OnMapEventEnded(MapEvent mapEvent)
        {
            if (mapEvent.HasWinner && (mapEvent.IsFieldBattle || mapEvent.IsSallyOut || mapEvent.IsSiegeAssault || mapEvent.IsSiegeOutside))
            {
                var defeatedLeader = mapEvent.GetLeaderParty(mapEvent.DefeatedSide);
                var winningLeader = mapEvent.GetLeaderParty(mapEvent.WinningSide);
                var nearestVillage = SettlementHelper.FindNearestVillage(toMapPoint: winningLeader.MobileParty);
#if DEBUG
                if (mapEvent.InvolvedParties.Any(x => x.IsMobile && x.MobileParty.IsDeserterParty()))
                {
                    _totalBattles++;

                    if (defeatedLeader.IsMobile && defeatedLeader.MobileParty.IsCaravan)
                    {
                        _totalCaravanBattles++;
                        _totalCaravanWonBattles++;
                    }
                    else if (defeatedLeader.IsMobile && defeatedLeader.MobileParty.IsVillager)
                    {
                        _totalVillagerBattles++;
                        _totalVillagerWonBattles++;
                    }
#if CALRADIAN_PATROLSV2
                    else if(defeatedLeader.IsMobile && defeatedLeader.MobileParty.PartyComponent is PatrolPartyComponent)
                    {
                        _totalPatrolPartyWonBattles++;
                    }
#endif

                    if (winningLeader.IsMobile && winningLeader.MobileParty.IsCaravan)
                    {
                        _totalCaravanBattles++;
                    }
                    else if (winningLeader.IsMobile && winningLeader.MobileParty.IsVillager)
                    {
                        _totalVillagerBattles++;
                    }
#if CALRADIAN_PATROLSV2
                    else if(winningLeader.IsMobile && winningLeader.MobileParty.PartyComponent is PatrolPartyComponent)
                    {
                        _totalPatrolPartyBattles++;
                    }
#endif

                    if (mapEvent.GetMapEventSide(mapEvent.WinningSide).Parties.Any(y => y.Party.MobileParty?.IsDeserterParty() == true))
                    {
                        _totalWonBattles++;
                    }
                }
#endif


                if (_nextChanceToSpawnDeserters.TryGetValue(nearestVillage, out var time) && time.IsFuture)
                {
#if DEBUG
                    //cant create party bcs there is a time limit
#endif
                    return;
                }
                if (_deserterParties.Count >= Settings.GetInstance().MaxPartyCount)
                {
                    return;
                }

                if (MBRandom.RandomFloat >= Settings.GetInstance().BaseSpawnPartyChance)
                {
                    return;
                }

                if (!defeatedLeader.IsMobile || !winningLeader.IsMobile || defeatedLeader.MapFaction == null || !defeatedLeader.MapFaction.IsKingdomFaction)
                {
                    return;
                }

                if (defeatedLeader.MobileParty.IsDeserterParty() || winningLeader.MobileParty.IsDeserterParty())
                {
                    return;
                }

                if (!(defeatedLeader.MobileParty.IsLordParty || defeatedLeader.MobileParty.LeaderHero?.IsMinorFactionHero == true) ||
                    !(winningLeader.MobileParty.IsLordParty || winningLeader.MobileParty.LeaderHero?.IsMinorFactionHero == true))
                {
                    return;
                }

                var count = (int)(defeatedLeader.MapEventSide.Casualties / 10);
                var troopRoster = GetTroopsForParty((Kingdom)defeatedLeader.MapFaction, MBMath.ClampInt(count, 0, 15));

                var homeSettlement = mapEvent.MapEventSettlement ?? nearestVillage;
                _nextChanceToSpawnDeserters[nearestVillage] = CampaignTime.DaysFromNow(7);
                CreateDeserterParty(homeSettlement, GetDeserterClan((Kingdom)defeatedLeader.MapFaction), defeatedLeader.MapFaction, troopRoster, mapEvent.Position);
            }
        }
        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
        {
            if (mobileParty.IsDeserterParty())
            {
                _nextDecisionTimes.Remove(mobileParty);
                _deserterParties.Remove(mobileParty);
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_desertersDecisionTimes", ref _nextDecisionTimes);
            dataStore.SyncData("_nextChanceToSpawnDeserters", ref _nextChanceToSpawnDeserters);
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
                var canAttackVillagers = mobileParty.IsVillager && Settings.GetInstance().AttackVillagers;
                var canAttackCaravans = mobileParty.IsCaravan && Settings.GetInstance().AttackVillagers;
                var attackPatrols = Settings.GetInstance().AttackPatrolParties && mobileParty.StringId.StartsWith("patrol_party");

                if (mobileParty.IsActive && (mobileParty.IsMilitia || mobileParty.CurrentSettlement == null) && (canAttackVillagers || canAttackCaravans || mobileParty.IsLordParty ||
                    (mobileParty.IsMilitia && mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.IsVillage && Settings.GetInstance().RaidVillages))
                    || attackPatrols

                    )
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
                var targetPosition = targetDirectionNormalized * 20 + party.Position2D;

                for (int i = 0; i < tryCount; i++)
                {
                    targetPosition = targetDirectionNormalized * 20 + party.Position2D;
                    targetPosition.RotateCCW((i * 360 / tryCount) * MathF.PI / 180);

                    var faceIndex = Campaign.Current.MapSceneWrapper.GetFaceIndex(targetPosition);

                    if (faceIndex.IsValid())
                    {
                        break;
                    }
                }
                party.Ai.SetMoveGoToPoint(targetPosition);
                DisableThinkForHours(party, 1);
            }
            else if (selectedTarget != null && !selectedTarget.IsMilitia)
            {
                SetPartyAiAction.GetActionForEngagingParty(party, selectedTarget);
                DisableThinkForHours(party, 6);
            }
            else if (selectedTarget != null && selectedTarget.IsMilitia && !selectedTarget.CurrentSettlement.IsRaided
                    && !selectedTarget.CurrentSettlement.IsUnderRaid)
            {
                SetPartyAiAction.GetActionForRaidingSettlement(party, selectedTarget.CurrentSettlement);
                DisableThinkForHours(party, 24);
            }
            else/* if (MBRandom.RandomFloat < 0.7f)*/
            {
                var settlement = SettlementHelper.FindRandomSettlement((Settlement s) => s.IsVillage && Campaign.Current.Models.MapDistanceModel.GetDistance(party, s) < 50);
                if (settlement == null)
                {
                    settlement = SettlementHelper.FindNearestVillage(toMapPoint: party);
                }

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
            var party = DeserterPartyComponent.CreateDeserterParty("deserter_party_1", deserterClan, deserterOf, homeSettlement, finalRoster, position, 15, 10);
            _deserterParties.Add(party);
            ClearDecision(party);
        }


        private TroopRoster GetTroopsForParty(Kingdom deserterOf, int extraTroops = 0)
        {
            var roster = TroopRoster.CreateDummyTroopRoster();

            var factionTroops = GetTroopTreeOfFaction(deserterOf);
            var deserterTroops = GetTroopTreeOfFaction(GetDeserterClan(deserterOf));

            if (factionTroops.Any())
            {
                for (int i = 0; i < Settings.GetInstance().MinimumPartyTroopSize; i++)
                {
                    roster.AddToCounts(factionTroops.GetRandomElement(), 1);
                }
            }

            if (deserterTroops.Any())
            {
                for (int i = 0; i < extraTroops + (Settings.GetInstance().MinimumPartyTroopSize - roster.TotalManCount); i++)
                {
                    roster.AddToCounts(deserterTroops.GetRandomElement(), 1);
                }
            }
          
            return roster;
        }

        private float GetAttackScoreOfParty(MobileParty enemyParty, MobileParty party)
        {
            if (party.GetTotalStrengthWithFollowers() < 1.33f * enemyParty.GetTotalStrengthWithFollowers())
            {
                return -1f;
            }

            if(enemyParty.IsMainParty && !CampaignCheats.MainPartyIsAttackable)
            {
                return -1f;
            }

            var strengthFactor = Math.Max(party.Party.TotalStrength - enemyParty.GetTotalStrengthWithFollowers(), 0f) / 50f;

            var multiplier = enemyParty.IsLordParty ? 0.6f : (enemyParty.IsCaravan || enemyParty.IsVillager ? 1.2f : 1.0f) ;

            var distance = Campaign.Current.Models.MapDistanceModel.GetDistance(party, enemyParty);

            return multiplier * (strengthFactor + 0.5f) + (1 / (distance * distance));
        }

        private Clan GetDeserterClan(Kingdom faction)
        {
            if (faction.StringId == "empire" || faction.StringId == "empire_w" || faction.StringId == "empire_s")
            {
                return Campaign.Current.CampaignObjectManager.Find<Clan>(x => x.StringId == $"deserters_empire");
            }
            else if (CalradianDesertersModuleManager.DeserterClanIds.Contains($"deserters_{faction.StringId}"))
            {
                return Campaign.Current.CampaignObjectManager.Find<Clan>(x => x.StringId == $"deserters_{faction.StringId}");
            }

            return Campaign.Current.CampaignObjectManager.Find<Clan>(x => x.StringId == $"deserters_empire");
        }

        private bool IsDeserterClan(Clan clan)
        {
            if (_deserterClans.TryGetValue(clan.StringId, out var v) && v == clan)
            {
                return true;
            }

            foreach (var kingdom in Kingdom.All)
            {
                if (clan == GetDeserterClan(kingdom))
                {
                    _deserterClans[clan.StringId] = clan;
                    return true;
                }
            }

            return false;
        }

        private void InitializeDeserterClans(bool onLoad = false)
        {
            foreach (var stringId in CalradianDesertersModuleManager.DeserterClanIds)
            {
                var deserterClan = Campaign.Current.CampaignObjectManager.Find<Clan>(stringId);

                if (deserterClan == null && onLoad)
                {
                    var document = new XmlDocument();
                    var tr = new StreamReader(ModuleHelper.GetModuleFullPath("CalradianDeserters") + "ModuleData/clans.xml");
                    document.LoadXml(tr.ReadToEnd());
                    tr.Close();
                    LoadXml(document, stringId);
                    deserterClan = Campaign.Current.CampaignObjectManager.Find<Clan>(stringId);
                }

                if (deserterClan != null)
                {
                    deserterClan.BasicTroop = MBObjectManager.Instance.GetObject<CharacterObject>("deserter");

                    if (deserterClan.Leader == null)
                    {
                        var deserterLeader = HeroCreator.CreateSpecialHero(deserterClan.MinorFactionCharacterTemplates.GetRandomElementInefficiently(), null, deserterClan);
                        deserterLeader.ChangeState(Hero.CharacterStates.Dead);
                        deserterLeader.CharacterObject.HiddenInEncylopedia = true;

                        deserterClan.SetLeader(deserterLeader);
                    }
                }
            }

            DeclareWarWithFactions();
        }

        private void DeclareWarWithFactions()
        {
            foreach (var kingdom in Kingdom.All)
            {
                var deserterClan = GetDeserterClan(kingdom);
                foreach (var kingdom2 in Kingdom.All)
                {
                    if (!kingdom2.IsAtWarWith(deserterClan))
                    {
                        FactionManager.DeclareWar(kingdom2, deserterClan, true);
                    }
                }


                foreach (var clan in Clan.NonBanditFactions)
                {
                    if (!clan.IsAtWarWith(deserterClan) && !IsDeserterClan(clan))
                    {
                        FactionManager.DeclareWar(clan, deserterClan, true);
                    }
                }
            }
        }

        private void DeclareWarWithFaction(IFaction faction)
        {
            foreach (var kingdom in Kingdom.All)
            {
                var deserterClan = GetDeserterClan(kingdom);
                FactionManager.DeclareWar(faction, deserterClan, true);
            }
        }

        private List<CharacterObject> GetTroopTreeOfFaction(IFaction faction)
        {
            int minTier = MBMath.ClampInt(Settings.GetInstance().MinimumTroopTier, 0, 5);
            if (_troopTrees.TryGetValue((faction, minTier), out var tree))
            {
                return tree;
            }

            tree = CharacterHelper.GetTroopTree(faction.BasicTroop, minTier).ToList();
            _troopTrees.Add((faction, minTier), tree);
            return tree;
        }

        public void LoadXml(XmlDocument doc, string stringId = null)
        {
            XmlNode node = doc.ChildNodes[1].ChildNodes[0];
            while (node != null)
            {
                if (node.NodeType != XmlNodeType.Comment)
                {
                    string objName = node.Attributes["id"].Value;
                    if (stringId == null || stringId == objName)
                    {
                        var clan = Clan.CreateClan(objName);
                        clan.Deserialize(MBObjectManager.Instance, node);
                        clan.AfterInitialized();
                    }
                }

                node = node.NextSibling;
            }
        }


        [CommandLineFunctionality.CommandLineArgumentFunction("create_deserters_around", "deserters")]
        public static string CreateDeserters(List<string> str)
        {
            var behavior = Campaign.Current.GetCampaignBehavior<CalradianDesertersCampaignBehavior>();
            var nearestVillage = SettlementHelper.FindNearestVillage(toMapPoint: MobileParty.MainParty);

            foreach (var kingdom in Kingdom.All)
            {
                behavior.CreateDeserterParty(nearestVillage, behavior.GetDeserterClan(kingdom), kingdom, behavior.GetTroopsForParty(kingdom), MobileParty.MainParty.Position2D);
            }
            return "OK";
        }
    }
}
