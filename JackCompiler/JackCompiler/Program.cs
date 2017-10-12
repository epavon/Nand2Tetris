using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> keywords = SettingsReader.Keywords;

            if (args.Length < 1)
            {
                Console.WriteLine("This program expects 1 argument consisting of name of VM file to translate or the folder containing vm files.");
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

            Console.ReadLine();
        }

        static void CompileFile(string inputFile)
        {
            string outputFile = inputFile.Remove(inputFile.IndexOf(".vm")) + ".asm";

            Tokenizer tokenizer = new Tokenizer(inputFile);
            //CodeWriter codeWriter = new CodeWriter(outputFile);

            //foreach (var command in tokenizer.HasMoreTokens())
            //{
            //    if (command != null && !string.IsNullOrWhiteSpace(command.Command))
            //    {
            //        codeWriter.WriteCommand(command);
            //    }
            //}
            //codeWriter.WriteToFile();

            Console.WriteLine("Done! Output File: " + outputFile);
        }

        static void CompileDirectory(string folderName, string[] inputFiles)
        {
            string outputFile = folderName + "/" + folderName + ".asm";

            // Handle bootstrap code
            //CommandData sysCommand = new CommandData { Command = "call", MemSegment = "Sys.init", CommandType = CommandType.C_CALL, Literal = "Call Sys.init" };
            //CodeWriter codeWriter = new CodeWriter(outputFile);
            //codeWriter.WriteBoostrap(sysCommand);

            //foreach (var inputFile in inputFiles)
            //{
            //    Parser parser = new Parser(inputFile);
            //    foreach (var command in parser.ParseNextCommand())
            //    {
            //        if (command != null && !string.IsNullOrWhiteSpace(command.Command))
            //        {
            //            codeWriter.WriteCommand(command);
            //        }
            //    }
            //}
            //codeWriter.WriteToFile();

            Console.WriteLine("Done! Output File: " + outputFile);
        }
    }
}
