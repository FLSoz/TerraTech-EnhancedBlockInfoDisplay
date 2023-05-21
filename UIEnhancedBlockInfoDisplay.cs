using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using TMPro;
using UnityEngine.Serialization;

namespace EnhancedBlockInfoDisplay
{
    public class UIEnhancedBlockInfoDisplay : MonoBehaviour
    {
        private static Dictionary<BlockTypes, int> hpCache = new Dictionary<BlockTypes, int>();
        private static int GetHP(BlockTypes blockType, TankBlock blockPrefab = null)
        {
            if (blockPrefab == null)
            {
                blockPrefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(blockType);
            }
            if (blockPrefab == null)
            {
                return -1;
            }
            if (!hpCache.TryGetValue(blockType, out int hp))
            {
                ModuleDamage moduleDamage = blockPrefab.GetComponent<ModuleDamage>();
                if (moduleDamage != null)
                {
                    hp = moduleDamage.maxHealth;
                    hpCache[blockType] = hp;
                }
                else
                {
                    hp = -1;
                }
            }
            return hp;
        }

        internal static void ResetCaches()
        {
            hpCache.Clear();
        }

        public void UpdateBlock(BlockTypes blockType)
        {
            TankBlock blockPrefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(blockType);
            if (blockPrefab != null)
            {
                if (this.m_HP != null)
                {
                    int hp = GetHP(blockType, blockPrefab);
                    this.m_HP.text = hp > 0 ? hp.ToString() : "<UNKNOWN>";
                }
                if (this.m_DPS != null)
                {
                    this.m_DPS.text = "<UNKNOWN>";
                }
            }
        }

        private UIBlockInfoDisplay m_BlockInfoDisplay;
        #region ReflectionFields
        private static readonly FieldInfo m_NameTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_NameText");
		public TextMeshProUGUI m_NameText { get => (TextMeshProUGUI)m_NameTextField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_DescriptionTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_DescriptionText");
		public TextMeshProUGUI m_DescriptionText { get => (TextMeshProUGUI)m_DescriptionTextField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_DescriptionScrollerField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_DescriptionScroller");
		public UITimedAutoScrollView m_DescriptionScroller { get => (UITimedAutoScrollView)m_DescriptionScrollerField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_BlockDescriptionWarningColourField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_BlockDescriptionWarningColour");
		public Color m_BlockDescriptionWarningColour { get => (Color)m_BlockDescriptionWarningColourField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_BlockCategoryTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_BlockCategoryText");
        public TextMeshProUGUI m_BlockCategoryText { get => (TextMeshProUGUI)m_BlockCategoryTextField.GetValue(this.m_BlockInfoDisplay); }
        private static readonly FieldInfo m_BlockCategoryIconField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_BlockCategoryIcon");
		public Image m_BlockCategoryIcon { get => (Image)m_BlockCategoryIconField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_BlockCategoryTooltipField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_BlockCategoryTooltip");
		public TooltipComponent m_BlockCategoryTooltip { get => (TooltipComponent)m_BlockCategoryTooltipField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_CorpIconField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_CorpIcon");
		public Image m_CorpIcon { get => (Image)m_CorpIconField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_GradeValueTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_GradeValueText");
		public TextMeshProUGUI m_GradeValueText { get => (TextMeshProUGUI)m_GradeValueTextField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_GradeStringTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_GradeStringText");
		public TextMeshProUGUI m_GradeStringText { get => (TextMeshProUGUI)m_GradeStringTextField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_CorpAndGradeTooltipField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_CorpAndGradeTooltip");
		public TooltipComponent m_CorpAndGradeTooltip { get => (TooltipComponent)m_CorpAndGradeTooltipField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_RarityTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_RarityText");
		public TextMeshProUGUI m_RarityText { get => (TextMeshProUGUI)m_RarityTextField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_RarityIconField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_RarityIcon");
		public Image m_RarityIcon { get => (Image)m_RarityIconField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_RarityTooltipField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_RarityTooltip");
		public TooltipComponent m_RarityTooltip { get => (TooltipComponent)m_RarityTooltipField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_LimiterCostTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_LimiterCostText");
		public TextMeshProUGUI m_LimiterCostText { get => (TextMeshProUGUI)m_LimiterCostTextField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_DamageTypeDisplayField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_DamageTypeDisplay");
		public UIDamageTypeDisplay m_DamageTypeDisplay { get => (UIDamageTypeDisplay)m_DamageTypeDisplayField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_DamageableTypeDisplayField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_DamageableTypeDisplay");
		public UIDamageTypeDisplay m_DamageableTypeDisplay { get => (UIDamageTypeDisplay)m_DamageableTypeDisplayField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_ItemBlockAttrPrefabField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_ItemBlockAttrPrefab");
		public InfoPanelItemAttribute m_ItemBlockAttrPrefab { get => (InfoPanelItemAttribute)m_ItemBlockAttrPrefabField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_BlockAttrContainerField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_BlockAttrContainer");
		public RectTransform m_BlockAttrContainer { get => (RectTransform)m_BlockAttrContainerField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_DisableBlockAttrContainerOnEmptyField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_DisableBlockAttrContainerOnEmpty");
		public bool m_DisableBlockAttrContainerOnEmpty { get => (bool)m_DisableBlockAttrContainerOnEmptyField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_PurchaseBlockButtonField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_PurchaseBlockButton");
		public Button m_PurchaseBlockButton { get => (Button)m_PurchaseBlockButtonField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_PurchaseButtonImageField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_PurchaseButtonImage");
		public Image m_PurchaseButtonImage { get => (Image)m_PurchaseButtonImageField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_PurchaseOkColourField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_PurchaseOkColour");
		public Color m_PurchaseOkColour { get => (Color)m_PurchaseOkColourField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_PurchaseInvalidColourField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_PurchaseInvalidColour");
		public Color m_PurchaseInvalidColour { get => (Color)m_PurchaseInvalidColourField.GetValue(this.m_BlockInfoDisplay); }

        private static readonly FieldInfo m_PurchaseBlockPriceTextField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_PurchaseBlockPriceText");
		public TextMeshProUGUI m_PurchaseBlockPriceText { get => (TextMeshProUGUI)m_PurchaseBlockPriceTextField.GetValue(this.m_BlockInfoDisplay); }

        #endregion ReflectionFields

        private TextMeshProUGUI m_HP;
        private TextMeshProUGUI m_DPS;

        private static TextMeshProUGUI CreateDamageDisplayTitle(UIDamageTypeDisplay damageDisplay)
        {
            UILocalisedText damageableTitle = damageDisplay.GetComponentInChildren<UILocalisedText>();
            UILocalisedText tempTitle = GameObject.Instantiate<UILocalisedText>(damageableTitle);
            tempTitle.transform.SetParent(damageableTitle.transform.parent, false);
            tempTitle.transform.localPosition = damageableTitle.transform.localPosition;
            TextMeshProUGUI actualTitle = tempTitle.GetComponent<TextMeshProUGUI>();
            GameObject.Destroy(tempTitle);
            return actualTitle;
        }
        private static readonly FieldInfo m_DamageDisplayNameText = AccessTools.Field(typeof(UIDamageTypeDisplay), "m_NameText");
        private static TextMeshProUGUI CreateDamageDisplayValue(UIDamageTypeDisplay damageDisplay)
        {
            TextMeshProUGUI currNameText = (TextMeshProUGUI)m_DamageDisplayNameText.GetValue(damageDisplay);
            TextMeshProUGUI valueDisplay = GameObject.Instantiate<TextMeshProUGUI>(currNameText);
            valueDisplay.transform.SetParent(currNameText.transform.parent);
            valueDisplay.transform.localPosition = currNameText.transform.localPosition;
            return valueDisplay;
        }

        internal void Setup(UIBlockInfoDisplay blockInfoDisplay)
        {
            this.m_BlockInfoDisplay = blockInfoDisplay;

            // setup HP and DPS
            if (this.m_DamageableTypeDisplay != null)
            {
                TextMeshProUGUI hpTitle = CreateDamageDisplayTitle(this.m_DamageableTypeDisplay);
                hpTitle.text = "HP";
                hpTitle.alignment = TextAlignmentOptions.Right;
                this.m_HP = CreateDamageDisplayValue(this.m_DamageableTypeDisplay);
                this.m_HP.alignment = TextAlignmentOptions.Right;
            }
            if (this.m_DamageTypeDisplay != null)
            {
                TextMeshProUGUI dpsTitle = CreateDamageDisplayTitle(this.m_DamageTypeDisplay);
                dpsTitle.text = "DPS";
                dpsTitle.alignment = TextAlignmentOptions.Right;
                this.m_DPS = CreateDamageDisplayValue(this.m_DamageTypeDisplay);
                this.m_DPS.alignment = TextAlignmentOptions.Right;
            }
        }
    }

    [HarmonyPatch(typeof(UIBlockInfoDisplay), "UpdateBlock")]
    internal static class PatchBlockDisplay
    {
        internal static void Postfix(UIBlockInfoDisplay __instance, BlockTypes blockType)
        {
            UIEnhancedBlockInfoDisplay enhancedDisplay = __instance.GetComponent<UIEnhancedBlockInfoDisplay>();
            if (enhancedDisplay == null)
            {
                enhancedDisplay = __instance.gameObject.AddComponent<UIEnhancedBlockInfoDisplay>();
                enhancedDisplay.Setup(__instance);
            }

            enhancedDisplay.UpdateBlock(blockType);
        }
    }
}
