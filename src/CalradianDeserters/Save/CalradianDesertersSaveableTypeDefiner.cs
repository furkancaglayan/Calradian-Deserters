using CalradianDeserters.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace CalradianDeserters.Save
{
    public class CalradianDesertersSaveableTypeDefiner : CampaignBehaviorBase.SaveableCampaignBehaviorTypeDefiner
    {
        public CalradianDesertersSaveableTypeDefiner() : base(4356742)
        {

        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(DeserterPartyComponent), 1);
        }

        protected override void DefineContainerDefinitions()
        {
        }

        protected override void DefineEnumTypes()
        {
        }
    }
}
