﻿/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.Unreflect.Core;
using Gibbed.Unreflect.Runtime;
using Newtonsoft.Json;

namespace BorderlandsOzDatamining
{
    public class Dataminer
    {
        public static JsonTextWriter NewDump(params string[] paths)
        {
            Directory.CreateDirectory("dumps");
            var fullPaths = new List<string>();
            fullPaths.Add("dumps");
            fullPaths.AddRange(paths);
            var writer = new StreamWriter(Path.Combine(fullPaths.ToArray()), false, Encoding.UTF8);
            var jsonWriter = new JsonTextWriter(writer);
            jsonWriter.Indentation = 2;
            jsonWriter.IndentChar = ' ';
            jsonWriter.Formatting = Formatting.Indented;
            return jsonWriter;
        }

        private Process FindSuitableProcess(out string config)
        {
            var processes = Process.GetProcessesByName("borderlandspresequel");
            if (processes.Length == 0)
            {
                config = null;
                return null;
            }

            foreach (var process in processes)
            {
                var fvi = process.MainModule.FileVersionInfo;
                /* FileVersionInfo seems to be bugged and ProductPrivatePart
                 * doesn't actually contain the right value, so we'll extract it
                 * from ProductVersion. */
                var build = fvi.ProductVersion.Split('.').Last();
                config = Configurations.ResourceManager.GetString(build);
                if (config != null)
                {
                    return process;
                }
            }

            config = null;
            return null;
        }

        public void Run(string[] args, Action<Engine> callback)
        {
            string configText;
            var process = this.FindSuitableProcess(out configText);
            if (process == null)
            {
                Console.WriteLine("Failed to find a suitable running Borderlands: The Pre-Sequel process.");
                return;
            }

            var config = Configuration.Deserialize(configText);
            config.AdjustAddresses(process.MainModule);

            using (var runtime = new RuntimeProcess())
            {
                if (runtime.OpenProcess(process) == false)
                {
                    Console.WriteLine("Failed to open process {0}.", process.Id);
                    return;
                }

                runtime.SuspendThreads();

                try
                {
                    Console.WriteLine("Creating engine...");
                    var engine = new Engine(config, runtime);

                    Console.WriteLine("Datamining...");
                    callback(engine);
                }
                finally
                {
                    runtime.ResumeThreads();
                    runtime.CloseProcess();
                }
            }
        }
    }
}
