using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace CompileService
{
    public partial class Compiler : ServiceBase
    {
        Int32 MaxTask = 0;
        Int32 Heartbeat = 5000;
        String HostName = "";
        Boolean isStop = false;
        String WorkDir = "";
        List<int> tasks = new List<int>();
        System.Threading.Thread tr;

        public Compiler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            using (TaskRequestServ.TaskPoolClient tp = new TaskRequestServ.TaskPoolClient())
            {
                HostName = GetType().Assembly.Location.Split('\\').LastOrDefault().Substring(1).Replace(".exe", "");
                var WCFBack = tp.IamOnline(HostName, Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(x=>x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().ToString(), getRAM(), Environment.ProcessorCount, Environment.OSVersion.ToString(), "c");
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
                            WCFBack = tp.GetTask(HostName, tasks.Count);
                            tp.Close();
                        }
                        if (WCFBack != null)
                        {
                            if (WCFBack["Usage"] == "Work")
                            {
                                Int32 id = Int32.Parse(WCFBack["Id"]);
                                String compilerpath = WCFBack["CompilerPath"];
                                String compilerargs = WCFBack["CompilerArgs"];
                                String codeformat = WCFBack["CodeFormat"];
                                String executionformat = WCFBack["ExecutionFormat"];
                                Boolean isscript = Boolean.Parse(WCFBack["IsScript"]);
                                Int32 compilelimit = Int32.Parse(WCFBack["CompileLimit"]);
                                String code = WCFBack["Code"];
                                tasks.Add(id);
                                Judge(id, compilerpath, compilerargs, code, codeformat, executionformat, isscript, compilelimit);
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
                    catch (Exception)
                    {
                        ;
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void updateTaskCompilingResult(int id, String status, Double time, String Reply)
        {
            while (true)
            {
                try
                {
                    using (TaskRequestServ.TaskPoolClient tp = new TaskRequestServ.TaskPoolClient())
                    {
                        tp.updateTaskCompilingResult(id, status, time, Reply, tasks.Count);
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

        private async Task Judge(Int32 Id, String CompilerPath, String CompilerArgs, String Code, String CodeFormat, String ExecutionFormat, Boolean IsScript, Int32 compilelimit)
        {
            try
            {
                String filePath = WorkDir + "n" + Id.ToString() + CodeFormat;
                String outfileName = WorkDir + "n" + Id.ToString() + ExecutionFormat;
                System.IO.File.WriteAllText(WorkDir + "n" + Id.ToString() + CodeFormat, Code);
                if (IsScript)
                {
                    if (System.IO.File.Exists(WorkDir + "n" + Id.ToString() + CodeFormat))
                    {
                        updateTaskCompilingResult(Id, "CS", 0f, "");
                        tasks.Remove(Id);
                        return;
                    }
                    else
                    {
                        updateTaskCompilingResult(Id, "CE", 0f, "");
                        tasks.Remove(Id);
                        return;
                    }
                }
                Process cp = new Process();
                cp.StartInfo.UseShellExecute = false;
                cp.StartInfo.RedirectStandardOutput = true;
                cp.StartInfo.RedirectStandardError = true;
                cp.StartInfo.RedirectStandardInput = true;
                cp.StartInfo.WorkingDirectory = WorkDir;
                cp.StartInfo.FileName = CompilerPath.Replace("{id}", "n" + Id.ToString()).Replace("{workdir}", WorkDir);
                if (CompilerArgs != null)
                {
                    cp.StartInfo.Arguments = CompilerArgs.Replace("{id}", "n" + Id.ToString()).Replace("{workdir}", WorkDir);
                }
                cp.StartInfo.CreateNoWindow = true;
                cp.StartInfo.ErrorDialog = false;
                cp.Start();
                if (!cp.WaitForExit(compilelimit))
                {
                    cp.Kill();
                    String erro = cp.StandardError.ReadToEnd();
                    updateTaskCompilingResult(Id, "TLE", cp.TotalProcessorTime.TotalMilliseconds, erro.Length != 0 ? erro : cp.StandardOutput.ReadToEnd());
                    tasks.Remove(Id);
                    return;
                }
                if (!System.IO.File.Exists(outfileName))
                {
                    String erro = cp.StandardError.ReadToEnd();
                    updateTaskCompilingResult(Id, "CE", cp.TotalProcessorTime.TotalMilliseconds, erro.Length != 0 ? erro : cp.StandardOutput.ReadToEnd());
                    tasks.Remove(Id);
                    return;
                }
                else
                {
                    String erro = cp.StandardError.ReadToEnd();
                    updateTaskCompilingResult(Id, "CS", cp.TotalProcessorTime.TotalMilliseconds, erro.Length != 0 ? erro : cp.StandardOutput.ReadToEnd());
                    tasks.Remove(Id);
                }
            }
            catch (Exception ex)
            {
                updateTaskCompilingResult(Id, "SE", 0f, ex.Message + "\r\n" + ex.Source + "\r\n" + ex.TargetSite);
                tasks.Remove(Id);
            }
        }

        private static long getRAM()
        {
            try
            {
                string st = "";
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    st = mo["TotalPhysicalMemory"].ToString();
                }
                moc = null;
                mc = null;
                return (Int64.Parse(st) / 1024 / 1024);
            }
            catch
            {
                return 0;
            }
            finally { }
        }
    }
}
