using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace NugetPushTool
{
    internal class FileVersionCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            Regex prefix = new Regex(@"(?<!\.)\b(\w+\.)+(?=\b(\d+\.){4}nupkg\b)");
            Regex versionRegex = new Regex(@"(?<=(\w\.)+)(\d+\.){4}(?=nupkg\b)");

            //提取文件前缀
            var prefix_x = prefix.Match(x);
            var prefix_y = prefix.Match(y);
            if (prefix_x.Success && prefix_y.Success)
            {
                if (prefix_x.Value != prefix_y.Value)
                {
                    return x.CompareTo(y);
                }
            }
            else
            {
                return x.CompareTo(y);
            }

            //提取版本号
            var version_x = versionRegex.Match(x);
            var version_y = versionRegex.Match(y);
            if (version_x.Success && version_y.Success)
            {
                return version_x.Value.CompareTo(version_y.Value);
            }
            else
            {
                return x.CompareTo(y);
            }
        }
    }
}
