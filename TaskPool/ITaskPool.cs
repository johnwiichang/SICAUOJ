using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;

namespace TaskPool
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“ITaskPool”。
    [ServiceContract]
    public interface ITaskPool
    {
        [OperationContract]
        Dictionary<String, String> GetTask(String host, Int32 tasks);

        [OperationContract]
        Dictionary<String, String> GetTaskRun(String host, Int32 tasks);

        [OperationContract]
        void updateTaskRunningResult(Int32 Id, String Reply, Double Runtime, Int64 Mem, String Status, Boolean isPass, Int32 tasks);

        [OperationContract]
        void updateTaskCompilingResult(Int32 Id, String Status, Double CompileTime, String Reply, Int32 tasks);

        [OperationContract]
        Dictionary<String, String> IamOnline(String Id, String IPAddress, long RAM, int Cores, String SystemInfo, String type);
    }
}
