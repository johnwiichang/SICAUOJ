using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace JudgeService
{
    public partial class Judger : ServiceBase
    {
        Int32 MaxTask = 0;
        String HostName = "";
        Boolean isStop = false;
        String WorkDir = "";
        Int32 Heartbeat = 5000;
        List<int> tasks = new List<int>();
        System.Threading.Thread tr;

        public Judger()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            using (TaskRequestServ.TaskPoolClient tp = new TaskRequestServ.TaskPoolClient())
            {
                HostName = GetType().Assembly.Location.Split('\\').LastOrDefault().Substring(1).Replace(".exe", "");
                var WCFBack = tp.IamOnline(HostName, "r");
                if (WCFBack["Err"] != "")
                {
                    throw new Exception("Instance already exist.");
                }
                MaxTask = Int32.Parse(WCFBack["MaxTask"]);
                WorkDir = WCFBack["WorkDir"];
                Heartbeat = Int32.Parse(WCFBack["Heartbeat"]);
                tp.Close();
            }
            tr = new System.Threading.Thread(new System.Threading.ThreadStart(() => StartWork()));
            tr.Start();
        }

        protected override void OnStop()
        {
            isStop = true;
            while (tasks.Count != 0)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void StartWork()
        {
            while (!isStop)
            {
                while (MaxTask - tasks.Count > 0 && !isStop)
                {
                    Dictionary<string, string> WCFBack = null;
                    try
                    {
                        using (TaskRequestServ.TaskPoolClient tp = new TaskRequestServ.TaskPoolClient())
                        {
                            WCFBack = tp.GetTaskRun(HostName, tasks.Count);
                            tp.Close();
                        }
                        if (WCFBack != null)
                        {
                            if (WCFBack["Usage"] == "Work")
                            {
                                Int32 id = Int32.Parse(WCFBack["Id"]);
                                String runnerpatth = WCFBack["RunnerPath"];
                                String runnerargs = WCFBack["RunnerArgs"];
                                Int64 mem = Int64.Parse(WCFBack["Mem"]);
                                Int32 runtimelimit = Int32.Parse(WCFBack["RunTimeLimit"]);
                                String executionformat = WCFBack["ExecutionFormat"];
                                tasks.Add(id);
                                Judge(id, runnerpatth, runnerargs, executionformat, runtimelimit, mem, WCFBack["Input"], WCFBack["Output"]);
                            }
                            else
                            {
                                MaxTask = Int32.Parse(WCFBack["MaxTask"]);
                                WorkDir = WCFBack["WorkDir"];
                                Heartbeat = Int32.Parse(WCFBack["Heartbeat"]);
                                System.Threading.Thread.Sleep(Heartbeat);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ;
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        private Dictionary<String, String> Sentence(String fromProcess, String output)
        {
            var retVal = new Dictionary<String, String>();
            if (output.Contains("\r\n[OR]\r\n"))
            {
                var result = "";
                foreach (var item in output.Split(new String[] { "\r\n[OR]\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var ret = Sentence(fromProcess, item);
                    if (ret["Sentence"] == "YES")
                    {
                        retVal.Add("Sentence", "YES");
                        retVal.Add("Result", "AC");
                        return retVal;
                    }
                    else
                    {
                        result = ret["Result"];
                        continue;
                    }
                }
                retVal.Add("Sentence", "NO");
                retVal.Add("Result", result);
                return retVal;
            }
            if (fromProcess == output || fromProcess == output + "\r\n")
            {
                retVal.Add("Sentence", "YES");
                retVal.Add("Result", "AC");
                return retVal;
            }
            else if (fromProcess.Replace(" ", "").Replace("\n", "").Replace("\r", "") == output.Replace(" ", "").Replace("\n", "").Replace("\r", ""))
            {
                retVal.Add("Sentence", "NO");
                retVal.Add("Result", "PE");
                return retVal;
            }
            else
            {
                retVal.Add("Sentence", "NO");
                retVal.Add("Result", "WA");
                return retVal;
            }
        }

        private void updateTaskRunningResult(int id, String reply, Double runtime, long mem, string status, bool ispass)
        {
            while (true)
            {
                try
                {
                    using (TaskRequestServ.TaskPoolClient tp = new TaskRequestServ.TaskPoolClient())
                    {
                        tp.updateTaskRunningResult(id, reply, runtime, mem, status, ispass, tasks.Count);
                        tp.Close();
                    }
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(3000);
                    continue;
                }
                return;
            }
        }

        private async Task Judge(Int32 Id, String RunnerPath, String RunnerArgs, String ExecutionFormat, Int32 runtimelimit, Int64 Mem, String input, String output)
        {
            try
            {
                Process rp = new Process();
                rp.StartInfo.UseShellExecute = false;
                rp.StartInfo.RedirectStandardOutput = true;
                rp.StartInfo.RedirectStandardError = true;
                rp.StartInfo.RedirectStandardInput = true;
                rp.StartInfo.WorkingDirectory = WorkDir;
                rp.StartInfo.FileName = RunnerPath.Replace("{id}", "n" + Id.ToString()).Replace("{workdir}", WorkDir);
                if (RunnerArgs != null)
                {
                    rp.StartInfo.Arguments = RunnerArgs.Replace("{id}", "n" + Id.ToString()).Replace("{workdir}", WorkDir);
                }
                rp.StartInfo.ErrorDialog = false;
                rp.StartInfo.CreateNoWindow = true;
                var inputCollection = input.Split(new[] { "\r\n[thisisaseparator]\r\n" }, StringSplitOptions.None);
                var outputCollection = output.Split(new[] { "\r\n[thisisaseparator]\r\n" }, StringSplitOptions.None);
                long peek = 0;
                double runtime = 0;
                String res = "";
                for (int i = 0; i < inputCollection.Length; i++)
                {
                    try
                    {
                        rp.Start();
                    }
                    catch (Exception ex)
                    {
                        if (!rp.HasExited)
                        {
                            try
                            {
                                rp.Kill();
                            }
                            catch (Exception es)
                            {
                                ;
                            }
                        }
                        updateTaskRunningResult(Id, res + "\r\n[thisisaseparator]\r\n" + rp.StandardError.ReadToEnd(), rp.TotalProcessorTime.TotalMilliseconds, peek, "RE", false);
                        tasks.Remove(Id);
                        return;
                    }
                    if (rp.HasExited)
                    {
                        break;
                    }
                    peek = peek > rp.PrivateMemorySize64 ? peek : rp.PrivateMemorySize64;
                    rp.StandardInput.WriteLine(inputCollection[i]);
                    if (!rp.WaitForExit(runtimelimit))
                    {
                        rp.Kill();
                        updateTaskRunningResult(Id, res + "\r\n[thisisaseparator]\r\n" + "TimeOut", rp.TotalProcessorTime.TotalMilliseconds, peek, "TLE", false);
                        tasks.Remove(Id);
                        return;
                    }
                    else
                    {
                        runtime = runtime > rp.TotalProcessorTime.TotalMilliseconds ? runtime : rp.TotalProcessorTime.TotalMilliseconds;
                        if (peek > Mem)
                        {
                            updateTaskRunningResult(Id, res + "\r\n[thisisaseparator]\r\n" + "OutOfMemory", runtime, peek, "MLE", false);
                            tasks.Remove(Id);
                            return;
                        }
                        var fromProcess = rp.StandardOutput.ReadToEnd();
                        var perr = rp.StandardError.ReadToEnd();
                        if (perr.Length != 0)
                        {
                            updateTaskRunningResult(Id, res + "\r\n[thisisaseparator]\r\n" + perr, runtime, peek, "RE", false);
                            tasks.Remove(Id);
                            return;
                        }
                        var ret = Sentence(fromProcess, outputCollection[i]);
                        res += (res != "" ? "\r\n[thisisaseparator]\r\n" : "") + fromProcess;
                        if (ret["Sentence"] == "NO")
                        {
                            updateTaskRunningResult(Id, res, runtime, peek, ret["Result"], false);
                            tasks.Remove(Id);
                            return;
                        }
                    }
                }
                updateTaskRunningResult(Id, res, runtime, peek, "AC", true);
                tasks.Remove(Id);
            }
            catch (Exception ex)
            {
                updateTaskRunningResult(Id, ex.Message + "\r\n" + ex.Source + "\r\n" + ex.TargetSite, 0f, 0, "SE", false);
                tasks.Remove(Id);
            }
        }
    }
}
