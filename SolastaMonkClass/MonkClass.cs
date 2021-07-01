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
        static public ConditionDefinition flurry_of_blows_condition;
        static public NewFeatureDefinitions.PowerWithRestrictions flurry_of_blows;
        static public NewFeatureDefinitions.PowerWithRestrictions patient_defense;
        static public NewFeatureDefinitions.PowerWithRestrictions step_of_the_wind;
        static public FeatureDefinitionFeatureSet deflect_missiles;
        static public FeatureDefinitionAttributeModifier extra_attack;

        //way of the open hand
        static public FeatureDefinitionFeatureSet open_hand_technique;
        static public NewFeatureDefinitions.PowerWithRestrictions open_hand_technique_knock;
        static public NewFeatureDefinitions.PowerWithRestrictions open_hand_technique_push;
        static public NewFeatureDefinitions.PowerWithRestrictions open_hand_technique_forbid_reaction;

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
            createDeflectMissiles();
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
            Definition.FeatureUnlocks.Add(new FeatureUnlockByLevel(deflect_missiles, 3));
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
            subclassChoicesGuiPresentation.Title = "Subclass/&MonkSubclassMonasticTraditionTitle";
            subclassChoicesGuiPresentation.Description = "Subclass/&MonkSubclassMonasticTraditionDescription";
            MonkFeatureDefinitionSubclassChoice = this.BuildSubclassChoice(3, "MonasticTradition", false, "SubclassChoiceMonkSpecialistArchetypes", subclassChoicesGuiPresentation, MonkClassSubclassesGuid);
        }


        static void createDeflectMissiles()
        {
            string deflect_missiles_title_string = "Feature/&MonkClassDeflectMissilesTitle";
            string deflect_missiles_description_string = "Feature/&MonkClassDeflectMissilesDescription";

            var deflect_missiles_affinity = Helpers.FeatureBuilder<NewFeatureDefinitions.DeflectMissileCustom>.createFeature("MonkClassDeflectMissilesActionAffinity",
                                                                                                                          "",
                                                                                                                          deflect_missiles_title_string,
                                                                                                                          deflect_missiles_description_string,
                                                                                                                          null,
                                                                                                                          a =>
                                                                                                                          {
                                                                                                                              a.characterStat = Helpers.Stats.Dexterity;
                                                                                                                              a.characterClass = monk_class;
                                                                                                                          }
                                                                                                                          );

            deflect_missiles = Helpers.FeatureSetBuilder.createFeatureSet("MonkClassDeflectMissiles",
                                                                         "",
                                                                         deflect_missiles_title_string,
                                                                         deflect_missiles_description_string,
                                                                         false,
                                                                         FeatureDefinitionFeatureSet.FeatureSetMode.Union,
                                                                         false,
                                                                         deflect_missiles_affinity
                                                                         );
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

            flurry_of_blows_condition = Helpers.ConditionBuilder.createConditionWithInterruptions("MonkClassFlurryOfBlowsCondition",
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


        static void createOpenHandTechnique()
        {
            string open_hand_technique_title_string = "Feature/&MonkSubclassWayOfTheOpenHandTechniqueTitle";
            string open_hand_technique_description_string = "Feature/&MonkSubclassWayOfTheOpenHandTechniqueDescription";

            var open_hand_used_condition = Helpers.ConditionBuilder.createConditionWithInterruptions("MonkSubclassWayOfTheOpenHandUsedCondition",
                                                                                                    "",
                                                                                                    "",
                                                                                                    "",
                                                                                                    null,
                                                                                                    DatabaseHelper.ConditionDefinitions.ConditionDummy,
                                                                                                    new RuleDefinitions.ConditionInterruption[] {RuleDefinitions.ConditionInterruption.AttacksAndDamages }
                                                                                                    );
            NewFeatureDefinitions.ConditionsData.no_refresh_conditions.Add(open_hand_used_condition);
                                                                                                         
            createOpenHandTechniqueKnock();
            createOpenHandTechniquePush();
            createOpenHandTechniqueForbidReaction();

            var open_hand_used_feature = Helpers.FeatureBuilder<NewFeatureDefinitions.ApplyConditionOnPowerUseToSelf>.createFeature("MonkSubclassWayOfTheOpenHandUsedCondition",
                                                                                                                                      "",
                                                                                                                                      Common.common_no_title,
                                                                                                                                      Common.common_no_title,
                                                                                                                                      Common.common_no_icon,
                                                                                                                                      a =>
                                                                                                                                      {
                                                                                                                                          a.condition = open_hand_used_condition;
                                                                                                                                          a.durationType = RuleDefinitions.DurationType.Round;
                                                                                                                                          a.durationValue = 1;
                                                                                                                                          a.powers = new List<FeatureDefinitionPower> { open_hand_technique_knock, open_hand_technique_push, open_hand_technique_forbid_reaction };


                                                                                                                                      }
                                                                                                                                      );


            open_hand_technique = Helpers.FeatureSetBuilder.createFeatureSet("MonkSubclassWayOfTheOpenHandTechnique",
                                                                             "",
                                                                             open_hand_technique_title_string,
                                                                             open_hand_technique_description_string,
                                                                             false,
                                                                             FeatureDefinitionFeatureSet.FeatureSetMode.Union,
                                                                             false,
                                                                             open_hand_technique_knock,
                                                                             open_hand_technique_push,
                                                                             open_hand_technique_forbid_reaction,
                                                                             open_hand_used_feature
                                                                             );
            open_hand_technique_knock.restrictions.Add(new NewFeatureDefinitions.NoConditionRestriction(open_hand_used_condition));
            open_hand_technique_push.restrictions.Add(new NewFeatureDefinitions.NoConditionRestriction(open_hand_used_condition));
            open_hand_technique_forbid_reaction.restrictions.Add(new NewFeatureDefinitions.NoConditionRestriction(open_hand_used_condition));
        }


        static void createOpenHandTechniqueForbidReaction()
        {
            string open_hand_forbid_reaction_title_string = "Feature/&MonkSubclassWayOfTheOpenHandForbidReactionTitle";
            string open_hand_forbid_reaction_description_string = "Feature/&MonkSubclassWayOfTheOpenHandForbidReactionDescription";
            string use_open_hand_forbid_reaction_react_description = "Reaction/&SpendMonkSubclassWayOfTheOpenHandForbidReactionPowerReactDescription";
            string use_open_hand_forbid_reaction_react_title = "Reaction/&CommonUsePowerReactTitle";

            string open_hand_forbid_reaction_condition_title_string = "Rules/&ConditionMonkSubclassWayOfTheOpenHandForbidReactionPowerTitle";
            string open_hand_forbid_reaction_condition_description_string = "Rules/&ConditionMonkSubclassWayOfTheOpenHandForbidReactionPowerDescription";

            var condition = Helpers.ConditionBuilder.createCondition("MonkSubclassWayOfTheOpenHandForbidReactionCondition",
                                                                    "",
                                                                    open_hand_forbid_reaction_condition_title_string,
                                                                    open_hand_forbid_reaction_condition_description_string,
                                                                    null,
                                                                    DatabaseHelper.ConditionDefinitions.ConditionDazzled,
                                                                    DatabaseHelper.FeatureDefinitionActionAffinitys.ActionAffinityConditionDazzled
                                                                    );

            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainBattleDecisiveStrike.EffectDescription);
            effect.DurationParameter = 1;
            effect.DurationType = RuleDefinitions.DurationType.Round;
            effect.SetSavingThrowDifficultyAbility(Helpers.Stats.Wisdom);
            effect.SavingThrowAbility = Helpers.Stats.Strength;
            effect.hasSavingThrow = false;
            effect.EffectForms.Clear();

            var effect_form = new EffectForm();
            effect_form.ConditionForm = new ConditionForm();
            effect_form.FormType = EffectForm.EffectFormType.Condition;
            effect_form.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
            effect_form.ConditionForm.ConditionDefinition = condition;
            effect.EffectForms.Add(effect_form);

            var power = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                        .createPower("MonkSubclassWayOfTheOpenHandForbidReaction",
                                                                     "",
                                                                     open_hand_forbid_reaction_title_string,
                                                                     open_hand_forbid_reaction_description_string,
                                                                     flurry_of_blows.GuiPresentation.SpriteReference,
                                                                     effect,
                                                                     RuleDefinitions.ActivationTime.OnAttackHit,
                                                                     2,
                                                                     RuleDefinitions.UsesDetermination.Fixed,
                                                                     RuleDefinitions.RechargeRate.AtWill
                                                                     );
            power.restrictions = new List<NewFeatureDefinitions.IRestriction>()
                                            {
                                                new NewFeatureDefinitions.HasAtLeastOneConditionFromListRestriction(flurry_of_blows_condition)
                                            };
            power.checkReaction = true;

            open_hand_technique_forbid_reaction = power;
            ki.powers.Add(open_hand_technique_forbid_reaction);

            Helpers.StringProcessing.addPowerReactStrings(open_hand_technique_forbid_reaction, open_hand_forbid_reaction_title_string, use_open_hand_forbid_reaction_react_description,
                                                                    use_open_hand_forbid_reaction_react_title, use_open_hand_forbid_reaction_react_description, "SpendPower");
        }


        static void createOpenHandTechniquePush()
        {
            string open_hand_push_title_string = "Feature/&MonkSubclassWayOfTheOpenHandPushTitle";
            string open_hand_push_description_string = "Feature/&MonkSubclassWayOfTheOpenHandPushDescription";
            string use_open_hand_push_react_description = "Reaction/&SpendMonkSubclassWayOfTheOpenHandPushPowerReactDescription";
            string use_open_hand_push_react_title = "Reaction/&CommonUsePowerReactTitle";

            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainBattleDecisiveStrike.EffectDescription);
            effect.DurationParameter = 1;
            effect.DurationType = RuleDefinitions.DurationType.Instantaneous;
            effect.SetSavingThrowDifficultyAbility(Helpers.Stats.Wisdom);
            effect.SavingThrowAbility = Helpers.Stats.Strength;
            effect.hasSavingThrow = true;
            effect.SetDifficultyClassComputation(RuleDefinitions.EffectDifficultyClassComputation.AbilityScoreAndProficiency);
            effect.EffectForms.Clear();

            var effect_form = new EffectForm();
            effect_form.motionForm = new MotionForm();
            effect_form.FormType = EffectForm.EffectFormType.Motion;
            effect_form.motionForm.type = MotionForm.MotionType.PushFromOrigin;
            effect_form.motionForm.distance = 3;
            effect.EffectForms.Add(effect_form);

            var power = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                        .createPower("MonkSubclassWayOfTheOpenHandPush",
                                                                     "",
                                                                     open_hand_push_title_string,
                                                                     open_hand_push_description_string,
                                                                     flurry_of_blows.GuiPresentation.SpriteReference,
                                                                     effect,
                                                                     RuleDefinitions.ActivationTime.OnAttackHit,
                                                                     2,
                                                                     RuleDefinitions.UsesDetermination.Fixed,
                                                                     RuleDefinitions.RechargeRate.AtWill
                                                                     );
            power.restrictions = new List<NewFeatureDefinitions.IRestriction>()
                                            {
                                                new NewFeatureDefinitions.HasAtLeastOneConditionFromListRestriction(flurry_of_blows_condition)
                                            };
            power.checkReaction = true;

            open_hand_technique_push = power;

            ki.powers.Add(open_hand_technique_push);
            Helpers.StringProcessing.addPowerReactStrings(open_hand_technique_push, open_hand_push_title_string, use_open_hand_push_react_description,
                                                        use_open_hand_push_react_title, use_open_hand_push_react_description, "SpendPower");
        }


        static void createOpenHandTechniqueKnock()
        {
            string open_hand_knock_title_string = "Feature/&MonkSubclassWayOfTheOpenHandKnockTitle";
            string open_hand_knock_description_string = "Feature/&MonkSubclassWayOfTheOpenHandKnockDescription";
            string use_open_hand_knock_react_description = "Reaction/&SpendMonkSubclassWayOfTheOpenHandKnockPowerReactDescription";
            string use_open_hand_knock_react_title = "Reaction/&CommonUsePowerReactTitle";

            var effect = new EffectDescription();
            effect.Copy(DatabaseHelper.FeatureDefinitionPowers.PowerDomainBattleDecisiveStrike.EffectDescription);
            effect.DurationParameter = 1;
            effect.DurationType = RuleDefinitions.DurationType.Instantaneous;
            effect.SetSavingThrowDifficultyAbility(Helpers.Stats.Wisdom);
            effect.SavingThrowAbility = Helpers.Stats.Dexterity;
            effect.SetDifficultyClassComputation(RuleDefinitions.EffectDifficultyClassComputation.AbilityScoreAndProficiency);
            effect.EffectForms.Clear();

            var effect_form = new EffectForm();
            effect_form.motionForm = new MotionForm();
            effect_form.FormType = EffectForm.EffectFormType.Motion;
            effect_form.motionForm.type = MotionForm.MotionType.FallProne;
            effect_form.motionForm.distance = 3;
            effect.EffectForms.Add(effect_form);

            var power = Helpers.GenericPowerBuilder<NewFeatureDefinitions.PowerWithRestrictions>
                                                        .createPower("MonkSubclassWayOfTheOpenHandKnock",
                                                                     "",
                                                                     open_hand_knock_title_string,
                                                                     open_hand_knock_description_string,
                                                                     flurry_of_blows.GuiPresentation.SpriteReference,
                                                                     effect,
                                                                     RuleDefinitions.ActivationTime.OnAttackHit,
                                                                     2,
                                                                     RuleDefinitions.UsesDetermination.Fixed,
                                                                     RuleDefinitions.RechargeRate.AtWill
                                                                     );
            power.restrictions = new List<NewFeatureDefinitions.IRestriction>()
                                            {
                                                new NewFeatureDefinitions.HasAtLeastOneConditionFromListRestriction(flurry_of_blows_condition)
                                            };
            power.checkReaction = true;

            open_hand_technique_knock = power;

            ki.powers.Add(open_hand_technique_knock);
            Helpers.StringProcessing.addPowerReactStrings(open_hand_technique_knock, open_hand_knock_title_string, use_open_hand_knock_react_description,
                                            use_open_hand_knock_react_title, use_open_hand_knock_react_description, "SpendPower");
        }


        static CharacterSubclassDefinition createWayOfTheOpenHand()
        {
            createOpenHandTechnique();

            var gui_presentation = new GuiPresentationBuilder(
                    "Subclass/&MonkSubclassWayOfTheOpenHandDescription",
                    "Subclass/&MonkSubclassWayOfTheOpenHandTitle")
                    .SetSpriteReference(DatabaseHelper.CharacterSubclassDefinitions.DomainLife.GuiPresentation.SpriteReference)
                    .Build();

            CharacterSubclassDefinition definition = new CharacterSubclassDefinitionBuilder("MonkSubclassWayOfTheOpenHand", "4c6e8abe-1983-49b7-bc3b-2572865f6c17")
                    .SetGuiPresentation(gui_presentation)
                    .AddFeatureAtLevel(open_hand_technique, 3)
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

            MonkFeatureDefinitionSubclassChoice.Subclasses.Add(createWayOfTheOpenHand().Name);
        }

        private static FeatureDefinitionSubclassChoice MonkFeatureDefinitionSubclassChoice;
    }
}
