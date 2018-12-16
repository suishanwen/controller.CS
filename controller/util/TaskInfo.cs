using System;
using System.Collections.Generic;

namespace controller.util
{
    class TaskInfo
    {

        public string ProjectName { get; set; }
        public double Price { get; set; }

        public TaskInfo(string projectName, double price)
        {
            ProjectName = projectName;
            Price = price;
        }
    }

    class TaskInfos
    {
        public static string pathName = "";

        public static void Init(string iniPathName)
        {
            pathName = iniPathName;
        }

        public static void Clear(int vm)
        {
            Dictionary<int, TaskInfo> taskInfoDict = Get();
            if (taskInfoDict.ContainsKey(vm))
            {
                taskInfoDict.Remove(vm);
            }
            Set(taskInfoDict);
        }

        public static void Set(Dictionary<int, TaskInfo> taskInfoDict)
        {
            string taskInfos = "";
            foreach (int key in taskInfoDict.Keys)
            {
                taskInfos += string.Format("{0}:{1}-{2}|", key.ToString(), taskInfoDict[key].ProjectName, taskInfoDict[key].Price.ToString());
            }
            if (taskInfos.Length > 0)
            {
                taskInfos = taskInfos.Substring(0, taskInfos.Length - 1);
            }
            IniReadWriter.WriteIniKeys("Command", "TaskInfos", taskInfos, pathName);
        }

        public static Dictionary<int, TaskInfo> Get()
        {
            Dictionary<int, TaskInfo> taskInfoDict = new Dictionary<int, TaskInfo>();
            try
            {
                string taskInfos = IniReadWriter.ReadIniKeys("Command", "TaskInfos", pathName).Trim();
                if (taskInfos.Length > 0)
                {
                    string[] taskInfoArray = taskInfos.Split('|');
                    foreach (string taskInfo in taskInfoArray)
                    {
                        int key = int.Parse(taskInfo.Substring(0, taskInfo.IndexOf(":")));
                        string[] task = taskInfo.Substring(taskInfo.IndexOf(":") + 1).Split('-');
                        taskInfoDict.Add(key, new TaskInfo(task[0], double.Parse(task[1])));
                    }
                }
            }
            catch (Exception) { }
            return taskInfoDict;
        }

        public static void Update(List<VoteProject> voteProjectList) {
            Dictionary<int, TaskInfo> taskInfoDict = Get();
            foreach (int key in taskInfoDict.Keys)
            {
                TaskInfo taskInfo = taskInfoDict[key];
                for(int i=0;i< voteProjectList.Count; i++)
                {
                    if(voteProjectList[i].ProjectName == taskInfo.ProjectName)
                    {
                        taskInfo.Price = voteProjectList[i].Price;
                        break;
                    }
                }
            }
            Set(taskInfoDict);
        }

        public static string Active()
        {
            Dictionary<String, List<String>> taskInfoVMDict = new Dictionary<String, List<String>>();
            Dictionary<int, TaskInfo> taskInfoDict = Get();
            foreach (int key in taskInfoDict.Keys)
            {
               TaskInfo taskInfo = taskInfoDict[key];
                if (taskInfoVMDict.ContainsKey(taskInfo.ProjectName))
                {
                    taskInfoVMDict[taskInfo.ProjectName].Add(key.ToString());
                }
                else
                {
                    List<String> list = new List<String>();
                    list.Add(key.ToString());
                    taskInfoVMDict.Add(taskInfo.ProjectName, list);
                }
            }
            if(taskInfoVMDict.Keys.Count > 0)
            {
                List<string> projectList = new List<string>();
                foreach (string projectName in taskInfoVMDict.Keys)
                {
                    projectList.Add(string.Format("{0}({1}):{2}", projectName,
                        taskInfoDict[int.Parse(taskInfoVMDict[projectName][0])].Price,
                        string.Join(",", taskInfoVMDict[projectName].ToArray())));
                }
                return string.Join("\n", projectList.ToArray());
            }
            else
            {
                return "无";
            }
        }


    }
}
