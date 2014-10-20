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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using net.r_eg.vsSBE.Exceptions;

namespace net.r_eg.vsSBE.SBEScripts.Components
{
    public class OWPComponent: IComponent
    {
        /// <summary>
        /// Type of implementation
        /// </summary>
        public ComponentType Type
        {
            get { return ComponentType.OWP; }
        }

        /// <summary>
        /// Handling with current type
        /// </summary>
        /// <param name="data">mixed data</param>
        /// <returns>prepared and evaluated data</returns>
        public string parse(string data)
        {
            Match m = Regex.Match(data, @"^\[OWP
                                              \s+
                                              (                  #1 - full ident
                                                ([A-Za-z_0-9]+)  #2 - subtype
                                                .*
                                              )
                                           \]$", 
                                           RegexOptions.IgnorePatternWhitespace);

            if(!m.Success) {
                throw new SyntaxIncorrectException("Failed OWPComponent - '{0}'", data);
            }

            switch(m.Groups[2].Value) {
                case "out": {
                    Log.nlog.Debug("OWPComponent: use stOut");
                    return stOut(m.Groups[1].Value);
                }
            }
            throw new SubtypeNotFoundException("OWPComponent: not found subtype - '{0}'", m.Groups[2].Value);
        }

        /// <summary>
        /// Any getting data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string stOut(string data)
        {
            Match m = Regex.Match(data, @"out
                                          (?:
                                            \s*
                                            \((.+)\)  #1 - arguments (optional)
                                          )?
                                          (.*)        #2 - property
                                          ", 
                                          RegexOptions.IgnorePatternWhitespace);

            if(!m.Success) {
                throw new TermNotFoundException("Failed stOut - '{0}'", data);
            }
            
            if(m.Groups[1].Success)
            {
                string item = m.Groups[1].Value;
                Log.nlog.Debug("stOut: item = '{0}'", item);

                if(item == "Build") {
                    //used by default - #[OWP out(Build)] / #[OWP out]
                }
                else {
                    throw new NotSupportedOperationException("item - '{0}' not yet supported", item);
                }
            }
            string property = m.Groups[2].Value.Trim();
            Log.nlog.Debug("stOut: property = '{0}'", property);

            // #[OWP out.All] / #[OWP out]
            if(property == ".All" || property == String.Empty) {
                return OWP.Items._.Build.Raw;
            }

            // #[OWP out.Warnings.Raw] / #[OWP out.Warnings]
            if(property == ".Warnings" || property == ".Warnings.Raw") {
                return (OWP.Items._.Build.IsWarnings)? OWP.Items._.Build.Raw : String.Empty;
            }

            // #[OWP out.Warnings.Count]
            if(property == ".Warnings.Count") {
                return OWP.Items._.Build.WarningsCount.ToString();
            }

            // #[OWP out.Warnings.Codes]
            if(property == ".Warnings.Codes") {
                return String.Join(",", OWP.Items._.Build.Warnings);
            }

            // #[OWP out.Errors.Raw] / #[OWP out.Errors]
            if(property == ".Errors" || property == ".Errors.Raw") {
                return (OWP.Items._.Build.IsErrors)? OWP.Items._.Build.Raw : String.Empty;
            }

            // #[OWP out.Errors.Count]
            if(property == ".Errors.Count") {
                return OWP.Items._.Build.ErrorsCount.ToString();
            }

            // #[OWP out.Errors.Codes]
            if(property == ".Errors.Codes") {
                return String.Join(",", OWP.Items._.Build.Errors);
            }

            throw new NotSupportedOperationException("property - '{0}' not yet supported", property);
        }
    }
}
