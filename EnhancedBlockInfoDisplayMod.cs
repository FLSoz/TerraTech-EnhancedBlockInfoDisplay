using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnhancedBlockInfoDisplay
{
    public class EnhancedBlockInfoDisplayMod : ModBase
    {
        private const string HarmonyID = "com.flsoz.ttmods.EnhancedBlockInfoDisplay";
        internal static Harmony harmony;
        public override void DeInit()
        {
            harmony.UnpatchAll(HarmonyID);
        }

        public override void Init()
        {
            harmony = new Harmony(HarmonyID);
            harmony.PatchAll();
        }
    }
}
