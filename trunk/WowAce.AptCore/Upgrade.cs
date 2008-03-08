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
    public class AptActionUpgrade : AptActionInstall
    {
        private List<string> NamePatterns;
        private List<string> ExcludedAddons;
        private List<string> UpdateQueue;
        private List<string> DependencyQueue;
        private List<string> UnresolvedDeps;

        public AptActionUpgrade(AptEnvironment env)
            : base(env)
        {
            AptEnv = env;
            AptL = new AptLocal(env);
            AptR = new AptRemote(env);
            AptRepo = new AptRepository(env);

            Initialize();
        }

        public AptActionUpgrade(AptEnvironment env, AptLocal local, AptRemote remote, AptRepository repo)
            : base(env, local, remote, repo)
        {
            AptEnv = env;
            AptL = local;
            AptR = remote;
            AptRepo = repo;

            Initialize();
        }

        private void Initialize()
        {
            ExcludedAddons = new List<string>();
            NamePatterns = new List<string>();
            DependencyQueue = new List<string>();
            UnresolvedDeps = new List<string>();
        }

        public int Prepare()
        {
            UpdateQueue = new List<string>();

            bool usePatterns = NamePatterns.Count > 0;

            foreach (LocalAddonInfo local in AptL)
            {
                string name = local.Name.ToLower();
                bool match = false;

                SendDebugMessage("upgrade.prepare", name);

                // excluded?
                if (ExcludedAddons.Contains(name))
                {
                    SendDebugMessage("upgrade.prepare.excluded", name);
                    continue;
                }

                // use name patterns?
                if (usePatterns)
                {
                    for (int i = 0; i < NamePatterns.Count; ++i)
                    {
                        // *
                        if (NamePatterns[i].Contains("*"))
                        {
                            if (name.Contains(NamePatterns[i].Replace("*", "")))
                            {
                                match = true;
                                SendDebugMessage("upgrade.prepare.pattern.match", name, NamePatterns[i]);
                                break;
                            }
                        }
                        else
                        {
                            if (name == NamePatterns[i])
                            {
                                match = true;
                                SendDebugMessage("upgrade.prepare.pattern.match", name, NamePatterns[i]);
                                break;
                            }
                        }
                    }
                }

                // addon doesn't match -> next one
                if (usePatterns && !match)
                {
                    SendDebugMessage("upgrade.prepare.pattern.nomatch", name);
                    continue;
                }

                // check if it's a module of some addon
                RepositoryAddonInfo update = AptRepo.GetAddonInfo(name);
                string parent = AptL.IsModule(local.Name);

                // if it's a module, add parent to queue
                if (parent != null)
                {
                    SendDebugMessage("upgrade.prepare.ismodule", name, parent);

                    // addon in repository?
                    if (!AptRepo.IsAddonInRepository(parent))
                    {
                        SendDebugMessage("upgrade.prepare.parent.notinrepo", parent);
                        SendStatus("notinrepo");
                        continue;
                    }

                    // no new version -> next one
                    if (AptL.GetAddonInfo(parent).Version >= AptRepo.GetAddonInfo(parent).Version)
                    {
                        SendDebugMessage("upgrade.prepare.parent.noewnewversion", parent);
                        SendStatus("nonewversion");
                        continue;
                    }
                    
                    AddToQueue(parent);
                }
                else
                {
                    // addon in repository?
                    if (!AptRepo.IsAddonInRepository(name))
                    {
                        SendDebugMessage("upgrade.prepare.notinrepo", name);
                        SendStatus("notinrepo");
                        continue;
                    }

                    // no new version -> next one
                    if (local.Version >= update.Version)
                    {
                        SendDebugMessage("upgrade.prepare.nonewversion", name);
                        SendStatus("nonewversion");
                        continue;
                    }
                    
                    AddToQueue(name);
                }

                // fetch dependencies?
                if (!AptEnv.UseExternals && AptEnv.FetchRequiredDeps)
                {
                    SendDebugMessage("upgrade.prepare.dependencies.resolve" + name);
                    SendStatus("resolvedeps");

                    if (update.RequiredDeps != null)
                    {
                        bool depError = false;

                        for (int i = 0; i < update.RequiredDeps.Count; ++i)
                        {
                            if (AptRepo.IsAddonInRepository(update.RequiredDeps[i]))
                            {
                                SendDebugMessage("upgrade.prepare.dependencies.add", update.RequiredDeps[i]);
                                AddDependency(update.RequiredDeps[i]);
                            }
                            else
                            {
                                SendDebugMessage("upgrade.prepare.dependencies.unresolved", update.RequiredDeps[i]);
                                UnresolvedDeps.Add(update.RequiredDeps[i]);
                                depError = true;
                            }
                        }

                        if (depError)
                        {
                            SendStatus("unresolveddeps");
                        }
                    }
                    else
                    {
                        SendDebugMessage("upgrade.prepare.dependencies.nodeps");
                        SendStatus("nodeps");
                    }
                }
            }

            return (UpdateQueue.Count + DependencyQueue.Count);
        }

        public void Run()
        {
            // fetch dependencies
            foreach (string dependency in DependencyQueue)
            {
                SendDebugMessage("upgrade.run.depdendency", dependency);

                // already in the addon queue
                if (UpdateQueue.Contains(dependency))
                {
                    SendDebugMessage("upgrade.run.depdendency.inaddonqueue", dependency);
                    continue;
                }

                // no new version -> next one
                if (AptL.IsInstalled(dependency))
                {
                    if (AptL.GetAddonInfo(dependency).Version >= AptRepo.GetAddonInfo(dependency).Version)
                    {
                        SendDebugMessage("upgrade.run.depdendency.uptodate", dependency);
                        continue;
                    }
                }

                // install
                Install(dependency, true);
            }

            // fetch addons
            foreach (string addon in UpdateQueue)
            {
                SendDebugMessage("upgrade.run.install", addon);
                
                // install
                Install(addon, true);
            }
        }

        public List<string> GetQueue()
        {
            List<string> all = new List<string>();
            foreach (string addon in UpdateQueue)
            {
                string name = AptRepo.GetAddonInfo(addon).Name;
                if (!all.Contains(name))
                {
                    all.Add(name);
                }
            }
            foreach (string addon in DependencyQueue)
            {
                string name = AptRepo.GetAddonInfo(addon).Name;
                if (!all.Contains(name))
                {
                    all.Add(name);
                }
            }

            return all;
        }

        public void AddPattern(string namePattern)
        {
            namePattern = namePattern.ToLower();
            if(!NamePatterns.Contains(namePattern))
            {
                NamePatterns.Add(namePattern);
            }
        }

        public void Exclude(string addonName)
        {
            addonName = addonName.ToLower();
            if (!ExcludedAddons.Contains(addonName))
            {
                ExcludedAddons.Add(addonName);
            }
        }

        private void AddToQueue(string addonName)
        {
            addonName = addonName.ToLower();
            if (!UpdateQueue.Contains(addonName))
            {
                UpdateQueue.Add(addonName);
                SendDebugMessage("upgrade.prepare.add", addonName);
                SendStatus("addaddon", addonName);
            }
        }

        private void AddDependency(string depName)
        {
            depName = depName.ToLower();
            if (!DependencyQueue.Contains(depName))
            {
                DependencyQueue.Add(depName);
                SendDebugMessage("upgrade.prepare.dependency.add", depName);
                SendStatus("adddependency", depName);
            }
        }
    }
}
