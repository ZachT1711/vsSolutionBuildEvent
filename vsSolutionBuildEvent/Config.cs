﻿/* 
 * Boost Software License - Version 1.0 - August 17th, 2003
 * 
 * Copyright (c) 2013-2014 Developed by reg [Denis Kuzmin] <entry.reg@gmail.com>
 * 
 * Permission is hereby granted, free of charge, to any person or organization
 * obtaining a copy of the software and accompanying documentation covered by
 * this license (the "Software") to use, reproduce, display, distribute,
 * execute, and transmit the Software, and to prepare derivative works of the
 * Software, and to permit third-parties to whom the Software is furnished to
 * do so, all subject to the following:
 * 
 * The copyright notices in the Software and this entire statement, including
 * the above license grant, this restriction and the following disclaimer,
 * must be included in all copies of the Software, in whole or in part, and
 * all derivative works of the Software, unless such copies or derivative
 * works are solely in the form of machine-executable object code generated by
 * a source language processor.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
 * SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
 * FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE. 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using net.r_eg.vsSBE.Events;
using net.r_eg.vsSBE.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace net.r_eg.vsSBE
{
    /// <summary>
    /// hooking up notifications
    /// </summary>
    internal delegate void ConfigEventHandler();

    internal class Config
    {
        /// <summary>
        /// Event after updates SBE-data
        /// </summary>
        public event ConfigEventHandler Update = delegate { };

        public struct Entity
        {
            /// <summary>
            /// Current config version
            /// Notice: version of app is controlled by Package
            /// </summary>
            public static readonly System.Version Version = new System.Version(0, 9);

            /// <summary>
            /// To file system
            /// </summary>
            public const string NAME = ".vssbe";
        }

        /// <summary>
        /// SBE data at runtime
        /// </summary>
        public SolutionEvents Data
        {
            get { return data; }
        }
        protected SolutionEvents data = null;
        
        /// <summary>
        /// Thread-safe getting the instance of Config class
        /// </summary>
        public static Config _
        {
            get { return _lazy.Value; }
        }
        private static readonly Lazy<Config> _lazy = new Lazy<Config>(() => new Config());

        /// <summary>
        /// identification with full path
        /// </summary>
        private string _Link
        {
            get { return Settings.WorkingPath + Entity.NAME; }
        }

        /// <summary>
        /// Initializing settings from file
        /// </summary>
        /// <param name="path">path to configuration file</param>
        public void load(string path)
        {
            Settings.setWorkingPath(path);
            _xprojvsbeUpgrade();

            try
            {
                using(StreamReader stream = new StreamReader(_Link, Encoding.UTF8, true))
                {
                    data = deserialize(stream);
                    if(data == null) {
                        throw new SBEException("empty or incorrect deserialized");
                    }
                    compatibility(stream);
                }
                Log.nlog.Info("Loaded settings (v{0}): '{1}'\n\nReady:", data.Header.Compatibility, Settings.WorkingPath);
                Update();
            }
            catch(FileNotFoundException)
            {
                Log.nlog.Info("Initialize with new settings");
            }
            catch(Exception ex)
            {
                data = new SolutionEvents();
                Log.nlog.Fatal("Configuration file is corrupt - '{0}'", ex.Message);
                //TODO: provide actions with UI, e.g.: restore, new..
            }

            // now compatibility should be updated to the latest
            data.Header.Compatibility = Entity.Version.ToString();
        }

        /// <summary>
        /// Initializing settings from object
        /// </summary>
        /// <param name="data"></param>
        public void load(SolutionEvents data)
        {
            this.data = data;
        }

        /// <summary>
        /// with changing path
        /// </summary>
        /// <param name="path">path to configuration file</param>
        public void save(string path)
        {
            Settings.setWorkingPath(path);
            save();
        }

        public void save()
        {
            try {
                using(TextWriter stream = new StreamWriter(_Link, false, Encoding.UTF8)) {
                    serialize(stream, data);
                }
                Log.nlog.Debug("Configuration saved: {0}", Settings.WorkingPath);
                Update();
            }
            catch(Exception ex) {
                Log.nlog.Error("Cannot apply configuration {0}", ex.Message);
            }
        }

        public string serialize(SolutionEvents data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter{ 
                AllowIntegerValues  = false,
                CamelCaseText       = true
            });
            settings.NullValueHandling = NullValueHandling.Include;
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        public SolutionEvents deserialize(string data)
        {
            return JsonConvert.DeserializeObject<SolutionEvents>(data);
        }

        protected SolutionEvents deserialize(StreamReader stream)
        {
            using(JsonTextReader reader = new JsonTextReader(stream)) {
                return (new JsonSerializer()).Deserialize<SolutionEvents>(reader);
            }
        }

        protected void serialize(TextWriter stream, SolutionEvents data)
        {
            stream.Write(serialize(data));
        }

        /// <summary>
        /// Older versions support :: Check version and reorganize structure if needed..
        /// </summary>
        /// <param name="stream"></param>
        protected void compatibility(StreamReader stream)
        {
            System.Version cfg = System.Version.Parse(data.Header.Compatibility);

            if(cfg.Major > Entity.Version.Major || (cfg.Major == Entity.Version.Major && cfg.Minor > Entity.Version.Minor)) {
                Log.nlog.Warn(
                    "Version {0} of configuration file is higher supported version {1}. Please update application. Several settings may be not correctly loaded.",
                    cfg.ToString(2), Entity.Version.ToString(2)
                );
            }

            //TODO: v0.7 -> v0.9

            if(cfg.Major == 0 && cfg.Minor < 4)
            {
                Log.show();
                Log.nlog.Info("Start upgrade configuration 0.3 -> 0.4");
                //Upgrade.Migration03_04.migrate(stream);
                //TODO: to ErrorList
                Log.nlog.Warn("Successfully upgraded. *Please, save manually!");
            }
        }

        /// <summary>
        /// Older versions support :: Change name settings
        /// </summary>
        /// <returns></returns>
        private void _xprojvsbeUpgrade()
        {
            string oldcfg = Settings.WorkingPath + ".xprojvsbe";
            if(!(File.Exists(oldcfg) && !File.Exists(_Link))) {
                return;
            }

            try {
                File.Move(oldcfg, _Link);
                Log.nlog.Info("Successfully upgraded settings :: .xprojvsbe -> {0}", Entity.NAME);
            }
            catch(Exception e) {
                Log.nlog.Fatal("Failed upgrade .xprojvsbe\n\n-----\n{0}\n", e.Message);
            }
        }


        private Config(){}
    }
}
