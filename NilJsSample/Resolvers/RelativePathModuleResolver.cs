using NiL.JS;
using System.IO;

namespace NilJsSample.Resolvers
{
    public class RelativePathModuleResolver : CachedModuleResolverBase
    {
        // this resolver resolves full file paths from the current working directory
        public override bool TryGetModule(ModuleRequest moduleRequest, out Module result)
        {
            // if the module wasn't a known, named one, we see if the supplied path is available in the CWD
            var resolvedPath = moduleRequest.AbsolutePath;

            if (!File.Exists(resolvedPath))
            {
                result = null;
                return false;
            }

            var code = File.ReadAllText(resolvedPath);

            result = new Module(moduleRequest.AbsolutePath, code);
            return true;
        }
    }
}
