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
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;

using WowAce.AptCore;

namespace WowAce.AptGet
{
    partial class Program
    {
        public void ShowHelp()
        {
            Console.Clear();

            PrintAppHeader();

            Console.Write("\nWowAceAptGet is a package manager for the WowAce.com 'files' repository.");

            Console.Write("\n\n");
            Console.Write("  Usage:");
            Console.Write("\n    ace-get command [addon1 [addon2 [addon3]]...] [options...] ");

            Console.Write("\n\n");
            Console.Write("  Examples:");
            Console.Write("\n    ace-get help");
            Console.Write("\n    ace-get install AnAddon");
            Console.Write("\n    ace-get install AnAddon AnotherAddon");
            Console.Write("\n    ace-get upgrade --no-ext --debug");
            Console.Write("\n    ace-get upgrade add*");
            Console.Write("\n    ace-get upgrade --exclude=AnAddon --exclude=AnotherAddon");

            Console.Write("\n\n");
            Console.Write("  Commands:");
            Console.Write("\n    help                         Show this help :)");
            Console.Write("\n    update                       Update repository index file");
            Console.Write("\n    upgrade                      Upgrade addon(s)");
            Console.Write("\n    install                      Installs one or more addons");
            Console.Write("\n    remove                       Uninstall one or more addons");
            Console.Write("\n    clear                        Remove SavedVariables of one or more addons");
            Console.Write("\n    show                         Shows some information about an addon");
            Console.Write("\n    search                       Search for an addon");
            Console.Write("\n    pack                         (Re-)pack addons");
            Console.Write("\n    unpack                       Unpack addons");
            Console.Write("\n    changelog                    Print last [n]-commit messages");
            Console.Write("\n    backup                       Backup whole addon directory");
            Console.Write("\n    restore                      Restore a backup");
            Console.Write("\n    info                         Show some internal infos");
            Console.Write("\n    config                       Save passed options");

            Console.Write("\n\n");
            Console.Write("  Options:");
            Console.Write("\n    --ext/--no-ext               Enabled/disable externals");
            Console.Write("\n    --deps/--no-deps             (Don't) fetch required dependencies");
            Console.Write("\n    --pack/--unpack              Pack/unpack addon packages");
            Console.Write("\n    --debug/--no-debug           Enabled/disable debug output");
            Console.Write("\n    --ask/--silent               Proceed with/without user interaction");
            Console.Write("\n    --keep-zips                  Don't delete ZIP archive after extract");
            Console.Write("\n    --exclude=[name]             Exclude a specific addon");
            Console.Write("\n    --update                     Perform an index update before command is executed");
            Console.Write("\n    --sv/--no-sv                 With/without SavedVariables");

            Console.Write("\n");
        }
    }
}
