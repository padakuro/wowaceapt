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
    along with WowAce.AptCore.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace WowAce.AptCore
{
    public class AptActionRemove : AptAction
    {
        private AptEnvironment AptEnv;
        private AptLocal AptL;
        private AptRemote AptR;
        private AptRepository AptRepo;

        private List<string> Queue;

        public AptActionRemove(AptEnvironment env)
        {
            AptEnv = env;
            AptL = new AptLocal(env);
            AptR = new AptRemote(env);
            AptRepo = new AptRepository(env);

            Initialize();
        }

        public AptActionRemove(AptEnvironment env, AptLocal local, AptRemote remote, AptRepository repo)
        {
            AptEnv = env;
            AptL = local;
            AptR = remote;
            AptRepo = repo;

            Initialize();
        }

        private void Initialize()
        {
            Queue = new List<string>();
        }

        public bool Add(string addonName)
        {
            addonName = addonName.ToLower();

            if (!AptL.IsInstalled(addonName))
            {
                return false;
            }

            Queue.Add(addonName);
            
            // don't delete dependencies
            if (!AptEnv.FetchRequiredDeps)
            {
                return true;
            }

            // add unused dependencies to remove queue
            RepositoryAddonInfo info = AptRepo.GetAddonInfo(addonName);
            if (info.RequiredDeps != null)
            {
                // fetch list of needed dependencies
                List<string> usedDependecies = new List<string>();
                foreach (LocalAddonInfo depInfo in AptL)
                {
                    if (depInfo.Name.ToLower() != addonName)
                    {
                        List<string> deps = AptRepo.GetAddonInfo(depInfo.Name).RequiredDeps;

                        if (deps != null)
                        {
                            foreach (string dependency in deps)
                            {
                                if (!usedDependecies.Contains(dependency.ToLower()))
                                {
                                    usedDependecies.Add(dependency.ToLower());
                                }
                            }
                        }
                    }
                }

                foreach (string dependency in info.RequiredDeps)
                {
                    string name = dependency.ToLower();
                    if (!usedDependecies.Contains(name) && AptL.IsInstalled(name))
                    {
                        Queue.Add(name);
                    }
                }
            }

            return true;
        }

        public void Run()
        {
            foreach (string addon in Queue)
            {
                SendStatus("remove", addon);
                if (AptL.Uninstall(addon))
                {
                    SendStatus("remove.success");
                }
                else
                {
                    SendStatus("remove.failed");
                }
            }
        }

        public List<string> GetQueue()
        {
            return Queue;
        }
    }
}
