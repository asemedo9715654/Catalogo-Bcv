using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace CatalogoBCV.Filters;

public class SerilogActionFilter : IActionFilter
{
    private readonly IDiagnosticContext _diagnosticContext;

    public SerilogActionFilter(IDiagnosticContext diagnosticContext)
    {
        _diagnosticContext = diagnosticContext;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var actionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (actionDescriptor != null)
        {
            _diagnosticContext.Set("ControllerName", actionDescriptor.ControllerName);
            _diagnosticContext.Set("ActionName", actionDescriptor.ActionName);
        }

        // Log the start of the action
        Log.Information("Executing Action {ActionName} on Controller {ControllerName}", 
            actionDescriptor?.ActionName, 
            actionDescriptor?.ControllerName);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var actionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        
        // Log the end of the action
        Log.Information("Executed Action {ActionName} on Controller {ControllerName}", 
            actionDescriptor?.ActionName, 
            actionDescriptor?.ControllerName);
    }
}
