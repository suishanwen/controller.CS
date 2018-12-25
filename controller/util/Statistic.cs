using System;
using System.Collections.Generic;
using System.Text;

namespace controller.util
{
    class Statistic
    {
        public static string GenerateStatistic()
        {
            var content = "<html>\n<body>\n<div>";
            content += "\n<h2>收益统计</h2>";
            var path = $"{Form1.GetPathShare()}/Statistic.ini";
            List<string> sectionList =  IniSections.ReadSections(path);
            sectionList.Sort((a, b) =>  int.Parse(a).CompareTo(int.Parse(b)));
            double reward = 0;
            sectionList.ForEach(section =>
            {
                double subReward = 0;
                content += "\n<div>";
                content += $"\n<h3>{section}号</h3>";
                content += $"\n<table>";
                List<string> keyList = IniSections.ReadSingleSection(section, path);
                keyList.Sort((a, b) => a.CompareTo(b));
                keyList.ForEach(key =>
                {
                    string[] info = key.Split('|');
                    string name = info[0];
                    double price = double.Parse(info[1]);
                    int succ = int.Parse(IniReadWriter.ReadIniKeys(section, key, path));
                    double vmReward = price * succ/100;
                    subReward += vmReward;
                    content += "\n<tr>";
                    content += $"\n<td style='width:200px'>{name}</td>";
                    content += $"\n<td style='width:100px'>{price}</td>";
                    content += $"\n<td style='width:100px'>{succ}</td>";
                    content += $"\n<td style='width:100px'>{vmReward}</td>";
                    content += "\n</tr>";
                });
                content += "\n<tr>";
                content += "\n<td colspan='3'>合计</td>";
                content += $"\n<td>{subReward}</td>";
                content += "\n</tr>";
                content += "\n</table>";
                content += "\n</div><br/>";
                reward += subReward;
                IniReadWriter.WriteIniKeys(section, null, null, path);
            });
            content += $"\n<h2>合计:{reward}</h2>";
            content += "\n</div>\n</body>\n</html>";
            return content;
        }
    }
}
