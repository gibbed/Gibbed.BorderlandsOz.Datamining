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
using Dataminer = BorderlandsOzDatamining.Dataminer;

namespace DumpSkills
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Dataminer().Run(args, Go);
        }

        private static void Go(Engine engine)
        {
            var skillDefinitionClass = engine.GetClass("WillowGame.SkillDefinition");
            if (skillDefinitionClass == null)
            {
                throw new InvalidOperationException();
            }

            var skillDefinitions = engine.Objects
                .Where(o => o.IsA(skillDefinitionClass) &&
                            o.GetName().StartsWith("Default__") == false)
                .OrderBy(o => o.GetPath());
            using (var writer = Dataminer.NewDump("Skills.json"))
            {
                writer.WriteStartObject();

                foreach (dynamic skillDefinition in skillDefinitions)
                {
                    writer.WritePropertyName(skillDefinition.GetPath());
                    writer.WriteStartObject();

                    if (string.IsNullOrEmpty(skillDefinition.SkillName) == false)
                    {
                        writer.WritePropertyName("name");
                        writer.WriteValue(skillDefinition.SkillName);
                    }

                    if (string.IsNullOrEmpty(skillDefinition.SkillDescription) == false)
                    {
                        writer.WritePropertyName("description");
                        writer.WriteValue(skillDefinition.SkillDescription);
                    }

                    /*
                    if (skillDefinition.SkillClass != null)
                    {
                        writer.WritePropertyName("class");
                        writer.WriteValue(skillDefinition.SkillClass.Path);
                    }
                    */

                    writer.WritePropertyName("default_starting_grade");
                    writer.WriteValue(skillDefinition.DefaultStartingGrade);

                    writer.WritePropertyName("max_grade");
                    writer.WriteValue(skillDefinition.MaxGrade);

                    writer.WritePropertyName("player_level_requirement");
                    writer.WriteValue(skillDefinition.PlayerLevelRequirement);

                    if (string.IsNullOrEmpty(skillDefinition.SkillIconTextureName) == false)
                    {
                        writer.WritePropertyName("icon_texture_name");
                        writer.WriteValue(skillDefinition.SkillIconTextureName);
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.Flush();
            }
        }
    }
}
