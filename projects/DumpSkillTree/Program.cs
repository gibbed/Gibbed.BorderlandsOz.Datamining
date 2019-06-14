/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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

namespace DumpSkillTree
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new BorderlandsOzDatamining.Dataminer().Run(args, Go);
        }

        private static void Go(Engine engine)
        {
            var skillTreeDefinitionClass = engine.GetClass("WillowGame.SkillTreeDefinition");
            if (skillTreeDefinitionClass == null)
            {
                throw new InvalidOperationException();
            }

            var skillTreeDefinitions = engine.Objects
                                             .Where(o => o.IsA(skillTreeDefinitionClass) &&
                                                         o.GetName().StartsWith("Default__") ==
                                                         false)
                                             .OrderBy(o => o.GetPath());

            foreach (dynamic skillTreeDefinition in skillTreeDefinitions)
            {
                using (var output = BorderlandsOzDatamining.Dataminer.NewDump("Skill Tree [" + skillTreeDefinition.GetPath() + "].json"))
                using (var writer = new JsonTextWriter(output))
                {
                    writer.Indentation = 2;
                    writer.IndentChar = ' ';
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartObject();

                    writer.WritePropertyName(skillTreeDefinition.GetPath());
                    writer.WriteStartObject();

                    writer.WritePropertyName("root");
                    DumpBranch(writer, skillTreeDefinition.Root);

                    writer.WriteEndObject();

                    writer.WriteEndObject();
                    writer.Flush();
                }
            }
        }

        private static void DumpBranch(JsonTextWriter writer, dynamic root)
        {
            writer.WriteStartObject();

            if (string.IsNullOrEmpty(root.BranchName) == false)
            {
                writer.WritePropertyName("name");
                writer.WriteValue(root.BranchName);
            }

            if (root.Children != null)
            {
                var children = root.Children;
                if (children.Length > 0)
                {
                    writer.WritePropertyName("children");
                    writer.WriteStartArray();
                    foreach (var child in children)
                    {
                        DumpBranch(writer, child);
                    }
                    writer.WriteEndArray();
                }
            }

            writer.WritePropertyName("tiers");
            writer.WriteStartArray();
            foreach (var tier in root.Tiers)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("skills");
                writer.WriteStartArray();
                foreach (var skill in tier.Skills)
                {
                    writer.WriteValue(skill.GetPath());
                }
                writer.WriteEndArray();

                writer.WritePropertyName("points_to_unlock_next_tier");
                writer.WriteValue(tier.PointsToUnlockNextTier);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            if (root.Layout != null)
            {
                writer.WritePropertyName("layout");
                writer.WriteStartObject();

                if (root.Layout.Tiers != null)
                {
                    writer.WritePropertyName("tiers");
                    writer.WriteStartArray();
                    foreach (var tier in root.Layout.Tiers)
                    {
                        writer.WriteStartArray();
                        foreach (var cellIsOccupied in tier.bCellIsOccupied)
                        {
                            writer.WriteValue(cellIsOccupied);
                        }
                        writer.WriteEndArray();
                    }
                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }
    }
}
