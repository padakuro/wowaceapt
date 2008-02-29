/*
    This file is part of WowAce.AptGet.
    Copyright (C) 2008  Sairén of EU-Malfurion

    WowAce.AptCore is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    WowAce.AptCore is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

using Microsoft.Win32;

using WowAce.AptCore;

namespace WowAce.AptGet
{
    partial class Program
    {
        private bool Is64BitOS = false;
        private Config Cfg;
        
        private Thread Worker;

        private AptEnvironment AddonEnv;
        private AptLocal AddonLocal;
        private AptRemote AddonRemote;
        private AptRepository AddonRepo;

        private List<string> ArgAddons;
        private List<string> ArgExclude;

        public Program(string[] args)
        {
            Console.Clear();

            // init config
            Cfg = new Config(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"));

            // 64bit OS?
            if(System.Runtime.InteropServices.Marshal.SizeOf(typeof(IntPtr)) == 8)
            {
                Is64BitOS = true;
            }

            // detect WoW installation
            if (Cfg.AutoDetectWoW)
            {
                string path = GetWoWInstallationPath();
                if (path != null)
                {
                    SendDebugMessage("aptget.wow.installdir", path);
                    Cfg.WoWInstallPath = path;
                }
            }

            // check if WoW directory exists
            if (!Directory.Exists(Cfg.WoWInstallPath))
            {
                Output.Error("Invalid WoW installation: " + Cfg.WoWInstallPath);
                Environment.Exit(1);
            }

            PrintAppHeader();

            if (args.Length < 1)
            {
                ExitWrongUsage();
            }

            // parse arguments
            ArgAddons = new List<string>();
            ArgExclude = new List<string>();

            for (int i = 1; i < args.Length; ++i)
            {
                Match m = Regex.Match(args[i], @"--([\w-]+)=?([\w]+)?");
                
                // is option
                if(m.Success) {
                    switch (m.Groups[1].Value)
                    {
                        case "ext":         Cfg.UseExternals = true; break;
                        case "no-ext":      Cfg.UseExternals = false; break;
                        case "silent":      Cfg.SilentMode = true; break;
                        case "ask":         Cfg.SilentMode = false; break;
                        case "no-debug":    Cfg.DebugEnabled = false; break;
                        case "debug":       Cfg.DebugEnabled = true; break;
                        case "unpack":      Cfg.UnpackPackages = true; break;
                        case "keep-zips":   Cfg.KeepZips = true; break;
                        case "no-deps":     Cfg.FetchRequiredDeps = false; break;
                        case "deps":        Cfg.FetchRequiredDeps = true; break;
                        case "update":      Cfg.AutoUpdateIndex = true; break;
                        case "exclude":
                            if (m.Groups[2].Value != null)
                            {
                                ArgExclude.Add(m.Groups[2].Value.ToLower());
                            }
                            break;
                        
                        default:
                            Output.Error("Unrecognized argument: " + m.Groups[1].Value);
                            break;
                    }
                }
                // is addon
                else {
                    ArgAddons.Add(args[i]);
                }
            }

            // initalize AptEnvironment
            AddonEnv = new AptEnvironment();
            AddonEnv.UpdateServerUri = "http://files.wowace.com/";
            if (Cfg.UseExternals)
            {
                AddonEnv.UpdateIndexFile = "latest.xml";
            }
            else
            {
                AddonEnv.UpdateIndexFile = "latest-noext.xml";
            }
            AddonEnv.WoWInstallPath = Cfg.WoWInstallPath;
            AddonEnv.DeleteBeforeExtract = Cfg.DeleteBeforeExtract;
            AddonEnv.UnpackPackages = Cfg.UnpackPackages;
            AddonEnv.KeepZips = Cfg.KeepZips;
            AddonEnv.FetchRequiredDeps = Cfg.FetchRequiredDeps;
            AddonEnv.FetchOptionalDeps = Cfg.FetchOptionalDeps;

            AptCommon.AddDebugListener(new AptCommon.DebugMessageEventHandler(ConsoleDebug));

            // execute command
            switch (args[0])
            {
                // show some help
                case "help":
                    ShowHelp();
                    break;
                
                // set default configuration
                case "config":
                    Cfg.Save();
                    break;
                
                // update repository file
                case "update":
                    Worker = new Thread(new ThreadStart(DoUpdate));
                    Worker.Start();
                    break;

                // upgrade addons
                case "upgrade":
                    Worker = new Thread(new ThreadStart(DoUpgrade));
                    Worker.Start();
                    break;

                // install addon(s)
                case "install":
                    if (ArgAddons.Count > 0)
                    {
                        Worker = new Thread(new ThreadStart(DoInstall));
                        Worker.Start();
                    }
                    break;

                // remove addon(s)
                case "remove":
                    if (ArgAddons.Count > 0)
                    {
                        Worker = new Thread(new ThreadStart(DoRemove));
                        Worker.Start();
                    }
                    break;

                // clear SavedVariables
                case "clear":
                    Output.Error("Not implemented yet.");
                    break;

                // search repository
                case "search":
                    Output.Error("Not implemented yet.");
                    break;

                // show addon info from repository
                case "show":
                    if (ArgAddons.Count > 0)
                    {
                        Worker = new Thread(new ThreadStart(DoShow));
                        Worker.Start();
                    }
                    break;

                // print changelog of an addon
                case "changelog":
                    Output.Error("Not implemented yet.");
                    break;

                // backup all addons (with/without SVs) into one archive
                case "backup":
                    Output.Error("Not implemented yet.");
                    break;

                // restore a backup
                case "restore":
                    Output.Error("Not implemented yet.");
                    break;

                // print some apt information
                case "info":
                    Output.Error("Not implemented yet.");
                    break;

                // pack (if possible)
                case "pack":
                    Worker = new Thread(new ThreadStart(DoPack));
                    Worker.Start();
                    break;

                // unpack (if possible)
                case "unpack":
                    Worker = new Thread(new ThreadStart(DoUnpack));
                    Worker.Start();
                    break;

                default:
                    ExitWrongUsage();
                    break;
            }
        }

        public void ConsoleDebug(string[] message)
        {
            if (!Cfg.DebugEnabled)
            {
                return;
            }
            
            Output.NewLine();
            Console.Write(String.Format("[d] ({0})", message[0]));
            if (message.Length >= 2)
            {
                Console.Write(" " + message[1]);
            }
            if (message.Length >= 3)
            {
                Console.Write(", " + message[2]);
            }
            if (message.Length == 4)
            {
                Console.Write(", " + message[3]);
            }
        }

        public void ConsoleError(string[] message)
        {
            Output.Error(message[0]);
        }

        private void PrintEnvInfo()
        {
            Output.Info("Options:\n");

            if (Cfg.UseExternals)
            {
                Output.Append("  * With externals");
            }
            else
            {
                Output.Append("  * Without externals");
                if (Cfg.FetchRequiredDeps)
                {
                    Output.NewLine();
                    Output.Append("  * Fetch required dependencies");
                }
            }
            Output.NewLine();
            if (Cfg.UnpackPackages)
            {
                Output.Append("  * Unpack packages");
            }
            else
            {
                Output.Append("  * Don't unpack packages");
            }
            Output.NewLine();
            if (Cfg.DebugEnabled)
            {
                Output.Append("  * Debug enabled");
            }
            else
            {
                Output.Append("  * No debug");
            }
            Output.NewLine();
            if (Cfg.SilentMode)
            {
                Output.Append("  * Silent proceed");
            }
            else
            {
                Output.Append("  * Ask before proceed");
            }
        }

        private void PrintAppHeader()
        {
            // print more useless info to console
            Console.Write("WowAceAptGet - Version 1.0");
            if (Is64BitOS)
            {
                Console.Write(" - 64bit");
            }
            else
            {
                Console.Write(" - 32bit");
            }
            Console.Write("\n==================================");
            Console.Write("\n");
        }

        private void PrintAptInfo()
        {
            Output.Info(AddonLocal.GetAddonNum() + " addons installed.");
            Output.Info(AddonRepo.GetAddonNum() + " addons in repository.");
        }

        private  void PrintAddonInfo(string key, string value)
        {
            Output.Append(String.Format("\n  {0}{1}: {2}", key, new String(' ', 15-key.Length), value));
        }

        private void PrintAddonInfo(string text)
        {
            Output.Append(String.Format("\n  {0}", text));
        }

        private bool InitializeRepository()
        {
            try
            {
                AddonRepo = new AptRepository(AddonEnv);
                return true;
            }
            catch (Exception e)
            {
                Output.Error("Failed to load repository index.");
            }
            return false;
        }

        private void ExitWrongUsage()
        {
            Output.Error("Wrong command usage. Enter 'ace-get help' for more information.\n");
            Environment.Exit(1);
        }

        private string GetWoWInstallationPath()
        {
            RegistryKey key;
            if (Is64BitOS)
            {
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Blizzard Entertainment\World of Warcraft", false);
            }
            else
            {
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Blizzard Entertainment\World of Warcraft", false);
            }
            
            if (key != null)
            {
                return key.GetValue("InstallPath", "") as string;
            }
            return null;
        }

        private void SendDebugMessage(string str1)
        {
            ConsoleDebug(new string[] { str1 });
        }

        private void SendDebugMessage(string str1, string str2)
        {
            ConsoleDebug(new string[] { str1, str2 });
        }
    }
}
