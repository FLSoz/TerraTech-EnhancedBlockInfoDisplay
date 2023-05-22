using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            UIEnhancedBlockInfoDisplay.ResetCaches();
        }

        public override void Init()
        {
            harmony = new Harmony(HarmonyID);
            harmony.PatchAll();

            CacheVanillaBlockInfo();
        }

        private void CacheVanillaBlockInfo()
        {
            BlockTypes[] allTypes = (BlockTypes[]) Enum.GetValues(typeof(BlockTypes));
            foreach (BlockTypes type in allTypes)
            {
                UIEnhancedBlockInfoDisplay.GetMetadata(type);
            }
        }

        private static readonly FieldInfo m_CurrentSession = AccessTools.Field(typeof(ManMods), "m_CurrentSession");
        // if using TTSMM, cache all this block info immediately
        public void LateInit()
        {
            ModSessionInfo currSession = (ModSessionInfo)m_CurrentSession.GetValue(Singleton.Manager<ManMods>.inst);
            foreach (int blockID in currSession.BlockIDs.Keys)
            {
                UIEnhancedBlockInfoDisplay.GetMetadata((BlockTypes)blockID);
            }
        }
    }
}
