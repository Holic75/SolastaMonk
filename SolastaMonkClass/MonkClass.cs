using SolastaModApi;
using SolastaModApi.Extensions;
using SolastaModHelpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using static FeatureDefinitionSavingThrowAffinity;

using Helpers = SolastaModHelpers.Helpers;
using NewFeatureDefinitions = SolastaModHelpers.NewFeatureDefinitions;
using ExtendedEnums = SolastaModHelpers.ExtendedEnums;

namespace SolastaMonkClass
{
    internal class MonkClassBuilder : CharacterClassDefinitionBuilder
    {
        const string MonkClassName = "MonkClass";
        const string MonkClassNameGuid = "016ebfea-7e5e-46e9-87d4-dbc6c6088d20";
        const string MonkClassSubclassesGuid = "b15810d7-739c-4bc0-95c0-5c274c232d3e";

        static public List<string> monk_weapons = new List<string>
        {
            Helpers.WeaponProficiencies.ShortSword,
            Helpers.WeaponProficiencies.Club,
            Helpers.WeaponProficiencies.Dagger,
            Helpers.WeaponProficiencies.Handaxe,
            Helpers.WeaponProficiencies.Javelin,
            Helpers.WeaponProficiencies.Mace,
            Helpers.WeaponProficiencies.QuarterStaff,
            Helpers.WeaponProficiencies.Spear,
            Helpers.WeaponProficiencies.Unarmed
        };
        static public NewFeatureDefinitions.ArmorClassStatBonus unarmored_defense;
        static public FeatureDefinitionFeatureSet martial_arts;
        static public NewFeatureDefinitions.MovementBonusBasedOnEquipment unarmored_movement;
        static public Dictionary<int, NewFeatureDefinitions.MovementBonusBasedOnEquipment> unarmored_movement_improvements = new Dictionary<int, NewFeatureDefinitions.MovementBonusBasedOnEquipment>();
        static List<int> unarmored_movement_improvement_levels = new List<int> { 6, 10, 14, 18 };
        static public NewFeatureDefinitions.IncreaseNumberOfPowerUsesPerClassLevel ki;
        static public NewFeatureDefinitions.PowerWithRestrictions flurry_of_blows;
        static public NewFeatureDefinitions.PowerWithRestrictions patient_defense;
        static public NewFeatureDefinitions.PowerWithRestrictions step_of_the_wind;

        static Dictionary<int, int> rage_bonus_damage_level_map = new Dictionary<int, int> { { 2, 1 }, { 3, 9 }, { 4, 16 } };
        static List<int> rage_uses_increase_levels = new List<int> { 3, 6, 12, 17 };
        
        static public NewFeatureDefinitions.SpellcastingForbidden rage_spellcasting_forbiden;
        static public Dictionary<int, NewFeatureDefinitions.PowerWithRestrictions> rage_powers = new Dictionary<int, NewFeatureDefinitions.PowerWithRestrictions>();
        static public Dictionary<int, NewFeatureDefinitions.IncreaseNumberOfPowerUses> rage_power_extra_use = new Dictionary<int, NewFeatureDefinitions.IncreaseNumberOfPowerUses>();
        static public FeatureDefinitionAttributeModifier extra_attack;

        //Frozen Fury
        static NewFeatureDefinitions.ApplyPowerOnTurnEndBasedOnClassLevel frozen_fury_rage_feature;
        static public FeatureDefinition frozen_fury;
        static public NewFeatureDefinitions.ArmorBonusAgainstAttackType frigid_body;
        static public FeatureDefinitionFeatureSet numb;
        //War Shaman
        static public SpellListDefinition war_shaman_spelllist;
        static public FeatureDefinitionCastSpell war_shaman_spellcasting;
        static public Dictionary<int, NewFeatureDefinitions.PowerWithRestrictions> share_rage_powers = new Dictionary<int, NewFeatureDefinitions.PowerWithRestrictions>();
        static public FeatureDefinition ragecaster;


        //Berserker
        //frenzy
        static public NewFeatureDefinitions.PowerWithRestrictions frenzy;
        static public ConditionDefinition exhausted_after_frenzy_condition;
        static public FeatureDefinitionFeatureSet mindless_rage;
        static public FeatureDefinitionPower intimidating_presence;
        static public FeatureDefinitionFeatureSet intimidating_presence_feature;

        static public CharacterClassDefinition monk_class;


        protected MonkClassBuilder(string name, string guid) : base(name, guid)
        {
            var monk_class_sprite = SolastaModHelpers.CustomIcons.Tools.storeCustomIcon("MonkClassSprite",
                                                                                                $@"{UnityModManagerNet.UnityModManager.modsPath}/SolastaMonkClass/Sprites/MonkClass.png",
                                                                                                1024, 576);

            var fighter = DatabaseHelper.CharacterClassDefinitions.Fighter;
            monk_class = Definition;
            Definition.GuiPresentation.Title = "Class/&MonkClassTitle";
            Definition.GuiPresentation.Description = "Class/&MonkClassDescription";
            Definition.GuiPresentation.SetSpriteReference(monk_class_sprite);

            Definition.SetClassAnimationId(AnimationDefinitions.ClassAnimationId.Fighter);
            Definition.SetClassPictogramReference(fighter.ClassPictogramReference);
            Definition.SetDefaultBattleDecisions(fighter.DefaultBattleDecisions);
            Definition.SetHitDice(RuleDefinitions.DieType.D8);
            Definition.SetIngredientGatheringOdds(fighter.IngredientGatheringOdds);
            Definition.SetRequiresDeity(false);

            Definition.AbilityScoresPriority.Clear();
            Definition.AbilityScoresPriority.AddRange(new List<string> {Helpers.Stats.Dexterity,
                                                                        Helpers.Stats.Wisdom,
                                                                        Helpers.Stats.Constitution,
                                                                        Helpers.Stats.Strength,
                                                                        Helpers.Stats.Charisma,
                                                                        Helpers.Stats.Intelligence});

            Definition.FeatAutolearnPreference.AddRange(fighter.FeatAutolearnPreference);
            Definition.PersonalityFlagOccurences.AddRange(fighter.PersonalityFlagOccurences);

            Definition.SkillAutolearnPreference.Clear();
            Definition.SkillAutolearnPreference.AddRange(new List<string> { Helpers.Skills.Acrobatics,
                                                                            Helpers.Skills.Athletics,
                                                                            Helpers.Skills.History,
                                                                            Helpers.Skills.Insight,
                                                                            Helpers.Skills.Religion,
                                                                            Helpers.Skills.Stealth,
                                                                            Helpers.Skills.Perception,
                                                                            Helpers.Skills.Survival });

            Definition.ToolAutolearnPreference.Clear();
            Definition.ToolAutolearnPreference.AddRange(new List<string> { Helpers.Tools.SmithTool, Helpers.Tools.HerbalismKit });


            Definition.EquipmentRows.AddRange(fighter.EquipmentRows);
            Definition.EquipmentRows.Clear();

            this.AddEquipmentRow(new List<CharacterClassDefinition.HeroEquipmentOption>
                                    {
                                        EquipmentOptionsBuilder.Option(DatabaseHelper.ItemDefinitions.Shortsword, EquipmentDefinitions.OptionWeapon, 1),
                                    },
                                new List<CharacterClassDefinition.HeroEquipmentOption>
                                    {
                                        EquipmentOptionsBuilder.Option(DatabaseHelper.ItemDefinitions.Shortsword, EquipmentDefinitions.OptionWeaponSimpleChoice, 1),
                                    }
            );

            this.AddEquipmentRow(new List<CharacterClassDefinition.HeroEquipmentOption>
                                    {
                                        EquipmentOptionsBuilder.Option(DatabaseHelper.ItemDefinitions.DungeoneerPack, EquipmentDefinitions.OptionStarterPack, 1),
                                    },
                                    new List<CharacterClassDefinition.HeroEquipmentOption>
                                    {
                                        EquipmentOptionsBuilder.Option(DatabaseHelper.ItemDefinitions.ExplorerPack, EquipmentDefinitions.OptionStarterPack, 1),
                                    }
            );

            this.AddEquipmentRow(new List<CharacterClassDefinition.HeroEquipmentOption>
            {
                EquipmentOptionsBuilder.Option(DatabaseHelper.ItemDefinitions.Dart, EquipmentDefinitions.OptionWeapon, 10),
            });

            var saving_throws = Helpers.ProficiencyBuilder.CreateSavingthrowProficiency("MonkSavingthrowProficiency",
                                                                                        "",
                                                                                        Helpers.Stats.Strength, Helpers.Stats.Dexterity);


            var weapon_proficiency = Helpers.ProficiencyBuilder.CreateWeaponProficiency("MonkWeaponProficiency",
                                                                          "",
                                                                          "Feature/&MonkWeaponProficiencyTitle",
                                                                          "",
                                                                          Helpers.WeaponProficiencies.Simple,
                                                                          Helpers.WeaponProficiencies.ShortSword
                                                                          );

            var skills = Helpers.PoolBuilder.createSkillProficiency("MonkSkillProficiency",
                                                                    "",
                                                                    "Feature/&MonkClassSkillPointPoolTitle",
                                                                    "Feature/&SkillGainChoicesPluralDescription",
                                                                    2,
                                                                    Helpers.Skills.Acrobatics,
                                                                    Helpers.Skills.Athletics,
                                                                    Helpers.Skills.History,
                                                                    Helpers.Skills.Insight,
                                                                    Helpers.Skills.Religion,
                                                                    Helpers.Skills.Stealth
                                                                    );
            createUnarmoredDefense();
            createMartialArts();
            createUnarmoredMovement();
            createKi();
            createRage();
            createExtraAttack();
            Definition.FeatureUnlocks.Clear();
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(saving_throws, 1));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(weapon_proficiency, 1));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(skills, 1));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(unarmored_defense, 1));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(martial_arts, 1));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(unarmored_movement, 2));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(ki, 2));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(flurry_of_blows, 2));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(patient_defense, 2));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(step_of_the_wind, 2));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice, 4));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(extra_attack, 5));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(unarmored_movement_improvements[6], 6));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice, 8));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(unarmored_movement_improvements[10], 10));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice, 12));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(unarmored_movement_improvements[14], 14));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice, 16));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(unarmored_movement_improvements[18], 18));
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetAbilityScoreChoice, 19));

            var subclassChoicesGuiPresentation = new GuiPresentation();
            subclassChoicesGuiPresentation.Title = "Subclass/&MonkSubclassPrimalPathTitle";
            subclassChoicesGuiPresentation.Description = "Subclass/&MonkSubclassPrimalPathDescription";
            MonkFeatureDefinitionSubclassChoice = this.BuildSubclassChoice(3, "PrimalPath", false, "SubclassChoiceMonkSpecialistArchetypes", subclassChoicesGuiPresentation, MonkClassSubclassesGuid);
        }


        static void createKi()
        {
            string ki_title_string = "Feature/&MonkClassKiTitle";
            string ki_description_string = "Feature/&MonkClassKiDescription";

            createFlurryOfBlows();
            createPatientDefense();
            createStepOfTheWind();

            ki = Helpers.FeatureBuilder<NewFeatureDefinitions.IncreaseNumberOfPowerUsesPerClassLevel>.createFeature("MonkClassKi",
                                                                                                                    "",
                                                                                                                    ki_title_string,
                                                                                                                    ki_description_string,
                                                                                                                    Common.common_no_icon,
                                                                                                                    a =>
                                                                                                                    {
                                                                                                                        a.powers = new List<FeatureDefinitionPower> { flurry_of_blows, patient_defense, step_of_the_wind };
                                                                                                                        a.characterClass = monk_class;
                                                                                                                        a.levelIncreaseList = new List<(int, int)>();
                                                                                                                        for (int i = 3; i <= 20; i++)
                                                                                                                        {
                                                                                                                            a.levelIncreaseList.Add((i, 1));
                                                                                                                        }
                                                                                                                    }
                                                                                                                   );
        }


        static void createStepOfTheWind()
        {
            string step_of_the_wind_title_string = "Feature/&MonkClassStepOfTheWindPowerTitle";
            string step_of_the_wind_description_string = "Feature/&MonkClassStepOfTheWindPowerDescription";

            var step_of_the_wind_feature = Helpers.CopyFeatureBuilder<FeatureDefinitionAdditionalAction>.createFeatureCopy("MonkClassStepOfTheWindFeature",
                                                                                                                           "",
                                                                                                                           Common.common_no_title,
                                                                                                                           Common.common_no_title,
                                                                                                                           null,
                                                                                                                           DatabaseHelper.FeatureDefinitionAdditionalActions.AdditionalActionHasted,
                                                                                                                           a =>
                                                                                                                           {
                                                                                                                               a.restrictedActions = new List<ActionDefinitions.Id>
                                                                                                                               {
                                                                                                                                   ActionDefinitions.Id.DisengageMain,
                                                                                                                                   ActionDefinitions.Id.DashMain
                                                                                                                               };
                                                                                                                           }
                                                                                                                           );

            var step_of_the_wind_condition = Helpers.ConditionBuilder.createCondition("MonkClassStepOfTheWindCondition",
                                                                                     "",
                                                                                     step_of_the_wind_title_string,
                                                                                     step_of_the_wind_description_string,
                                                                                     null,
                                                                                     DatabaseHelper.ConditionDefinitions.ConditionHasted,
                                                                                     step_of_the_wind_feature,
                                                                                     DatabaseHelper.FeatureDefinitionMovementAffinitys.MovementAffinityJump
                                                                                     );
            step_of_the_wind_condition.SetSubsequentOnRemoval(null);
            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.SpellDefinitions.Haste.EffectDescription);
            effect.SetRangeType(RuleDefinitions.RangeType.Self);
            effect.SetRangeParameter(1);
            effect.DurationParameter = 1;
            effect.DurationType = RuleDefinitions.DurationType.Round;
            effect.EffectForms.Clear();
            effect.SetTargetType(RuleDefinitions.TargetType.Self);

            var effect_form = new EffectForm();
            effect_form.ConditionForm = new ConditionForm();
            effect_form.FormType = EffectForm.EffectFormType.Condition;
            effect_form.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
            effect_form.ConditionForm.ConditionDefinition = step_of_the_wind_condition;
            effect.EffectForms.Add(effect_form);

            step_of_the_wind = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                        .createPower("MonkClassStepOfTheWindPower",
                                                                     "",
                                                                     step_of_the_wind_title_string,
                                                                     step_of_the_wind_description_string,
                                                                     DatabaseHelper.FeatureDefinitionPowers.PowerOathOfDevotionAuraDevotion.GuiPresentation.SpriteReference,
                                                                     effect,
                                                                     RuleDefinitions.ActivationTime.BonusAction,
                                                                     2,
                                                                     RuleDefinitions.UsesDetermination.Fixed,
                                                                     RuleDefinitions.RechargeRate.ShortRest
                                                                     );
            step_of_the_wind.restrictions = new List<NewFeatureDefinitions.IRestriction>()
                                            {
                                                new NewFeatureDefinitions.NoArmorRestriction(),
                                                new NewFeatureDefinitions.NoShieldRestriction()
                                            };
            step_of_the_wind.linkedPower = flurry_of_blows;
        }


        static void createPatientDefense()
        {
            string patient_defense_title_string = "Feature/&MonkClassPatientDefensePowerTitle";
            string patient_defense_description_string = "Feature/&MonkClassPatientDefensePowerDescription";

            var patient_defense_feature = Helpers.CopyFeatureBuilder<FeatureDefinitionAdditionalAction>.createFeatureCopy("MonkClassPatientDefenseFeature",
                                                                                                                           "",
                                                                                                                           Common.common_no_title,
                                                                                                                           Common.common_no_title,
                                                                                                                           null,
                                                                                                                           DatabaseHelper.FeatureDefinitionAdditionalActions.AdditionalActionHasted,
                                                                                                                           a =>
                                                                                                                           {
                                                                                                                               a.restrictedActions = new List<ActionDefinitions.Id>
                                                                                                                               {
                                                                                                                                   ActionDefinitions.Id.Dodge
                                                                                                                               };
                                                                                                                           }
                                                                                                                           );

            var patient_defense_condition = Helpers.ConditionBuilder.createCondition("MonkClassPatientDefenseCondition",
                                                                                     "",
                                                                                     patient_defense_title_string,
                                                                                     patient_defense_description_string,
                                                                                     null,
                                                                                     DatabaseHelper.ConditionDefinitions.ConditionHasted,
                                                                                     patient_defense_feature
                                                                                     );
            patient_defense_condition.SetSubsequentOnRemoval(null);
            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.SpellDefinitions.Haste.EffectDescription);
            effect.SetRangeType(RuleDefinitions.RangeType.Self);
            effect.SetRangeParameter(1);
            effect.DurationParameter = 1;
            effect.DurationType = RuleDefinitions.DurationType.Round;
            effect.EffectForms.Clear();
            effect.SetTargetType(RuleDefinitions.TargetType.Self);

            var effect_form = new EffectForm();
            effect_form.ConditionForm = new ConditionForm();
            effect_form.FormType = EffectForm.EffectFormType.Condition;
            effect_form.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
            effect_form.ConditionForm.ConditionDefinition = patient_defense_condition;
            effect.EffectForms.Add(effect_form);

            patient_defense = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                        .createPower("MonkClassPatientDefensePower",
                                                                     "",
                                                                     patient_defense_title_string,
                                                                     patient_defense_description_string,
                                                                     DatabaseHelper.FeatureDefinitionPowers.PowerShadowcasterShadowDodge.GuiPresentation.SpriteReference,
                                                                     effect,
                                                                     RuleDefinitions.ActivationTime.BonusAction,
                                                                     2,
                                                                     RuleDefinitions.UsesDetermination.Fixed,
                                                                     RuleDefinitions.RechargeRate.ShortRest
                                                                     );
            patient_defense.restrictions = new List<NewFeatureDefinitions.IRestriction>()
                                            {
                                                new NewFeatureDefinitions.NoArmorRestriction(),
                                                new NewFeatureDefinitions.NoShieldRestriction()
                                            };
            patient_defense.linkedPower = flurry_of_blows;
        }


        static void createFlurryOfBlows()
        {
            string flurry_of_blows_title_string = "Feature/&MonkClassFlurryOfBlowsPowerTitle";
            string flurry_of_blows_description_string = "Feature/&MonkClassFlurryOfBlowsPowerDescription";

            var flurry_of_blows_feature_extra_attack = Helpers.CopyFeatureBuilder<FeatureDefinitionAdditionalAction>.createFeatureCopy("MonkClassFlurryOfBlowsFeature",
                                                                                                                           "",
                                                                                                                           Common.common_no_title,
                                                                                                                           Common.common_no_title,
                                                                                                                           null,
                                                                                                                           DatabaseHelper.FeatureDefinitionAdditionalActions.AdditionalActionHasted,
                                                                                                                           a =>
                                                                                                                           {
                                                                                                                               a.restrictedActions = new List<ActionDefinitions.Id>
                                                                                                                               {
                                                                                                                                   ActionDefinitions.Id.AttackMain
                                                                                                                               };
                                                                                                                               a.SetMaxAttacksNumber(2);
                                                                                                                               //a.actionType = ActionDefinitions.ActionType.Bonus;
                                                                                                                           }
                                                                                                                           );

            var unarmed_attack = Helpers.FeatureBuilder<NewFeatureDefinitions.ExtraUnarmedAttack>.createFeature("MonkClassFlurryOfBlowsUnarmedAttack",
                                                                                                                    "",
                                                                                                                    Common.common_no_title,
                                                                                                                    Common.common_no_title,
                                                                                                                    null,
                                                                                                                    a =>
                                                                                                                    {
                                                                                                                        a.allowedWeaponTypes = monk_weapons;
                                                                                                                        a.allowArmor = false;
                                                                                                                        a.allowShield = false;
                                                                                                                        a.clearAllAttacks = true;
                                                                                                                        a.actionType = ActionDefinitions.ActionType.Main;
                                                                                                                    }
                                                                                                                    );

            var flurry_of_blows_condition = Helpers.ConditionBuilder.createConditionWithInterruptions("MonkClassFlurryOfBlowsCondition",
                                                                                     "",
                                                                                     flurry_of_blows_title_string,
                                                                                     flurry_of_blows_description_string,
                                                                                     null,
                                                                                     DatabaseHelper.ConditionDefinitions.ConditionHasted,
                                                                                     new RuleDefinitions.ConditionInterruption[] {RuleDefinitions.ConditionInterruption.AnyBattleTurnEnd },
                                                                                     unarmed_attack,
                                                                                     DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierFighterExtraAttack,
                                                                                     flurry_of_blows_feature_extra_attack
                                                                                     );
            flurry_of_blows_condition.SetSubsequentOnRemoval(null);
            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.SpellDefinitions.Haste.EffectDescription);
            effect.SetRangeType(RuleDefinitions.RangeType.Self);
            effect.SetRangeParameter(1);
            effect.DurationParameter = 1;
            effect.DurationType = RuleDefinitions.DurationType.Round;
            effect.EffectForms.Clear();
            effect.SetTargetType(RuleDefinitions.TargetType.Self);

            var effect_form = new EffectForm();
            effect_form.ConditionForm = new ConditionForm();
            effect_form.FormType = EffectForm.EffectFormType.Condition;
            effect_form.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
            effect_form.ConditionForm.ConditionDefinition = flurry_of_blows_condition;
            effect.EffectForms.Add(effect_form);

            flurry_of_blows = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                        .createPower("MonkClassFlurryOfBlowsPower",
                                                                     "",
                                                                     flurry_of_blows_title_string,
                                                                     flurry_of_blows_description_string,
                                                                     DatabaseHelper.FeatureDefinitionPowers.PowerReckless.GuiPresentation.SpriteReference,
                                                                     effect,
                                                                     RuleDefinitions.ActivationTime.BonusAction,
                                                                     2,
                                                                     RuleDefinitions.UsesDetermination.Fixed,
                                                                     RuleDefinitions.RechargeRate.ShortRest
                                                                     );
            flurry_of_blows.restrictions = new List<NewFeatureDefinitions.IRestriction>()
                                            {
                                                new NewFeatureDefinitions.NoArmorRestriction(),
                                                new NewFeatureDefinitions.NoShieldRestriction(),
                                                new NewFeatureDefinitions.UsedAllMainAttacksRestriction(),
                                                new NewFeatureDefinitions.FreeOffHandRestriciton()
                                            };
        }


        static void createUnarmoredMovement()
        {
            string unarmored_movement_title_string = "Feature/&MonkClassUnarmoredMovementTitle";
            string unarmored_movement_description_string = "Feature/&MonkClassUnarmoredMovementDescription";

            string unarmored_movement_improvement_title_string = "Feature/&MonkClassUnarmoredMovementImprovementTitle";
            string unarmored_movement_improvement_description_string = "Feature/&MonkClassUnarmoredMovementImprovementDescription";

            var unarmored_movement_feature = Helpers.CopyFeatureBuilder<FeatureDefinitionMovementAffinity>.createFeatureCopy("MonkClassUnarmoredMovementEffectFeature",
                                                                                                            "",
                                                                                                            Common.common_no_title,
                                                                                                            Common.common_no_title,
                                                                                                            null,
                                                                                                            DatabaseHelper.FeatureDefinitionMovementAffinitys.MovementAffinityLongstrider
                                                                                                            );
            unarmored_movement = Helpers.FeatureBuilder<NewFeatureDefinitions.MovementBonusBasedOnEquipment>.createFeature("MonkClassUnarmoredMovementFeature",
                                                                                                                         "",
                                                                                                                         unarmored_movement_title_string,
                                                                                                                         unarmored_movement_description_string,
                                                                                                                         null,
                                                                                                                         a =>
                                                                                                                         {
                                                                                                                             a.allowArmor = false;
                                                                                                                             a.allowShield = false;
                                                                                                                             a.modifiers = new List<FeatureDefinition> { unarmored_movement_feature };
                                                                                                                         }
                                                                                                                         );

            var unarmored_movement_improvement_feature = Helpers.CopyFeatureBuilder<FeatureDefinitionMovementAffinity>.createFeatureCopy("MonkClassUnarmoredMovementImprovementEffectFeature",
                                                                                                            "",
                                                                                                            Common.common_no_title,
                                                                                                            Common.common_no_title,
                                                                                                            null,
                                                                                                            DatabaseHelper.FeatureDefinitionMovementAffinitys.MovementAffinityLongstrider,
                                                                                                            c =>
                                                                                                            {
                                                                                                                c.SetBaseSpeedAdditiveModifier(1);
                                                                                                            }
                                                                                                            );

            for (int i = 0; i < unarmored_movement_improvement_levels.Count; i++)
            {
                var lvl = unarmored_movement_improvement_levels[i];
                int bonus_feet = 15 + i * 5;
                unarmored_movement_improvements[lvl] = Helpers.FeatureBuilder<NewFeatureDefinitions.MovementBonusBasedOnEquipment>.createFeature($"MonkClassUnarmoredMovementImprovementFeature{lvl}",
                                                                                                             "",
                                                                                                             unarmored_movement_improvement_title_string,
                                                                                                             Helpers.StringProcessing
                                                                                                                .replaceTagsInString(unarmored_movement_improvement_description_string,
                                                                                                                                     unarmored_movement_improvement_description_string + lvl.ToString(),
                                                                                                                                     ("<LEVEL>", lvl.ToString()),
                                                                                                                                     ("<FEET>", bonus_feet.ToString())
                                                                                                                                     ),
                                                                                                             null,
                                                                                                             a =>
                                                                                                             {
                                                                                                                 a.allowArmor = false;
                                                                                                                 a.allowShield = false;
                                                                                                                 a.modifiers = new List<FeatureDefinition> { unarmored_movement_improvement_feature };
                                                                                                             }
                                                                                                             );
            }
        }



        static void createUnarmoredDefense()
        {
            unarmored_defense = Helpers.FeatureBuilder<NewFeatureDefinitions.ArmorClassStatBonus>.createFeature("MonkClassUnarmoredDefense",
                                                                                                                "",
                                                                                                                "Feature/&MonkClassUnarmoredDefenseTitle",
                                                                                                                "Feature/&MonkClassUnarmoredDefenseDescription",
                                                                                                                null,
                                                                                                                a =>
                                                                                                                {
                                                                                                                    a.armorAllowed = false;
                                                                                                                    a.shieldAlowed = false;
                                                                                                                    a.stat = Helpers.Stats.Wisdom;
                                                                                                                    a.forbiddenConditions = new List<ConditionDefinition>
                                                                                                                    {
                                                                                                                        DatabaseHelper.ConditionDefinitions.ConditionBarkskin,
                                                                                                                        DatabaseHelper.ConditionDefinitions.ConditionMagicallyArmored
                                                                                                                    };
                                                                                                                }
                                                                                                                );
        }


        static void createMartialArts()
        {
            string martial_arts_title_string = "Feature/&MonkClassMartialArtsTitle";
            string martial_arts_description_string = "Feature/&MonkClassMartialArtsDescription";

            var dex_on_weapons = Helpers.FeatureBuilder<NewFeatureDefinitions.canUseDexterityWithSpecifiedWeaponTypes>.createFeature("MonkClassMartialArtsDexForWeapons",
                                                                                                                                        "",
                                                                                                                                        Common.common_no_title,
                                                                                                                                        Common.common_no_title,
                                                                                                                                        null,
                                                                                                                                        a =>
                                                                                                                                        {
                                                                                                                                            a.weaponTypes = monk_weapons;
                                                                                                                                            a.allowArmor = false;
                                                                                                                                            a.allowShield = false;
                                                                                                                                        }
                                                                                                                                        );

            var damage_dice = Helpers.FeatureBuilder<NewFeatureDefinitions.OvewriteDamageOnSpecificWeaponTypesBasedOnClassLevel>.createFeature("MonkClassMartialArtsDamageDice",
                                                                                                                                    "",
                                                                                                                                    Common.common_no_title,
                                                                                                                                    Common.common_no_title,
                                                                                                                                    null,
                                                                                                                                    a =>
                                                                                                                                    {
                                                                                                                                        a.weaponTypes = monk_weapons;
                                                                                                                                        a.allowArmor = false;
                                                                                                                                        a.allowShield = false;
                                                                                                                                        a.characterClass = monk_class;
                                                                                                                                        a.levelDamageList = new List<(int, int, RuleDefinitions.DieType)>
                                                                                                                                        {
                                                                                                                                            (4, 1, RuleDefinitions.DieType.D4),
                                                                                                                                            (10, 1, RuleDefinitions.DieType.D6),
                                                                                                                                            (16, 1, RuleDefinitions.DieType.D8),
                                                                                                                                            (20, 1, RuleDefinitions.DieType.D10)
                                                                                                                                        };
                                                                                                                                    }
                                                                                                                                    );

            var bonus_unarmed_attack = Helpers.FeatureBuilder<NewFeatureDefinitions.ExtraUnarmedAttack>.createFeature("MonkClassMartialArtsBonusUnarmedAttack",
                                                                                                                                "",
                                                                                                                                Common.common_no_title,
                                                                                                                                Common.common_no_title,
                                                                                                                                null,
                                                                                                                                a =>
                                                                                                                                {
                                                                                                                                    a.allowedWeaponTypes = monk_weapons;
                                                                                                                                    a.allowArmor = false;
                                                                                                                                    a.allowShield = false;
                                                                                                                                    a.clearAllAttacks = false;
                                                                                                                                    a.actionType = ActionDefinitions.ActionType.Bonus;
                                                                                                                                }
                                                                                                                                );
            martial_arts = Helpers.FeatureSetBuilder.createFeatureSet("MonkClassMartialArts",
                                                                      "",
                                                                      martial_arts_title_string,
                                                                      martial_arts_description_string,
                                                                      false,
                                                                      FeatureDefinitionFeatureSet.FeatureSetMode.Union,
                                                                      false,
                                                                      dex_on_weapons,
                                                                      damage_dice,
                                                                      bonus_unarmed_attack
                                                                      );

        }


        static void createExtraAttack()
        {
            extra_attack = Helpers.CopyFeatureBuilder<FeatureDefinitionAttributeModifier>.createFeatureCopy("MonkClassExtraAttack",
                                                                                                            "",
                                                                                                            "",
                                                                                                            "",
                                                                                                            null,
                                                                                                            DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierFighterExtraAttack
                                                                                                            );
        }


        static void createRage()
        {
            string rage_title_string = "Feature/&MonkClassRagePowerTitle";
            string rage_description_string = "Feature/&MonkClassRagePowerDescription";
            string rage_condition_string = "Rules/&MonkClassRageCondition";

            rage_spellcasting_forbiden = Helpers.FeatureBuilder<NewFeatureDefinitions.SpellcastingForbidden>.createFeature("MonkClassRageSpellcastingForbidden",
                                                                                                                           "",
                                                                                                                           Common.common_no_title,
                                                                                                                           Common.common_no_title,
                                                                                                                           Common.common_no_icon,
                                                                                                                           r =>
                                                                                                                           {
                                                                                                                               r.exceptionFeatures = new List<FeatureDefinition>();
                                                                                                                           }
                                                                                                                           );

            var condition_can_continue_rage = Helpers.ConditionBuilder.createConditionWithInterruptions("MonkClassCanContinueRageCondition",
                                                                                                          "",
                                                                                                          "Rules/&MonkClassCanContinueRageCondition",
                                                                                                          Common.common_no_title,
                                                                                                          Common.common_no_icon,
                                                                                                          DatabaseHelper.ConditionDefinitions.ConditionHeroism,
                                                                                                          new RuleDefinitions.ConditionInterruption[] { }
                                                                                                          );
            condition_can_continue_rage.SetSilentWhenAdded(true);
            condition_can_continue_rage.SetSilentWhenRemoved(true);

            NewFeatureDefinitions.PowerWithRestrictions previous_power = null;

            foreach (var kv in rage_bonus_damage_level_map)
            {
                var damage_bonus = Helpers.FeatureBuilder<NewFeatureDefinitions.WeaponDamageBonusWithSpecificStat>.createFeature("MonkClassRageDamageBonus" + kv.Value.ToString(),
                                                                                                                   "",
                                                                                                                   rage_condition_string,
                                                                                                                   rage_condition_string,
                                                                                                                   null,
                                                                                                                   d =>
                                                                                                                   {
                                                                                                                       d.value = kv.Key;
                                                                                                                       d.attackStat = Helpers.Stats.Strength;
                                                                                                                   }
                                                                                                                   );

                var rage_condition = Helpers.ConditionBuilder.createConditionWithInterruptions("MonkClassRageCondition" + kv.Value.ToString(),
                                                                                          "",
                                                                                          rage_condition_string,
                                                                                          rage_description_string,
                                                                                          null,
                                                                                          DatabaseHelper.ConditionDefinitions.ConditionHeroism,
                                                                                          new RuleDefinitions.ConditionInterruption[] { },
                                                                                          DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityBludgeoningResistance,
                                                                                          DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinityPiercingResistance,
                                                                                          DatabaseHelper.FeatureDefinitionDamageAffinitys.DamageAffinitySlashingResistance,
                                                                                          DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityConditionBullsStrength,
                                                                                          damage_bonus,
                                                                                          rage_spellcasting_forbiden
                                                                                          );

                var rage_watcher = Helpers.FeatureBuilder<NewFeatureDefinitions.RageWatcher>.createFeature("MonkClassRageAttackWatcher" + kv.Value.ToString(),
                                                                                               "",
                                                                                               Common.common_no_title,
                                                                                               Common.common_no_title,
                                                                                               null,
                                                                                               r =>
                                                                                               {
                                                                                                   r.requiredCondition = condition_can_continue_rage;
                                                                                                   r.conditionToRemove = rage_condition;
                                                                                               }
                                                                                               );

                rage_condition.Features.Add(rage_watcher);

                var effect = new EffectDescription();
                effect.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainBattleHeraldOfBattle.EffectDescription);
                effect.SetRangeType(RuleDefinitions.RangeType.Self);
                effect.SetRangeParameter(1);
                effect.DurationParameter = 1;
                effect.DurationType = RuleDefinitions.DurationType.Minute;
                effect.EffectForms.Clear();
                effect.SetTargetType(RuleDefinitions.TargetType.Self);

                var effect_form = new EffectForm();
                effect_form.ConditionForm = new ConditionForm();
                effect_form.FormType = EffectForm.EffectFormType.Condition;
                effect_form.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
                effect_form.ConditionForm.ConditionDefinition = rage_condition;
                effect.EffectForms.Add(effect_form);

                var rage_power = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                          .createPower("MonkClassRagePower" + kv.Value.ToString(),
                                                             "",
                                                             Helpers.StringProcessing.appendToString(rage_title_string,
                                                                                                             rage_title_string + kv.Value.ToString(),
                                                                                                             $" (+{kv.Key})"),
                                                             rage_description_string,
                                                             //DatabaseHelper.FeatureDefinitionPowers.PowerDomainBattleDivineWrath.GuiPresentation.SpriteReference,
                                                             DatabaseHelper.SpellDefinitions.Heroism.GuiPresentation.SpriteReference,
                                                             effect,
                                                             RuleDefinitions.ActivationTime.BonusAction,
                                                             2 + rage_uses_increase_levels.Count(l => l < kv.Value),
                                                             RuleDefinitions.UsesDetermination.Fixed,
                                                             RuleDefinitions.RechargeRate.LongRest
                                                             );
                rage_power.restrictions = new List<NewFeatureDefinitions.IRestriction>()
                {
                    new NewFeatureDefinitions.InBattleRestriction(),
                    new NewFeatureDefinitions.NoConditionRestriction(condition_can_continue_rage),
                    new NewFeatureDefinitions.ArmorTypeRestriction(DatabaseHelper.ArmorCategoryDefinitions.HeavyArmorCategory, inverted: true)
                };

                rage_power.SetShortTitleOverride(rage_title_string);
                if (previous_power != null)
                {
                    rage_power.SetOverriddenPower(previous_power);
                }
                previous_power = rage_power;
                rage_powers.Add(kv.Value, rage_power);
            }

            string rage_extra_use_title_string = "Feature/&MonkClassRageExtraUseTitle";
            string rage_extra_use_description_string = "Feature/&MonkClassRageExtraUseDescription";

            foreach (var l in rage_uses_increase_levels)
            {
                var feature = Helpers.FeatureBuilder<NewFeatureDefinitions.IncreaseNumberOfPowerUses>.createFeature("MonkClassExtraRage" + l.ToString(),
                                                                                                                    "",
                                                                                                                    rage_extra_use_title_string,
                                                                                                                    rage_extra_use_description_string,
                                                                                                                    null,
                                                                                                                    f =>
                                                                                                                    {
                                                                                                                        f.value = 1;
                                                                                                                        f.powers = rage_powers.Where(kv => kv.Key < l).Select(kv => kv.Value)
                                                                                                                                        .Cast<FeatureDefinitionPower>().ToList();
                                                                                                                    }
                                                                                                                    );
                rage_power_extra_use.Add(l, feature);
            }
        }


        static CharacterSubclassDefinition createPathOfFrozenFury()
        {
            createWintersFury();
            createFrigidBody();
            createNumb();

            var gui_presentation = new GuiPresentationBuilder(
                    "Subclass/&MonkSubclassPrimalPathOfFrozenFuryDescription",
                    "Subclass/&MonkSubclassPrimalPathOfFrozenFuryTitle")
                    .SetSpriteReference(DatabaseHelper.CharacterSubclassDefinitions.DomainElementalCold.GuiPresentation.SpriteReference)
                    .Build();

            CharacterSubclassDefinition definition = new CharacterSubclassDefinitionBuilder("MonkSubclassPrimalPathOfFrozenFury", "8039f501-1be5-47e8-9777-82129a24f46d")
                    .SetGuiPresentation(gui_presentation)
                    .AddFeatureAtLevel(frozen_fury, 3)
                    .AddFeatureAtLevel(frigid_body, 6)
                    .AddFeatureAtLevel(numb, 10)
                    .AddToDB();

            frozen_fury_rage_feature.requiredSubclass = definition;
            return definition;
        }


        static void createNumb()
        {
            string numb_title_string = "Feature/&MonkSubclassFrozenFuryNumbTitle";
            string numb_description_string = "Feature/&MonkSubclassFrozenFuryNumbDescription";

            var conditons = new List<ConditionDefinition> { DatabaseHelper.ConditionDefinitions.ConditionPoisoned, DatabaseHelper.ConditionDefinitions.ConditionFrightened };
            var numb_immunity = Helpers.FeatureBuilder<NewFeatureDefinitions.ImmunityToCondtionIfHasSpecificConditions>.createFeature("MonkSubclassFrozenFuryNumbImmunity",
                                                                                                                          "",
                                                                                                                          numb_title_string,
                                                                                                                          numb_description_string,
                                                                                                                          null,
                                                                                                                          f =>
                                                                                                                          {
                                                                                                                              f.immuneCondtions = conditons;
                                                                                                                              f.requiredConditions = new List<ConditionDefinition>();
                                                                                                                          }
                                                                                                                          );

            var numb_removal = Helpers.FeatureBuilder<NewFeatureDefinitions.RemoveConditionsOnConditionApplication>.createFeature("MonkSubclassFrozenFuryNumbRemoval",
                                                                                                              "",
                                                                                                              numb_title_string,
                                                                                                              numb_description_string,
                                                                                                              null,
                                                                                                              f =>
                                                                                                              {
                                                                                                                  f.removeConditions = conditons;
                                                                                                                  f.appliedConditions = new List<ConditionDefinition>();
                                                                                                              }
                                                                                                              );

            foreach (var rp in rage_powers)
            {
                numb_immunity.requiredConditions.Add(rp.Value.EffectDescription.EffectForms[0].ConditionForm.ConditionDefinition);
                numb_removal.appliedConditions.Add(rp.Value.EffectDescription.EffectForms[0].ConditionForm.ConditionDefinition);
            }


            numb = Helpers.FeatureSetBuilder.createFeatureSet("MonkSubclassFrozenFuryNumb",
                                                              "",
                                                              numb_title_string,
                                                              numb_description_string,
                                                              false,
                                                              FeatureDefinitionFeatureSet.FeatureSetMode.Union,
                                                              false,
                                                              numb_immunity,
                                                              numb_removal
                                                              );
        }


        static void createFrigidBody()
        {
            string frigid_body_title_string = "Feature/&MonkSubclassFrozenFuryFrigidBodyTitle";
            string frigid_body_description_string = "Feature/&MonkSubclassFrozenFuryFrigidBodyDescription";

            frigid_body = Helpers.FeatureBuilder<NewFeatureDefinitions.ArmorBonusAgainstAttackType>.createFeature("MonkSubclassFrozenFuryFrigidBody",
                                                                                                                  "",
                                                                                                                  frigid_body_title_string,
                                                                                                                  frigid_body_description_string,
                                                                                                                  null,
                                                                                                                  f =>
                                                                                                                  {
                                                                                                                      f.applyToMelee = false;
                                                                                                                      f.applyToRanged = true;
                                                                                                                      f.requiredConditions = new List<ConditionDefinition>();
                                                                                                                      f.value = 2;
                                                                                                                  }
                                                                                                                  );
            foreach (var rp in rage_powers)
            {
                frigid_body.requiredConditions.Add(rp.Value.EffectDescription.EffectForms[0].ConditionForm.ConditionDefinition);
            }
                                                                
        }


        static void createWintersFury()
        {
            string winters_fury_title_string = "Feature/&MonkSubclassFrozenFuryWintersFuryTitle";
            string winters_fury_description_string = "Feature/&MonkSubclassFrozenFuryWintersFuryDescription";

            List<(int level, int dice_number, RuleDefinitions.DieType die_type)> frozen_fury_damages = new List<(int level, int dice_number, RuleDefinitions.DieType die_type)>
            {
                {(5, 1, RuleDefinitions.DieType.D6) },
                {(9, 1, RuleDefinitions.DieType.D10) },
                {(13, 2, RuleDefinitions.DieType.D6) },
                {(20, 2, RuleDefinitions.DieType.D10) }
            };

            List<(int, FeatureDefinitionPower)> power_list = new List<(int, FeatureDefinitionPower)>();
            foreach (var entry in frozen_fury_damages)
            {
                var damage = new DamageForm();
                damage.DiceNumber = entry.dice_number;
                damage.DieType = entry.die_type;
                damage.VersatileDieType = entry.die_type;
                damage.DamageType = Helpers.DamageTypes.Cold;

                var effect = new EffectDescription();
                effect.Copy(DatabaseHelper.SpellDefinitions.FireShieldCold.EffectDescription);
                effect.SetRangeType(RuleDefinitions.RangeType.Self);
                effect.SetTargetType(RuleDefinitions.TargetType.Sphere);
                effect.SetTargetSide(RuleDefinitions.Side.All);
                effect.SetTargetParameter(2);
                effect.SetTargetParameter2(1);
                effect.SetRangeParameter(1);
                effect.SetCanBePlacedOnCharacter(true);
                effect.DurationType = RuleDefinitions.DurationType.Instantaneous;
                effect.DurationParameter = 0;
                effect.SetEffectParticleParameters(DatabaseHelper.FeatureDefinitionPowers.PowerDomainElementalHeraldOfTheElementsCold.EffectDescription.EffectParticleParameters);
                effect.SetTargetExcludeCaster(true);

                effect.EffectForms.Clear();
                var effect_form = new EffectForm();
                effect_form.DamageForm = damage;
                effect_form.FormType = EffectForm.EffectFormType.Damage;
                effect.EffectForms.Add(effect_form);


                var power = Helpers.PowerBuilder.createPower("MonkSubclassFrozenFuryWintersFuryPower" + entry.level,
                                                         "",
                                                         winters_fury_title_string,
                                                         winters_fury_description_string,
                                                         null,
                                                         DatabaseHelper.FeatureDefinitionPowers.PowerDomainElementalHeraldOfTheElementsCold,
                                                         effect,
                                                         RuleDefinitions.ActivationTime.NoCost,
                                                         1,
                                                         RuleDefinitions.UsesDetermination.Fixed,
                                                         RuleDefinitions.RechargeRate.AtWill,
                                                         show_casting: false);
                power_list.Add((entry.level, power));
            }

            frozen_fury_rage_feature = Helpers.FeatureBuilder<NewFeatureDefinitions.ApplyPowerOnTurnEndBasedOnClassLevel>.createFeature("MonkSubclassFrozenFuryWintersFuryRageFeature",
                                                                                                                                          "",
                                                                                                                                          winters_fury_title_string,
                                                                                                                                          winters_fury_description_string,
                                                                                                                                          null,
                                                                                                                                          f =>
                                                                                                                                          {
                                                                                                                                              f.characterClass = monk_class;
                                                                                                                                              f.powerLevelList = power_list;
                                                                                                                                              //will fill subclass in FrozenFuryPath creation
                                                                                                                                          }
                                                                                                                                          );

            foreach (var rp in rage_powers)
            {
                var rage_conditon = rp.Value.EffectDescription.EffectForms[0].conditionForm.conditionDefinition;
                rage_conditon.Features.Add(frozen_fury_rage_feature);
            }

            frozen_fury = Helpers.OnlyDescriptionFeatureBuilder.createOnlyDescriptionFeature("MonkSubclassFrozenFuryWintersFuryFeature",
                                                                                             "",
                                                                                             winters_fury_title_string,
                                                                                             winters_fury_description_string
                                                                                             );

        }

        static void createWarShamanSpellcasting()
        {
            war_shaman_spelllist = Helpers.SpelllistBuilder.create9LevelSpelllist("MonkSubclassWarshamanSpelllist", "", "",
                                                                    new List<SpellDefinition>
                                                                    {
                                                                                    DatabaseHelper.SpellDefinitions.AnnoyingBee,
                                                                                    DatabaseHelper.SpellDefinitions.Guidance,
                                                                                    DatabaseHelper.SpellDefinitions.PoisonSpray,
                                                                                    DatabaseHelper.SpellDefinitions.Resistance,
                                                                    },
                                                                    new List<SpellDefinition>
                                                                    {
                                                                                    DatabaseHelper.SpellDefinitions.AnimalFriendship,
                                                                                    DatabaseHelper.SpellDefinitions.CharmPerson,
                                                                                    DatabaseHelper.SpellDefinitions.CureWounds,
                                                                                    DatabaseHelper.SpellDefinitions.DetectMagic,
                                                                                    DatabaseHelper.SpellDefinitions.FaerieFire,
                                                                                    DatabaseHelper.SpellDefinitions.FogCloud,
                                                                                    DatabaseHelper.SpellDefinitions.Goodberry,
                                                                                    DatabaseHelper.SpellDefinitions.HealingWord,
                                                                                    DatabaseHelper.SpellDefinitions.Jump,
                                                                                    DatabaseHelper.SpellDefinitions.Longstrider,
                                                                                    DatabaseHelper.SpellDefinitions.Thunderwave
                                                                    },
                                                                    new List<SpellDefinition>
                                                                    {
                                                                                    DatabaseHelper.SpellDefinitions.Barkskin,
                                                                                    DatabaseHelper.SpellDefinitions.Darkvision,
                                                                                    DatabaseHelper.SpellDefinitions.EnhanceAbility,
                                                                                    DatabaseHelper.SpellDefinitions.FindTraps,
                                                                                    //DatabaseHelper.SpellDefinitions.FlameBlade,
                                                                                    DatabaseHelper.SpellDefinitions.FlamingSphere,
                                                                                    DatabaseHelper.SpellDefinitions.GustOfWind,
                                                                                    DatabaseHelper.SpellDefinitions.HoldPerson,
                                                                                    DatabaseHelper.SpellDefinitions.LesserRestoration,
                                                                                    DatabaseHelper.SpellDefinitions.PassWithoutTrace,
                                                                                    DatabaseHelper.SpellDefinitions.ProtectionFromPoison
                                                                    },
                                                                    new List<SpellDefinition>
                                                                    {
                                                                                    DatabaseHelper.SpellDefinitions.ConjureAnimals,
                                                                                    DatabaseHelper.SpellDefinitions.Daylight,
                                                                                    DatabaseHelper.SpellDefinitions.DispelMagic,
                                                                                    DatabaseHelper.SpellDefinitions.ProtectionFromEnergy,
                                                                                    DatabaseHelper.SpellDefinitions.SleetStorm,
                                                                                    DatabaseHelper.SpellDefinitions.WindWall
                                                                    },
                                                                    new List<SpellDefinition>
                                                                    {
                                                                                    DatabaseHelper.SpellDefinitions.Blight,
                                                                                    DatabaseHelper.SpellDefinitions.Confusion,
                                                                                    DatabaseHelper.SpellDefinitions.FreedomOfMovement,
                                                                                    DatabaseHelper.SpellDefinitions.GiantInsect,
                                                                                    DatabaseHelper.SpellDefinitions.IceStorm,
                                                                                    DatabaseHelper.SpellDefinitions.Stoneskin,
                                                                                    DatabaseHelper.SpellDefinitions.WallOfFire
                                                                    }
                                                                    );
            war_shaman_spelllist.SetMaxSpellLevel(4);
            war_shaman_spelllist.SetHasCantrips(true);

            war_shaman_spellcasting = Helpers.SpellcastingBuilder.createSpontaneousSpellcasting("MonkSubclassWarshamanSpellcasting",
                                                                                              "",
                                                                                              "Feature/&MonkSubclassWarShamanClassSpellcastingTitle",
                                                                                              "Feature/&MonkSubclassWarShamanClassSpellcastingDescription",
                                                                                              war_shaman_spelllist,
                                                                                              Helpers.Stats.Wisdom,
                                                                                              new List<int> {0, 0, 2, 2, 2, 2, 2, 2, 2, 2,
                                                                                                             3, 3, 3, 3, 3, 3, 3, 3, 3, 3 },
                                                                                              new List<int> { 0,  0,  3,  4,  4,  4,  5,  6,  6,  7,
                                                                                                              8,  8,  9, 10, 10, 11, 11, 11, 12, 13},
                                                                                              Helpers.Misc.createSpellSlotsByLevel(new List<int> { 0, 0, 0, 0 }, 
                                                                                                                                   new List<int> { 0, 0, 0, 0 },
                                                                                                                                   new List<int> { 2, 0, 0, 0 },//3
                                                                                                                                   new List<int> { 3, 0, 0, 0 },//4
                                                                                                                                   new List<int> { 3, 0, 0, 0 },//5
                                                                                                                                   new List<int> { 3, 0, 0, 0 },//6
                                                                                                                                   new List<int> { 4, 2, 0, 0 },//7
                                                                                                                                   new List<int> { 4, 2, 0, 0 },//8
                                                                                                                                   new List<int> { 4, 2, 0, 0 },//9
                                                                                                                                   new List<int> { 4, 3, 0, 0 },//10
                                                                                                                                   new List<int> { 4, 3, 0, 0 },//11
                                                                                                                                   new List<int> { 4, 3, 0, 0 },//12
                                                                                                                                   new List<int> { 4, 3, 2, 0 },//13
                                                                                                                                   new List<int> { 4, 3, 2, 0 },//14
                                                                                                                                   new List<int> { 4, 3, 2, 0 },//15
                                                                                                                                   new List<int> { 4, 3, 3, 0 },//16
                                                                                                                                   new List<int> { 4, 3, 3, 0 },//17
                                                                                                                                   new List<int> { 4, 3, 3, 0 },//18
                                                                                                                                   new List<int> { 4, 3, 3, 1 },//19
                                                                                                                                   new List<int> { 4, 3, 3, 1 }//20
                                                                                                                                   )
                                                                                              );
            war_shaman_spellcasting.SetSpellCastingLevel(-1);
            war_shaman_spellcasting.SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Subclass);
        }


        static void createRagecaster()
        {
            string ragecaster_title_string = "Feature/&MonkSubclassWarShamanClassRagecasterTitle";
            string ragecaster_description_string = "Feature/&MonkSubclassWarShamanClassRagecasterDescription";

            ragecaster = Helpers.OnlyDescriptionFeatureBuilder.createOnlyDescriptionFeature("MonkSubclassWarshamanRagecaster",
                                                                                            "",
                                                                                            ragecaster_title_string,
                                                                                            ragecaster_description_string
                                                                                            );
            rage_spellcasting_forbiden.exceptionFeatures.Add(ragecaster);
        }


        static void createShareRage()
        {
            string share_rage_title_string = "Feature/&MonkSubclassWarShamanClassShareRageTitle";
            string share_rage_description_string = "Feature/&MonkSubclassWarShamanClassShareRageDescription";

            NewFeatureDefinitions.PowerWithRestrictions previous_power = null;

            foreach (var kv in rage_bonus_damage_level_map)
            {
                var power = Helpers.CopyFeatureBuilder<NewFeatureDefinitions.PowerWithRestrictions>.createFeatureCopy("MonkSubclassWarshamanShareRagePower" + kv.Value.ToString(),
                                                                                                                      "",
                                                                                                                      Helpers.StringProcessing.appendToString(share_rage_title_string,
                                                                                                                                                              share_rage_title_string + kv.Value.ToString(),
                                                                                                                                                              $" (+{kv.Key})"),
                                                                                                                      share_rage_description_string,
                                                                                                                      DatabaseHelper.FeatureDefinitionPowers.PowerDomainLawHolyRetribution.GuiPresentation.SpriteReference,
                                                                                                                      rage_powers[kv.Value]
                                                                                                                      );
                var effect = new EffectDescription();
                effect.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainBattleHeraldOfBattle.EffectDescription);
                effect.SetRangeType(RuleDefinitions.RangeType.Distance);
                effect.SetRangeParameter(6);
                effect.SetTargetParameter(1);
                effect.SetTargetParameter2(1);
                effect.DurationParameter = 1;
                effect.DurationType = RuleDefinitions.DurationType.Minute;
                effect.EffectForms.Clear();
                effect.SetTargetType(RuleDefinitions.TargetType.Individuals);
                effect.SetTargetSide(RuleDefinitions.Side.Ally);
                effect.SetTargetFilteringTag((RuleDefinitions.TargetFilteringTag)(ExtendedEnums.ExtraTargetFilteringTag.NonCaster | ExtendedEnums.ExtraTargetFilteringTag.NoHeavyArmor));

                effect.EffectForms.Add(rage_powers[kv.Value].EffectDescription.EffectForms[0]);
                var effect_form = new EffectForm();
                effect_form.ConditionForm = new ConditionForm();
                effect_form.FormType = EffectForm.EffectFormType.Condition;
                effect_form.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
                effect_form.ConditionForm.ConditionDefinition = rage_powers[kv.Value].EffectDescription.EffectForms[0].conditionForm.conditionDefinition;
                effect_form.conditionForm.SetApplyToSelf(true);
                effect.EffectForms.Add(effect_form);

                power.SetEffectDescription(effect);
                power.SetRechargeRate(RuleDefinitions.RechargeRate.SpellSlot);
                power.SetSpellcastingFeature(war_shaman_spellcasting);
                power.SetFixedUsesPerRecharge(10);

                if (previous_power != null)
                {
                    power.SetOverriddenPower(previous_power);
                }
                previous_power = power;
                power.SetShortTitleOverride(share_rage_title_string);
                power.linkedPower = rage_powers[kv.Value];

                share_rage_powers.Add(kv.Value, power);
            }
        }


        static CharacterSubclassDefinition createPathOfWarShaman()
        {
            createWarShamanSpellcasting();
            createShareRage();
            createRagecaster();

            var gui_presentation = new GuiPresentationBuilder(
                    "Subclass/&MonkSubclassPrimalPathOfWarShamanDescription",
                    "Subclass/&MonkSubclassPrimalPathOfWarShamanTitle")
                    .SetSpriteReference(DatabaseHelper.CharacterSubclassDefinitions.TraditionGreenmage.GuiPresentation.SpriteReference)
                    .Build();

            CharacterSubclassDefinition definition = new CharacterSubclassDefinitionBuilder("MonkSubclassPrimalPathOfWarShaman", "4c9bf92d-873f-43e6-aa47-b4e5838c31d6")
                    .SetGuiPresentation(gui_presentation)
                    .AddFeatureAtLevel(war_shaman_spellcasting, 3)
                    .AddFeatureAtLevel(share_rage_powers[1], 6)
                    .AddFeatureAtLevel(share_rage_powers[9], 9)
                    .AddFeatureAtLevel(ragecaster, 10)
                    .AddFeatureAtLevel(share_rage_powers[16], 16)
                    .AddToDB();

            return definition;
        }


        static void createFrenzy()
        {
            string frenzy_exhausted_title_string = "Feature/&MonkSubclassBerserkerFrenzyExhaustedTitle";
            string frenzy_exhausted_description_string = "Feature/&MonkSubclassBerserkerFrenzyExhaustedDescription";
            string frenzy_condition_title_string = "Rules/&MonkSubclassBerserkerFrenzyCondition";


            exhausted_after_frenzy_condition = Helpers.ConditionBuilder.createConditionWithInterruptions("MonkSubclassBerserkerFrenzyExhasutedCondition",
                                                                                      "",
                                                                                      frenzy_exhausted_title_string,
                                                                                      frenzy_exhausted_description_string,
                                                                                      null,
                                                                                      DatabaseHelper.ConditionDefinitions.ConditionExhausted,
                                                                                      new RuleDefinitions.ConditionInterruption[] { RuleDefinitions.ConditionInterruption.AnyBattleTurnEnd },
                                                                                      DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityBestowCurseStrength,
                                                                                      DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityBestowCurseDexterity,
                                                                                      DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityBestowCurseConstitution,
                                                                                      DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityBestowCurseIntelligence,
                                                                                      DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityBestowCurseWisdom,
                                                                                      DatabaseHelper.FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityBestowCurseCharisma
                                                                                      );
            exhausted_after_frenzy_condition.SetConditionType(RuleDefinitions.ConditionType.Detrimental);

            string frenzy_title_string = "Feature/&MonkSubclassBerserkerFrenzyTitle";
            string frenzy_description_string = "Feature/&MonkSubclassBerserkerFrenzyDescription";
            var feature_attack = Helpers.CopyFeatureBuilder<FeatureDefinitionAttributeModifier>.createFeatureCopy("MonkSubclassBerserkerFrenzyExtraAttack",
                                                                                                           "",
                                                                                                           "",
                                                                                                           "",
                                                                                                           null,
                                                                                                           DatabaseHelper.FeatureDefinitionAttributeModifiers.AttributeModifierFighterExtraAttack
                                                                                                           );
            var feature_no_bonus_attack = Helpers.CopyFeatureBuilder<FeatureDefinitionActionAffinity>.createFeatureCopy("MonkSubclassBerserkerNoBonusAction",
                                                                                                                           "",
                                                                                                                           "",
                                                                                                                           "",
                                                                                                                           null,
                                                                                                                           DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinityConditionRestrained,
                                                                                                                           a =>
                                                                                                                           {
                                                                                                                               a.allowedActionTypes = new bool[]
                                                                                                                               {
                                                                                                                                   true, false, true, true, true, true
                                                                                                                               };
                                                                                                                           }
                                                                                                                           );

            var condition = Helpers.ConditionBuilder.createConditionWithInterruptions("MonkSubclassBerserkerFrenzyCondition",
                                                                          "",
                                                                          frenzy_condition_title_string,
                                                                          frenzy_description_string,
                                                                          null,
                                                                          DatabaseHelper.ConditionDefinitions.ConditionHeraldOfBattle,
                                                                          new RuleDefinitions.ConditionInterruption[] {},
                                                                          feature_attack,
                                                                          feature_no_bonus_attack
                                                                          );

            var frenzy_watcher = Helpers.FeatureBuilder<NewFeatureDefinitions.FrenzyWatcher>.createFeature("MonkSubclassBerserkerFrenzyWatcher",
                                                                               "",
                                                                               Common.common_no_title,
                                                                               Common.common_no_title,
                                                                               null,
                                                                               r =>
                                                                               {
                                                                                   r.requiredConditions = rage_powers.Select(kv => kv.Value.EffectDescription.EffectForms[0].conditionForm.conditionDefinition).ToList();
                                                                                   r.targetCondition = condition;
                                                                                   r.afterCondition = exhausted_after_frenzy_condition;
                                                                               }
                                                                               );

            condition.Features.Add(frenzy_watcher);

            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.SpellDefinitions.Haste.EffectDescription);
            effect.SetRangeType(RuleDefinitions.RangeType.Self);
            effect.SetRangeParameter(1);
            effect.DurationParameter = 1;
            effect.DurationType = RuleDefinitions.DurationType.UntilLongRest;
            effect.EffectForms.Clear();
            effect.SetTargetType(RuleDefinitions.TargetType.Self);

            var effect_form = new EffectForm();
            effect_form.ConditionForm = new ConditionForm();
            effect_form.FormType = EffectForm.EffectFormType.Condition;
            effect_form.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
            effect_form.ConditionForm.ConditionDefinition = condition;
            effect.EffectForms.Add(effect_form);

            frenzy = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                      .createPower("MonkSubclassBerserkerFrenzyPower",
                                                         "",
                                                         frenzy_title_string,
                                                         frenzy_description_string,
                                                         DatabaseHelper.FeatureDefinitionPowers.PowerOathOfTirmarSmiteTheHidden.GuiPresentation.SpriteReference,
                                                         effect,
                                                         RuleDefinitions.ActivationTime.BonusAction,
                                                         1,
                                                         RuleDefinitions.UsesDetermination.Fixed,
                                                         RuleDefinitions.RechargeRate.LongRest
                                                         );
            frenzy.restrictions = new List<NewFeatureDefinitions.IRestriction>()
            {
                new NewFeatureDefinitions.InBattleRestriction(),
                new NewFeatureDefinitions.NoConditionRestriction(exhausted_after_frenzy_condition),
                new NewFeatureDefinitions.HasAtLeastOneConditionFromListRestriction(rage_powers.Select(kv => kv.Value.EffectDescription.EffectForms[0].conditionForm.conditionDefinition).ToArray())
            };

            frenzy.SetShortTitleOverride(frenzy_title_string);
        }


        static void createMindlessRage()
        {
            string mindless_rage_title_string = "Feature/&MonkSubclassMindlessRageTitle";
            string mindless_rage_description_string = "Feature/&MonkSubclassMindlessRageDescription";

            var conditons = new List<ConditionDefinition> { DatabaseHelper.ConditionDefinitions.ConditionCharmed, DatabaseHelper.ConditionDefinitions.ConditionFrightened };
            var immunity = Helpers.FeatureBuilder<NewFeatureDefinitions.ImmunityToCondtionIfHasSpecificConditions>.createFeature("MonkSubclassBerserkerMindlessRageImmunity",
                                                                                                                                    "",
                                                                                                                                    mindless_rage_title_string,
                                                                                                                                    mindless_rage_description_string,
                                                                                                                                    null,
                                                                                                                                    f =>
                                                                                                                                    {
                                                                                                                                        f.immuneCondtions = conditons;
                                                                                                                                        f.requiredConditions = new List<ConditionDefinition>();
                                                                                                                                    }
                                                                                                                                    );

            var removal = Helpers.FeatureBuilder<NewFeatureDefinitions.RemoveConditionsOnConditionApplication>.createFeature("MonkSubclassBerserkerMindlessRageRemoval",
                                                                                                                              "",
                                                                                                                              mindless_rage_title_string,
                                                                                                                              mindless_rage_description_string,
                                                                                                                              null,
                                                                                                                              f =>
                                                                                                                              {
                                                                                                                                  f.removeConditions = conditons;
                                                                                                                                  f.appliedConditions = new List<ConditionDefinition>();
                                                                                                                              }
                                                                                                                              );

            foreach (var rp in rage_powers)
            {
                immunity.requiredConditions.Add(rp.Value.EffectDescription.EffectForms[0].ConditionForm.ConditionDefinition);
                removal.appliedConditions.Add(rp.Value.EffectDescription.EffectForms[0].ConditionForm.ConditionDefinition);
            }


            mindless_rage = Helpers.FeatureSetBuilder.createFeatureSet("MonkSubclassBerserkerMindlessRage",
                                                                          "",
                                                                          mindless_rage_title_string,
                                                                          mindless_rage_description_string,
                                                                          false,
                                                                          FeatureDefinitionFeatureSet.FeatureSetMode.Union,
                                                                          false,
                                                                          immunity,
                                                                          removal
                                                                          );
        }


        static void createIntimidatingPresence()
        {
            string intimidating_presence_title_string = "Feature/&MonkSubclassBerserkerIntimidatingPresenceTitle";
            string intimidating_presence_description_string = "Feature/&MonkSubclassBerserkerIntimidatingPresenceDescription";

            var immune_condition = Helpers.CopyFeatureBuilder<ConditionDefinition>.createFeatureCopy("MonkSubclassBerserkerIntimidatingPresenceImmunityCondition",
                                                                                                     "",
                                                                                                     Common.common_no_title,
                                                                                                     Common.common_no_title,
                                                                                                     Common.common_no_icon,
                                                                                                     DatabaseHelper.ConditionDefinitions.ConditionTemporaryHitPoints,
                                                                                                     c =>
                                                                                                     {
                                                                                                         c.features = new List<FeatureDefinition>();
                                                                                                         c.SetConditionType(RuleDefinitions.ConditionType.Neutral);
                                                                                                         c.SetSpecialDuration(true);
                                                                                                         c.SetDurationType(RuleDefinitions.DurationType.UntilLongRest);
                                                                                                         c.SetDurationParameterDie(RuleDefinitions.DieType.D1);
                                                                                                     }
                                                                                                     );
            immune_condition.SetSilentWhenAdded(true);
            immune_condition.SetSilentWhenRemoved(true);


            //Add to our new effect
            EffectDescription effect_description = new EffectDescription();
            effect_description.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainOblivionMarkOfFate.EffectDescription);
            effect_description.SetSavingThrowDifficultyAbility(Helpers.Stats.Charisma);
            effect_description.SetDifficultyClassComputation(RuleDefinitions.EffectDifficultyClassComputation.AbilityScoreAndProficiency);
            effect_description.SavingThrowAbility = Helpers.Stats.Wisdom;
            effect_description.HasSavingThrow = true;
            effect_description.DurationType = RuleDefinitions.DurationType.Round;
            effect_description.DurationParameter = 2;
            effect_description.SetRangeType(RuleDefinitions.RangeType.Distance);
            effect_description.SetRangeParameter(6);
            effect_description.SetTargetType(RuleDefinitions.TargetType.Individuals);
            effect_description.SetTargetSide(RuleDefinitions.Side.Enemy);
            effect_description.SetEndOfEffect(RuleDefinitions.TurnOccurenceType.EndOfTurn);
            effect_description.immuneCreatureFamilies = new List<string> {Helpers.Misc.createImmuneIfHasConditionFamily(immune_condition) };
            

            effect_description.EffectForms.Clear();
            EffectForm effect_form = new EffectForm();
            effect_form.FormType = EffectForm.EffectFormType.Condition;
            effect_form.ConditionForm = new ConditionForm();
            effect_form.ConditionForm.ConditionDefinition = DatabaseHelper.ConditionDefinitions.ConditionFrightened;
            effect_form.hasSavingThrow = true;
            effect_form.SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.Negates;
            effect_form.conditionForm.operation = ConditionForm.ConditionOperation.Add;
            effect_description.EffectForms.Add(effect_form);

            intimidating_presence = Helpers.GenericPowerBuilder<FeatureDefinitionPower>
                                                          .createPower("MonkSubclassBerserkerIntimidatingPresencePower",
                                                             "",
                                                             intimidating_presence_title_string,
                                                             intimidating_presence_description_string,
                                                             //DatabaseHelper.FeatureDefinitionPowers.PowerDomainOblivionMarkOfFate.GuiPresentation.SpriteReference,
                                                             DatabaseHelper.SpellDefinitions.Fear.GuiPresentation.SpriteReference,
                                                             effect_description,
                                                             RuleDefinitions.ActivationTime.Action,
                                                             1,
                                                             RuleDefinitions.UsesDetermination.Fixed,
                                                             RuleDefinitions.RechargeRate.AtWill
                                                             );
            var immune_application = Helpers.FeatureBuilder<NewFeatureDefinitions.ApplyConditionOnPowerUseToTarget>.createFeature("MonkSubclassBerserkerIntimidatingPresenceApplyImmuneFeature",
                                                                                                                                  "",
                                                                                                                                  Common.common_no_title,
                                                                                                                                  Common.common_no_title,
                                                                                                                                  Common.common_no_icon,
                                                                                                                                  a =>
                                                                                                                                  {
                                                                                                                                      a.condition = immune_condition;
                                                                                                                                      a.durationType = RuleDefinitions.DurationType.Day;
                                                                                                                                      a.durationValue = 1;
                                                                                                                                      a.turnOccurence = RuleDefinitions.TurnOccurenceType.EndOfTurn;
                                                                                                                                      a.power = intimidating_presence;
                                                                                                                                      a.onlyOnSucessfulSave = true;
                                                                                                                                  }
                                                                                                                                  );
            intimidating_presence_feature = Helpers.FeatureSetBuilder.createFeatureSet("MonkSubclassBerserkerIntimidatingPresenceFeatureSet",
                                                                                       "",
                                                                                       intimidating_presence_title_string,
                                                                                       intimidating_presence_description_string,
                                                                                       false,
                                                                                       FeatureDefinitionFeatureSet.FeatureSetMode.Union,
                                                                                       false,
                                                                                       immune_application,
                                                                                       intimidating_presence
                                                                                       );
        }



        static CharacterSubclassDefinition createPathOfBerserker()
        {
            createFrenzy();
            createMindlessRage();
            createIntimidatingPresence();


            var gui_presentation = new GuiPresentationBuilder(
                    "Subclass/&MonkSubclassPrimalPathOfBerserkerDescription",
                    "Subclass/&MonkSubclassPrimalPathOfBerserkerTitle")
                    .SetSpriteReference(DatabaseHelper.CharacterSubclassDefinitions.RoguishDarkweaver.GuiPresentation.SpriteReference)
                    .Build();

            CharacterSubclassDefinition definition = new CharacterSubclassDefinitionBuilder("MonkSubclassPrimalPathOfBersrker", "4c6e8abe-1983-49b7-bc3b-2572865f6c17")
                    .SetGuiPresentation(gui_presentation)
                    .AddFeatureAtLevel(frenzy, 3)
                    .AddFeatureAtLevel(mindless_rage, 6)
                    .AddFeatureAtLevel(intimidating_presence_feature, 10)
                    .AddToDB();

            return definition;
        }


        public static void BuildAndAddClassToDB()
        {
            var MonkClass = new MonkClassBuilder(MonkClassName, MonkClassNameGuid).AddToDB();
            MonkClass.FeatureUnlocks.Sort(delegate (FeatureUnlockByLevel a, FeatureUnlockByLevel b)
                                          {
                                              return a.Level - b.Level;
                                          }
                                         );

            MonkFeatureDefinitionSubclassChoice.Subclasses.Add(createPathOfBerserker().Name);
            MonkFeatureDefinitionSubclassChoice.Subclasses.Add(createPathOfFrozenFury().Name);
            MonkFeatureDefinitionSubclassChoice.Subclasses.Add(createPathOfWarShaman().Name);
        }

        private static FeatureDefinitionSubclassChoice MonkFeatureDefinitionSubclassChoice;
    }
}
