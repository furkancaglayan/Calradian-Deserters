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
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Election;

namespace CalradianDeserters.Models
{
    public class CustomDiplomacyModel : DiplomacyModel
    {
        private DiplomacyModel _baseModel;

        public CustomDiplomacyModel(CampaignGameStarter gameStarter)
        {
            _baseModel = CalradianDesertersExtensions.GetModelByType<DiplomacyModel>(gameStarter);
        }

        public override int MaxRelationLimit => _baseModel.MaxRelationLimit;
        public override int MinRelationLimit => _baseModel.MinRelationLimit;
        public override int MaxNeutralRelationLimit => _baseModel.MaxNeutralRelationLimit;
        public override int MinNeutralRelationLimit => _baseModel.MinNeutralRelationLimit;
        public override int MinimumRelationWithConversationCharacterToJoinKingdom => _baseModel.MinimumRelationWithConversationCharacterToJoinKingdom;
        public override int GiftingTownRelationshipBonus => _baseModel.GiftingTownRelationshipBonus;
        public override int GiftingCastleRelationshipBonus => _baseModel.GiftingCastleRelationshipBonus;

        public override bool CanSettlementBeGifted(Settlement settlement)
        {
            return _baseModel.CanSettlementBeGifted(settlement);
        }

        public override float DenarsToInfluence()
        {
           return _baseModel.DenarsToInfluence();
        }

        public override IEnumerable<BarterGroup> GetBarterGroups()
        {
            return _baseModel.GetBarterGroups();
        }

        public override int GetBaseRelation(Hero hero, Hero hero1)
        {
           return _baseModel.GetBaseRelation(hero, hero1);
        }

        public override int GetCharmExperienceFromRelationGain(Hero hero, float relationChange, ChangeRelationAction.ChangeRelationDetail detail)
        {
            return _baseModel.GetCharmExperienceFromRelationGain((Hero)hero, relationChange, detail);
        }

        public override float GetClanStrength(Clan clan)
        {
            return _baseModel.GetClanStrength(clan);
        }

        public override int GetDailyTributeForValue(int value)
        {
           return _baseModel.GetDailyTributeForValue((int)value);
        }

        public override int GetEffectiveRelation(Hero hero, Hero hero1)
        {
            return _baseModel.GetEffectiveRelation((Hero)hero, hero1);
        }

        public override float GetHeroCommandingStrengthForClan(Hero hero)
        {
           return _baseModel.GetHeroCommandingStrengthForClan((Hero)hero);
        }

        public override void GetHeroesForEffectiveRelation(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
        {
            _baseModel.GetHeroesForEffectiveRelation(hero1, hero2, out effectiveHero1, out effectiveHero2);
        }

        public override float GetHeroGoverningStrengthForClan(Hero hero)
        {
            return _baseModel.GetHeroGoverningStrengthForClan(hero);
        }

        public override float GetHourlyInfluenceAwardForBeingArmyMember(MobileParty mobileParty)
        {
            return _baseModel.GetHourlyInfluenceAwardForBeingArmyMember(mobileParty);
        }

        public override float GetHourlyInfluenceAwardForBesiegingEnemyFortification(MobileParty mobileParty)
        {
            return _baseModel.GetHourlyInfluenceAwardForBesiegingEnemyFortification(mobileParty);
        }

        public override float GetHourlyInfluenceAwardForRaidingEnemyVillage(MobileParty mobileParty)
        {
            return _baseModel.GetHourlyInfluenceAwardForRaidingEnemyVillage(mobileParty);
        }

        public override int GetInfluenceAwardForSettlementCapturer(Settlement settlement)
        {
            return _baseModel.GetInfluenceAwardForSettlementCapturer(settlement);
        }

        public override int GetInfluenceCostOfAbandoningArmy()
        {
           return _baseModel.GetInfluenceCostOfAbandoningArmy();
        }

        public override int GetInfluenceCostOfAnnexation(Kingdom proposingKingdom)
        {
            return _baseModel.GetInfluenceCostOfAnnexation(proposingKingdom);
        }

        public override int GetInfluenceCostOfChangingLeaderOfArmy()
        {
            return _baseModel.GetInfluenceCostOfChangingLeaderOfArmy();
        }

        public override int GetInfluenceCostOfDisbandingArmy()
        {
            return _baseModel.GetInfluenceCostOfDisbandingArmy();
        }

        public override int GetInfluenceCostOfExpellingClan()
        {
            return _baseModel.GetInfluenceCostOfExpellingClan();
        }

        public override int GetInfluenceCostOfPolicyProposalAndDisavowal()
        {
            return _baseModel.GetInfluenceCostOfPolicyProposalAndDisavowal();
        }

        public override int GetInfluenceCostOfProposingPeace()
        {
            return _baseModel.GetInfluenceCostOfProposingPeace();
        }

        public override int GetInfluenceCostOfProposingWar(Kingdom proposingKingdom)
        {
            return _baseModel.GetInfluenceCostOfProposingWar(proposingKingdom);
        }

        public override int GetInfluenceCostOfSupportingClan()
        {
            return _baseModel.GetInfluenceCostOfSupportingClan();
        }

        public override int GetInfluenceValueOfSupportingClan()
        {
            return _baseModel.GetInfluenceValueOfSupportingClan();
        }

        public override uint GetNotificationColor(ChatNotificationType notificationType)
        {
            return _baseModel.GetNotificationColor(notificationType);
        }

        public override int GetRelationChangeAfterClanLeaderIsDead(Hero deadLeader, Hero relationHero)
        {
            return _baseModel.GetRelationChangeAfterClanLeaderIsDead(deadLeader, relationHero);
        }

        public override int GetRelationChangeAfterVotingInSettlementOwnerPreliminaryDecision(Hero supporter, bool hasHeroVotedAgainstOwner)
        {
            return _baseModel.GetRelationChangeAfterVotingInSettlementOwnerPreliminaryDecision(supporter, hasHeroVotedAgainstOwner);
        }

        public override int GetRelationCostOfDisbandingArmy(bool isLeaderParty)
        {
            return _baseModel.GetRelationCostOfDisbandingArmy(isLeaderParty);
        }

        public override int GetRelationCostOfExpellingClanFromKingdom()
        {
            return _baseModel.GetRelationCostOfExpellingClanFromKingdom();
        }

        public override float GetRelationIncreaseFactor(Hero hero1, Hero hero2, float relationValue)
        {
            return _baseModel.GetRelationIncreaseFactor(hero1, hero2, relationValue);
        }

        public override int GetRelationValueOfSupportingClan()
        {
            return _baseModel.GetRelationValueOfSupportingClan();
        }

        public override float GetScoreOfClanToJoinKingdom(Clan clan, Kingdom kingdom)
        {
            if (clan.IsDeserterClan())
            {
                return float.MinValue;
            }
            return _baseModel.GetRelationValueOfSupportingClan();
        }

        public override float GetScoreOfClanToLeaveKingdom(Clan clan, Kingdom kingdom)
        {
            return _baseModel.GetScoreOfClanToLeaveKingdom(clan, kingdom);
        }

        public override float GetScoreOfDeclaringPeace(IFaction factionDeclaresPeace, IFaction factionDeclaredPeace, IFaction evaluatingFaction, out TextObject reason)
        {
            reason = TextObject.Empty;
            if (factionDeclaresPeace.IsDeserterClan() || factionDeclaredPeace.IsDeserterClan())
            {
                return float.MinValue;
            }

            return _baseModel.GetScoreOfDeclaringPeace(factionDeclaresPeace, factionDeclaredPeace, evaluatingFaction, out reason);
        }

        public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingFaction, out TextObject reason)
        {
            return _baseModel.GetScoreOfDeclaringWar(factionDeclaresWar, factionDeclaredWar, evaluatingFaction, out reason);
        }

        public override float GetScoreOfKingdomToGetClan(Kingdom kingdom, Clan clan)
        {
            if (clan.IsDeserterClan())
            {
                return float.MinValue;
            }
            return _baseModel.GetScoreOfKingdomToGetClan(kingdom, clan);
        }

        public override float GetScoreOfKingdomToHireMercenary(Kingdom kingdom, Clan mercenaryClan)
        {
            if (mercenaryClan.IsDeserterClan())
            {
                return float.MinValue;
            }
            return _baseModel.GetScoreOfKingdomToHireMercenary(kingdom, mercenaryClan);
        }

        public override float GetScoreOfKingdomToSackClan(Kingdom kingdom, Clan clan)
        {
            return _baseModel.GetScoreOfKingdomToSackClan(kingdom, clan);
        }

        public override float GetScoreOfKingdomToSackMercenary(Kingdom kingdom, Clan mercenaryClan)
        {
            return _baseModel.GetScoreOfKingdomToSackMercenary(kingdom, mercenaryClan);
        }

        public override float GetScoreOfLettingPartyGo(MobileParty party, MobileParty partyToLetGo)
        {
            return _baseModel.GetScoreOfLettingPartyGo(party, partyToLetGo);

        }

        public override float GetScoreOfMercenaryToJoinKingdom(Clan clan, Kingdom kingdom)
        {
            if (clan.IsDeserterClan())
            {
                return float.MinValue;
            }
            return _baseModel.GetScoreOfMercenaryToJoinKingdom(clan, kingdom);
        }

        public override float GetScoreOfMercenaryToLeaveKingdom(Clan clan, Kingdom kingdom)
        {
            return _baseModel.GetScoreOfMercenaryToLeaveKingdom(clan, kingdom);
        }

        public override float GetStrengthThresholdForNonMutualWarsToBeIgnoredToJoinKingdom(Kingdom kingdomToJoin)
        {
            return _baseModel.GetStrengthThresholdForNonMutualWarsToBeIgnoredToJoinKingdom(kingdomToJoin);
        }

        public override int GetValueOfDailyTribute(int dailyTributeAmount)
        {
            return _baseModel.GetValueOfDailyTribute(dailyTributeAmount);
        }

        public override float GetValueOfHeroForFaction(Hero examinedHero, IFaction targetFaction, bool forMarriage = false)
        {
            return _baseModel.GetValueOfHeroForFaction(examinedHero, targetFaction, forMarriage);
        }

        public override bool IsClanEligibleToBecomeRuler(Clan clan)
        {
            if (clan.IsDeserterClan())
            {
                return false;
            }

            return _baseModel.IsClanEligibleToBecomeRuler(clan);
        }
    }
}
