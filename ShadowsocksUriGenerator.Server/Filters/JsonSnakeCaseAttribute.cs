using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace ShadowsocksUriGenerator.Server.Filters
{
    public class JsonSnakeCaseAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Add(new SystemTextJsonOutputFormatter(new()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    IgnoreReadOnlyProperties = true,
                    PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
                }));
            }
        }
    }
}
