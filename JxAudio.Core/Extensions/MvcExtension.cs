using System.Runtime.CompilerServices;
using JxAudio.Core.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RouteAttribute = Microsoft.AspNetCore.Components.RouteAttribute;

namespace JxAudio.Core.Extensions;

public static class MvcExtension
{
    public static IMvcBuilder AddServiceController(this IServiceCollection collection,
        bool enableDynamicController = true)
    {
        using var serviceProvider = collection.BuildServiceProvider();
        var option = serviceProvider.GetService<IOptions<AppConfigOption>>()?.Value ?? new AppConfigOption();
        var builder = collection.AddMvc(options =>
        {
            if (enableDynamicController)
            {
                options.Conventions.Add(new DynamicControllerFeatureProvider(option));
            }
        });

        return builder;
    }

    private class DynamicControllerFeatureProvider(AppConfigOption option) : IApplicationModelConvention
    {

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                // 检查是否是ControllerBase的派生类
                if (controller.ControllerType.IsAssignableTo(typeof(DynamicControllerBase)))
                {
                    // 检查是否已经有RouteAttribute定义
                    var hasRouteAttribute = controller.Selectors.Any(selector =>
                        selector.AttributeRouteModel != null);

                    if (!hasRouteAttribute)
                    {
                        var routeAttribute = new Microsoft.AspNetCore.Mvc.RouteAttribute(
                            $"{option.DynamicPrefix}{(option.DynamicPrefix?.EndsWith('/') == false ? "/" : "")}[controller]");
                        var selectors = controller.Selectors.First();
                        var nullableContext = selectors.EndpointMetadata
                            .FirstOrDefault(x => x is NullableContextAttribute);
                        if (nullableContext != null)
                        {
                            selectors.EndpointMetadata.Remove(nullableContext);
                        }
                        // 没有RouteAttribute，所以添加一个

                        selectors.AttributeRouteModel = new AttributeRouteModel(routeAttribute);

                    }

                    foreach (var action in controller.Actions)
                    {
                        // 检查动作是否已经有HTTP方法特性（HttpGet, HttpPost, 等等）
                        var hasHttpMethodAttribute = action.Selectors.Any(selector =>
                            selector.EndpointMetadata.OfType<HttpMethodAttribute>().Any());

                        // 如果没有HTTP方法特性，则添加一个默认的HttpGet特性
                        if (!hasHttpMethodAttribute)
                        {
                            var selectors = action.Selectors.First();
                            var nullableContext = selectors.EndpointMetadata
                                .FirstOrDefault(x => x is NullableContextAttribute);
                            if (nullableContext != null)
                            {
                                selectors.EndpointMetadata.Remove(nullableContext);
                            }

                            var prefix = option.GetPrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                            if (prefix != null)
                            {
                                if (option.AutoRemoveDynamicPrefix)
                                {
                                    action.ActionName = action.ActionName[prefix.Length..];
                                }

                                selectors.AttributeRouteModel =
                                    new AttributeRouteModel(new Microsoft.AspNetCore.Mvc.RouteAttribute("[action]"));
                                selectors.EndpointMetadata.Add(new HttpGetAttribute("[action]"));
                                selectors.EndpointMetadata.Add(new HttpMethodMetadata(["GET"]));

                                selectors.ActionConstraints.Add(new HttpMethodActionConstraint(["GET"]));
                                continue;
                            }

                            prefix = option.PutPrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                            if (prefix != null)
                            {
                                if (option.AutoRemoveDynamicPrefix)
                                {
                                    action.ActionName = action.ActionName[prefix.Length..];
                                }

                                selectors.AttributeRouteModel =
                                    new AttributeRouteModel(new Microsoft.AspNetCore.Mvc.RouteAttribute("[action]"));
                                selectors.EndpointMetadata.Add(new HttpPutAttribute("[action]"));
                                selectors.EndpointMetadata.Add(new HttpMethodMetadata(["PUT"]));

                                selectors.ActionConstraints.Add(new HttpMethodActionConstraint(["PUT"]));
                                continue;
                            }

                            prefix = option.DeletePrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                            if (prefix != null)
                            {
                                if (option.AutoRemoveDynamicPrefix)
                                {
                                    action.ActionName = action.ActionName[prefix.Length..];
                                }

                                selectors.AttributeRouteModel =
                                    new AttributeRouteModel(new Microsoft.AspNetCore.Mvc.RouteAttribute("[action]"));
                                selectors.EndpointMetadata.Add(new HttpDeleteAttribute("[action]"));
                                selectors.EndpointMetadata.Add(new HttpMethodMetadata(["DELETE"]));

                                selectors.ActionConstraints.Add(new HttpMethodActionConstraint(["DELETE"]));
                                continue;
                            }

                            prefix = option.PostPrefix.FirstOrDefault(x => action.ActionName.StartsWith(x));
                            if (option.AutoRemoveDynamicPrefix && prefix != null)
                            {
                                action.ActionName = action.ActionName[prefix.Length..];
                            }

                            selectors.AttributeRouteModel =
                                new AttributeRouteModel(new Microsoft.AspNetCore.Mvc.RouteAttribute("[action]"));
                            selectors.EndpointMetadata.Add(new HttpPostAttribute("[action]"));
                            selectors.EndpointMetadata.Add(new HttpMethodMetadata(["POST"]));

                            selectors.ActionConstraints.Add(new HttpMethodActionConstraint(["POST"]));
                        }
                    }
                }
            }
        }
    }
}