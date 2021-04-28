using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using NilJsSample.Resolvers;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace NilJsSample.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger) => _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Import()
        {
            var script = $@"
                import dayjs from 'dayjs';

                async function script() {{
                    return JSON.stringify({{
                        success: true,
                        time: dayjs().format()
                    }});
                }}
            ";

            var module = new Module("default", script);

            // add multiple resolvers to chain
            module.ModuleResolversChain.Add(new NamedPackageModuleResolver());
            module.ModuleResolversChain.Add(new RelativePathModuleResolver());

            try
            {
                module.Run();

                var returned = await module.Context.GetVariable("script")
                    .As<Function>()
                    .Call(new Arguments())
                    .As<Promise>()
                    .Task;

                var value = JsonSerializer.Deserialize<JsonDocument>((string)returned.Value);

                return new JsonResult(value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                return new JsonResult(new
                {
                    success = false,
                    error = e.Message
                });
            }
        }
    }
}
