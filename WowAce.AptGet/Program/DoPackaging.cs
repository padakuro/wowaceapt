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
        public void DoPack()
        {
            AddonLocal = new AptLocal(AddonEnv);

            if (ArgAddons.Count > 0)
            {
                foreach (string addon in ArgAddons)
                {
                    _DoPack(addon);
                }
            }
            else
            {
                foreach (LocalAddonInfo info in AddonLocal)
                {
                    _DoPack(info.Name);
                }
            }
        }

        private void _DoPack(string addonName)
        {
            if (AddonLocal.IsPackage(addonName))
            {
                if (AddonLocal.IsUnpacked(addonName))
                {
                    AddonLocal.Pack(addonName);
                }
                else
                {
                    Output.Error(AddonLocal.GetAddonInfo(addonName).Name + " is currently not unpacked.");
                }
            }
            else
            {
                Output.Error(AddonLocal.GetAddonInfo(addonName).Name + " has no modules.");
            }
        }

        public void DoUnpack()
        {
            AddonLocal = new AptLocal(AddonEnv);

            if (ArgAddons.Count > 0)
            {
                foreach (string addon in ArgAddons)
                {
                    _DoUnpack(addon);
                }
            }
            else
            {
                foreach (LocalAddonInfo info in AddonLocal)
                {
                    _DoUnpack(info.Name);
                }
            }
        }

        public void _DoUnpack(string addonName)
        {
            if (AddonLocal.IsPackage(addonName))
            {
                if (!AddonLocal.IsUnpacked(addonName))
                {
                    AddonLocal.Unpack(addonName);
                }
                else
                {
                    Output.Error(AddonLocal.GetAddonInfo(addonName).Name + " is currently not packed.");
                }
            }
            else
            {
                Output.Error(AddonLocal.GetAddonInfo(addonName).Name + " has no modules.");
            }
        }
    }
}
