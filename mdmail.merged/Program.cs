using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mdmail.merged
{
    class Program
    {
        static void Main(string[] args)
        {
            AssemblyLoader.Install();
            AssemblyLoader.ExecuteMainMethod("mdmail", "EmailTemplate.Program", args);
        }
    }
}
