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
    public class AptEnvironment
    {
        // wow install info
        private string _WoWInstallPath;
        public string WoWInstallPath {
            get
            {
                return _WoWInstallPath;
            }
            set
            {
                _WoWInstallPath = value;
                WoWAddonsPath = Path.Combine(_WoWInstallPath, @"Interface\Addons\");
                WoWSVPath = Path.Combine(_WoWInstallPath, @"WTF\");
            }
        }
        public string WoWAddonsPath { get; private set; }
        public string WoWSVPath { get; private set; }

        // addon repository
        public string UpdateServerUri { get; set; }
        public string UpdateIndexFile { get; set; }

        // apt info
        public string AptDataPath { get; protected set; }
        public string AptZipPath { get; protected set; }
        public string AptBackupPath { get; protected set; }

        // settings
        public bool UseExternals { get; set; }
        public bool KeepZips { get; set; }
        public bool DeleteBeforeExtract { get; set; }
        public bool CreateFullBackupBeforeUpgrade { get; set; }
        public bool UnpackPackages { get; set; }
        public bool FetchRequiredDeps { get; set; }
        public bool FetchOptionalDeps { get; set; }

        public AptEnvironment()
        {
            AptDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"data\");
            AptZipPath = Path.Combine(AptDataPath, @"zips\");
            AptBackupPath = Path.Combine(AptDataPath, @"backup\");

            // some defaults
            UseExternals = true;
            KeepZips = true;
            DeleteBeforeExtract = true;
            CreateFullBackupBeforeUpgrade = false;
            UnpackPackages = false;
            FetchRequiredDeps = false;
            FetchOptionalDeps = false;

            if (!Directory.Exists(AptDataPath))
            {
                Directory.CreateDirectory(AptDataPath);
            }
            if (!Directory.Exists(AptZipPath))
            {
                Directory.CreateDirectory(AptZipPath);
            }
            if (!Directory.Exists(AptBackupPath))
            {
                Directory.CreateDirectory(AptBackupPath);
            }
        }
    }
}
