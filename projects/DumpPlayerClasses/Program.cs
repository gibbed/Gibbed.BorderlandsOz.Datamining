/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.Unreflect.Core;
using Newtonsoft.Json;

namespace DumpPlayerClasses
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new BorderlandsOzDatamining.Dataminer().Run(args, Go);
        }

        private static void Go(Engine engine)
        {
            var playerClassDefinitionClass = engine.GetClass("WillowGame.PlayerClassDefinition");
            if (playerClassDefinitionClass == null)
            {
                throw new InvalidOperationException();
            }

            var playerClassIdentifierDefinitionClass = engine.GetClass("WillowGame.PlayerClassIdentifierDefinition");
            if (playerClassIdentifierDefinitionClass == null)
            {
                throw new InvalidOperationException();
            }

            using (var output = BorderlandsOzDatamining.Dataminer.NewDump("Player Classes.json"))
            using (var writer = new JsonTextWriter(output))
            {
                writer.Indentation = 2;
                writer.IndentChar = ' ';
                writer.Formatting = Formatting.Indented;

                writer.WriteStartObject();

                var playerClassDefinitionClasses = engine.Objects
                                                         .Where(o => o.IsA(playerClassDefinitionClass) &&
                                                                     o.GetName().StartsWith("Default__") ==
                                                                     false)
                                                         .OrderBy(o => o.GetPath());
                foreach (dynamic playerClassDefinition in playerClassDefinitionClasses)
                {
                    writer.WritePropertyName(playerClassDefinition.GetPath());
                    writer.WriteStartObject();

                    var characterNameId = playerClassDefinition.CharacterNameId;
                    if (characterNameId == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var characterClassId = characterNameId.CharacterClassId;
                    if (characterClassId == null)
                    {
                        throw new InvalidOperationException();
                    }

                    writer.WritePropertyName("name");
                    writer.WriteValue(characterNameId.LocalizedCharacterName);

                    writer.WritePropertyName("class");
                    writer.WriteValue(characterClassId.LocalizedClassNameNonCaps);

                    writer.WritePropertyName("sort_order");
                    writer.WriteValue(characterNameId.UISortOrder);

                    if (characterClassId.DlcCharacterDef != null)
                    {
                        writer.WritePropertyName("dlc");
                        writer.WriteValue(characterClassId.DlcCharacterDef.GetPath());
                    }

                    DumpDefaultSaveGame(writer, playerClassDefinition.CharacterNameId.DefaultSaveGame);

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.Flush();
            }
        }

        private static void DumpDefaultSaveGame(JsonTextWriter writer, dynamic defaultSaveGame)
        {
            writer.WritePropertyName("default_save");
            writer.WriteStartObject();

            writer.WritePropertyName("ui_preferences");
            writer.WriteStartObject();

            writer.WritePropertyName("character_name");
            writer.WriteValue((string)defaultSaveGame.UIPreferences.CharacterName);

            writer.WriteEndObject();

            writer.WritePropertyName("exp_level");
            writer.WriteValue(defaultSaveGame.ExpLevel);

            writer.WritePropertyName("exp_points");
            writer.WriteValue(defaultSaveGame.ExpPoints);

            writer.WritePropertyName("general_skill_points");
            writer.WriteValue(defaultSaveGame.GeneralSkillPoints);

            writer.WritePropertyName("specialist_skill_points");
            writer.WriteValue(defaultSaveGame.SpecialistSkillPoints);

            writer.WritePropertyName("currency_on_hand");
            writer.WriteStartArray();
            foreach (var currencyOnHand in defaultSaveGame.CurrencyOnHand)
            {
                writer.WriteValue(currencyOnHand);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("skill_data");
            writer.WriteStartArray();
            foreach (var skillData in defaultSaveGame.SkillData)
            {
                writer.WriteStartObject();

                if (skillData.SkillDefinition != null)
                {
                    writer.WritePropertyName("skill");
                    writer.WriteValue(skillData.SkillDefinition.GetPath());
                }

                writer.WritePropertyName("amount");
                writer.WriteValue((int)skillData.Grade);

                writer.WritePropertyName("grade_points");
                writer.WriteValue((int)skillData.GradePoints);

                writer.WritePropertyName("equipped_slot_index");
                writer.WriteValue((int)skillData.EquippedSlotIndex);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("resource_data");
            writer.WriteStartArray();
            foreach (var resourceData in defaultSaveGame.ResourceData)
            {
                writer.WriteStartObject();

                if (resourceData.ResourceDefinition != null)
                {
                    writer.WritePropertyName("resource");
                    writer.WriteValue(resourceData.ResourceDefinition.GetPath());
                }

                if (resourceData.ResourcePoolDefinition != null)
                {
                    writer.WritePropertyName("pool");
                    writer.WriteValue(resourceData.ResourcePoolDefinition.GetPath());
                }

                writer.WritePropertyName("amount");
                writer.WriteValue((float)resourceData.Amount);

                writer.WritePropertyName("upgrade_level");
                writer.WriteValue((int)resourceData.UpgradeLevel);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("item_data");
            writer.WriteStartArray();
            foreach (var itemData in defaultSaveGame.ItemData)
            {
                writer.WriteStartObject();

                var definitionData = itemData.DefinitionData;

                writer.WritePropertyName("data");
                writer.WriteStartObject();

                if (definitionData.ItemDefinition != null)
                {
                    writer.WritePropertyName("type");
                    writer.WriteValue(definitionData.ItemDefinition.GetPath());
                }

                if (definitionData.BalanceDefinition != null)
                {
                    writer.WritePropertyName("balance");
                    writer.WriteValue(definitionData.BalanceDefinition.GetPath());
                }

                if (definitionData.ManufacturerDefinition != null)
                {
                    writer.WritePropertyName("manufacturer");
                    writer.WriteValue(definitionData.ManufacturerDefinition.GetPath());
                }

                writer.WritePropertyName("manufacturer_grade_index");
                writer.WriteValue((int)definitionData.ManufacturerGradeIndex);

                if (definitionData.AlphaItemPartDefinition != null)
                {
                    writer.WritePropertyName("alpha_part");
                    writer.WriteValue(definitionData.AlphaItemPartDefinition.GetPath());
                }

                if (definitionData.BetaItemPartDefinition != null)
                {
                    writer.WritePropertyName("beta_part");
                    writer.WriteValue(definitionData.BetaItemPartDefinition.GetPath());
                }

                if (definitionData.GammaItemPartDefinition != null)
                {
                    writer.WritePropertyName("gamma_part");
                    writer.WriteValue(definitionData.GammaItemPartDefinition.GetPath());
                }

                if (definitionData.DeltaItemPartDefinition != null)
                {
                    writer.WritePropertyName("delta_part");
                    writer.WriteValue(definitionData.DeltaItemPartDefinition.GetPath());
                }

                if (definitionData.EpsilonItemPartDefinition != null)
                {
                    writer.WritePropertyName("epsilon_part");
                    writer.WriteValue(definitionData.EpsilonItemPartDefinition.GetPath());
                }

                if (definitionData.ZetaItemPartDefinition != null)
                {
                    writer.WritePropertyName("zeta_part");
                    writer.WriteValue(definitionData.ZetaItemPartDefinition.GetPath());
                }

                if (definitionData.EtaItemPartDefinition != null)
                {
                    writer.WritePropertyName("eta_part");
                    writer.WriteValue(definitionData.EtaItemPartDefinition.GetPath());
                }

                if (definitionData.ThetaItemPartDefinition != null)
                {
                    writer.WritePropertyName("theta_part");
                    writer.WriteValue(definitionData.ThetaItemPartDefinition.GetPath());
                }

                if (definitionData.MaterialItemPartDefinition != null)
                {
                    writer.WritePropertyName("material_part");
                    writer.WriteValue(definitionData.MaterialItemPartDefinition.GetPath());
                }

                if (definitionData.PrefixItemNamePartDefinition != null)
                {
                    writer.WritePropertyName("prefix_part");
                    writer.WriteValue(definitionData.PrefixItemNamePartDefinition.GetPath());
                }

                if (definitionData.TitleItemNamePartDefinition != null)
                {
                    writer.WritePropertyName("title_part");
                    writer.WriteValue(definitionData.TitleItemNamePartDefinition.GetPath());
                }

                writer.WritePropertyName("game_stage");
                writer.WriteValue((int)definitionData.GameStage);

                writer.WritePropertyName("unique_id");
                writer.WriteValue((int)definitionData.UniqueId);

                writer.WriteEndObject();

                writer.WritePropertyName("quantity");
                writer.WriteValue((int)itemData.Quantity);

                writer.WritePropertyName("equipped");
                writer.WriteValue((bool)itemData.bEquipped);

                writer.WritePropertyName("mark");
                writer.WriteValue((PlayerMark)((int)itemData.Mark));

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("weapon_data");
            writer.WriteStartArray();
            foreach (var weaponData in defaultSaveGame.WeaponData)
            {
                writer.WriteStartObject();

                var definitionData = weaponData.WeaponDefinitionData;

                writer.WritePropertyName("data");
                writer.WriteStartObject();

                if (definitionData.WeaponTypeDefinition != null)
                {
                    writer.WritePropertyName("type");
                    writer.WriteValue(definitionData.WeaponTypeDefinition.GetPath());
                }

                if (definitionData.BalanceDefinition != null)
                {
                    writer.WritePropertyName("balance");
                    writer.WriteValue(definitionData.BalanceDefinition.GetPath());
                }

                if (definitionData.ManufacturerDefinition != null)
                {
                    writer.WritePropertyName("manufacturer");
                    writer.WriteValue(definitionData.ManufacturerDefinition.GetPath());
                }

                writer.WritePropertyName("manufacturer_grade_index");
                writer.WriteValue((int)definitionData.ManufacturerGradeIndex);

                if (definitionData.BodyPartDefinition != null)
                {
                    writer.WritePropertyName("body_part");
                    writer.WriteValue(definitionData.BodyPartDefinition.GetPath());
                }

                if (definitionData.GripPartDefinition != null)
                {
                    writer.WritePropertyName("grip_part");
                    writer.WriteValue(definitionData.GripPartDefinition.GetPath());
                }

                if (definitionData.BarrelPartDefinition != null)
                {
                    writer.WritePropertyName("barrel_part");
                    writer.WriteValue(definitionData.BarrelPartDefinition.GetPath());
                }

                if (definitionData.SightPartDefinition != null)
                {
                    writer.WritePropertyName("sight_part");
                    writer.WriteValue(definitionData.SightPartDefinition.GetPath());
                }

                if (definitionData.StockPartDefinition != null)
                {
                    writer.WritePropertyName("stock_part");
                    writer.WriteValue(definitionData.StockPartDefinition.GetPath());
                }

                if (definitionData.ElementalPartDefinition != null)
                {
                    writer.WritePropertyName("elemental_part");
                    writer.WriteValue(definitionData.ElementalPartDefinition.GetPath());
                }

                if (definitionData.Accessory1PartDefinition != null)
                {
                    writer.WritePropertyName("accessory_1_part");
                    writer.WriteValue(definitionData.Accessory1PartDefinition.GetPath());
                }

                if (definitionData.Accessory2PartDefinition != null)
                {
                    writer.WritePropertyName("accessory_2_part");
                    writer.WriteValue(definitionData.Accessory2PartDefinition.GetPath());
                }

                if (definitionData.MaterialPartDefinition != null)
                {
                    writer.WritePropertyName("material_part");
                    writer.WriteValue(definitionData.MaterialPartDefinition.GetPath());
                }

                if (definitionData.PrefixPartDefinition != null)
                {
                    writer.WritePropertyName("prefix_part");
                    writer.WriteValue(definitionData.PrefixPartDefinition.GetPath());
                }

                if (definitionData.TitlePartDefinition != null)
                {
                    writer.WritePropertyName("title_part");
                    writer.WriteValue(definitionData.TitlePartDefinition.GetPath());
                }

                writer.WritePropertyName("game_stage");
                writer.WriteValue((int)definitionData.GameStage);

                writer.WritePropertyName("unique_id");
                writer.WriteValue((int)definitionData.UniqueId);

                writer.WriteEndObject();

                writer.WritePropertyName("quick_slot");
                writer.WriteValue((QuickWeaponSlot)((int)weaponData.QuickSlot));

                writer.WritePropertyName("mark");
                writer.WriteValue((PlayerMark)((int)weaponData.Mark));

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName("applied_customizations");
            writer.WriteStartArray();
            foreach (var appliedCustomization in defaultSaveGame.AppliedCustomizations)
            {
                if (appliedCustomization == null)
                {
                    writer.WriteValue("");
                }
                else
                {
                    writer.WriteValue(appliedCustomization.GetPath());
                }
            }
            writer.WriteEndArray();

            writer.WritePropertyName("player_flags");
            writer.WriteStartArray();
            foreach (var playerFlag in (IEnumerable<int>)defaultSaveGame.PlayerFlags)
            {
                writer.WriteValue(playerFlag);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("vehicle_steering_mode");
            writer.WriteValue((int)defaultSaveGame.VehicleSteeringMode);

            writer.WriteEndObject();
        }
    }
}
