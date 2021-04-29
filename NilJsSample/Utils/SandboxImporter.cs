using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using NilJsSample.Resolvers;
using System.IO;
using System.Threading.Tasks;

namespace NilJsSample.Utils
{
    public static class SandboxImporter
    {
        public static Promise JavaScriptFirst(string moduleName)
        {
            var container = $@"
                import * as {moduleName} from '{moduleName}';
                async function sandboxImport() {{
                    return {moduleName};
                }}
            ";

            var module = new Module("sandboxImport", container);

            module.ModuleResolversChain.Add(new NamedPackageModuleResolver());
            module.ModuleResolversChain.Add(new RelativePathModuleResolver());

            module.Run();

            return module.Context.GetVariable("sandboxImport")
                    .As<Function>()
                    .Call(new Arguments())
                    .As<Promise>();
        }

        public static async Task<JSValue> CSharpFirst(string moduleName)
        {
            var modulePath = ModuleMapper.GetPath(moduleName);

            if (modulePath is null)
            {
                return JSValue.Undefined;
            }

            var resolvedPath = Path.Join(Directory.GetCurrentDirectory(), modulePath);

            if (!File.Exists(resolvedPath))
            {
                return JSValue.Undefined;
            }

            var code = await File.ReadAllTextAsync(resolvedPath);
            var module = new Module(resolvedPath, code);

            module.ModuleResolversChain.Add(new NamedPackageModuleResolver());
            module.ModuleResolversChain.Add(new RelativePathModuleResolver());

            try
            {
                module.Run();
            }
            catch
            {
                return JSValue.Undefined;
            }

            return module.Exports.CreateExportList();
        }
    }
}
