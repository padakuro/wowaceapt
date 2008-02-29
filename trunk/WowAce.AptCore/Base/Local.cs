/*
    This file is part of WowAce.AptCore.
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using ICSharpCode.SharpZipLib.Zip;

namespace WowAce.AptCore
{
    public class AptLocal : AptCommon, IEnumerable<LocalAddonInfo>
    {
        private AptEnvironment AptEnv;
        private Dictionary<string, LocalAddonInfo> AddonList;
        private Dictionary<string, string> ModuleList;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<LocalAddonInfo> GetEnumerator()
        {
            foreach (KeyValuePair<string, LocalAddonInfo> pair in AddonList)
            {
                yield return pair.Value;
            }
        }

        public AptLocal(AptEnvironment env)
        {
            AptEnv = env;

            SanityCheck();
            FetchInstalledAddons();
        }

        public LocalAddonInfo GetAddonInfo(string addonName)
        {
            return AddonList[addonName.ToLower()];
        }

        public int GetAddonNum()
        {
            return AddonList.Count;
        }

        public bool Install(string zipFile)
        {
            SendDebugMessage("local.install", zipFile);

            try
            {
                FastZip zip = new FastZip();
                zip.ExtractZip(Path.Combine(AptEnv.AptZipPath, zipFile), AptEnv.WoWAddonsPath, "");
                SendDebugMessage("local.install.success", zipFile);
                return true;
            }
            catch (ZipException e)
            {
                SendDebugMessage("local.install.failed", zipFile, e.Message);
                return false;
            }
        }

        public bool Uninstall(string addonName)
        {
            addonName = addonName.ToLower();

            SendDebugMessage("local.uninstall", addonName);

            if (IsPackage(addonName) && IsUnpacked(addonName))
            {
                List<string> modules = GetModules(addonName);

                foreach (string module in modules)
                {
                    string moduleDir = Path.Combine(AptEnv.WoWAddonsPath, module.Replace("@", AddonList[addonName].Name + "_"));

                    try
                    {
                        SendDebugMessage("local.uninstall.module", module);
                        Directory.Delete(moduleDir, true);
                        SendDebugMessage("local.uninstall.module.success", module);
                    }
                    catch (IOException e)
                    {
                        SendDebugMessage("local.uninstall-module.failed", module, e.Message);
                    }
                }
            }

            try
            {
                Directory.Delete(Path.Combine(AptEnv.WoWAddonsPath, AddonList[addonName].Name), true);
                SendDebugMessage("local.uninstall.success", addonName);
                return true;
            }
            catch (IOException e)
            {
                SendDebugMessage("local.uninstall.failed", addonName, e.Message);
            }

            return false;
        }

        public bool Unpack(string addonName)
        {
            if (!IsPackage(addonName))
            {
                return false;
            }

            addonName = addonName.ToLower();

            SendDebugMessage("local.unpack", addonName);
            List<string> modules = GetModules(AddonList[addonName].Name);
            bool success = true;

            for (int i = 0; i < modules.Count; ++i)
            {
                string sourceDir = Path.Combine(AptEnv.WoWAddonsPath + AddonList[addonName].Name, modules[i].Replace("@", ""));
                string destDir = Path.Combine(AptEnv.WoWAddonsPath, modules[i].Replace("@", AddonList[addonName].Name + "_"));
                
                try
                {
                    Directory.Move(sourceDir, destDir);
                    SendDebugMessage("local.unpack.success", sourceDir, destDir);
                }
                catch (IOException e)
                {
                    SendDebugMessage("local.unpack.failed", sourceDir, destDir, e.Message);
                    success = false;
                }
            }

            return success;
        }

        public bool Pack(string addonName)
        {
            if (!IsPackage(addonName))
            {
                return false;
            }

            addonName = addonName.ToLower();

            SendDebugMessage("local.pack", addonName);
            List<string> modules = GetModules(AddonList[addonName].Name);
            bool success = true;

            for (int i = 0; i < modules.Count; ++i)
            {
                string sourceDir = Path.Combine(AptEnv.WoWAddonsPath, modules[i].Replace("@", AddonList[addonName].Name + "_"));
                string destDir = Path.Combine(AptEnv.WoWAddonsPath + AddonList[addonName].Name, modules[i].Replace("@", ""));

                try
                {
                    Directory.Move(sourceDir, destDir);
                    SendDebugMessage("local.pack.success", sourceDir, destDir);
                }
                catch (IOException e)
                {
                    SendDebugMessage("local.pack.failed", sourceDir, destDir, e.Message);
                    success = false;
                }
            }

            return success;
        }

        public bool ClearSavedVariables(string addonName)
        {
            return false;
        }

        public bool IsPackage(string addonName)
        {
            return File.Exists(Path.Combine(AptEnv.WoWAddonsPath, addonName + @"\filelist.wau"));
        }

        public bool IsInstalled(string addonName)
        {
            return AddonList.ContainsKey(addonName.ToLower());
        }

        public bool IsUnpacked(string addonName)
        {
            List<string> modules = GetModules(addonName);

            if (modules == null)
            {
                return false;
            }

            addonName = addonName.ToLower();

            for (int i = 0; i < modules.Count; ++i)
            {
                string moduleDir = Path.Combine(AptEnv.WoWAddonsPath, modules[i].Replace("@", AddonList[addonName].Name + "_"));

                if (Directory.Exists(moduleDir))
                {
                    return true;
                }
            }

            return false;
        }

        public string IsModule(string addonName)
        {
            if (ModuleList == null)
            {
                FetchModuleList();
            }
            
            addonName = addonName.ToLower();
            if (ModuleList.ContainsKey(addonName))
            {
                return ModuleList[addonName];
            }
            return null;
        }

        public AddonVersionNumber GetCurrentVersion(string addonName)
        {
            return new AddonVersionNumber(0);
        }

        private void FetchModuleList()
        {
            ModuleList = new Dictionary<string, string>();
            
            foreach (KeyValuePair<string, LocalAddonInfo> pair in AddonList)
            {
                if (IsPackage(pair.Value.Name) && IsUnpacked(pair.Key))
                {
                    List<string> modules = GetModules(pair.Value.Name);

                    for (int i = 0; i < modules.Count; ++i)
                    {
                        string moduleName = modules[i].Replace("@", pair.Value.Name + "_");

                        if (!ModuleList.ContainsKey(moduleName))
                        {
                            ModuleList.Add(moduleName.ToLower(), pair.Key);
                        }
                    }
                }
            }
        }

        private List<string> GetModules(string addonName)
        {
            addonName = addonName.ToLower();
            
            if (AddonList[addonName].Modules != null)
            {
                return AddonList[addonName].Modules;
            }

            StreamReader wauFile = new StreamReader(Path.Combine(AptEnv.WoWAddonsPath, AddonList[addonName].Name + @"\filelist.wau"));

            List<string> modules = new List<string>();
            string line = string.Empty;

            while (wauFile.Peek() > 0)
            {
                line = wauFile.ReadLine();
                modules.Add(line);
            }
            wauFile.Close();

            return modules;
        }

        private void SanityCheck()
        {
            SendDebugMessage("local.sanity");
           
            // check if interface/addon directory exists, if not create it
            if (!Directory.Exists(AptEnv.WoWAddonsPath))
            {
                try
                {
                    Directory.CreateDirectory(AptEnv.WoWAddonsPath);
                    SendDebugMessage("local.sanity.addondir.success");
                }
                catch (IOException e)
                {
                    SendDebugMessage("local.sanity.addonfdir.failed", e.Message);
                }
            }
            else
            {
                SendDebugMessage("local.sanity.ok");
            }
        }

        private void FetchInstalledAddons()
        {
            AddonList = new Dictionary<string, LocalAddonInfo>();

            SendDebugMessage("local.read");

            // read local addon directory
            try
            {
                DirectoryInfo dir = new DirectoryInfo(AptEnv.WoWAddonsPath);
                DirectoryInfo[] subDirs = dir.GetDirectories();

                foreach (DirectoryInfo addonDir in subDirs)
                {
                    // really an addon? -> then there's a .toc file
                    if (File.Exists(String.Format(@"{0}\{1}.toc", addonDir.FullName, addonDir.Name)))
                    {
                        AddonList.Add(addonDir.Name.ToLower(), new LocalAddonInfo() {
                            Name = addonDir.Name,
                            Version = GetVersionFromChangelog(addonDir.Name),
                            Modules = null,
                        });

                        SendDebugMessage("local.read.add", addonDir.Name.ToLower(), AddonList[addonDir.Name.ToLower()].Version.Major.ToString());
                    }
                }

                SendDebugMessage("local.read.success");
            }
            catch (IOException e)
            {
                SendDebugMessage("local.read.failed", e.Message);
            }
        }

        // code borrowed from CmdAceUpdater courtesy of lys
        // http://www.wowace.com/forums/index.php?topic=4778.0
        private AddonVersionNumber GetVersionFromChangelog(string addonName)
        {
            AddonVersionNumber version = AddonVersionNumber.NO_VERSION;
            
            string[] changelogFiles = Directory.GetFiles(Path.Combine(AptEnv.WoWAddonsPath, addonName), "Changelog*.txt");

            foreach (string changelog in changelogFiles)
            {
                string fileName = Path.GetFileName(changelog);
                Match m = Regex.Match(fileName, "(?<=r)[\\d\\.]+\\d");
                
                if (!m.Success) { continue; }

                AddonVersionNumber check = AddonVersionNumber.Parse(m.ToString());
                if (check > version) { version = check; }
            }

            return version;
        }
    }
}
