using System.Collections.Generic;

namespace CalradianDeserters.Base
{
    public class CalradianDesertersModuleManager
    {
        public static CalradianDesertersModuleManager Current { get; private set; }

        public static List<string> DeserterClanIds => new List<string> { "deserters_vlandia",
                                                                         "deserters_empire",
                                                                         "deserters_sturgia",
                                                                         "deserters_battania",
                                                                         "deserters_aserai",
                                                                         "deserters_khuzait"};
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
