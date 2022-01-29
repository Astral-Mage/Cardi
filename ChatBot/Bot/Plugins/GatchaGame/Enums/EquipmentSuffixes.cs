using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Enums
{
    [Serializable]
    public enum EquipmentSuffixes
    {
        [Description("")]
        None,
        [Description("Of Culling")]
        OfCulling,
        [Description("Of Haste")]
        OfHaste,
        [Description("Of Strength")]
        OfStrength,
        [Description("Of Perversion")]
        OfPerversion,
        [Description("Of Rippling Death")]
        OfRipplingDeath,
    }
}
