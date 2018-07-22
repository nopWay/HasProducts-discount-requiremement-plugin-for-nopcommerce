using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.DiscountRules.HasProducts
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.DiscountRules.HasProducts.Configure",
                 "Plugins/DiscountRulesHasProducts/Configure",
                 new { controller = "DiscountRulesHasProducts", action = "Configure" },
                 new[] { "Nop.Plugin.DiscountRules.HasProducts.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasProducts.ProductAddPopup",
                 "Plugins/DiscountRulesHasProducts/ProductAddPopup",
                 new { controller = "DiscountRulesHasProducts", action = "ProductAddPopup" },
                 new[] { "Nop.Plugin.DiscountRules.HasProducts.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasProducts.ProductAddPopupList",
                 "Plugins/DiscountRulesHasProducts/ProductAddPopupList",
                 new { controller = "DiscountRulesHasProducts", action = "ProductAddPopupList" },
                 new[] { "Nop.Plugin.DiscountRules.HasProducts.Controllers" }
            );
            routes.MapRoute("Plugin.DiscountRules.HasProducts.ProductSelectedPopupList",
                "Plugins/DiscountRulesHasProducts/ProductSelectedPopupList",
                new { controller = "DiscountRulesHasProducts", action = "ProductSelectedPopupList" },
                new[] { "Nop.Plugin.DiscountRules.HasProducts.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
