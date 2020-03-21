using System.IO;
using System.IO.Compression;

namespace NeutrinoStudio.FileConverter.Core
{

    internal static class ZipUtil
    {

        public static string Unzip(string fileName)
        {

            string path = Path.ChangeExtension(fileName, "");
            if (path is null)
                throw new NeutrinoStudioFileConverterFileException(
                    "File name error when unzipping. Please check the file name.");
            Directory.CreateDirectory(path);
            ZipFile.ExtractToDirectory(fileName, path);
            return path;

        }

        public static void ZipVpr(DirectoryInfo tempDirectory, string targetFileName, string jsonFileName)
        {
            DirectoryInfo outputDirectory = tempDirectory.CreateSubdirectory("output");
            DirectoryInfo sequenceDirectory = outputDirectory.CreateSubdirectory("Project");
            File.Copy(jsonFileName, Path.Combine(sequenceDirectory.FullName, jsonFileName), true);
            ZipFile.CreateFromDirectory(outputDirectory.FullName, targetFileName, CompressionLevel.NoCompression, false);
        }

    }

}
