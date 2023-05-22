using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedBlockInfoDisplay
{
    public class BlockMetadata
    {
        public int MaxHP = 0;
        public float DPS = 0.0f;

        public bool HasRange = false;
        public float Range = 0.0f;
        public float MuzzleVelocity = 0.0f;

        public int ProjectileDamage = 0;
        public int ShotBurstCount = 0;
        public bool ProjectileHasGravity = false;
        public bool SeekingProjectile = false;
        public bool HasProjectile = false;

        public ManDamage.DamageType ExplosionDamageType;
        public float ExplosionDamage = 0.0f;
        public float ExplosionRadius = 0.0f;
        public float ExplosionMaxEffectRadius = 0.0f;
        public bool HasExplosion = false;

        public ModuleRadar.RadarScanType RadarType = ModuleRadar.RadarScanType.Techs;
        public float RadarRange = 0.0f;
        public bool HasRadar = false;

        public float EnergyCapacity = 0.0f;
        public float EnergyGeneration = 0.0f;
        public string EnergyGenerationTitle = "Energy Generation";
        public bool HasEnergy = false;

        public float ShieldEnergyPerDamage = 0.0f;
        public float ShieldEnergyPerHealed = 0.0f;
        public float ShieldStartupDrain = 0.0f;
        public float ShieldPassiveDrain = 0.0f;
        public bool HasShield = false;

        private static readonly FieldInfo m_ProjectileDamage = AccessTools.Field(typeof(WeaponRound), "m_Damage");
        private static readonly FieldInfo m_Explosion = AccessTools.Field(typeof(Projectile), "m_Explosion");
        private static readonly FieldInfo m_ExplosionDamageType = AccessTools.Field(typeof(Explosion), "m_DamageType");
        private static readonly FieldInfo m_ProjectileGravity = AccessTools.Field(typeof(Projectile), "m_CanHaveGravity");
        private static readonly FieldInfo m_ProjectileLifetime = AccessTools.Field(typeof(Projectile), "m_LifeTime");

        private static readonly FieldInfo m_FlamethrowerDPS = AccessTools.Field(typeof(ModuleWeaponFlamethrower), "m_DPS");
        private static readonly FieldInfo m_FlamethrowerLength = AccessTools.Field(typeof(ModuleWeaponFlamethrower), "m_MaxFlameLength");

        private static readonly FieldInfo m_BeamDPS = AccessTools.Field(typeof(BeamWeapon), "m_DamagePerSecond");
        public readonly static FieldInfo m_BeamFadeOutTime = AccessTools.Field(typeof(BeamWeapon), "m_FadeOutTime");

        private static readonly FieldInfo m_DrillDPS = AccessTools.Field(typeof(ModuleDrill), "m_DamagePerSecond");
        private static readonly FieldInfo m_VibroDPS = AccessTools.Field(typeof(ModuleVibroKnife), "m_DamagePerSecond");

        private static readonly FieldInfo m_GeneratorMultiplier = AccessTools.Field(typeof(ModuleItemConsume), "m_EnergyMultiplier");
        private static readonly FieldInfo m_GeneratorOutput = AccessTools.Field(typeof(ModuleEnergy), "m_OutputPerSecond");

        private static readonly FieldInfo m_RecipeListNames = AccessTools.Field(typeof(ModuleRecipeProvider), "m_RecipeListNames");

        private static readonly FieldInfo m_BurstCooldown = AccessTools.Field(typeof(ModuleWeaponGun), "m_BurstCooldown");
        private static readonly FieldInfo m_ShotCooldown = AccessTools.Field(typeof(ModuleWeaponGun), "m_ShotCooldown");

        public BlockMetadata(TankBlock blockPrefab)
        {
            // Console.WriteLine($"Getting metadata for {blockPrefab.name}");
            // do HP
            ModuleDamage moduleDamage = blockPrefab.GetComponent<ModuleDamage>();
            if (moduleDamage != null)
            {
                this.MaxHP = moduleDamage.maxHealth;
            }
            else
            {
                this.MaxHP = -1;
            }
            // Console.WriteLine("  Got hp");

            // do flamethrower data
            ModuleWeaponFlamethrower flamethrower = blockPrefab.GetComponent<ModuleWeaponFlamethrower>();
            if (flamethrower != null)
            {
                // Console.WriteLine("  Getting Flamethrower stuff");
                this.HasRange = true;
                this.Range = (float)m_FlamethrowerLength.GetValue(flamethrower);
                this.DPS = (float)m_FlamethrowerDPS.GetValue(flamethrower);
                // Console.WriteLine("  Got Flamethrower stuff");
            }

            // do tesla coil data
            ModuleWeaponTeslaCoil coil = blockPrefab.GetComponent<ModuleWeaponTeslaCoil>();
            if (coil != null)
            {
                // Console.WriteLine("  Getting Coil stuff");
                this.HasRange = true;
                this.Range = coil.m_TargetRadius;
                this.DPS = coil.m_DamagePerArc / coil.m_ChargeDurationBeforeFiring;
                // Console.WriteLine("  Got Coil stuff");
            }

            // do drill/vibroknife data
            ModuleDrill drill = blockPrefab.GetComponent<ModuleDrill>();
            if (drill != null)
            {
                this.DPS = (float)m_DrillDPS.GetValue(drill);
            }
            ModuleVibroKnife knife = blockPrefab.GetComponent<ModuleVibroKnife>();
            if (knife != null)
            {
                this.DPS = (float)m_VibroDPS.GetValue(knife);
            }

            // do energy
            ModuleEnergy energy = blockPrefab.GetComponent<ModuleEnergy>();
            ModuleEnergyStore energyStore = blockPrefab.GetComponent<ModuleEnergyStore>();
            if (energy != null)
            {
                ModuleItemConsume generator = blockPrefab.GetComponent<ModuleItemConsume>();
                bool GenerateFromChunks = false;
                if (generator != null)
                {
                    ModuleRecipeProvider recipeProvider = blockPrefab.GetComponent<ModuleRecipeProvider>();
                    RecipeManager.RecipeNameWrapper[] recipeListNames = (RecipeManager.RecipeNameWrapper[])m_RecipeListNames.GetValue(recipeProvider);
                    RecipeListWrapper[] recipeListWrappers = Singleton.Manager<RecipeManager>.inst.GetWrappedRecipeLists(recipeListNames);
                    RecipeListWrapper[] moddedRecipeListWrappers = Singleton.Manager<RecipeManager>.inst.GetWrappedRecipeLists(recipeListNames);
                    foreach (RecipeListWrapper recipeListWrapper in recipeListWrappers)
                    {
                        RecipeTable.RecipeList recipeList = recipeListWrapper.target;
                        foreach (RecipeTable.Recipe recipe in recipeList)
                        {
                            if (recipe.m_OutputType == RecipeTable.Recipe.OutputType.Energy)
                            {
                                GenerateFromChunks = true;
                                break;
                            }
                        }
                    }
                    foreach (RecipeListWrapper recipeListWrapper in moddedRecipeListWrappers)
                    {
                        RecipeTable.RecipeList recipeList = recipeListWrapper.target;
                        foreach (RecipeTable.Recipe recipe in recipeList)
                        {
                            if (recipe.m_OutputType == RecipeTable.Recipe.OutputType.Energy)
                            {
                                GenerateFromChunks = true;
                                break;
                            }
                        }
                    }
                }

                if (GenerateFromChunks)
                {
                    this.EnergyGenerationTitle = "Furnace Efficiency";
                    this.EnergyGeneration = (float)m_GeneratorMultiplier.GetValue(generator);
                }
                else
                {
                    this.EnergyGenerationTitle = "Energy/s";
                    this.EnergyGeneration = (float)m_GeneratorOutput.GetValue(energy);
                }
            }
            if (energyStore != null)
            {
                this.EnergyCapacity = energyStore.m_Capacity;
            }
            this.HasEnergy = this.EnergyCapacity > 0 || this.EnergyGeneration > 0;

            // do shield
            ModuleShieldGenerator shield = blockPrefab.GetComponent<ModuleShieldGenerator>();
            if (shield != null)
            {
                this.HasShield = true;
                this.ShieldEnergyPerHealed = shield.m_EnergyConsumedPerPointHealed;
                this.ShieldEnergyPerDamage = shield.m_EnergyConsumedPerDamagePoint;

                this.ShieldPassiveDrain = shield.m_EnergyConsumptionPerSec;
                this.ShieldStartupDrain = shield.m_InitialChargeEnergy;
            }

            // do radar
            ModuleRadar radar = blockPrefab.GetComponent<ModuleRadar>();
            ModuleVision vision = blockPrefab.GetComponent<ModuleVision>();
            if (vision != null || radar != null)
            {
                this.HasRadar = true;
                ModuleRadar.RadarScanType radarScanType = ModuleRadar.RadarScanType.Techs;
                if (radar != null)
                {
                    if ((radar.ScanType & ModuleRadar.RadarScanType.Resources) != (ModuleRadar.RadarScanType)0)
                    {
                        radarScanType = ModuleRadar.RadarScanType.Resources;
                    }
                    else if ((radar.ScanType & ModuleRadar.RadarScanType.Terrain) != (ModuleRadar.RadarScanType)0)
                    {
                        radarScanType = ModuleRadar.RadarScanType.Terrain;
                    }
                    else
                    {
                        // handle theoretical future radars
                        foreach (ModuleRadar.RadarScanType scanType in EnumIterator<ModuleRadar.RadarScanType>.Values())
                        {
                            if (scanType != ModuleRadar.RadarScanType.Techs && (radar.ScanType & scanType) != (ModuleRadar.RadarScanType)0)
                            {
                                radarScanType = scanType;
                                break;
                            }
                        }
                    }
                }
                this.RadarType = radarScanType;

                if (vision != null)
                {
                    this.RadarRange = vision.Range;
                }
                else if (radar != null)
                {
                    this.RadarRange = radar.GetRange(radarScanType);
                }
            }

            // do gun data
            float bulletsPerSecond = 1.0f;
            ModuleWeaponGun gun = blockPrefab.GetComponent<ModuleWeaponGun>();
            float shotCooldown = 0.0f;
            if (gun != null)
            {
                // Console.WriteLine("  Getting Gun stuff");
                CannonBarrel[] cannonBarrels = blockPrefab.gameObject.GetComponentsInChildren<CannonBarrel>(true);
                int numBarrels = cannonBarrels.Length;
                if (numBarrels == 0)
                {
                    Console.WriteLine($"FAILED TO GET BARREL COUNT FOR {blockPrefab.name} - 0 detected");
                    numBarrels = 1;
                }
                if (gun.m_BurstShotCount > 0)
                {
                    this.ShotBurstCount = gun.m_BurstShotCount;
                }
                else if (gun.m_FireControlMode == ModuleWeaponGun.FireControlMode.AllAtOnce)
                {
                    this.ShotBurstCount = numBarrels;
                }
                else
                {
                    this.ShotBurstCount = 1;
                }

                float burstCooldown = Mathf.Max((float)m_BurstCooldown.GetValue(gun), 0.05f);
                // highest fire rate will be once a frame - approx 1/20 s
                shotCooldown = Mathf.Max((float)m_ShotCooldown.GetValue(gun) / numBarrels, 0.05f);

                int cycleShots;
                float cycleTime;

                int salvoCount = (gun.m_FireControlMode == ModuleWeaponGun.FireControlMode.AllAtOnce ? numBarrels : 1);
                if (gun.m_BurstShotCount > 0)
                {
                    int salvosBeforeBurst = Mathf.CeilToInt((float)gun.m_BurstShotCount / (float)salvoCount);
                    cycleShots = salvosBeforeBurst * salvoCount;
                    cycleTime = ((salvosBeforeBurst - 1) * shotCooldown) + burstCooldown;
                }
                else
                {
                    cycleShots = salvoCount;
                    cycleTime = shotCooldown;
                }
                bulletsPerSecond = (float)cycleShots / cycleTime;
                if (bulletsPerSecond == 0.0f)
                {
                    Console.WriteLine($"FAILED TO GET SALVO COUNT FOR {blockPrefab.name} WITH {numBarrels} barrels, {shotCooldown} cooldown. Cycle time {cycleTime}, Cycle shots {cycleShots}");
                }
                // Console.WriteLine("  Got gun stuff");
            }

            // do projectile/beam data
            FireData fireData = blockPrefab.GetComponent<FireData>();
            if (fireData != null)
            {
                // Console.WriteLine("  Getting FireData");
                this.MuzzleVelocity = fireData.m_MuzzleVelocity;
                this.HasRange = true;
                bool isShotgun = false;
                if (fireData is FireDataShotgun shotgunData)
                {
                    // Console.WriteLine("  Is shotgun");
                    this.Range = shotgunData.m_ShotMaxRange;
                    isShotgun = true;
                }

                WeaponRound bullet = fireData.m_BulletPrefab;

                bool isBeam = false;
                {
                    // do beam handling
                    // Console.WriteLine("  Getting BeamWeapon stuff");
                    // is beam?
                    float maxRange = 0.0f;
                    float dps = 0;
                    foreach (CannonBarrel barrel in fireData.GetComponentsInChildren<CannonBarrel>(true))
                    {
                        if (barrel != null && barrel.beamWeapon != null)
                        {
                            isBeam = true;
                            maxRange = Mathf.Max(maxRange, barrel.beamWeapon.Range);
                            int beamDPS = (int)m_BeamDPS.GetValue(barrel.beamWeapon);
                            if (shotCooldown > 0.0f)
                            {
                                if (beamDPS < 0.0f)
                                {
                                    // should only happen with laser mod
                                    dps += -beamDPS * bulletsPerSecond;
                                    break;
                                }
                                else
                                {
                                    // somehow here without laser mod
                                    float fadeOutTime = (float)m_BeamFadeOutTime.GetValue(barrel.beamWeapon);
                                    float percentageActive = Mathf.Clamp01(fadeOutTime / shotCooldown);
                                    dps += beamDPS * percentageActive;
                                }
                            }
                            else {
                                dps += beamDPS;
                            }
                        }
                    }
                    if (isBeam)
                    {
                        this.DPS = dps;
                        this.Range = maxRange;
                    }
                    // Console.WriteLine("  Got BeamWeapon stuff");
                }

                if (!isBeam && bullet != null)
                {
                    this.ProjectileDamage = (int)m_ProjectileDamage.GetValue(bullet);
                    if (bullet is Projectile projectile)
                    {
                        // Console.WriteLine("  Getting Projectile stuff");
                        this.HasProjectile = true;

                        this.SeekingProjectile = projectile.SeekingProjectile != null;
                        this.ProjectileHasGravity = (bool)m_ProjectileGravity.GetValue(projectile);
                        Transform explosion = (Transform)m_Explosion.GetValue(projectile);
                        if (explosion != null)
                        {
                            Explosion actualExplosion = explosion.GetComponent<Explosion>();
                            if (actualExplosion != null)
                            {
                                // Console.WriteLine("  Getting Explosion stuff");
                                this.HasExplosion = true;
                                this.ExplosionDamage = actualExplosion.m_MaxDamageStrength;
                                this.ExplosionRadius = actualExplosion.m_EffectRadius;
                                this.ExplosionMaxEffectRadius = actualExplosion.m_EffectRadiusMaxStrength;
                                this.ExplosionDamageType = (ManDamage.DamageType)m_ExplosionDamageType.GetValue(actualExplosion);
                                // Console.WriteLine("  Got Explosion stuff");
                            }
                        }
                        this.DPS = bulletsPerSecond * (this.ProjectileDamage + this.ExplosionDamage);
                        if (this.DPS == 0.0f)
                        {
                            Console.WriteLine($"DPS OF 0 FOR {blockPrefab.name} - bps {bulletsPerSecond}, proj damage {this.ProjectileDamage}, explosion damage {this.ExplosionDamage}");
                        }

                        // calculate range
                        this.Range = Mathf.Infinity;
                        float lifetime = (float)m_ProjectileLifetime.GetValue(projectile);
                        if (lifetime > 0.0f)
                        {
                            // has lifetime
                            this.Range = Mathf.Min(this.Range, this.MuzzleVelocity * lifetime);
                        }
                        if (this.ProjectileHasGravity)
                        {
                            this.Range = Mathf.Min(this.Range, this.MuzzleVelocity * this.MuzzleVelocity / Physics.gravity.magnitude);
                        }
                    }
                    else if (isShotgun)
                    {
                        this.DPS = bulletsPerSecond * this.ProjectileDamage;
                    }
                    else
                    {
                        Console.WriteLine($"????? NON-SHOTGUN, NON-PROJECTILE WEAPONROUND {blockPrefab.name}");
                    }
                }
            }
            // Console.WriteLine("DONE!");
        }
    }
}
