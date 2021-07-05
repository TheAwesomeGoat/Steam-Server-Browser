using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEST.PacketBuilder
{
    class PacketBuilder2
    {
        public enum EnumRegions
        {
            UsEastCoast = 0x00,
            UsWestCoast = 0x01,
            SouthAmerica = 0x02,
            Europe = 0x03,
            Asia = 0x04,
            Australia = 0x05,
            MiddleEast = 0x06,
            Africa = 0x07,
            All = 0xFF
        }
        public enum SourceEngineGames
        {
            Half_Life2 = 220,
            Counter_Strike_Source = 240,
            Day_Of_Defeat = 300,
            Half_Life2_Deathmatch = 320,
            Team_Fortress_2 = 440,
            Left_4_Dead_2 = 500,
            Counter_Strike_Global_Offensive = 730,
            Garrys_Mod = 4000,
            Insurgency = 17700
        }
        public string BuildPacket(string first, string region,IpFilter filter)
        {
            int ParsedRegion = (int)Enum.Parse(typeof(EnumRegions), region);
            string fil = ProcessFilter(filter);
            return $"1{(char)ParsedRegion}{first}\0{fil}\0";
        }
        string ProcessFilter(IpFilter filter, bool isSubFilter = false)
        {
            StringBuilder filterStr = new StringBuilder();
            if (filter.IsDedicated)
                filterStr.Append(@"\type\d");
            if (filter.IsSecure)
                filterStr.Append(@"\secure\1");
            if (!string.IsNullOrEmpty(filter.GameDirectory))
                filterStr.Append(@"\gamedir\" + filter.GameDirectory);
            if (!string.IsNullOrEmpty(filter.Map))
                filterStr.Append(@"\map\" + filter.Map);
            if (filter.IsLinux)
                filterStr.Append(@"\linux\1");
            if (filter.IsNotEmpty)
                filterStr.Append(@"\empty\1");
            if (filter.IsNotFull)
                filterStr.Append(@"\full\1");
            if (filter.IsProxy)
                filterStr.Append(@"\proxy\1");
            if (filter.NAppId != null)
                filterStr.Append(@"\napp\" + filter.NAppId);
            if (filter.IsNoPlayers)
                filterStr.Append(@"\noplayers\1");
            if (filter.IsWhiteListed)
                filterStr.Append(@"\white\1");
            if (!string.IsNullOrEmpty(filter.Tags))
                filterStr.Append(@"\gametype\" + filter.Tags);
            if (!string.IsNullOrEmpty(filter.HiddenTagsAll))
                filterStr.Append(@"\gamedata\" + filter.HiddenTagsAll);
            if (!string.IsNullOrEmpty(filter.HiddenTagsAny))
                filterStr.Append(@"\gamedataor\" + filter.HiddenTagsAny);
            if (filter.AppId != null)
                filterStr.Append(@"\appid\" + filter.AppId);
            if (!string.IsNullOrEmpty(filter.HostName))
                filterStr.Append(@"\name_match\" + filter.HostName);
            if (!string.IsNullOrEmpty(filter.Version))
                filterStr.Append(@"\version_match\" + filter.Version);
            if (filter.IsUniqueIPAddress)
                filterStr.Append(@"\collapse_addr_hash\1");
            if (!string.IsNullOrEmpty(filter.IPAddress))
                filterStr.Append(@"\gameaddr\" + filter.IPAddress);
            if (filter.ExcludeAny != null)
            {
                filterStr.Append("\0nor");
                filterStr.Append(ProcessFilter(filter.ExcludeAny, true));
            }
            if (filter.ExcludeAll != null)
            {
                filterStr.Append("\0nand");
                filterStr.Append(ProcessFilter(filter.ExcludeAll, true));
            }
            if (!isSubFilter)
            {
                string[] Parts = null;
                string norStr = string.Empty, nandStr = string.Empty;
                Parts = filterStr.ToString().Split('\0');
                filterStr = new StringBuilder(Parts[0]);
                for (int i = 1; i < Parts.Length; i++)
                {
                    if (Parts[i].StartsWith("nor", StringComparison.OrdinalIgnoreCase))
                    {
                        norStr += Parts[i].Substring(3);
                    }
                    if (Parts[i].StartsWith("nand", StringComparison.OrdinalIgnoreCase))
                    {
                        nandStr += Parts[i].Substring(4);
                    }
                }
                if (!String.IsNullOrEmpty(norStr))
                {
                    filterStr.Append(@"\nor\");
                    filterStr.Append(norStr.Count(x => x == '\\') / 2);
                    filterStr.Append(norStr);
                }
                if (!String.IsNullOrEmpty(nandStr))
                {
                    filterStr.Append(@"\nand\");
                    filterStr.Append(nandStr.Count(x => x == '\\') / 2);
                    filterStr.Append(nandStr);
                }
            }
            return filterStr.ToString();
        }
    }
}
