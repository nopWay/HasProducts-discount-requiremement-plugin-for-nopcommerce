using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.DiscountRules.HasProducts
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute("Plugin.DiscountRules.HasProducts.Configure",
                "Plugins/DiscountRulesHasProducts/Configure",
                new { controller = "DiscountRulesHasProducts", action = "Configure" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasProducts.ProductAddPopup",
                "Plugins/DiscountRulesHasProducts/ProductAddPopup",
                new { controller = "DiscountRulesHasProducts", action = "ProductAddPopup" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasProducts.ProductAddPopupList",
                "Plugins/DiscountRulesHasProducts/ProductAddPopupList",
                new { controller = "DiscountRulesHasProducts", action = "ProductAddPopupList" });

            routeBuilder.MapRoute("Plugin.DiscountRules.HasProducts.ProductSelectedPopupList",
                "Plugins/DiscountRulesHasProducts/ProductSelectedPopupList",
                new { controller = "DiscountRulesHasProducts", action = "ProductSelectedPopupList" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
