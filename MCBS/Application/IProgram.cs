using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public interface IProgram
    {
        public int Main(string[] args);

        public void Exit();
    }
}
