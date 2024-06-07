using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Console
{
    public class ProcessInfo
    {
        static ProcessInfo()
        {
            CMD = new("cmd", string.Empty, "C:\\");
        }

        public ProcessInfo(string executableProgram, string startupArguments, string workingDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(executableProgram, nameof(executableProgram));
            ArgumentNullException.ThrowIfNull(startupArguments, nameof(startupArguments));
            ArgumentException.ThrowIfNullOrEmpty(workingDirectory, nameof(workingDirectory));

            ExecutableProgram = executableProgram;
            StartupArguments = startupArguments;
            WorkingDirectory = workingDirectory;
        }

        public static readonly ProcessInfo CMD;

        public string ExecutableProgram { get; }

        public string StartupArguments { get; }

        public string WorkingDirectory { get; }

        public static ProcessInfo ReadJsonFile(string path)
        {
            ArgumentNullException.ThrowIfNull(path, nameof(path));

            string json = File.ReadAllText(path);
            Model model = JsonConvert.DeserializeObject<Model>(json) ?? throw new NullReferenceException();

            if (string.IsNullOrEmpty(model.StartupArguments))
                model.StartupArguments = string.Empty;
            if (string.IsNullOrEmpty(model.WorkingDirectory))
                model.WorkingDirectory = Path.GetDirectoryName(path) ?? string.Empty;

            return new(model.ExecutableProgram, model.StartupArguments, model.WorkingDirectory);
        }

        public class Model
        {
            public required string ExecutableProgram { get; set; }

            public required string StartupArguments { get; set; }

            public required string WorkingDirectory { get; set; }
        }
    }
}
