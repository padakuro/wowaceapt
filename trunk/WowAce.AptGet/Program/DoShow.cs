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
        public void DoShow()
        {
            AddonLocal = new AptLocal(AddonEnv);

            if (InitializeRepository())
            {
                if (AddonRepo.IsAddonInRepository(ArgAddons[0]))
                {
                    RepositoryAddonInfo info = AddonRepo.GetAddonInfo(ArgAddons[0]);

                    PrintAddonInfo("Name", info.Name);
                    if (AddonLocal.IsInstalled(info.Name))
                    {
                        PrintAddonInfo("State", "Installed");
                        Output.Append(String.Format(" (Version {0})", AddonLocal.GetAddonInfo(info.Name).Version.Major.ToString()));
                    }
                    else
                    {
                        PrintAddonInfo("State", "Not installed");
                    }
                    if (info.Category != null) { PrintAddonInfo("Category", info.Category); }
                    if (info.Author != null) { PrintAddonInfo("Author", info.Author); }
                    PrintAddonInfo("Version", info.Version.Major.ToString());
                    PrintAddonInfo("Interface", info.InterfaceVersion.ToString());
                    if (info.IsStable)
                    {
                        PrintAddonInfo("Stable", "Yes");
                    }
                    else
                    {
                        PrintAddonInfo("Stable", "No");
                    }
                    if (info.PubDate != null)
                    {
                        PrintAddonInfo("Published", info.PubDate);
                    }
                    if (info.RequiredDeps != null)
                    {
                        PrintAddonInfo("Dependencies", String.Join(", ", info.RequiredDeps.ToArray()));
                    }
                    if (info.Description != null)
                    {
                        Output.NewLine();
                        PrintAddonInfo(info.Description);
                        Output.NewLine();
                    }
                }
                else
                {
                    Output.Info("Addon not found in repository.");
                }
            }
        }
    }
}
