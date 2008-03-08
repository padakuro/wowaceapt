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

namespace WowAce.AptCore
{
    public class AddonVersionNumber
    {
        private int _Major = 0;

        public int Major
        {
            get { return _Major; }
        }
        private int _Minor = 0;

        public int Minor
        {
            get { return _Minor; }
        }

        public static readonly AddonVersionNumber NO_VERSION = new AddonVersionNumber(0, 0);

        public AddonVersionNumber(int Major, int Minor)
        {
            _Major = Major;
            _Minor = Minor;
        }
        public AddonVersionNumber(int Major)
        {
            _Major = Major;
        }

        private enum compare { LESSER, GREATER, EQUAL };

        private compare Compare(AddonVersionNumber with)
        {
            compare c;

            if (_Major > with._Major)
                c = compare.GREATER;
            else if (_Major < with._Major)
                c = compare.LESSER;
            else
            {
                if (_Minor > with._Minor)
                    c = compare.GREATER;
                else if (_Minor < with._Minor)
                    c = compare.LESSER;
                else
                    c = compare.EQUAL;
            }

            return c;
        }

        public override bool Equals(object o)
        {
            if (o is AddonVersionNumber)
                return Compare((AddonVersionNumber)o) == compare.EQUAL;

            return false;
        }

        public override int GetHashCode()
        {
            return _Major << 16 + _Minor;
        }

        public override string ToString()
        {
            if (_Minor > 0)
                return String.Format("{0}.{1}", _Major, _Minor);

            return String.Format("{0}", _Major);
        }

        public static bool operator >=(AddonVersionNumber a, AddonVersionNumber b)
        {
            return (a.Compare(b) == compare.GREATER || a.Compare(b) == compare.EQUAL);
        }
        public static bool operator <=(AddonVersionNumber a, AddonVersionNumber b)
        {
            return (a.Compare(b) == compare.GREATER || a.Compare(b) == compare.EQUAL);
        }
        public static bool operator <(AddonVersionNumber a, AddonVersionNumber b)
        {
            return a.Compare(b) == compare.LESSER;
        }
        public static bool operator >(AddonVersionNumber a, AddonVersionNumber b)
        {
            return a.Compare(b) == compare.GREATER;
        }
        public static bool operator ==(AddonVersionNumber a, AddonVersionNumber b)
        {
            return a.Compare(b) == compare.EQUAL;
        }
        public static bool operator !=(AddonVersionNumber a, AddonVersionNumber b)
        {
            return !(a.Compare(b) == compare.EQUAL);
        }

        public static AddonVersionNumber Parse(string s)
        {
            int min = 0, maj = 0;
            char[] d = { '.' };

            string[] ss = s.Split(d);

            if (ss.GetLength(0) > 0)
            {
                int i = ss.GetLowerBound(0);
                maj = Int32.Parse(ss[i]);
                if (ss.GetLength(0) > 1)
                    min = Int32.Parse(ss[++i]);
            }

            return new AddonVersionNumber(maj, min);
        }
    }
}
