using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NiL.JS;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using NilJsSample.Models;
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
            // this should be a valid way to import dayjs - typeof dayjs is object, serializes to just '{}'
            var failingScript = $@"
                import * as dayjs from 'dayjs';

                async function script() {{
                    return JSON.stringify({{
                        success: true,
                        time: dayjs().format()
                    }});
                }}
            ";

            // this works, but doesn't line up with how dayjs docs show import via ESM: https://day.js.org/docs/en/installation/typescript
            var workingScript = $@"
                import 'dayjs';

                async function script() {{
                    return JSON.stringify({{
                        success: true,
                        time: dayjs().format()
                    }});
                }}
            ";

            var failingModule = new Module("failing", failingScript);
            var workingModule = new Module("working", workingScript);

            // add multiple resolvers to chain
            failingModule.ModuleResolversChain.Add(new NamedPackageModuleResolver());
            failingModule.ModuleResolversChain.Add(new RelativePathModuleResolver());
            workingModule.ModuleResolversChain.Add(new NamedPackageModuleResolver());
            workingModule.ModuleResolversChain.Add(new RelativePathModuleResolver());

            var result = new ImportResultModel();

            // try the failing module and catch errors
            try
            {
                failingModule.Run();

                var returned = await failingModule.Context.GetVariable("script")
                    .As<Function>()
                    .Call(new Arguments())
                    .As<Promise>()
                    .Task;

                var value = JsonSerializer.Deserialize<JsonDocument>((string)returned.Value);

                result.FailResult = value;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                result.FailResult = new
                {
                    success = false,
                    error = e.Message
                };
            }

            // try the working module and catch errors
            try
            {
                workingModule.Run();

                var returned = await workingModule.Context.GetVariable("script")
                    .As<Function>()
                    .Call(new Arguments())
                    .As<Promise>()
                    .Task;

                var value = JsonSerializer.Deserialize<JsonDocument>((string)returned.Value);

                result.SuccessResult = value;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                result.SuccessResult = new
                {
                    success = false,
                    error = e.Message
                };
            }

            return new JsonResult(result);
        }
    }
}
