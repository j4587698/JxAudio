using JxAudio.Core.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace JxAudio.Core.Extensions;

public static class MvcExtension
{
    public static IMvcCoreBuilder AddServiceController(this IServiceCollection collection, bool enableDynamicController = true)
    {
        using var serviceProvider = collection.BuildServiceProvider();
        var option = serviceProvider.GetService<IOptions<AppConfigOption>>()?.Value ?? new AppConfigOption();
        var builder = collection.AddMvcCore(options =>
        {
            if (enableDynamicController)
            {
                options.Conventions.Add(new DynamicControllerFeatureProvider(option));
            }
        }).AddControllersAsServices();
        
        return builder;
    }
    
    private class DynamicControllerFeatureProvider(AppConfigOption option) : IApplicationModelConvention
    {

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                // 检查是否是ControllerBase的派生类
                if (controller.ControllerType.IsSubclassOf(typeof(ControllerBase)))
                {
                    // 检查是否已经有RouteAttribute定义
                    var hasRouteAttribute = controller.Selectors.Any(selector =>
                        selector.AttributeRouteModel != null);

                    if (!hasRouteAttribute)
                    {
                        var routeAttribute = new Microsoft.AspNetCore.Mvc.RouteAttribute("[controller]");
                        // 没有RouteAttribute，所以添加一个
                        controller.Selectors.Add(new SelectorModel
                        {
                            AttributeRouteModel = new AttributeRouteModel(routeAttribute)
                        });
                    }
                }
                
                foreach (var action in controller.Actions)
                {
                    // 检查动作是否已经有HTTP方法特性（HttpGet, HttpPost, 等等）
                    var hasHttpMethodAttribute = action.Selectors.Any(selector =>
                        selector.EndpointMetadata.OfType<HttpMethodAttribute>().Any());

                    // 如果没有HTTP方法特性，则添加一个默认的HttpGet特性
                    if (!hasHttpMethodAttribute)
                    {
                        var hasAttribute = false;
                        var prefix = option.GetPrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                        if (prefix != null)
                        {
                            if (option.AutoRemoveDynamicPrefix)
                            {
                                action.ActionName = action.ActionName[prefix.Length..];
                            }
                            action.Selectors.Add(new SelectorModel
                            {
                                EndpointMetadata = { new HttpGetAttribute("[action]") }
                            });
                            hasAttribute = true;
                        }
                        prefix = option.PutPrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                        if (prefix != null)
                        {
                            if (option.AutoRemoveDynamicPrefix)
                            {
                                action.ActionName = action.ActionName[prefix.Length..];
                            }
                            action.Selectors.Add(new SelectorModel
                            {
                                EndpointMetadata = { new HttpPutAttribute("[action]") }
                            });
                            hasAttribute = true;
                        }
                        prefix = option.DeletePrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                        if (prefix != null)
                        {
                            if (option.AutoRemoveDynamicPrefix)
                            {
                                action.ActionName = action.ActionName[prefix.Length..];
                            }
                            action.Selectors.Add(new SelectorModel
                            {
                                EndpointMetadata = { new HttpDeleteAttribute("[action]") }
                            });
                            hasAttribute = true;
                        }
                        prefix = option.PostPrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                        if (!hasAttribute)
                        {
                            if (option.AutoRemoveDynamicPrefix && prefix != null)
                            {
                                action.ActionName = action.ActionName[prefix.Length..];
                            }
                            action.Selectors.Add(new SelectorModel
                            {
                                EndpointMetadata = { new HttpDeleteAttribute("[action]") }
                            });
                        }
                    }
                }
            }
        }
    }
}