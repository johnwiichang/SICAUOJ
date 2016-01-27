namespace CompileService
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
            this.OJ_Judge_CompilerInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.OJ_Judge_Compiler = new System.ServiceProcess.ServiceInstaller();
            // 
            // OJ_Judge_CompilerInstaller
            // 
            this.OJ_Judge_CompilerInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.OJ_Judge_CompilerInstaller.Password = null;
            this.OJ_Judge_CompilerInstaller.Username = null;
            // 
            // OJ_Judge_Compiler
            // 
            this.OJ_Judge_Compiler.DelayedAutoStart = true;
            this.OJ_Judge_Compiler.Description = "Compile user-submitted code file.";
            this.OJ_Judge_Compiler.DisplayName = "SICAU OJ Compile Service";
            this.OJ_Judge_Compiler.ServiceName = "OJ_Judge_Compiler";
            this.OJ_Judge_Compiler.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.OJ_Judge_CompilerInstaller,
            this.OJ_Judge_Compiler});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller OJ_Judge_CompilerInstaller;
        private System.ServiceProcess.ServiceInstaller OJ_Judge_Compiler;
    }
}