using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Task = NeuTask.Task;
using TaskStatus = NeuTask.TaskStatus;

namespace NeutrinoStudio.Core.Tasks
{
    public sealed class LabelTask : Task
    {

        /// <summary>
        /// Create a new label task.
        /// </summary>
        /// <param name="workingDir">The working dir where there is a musicXMLtoLabel executable file.</param>
        /// <param name="inputDir">The input xml(musicxml) file.</param>
        /// <param name="monoOutputDir">The output mono lab file.</param>
        /// <param name="fullOutputDir">The output full lab file.</param>
        public LabelTask(string workingDir, string inputDir, string monoOutputDir, string fullOutputDir)
        {
            _mainProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = $" {inputDir} {fullOutputDir} {monoOutputDir}",
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = workingDir,
                    FileName = Path.Combine(workingDir, "bin\\musicXMLtoLabel.exe"),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };
            Status = TaskStatus.Waiting;
            Target = fullOutputDir;
            Percentage = 0;
            Message = "启动";
        }

        public string Name { get; } = "转换";

        private readonly Process _mainProcess;

        public override void Start()
        {
            if (Status != TaskStatus.Waiting) return;
            _mainProcess.Exited += MainProcessOnExited;
            _mainProcess.OutputDataReceived += MainProcessOnOutputDataReceived;
            _mainProcess.ErrorDataReceived += MainProcessOnErrorDataReceived;
            _mainProcess.Start();
            _mainProcess.BeginErrorReadLine();
            _mainProcess.BeginOutputReadLine();
            Status = TaskStatus.Running;
        }

        private void MainProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            Message = e.Data;
            Percentage = 1;
            _mainProcess.Dispose();
            Status = TaskStatus.Failed;
        }

        private void MainProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null) return;
            if (e.Data.Contains("Convert MusicXML to label"))
            {
                Message = "正在转换";
                Percentage = 0.5;
            }
        }

        private void MainProcessOnExited(object sender, EventArgs e)
        {
            _mainProcess.Dispose();
            Percentage = 1;
            Message = "完成";
            Status = TaskStatus.Complete;
        }

        public override void Stop()
        {
            if (Status != TaskStatus.Running) return;
            try
            {
                _mainProcess.OutputDataReceived -= MainProcessOnOutputDataReceived;
                _mainProcess.Kill();
                _mainProcess.Dispose();
            }
            catch (Exception e)
            {
                // ignored
            }

            Status = TaskStatus.Failed;
        }
    }
}
