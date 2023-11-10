using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Console
{
    public class ProcessRun
    {
        static ProcessRun()
        {
            Cmd = new("cmd", string.Empty, "C:\\Users\\Administrator");
        }

        public ProcessRun(string executableProgram, string startupArguments, string workingDirectory)
        {
            if (string.IsNullOrEmpty(executableProgram))
                throw new ArgumentException($"“{nameof(executableProgram)}”不能为 null 或空。", nameof(executableProgram));
            if (startupArguments is null)
                throw new ArgumentNullException(nameof(startupArguments));
            if (string.IsNullOrEmpty(workingDirectory))
                throw new ArgumentException($"“{nameof(workingDirectory)}”不能为 null 或空。", nameof(workingDirectory));

            ExecutableProgram = executableProgram;
            StartupArguments = startupArguments;
            WorkingDirectory = workingDirectory;
        }

        public static readonly ProcessRun Cmd;

        public string ExecutableProgram { get; }

        public string StartupArguments { get; }

        public string WorkingDirectory { get; }

        public static ProcessRun ReadJsonFile(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

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
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public string ExecutableProgram { get; set; }

            public string StartupArguments { get; set; }

            public string WorkingDirectory { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
