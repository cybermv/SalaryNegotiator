using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Vele.SalaryNegotiator.Core.Exceptions;

namespace Vele.SalaryNegotiator.Web.Exceptions;

public class ExceptionFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        ILogger logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ExceptionFilterAttribute>>();

        if (context.Exception is BaseException baseEx)
        {
            logger.LogWarning(baseEx, "Exception occurred");

            if (baseEx is ValidationException validationEx)
            {
                if (validationEx.ValidationResult != null)
                {
                    context.Result = new ObjectResult(validationEx.ValidationResult.ToString()) { StatusCode = (int)HttpStatusCode.BadRequest };
                }
                else
                {
                    context.Result = new ObjectResult(validationEx.Message) { StatusCode = (int)HttpStatusCode.BadRequest };
                }
            }
            else if (baseEx is NotFoundException notFoundEx)
            {
                context.Result = new ObjectResult(notFoundEx.Message) { StatusCode = (int)HttpStatusCode.NotFound };
            }
            else if (baseEx is ForbiddenException forbiddenEx)
            {
                context.Result = new ObjectResult(forbiddenEx.Message) { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            else
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            context.ExceptionHandled = true;
        }
    }
}