using NiL.JS;
using NilJsSample.Utils;
using System.IO;

namespace NilJsSample.Resolvers
{
    public sealed class NamedPackageModuleResolver : CachedModuleResolverBase
    {
        // this resolver gets the file path from a known, named module
        public override bool TryGetModule(ModuleRequest moduleRequest, out Module result)
        {
            var modulePath = ModuleMapper.GetPath(moduleRequest.CmdArgument);

            if (modulePath is null)
            {
                result = null;
                return false;
            }

            var resolvedPath = Path.Join(Directory.GetCurrentDirectory(), modulePath);

            if (!File.Exists(resolvedPath))
            {
                result = null;
                return false;
            }

            var code = File.ReadAllText(resolvedPath);

            // by setting module name to full path, relative paths are available if/when the resolved file imports more files
            result = new Module(resolvedPath, code);
            return true;
        }
    }
}
