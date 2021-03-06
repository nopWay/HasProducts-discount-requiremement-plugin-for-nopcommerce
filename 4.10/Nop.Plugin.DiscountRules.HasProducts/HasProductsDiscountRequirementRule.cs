using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
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
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly ILocalizationService _localizationService;

        public HasProductsDiscountRequirementRule(ISettingService settingService,
            IActionContextAccessor actionContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._actionContextAccessor = actionContextAccessor;
            this._urlHelperFactory = urlHelperFactory;
            this._localizationService = localizationService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            var productQuantityMin = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMin-{request.DiscountRequirementId}");
            var productQuantityMax = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMax-{request.DiscountRequirementId}");
            var restrictedProductIds = _settingService.GetSettingByKey<string>($"DiscountRequirement.RestrictedProductIds-{request.DiscountRequirementId}");

            if (string.IsNullOrWhiteSpace(restrictedProductIds))
                return result;

            if (productQuantityMin <= 0 || productQuantityMax <= 0 || productQuantityMin > productQuantityMax)
                return result;

            if (request.Customer == null)
                return result;

            //we support comma-separated list of product identifiers (e.g. 77, 123, 156).
            var restrictedProducts = restrictedProductIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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
            var totalQuantity = 0;

            foreach (var sci in cart)
            {
                if (restrictedProducts.Any(id => id == sci.ProductId.ToString()))
                {
                    totalQuantity += sci.TotalQuantity;

                    if (totalQuantity > productQuantityMax)
                        return result;
                }
            }

            result.IsValid = totalQuantity >= productQuantityMin && totalQuantity <= productQuantityMax;
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
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            return urlHelper.Action("Configure", "DiscountRulesHasProducts",
                new { discountId, discountRequirementId }).TrimStart('/');
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products", "Products");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). Quantity and range aren't applicable here. You can find a product ID on its details page.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min", "Minimum quantity");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min.Hint", "Discount will be applied if cart contains more selected products than the defined value here. Minimum quantity should be greater than zero.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max", "Maximum quantity");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max.Hint", "Discount will be applied if cart contains fewer selected products than the defined value here. Maximum quantity should be greater than zero and minimum quantity.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Multiple.Selected", "{0} products selected");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Single.Selected", "One product selected");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Not.Selected", "No products selected");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.AddNew", "Add product");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.Choose", "Choose");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.HasProducts.ViewSelectedProducts", "View Selected Products");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Products.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Min.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Fields.Quantity.Max.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Multiple.Selected");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Single.Selected");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Not.Selected");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.AddNew");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.Choose");
            _localizationService.DeletePluginLocaleResource("Plugins.DiscountRules.HasProducts.ViewSelectedProducts");

            base.Uninstall();
        }
    }
}