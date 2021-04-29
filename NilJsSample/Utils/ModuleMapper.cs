namespace NilJsSample.Utils
{
    public static class ModuleMapper
    {
        public static string GetPath(string moduleName) => moduleName switch
        {
            "dayjs" => "node_modules/dayjs/esm/index.js",
            "testmodule" => "wwwroot/lib/index.js",
            _ => null
        };
    }
}
