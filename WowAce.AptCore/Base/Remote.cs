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
using System.Net;
using System.IO;

namespace WowAce.AptCore
{
    public class AptRemote : AptCommon
    {
        private AptEnvironment AptEnv;

        public AptRemote(AptEnvironment env)
        {
            AptEnv = env;
        }

        public bool UpdateIndexFile()
        {
            SendDebugMessage("remote.download.index");
            
            using (WebClient client = GetWebClient())
            {
                try
                {
                    string uri = AptEnv.UpdateServerUri + AptEnv.UpdateIndexFile;
                    
                    client.Headers.Add("Accept-Encoding:  gzip, deflate");
                    client.DownloadFile(uri, Path.Combine(AptEnv.AptDataPath, AptEnv.UpdateIndexFile));
                    SendDebugMessage("remote.download.index.success");
                    return true;
                }
                catch (WebException e)
                {
                    SendDebugMessage("remote.download.index.failed", e.Message);
                    return false;
                }
            }
        }

        // code borrowed from CmdAceUpdater courtesy of lys
        // http://www.wowace.com/forums/index.php?topic=4778.0
        public bool DownloadZip(RepositoryAddonInfo addonInfo)
        {
            using (WebClient client = GetWebClient())
            {
                SendDebugMessage("remote.download.zip", addonInfo.Name, addonInfo.EnclosureUrl);
                
                try
                {
                    string saveTo = Path.Combine(AptEnv.AptZipPath, addonInfo.ZipFileName);

                    client.DownloadFile(addonInfo.EnclosureUrl, saveTo);
                    SendDebugMessage("remote.download.zip.success", addonInfo.Name, saveTo);
                    return true;
                }
                catch (WebException e)
                {
                    SendDebugMessage("remote.download.zip.failed", addonInfo.Name, addonInfo.EnclosureUrl, e.Message);
                    return false;
                }
            }
        }

        private WebClient GetWebClient()
        {
            WebClient Client = new WebClient();
            Client.Headers.Add("User-Agent", String.Format("{0}/{1}", "WowAceAptCore", 1));

            return Client;
        }
    }
}
