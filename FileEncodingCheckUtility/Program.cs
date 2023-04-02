using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ProgramSynthesis.Detection;

namespace CheckEncodingOfFiles
{
    class Program
    {
        //The path to the folder where files are kept. Please provide comma separated path in case of multiple paths
        static readonly string inputFolder = System.Configuration.ConfigurationManager.AppSettings.Get("InputFolderPath");//@"D:\Projects\ITR\API\ITR.Database\dbo\Upgrade Package\AUG-2021\00-Pre-Upgrade-Scripts,D:\Projects\ITR\API\ITR.Database\dbo\Upgrade Package\AUG-2021\02-Incremental System,D:\Projects\ITR\API\ITR.Database\dbo\Upgrade Package\AUG-2021\04-Post-Requisite Scripts,D:\Projects\ITR\API\ITR.Database\dbo\Upgrade Package\AUG-2021\05-Post-Upgrade-Scripts,D:\Projects\ITR\API\ITR.Database\dbo\Upgrade Package\AUG-2021\01-Schema";
        //The encoding to check and if not found then mark that file as invalid. Please provide comma separated strings in case of multiple encodings
        static readonly string validEncodingtype = System.Configuration.ConfigurationManager.AppSettings.Get("validEncodingtype");
        //The output path for loog file
        static readonly string logFilePath = System.Configuration.ConfigurationManager.AppSettings.Get("logFilePath");
        static void Main(string[] args)
        {
            List<string> lsFolders = inputFolder.Split(',').ToList();
            List<string> lsValidEncodingType = validEncodingtype.Split(",").ToList().ConvertAll(m => m.ToLower());
            Dictionary<string, string> invalidencodingfiles = new Dictionary<string, string>();
            foreach (string folder in lsFolders)
            {
                string[] documents = System.IO.Directory.GetFiles(folder, "*.sql", SearchOption.AllDirectories);
                foreach (string ipFile in documents)
                {
                    var detectedEncoding = DetectEncoding(ipFile);
                    var isValid = lsValidEncodingType.Any(x => detectedEncoding.Contains(x));
                    if (!isValid)
                        invalidencodingfiles.Add(ipFile, detectedEncoding);
                    else
                        Console.WriteLine(ipFile);
                }
            }
            WriteToOutputFile(invalidencodingfiles, logFilePath);
        }

        private static void WriteToOutputFile(Dictionary<string, string> fileEncodings, string FilePath)
        {
            using (FileStream fs = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                using (TextWriter tw = new StreamWriter(fs))

                    foreach (KeyValuePair<string, string> kvp in fileEncodings)
                    {
                        tw.WriteLine(string.Format("{0} - {1}", kvp.Key, kvp.Value));
                    }
            }
        }

        public static string DetectEncoding(string filename)
        {
            string FileEncoding = string.Empty;
            
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var detectedEncoding = Microsoft.ProgramSynthesis.Detection.Encoding.EncodingIdentifier.IdentifyEncoding(file);

                    switch (detectedEncoding)
                    {
                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Utf8:

                            return FileEncoding = Encoding.UTF8.BodyName.ToLower();

                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Utf16Be:

                            return FileEncoding = Encoding.BigEndianUnicode.BodyName.ToLower();

                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Utf16Le:

                            return FileEncoding = Encoding.Unicode.BodyName.ToLower();

                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Utf32Le:

                            return FileEncoding = Encoding.UTF32.BodyName.ToLower();

                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Ascii:

                            return FileEncoding = Encoding.ASCII.BodyName.ToLower();

                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Iso88591:

                            return FileEncoding = Encoding.GetEncoding("ISO-8859-1", EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback).BodyName.ToLower();

                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Unknown:

                        case Microsoft.ProgramSynthesis.Detection.Encoding.EncodingType.Windows1252:

                            return FileEncoding = CodePagesEncodingProvider.Instance.GetEncoding(1252).BodyName.ToLower();

                        default:

                            return FileEncoding = Encoding.Default.BodyName.ToLower();
                    }
                }
                catch (Exception )
                {
                    FileEncoding = "Fail to get encoding for " + filename;
                }
                return FileEncoding;
            }
        }
    }
}