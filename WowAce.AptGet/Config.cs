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
using System.Xml;
using System.Text;
using System.IO;

namespace WowAce.AptGet
{
    class Config
    {
        private const string ConfigFile = "config.xml";

        private XmlDocument Cfg;
        private string DataPath;

        public bool UseExternals { get; set; }
        public bool KeepZips { get; set; }
        public bool DeleteBeforeExtract { get; set; }
        public bool CreateFullBackupBeforeUpgrade { get; set; }
        public bool UnpackPackages { get; set; }
        public bool FetchRequiredDeps { get; set; }
        public bool FetchOptionalDeps { get; set; }
        public bool DebugEnabled { get; set; }
        public bool SilentMode { get; set; }
        public bool AutoDetectWoW { get; set; }
        public bool Log { get; set; }
        public bool LogDebug { get; set; }
        public bool AutoUpdateIndex { get; set; }
        public string WoWInstallPath { get; set; }

        public Config(string dataPath)
        {
            DataPath = dataPath;
            
            // some defaults
            UseExternals = true;
            KeepZips = false;
            DeleteBeforeExtract = true;
            CreateFullBackupBeforeUpgrade = false;
            UnpackPackages = false;
            FetchRequiredDeps = false;
            FetchOptionalDeps = false;
            DebugEnabled = false;
            SilentMode = false;
            AutoDetectWoW = true;
            Log = false;
            LogDebug = false;
            WoWInstallPath = "";
            AutoUpdateIndex = false;
            
            try
            {
                Cfg = new XmlDocument();
                Cfg.Load(Path.Combine(DataPath, ConfigFile));

                XmlNode settings = Cfg.SelectSingleNode("wowace/aptget");

                if (settings == null)
                {
                    Cfg = null;
                    return;
                }

                if (settings["UseExternals"] != null)           { UseExternals = Boolean.Parse(settings["UseExternals"].InnerText); }
                if (settings["KeepZips"] != null)               { KeepZips = Boolean.Parse(settings["KeepZips"].InnerText); }
                if (settings["DeleteBeforeExtract"] != null)    { DeleteBeforeExtract = Boolean.Parse(settings["DeleteBeforeExtract"].InnerText); }
                if (settings["CreateFullBackupBeforeUpgrade"] != null) { CreateFullBackupBeforeUpgrade = Boolean.Parse(settings["CreateFullBackupBeforeUpgrade"].InnerText); }
                if (settings["UnpackPackages"] != null)         { UnpackPackages = Boolean.Parse(settings["UnpackPackages"].InnerText); }
                if (settings["FetchRequiredDeps"] != null)      { FetchRequiredDeps = Boolean.Parse(settings["FetchRequiredDeps"].InnerText); }
                if (settings["FetchOptionalDeps"] != null)      { FetchOptionalDeps = Boolean.Parse(settings["FetchOptionalDeps"].InnerText); }
                if (settings["DebugEnabled"] != null)           { DebugEnabled = Boolean.Parse(settings["DebugEnabled"].InnerText); }
                if (settings["SilentMode"] != null)             { SilentMode = Boolean.Parse(settings["SilentMode"].InnerText); }
                if (settings["AutoDetectWoW"] != null)          { AutoDetectWoW = Boolean.Parse(settings["AutoDetectWoW"].InnerText); }
                if (settings["WoWInstallPath"] != null)         { WoWInstallPath = settings["WoWInstallPath"].InnerText; }
                if (settings["Log"] != null)                    { Log = Boolean.Parse(settings["Log"].InnerText); }
                if (settings["LogDebug"] != null)               { LogDebug = Boolean.Parse(settings["LogDebug"].InnerText); }
                if (settings["AutoUpdateIndex"] != null)        { AutoUpdateIndex = Boolean.Parse(settings["AutoUpdateIndex"].InnerText); }
            }
            catch (IOException e)
            {
                Cfg = null;
            }
        }

        public void Save()
        {
            // check path
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
            
            XmlTextWriter xml = new XmlTextWriter(Path.Combine(DataPath, ConfigFile), Encoding.UTF8);

            xml.WriteStartDocument(true);
            xml.WriteStartElement("wowace");
            xml.WriteStartElement("aptget");

            WriteSetting(xml, "UseExternals", UseExternals.ToString());
            WriteSetting(xml, "KeepZips", KeepZips.ToString());
            WriteSetting(xml, "DeleteBeforeExtract", DeleteBeforeExtract.ToString());
            WriteSetting(xml, "CreateFullBackupBeforeUpgrade", CreateFullBackupBeforeUpgrade.ToString());
            WriteSetting(xml, "UnpackPackages", UnpackPackages.ToString());
            WriteSetting(xml, "FetchRequiredDeps", FetchRequiredDeps.ToString());
            WriteSetting(xml, "FetchOptionalDeps", FetchOptionalDeps.ToString());
            WriteSetting(xml, "DebugEnabled", DebugEnabled.ToString());
            WriteSetting(xml, "SilentMode", SilentMode.ToString());
            WriteSetting(xml, "AutoDetectWoW", AutoDetectWoW.ToString());
            WriteSetting(xml, "WoWInstallPath", WoWInstallPath);
            WriteSetting(xml, "Log", Log.ToString());
            WriteSetting(xml, "LogDebug", LogDebug.ToString());
            WriteSetting(xml, "AutoUpdateIndex", AutoUpdateIndex.ToString());

            xml.WriteEndElement();
            xml.WriteEndElement();

            xml.Close();
        }

        private void WriteSetting(XmlTextWriter xml, string element, string value)
        {
            xml.WriteStartElement(element);
            xml.WriteValue(value);
            xml.WriteEndElement();
        }
    }
}
