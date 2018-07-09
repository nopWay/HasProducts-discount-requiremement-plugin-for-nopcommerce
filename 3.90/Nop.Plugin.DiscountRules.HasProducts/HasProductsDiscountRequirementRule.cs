using System;
using System.Linq;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Orders;

namespace Nop.Plugin.DiscountRules.HasProducts
{
    public partial class HasProductsDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;

        public HasProductsDiscountRequirementRule(ISettingService settingService)
        {
            this._settingService = settingService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            
            //invalid by default
            var result = new DiscountRequirementValidationResult();

            var productQuantityMin = _settingService.GetSettingByKey<int>(string.Format("DiscountRequirement.ProductQuantityMin-{0}", request.DiscountRequirementId));
            var productQuantityMax = _settingService.GetSettingByKey<int>(string.Format("DiscountRequirement.ProductQuantityMax-{0}", request.DiscountRequirementId));
            var restrictedProductIds = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirement.RestrictedProductIds-{0}", request.DiscountRequirementId));
            if (String.IsNullOrWhiteSpace(restrictedProductIds))
            {
                //valid
                result.IsValid = true;
                return result;
            }

            if (request.Customer == null)
                return result;

            //we support comma-separated list of product identifiers (e.g. 77, 123, 156).
            var restrictedProducts = restrictedProductIds
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
            if (!restrictedProducts.Any())
                return result;

            //group products in the cart by product ID
            //it could be the same product with distinct product attributes
            //that's why we get the total quantity of this product
            var cartQuery = from sci in request.Customer.ShoppingCartItems.LimitPerStore(request.Store.Id)
                            where sci.ShoppingCartType == ShoppingCartType.ShoppingCart
                            group sci by sci.ProductId into g
                            select new { ProductId = g.Key, TotalQuantity = g.Sum(x => x.Quantity) };
            var cart = cartQuery.ToList();

            int totalQuantity = 0;
            foreach (var restrictedProduct in restrictedProducts)
            {
                if (String.IsNullOrWhiteSpace(restrictedProduct))
                    continue;

                foreach (var sci in cart)
                {
                    if (int.TryParse(restrictedProduct, out int restrictedProductId))
                    {
                        if (sci.ProductId == restrictedProductId)
                        {
                            totalQuantity += sci.TotalQuantity;

                            if (productQuantityMin > 0 && productQuantityMax > 0 &&
                                totalQuantity >= productQuantityMin && totalQuantity <= productQuantityMax)
                            {
                                result.IsValid = true;
                                return result;
                            }
                        }
                    }
                }               
            }

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(int discountId, int? discountRequirementId)
        {
            //configured in RouteProvider.cs
            string result = "Plugins/DiscountRulesHasProducts/Configure/?discountId=" + discountId;
            if (discountRequirementId.HasValue)
                result += string.Format("&discountRequirementId={0}", discountRequirementId.Value);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products", "Products");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). Quantity and range aren't applicable here. You can find a product ID on its details page.");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min", "Minimum quantity");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min.Hint", "Discount will be applied if cart contains more selected products than the defined value here. Minimum quantity should be greater than zero.");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max", "Maximum quantity");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max.Hint", "Discount will be applied if cart contains fewer selected products than the defined value here. Maximum quantity should be greater than zero and minimum quantity.");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Multiple.Selected", "{0} products selected");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Single.Selected", "One product selected");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Not.Selected", "No products selected");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.AddNew", "Add product");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Choose", "Choose");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Hint");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min.Hint");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max.Hint");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Multiple.Selected");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Single.Selected");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Not.Selected");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.AddNew");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Choose");
            base.Uninstall();
        }
    }
}