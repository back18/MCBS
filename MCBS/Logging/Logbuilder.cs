using log4net;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Logging
{
    public class Logbuilder : ILogbuilder
    {
        public static readonly Logbuilder Default = new(LogUtil.GetLogger);

        private readonly Func<string, ILog> _func;

        public Logbuilder(Func<string, ILog> func)
        {
            ArgumentNullException.ThrowIfNull(func, nameof(func));

            _func = func;
        }

        public ILogger GetLogger()
        {
            StackFrame frame = new(1);
            MethodBase? method = frame.GetMethod();
            Type? type = method?.DeclaringType;
            if (type is null)
                return GetLogger("null");
            return GetLogger(type);
        }

        public ILogger GetLogger(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            return GetLogger(type.FullName ?? type.Name);
        }

        public ILogger GetLogger(string name)
        {
            return new Log4NetLogger(_func.Invoke(name));
        }
    }
}
