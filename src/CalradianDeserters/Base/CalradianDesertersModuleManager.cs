using CalradianDeserters.Settings;
using SandBox.View.Map;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace CalradianPatrols.Base
{
    public class CalradianDesertersModuleManager
    {
        public static CalradianDesertersModuleManager Current { get; private set; }

        //private bool _registered;

        public static Settings Settings
        {
            get
            {
                if (Settings.Instance == null)
                {
                    if (_baseSettings == null)
                    {
                        _baseSettings = new Settings();
                    }

                    return _baseSettings;
                }
                else
                {
                    return Settings.Instance;
                }
            }
        }
        private static Settings _baseSettings;
      

        public CalradianDesertersModuleManager()
        {
            Current = this;
        }
    }
}
