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

namespace WowAce.AptCore
{   
    public struct RepositoryAddonInfo
    {
        public string Name;
        public string Description;
        public string Author;
        public string Category;
        public string CommentsUrl;
        public string EnclosureUrl;
        public string Guid;
        public string PubDate;
        public AddonVersionNumber Version;
        public int InterfaceVersion;
        public string Link;
        public bool IsStable;
        public bool IsPackage;
        public string ZipFileName;
        public int ZipFileSize;
        public List<string> RequiredDeps;
        public List<string> OptionalDeps;
        public List<string> Provides;
    };

    public struct LocalAddonInfo
    {
        public string Name;
        public AddonVersionNumber Version;
        public List<string> Modules;
    };

    public abstract class AptCommon
    {
        public delegate void DebugMessageEventHandler(string[] message);
        protected static event DebugMessageEventHandler eDebugMessage;
        
        protected AptCommon()
        {
            
        }

        public static void AddDebugListener(DebugMessageEventHandler func)
        {
            eDebugMessage += func;
        }

        protected static void SendDebugMessage(string str1)
        {
            DebugMessageEventHandler copy = eDebugMessage;

            if (copy != null) { copy(new string[] { str1 }); }
        }

        protected static void SendDebugMessage(string str1, string str2)
        {
            DebugMessageEventHandler copy = eDebugMessage;

            if (copy != null) { copy(new string[] { str1, str2 }); }
        }

        protected static void SendDebugMessage(string str1, string str2, string str3)
        {
            DebugMessageEventHandler copy = eDebugMessage;

            if (copy != null) { copy(new string[] { str1, str2, str3 }); }
        }

        protected static void SendDebugMessage(string str1, string str2, string str3, string str4)
        {
            DebugMessageEventHandler copy = eDebugMessage;

            if (copy != null) { copy(new string[] { str1, str2, str3, str4 }); }
        }
    }
}
