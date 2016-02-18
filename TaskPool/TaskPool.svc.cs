using OJ_WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace TaskPool
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“TaskPool”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 TaskPool.svc 或 TaskPool.svc.cs，然后开始调试。
    [ServiceBehavior]
    public class TaskPool : ITaskPool
    {
        OJ_WebAppContext entity = new OJ_WebAppContext();

        public Dictionary<String, String> GetTask(String host, Int32 tasks)
        {
            var node = entity.Nodes.Find(host);
            if (!node.inUse)
            {
                return null;
            }
            var taskinfo = new Dictionary<String, String>();
            Task t = (from x in entity.Tasks where x.status == "WC" select x).FirstOrDefault();
            if (t == null)
            {
                taskinfo.Add("Usage", "Configuration");
                taskinfo.Add("MaxTask", node.MaxTask.ToString());
                taskinfo.Add("Heartbeat", node.Heartbeat.ToString());
                taskinfo.Add("WorkDir", node.WorkDir);
            }
            else
            {
                tasks++;
                t.Handler = host;
                t.status = "CA";
                t = ValidTask(t);
                if (entity.Tasks.Find(t.Id).status != t.status)
                {
                    return null;
                }
                taskinfo.Add("Usage", "Work");
                taskinfo.Add("Id", t.Id.ToString());
                taskinfo.Add("CompilerPath", t.Compiler.CompilerPath);
                taskinfo.Add("CompilerArgs", t.Compiler.CompilerArgs);
                taskinfo.Add("CodeFormat", t.Compiler.CodeFormat);
                taskinfo.Add("IsScript", t.Compiler.isScript.ToString());
                taskinfo.Add("CompileLimit", t.Issue.ComplieTime.ToString());
                taskinfo.Add("ExecutionFormat", t.Compiler.ExecutionFormat);
                taskinfo.Add("Code", t.Answer.Replace("[thisisanojanswertosicau]", "n" + t.Id));
            }
            node.CompilerLastReport = DateTime.Now;
            node.Compiling = tasks;
            entity.SaveChanges();
            return taskinfo;
        }

        public Dictionary<String, String> GetTaskRun(String host, Int32 tasks)
        {
            var node = entity.Nodes.Find(host);
            var taskinfo = new Dictionary<String, String>();
            Task t = (from x in entity.Tasks where x.status == "CS" && x.Handler == host select x).FirstOrDefault();
            if (t == null)
            {
                taskinfo.Add("Usage", "Configuration");
                taskinfo.Add("MaxTask", node.MaxTask.ToString());
                taskinfo.Add("Heartbeat", node.Heartbeat.ToString());
                taskinfo.Add("WorkDir", node.WorkDir);
            }
            else
            {
                tasks++;
                taskinfo.Add("Usage", "Work");
                t.status = "RA";
                t = ValidTask(t);
                if (entity.Tasks.Find(t.Id).status != t.status)
                {
                    return null;
                }
                taskinfo.Add("Id", t.Id.ToString());
                taskinfo.Add("RunnerPath", t.Compiler.RunnerPath);
                taskinfo.Add("RunnerArgs", t.Compiler.RunnerArgs);
                taskinfo.Add("Mem", t.Issue.PrivateMemorySize.ToString());
                taskinfo.Add("RunTimeLimit", t.Issue.RunTime.ToString());
                taskinfo.Add("Input", t.Issue.Input);
                taskinfo.Add("Output", t.Issue.Output);
                taskinfo.Add("ExecutionFormat", t.Compiler.ExecutionFormat);
            }
            node.RunnerLastReport = DateTime.Now;
            node.Running = tasks;
            entity.SaveChanges();
            return taskinfo;
        }

        public void updateTaskRunningResult(Int32 Id, String Reply, Double Runtime, Int64 Mem, String Status, Boolean isPass, Int32 tasks)
        {
            var t = entity.Tasks.Find(Id);
            t = ValidTask(t);
            t.Reply = Reply;
            t.Runtime = Runtime;
            t.Mem = Mem;
            t.status = Status;
            t.isPass = isPass;
            var n = entity.Nodes.Find(t.Handler);
            n.RunnerLastReport = DateTime.Now;
            n.Running = tasks;
            entity.SaveChanges();
        }

        public void updateTaskCompilingResult(Int32 Id, String Status, Double CompileTime, String Reply, Int32 tasks)
        {
            var t = entity.Tasks.Find(Id);
            t = ValidTask(t);
            t.Reply = Reply;
            t.Compiletime = CompileTime;
            t.status = Status;
            var n = entity.Nodes.Find(t.Handler);
            n.CompilerLastReport = DateTime.Now;
            n.Compiling = tasks;
            entity.SaveChanges();
        }

        private Task ValidTask(Task t)
        {
            t.Owner = t.Owner;
            t.Issue = t.Issue;
            t.Compiler = t.Compiler;
            return t;
        }

        public Dictionary<string, string> IamOnline(string Id, string IPAddress, long RAM, int Cores, string SystemInfo, string type)
        {
            var node = entity.Nodes.Find(Id);
            var taskinfo = new Dictionary<String, String>();
            if (node == null)
            {
                node = new Node() { Name = Id, IPAddress = IPAddress, RAM = RAM, Cores = Cores, SystemInfo = SystemInfo };
                entity.Nodes.Add(node);
            }
            else if ((type == "r" && node.RunnerLastReport > DateTime.Now.AddSeconds(-10)) || (type == "c" && node.CompilerLastReport > DateTime.Now.AddSeconds(-10)))
            {
                taskinfo.Add("Err", "Err");
                return taskinfo;
            }
            else if ((node.RunnerLastReport > DateTime.Now.AddSeconds(-10) || node.CompilerLastReport > DateTime.Now.AddSeconds(-10)) && node.IPAddress != IPAddress)
            {
                taskinfo.Add("Err", "Err");
                return taskinfo;
            }
            if (type == "c")
            {
                node.CompilerLastReport = DateTime.Now;
                node.Compiling = 0;
            }
            else
            {
                node.RunnerLastReport = DateTime.Now;
                node.Running = 0;
            }
            node.RAM = RAM;
            node.IPAddress = IPAddress;
            node.SystemInfo = SystemInfo;
            node.Cores = Cores;
            entity.SaveChanges();
            taskinfo.Add("MaxTask", node.MaxTask.ToString());
            taskinfo.Add("WorkDir", node.WorkDir);
            taskinfo.Add("Heartbeat", node.Heartbeat.ToString());
            taskinfo.Add("Err", "");
            return taskinfo;
        }
    }
}
