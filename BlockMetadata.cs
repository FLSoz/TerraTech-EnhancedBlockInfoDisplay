﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FunctionTree;

namespace EnhancedBlockInfoDisplay
{
    public class BlockMetadata
    {
        public int MaxHP = 0;
        public float DPS = 0.0f;

        public int CellCount = 0;

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

        public float Mass = 0.0f;

        public bool HasFan = false;
        public bool MultiAxisFan = false;
        public float FanForce = 0.0f;
        public Vector2 MaxFan = Vector2.zero;
        public Vector2 FanX = Vector2.zero;
        public Vector2 FanY = Vector2.zero;
        public Vector2 FanZ = Vector2.zero;
        public float FanEfficiency = 0.0f;

        public bool HasBooster = false;
        public bool BoosterBurnsFuel = false;
        public float BoosterFuelBurn = 0.0f;
        public float BoosterForce = 0.0f;
        public bool MultiAxisBooster = false;
        public Vector2 MaxBooster = Vector2.zero;
        public Vector2 BoosterX = Vector2.zero;
        public Vector2 BoosterY = Vector2.zero;
        public Vector2 BoosterZ = Vector2.zero;

        public BlockTypes BlockID;

        public BlockMetadata(TankBlock blockPrefab, BlockTypes blockID)
        {
            this.Mass = blockPrefab.m_DefaultMass;

            this.FetchHPData(blockPrefab);

            this.FetchAPData(blockPrefab);

            this.FetchModuleBoosterData(blockPrefab);

            this.FetchNonGunWeaponData(blockPrefab);

            this.FetchEnergyData(blockPrefab);

            this.FetchShieldData(blockPrefab);

            this.FetchRadarData(blockPrefab);

            this.FetchGunData(blockPrefab);

            this.BlockID = blockID;
        }

        #region Reflection
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

        private static readonly FieldInfo m_BoosterForce = AccessTools.Field(typeof(Thruster), "m_Force");
        private static readonly FieldInfo m_BoosterActivationDelay = AccessTools.Field(typeof(MissileProjectile), "m_BoosterActivationDelay");
        private static readonly FieldInfo m_MaxBoosterLifetime = AccessTools.Field(typeof(MissileProjectile), "m_MaxBoosterLifetime");

        private static readonly FieldInfo m_FanForce = AccessTools.Field(typeof(Thruster), "m_Force");
        private static readonly FieldInfo m_FanBackForce = AccessTools.Field(typeof(FanJet), "backForce");

        private static readonly FieldInfo m_BoosterEffector = AccessTools.Field(typeof(BoosterJet), "m_Effector");
        private static readonly FieldInfo m_FanEffector = AccessTools.Field(typeof(FanJet), "m_Effector");

        private static readonly FieldInfo m_fans = AccessTools.Field(typeof(ModuleBooster), "fans");
        private static readonly FieldInfo m_jets = AccessTools.Field(typeof(ModuleBooster), "jets");
        #endregion Reflection

        // return true if we have multiaxis
        private bool GetForceByAxis<T>(List<T> jets, Func<T, Vector3> getDirection, Func<T, float> getForce, ref Vector2 xForce, ref Vector2 yForce, ref Vector2 zForce)
        {
            int seenX = 0;
            int seenY = 0;
            int seenZ = 0;

            foreach (T jet in jets)
            {
                Vector3 direction = getDirection(jet);
                float force = getForce(jet);
                float forwardThrottle = Mathf.Clamp(Vector3.Dot(Vector3.forward, direction), -1f, 1f);
                if (forwardThrottle.Approximately(0, 0.01f))
                {
                    // do not show approximation error
                }
                else if (forwardThrottle > 0f)
                {
                    seenZ = 1;
                    zForce.x += force * forwardThrottle;
                }
                else if (forwardThrottle < 0f)
                {
                    seenZ = 1;
                    zForce.y += force * forwardThrottle;
                }
                float upThrottle = Mathf.Clamp(Vector3.Dot(Vector3.up, direction), -1f, 1f);
                if (upThrottle.Approximately(0, 0.01f))
                {
                    // do not show approximation error
                }
                else if (upThrottle > 0f)
                {
                    seenY = 1;
                    yForce.x += force * upThrottle;
                }
                else if (upThrottle < 0f)
                {
                    seenY = 1;
                    yForce.y += force * upThrottle;
                }
                float rightThrottle = Mathf.Clamp(Vector3.Dot(Vector3.right, direction), -1f, 1f);
                if (rightThrottle.Approximately(0, 0.01f))
                {
                    // do not show approximation error
                }
                else if (rightThrottle > 0f)
                {
                    seenX = 1;
                    xForce.x += force * rightThrottle;
                }
                else if (rightThrottle < 0f)
                {
                    seenX = 1;
                    xForce.y += force * rightThrottle;
                }
            }

            return seenX + seenY + seenZ > 1;
        }

        private static Vector2 GetMaxForce(Vector2 rightForce, Vector2 upForce, Vector2 forwardForce)
        {
            float maxMagnitude = rightForce.sqrMagnitude;
            Vector2 maxForce = rightForce;
            if (upForce.sqrMagnitude > maxMagnitude)
            {
                maxForce = upForce;
                maxMagnitude = upForce.sqrMagnitude;
            }
            if (forwardForce.sqrMagnitude > maxMagnitude)
            {
                maxForce = forwardForce;
            }
            return maxForce;
        }

        private static Vector3 GetLocalDirection(TankBlock blockPrefab, Transform effector)
        {
            return blockPrefab.transform.InverseTransformDirection(effector.forward);
        }

        private bool FetchFanProps(TankBlock blockPrefab, List<FanJet> fans, out Vector2 rightForce, out Vector2 upForce, out Vector2 forwardForce)
        {
            rightForce = Vector2.zero;
            upForce = Vector2.zero;
            forwardForce = Vector2.zero;
            bool forwardMultiAxis = GetForceByAxis(
                fans,
                (FanJet fan) => { return GetLocalDirection(blockPrefab, (Transform)m_FanEffector.GetValue(fan)); },
                (FanJet fan) => { return (float) m_FanForce.GetValue(fan); },
                ref rightForce,
                ref upForce,
                ref forwardForce
            );
            bool backwardMultiAxis = GetForceByAxis(
                fans,
                (FanJet fan) => { return -GetLocalDirection(blockPrefab, (Transform)m_FanEffector.GetValue(fan)); },
                (FanJet fan) => { return (float)m_FanBackForce.GetValue(fan); },
                ref rightForce,
                ref upForce,
                ref forwardForce
            );
            return forwardMultiAxis || backwardMultiAxis;
        }

        private bool FetchBoosterProps(TankBlock blockPrefab, List<BoosterJet> boosters, out Vector2 rightForce, out Vector2 upForce, out Vector2 forwardForce)
        {
            rightForce = Vector2.zero;
            upForce = Vector2.zero;
            forwardForce = Vector2.zero;
            return GetForceByAxis(
                boosters,
                (BoosterJet booster) => { return GetLocalDirection(blockPrefab, (Transform)m_BoosterEffector.GetValue(booster)); },
                (BoosterJet booster) => { return (float)m_BoosterForce.GetValue(booster); },
                ref rightForce,
                ref upForce,
                ref forwardForce
            );
        }

        private void FetchModuleBoosterData(TankBlock blockPrefab)
        {
            // boosters and fans
            ModuleBooster moduleBooster = blockPrefab.GetComponent<ModuleBooster>();
            if (moduleBooster != null)
            {
                List<FanJet>  fans = new List<FanJet>(blockPrefab.GetComponentsInChildren<FanJet>(true));
                if (fans.Count > 0)
                {
                    this.HasFan = true;
                    this.MultiAxisFan = this.FetchFanProps(blockPrefab, fans, out Vector2 rightForce, out Vector2 upForce, out Vector2 forwardForce);
                    this.FanX = rightForce;
                    this.FanY = upForce;
                    this.FanZ = forwardForce;
                    this.MaxFan = GetMaxForce(rightForce, upForce, forwardForce);
                    if (!this.MultiAxisFan)
                    {
                        if (this.MaxFan.x.Approximately(-this.MaxFan.y, 1))
                        {
                            this.FanForce = this.MaxFan.x;
                        }
                        this.FanEfficiency = (Mathf.Abs(this.MaxFan.y) - (this.Mass * Physics.gravity.magnitude)) / this.CellCount;
                    }
                }

                this.BoosterFuelBurn = moduleBooster.FuelBurnPerSecond();
                List<BoosterJet> boosters = new List<BoosterJet>(blockPrefab.GetComponentsInChildren<BoosterJet>(true));
                if (boosters.Count > 0)
                {
                    this.HasBooster = true;
                    foreach (BoosterJet booster in boosters)
                    {
                        if (booster.ConsumesFuel)
                        {
                            this.BoosterBurnsFuel = true;
                            break;
                        }
                    }
                    this.MultiAxisBooster = this.FetchBoosterProps(blockPrefab, boosters, out Vector2 rightForce, out Vector2 upForce, out Vector2 forwardForce);
                    this.BoosterX = rightForce;
                    this.BoosterY = upForce;
                    this.BoosterZ = forwardForce;
                    this.MaxBooster = GetMaxForce(rightForce, upForce, forwardForce);
                    if (!this.MultiAxisBooster && this.MaxBooster.x.Approximately(-this.MaxBooster.y, 1))
                    {
                        this.BoosterForce = this.MaxBooster.x;
                    }

                    // handle cosmetic boosters
                    if (this.BoosterForce == 0.0f && !this.BoosterBurnsFuel)
                    {
                        this.HasBooster = false;
                    }
                }
            }
        }

        private void FetchGunData(TankBlock blockPrefab)
        {
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
                            else
                            {
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

                        // handle missiles
                        bool firesForward = true;
                        // ^ no good way to detect firing direction as of now
                        float boosterForce = 0.0f;
                        float boosterDuration = 0.0f;
                        float boosterDelay = 0.0f;
                        if (projectile is MissileProjectile missile)
                        {
                            BoosterJet[] boosters = missile.GetComponentsInChildren<BoosterJet>();
                            foreach (BoosterJet booster in boosters)
                            {
                                boosterForce += (float)m_BoosterForce.GetValue(booster);
                            }
                            boosterDuration = (float)m_MaxBoosterLifetime.GetValue(missile);
                            boosterDelay = (float)m_BoosterActivationDelay.GetValue(missile);
                        }

                        this.ProjectileHasGravity = (bool)m_ProjectileGravity.GetValue(projectile);
                        Transform explosion = (Transform)m_Explosion.GetValue(projectile);
                        if (explosion != null)
                        {
                            Explosion actualExplosion = explosion.GetComponent<Explosion>();
                            if (actualExplosion != null)
                            {
                                this.HasExplosion = true;
                                this.ExplosionDamage = actualExplosion.m_MaxDamageStrength;
                                this.ExplosionRadius = actualExplosion.m_EffectRadius;
                                this.ExplosionMaxEffectRadius = actualExplosion.m_EffectRadiusMaxStrength;
                                this.ExplosionDamageType = (ManDamage.DamageType)m_ExplosionDamageType.GetValue(actualExplosion);
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
                            float lifetimeBounded = this.MuzzleVelocity * lifetime;
                            if (boosterForce > 0.0f)
                            {
                                // if has gravity, assume is vertical launch, not impact range
                                float startVelocity = !firesForward ? 0.0f : this.MuzzleVelocity;
                                float startDistance = startVelocity * boosterDelay;
                                // this is hard set by Projectile OnPool
                                float mass = 0.00123f;
                                float acceleration = boosterForce / mass;
                                float boostingTime = Mathf.Max(Mathf.Min(boosterDuration, lifetime - boosterDelay), 0.0f);
                                float finalSpeed = acceleration * boostingTime + startVelocity;
                                float acceleratedDistance = acceleration * boostingTime * boostingTime / 2 + startVelocity * boostingTime;
                                float finalDistance = finalSpeed * Mathf.Max(0.0f, lifetime - (boosterDelay + boosterDuration));
                                /* Console.WriteLine($"Getting missile range for {blockPrefab.name}");
                                Console.WriteLine($"  booster delay {boosterDelay}");
                                Console.WriteLine($"  booster force {boosterForce}");
                                Console.WriteLine($"  missile mass {mass}");
                                Console.WriteLine($"  acceleration {acceleration}");
                                Console.WriteLine($"  boosting time {boostingTime}");
                                Console.WriteLine($"  start distance {startDistance}");
                                Console.WriteLine($"  accelerated distance {acceleratedDistance}");
                                Console.WriteLine($"  final distance {finalDistance}");
                                Console.WriteLine($"  lifetime {lifetime}"); */
                                lifetimeBounded = startDistance + acceleratedDistance + finalDistance;
                            }
                            this.Range = Mathf.Min(this.Range, lifetimeBounded);
                        }
                        // if we are missile, then don't consider classical projectile kinematics
                        if (this.ProjectileHasGravity && boosterForce <= 0.0f)
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

        private void FetchNonGunWeaponData(TankBlock blockPrefab)
        {
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
        }

        private void FetchEnergyData(TankBlock blockPrefab)
        {
            // do energy
            ModuleEnergy energy = blockPrefab.GetComponent<ModuleEnergy>();
            ModuleEnergyStore energyStore = blockPrefab.GetComponent<ModuleEnergyStore>();
            if (energy != null)
            {
                ModuleItemConsume generator = blockPrefab.GetComponent<ModuleItemConsume>();
                bool GenerateFromChunks = false;
                float olasticEnergy = 0.0f;
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
                                if (recipe.m_InputItems.Length == 1)
                                {
                                    RecipeTable.Recipe.ItemSpec inputSpec = recipe.m_InputItems[0];
                                    ItemTypeInfo input = inputSpec.m_Item;
                                    if (input.ObjectType == ObjectTypes.Chunk && input.ItemType == (int)ChunkTypes.OlasticBrick)
                                    {
                                        // we found olastic brick burning
                                        olasticEnergy = recipe.m_EnergyOutput / inputSpec.m_Quantity / (recipe.m_BuildTimeSeconds * generator.GetDurationMultiplier(Singleton.Manager<ManSpawn>.inst.GetCorporation(this.BlockID)));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if (GenerateFromChunks)
                {
                    this.EnergyGenerationTitle = "Olastic Energy/s";
                    this.EnergyGeneration = olasticEnergy * (float)m_GeneratorMultiplier.GetValue(generator);
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
        }

        private void FetchShieldData(TankBlock blockPrefab)
        {
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
        }

        private void FetchRadarData(TankBlock blockPrefab)
        {
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
        }

        private void FetchHPData(TankBlock blockPrefab)
        {
            ModuleDamage moduleDamage = blockPrefab.GetComponent<ModuleDamage>();
            if (moduleDamage != null)
            {
                this.MaxHP = moduleDamage.maxHealth;
            }
            else
            {
                this.MaxHP = -1;
            }
        }

        private void FetchAPData(TankBlock blockPrefab)
        {
            this.CellCount = blockPrefab.filledCells.Count();
        }
    }
}
