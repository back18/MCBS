using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public static class ApplicationLoader
    {
        private static Assembly LoadAssembly(string assemblyString, Func<string, Assembly> loader)
        {
            try
            {
                return loader.Invoke(assemblyString);
            }
            catch (PathTooLongException ex)
            {
                throw new ApplicationLoadException(assemblyString, ApplicationLoadError.PathTooLong, ex);
            }
            catch (FileNotFoundException ex)
            {
                throw new ApplicationLoadException(assemblyString, ApplicationLoadError.FileNotFound, ex);
            }
            catch (FileLoadException ex)
            {
                throw new ApplicationLoadException(assemblyString, ApplicationLoadError.FileLoadFailed, ex);
            }
            catch (SecurityException ex)
            {
                throw new ApplicationLoadException(assemblyString, ApplicationLoadError.FileSecurityError, ex);
            }
            catch (BadImageFormatException ex)
            {
                throw new ApplicationLoadException(assemblyString, ApplicationLoadError.AssemblyLoadFailed, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationLoadException(assemblyString, ApplicationLoadError.UnknownError, ex);
            }
        }

        public static ApplicationManifest LoadFromFile(string assemblyFile)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assemblyFile, nameof(assemblyFile));

            Assembly assembly = LoadAssembly(assemblyFile, Assembly.LoadFrom);
            return LoadFromAssembly(assembly);
        }

        public static ApplicationManifest LoadFromName(string assemblyName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName, nameof(assemblyName));

            Assembly assembly = LoadAssembly(assemblyName, Assembly.Load);
            return LoadFromAssembly(assembly);
        }

        public static ApplicationManifest LoadFromAssembly(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

            string? assemblyName = assembly.GetName().Name;
            string manifestPath = assemblyName + ".McbsApplication.json";

            Stream? manifestStream;
            try
            {
                manifestStream = assembly.GetManifestResourceStream(manifestPath);
            }
            catch (FileNotFoundException ex)
            {
                throw new ApplicationLoadException(assemblyName, ApplicationLoadError.ManifestNotFound, ex);
            }
            catch (FileLoadException ex)
            {
                throw new ApplicationLoadException(assemblyName, ApplicationLoadError.ManifestLoadFailed, ex);
            }
            catch (IOException ex)
            {
                throw new ApplicationLoadException(assemblyName, ApplicationLoadError.ManifestLoadFailed, ex);
            }
            catch (BadImageFormatException ex)
            {
                throw new ApplicationLoadException(assemblyName, ApplicationLoadError.AssemblyLoadFailed, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationLoadException(assemblyName, ApplicationLoadError.UnknownError, ex);
            }

            if (manifestStream is null)
                throw new ApplicationLoadException(assemblyName, ApplicationLoadError.ManifestNotFound);

            using (manifestStream)
            {
                ApplicationManifest.Model? manifestModel;
                try
                {
                    using StreamReader reader = new(manifestStream, Encoding.UTF8);
                    string manifestText = reader.ReadToEnd();
                    manifestModel = JsonConvert.DeserializeObject<ApplicationManifest.Model>(manifestText);
                }
                catch (Exception ex)
                {
                    throw new ApplicationLoadException(assemblyName, ApplicationLoadError.ManifestFormatError, ex);
                }

                if (manifestModel is null)
                    throw new ApplicationLoadException(assemblyName, ApplicationLoadError.ManifestFormatError);

                try
                {
                    ApplicationManifest applicationManifest = new(assembly, manifestModel);
                    return applicationManifest;
                }
                catch (Exception ex)
                {
                    throw new ApplicationLoadException(assemblyName, ApplicationLoadError.ClassLoadFailed, ex);
                }
            }
        }
    }
}
