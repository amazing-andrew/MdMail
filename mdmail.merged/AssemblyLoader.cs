using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mdmail.merged
{
    public class AssemblyLoader
    {
        public AssemblyLoader()
        {
            
        }

        public static void Install()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = Assembly.GetEntryAssembly();

            string name = new AssemblyName(args.Name).Name;
            string resourceName1 = name + ".dll";
            string resourceName2 = name + ".exe";

            string fullResourceName = FindResourceWithName(resourceName1, assembly);

            if (fullResourceName == null)
                fullResourceName = FindResourceWithName(resourceName2, assembly);


            
            if (fullResourceName != null)
                return LoadAssemblyFromEmbeddedResource(fullResourceName, assembly);
            else
                return Assembly.LoadFile(name);
        }

        private static Assembly LoadAssemblyFromEmbeddedResource(string fullResourceName, Assembly assembly)
        {
            byte[] assemblyBytes = GetResourceBytes(fullResourceName, assembly);
            return Assembly.Load(assemblyBytes);
        }

        public static string FindResourceWithName(string resourceName, Assembly assembly)
        {
            var names = assembly.GetManifestResourceNames();
            return names.Where(x => x.EndsWith(resourceName, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(x => x)
                .FirstOrDefault();
        }

        public static byte[] GetResourceBytes(string resouceName, Assembly assembly)
        {
            using (var s = assembly.GetManifestResourceStream(resouceName))
            {
                byte[] buffer = new byte[1024];
                byte[] data = new byte[s.Length];
                int offset = 0;
                int read;

                while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Buffer.BlockCopy(buffer, 0, data, offset, read);
                    offset += read;
                }

                return data;
            }
        }

        public static void ExecuteMainMethod(string assemblyName, string entryClassName, string[] args)
        {
            var assembly = CurrentDomain_AssemblyResolve(null, new ResolveEventArgs(assemblyName));

            if (assembly == null)
                throw new NullReferenceException("Could not find assembly with name " + assemblyName);

            var programClass = assembly.GetType(entryClassName);


            if (programClass == null)
                throw new NullReferenceException("Could not find the entry class with name of " + entryClassName);

            var mainMethod = programClass.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );

            if (mainMethod == null)
                throw new NullReferenceException("Could not find public static main method of " + programClass.GetType().FullName);
            

            mainMethod.Invoke(null, new object[] { args });
        }
    }
}
