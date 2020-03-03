using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NeuTask;
using Task = NeuTask.Task;

namespace NeutrinoStudio.Core.Tasks
{
    public sealed class SynthTask : Task
    {

        /// <summary>
        /// Create a new synth task.
        /// </summary>
        /// <param name="workingDir">The working dir where there is a musicXMLtoLabel executable file.</param>
        /// <param name="fullInputDir">Full lab file.</param>
        /// <param name="timingOutputDir">Timing lab file.</param>
        /// <param name="f0OutputDir">F0 file.</param>
        /// <param name="mgcOutPutDir">Mgc file.</param>
        /// <param name="bapOutputDir">Bap file.</param>
        /// <param name="modelDir">Model dir.</param>
        public SynthTask(
            string workingDir,
            string fullInputDir,
            string timingOutputDir,
            string f0OutputDir,
            string mgcOutPutDir,
            string bapOutputDir,
            string modelDir)
        {
            _mainProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = $" {fullInputDir} {timingOutputDir} {f0OutputDir} {mgcOutPutDir} {bapOutputDir} {modelDir}\\ -t",
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    WorkingDirectory = workingDir,
                    FileName = Path.Combine(workingDir, "bin\\NEUTRINO.exe"),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };
            Status = TaskStatus.Waiting;
            Target = f0OutputDir;
            Percentage = 0;
            Message = "启动";
        }

        public string Name { get; } = "生成";

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
            if (e.Data.Contains("Predict timing"))
            {
                Message = "正在生成时间";
                Percentage = 0.2;
            }
            else if (e.Data.Contains("Predict acoustic features"))
            {
                Message = "正在生成";
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
