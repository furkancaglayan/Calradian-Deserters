using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using CalradianDeserters.Extensions;

namespace CalradianDeserters.EncyclopediaPages
{
    [OverrideEncyclopediaModel(new[] { typeof(Clan) })]
    public class CustomClanEncyclopediaPage : DefaultEncyclopediaClanPage
    {
        protected override IEnumerable<EncyclopediaFilterGroup> InitializeFilterItems()
        {
            return base.InitializeFilterItems();
        }

        protected override IEnumerable<EncyclopediaListItem> InitializeListItems()
        {
            return base.InitializeListItems();
        }

        protected override IEnumerable<EncyclopediaSortController> InitializeSortControllers()
        {
            return base.InitializeSortControllers();
        }

        public override bool IsValidEncyclopediaItem(object o)
        {
            if (o is Clan c && c.IsDeserterClan())
            {
                return false;
            }
            return base.IsValidEncyclopediaItem(o);
        }
    }
}
