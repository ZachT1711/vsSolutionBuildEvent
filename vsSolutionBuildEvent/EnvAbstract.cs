﻿/*
 * Copyright (c) 2013-2016,2019  Denis Kuzmin < entry.reg@gmail.com > GitHub/3F
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Linq;
using net.r_eg.MvsSln;
using net.r_eg.MvsSln.Core;
using net.r_eg.vsSBE.API.Commands;
using net.r_eg.vsSBE.Exceptions;
using BuildType = net.r_eg.vsSBE.Bridge.BuildType;
using EProject = Microsoft.Build.Evaluation.Project;
using ProjectItem = net.r_eg.MvsSln.Core.ProjectItem;

namespace net.r_eg.vsSBE
{
    public abstract class EnvAbstract
    {
        /// <summary>
        /// Parsed solution data.
        /// </summary>
        protected ISlnResult sln;

        /// <summary>
        /// Activated environment for projects processing.
        /// </summary>
        protected IXProjectEnv slnEnv;

        //[Obsolete("integrate via IXProjectEnv use")]
        //protected IRuleOfConfig cfgRule = new RuleOfConfig();

        /// <summary>
        /// Project by default or "StartUp Project".
        /// </summary>
        public abstract string StartupProjectString { get; protected set; }

        /// <summary>
        /// Current context for actions.
        /// </summary>
        public BuildType BuildType
        {
            get;
            set;
        } = BuildType.Common;

        /// <summary>
        /// Sender of the core commands.
        /// </summary>
        public IFireCoreCommand CoreCmdSender
        {
            get;
            set;
        }

        /// <summary>
        /// Get instance of the Build.Evaluation.Project for accessing to properties etc.
        /// </summary>
        /// <param name="name">Specified project name. null value will use the name from startup-project.</param>
        /// <returns>Found relevant Microsoft.Build.Evaluation.Project.</returns>
        public virtual EProject getProject(string name = null)
        {
            // NOTE: Do not use ProjectCollection.GlobalProjectCollection from EnvDTE Environment because it can be empty.
            //       https://github.com/3F/vsSolutionBuildEvent/issues/8
            //       Either use DTE projects collection to refer to MBE projects, or use MvsSln's GetOrLoadProject

            Log.Trace($"getProject: started with '{name}' /{StartupProjectString}");

            if(String.IsNullOrEmpty(name)) {
                name = StartupProjectString;
            }

            ProjectItem project = sln.ProjectItems.FirstOrDefault(p => p.name == name);
            if(project.fullPath == null) {
                throw new NotFoundException($"Project '{name}' was not found. ['{project.name}', '{project.pGuid}']");
            }

            return slnEnv.GetOrLoadProject(project);
        }

        /// <summary>
        /// Returns formatted configuration from the SolutionConfiguration2
        /// </summary>
        public string SolutionCfgFormat(EnvDTE80.SolutionConfiguration2 cfg)
        {
            if(cfg == null) {
                return formatCfg(PropertyNames.UNDEFINED);
            }
            return formatCfg(cfg.Name, cfg.PlatformName);
        }

        protected string formatCfg(string name, string platform = null)
        {
            return ConfigItem.Format(name, platform ?? name);
        }
    }
}
