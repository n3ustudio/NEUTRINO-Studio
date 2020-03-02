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
        /// <param name="outputDir">The output mono lab file.</param>
        public LabelTask(string workingDir, string inputDir, string outputDir)
        {
            _mainProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = $"{inputDir} {outputDir}",
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = workingDir,
                    FileName = Path.Combine(workingDir, "musicXMLtoLabel.exe"),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };
            Status = TaskStatus.Waiting;
            Target = outputDir;
            Percentage = 0;
            Message = "启动";
        }

        private readonly Process _mainProcess;

        public override void Start()
        {
            if (Status != TaskStatus.Waiting) return;
            _mainProcess.Exited += MainProcessOnExited;
            _mainProcess.OutputDataReceived += MainProcessOnOutputDataReceived;
            _mainProcess.ErrorDataReceived += MainProcessOnErrorDataReceived;
            Status = TaskStatus.Running;
        }

        private void MainProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Message = e.Data;
            Percentage = 1;
            _mainProcess.Dispose();
            Status = TaskStatus.Failed;
        }

        private void MainProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data.Contains("Convert MusicXML to label"))
            {
                Message = "正在转换";
                Percentage = 0.5;
            }
        }

        private void MainProcessOnExited(object sender, EventArgs e)
        {
            _mainProcess.Dispose();
            Status = TaskStatus.Complete;
        }

        public override void Stop()
        {
            if (Status != TaskStatus.Running) return;
            _mainProcess.OutputDataReceived -= MainProcessOnOutputDataReceived;
            _mainProcess.Kill();
            _mainProcess.Dispose();
            Status = TaskStatus.Failed;
        }
    }
}
