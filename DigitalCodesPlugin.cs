using Nop.Services.Localization;
using Nop.Services.Plugins;
using System.Linq;
using Nop.Core.Domain.Messages;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Plugin.Misc.DigitalCodes.Infrastructure;
using Nop.Core;

namespace Nop.Plugin.Misc.DigitalCodes;

/// <summary>
/// E-pin / Dijital Kod teslimatı için temel plugin iskeleti
/// </summary>
public class DigitalCodesPlugin : BasePlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IStoreService _storeService;
    private readonly IWebHelper _webHelper;

    public DigitalCodesPlugin(ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        IStoreService storeService,
        IWebHelper webHelper)
    {
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _storeService = storeService;
        _webHelper = webHelper;
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/DigitalCodes/Configure";
    }

    public override async Task InstallAsync()
    {
        // TR kaynaklar
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.DigitalCodes"] = "Dijital Kodlar",
            ["Plugins.Misc.DigitalCodes.Description"] = "E‑pin / dijital kod teslimatı",
            ["Plugins.Misc.DigitalCodes.Fields.PoolName"] = "Havuz Adı",
            ["Plugins.Misc.DigitalCodes.Fields.Product"] = "Ürün",
            ["Plugins.Misc.DigitalCodes.Fields.IsActive"] = "Aktif",
            // EKSİK ALAN ANAHTARLARI
            ["Plugins.Misc.DigitalCodes.Fields.CodePool"] = "Havuz",
            ["Plugins.Misc.DigitalCodes.Fields.Code"] = "Kod",
            ["Plugins.Misc.DigitalCodes.Fields.Pin"] = "Pin",
            ["Plugins.Misc.DigitalCodes.Fields.Serial"] = "Seri",
            ["Plugins.Misc.DigitalCodes.Fields.ExpireOnUtc"] = "Bitiş Tarihi",
            ["Plugins.Misc.DigitalCodes.Fields.Status"] = "Durum",
            ["Plugins.Misc.DigitalCodes.Fields.ReservedUntilUtc"] = "Rezerv Bitişi",

            ["Plugins.Misc.DigitalCodes.Menu.Manage"] = "Dijital Kod Yönetimi",
            ["Plugins.Misc.DigitalCodes.Email.Subject"] = "{Store.Name} siparişiniz için dijital kodlar",
            ["Plugins.Misc.DigitalCodes.Email.Body.Intro"] = "Merhaba {Customer.FullName}, siparişiniz için dijital kodlar aşağıdadır:",
            ["Plugins.Misc.DigitalCodes.Email.Body.Footer"] = "İyi eğlenceler! {Store.Name}",
            ["Plugins.Misc.DigitalCodes.CodePools"] = "Dijital Kod Havuzları",
            ["Plugins.Misc.DigitalCodes.CodePools.Create"] = "Havuz Oluştur",
            ["Plugins.Misc.DigitalCodes.CodePools.Edit"] = "Havuz Düzenle",
            ["Plugins.Misc.DigitalCodes.Errors.CannotDeleteNonAvailable"] = "Sadece 'Uygun' durumdaki kod silinebilir.",
            
            // Configure ekranı alanları
            ["Plugins.Misc.DigitalCodes.Fields.EnableWebhook"] = "Webhook ile teslimatı etkinleştir",
            ["Plugins.Misc.DigitalCodes.Fields.EnableWebhook.Hint"] = "Sipariş ödendiğinde webhook çağrısı yapılmasını sağlar.",
            ["Plugins.Misc.DigitalCodes.Fields.WebhookUrl"] = "Webhook URL",
            ["Plugins.Misc.DigitalCodes.Fields.WebhookUrl.Hint"] = "POST çağrısı yapılacak adres.",
            ["Plugins.Misc.DigitalCodes.Fields.Secret"] = "İmzalama Sırrı",
            ["Plugins.Misc.DigitalCodes.Fields.Secret.Hint"] = "HMAC imzası için paylaşılan sır.",
            ["Plugins.Misc.DigitalCodes.Fields.TimeoutSeconds"] = "Zaman Aşımı (sn)",
            ["Plugins.Misc.DigitalCodes.Fields.TimeoutSeconds.Hint"] = "Webhook isteği zaman aşımı.",
            ["Plugins.Misc.DigitalCodes.Fields.RetryCount"] = "Tekrar Sayısı",
            ["Plugins.Misc.DigitalCodes.Fields.RetryCount.Hint"] = "Başarısız çağrı için maksimum tekrar.",

            // Liste ve log ekranları alanları
            ["Plugins.Misc.DigitalCodes.CodeItems"] = "Dijital Kod Kalemleri",
            ["Plugins.Misc.DigitalCodes.DeliveryLogs"] = "Teslimat Logları",
            ["Plugins.Misc.DigitalCodes.Fields.OrderId"] = "Sipariş",
            ["Plugins.Misc.DigitalCodes.Fields.OrderItemId"] = "Sipariş Kalemi",
            ["Plugins.Misc.DigitalCodes.Fields.CodeItemId"] = "Kod Kalemi",
            ["Plugins.Misc.DigitalCodes.Fields.Channel"] = "Kanal",
            ["Plugins.Misc.DigitalCodes.Fields.To"] = "Hedef",
            ["Plugins.Misc.DigitalCodes.Fields.Result"] = "Sonuç",
            ["Plugins.Misc.DigitalCodes.Fields.Message"] = "Mesaj",

            // Admin ortak anahtarlar (TR)
            ["Admin.Common.Search"] = "Ara",
            ["Admin.Common.AddNew"] = "Yeni Ekle",
            ["Admin.Catalog.Products.Fields.Product"] = "Ürün",
            ["Admin.Common.Active"] = "Aktif",
            ["Admin.Common.CreatedOn"] = "Oluşturulma",
            ["Admin.Common.UpdatedOn"] = "Güncellenme",
            ["Admin.Common.Edit"] = "Düzenle",
            ["Admin.Common.Delete"] = "Sil",
            ["Admin.Common.Info"] = "Bilgi",
            ["Admin.Common.Clear"] = "Temizle",
            ["Admin.Common.Save"] = "Kaydet",
            ["Admin.Common.BackToList"] = "Listeye geri dön"
        });

        // Mesaj şablonu
        var stores = await _storeService.GetAllStoresAsync();
        foreach (var store in stores)
        {
            var existing = (await _messageTemplateService.GetMessageTemplatesByNameAsync(DigitalCodesDefaults.MessageTemplateSystemName, store.Id)).FirstOrDefault();
            if (existing is null)
            {
                var template = new MessageTemplate
                {
                    Name = DigitalCodesDefaults.MessageTemplateSystemName,
                    BccEmailAddresses = null,
                    Subject = "%Plugins.Misc.DigitalCodes.Email.Subject%",
                    Body = "%Plugins.Misc.DigitalCodes.Email.Body.Intro%<br/>" + DigitalCodesDefaults.ItemsToken + "<br/>%Plugins.Misc.DigitalCodes.Email.Body.Footer%",
                    IsActive = true,
                    EmailAccountId = 0,
                    LimitedToStores = false,
                };
                await _messageTemplateService.InsertMessageTemplateAsync(template);
            }
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.DigitalCodes");

        // Mesaj şablonunu kaldır
        var stores = await _storeService.GetAllStoresAsync();
        foreach (var store in stores)
        {
            var templates = await _messageTemplateService.GetMessageTemplatesByNameAsync(DigitalCodesDefaults.MessageTemplateSystemName, store.Id);
            foreach (var t in templates)
                await _messageTemplateService.DeleteMessageTemplateAsync(t);
        }

        await base.UninstallAsync();
    }
}