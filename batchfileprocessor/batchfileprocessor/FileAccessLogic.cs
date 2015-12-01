using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BatchFileFramework
{
	public delegate void FileAccessProcess(IFileAccessLogic logic, FileInfo fileInfo);
	public delegate void FileAccessNotify(string message);
	
	public interface IFileAccessLogic
	{
		bool Recursive
		{
			get;
			set;
		}
		bool SkipReadOnly
		{
			get;
			set;
		}
		bool ForceWriteable
		{
			get;
			set;
		}
		string FilePattern
		{
			get;
			set;
		}
		bool Cancelled
		{
			get;
			set;
		}
		void Execute(string fullPath);
		void Cancel();
		void Notify(string message);
		event FileAccessProcess OnProcess;
		event FileAccessNotify OnNotify;
	}


	public class FileAccessLogic : IFileAccessLogic
	{
		private bool verbose = false;
		private bool recursive = false;
		private bool skipReadOnly = false;
		private bool forceWriteable = false;
		private string filePattern = "*.*";
		private bool cancelled = false;
		private bool running = false;
		public event FileAccessProcess OnProcess = null;
		public event FileAccessNotify OnNotify = null;

		public bool Verbose
		{
			get { return verbose; }
			set
			{
				if (!this.running)
					verbose = value;
			}
		}

		public bool Recursive
		{
			get { return recursive; }
			set
			{
				if (!this.running)
					recursive = value;
			}
		}
		public bool SkipReadOnly
		{
			get { return skipReadOnly; }
			set
			{
				if (!this.running)
					skipReadOnly = value;
			}
		}
		public bool ForceWriteable
		{
			get { return forceWriteable; }
			set
			{
				if (!this.running)
					forceWriteable = value;
			}
		}
		public string FilePattern
		{
			get { return filePattern; }
			set
			{
				if (!this.running)
					filePattern = value;
			}
		}
		public bool Cancelled
		{
			get { return cancelled; }
			set { cancelled = value; }
		}
		public void Execute(string fullPath)
		{
			cancelled = false;
			running = true;
			if (File.Exists(fullPath))
				Process(this, new FileInfo(fullPath));
			else if (Directory.Exists(fullPath))
				ProcessDirectory(fullPath);
			running = false;
		}

		public void Cancel()
		{
			cancelled = true;
		}
		public void Notify(string message)
		{
			if (!verbose)
			{
				if (this.OnNotify != null)
					this.OnNotify(message);
			}
		}
		
		private void ProcessDirectory(string directoryPath)
		{
			ProcessDirectory(new DirectoryInfo(directoryPath));
		}
		private void ProcessDirectory(DirectoryInfo directoryInfo)
		{
			if (cancelled)
				return;
			ProcessFiles(directoryInfo);
			if (recursive)
			{
				foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
					ProcessDirectory(subDirectoryInfo);
			}
		}

		private void ProcessFiles(DirectoryInfo directoryInfo)
		{
			foreach (FileInfo fileInfo in directoryInfo.GetFiles(this.filePattern))
			{
				if (cancelled)
					return;
				FileAttributes attributes = File.GetAttributes(fileInfo.FullName);
				if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					if (skipReadOnly)
						continue;
					else if (forceWriteable)
						File.SetAttributes(fileInfo.FullName, FileAttributes.Normal);
					else
						continue;
				}
				Process(this, fileInfo);
			}
		}

		protected virtual void Process(IFileAccessLogic logic, FileInfo fileInfo)
		{
			if (OnProcess != null)
				OnProcess(this, fileInfo);
			if (OnNotify != null)
				OnNotify("Processed file " +  fileInfo.ToString());

		}
	}
}