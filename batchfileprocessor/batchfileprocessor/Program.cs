using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BatchFileFramework;
namespace batchFileProcessor
{
	class Program
	{
		static long totalFilesLen = 0;
        static long fileCount = 0;

        // this is a comment
        // this is a comment
		static void ProcessFile(IFileAccessLogic lo, System.IO.FileInfo fi)
		{
			totalFilesLen += fi.Length;
            ++fileCount;
            Console.WriteLine(fi.Name);
            Console.WriteLine("  size: {0} bytes", fi.Length);
            Console.WriteLine("  full path: {0}", fi.DirectoryName);
            System.IO.File.Move(fi.FullName, fi.DirectoryName + "\\visited_" + fi.Name);
        }

		static void Main(string[] args)
		{
			if(args.Length < 1)
			{
				Console.WriteLine("Missing file or folder path: \nUsage: batchFileProcessor filepath(folderpath)");
				return;
			}

            // This is another comment
            // This is another comment
			FileAccessLogic accessor = new FileAccessLogic();
            accessor.FilePattern = "*.txt";
			accessor.OnProcess += ProcessFile;

			accessor.Recursive = true;
			accessor.Execute(args[0]);

			Console.WriteLine("{0} *.txt files in directory \"{1}\" with total size {2} bytes and average size {3} bytes", fileCount, args[0], totalFilesLen, (float)totalFilesLen / fileCount);
            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

		}		       

	}
}
