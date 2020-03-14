using NeuTask;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Task = NeuTask.Task;

namespace NeutrinoStudio.Core.Tasks
{
    public sealed class OutputTask : Task
    {

        /// <summary>
        /// Create a new output task.
        /// </summary>
        /// <param name="workingDir">The working dir where there is a musicXMLtoLabel executable file.</param>
        /// <param name="f0InputDir">F0 file.</param>
        /// <param name="mgcInPutDir">Mgc file.</param>
        /// <param name="bapInputDir">Bap file.</param>
        /// <param name="outputFile">Output file.</param>
        public OutputTask(
            string workingDir,
            string f0InputDir,
            string mgcInPutDir,
            string bapInputDir,
            string outputFile)
        {
            _mainProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = $" {f0InputDir} {mgcInPutDir} {bapInputDir} -o {outputFile} -t",
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = workingDir,
                    FileName = Path.Combine(workingDir, "bin\\WORLD.exe"),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };
            Status = TaskStatus.Waiting;
            Target = outputFile;
            Percentage = 0;
            Message = "启动";
        }

        public string Name { get; } = "合成";

        private readonly Process _mainProcess;

        public override void Start()
        {
            if (Status != TaskStatus.Waiting) return;
            if (File.Exists(Target)) File.Delete(Target);
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
            if (e.Data.Contains("Synthesis"))
            {
                Message = "正在合成";
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
