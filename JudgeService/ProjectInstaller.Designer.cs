namespace RunService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.OJ_Judge_JudgerInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SICAU_OJ_Judger = new System.ServiceProcess.ServiceInstaller();
            // 
            // OJ_Judge_JudgerInstaller
            // 
            this.OJ_Judge_JudgerInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.OJ_Judge_JudgerInstaller.Password = null;
            this.OJ_Judge_JudgerInstaller.Username = null;
            // 
            // SICAU_OJ_Judger
            // 
            this.SICAU_OJ_Judger.DelayedAutoStart = true;
            this.SICAU_OJ_Judger.Description = "Determine whether the program is correct or not.";
            this.SICAU_OJ_Judger.DisplayName = "SICAU OJ Judge Service";
            this.SICAU_OJ_Judger.ServiceName = "OJ Judge Runner";
            this.SICAU_OJ_Judger.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.OJ_Judge_JudgerInstaller,
            this.SICAU_OJ_Judger});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller OJ_Judge_JudgerInstaller;
        private System.ServiceProcess.ServiceInstaller SICAU_OJ_Judger;
    }
}