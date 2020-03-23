using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeutrinoStudio.FileConverter.Core;
using NeutrinoTask = NeuTask.Task;
using NeutrinoTaskStatus = NeuTask.TaskStatus;

namespace NeutrinoStudio.FileConverter.Tasks
{

    /// <summary>
    /// The input task - Convert project files to musicxml.
    /// </summary>
    public sealed class InputTask : NeutrinoTask
    {

        /// <summary>
        /// Create a new input task.
        /// </summary>
        /// <param name="format">The input format.</param>
        /// <param name="inputDir">The input file.</param>
        /// <param name="outputDir">The output file.</param>
        public InputTask(
            InputFormat format,
            string inputDir,
            string outputDir)
        {

            _coreTask = new BackgroundWorker();
            _coreTask.DoWork += (sender, e) =>
            {
                BackgroundWorker bw = sender as BackgroundWorker;
                CoreAction(bw, format, inputDir, outputDir);
                if (bw != null && bw.CancellationPending) e.Cancel = true;
            };

            _coreTask.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {
                    _coreTask.Dispose();
                    Percentage = 1;
                    Message = "取消";
                    Status = NeutrinoTaskStatus.Failed;
                }
                else if (e.Error != null)
                {
                    _coreTask.Dispose();
                    Percentage = 1;
                    Message = e.Error.Message;
                    Status = NeutrinoTaskStatus.Failed;
                }
                else
                {
                    _coreTask.Dispose();
                    Percentage = 1;
                    Message = "完成";
                    Status = NeutrinoTaskStatus.Complete;
                }
            };

            Status = NeutrinoTaskStatus.Waiting;
            Target = outputDir;
            Percentage = 0;
            Message = "启动";

        }

        #region Config

        public new string Name { get; } = "输入";

        #endregion

        #region Task

        private BackgroundWorker _coreTask;

        private void CoreAction(
            BackgroundWorker bw,
            InputFormat format,
            string inputDir,
            string outputDir)
        {
            Converter converter = new Converter();
            switch (format)
            {
                case InputFormat.MusicXml:
                case InputFormat.Xml:
                case InputFormat.Mxl:
                {
                    File.Copy(inputDir, outputDir, true);
                    return;
                }
                case InputFormat.Vsq3:
                    converter.ImportVsq3(new List<string> {inputDir});
                    break;
                case InputFormat.Vsq4:
                    converter.ImportVsq4(new List<string> { inputDir });
                    break;
                case InputFormat.Vpr:
                    converter.ImportVpr(new List<string> { inputDir });
                    break;
                case InputFormat.Ust:
                    converter.ImportUst(new List<string> { inputDir });
                    break;
                case InputFormat.Ccs:
                    converter.ImportCcs(new List<string> { inputDir });
                    break;
                default:
                    throw new NeutrinoStudioFileConverterOperationException("Input format not supported.");
            }
            converter.ExportMusicXml(outputDir);
        }

        #endregion

        #region Interface

        public override void Start()
        {
            if (Status != NeutrinoTaskStatus.Waiting) return;
            Message = "启动";
            Percentage = 0.5;
            _coreTask.RunWorkerAsync();
            Status = NeutrinoTaskStatus.Running;
        }

        public override void Stop()
        {
            if (Status != NeutrinoTaskStatus.Running) return;
            _coreTask.CancelAsync();
            Status = NeutrinoTaskStatus.Failed;
        }

        #endregion

    }

}
