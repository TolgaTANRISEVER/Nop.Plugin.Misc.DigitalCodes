using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.DigitalCodes.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Plugin.Misc.DigitalCodes.Services;
using Nop.Plugin.Misc.DigitalCodes.Models.CodePool;
using Nop.Plugin.Misc.DigitalCodes.Models.CodeItem;
using Nop.Plugin.Misc.DigitalCodes.Models.DeliveryLog;
using Nop.Plugin.Misc.DigitalCodes.Domain;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.DigitalCodes.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class DigitalCodesController : BasePluginController
{
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly ICodePoolService _codePoolService;
    private readonly IProductService _productService;
    private readonly ICodeItemService _codeItemService;
    private readonly IDeliveryLogService _deliveryLogService;

    public DigitalCodesController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IStoreContext storeContext,
        ICodePoolService codePoolService,
        IProductService productService,
        ICodeItemService codeItemService,
        IDeliveryLogService deliveryLogService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _storeContext = storeContext;
        _codePoolService = codePoolService;
        _productService = productService;
        _codeItemService = codeItemService;
        _deliveryLogService = deliveryLogService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<DigitalCodesSettings>(storeScope);

        var model = new ConfigurationModel
        {
            EnableWebhook = settings.EnableWebhook,
            WebhookUrl = settings.WebhookUrl,
            Secret = settings.Secret,
            TimeoutSeconds = settings.TimeoutSeconds,
            RetryCount = settings.RetryCount,
            ActiveStoreScopeConfiguration = storeScope
        };

        if (storeScope > 0)
        {
            model.EnableWebhook_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableWebhook, storeScope);
            model.WebhookUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.WebhookUrl, storeScope);
            model.Secret_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Secret, storeScope);
            model.TimeoutSeconds_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TimeoutSeconds, storeScope);
            model.RetryCount_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.RetryCount, storeScope);
        }

        return View("~/Plugins/Misc.DigitalCodes/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<DigitalCodesSettings>(storeScope);

        settings.EnableWebhook = model.EnableWebhook;
        settings.WebhookUrl = model.WebhookUrl;
        settings.Secret = model.Secret;
        settings.TimeoutSeconds = model.TimeoutSeconds;
        settings.RetryCount = model.RetryCount;

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableWebhook, model.EnableWebhook_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.WebhookUrl, model.WebhookUrl_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Secret, model.Secret_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TimeoutSeconds, model.TimeoutSeconds_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.RetryCount, model.RetryCount_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public IActionResult CodePools()
    {
        var model = new CodePoolSearchModel();
        return View("~/Plugins/Misc.DigitalCodes/Views/CodePools.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodePoolList(CodePoolSearchModel searchModel)
    {
        var pools = await _codePoolService.SearchAsync(searchModel.SearchName, searchModel.SearchProductId, searchModel.Page - 1, searchModel.PageSize);

        var model = new CodePoolListModel().PrepareToGrid(searchModel, pools, () =>
        {
            return pools.Select(x => new CodePoolModel
            {
                Id = x.Id,
                Name = x.Name,
                ProductId = x.ProductId,
                ProductName = x.ProductId.HasValue ? _productService.GetProductByIdAsync(x.ProductId.Value).Result?.Name : null,
                IsActive = x.IsActive,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc
            });
        });

        return Json(model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodePoolDelete(int id)
    {
        var entity = await _codePoolService.GetByIdAsync(id);
        if (entity == null)
            return Json(new { Result = false });

        await _codePoolService.DeleteAsync(entity);
        return Json(new { Result = true });
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public IActionResult CodePoolCreate()
    {
        var model = new CodePoolModel { IsActive = true };
        return View("~/Plugins/Misc.DigitalCodes/Views/CodePoolCreate.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodePoolCreate(CodePoolModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Plugins/Misc.DigitalCodes/Views/CodePoolCreate.cshtml", model);

        var entity = new Domain.CodePool
        {
            Name = model.Name,
            ProductId = model.ProductId,
            IsActive = model.IsActive
        };
        await _codePoolService.InsertAsync(entity);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.DataSaved"));
        return RedirectToAction("CodePools");
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodePoolEdit(int id)
    {
        var entity = await _codePoolService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("CodePools");

        var model = new CodePoolModel
        {
            Id = entity.Id,
            Name = entity.Name,
            ProductId = entity.ProductId,
            ProductName = entity.ProductId.HasValue ? (await _productService.GetProductByIdAsync(entity.ProductId.Value))?.Name : null,
            IsActive = entity.IsActive,
            CreatedOnUtc = entity.CreatedOnUtc,
            UpdatedOnUtc = entity.UpdatedOnUtc
        };
        return View("~/Plugins/Misc.DigitalCodes/Views/CodePoolEdit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodePoolEdit(CodePoolModel model)
    {
        var entity = await _codePoolService.GetByIdAsync(model.Id);
        if (entity == null)
            return RedirectToAction("CodePools");

        if (!ModelState.IsValid)
            return View("~/Plugins/Misc.DigitalCodes/Views/CodePoolEdit.cshtml", model);

        entity.Name = model.Name;
        entity.ProductId = model.ProductId;
        entity.IsActive = model.IsActive;
        await _codePoolService.UpdateAsync(entity);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.DataSaved"));
        return RedirectToAction("CodePools");
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public IActionResult CodeItems(int? codePoolId = null)
    {
        var model = new CodeItemSearchModel();
        if (codePoolId.HasValue && codePoolId.Value > 0)
            model.SearchCodePoolId = codePoolId.Value;
        return View("~/Plugins/Misc.DigitalCodes/Views/CodeItems.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodeItemList(CodeItemSearchModel searchModel)
    {
        int? poolIdFilter = (searchModel.SearchCodePoolId > 0) ? searchModel.SearchCodePoolId : null;
        var items = await _codeItemService.SearchAsync(poolIdFilter, searchModel.SearchStatus, searchModel.SearchOrderItemId, searchModel.SearchCode, searchModel.Page - 1, searchModel.PageSize);

        var model = new CodeItemListModel().PrepareToGrid(searchModel, items, () =>
        {
            return items.Select(x => new CodeItemModel
            {
                Id = x.Id,
                CodePoolId = x.CodePoolId,
                CodePoolName = _codePoolService.GetByIdAsync(x.CodePoolId).Result?.Name,
                Code = x.Code,
                Pin = x.Pin,
                Serial = x.Serial,
                ExpireOnUtc = x.ExpireOnUtc,
                Status = x.Status,
                StatusText = ((CodeItemStatus)x.Status).ToString(),
                OrderItemId = x.OrderItemId,
                OrderInfo = x.OrderItemId.HasValue ? $"OrderItem #{x.OrderItemId}" : null,
                ReservedUntilUtc = x.ReservedUntilUtc,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedOnUtc = x.UpdatedOnUtc
            });
        });

        return Json(model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodeItemCreate(int codePoolId)
    {
        var pool = await _codePoolService.GetByIdAsync(codePoolId);
        if (pool == null)
            return RedirectToAction("CodeItems");
        var model = new CodeItemModel
        {
            CodePoolId = codePoolId,
            CodePoolName = pool.Name
        };
        return View("~/Plugins/Misc.DigitalCodes/Views/CodeItemCreate.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodeItemCreate(CodeItemModel model)
    {
        if (!ModelState.IsValid)
        {
            var poolForInvalid = await _codePoolService.GetByIdAsync(model.CodePoolId);
            model.CodePoolName = poolForInvalid?.Name;
            return View("~/Plugins/Misc.DigitalCodes/Views/CodeItemCreate.cshtml", model);
        }

        try
        {
            var entity = new Domain.CodeItem
            {
                CodePoolId = model.CodePoolId,
                Code = model.Code,
                Pin = model.Pin,
                Serial = model.Serial,
                ExpireOnUtc = model.ExpireOnUtc,
                Status = (int)CodeItemStatus.Available
            };
            await _codeItemService.InsertAsync(entity);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.DataSaved"));
            return RedirectToAction("CodeItems", new { codePoolId = model.CodePoolId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            _notificationService.ErrorNotification(ex.Message);
            var pool = await _codePoolService.GetByIdAsync(model.CodePoolId);
            model.CodePoolName = pool?.Name;
            return View("~/Plugins/Misc.DigitalCodes/Views/CodeItemCreate.cshtml", model);
        }
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodeItemEdit(int id)
    {
        var entity = await _codeItemService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("CodeItems");

        var pool = await _codePoolService.GetByIdAsync(entity.CodePoolId);
        var model = new CodeItemModel
        {
            Id = entity.Id,
            CodePoolId = entity.CodePoolId,
            CodePoolName = pool?.Name,
            Code = entity.Code,
            Pin = entity.Pin,
            Serial = entity.Serial,
            ExpireOnUtc = entity.ExpireOnUtc,
            Status = entity.Status,
            StatusText = ((CodeItemStatus)entity.Status).ToString(),
            OrderItemId = entity.OrderItemId,
            ReservedUntilUtc = entity.ReservedUntilUtc,
            CreatedOnUtc = entity.CreatedOnUtc,
            UpdatedOnUtc = entity.UpdatedOnUtc
        };
        return View("~/Plugins/Misc.DigitalCodes/Views/CodeItemEdit.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodeItemEdit(CodeItemModel model)
    {
        var entity = await _codeItemService.GetByIdAsync(model.Id);
        if (entity == null)
            return RedirectToAction("CodeItems");

        if (!ModelState.IsValid)
            return View("~/Plugins/Misc.DigitalCodes/Views/CodeItemEdit.cshtml", model);

        entity.Code = model.Code;
        entity.Pin = model.Pin;
        entity.Serial = model.Serial;
        entity.ExpireOnUtc = model.ExpireOnUtc;
        await _codeItemService.UpdateAsync(entity);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Common.DataSaved"));
        return RedirectToAction("CodeItems", new { codePoolId = entity.CodePoolId });
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> CodeItemDelete(int id)
    {
        var entity = await _codeItemService.GetByIdAsync(id);
        if (entity == null)
            return Json(new { Result = false });

        // Sadece Available ise silmeye izin ver
        if (entity.Status != (int)CodeItemStatus.Available)
            return Json(new { Result = false, Message = await _localizationService.GetResourceAsync("Plugins.Misc.DigitalCodes.Errors.CannotDeleteNonAvailable") });

        await _codeItemService.DeleteAsync(entity);
        return Json(new { Result = true });
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public IActionResult DeliveryLogs()
    {
        var model = new DeliveryLogSearchModel();
        return View("~/Plugins/Misc.DigitalCodes/Views/DeliveryLogs.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> DeliveryLogList(DeliveryLogSearchModel searchModel)
    {
        var logs = await _deliveryLogService.SearchAsync(searchModel.SearchOrderId, searchModel.SearchOrderItemId, searchModel.SearchCodeItemId, searchModel.SearchChannel, searchModel.Page - 1, searchModel.PageSize);

        var model = new DeliveryLogListModel().PrepareToGrid(searchModel, logs, () =>
        {
            return logs.Select(x => new DeliveryLogModel
            {
                Id = x.Id,
                OrderId = x.OrderId,
                OrderItemId = x.OrderItemId,
                CodeItemId = x.CodeItemId,
                Channel = x.Channel,
                To = x.To,
                Result = x.Result,
                Message = x.Message,
                CreatedOnUtc = x.CreatedOnUtc
            });
        });
        return Json(model);
    }

}