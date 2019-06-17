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
using System.Linq;
using Gibbed.Unreflect.Core;
using Dataminer = BorderlandsOzDatamining.Dataminer;

namespace DumpDLC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Dataminer().Run(args, Go);
        }

        private static void Go(Engine engine)
        {
            var managerClass = engine.GetClass("WillowGame.WillowDownloadableContentManager");
            var packageClass = engine.GetClass("WillowGame.DownloadablePackageDefinition");
            if (managerClass == null || packageClass == null)
            {
                throw new InvalidOperationException();
            }

            var managers = engine.Objects
                .Where(o => o.IsA(managerClass) &&
                            o.GetName().StartsWith("Default__") == false)
                .OrderBy(o => o.GetPath())
                .ToArray();
            if (managers.Length != 1)
            {
                throw new InvalidOperationException();
            }
            dynamic manager = managers.First();

            using (var writer = Dataminer.NewDump("Downloadable Contents.json"))
            {
                writer.WriteStartObject();

                var allContent = manager.AllContent;

                foreach (var content in ((IEnumerable<dynamic>)allContent)
                    .OrderBy(c => c.PackageDef.PackageId)
                    .ThenBy(c => c.ContentId))
                {
                    writer.WritePropertyName(content.GetPath());
                    writer.WriteStartObject();

                    UnrealClass uclass = content.GetClass();
                    if (uclass.Path != "WillowGame.DownloadableExpansionDefinition" &&
                        uclass.Path != "WillowGame.DownloadableCustomizationSetDefinition" &&
                        uclass.Path != "WillowGame.DownloadableItemSetDefinition" &&
                        uclass.Path != "WillowGame.DownloadableVehicleDefinition" &&
                        uclass.Path != "WillowGame.DownloadableCharacterDefinition" &&
                        uclass.Path != "WillowGame.DownloadableBalanceModifierDefinition")
                    {
                        throw new NotSupportedException();
                    }

                    writer.WritePropertyName("id");
                    writer.WriteValue(content.ContentId);

                    writer.WritePropertyName("name");
                    writer.WriteValue(content.ContentDisplayName);

                    if (content.PackageDef == null)
                    {
                        throw new InvalidOperationException();
                    }

                    writer.WritePropertyName("package");
                    writer.WriteValue(content.PackageDef.GetPath());

                    writer.WritePropertyName("type");
                    writer.WriteValue(_ContentTypeMapping[uclass.Path]);

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.Flush();
            }

            using (var writer = Dataminer.NewDump("Downloadable Packages.json"))
            {
                writer.WriteStartObject();

                var packages = engine.Objects
                    .Where(o => o.IsA(packageClass) &&
                                o.GetName().StartsWith("Default__") == false)
                    .Cast<dynamic>()
                    .OrderBy(o => o.PackageId);
                foreach (dynamic package in packages)
                {
                    writer.WritePropertyName(package.GetPath());
                    writer.WriteStartObject();

                    writer.WritePropertyName("id");
                    writer.WriteValue(package.PackageId);

                    writer.WritePropertyName("dlc_name");
                    writer.WriteValue(package.DLCName);

                    writer.WritePropertyName("display_name");
                    writer.WriteValue(package.PackageDisplayName);

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.Flush();
            }
        }

        private static readonly Dictionary<string, string> _ContentTypeMapping = new Dictionary<string, string>()
        {
            { "WillowGame.DownloadableBalanceModifierDefinition", "BalanceModifier" },
            { "WillowGame.DownloadableCharacterDefinition", "Character" },
            { "WillowGame.DownloadableCustomizationSetDefinition", "CustomizationSet" },
            { "WillowGame.DownloadableExpansionDefinition", "Expansion" },
            { "WillowGame.DownloadableItemSetDefinition", "ItemSet" },
            { "WillowGame.DownloadableVehicleDefinition", "Vehicle" },
        };
    }
}
