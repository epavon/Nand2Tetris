using JackCompiler.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    class JackAnalyzer
    {
        static void Main(string[] args)
        {
            List<string> keywords = SettingsReader.Keywords;

            if (args.Length < 1)
            {
                Console.WriteLine("This program expects 1 argument consisting of name of Jack file to compile or the folder containing Jack files.");
                return;
            }

            FileAttributes attr = File.GetAttributes(args[0]);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                string[] jackFiles = Directory.GetFiles(args[0], "*.jack");
                string folderName = Path.GetFileName(args[0]);
                CompileDirectory(folderName, jackFiles);
            }
            else
            {
                string inputFile = args[0];
                CompileFile(inputFile);
            }

            //Console.ReadLine();
        }

        static void CompileFile(string inputFile)
        {
            string outputFile = inputFile.Remove(inputFile.IndexOf(".jack")) + ".vm";

            Tokenizer tokenizer = new Tokenizer(inputFile);
            CompilationEngine compilationEngine = new CompilationEngine(tokenizer, new XmlTokenWriter(outputFile));
            compilationEngine.CompileFile();

            Console.WriteLine("Done! Output File: " + outputFile);
        }

        static void CompileDirectory(string folderName, string[] inputFiles)
        {
            string outputFile = folderName + "/" + folderName + ".asm";

            foreach (var file in inputFiles)
            {
                var fileName = Path.GetFileName(file);
                CompileFile(folderName + "/" + fileName);
            }
            
        }
    }
}
