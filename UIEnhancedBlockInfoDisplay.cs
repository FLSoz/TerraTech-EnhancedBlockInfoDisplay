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
        private static Dictionary<BlockTypes, BlockMetadata> metadataCache = new Dictionary<BlockTypes, BlockMetadata>();
        internal static BlockMetadata GetMetadata(BlockTypes blockType, TankBlock blockPrefab = null)
        {
            if (blockPrefab == null)
            {
                blockPrefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(blockType);
            }
            if (blockPrefab == null)
            {
                return null;
            }
            if (!metadataCache.TryGetValue(blockType, out BlockMetadata data))
            {
                data = new BlockMetadata(blockPrefab);
                metadataCache[blockType] = data;
            }
            return data;
        }

        internal static void ResetCaches()
        {
            metadataCache.Clear();
        }

        public void UpdateBlock(BlockTypes blockType)
        {
            TankBlock blockPrefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(blockType);
            if (blockPrefab != null)
            {
                BlockMetadata data = GetMetadata(blockType, blockPrefab);
                
                // hp
                if (this.m_HP != null)
                {
                    int hp = data.MaxHP;
                    this.m_HP.text = hp > 0 ? hp.ToString() : "";
                }
                if (this.m_DPS != null)
                {
                    this.m_DPS.text = data.DPS.ToString();
                }

                // weapon
                if (this.m_Range != null)
                {
                    this.m_Range.text = data.Range.ToString();
                    SetCanvasGroupState(this.m_Range.transform.parent, data.HasRange);
                }
                if (this.m_MuzzleVelocity != null)
                {
                    this.m_MuzzleVelocity.text = data.MuzzleVelocity > 0.0f ? data.MuzzleVelocity.ToString() : "";
                }

                // projectile
                if (this.m_ShotDamage != null)
                {
                    this.m_ShotDamage.text = data.ProjectileDamage.ToString();
                    SetCanvasGroupState(this.m_ShotDamage.transform.parent, data.HasProjectile);
                }
                if (this.m_ShotBurstCount != null)
                {
                    this.m_ShotBurstCount.text = data.ShotBurstCount.ToString();
                }

                // explosion
                if (this.m_ExplosionDamage != null)
                {
                    this.m_ExplosionDamage.text = data.ExplosionDamage.ToString();
                    SetCanvasGroupState(this.m_ExplosionDamage.transform.parent, data.HasExplosion);
                }
                if (this.m_ExplosionDamageType != null)
                {
                    this.m_ExplosionDamageType.Set(data.ExplosionDamageType, data.HasExplosion);
                }
                if (this.m_ExplosionRadius != null)
                {
                    this.m_ExplosionRadius.text = data.ExplosionRadius.ToString();
                    SetCanvasGroupState(this.m_ExplosionRadius.transform.parent, data.HasExplosion);
                }
                if (this.m_ExplosionMaxRadius != null)
                {
                    this.m_ExplosionMaxRadius.text = data.ExplosionMaxEffectRadius.ToString();
                }

                // radar
                if (this.m_RadarRange != null)
                {
                    this.m_RadarRange.text = data.RadarRange.ToString();
                    SetCanvasGroupState(this.m_RadarRange.transform.parent, data.HasRadar);
                }
                if (this.m_RadarType != null)
                {
                    this.m_RadarType.text = data.RadarType.ToString();
                }

                // battery
                if (this.m_EnergyCapacity != null)
                {
                    this.m_EnergyCapacity.text = data.EnergyCapacity.ToString();
                    SetCanvasGroupState(this.m_EnergyCapacity.transform.parent, data.HasEnergy);
                }
                if (this.m_EnergyGeneration != null)
                {
                    this.m_EnergyGeneration.text = data.EnergyGeneration.ToString();
                }
                if (this.m_EnergyGenerationTitle != null)
                {
                    this.m_EnergyGenerationTitle.text = data.EnergyGenerationTitle;
                }

                // shield
                // row 1
                if (this.m_ShieldEnergyPerDamage != null)
                {
                    this.m_ShieldEnergyPerDamage.text = data.ShieldEnergyPerDamage.ToString();
                    SetCanvasGroupState(this.m_ShieldEnergyPerDamage.transform.parent, data.HasShield);
                }
                if (this.m_ShieldEnergyPerHealed != null) {
                    this.m_ShieldEnergyPerHealed.text = data.ShieldEnergyPerHealed.ToString();
                }
                // row 2
                if (this.m_ShieldPassiveDrain != null)
                {
                    this.m_ShieldPassiveDrain.text = data.ShieldPassiveDrain.ToString();
                    SetCanvasGroupState(this.m_ShieldPassiveDrain.transform.parent, data.HasShield);
                }
                if (this.m_ShieldStartupDrain != null) {
                    this.m_ShieldStartupDrain.text = data.ShieldStartupDrain.ToString();
                }
            }
        }

        private static void SetCanvasGroupState(Transform trans, bool state)
        {
            /* CanvasGroup canvasGroup = trans.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                if (state != (canvasGroup.alpha == 1f))
                {
                    canvasGroup.alpha = state ? 1f : 0f;
                }
            } */
            trans.gameObject.SetActive(state);
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

        private static readonly FieldInfo m_DoBlockAllowedChecksDefaultField = AccessTools.Field(typeof(UIBlockInfoDisplay), "m_DoBlockAllowedChecksDefault");
        public bool m_DoBlockAllowedChecksDefault { get => (bool)m_DoBlockAllowedChecksDefaultField.GetValue(this.m_BlockInfoDisplay); }

        #endregion ReflectionFields

        private TextMeshProUGUI m_HP;
        private TextMeshProUGUI m_DPS;

        private TextMeshProUGUI m_Range;
        private TextMeshProUGUI m_MuzzleVelocity;

        private TextMeshProUGUI m_ShotDamage;
        private TextMeshProUGUI m_ShotBurstCount;

        private TextMeshProUGUI m_ExplosionDamage;
        private UIDamageTypeDisplay m_ExplosionDamageType;

        private TextMeshProUGUI m_ExplosionRadius;
        private TextMeshProUGUI m_ExplosionMaxRadius;

        private TextMeshProUGUI m_RadarType;
        private TextMeshProUGUI m_RadarRange;

        private TextMeshProUGUI m_EnergyCapacity;
        private TextMeshProUGUI m_EnergyGeneration;
        private TextMeshProUGUI m_EnergyGenerationTitle;

        private TextMeshProUGUI m_ShieldEnergyPerDamage;
        private TextMeshProUGUI m_ShieldEnergyPerHealed;
        private TextMeshProUGUI m_ShieldStartupDrain;
        private TextMeshProUGUI m_ShieldPassiveDrain;

        private static void SetSameParent(Transform newTrans, Transform sourceTrans)
        {
            newTrans.SetParent(sourceTrans.parent, false);
            newTrans.localPosition = sourceTrans.localPosition;
            newTrans.localScale = sourceTrans.localScale;
            newTrans.localEulerAngles = sourceTrans.localEulerAngles;
        }

        private static TextMeshProUGUI CreateDamageDisplayTitle(UIDamageTypeDisplay damageDisplay)
        {
            UILocalisedText damageableTitle = damageDisplay.GetComponentInChildren<UILocalisedText>();
            GameObject copy = GameObject.Instantiate(damageableTitle.gameObject);
            UILocalisedText tempTitle = copy.GetComponent<UILocalisedText>();

            SetSameParent(tempTitle.transform, damageableTitle.transform);

            TextMeshProUGUI actualTitle = tempTitle.GetComponent<TextMeshProUGUI>();
            GameObject.DestroyImmediate(tempTitle);
            actualTitle.ForceMeshUpdate();
            return actualTitle;
        }
        private static readonly FieldInfo m_DamageDisplayNameText = AccessTools.Field(typeof(UIDamageTypeDisplay), "m_NameText");
        private static TextMeshProUGUI CreateDamageDisplayValue(UIDamageTypeDisplay damageDisplay)
        {
            TextMeshProUGUI currNameText = (TextMeshProUGUI)m_DamageDisplayNameText.GetValue(damageDisplay);
            GameObject copy = GameObject.Instantiate(currNameText.gameObject);
            TextMeshProUGUI valueDisplay = copy.GetComponent<TextMeshProUGUI>();

            SetSameParent(valueDisplay.transform, currNameText.transform);

            valueDisplay.fontSize = currNameText.fontSize;
            valueDisplay.ForceMeshUpdate();
            return valueDisplay;
        }

        public enum DisplayType
        {
            INVENTORY,
            FABRICATOR,
            TOOLTIP
        }

        public DisplayType m_DisplayType;

        private Transform AddRow(string left, string right, out TextMeshProUGUI leftComponent, out TextMeshProUGUI rightComponent, bool maintainDamage = false)
        {
            leftComponent = null;
            rightComponent = null;

            if (this.m_DamageableTypeDisplay != null)
            {
                UIDamageTypeDisplay clone = GameObject.Instantiate<UIDamageTypeDisplay>(this.m_DamageableTypeDisplay);
                GameObject cloneObj = clone.gameObject;
                if (!maintainDamage)
                {
                    GameObject.DestroyImmediate(clone);
                }

                // repurpose these values
                TextMeshProUGUI LeftTitle = cloneObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                UILocalisedText oldTitle = LeftTitle.GetComponent<UILocalisedText>();
                GameObject.DestroyImmediate(oldTitle);
                LeftTitle.text = left;
                LeftTitle.alignment = TextAlignmentOptions.TopLeft;
                leftComponent = cloneObj.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                leftComponent.alignment = TextAlignmentOptions.TopLeft;

                if (!maintainDamage)
                {
                    LeftTitle.transform.localPosition += new Vector3(-50, 0, 0);
                    leftComponent.transform.localPosition += new Vector3(-50, 0, 0);
                }

                TextMeshProUGUI RightTitle = cloneObj.transform.GetChild(4).GetComponent<TextMeshProUGUI>();
                RightTitle.text = right;
                RightTitle.alignment = TextAlignmentOptions.TopRight;
                RightTitle.transform.localPosition += new Vector3(40, 0, 0);
                rightComponent = cloneObj.transform.GetChild(5).GetComponent<TextMeshProUGUI>();
                rightComponent.alignment = TextAlignmentOptions.TopRight;

                SetSameParent(cloneObj.transform, this.m_DamageableTypeDisplay.transform);
                // cloneObj.transform.SetSiblingIndex(cloneObj.transform.childCount - 1);

                // Delete icon
                if (!maintainDamage)
                {
                    Transform icon = cloneObj.transform.GetChild(3);
                    icon.gameObject.SetActive(false);
                    GameObject.DestroyImmediate(icon);
                }
                return cloneObj.transform;
            }
            return null;
        }

        private void SetupInventoryType()
        {
            // Setup HP
            if (this.m_DamageableTypeDisplay != null)
            {
                TextMeshProUGUI hpTitle = CreateDamageDisplayTitle(this.m_DamageableTypeDisplay);
                hpTitle.text = "HP";
                hpTitle.alignment = TextAlignmentOptions.TopRight;
                this.m_HP = CreateDamageDisplayValue(this.m_DamageableTypeDisplay);
                this.m_HP.alignment = TextAlignmentOptions.TopRight;

                Vector3 descripLocalPos = this.m_HP.transform.localPosition;
                this.m_HP.transform.localPosition = new Vector3(
                    70,
                    descripLocalPos.y,
                    descripLocalPos.z
                );
            }
            // setup DPS
            if (this.m_DamageTypeDisplay != null)
            {
                TextMeshProUGUI dpsTitle = CreateDamageDisplayTitle(this.m_DamageTypeDisplay);
                dpsTitle.text = "DPS";
                dpsTitle.alignment = TextAlignmentOptions.TopRight;
                this.m_DPS = CreateDamageDisplayValue(this.m_DamageTypeDisplay);
                this.m_DPS.alignment = TextAlignmentOptions.TopRight;

                Vector3 descripLocalPos = this.m_DPS.transform.localPosition;
                this.m_DPS.transform.localPosition = new Vector3(
                    210,
                    descripLocalPos.y,
                    descripLocalPos.z
                );
            }

            // Add range and Muzzle Velocity;
            AddRow("Range", "Muzzle Velocity", out this.m_Range, out this.m_MuzzleVelocity);

            // Add Shot dmg and burst count
            AddRow("Shot DMG", "Burst Count", out this.m_ShotDamage, out this.m_ShotBurstCount);

            // Add explosion stuff
            Transform explosionRowTrans = AddRow("Explosion Type", "Explosion DMG", out TextMeshProUGUI discard, out this.m_ExplosionDamage, true);
            if (explosionRowTrans != null)
            {
                this.m_ExplosionDamageType = explosionRowTrans.GetComponent<UIDamageTypeDisplay>();
            }
            AddRow("Blast Radius", "Core Radius", out this.m_ExplosionRadius, out this.m_ExplosionMaxRadius);

            // Add radar
            AddRow("Radar Type", "Radar Range", out this.m_RadarType, out this.m_RadarRange);

            // Add energy
            Transform energyRowTrans = AddRow("Energy Capacity", "Energy Generation", out this.m_EnergyCapacity, out this.m_EnergyGeneration);
            if (energyRowTrans != null)
            {
                TextMeshProUGUI[] textThings = energyRowTrans.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI textMesh in textThings)
                {
                    if (textMesh.text.Equals("Energy Generation"))
                    {
                        this.m_EnergyGenerationTitle = textMesh;
                        break;
                    }
                }
            }

            // Add shield
            AddRow("Energy/DMG", "Energy/HP", out this.m_ShieldEnergyPerDamage, out this.m_ShieldEnergyPerHealed);
            AddRow("Startup Energy", "Bubble Energy/s", out this.m_ShieldStartupDrain, out this.m_ShieldPassiveDrain);

            // set description to be last
            if (this.m_DescriptionText != null)
            {
                Transform descriptionChild = this.transform.Find("Block info/DescriptionArea");
                if (descriptionChild != null)
                {
                    descriptionChild.transform.SetAsLastSibling();
                }
            }

            // stretch UI
            float scale = 1.5f;
            Transform background = this.transform.GetChild(0);
            background.localScale = new Vector3(background.localScale.x, background.localScale.y * scale, background.localScale.z);
            background.localPosition += new Vector3(0, background.localPosition.y * (scale - 1), 0);
            
            Transform collapseButton = this.transform.GetChild(1);
            collapseButton.localPosition += new Vector3(0, collapseButton.localPosition.y * (scale - 1), 0);

            Transform blockInfo = this.transform.GetChild(2);
            blockInfo.localPosition -= new Vector3(0, blockInfo.localPosition.y * (scale - 1), 0);

            Transform dropShadow = this.transform.GetChild(3);
            dropShadow.localScale = new Vector3(dropShadow.localScale.x, dropShadow.localScale.y * scale, dropShadow.localScale.z);
            dropShadow.localPosition += new Vector3(0, dropShadow.localPosition.y * (scale - 1), 0);

        }

        private void SetupFabricatorType()
        {

        }

        private void SetupTooltipType() {
        }

        internal void Setup(UIBlockInfoDisplay blockInfoDisplay)
        {
            this.m_BlockInfoDisplay = blockInfoDisplay;

            // setup HP and DPS
            if (!this.m_DoBlockAllowedChecksDefault || this.m_ItemBlockAttrPrefab == null || this.m_DescriptionText == null)
            {
                this.m_DisplayType = DisplayType.TOOLTIP;
                this.SetupTooltipType();
            }
            else if (this.m_PurchaseBlockButton == null)
            {
                this.m_DisplayType = DisplayType.INVENTORY;
                this.SetupInventoryType();
            }
            else
            {
                this.m_DisplayType = DisplayType.FABRICATOR;
                this.SetupFabricatorType();
            }
        }
    }

    [HarmonyPatch(typeof(UIPaletteBlockSelect))]
    internal static class PatchExpandedDisplay
    {

    }

    [HarmonyPatch(typeof(UIBlockInfoDisplay), "UpdateBlock")]
    internal static class PatchBlockDisplay
    {
        [HarmonyPriority(Priority.High)]
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
