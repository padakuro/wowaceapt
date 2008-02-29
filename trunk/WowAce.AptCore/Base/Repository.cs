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
using System.Xml;
using System.Text.RegularExpressions;

namespace WowAce.AptCore
{
    public class AptRepository : AptCommon, IEnumerable<RepositoryAddonInfo>
    {
        private AptEnvironment AptEnv;

        private XmlDocument Database;
        private XmlNamespaceManager DatabaseNsMgr;

        private List<string> LookupList;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<RepositoryAddonInfo> GetEnumerator()
        {
            foreach (KeyValuePair<string, RepositoryAddonInfo> pair in Database)
            {
                yield return pair.Value;
            }
        }

        public AptRepository(AptEnvironment env)
        {
            AptEnv = env;

            if (!LoadIndexFile())
            {
                throw new Exception("repository.index.load.failed");
            }
            if (!ParseIndexFile())
            {
                throw new Exception("repository.index.parse.failed");
            }
        }

        public string GetRepositoryDate()
        {
            return Database.SelectSingleNode("rss/channel/lastBuildDate").InnerText;
        }

        public int GetAddonNum()
        {
            return Database.SelectNodes("rss/channel/item").Count;
        }

        public RepositoryAddonInfo GetAddonInfo(string addonName)
        {
            XmlNodeList items = Database.SelectNodes("rss/channel/item");
            addonName = addonName.ToLower();

            try
            {
                foreach (XmlNode item in items)
                {
                    if (item["title"].InnerText.ToLower() == addonName)
                    {
                        RepositoryAddonInfo info = new RepositoryAddonInfo()
                        {
                            Name = item["title"].InnerText
                        };

                        if (item["link"] != null) { info.Link = item["link"].InnerText; }
                        if (item["description"] != null) { info.Description = item["description"].InnerText; }
                        if (item["comments"] != null) { info.CommentsUrl = item["comments"].InnerText; }
                        if (item["author"] != null) { info.Author = item["author"].InnerText; }
                        if (item["category"] != null) { info.Category = item["category"].InnerText; }
                        if (item["enclosure"] != null)
                        {
                            info.EnclosureUrl = item["enclosure"].Attributes["url"].InnerText;
                            info.ZipFileSize = Int32.Parse(item["enclosure"].Attributes["length"].InnerText);
                            info.ZipFileName = GetZipFileName(info.EnclosureUrl);
                        }
                        if (item["guid"] != null) { info.Guid = item["guid"].InnerText; }
                        if (item["pubDate"] != null) { info.PubDate = item["pubDate"].InnerText; }
                        if (item["wowaddon:version"] != null)
                        {
                            Match m = Regex.Match(item["wowaddon:version"].InnerText, @"(\d+)\.?(\d+)?");
                            if (m != null)
                            {
                                info.Version = new AddonVersionNumber(Int32.Parse(m.Groups[1].Value));
                            }
                        }
                        else
                        {
                            info.Version = AddonVersionNumber.NO_VERSION;
                        }
                        if (item["wowaddon:interface"] != null) { info.InterfaceVersion = Int32.Parse(item["wowaddon:interface"].InnerText); }
                        if (item["wowaddon:dependencies"] != null) { info.RequiredDeps = GetReqiredDependencies(item); }
                        if (item["wowaddon:provides"] != null)
                        {
                            info.Provides = GetProvidedModules(item);
                            info.IsPackage = true;
                        }
                        else
                        {
                            info.IsPackage = false;
                        }

                        return info;
                    }
                }
            }
            catch (Exception e)
            {
                SendDebugMessage("repository.info.failed", addonName, e.Message);
            }

            return new RepositoryAddonInfo();
        }

        public bool IsAddonInRepository(string addonName)
        {
            return LookupList.Contains(addonName.ToLower());
        }

        public List<string> Search(string addonName)
        {
            return new List<string>();
        }

        private List<string> GetProvidedModules(XmlNode item)
        {
            XmlNodeList modules = item.SelectNodes("wowaddon:provides", DatabaseNsMgr);

            if (modules.Count > 0)
            {
                List<string> list = new List<string>();

                foreach (XmlNode module in modules)
                {
                    list.Add(module.InnerText);
                }

                return list;
            }

            return null;
        }

        private List<string> GetReqiredDependencies(XmlNode item)
        {
            XmlNodeList deps = item.SelectNodes("wowaddon:dependencies", DatabaseNsMgr);

            if (deps.Count > 0)
            {
                List<string> list = new List<string>();

                foreach (XmlNode dep in deps)
                {
                    list.Add(dep.InnerText);
                }

                return list;
            }

            return null;
        }

        private string GetZipFileName(string url)
        {
            return url.Substring(url.LastIndexOf("/") + 1, (url.Length - url.LastIndexOf("/") - 1));
        }

        private bool LoadIndexFile()
        {
            SendDebugMessage("repository.index.load");
            try
            {
                Database = new XmlDocument();
                Database.Load(AptEnv.AptDataPath + AptEnv.UpdateIndexFile);

                // add namespace so we're able to read "wowaddon:" nodes (don't know if I got that right with the string uri, but it works :) )
                DatabaseNsMgr = new XmlNamespaceManager(Database.Schemas.NameTable);
                DatabaseNsMgr.AddNamespace("wowaddon", "http://www.wowace.com/xmlns/wowaddon/");

                SendDebugMessage("repository.index.load.success");
                return true;
            }
            catch (Exception e)
            {
                Database = null;
                SendDebugMessage("repository.index.load.failed", e.Message);
                return false;
            }
        }

        private bool ParseIndexFile()
        {
            SendDebugMessage("repository.index.parse");

            try
            {
                LookupList = new List<string>();
                
                XmlNodeList items = Database.SelectNodes("rss/channel/item");

                foreach (XmlNode item in items)
                {
                    if (item != null && item["title"] != null)
                    {
                        LookupList.Add(item["title"].InnerText.ToLower());
                    }
                }

                SendDebugMessage("repository.index.parse.success", LookupList.Count.ToString());
                return true;
            }
            catch (Exception e)
            {
                SendDebugMessage("repository.index.parse.failed", e.Message);
                return false;
            }
        }
    }
}
