using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.DigitalCodes.Domain;
using Nop.Plugin.Misc.DigitalCodes.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Common;
using Nop.Core.Domain.Messages;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

/// <summary>
/// Dijital kod teslimat servisi: Birincil/ikincil kanal (Webhook/Email) mantığı ile gönderim yapar
/// </summary>
public class DigitalCodesDeliveryService
{
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IWorkflowMessageService _workflowMessageService;
    private readonly IMessageTokenProvider _messageTokenProvider;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly IDigitalCodesWebhookClient _webhookClient;
    private readonly IRepository<DeliveryLog> _deliveryLogRepository;
    private readonly ILogger<DigitalCodesDeliveryService> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICustomerService _customerService;
    private readonly IAddressService _addressService;

    public DigitalCodesDeliveryService(
        IMessageTemplateService messageTemplateService,
        IWorkflowMessageService workflowMessageService,
        IMessageTokenProvider messageTokenProvider,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        IDigitalCodesWebhookClient webhookClient,
        IRepository<DeliveryLog> deliveryLogRepository,
        ILogger<DigitalCodesDeliveryService> logger,
        IEventPublisher eventPublisher,
        ICustomerService customerService,
        IAddressService addressService)
    {
        _messageTemplateService = messageTemplateService;
        _workflowMessageService = workflowMessageService;
        _messageTokenProvider = messageTokenProvider;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _webhookClient = webhookClient;
        _deliveryLogRepository = deliveryLogRepository;
        _logger = logger;
        _eventPublisher = eventPublisher;
        _customerService = customerService;
        _addressService = addressService;
    }

    public async Task<bool> DeliverAsync(Order order, Customer customer, IList<CodeItem> codes, DigitalCodesSettings settings)
    {
        // Varsayılan: Webhook açıksa birincil kanal Webhook, başarısız olursa e-posta.
        var store = await _storeContext.GetCurrentStoreAsync();

        // Email hazırlığı
        var emailSent = false;
        var webhookSent = false;

        if (settings.EnableWebhook)
        {
            webhookSent = await TrySendWebhookAsync(order, customer, codes, settings);
        }

        if (!webhookSent)
        {
            emailSent = await TrySendEmailAsync(order, customer, codes, store.Id);
        }
        else
        {
            // Birlikte çalışma talebi varsa ikincil olarak e-posta da gönderilebilir
            // İleride ayarla ile kontrol edilebilir. Şimdilik başarılı webhook sonrası e-posta göndermiyoruz.
        }

        if (!webhookSent && !emailSent)
        {
            _logger.LogWarning("Dijital kod teslimatı başarısız. OrderId={OrderId}", order.Id);
        }

        return webhookSent || emailSent;
    }

    private async Task<bool> TrySendEmailAsync(Order order, Customer customer, IList<CodeItem> codes, int storeId)
    {
        var template = (await _messageTemplateService.GetMessageTemplatesByNameAsync(DigitalCodesDefaults.MessageTemplateSystemName, storeId)).FirstOrDefault();
        if (template == null || !template.IsActive)
        {
            _logger.LogWarning("Mesaj şablonu bulunamadı/aktif değil: {Name}", DigitalCodesDefaults.MessageTemplateSystemName);
            return false;
        }

        var languageId = 0; // varsayılan dil
        var tokens = new List<Token>();

        await _messageTokenProvider.AddOrderTokensAsync(tokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(tokens, customer);
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        // EmailAccount parametresi token imzasında bekleniyor; burada null geçebiliriz çünkü gerçek gönderim sırasında hesap belirlenecek
        await _messageTokenProvider.AddStoreTokensAsync(tokens, currentStore, null, languageId);

        // Özel token: %DigitalCodes.Items%
        var itemsHtml = BuildItemsHtml(codes);
        tokens.Add(new Token("DigitalCodes.Items", itemsHtml, true));

        // Olay bildirimi (başka dinleyiciler için)
        await _eventPublisher.MessageTokensAddedAsync(template, tokens);

        var billing = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        var toEmail = billing?.Email ?? customer?.Email ?? string.Empty;
        var toName = billing != null 
            ? string.Join(" ", new[] { billing.FirstName, billing.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))) 
            : await _customerService.GetCustomerFullNameAsync(customer);

        var emailAccount = await EngineContext.Current.Resolve<IEmailAccountService>().GetEmailAccountByIdAsync(template.EmailAccountId)
                           ?? await EngineContext.Current.Resolve<IEmailAccountService>().GetEmailAccountByIdAsync(EngineContext.Current.Resolve<EmailAccountSettings>().DefaultEmailAccountId);

        var queuedId = await _workflowMessageService.SendNotificationAsync(template, emailAccount, languageId, tokens, toEmail, toName);

        // Log (her kod için)
        foreach (var code in codes)
        {
            await _deliveryLogRepository.InsertAsync(new DeliveryLog
            {
                OrderId = order.Id,
                OrderItemId = code.OrderItemId ?? 0,
                CodeItemId = code.Id,
                Channel = "Email",
                To = toEmail,
                Result = "Queued",
                Message = MaskCodeForLog(code.Code),
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        return queuedId > 0;
    }

    private async Task<bool> TrySendWebhookAsync(Order order, Customer customer, IList<CodeItem> codes, DigitalCodesSettings settings)
    {
        var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
        
        var payload = new
        {
            order = new { id = order.Id, number = order.CustomOrderNumber, createdOnUtc = order.CreatedOnUtc },
            customer = new { id = customer.Id, email = customer.Email, fullName = customerFullName },
            codes = codes.Select(c => new { id = c.Id, code = c.Code, pin = c.Pin, serial = c.Serial, expireOnUtc = c.ExpireOnUtc, orderItemId = c.OrderItemId })
        };

        var (success, response, status) = await _webhookClient.PostAsync(payload, settings.WebhookUrl, settings.Secret, settings.TimeoutSeconds, settings.RetryCount);

        foreach (var code in codes)
        {
            await _deliveryLogRepository.InsertAsync(new DeliveryLog
            {
                OrderId = order.Id,
                OrderItemId = code.OrderItemId ?? 0,
                CodeItemId = code.Id,
                Channel = "Webhook",
                To = settings.WebhookUrl,
                Result = success ? "OK" : ($"HTTP {(status == 0 ? "ERR" : status)}"),
                Message = MaskCodeForLog(code.Code),
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        return success;
    }

    private static string BuildItemsHtml(IList<CodeItem> codes)
    {
        var sb = new StringBuilder();
        sb.Append("<ul>");
        foreach (var c in codes)
        {
            sb.Append("<li>");
            sb.Append($"<strong>Kod:</strong> {System.Net.WebUtility.HtmlEncode(c.Code)}");
            if (!string.IsNullOrEmpty(c.Pin)) sb.Append($" | <strong>PIN:</strong> {System.Net.WebUtility.HtmlEncode(c.Pin)}");
            if (!string.IsNullOrEmpty(c.Serial)) sb.Append($" | <strong>Seri:</strong> {System.Net.WebUtility.HtmlEncode(c.Serial)}");
            if (c.ExpireOnUtc.HasValue) sb.Append($" | <strong>Son Kullanım:</strong> {c.ExpireOnUtc.Value.ToString("d", CultureInfo.GetCultureInfo("tr-TR"))}");
            sb.Append("</li>");
        }
        sb.Append("</ul>");
        return sb.ToString();
    }

    private static string MaskCodeForLog(string code)
    {
        if (string.IsNullOrEmpty(code)) return string.Empty;
        var last4 = code.Length <= 4 ? code : code[^4..];
        return $"***{last4}";
    }
}