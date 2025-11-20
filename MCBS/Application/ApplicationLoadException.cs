using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public class ApplicationLoadException : ExceptionBase
    {
        public ApplicationLoadException(string? assembly, ApplicationLoadError applicationLoadErrorCode, Exception? innerException = null)
        {
            Assembly = assembly;
            ApplicationLoadErrorCode = applicationLoadErrorCode;
        }

        protected override string DefaultMessage => $"从程序集“{Assembly}”加载应用程序失败（{(int)ApplicationLoadErrorCode}）：{ApplicationLoadErrorCode}";

        public string? Assembly { get; }

        public ApplicationLoadError ApplicationLoadErrorCode { get; }
    }
}
