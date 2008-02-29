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
using System.IO;

namespace WowAce.AptCore
{
    public class AptActionInstall : AptAction
    {
        protected AptEnvironment AptEnv;
        protected AptLocal AptL;
        protected AptRemote AptR;
        protected AptRepository AptRepo;

        public AptActionInstall(AptEnvironment env)
        {
            AptEnv = env;
            AptL = new AptLocal(env);
            AptR = new AptRemote(env);
            AptRepo = new AptRepository(env);
        }
        
        public AptActionInstall(AptEnvironment env, AptLocal local, AptRemote remote, AptRepository repo)
        {
            AptEnv = env;
            AptL = local;
            AptR = remote;
            AptRepo = repo;
        }

        public bool Install(string addonName, bool skipDependencies)
        {
            RepositoryAddonInfo info = AptRepo.GetAddonInfo(addonName);

            SendStatus("install", info.Name);

            bool success = false;

            // remove old
            if (AptL.IsInstalled(addonName))
            {
                SendStatus("uninstall");
                if (AptL.Uninstall(addonName))
                {
                    SendStatus("uninstall.success");
                }
                else
                {
                    SendStatus("uninstall.failed");
                }
            }

            // download zip
            string zipFile = Path.Combine(AptEnv.AptZipPath, info.ZipFileName);
            if (!File.Exists(zipFile))
            {
                SendStatus("zip.download");
                if (AptR.DownloadZip(info))
                {
                    SendStatus("zip.download.success");
                }
                else
                {
                    SendStatus("zip.download.failed");
                }
            }
            else
            {
                SendStatus("zip.cache");
            }

            // install locally
            SendStatus("zip.extract");
            if (AptL.Install(zipFile))
            {
                SendStatus("install.success");
                success = true;
            }
            else
            {
                SendStatus("install.failed");
            }

            // use externals? if not, shall we download the deps?
            if (!skipDependencies && !AptEnv.UseExternals && AptEnv.FetchRequiredDeps)
            {
                bool successDeps = true;

                SendStatus("install.dependencies");
                
                for (int i = 0; i < info.RequiredDeps.Count; ++i)
                {
                    if (Install(info.RequiredDeps[i], true))
                    {
                        SendStatus("install.dependency.success", info.RequiredDeps[i]);
                    }
                    else
                    {
                        SendStatus("install.dependency.failed", info.RequiredDeps[i]);
                        successDeps = false;
                    }
                }

                SendStatus("install.dependencies.finished",  successDeps.ToString());
            }

            // unpack?
            if (AptEnv.UnpackPackages)
            {
                SendStatus("unpack");
                if (AptL.IsPackage(addonName))
                {
                    if (AptL.Unpack(addonName))
                    {
                        SendStatus("unpack.success");
                    }
                    else
                    {
                        SendStatus("unpack.failed");
                    }
                }
                else
                {
                    SendStatus("unpack.nopackage");
                }
            }

            // remove zip file?
            if (!AptEnv.KeepZips)
            {
                SendStatus("clear");
                try
                {
                    File.Delete(Path.Combine(AptEnv.AptZipPath, info.ZipFileName));
                    SendStatus("clear.success");
                }
                catch(IOException e) 
                {
                    SendStatus("clear.failed");
                }
            }

            if (success)
            {
                SendStatus("success");
            }
            else
            {
                SendStatus("failed");
            }

            return success;
        }
    }
}
