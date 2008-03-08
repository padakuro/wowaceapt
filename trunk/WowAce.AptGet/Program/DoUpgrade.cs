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
using System.Collections.Generic;

using WowAce.AptCore;

namespace WowAce.AptGet
{
    partial class Program
    {
        private void UpgradeStatusMessage(string[] message)
        {
            switch (message[0])
            {
                default:
                    InstallStatusMessage(message);
                    break;
            }
        }
        
        public void DoUpgrade()
        {
            AddonLocal = new AptLocal(AddonEnv);
            AddonRemote = new AptRemote(AddonEnv);

            if (Cfg.AutoUpdateIndex)
            {
                DoUpdate();
            }

            if (InitializeRepository())
            {
                if (!Cfg.AutoUpdateIndex)
                {
                    Output.Info(String.Format("There are currently {0} addons in the database.", AddonRepo.GetAddonNum()));
                    PrintEnvInfo();
                }
                
                int updatedAddons;

                AptActionUpgrade upgrade = new AptActionUpgrade(AddonEnv, AddonLocal, AddonRemote, AddonRepo);
                upgrade.AddStatusListener(new AptAction.StatusMessageEventHandler(UpgradeStatusMessage));

                foreach (string excludeAddon in ArgExclude)
                {
                    upgrade.Exclude(excludeAddon);
                }

                if (ArgAddons.Count == 0)
                {
                    Output.Info("Action: upgrade all addons.");
                    updatedAddons = upgrade.Prepare();
                }
                else
                {
                    Output.Info("Action: upgrade some addons.");
                    
                    for (int i = 0; i < ArgAddons.Count; ++i)
                    {
                        upgrade.AddPattern(ArgAddons[i]);
                    }
                    updatedAddons = upgrade.Prepare();
                }

                if (updatedAddons > 0)
                {
                    Output.Info("An update is available for the following addons");

                    List<string> updated = upgrade.GetQueue();

                    // calculate download size
                    int downloadSize = 0;
                    foreach (string addon in updated)
                    {
                        downloadSize += AddonRepo.GetAddonInfo(addon).ZipFileSize;
                    }

                    Output.Append(String.Format(" (approx. {0} KB to download):\n\n", (downloadSize / 1024)));

                    Output.Append("  " + String.Join(", ", updated.ToArray()));
                    
                    if (!Cfg.SilentMode)
                    {
                        Output.Append("\n\nDo you want to proceed? [Y/N]: ");

                        if (Convert.ToChar(Console.Read()).ToString().ToLower() == "y")
                        {
                            upgrade.Run();
                        }
                    }
                    else
                    {
                        upgrade.Run();
                    }
                }
                else
                {
                    Output.Info("No updates available.");
                }
            }
        }
    }
}
