﻿using ASPNETCoreIdentitySample.Common.GuardToolkit;
using ASPNETCoreIdentitySample.Common.IdentityToolkit;
using ASPNETCoreIdentitySample.Common.WebToolkit;
using ASPNETCoreIdentitySample.Services.Contracts.Identity;
using ASPNETCoreIdentitySample.Services.Identity;
using ASPNETCoreIdentitySample.ViewModels.Identity;
using DNTBreadCrumb.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASPNETCoreIdentitySample.Areas.Identity.Controllers
{
    [Authorize(Roles = ConstantRoles.Admin)]
    [Area(AreaConstants.IdentityArea)]
    [BreadCrumb(Title = "مدیریت نقش‌های پویا", UseDefaultRouteUrl = true, Order = 0)]
    public class DynamicRoleClaimsManagerController : Controller
    {
        private readonly IMvcActionsDiscoveryService _mvcActionsDiscoveryService;
        private readonly IApplicationRoleManager _roleManager;

        public DynamicRoleClaimsManagerController(
            IMvcActionsDiscoveryService mvcActionsDiscoveryService,
            IApplicationRoleManager roleManager)
        {
            _mvcActionsDiscoveryService = mvcActionsDiscoveryService;
            _mvcActionsDiscoveryService.CheckArgumentIsNull(nameof(_mvcActionsDiscoveryService));

            _roleManager = roleManager;
            _roleManager.CheckArgumentIsNull(nameof(_roleManager));
        }

        [BreadCrumb(Title = "ایندکس", Order = 1)]
        public async Task<IActionResult> Index(int? id)
        {
            this.AddBreadCrumb(new BreadCrumb
            {
                Title = "مدیریت نقش‌ها",
                Url = Url.Action("Index", "RolesManager"),
                Order = -1
            });

            if (!id.HasValue)
            {
                return View("Error");
            }

            var role = await _roleManager.FindRoleIncludeRoleClaimsAsync(id.Value).ConfigureAwait(false);
            if (role == null)
            {
                return View("NotFound");
            }

            var securedControllerActions = _mvcActionsDiscoveryService.GetAllSecuredControllerActionsWithPolicy(ConstantPolicies.DynamicPermission);
            return View(model: new DynamicRoleClaimsManagerViewModel
            {
                SecuredControllerActions = securedControllerActions,
                RoleIncludeRoleClaims = role
            });
        }

        [AjaxOnly, HttpPost, ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(DynamicRoleClaimsManagerViewModel model)
        {
            var result = await _roleManager.AddOrUpdateRoleClaimsAsync(
                roleId: model.RoleId,
                roleClaimType: ConstantPolicies.DynamicPermissionClaimType,
                selectedRoleClaimValues: model.ActionIds).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                return BadRequest(error: result.DumpErrors(useHtmlNewLine: true));
            }
            return Json(new { success = true });
        }
    }
}