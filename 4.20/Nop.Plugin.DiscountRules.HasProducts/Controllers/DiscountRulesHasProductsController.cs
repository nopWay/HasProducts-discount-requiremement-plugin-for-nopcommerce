using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.DiscountRules.HasProducts.Models;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Discounts;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Services.Seo;

namespace Nop.Plugin.DiscountRules.HasProducts.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DiscountRulesHasProductsController : BasePluginController
    {
        private readonly IDiscountService _discountService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;

        public DiscountRulesHasProductsController(IDiscountService discountService,
            ISettingService settingService,
            IPermissionService permissionService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IProductService productService,
            IUrlRecordService urlRecordService)
        {
            _discountService = discountService;
            _settingService = settingService;
            _permissionService = permissionService;
            _workContext = workContext;
            _localizationService = localizationService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _productService = productService;
            _urlRecordService = urlRecordService;
        }

        public IActionResult Configure(int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            if (discountRequirementId.HasValue)
            {
                var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);
                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var requirementId = discountRequirementId ?? 0;
            var model = new RequirementModel
            {
                RequirementId = requirementId,
                DiscountId = discountId,
                Products = _settingService.GetSettingByKey<string>($"DiscountRequirement.RestrictedProductIds-{requirementId}"),
                ProductQuantityMin = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMin-{requirementId}"),
                ProductQuantityMax = _settingService.GetSettingByKey<int>($"DiscountRequirement.ProductQuantityMax-{requirementId}")
            };

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = $"DiscountRulesHasProducts{requirementId}";

            return View("~/Plugins/DiscountRules.HasProducts/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(int discountId, int? discountRequirementId, int productQuantityMin, int productQuantityMax, string productIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Json(new { Result = false, ErrorMessage = "Access denied" });

            if (string.IsNullOrEmpty(productIds))
                return Json(new { Result = false, ErrorMessage = "Please select at least one product" });

            if (productQuantityMin <= 0 || productQuantityMax <= 0)
                return Json(new { Result = false, ErrorMessage = "Minimum and maximum quantities should be greater than zero" });

            if (productQuantityMin > productQuantityMax)
                return Json(new { Result = false, ErrorMessage = "Max quantity should be greater than minimum quantity" });

            var discount = _discountService.GetDiscountById(discountId);
            if (discount == null)
                return Json(new { Result = false, ErrorMessage = "Discount could not be loaded" });

            DiscountRequirement discountRequirement = null;
            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);

            if (discountRequirement == null)
            {
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.HasProducts"
                };
                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);
            }

            _settingService.SetSetting($"DiscountRequirement.ProductQuantityMin-{discountRequirement.Id}", productQuantityMin);
            _settingService.SetSetting($"DiscountRequirement.ProductQuantityMax-{discountRequirement.Id}", productQuantityMax);
            _settingService.SetSetting($"DiscountRequirement.RestrictedProductIds-{discountRequirement.Id}", productIds);

            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        public IActionResult ProductAddPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            var model = new RequirementModel.AddProductModel
            {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View("~/Plugins/DiscountRules.HasProducts/Views/ProductAddPopup.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult ProductAddPopupList(RequirementModel.AddProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                model.SearchVendorId = _workContext.CurrentVendor.Id;

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: model.Page - 1,
                pageSize: model.PageSize,
                showHidden: true
                );

            return Json(GetGridModel(model, products));
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult ProductSelectedPopupList(string selectedItemIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("Access denied");

            var products = _productService.GetProductsByIds(Array.ConvertAll(selectedItemIds.Split(','), int.Parse));
            var model = new RequirementModel.AddProductModel();
            return Json(GetGridModel(model, products.ToPagedList(model)));
        }

        private AddProductToDiscountListModel GetGridModel(RequirementModel.AddProductModel searchModel, IPagedList<Product> products)
        {
            return new AddProductToDiscountListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(product =>
                {
                    var productModel = product.ToModel<ProductModel>();
                    productModel.SeName = _urlRecordService.GetSeName(product, 0, true, false);

                    return productModel;
                });
            });
        }
    }
}