using CalradianDeserters.Base;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v1;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace CalradianDeserters
{
    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "CalradianDeserters";
        public override string DisplayName => new TextObject("{=str_calradiandeserters_mod_name}CalradianDeserters").ToString();
        public override string FolderName => "CalradianDeserters";
        public override string FormatType => "xml";


        [SettingPropertyFloatingInteger("{=str_calradiandeserters_settings_0_0}Minimum Troop Size", 1f, 200, "0", Order = 0, RequireRestart = false, HintText = "{=str_calradiandeserters_settings_0_1}Minimum Deserter Party Size")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Deserters - Basic")]
        public int MinimumPartyTroopSize { get; set; } = 45;

        [SettingPropertyFloatingInteger("{=str_calradiandeserters_settings_1_0}Base Spawn Chance", 0f, 1f, "0%", Order = 0, RequireRestart = false, HintText = "{=str_calradiandeserters_settings_1_1}Base chance to spawn a deserter aprty after battles.")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Deserters - Basic", GroupOrder = 0)]
        public float BaseSpawnPartyChance { get; set; } = 0.8f;

        [SettingProperty("{=str_calradiandeserters_settings_2_0}Attack Caravans", RequireRestart = false, HintText = "{=str_calradiandeserters_settings_2_1}Deserters will attack caravan parties")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Deserters - Basic", GroupOrder = 0)]
        public bool AttackCaravans { get; set; } = true;

        [SettingProperty("{=str_calradiandeserters_settings_3_0}Attack Villagers", RequireRestart = false, HintText = "{=str_calradiandeserters_settings_3_1}Deserters will attack villager parties")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Deserters - Basic", GroupOrder = 0)]
        public bool AttackVillagers { get; set; } = true;

#if CALRADIAN_PATROLSV2
        [SettingProperty("{=str_calradiandeserters_settings_4_0}Attack Patrol Parties", RequireRestart = false, HintText = "{=str_calradiandeserters_settings_4_1}Deserters will attack patrol parties")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Deserters - Basic", GroupOrder = 0)]
        public bool AttackPatrolParties { get; set; } = true;
#endif

        [SettingProperty("{=str_calradiandeserters_settings_5_0}Raid Villages", RequireRestart = false, HintText = "{=str_calradiandeserters_settings_5_1}Deserters will try to raid villages")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Deserters - Basic", GroupOrder = 0)]
        public bool RaidVillages { get; set; } = true;

        [SettingProperty("{=str_calradiandeserters_settings_6_0}Merge Deserter Parties", RequireRestart = false, HintText = "{=str_calradiandeserters_settings_6_1}Different deserter parties will try to merge into a more powerful party")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Deserters - Basic", GroupOrder = 0)]
        public bool MergeParties { get; set; } = true;



        [SettingPropertyFloatingInteger("{=str_calradiandeserters_settings_7_0}Minimum Troop Tier", 1f, 6f, "0", Order = 0, RequireRestart = false, HintText = "{=str_calradiandeserters_settings_7_1}Minimum troop tier for deserter parties")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_advanced}Calradian Deserters - Advanced")]
        public int MinimumTroopTier { get; set; } = 3;


        [SettingPropertyFloatingInteger("{=str_calradiandeserters_settings_8_0}Max Party Size", 50, 300, "0", Order = 0, RequireRestart = false, HintText = "{=str_calradiandeserters_settings_8_1}Maximum allowed party count")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_advanced}Calradian Deserters - Advanced")]
        public int MaxPartyCount { get; set; } = 75;


        [SettingPropertyFloatingInteger("{=str_calradianpatrols_settings_9_0}Deserter Party Speed Bonus", 0f, 10f, "0%", Order = 0, RequireRestart = false, HintText = "{=str_calradiandeserters_settings_9_1}Increases deserter party speed")]
        [SettingPropertyGroup("{=str_calradiandeserters_settings_basic}Calradian Patrols - Basic")]
        public float DeserterPartySpeedBonus { get; set; } = 1.2f;

        public static Settings GetInstance() => CalradianDesertersModuleManager.Settings;
    }
}