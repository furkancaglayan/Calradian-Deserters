using CalradianDeserters.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace CalradianDeserters.Extensions
{
    public static class CalradianDesertersExtensions
    {
        public static bool IsDeserterParty(this MobileParty mobileParty)
        {
            return mobileParty.PartyComponent is DeserterPartyComponent;
        }

        public static bool IsDeserterParty(this MobileParty mobileParty, out DeserterPartyComponent deserterPartyComponent)
        {
            deserterPartyComponent = null;
            if (mobileParty.PartyComponent != null && mobileParty.PartyComponent is DeserterPartyComponent partyComponent)
            {
                deserterPartyComponent = partyComponent;
                return true;
            }

            return false;
        }

        public static T GetModelByType<T>(this CampaignGameStarter campaignGameStarter) where T : GameModel
        {
            return (T)campaignGameStarter.Models.LastOrDefault(x => x is T);
        }
        public static void Set<T1, T2>(this Dictionary<T1, T2> dict, T1 key, T2 value)
        {
            if (dict.TryGetValue(key, out T2 oldValue))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}
