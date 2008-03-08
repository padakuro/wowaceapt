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
    along with WowAce.AptGet.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;

using WowAce.AptCore;

namespace WowAce.AptGet
{
    partial class Program
    {
        public void InstallStatusMessage(string[] message)
        {
            switch (message[0])
            {
                case "install":             Output.Append(String.Format("\nInstalling {0}...", message[1])); break;
                case "uninstall":           Output.Append("\n  * uninstalling..."); break;
                case "zip.download":        Output.Append("\n  * downloading..."); break;
                case "zip.cache":           Output.Append("\n  * using cache..."); break;
                case "zip.extract":         Output.Append("\n  * extracting zip file..."); break;
                case "install.dependencies":Output.Append("\n  * installing dependencies..."); break;
                case "unpack":              Output.Append("\n  * unpacking..."); break;
                case "clear":               Output.Append("\n  * clear cache..."); break;
                case "success":             Output.Append("\n  > installation successful!"); break;
                case "failed":              Output.Append("\n  > installation failed!"); break;

                case "unpack.nopackage":    Output.Append(" no package."); break;

                case "uninstall.failed":
                case "zip.download.failed":
                case "install.failed":
                case "unpack.failed":
                case "clear.failed":
                    Output.Append(" failed.");
                    break;

                case "uninstall.success":
                case "zip.download.success":
                case "install.success":
                case "install.dependencies.finished":
                case "install.dependencies.none":
                case "unpack.success":
                case "clear.success":
                    Output.Append(" done.");
                    break;
            }
        }
        
        public void DoInstall()
        {
            AddonLocal = new AptLocal(AddonEnv);
            AddonRemote = new AptRemote(AddonEnv);

            if (Cfg.AutoUpdateIndex)
            {
                DoUpdate();
            }

            Output.Info("Action: install addon(s).");

            int installedAddons = 0;

            if (InitializeRepository())
            {
                if (!Cfg.AutoUpdateIndex)
                {
                    Output.Info(String.Format("There are currently {0} addons in the database.", AddonRepo.GetAddonNum()));
                    PrintEnvInfo();
                }

                AptActionInstall install = new AptActionInstall(AddonEnv, AddonLocal, AddonRemote, AddonRepo);
                install.AddStatusListener(new AptAction.StatusMessageEventHandler(InstallStatusMessage));

                foreach (string addon in ArgAddons)
                {
                    if (AddonRepo.IsAddonInRepository(addon))
                    {
                        if (install.Install(addon, false))
                        {
                            installedAddons++;
                        }
                    }
                    else
                    {
                        Output.Error(String.Format("Addon not found: {0}", addon));
                    }
                }

                Output.Info(String.Format("Addons installed: {0}", installedAddons));
            }
        }

        public void DoUpdate()
        {
            AddonRemote = new AptRemote(AddonEnv);
            
            Output.Info("Action: update repository database.");
            PrintEnvInfo();
            try
            {
                if (AddonRepo == null)
                {
                    AddonRepo = new AptRepository(AddonEnv);
                }
                Output.Info(String.Format("Current index from: {0}", AddonRepo.GetRepositoryDate()));
                Output.Info(String.Format("There are currently {0} addons in the database.", AddonRepo.GetAddonNum()));
            }
            catch (Exception e)
            {
                Output.Error("Failed to load current index file.");
            }
            
            Output.Info("Downloading index file...");

            if (AddonRemote.UpdateIndexFile())
            {
                Output.Info("Download successful.");

                try
                {
                    AddonRepo = new AptRepository(AddonEnv);
                    Output.Info(String.Format("Updated index from: {0}", AddonRepo.GetRepositoryDate()));
                    Output.Info(String.Format("There are now {0} addons in the database.", AddonRepo.GetAddonNum()));
                }
                catch (Exception e)
                {
                    Output.Error("Failed to downloaded index file.");
                }
            }
            else
            {
                Output.Error("Download failed.");
            }
        }
    }
}
