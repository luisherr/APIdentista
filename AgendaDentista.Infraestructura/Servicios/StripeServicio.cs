using AgendaDentista.Dominio.Entidades;
using AgendaDentista.Dominio.Interfaces;
using AgendaDentista.Infraestructura.Configuraciones;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace AgendaDentista.Infraestructura.Servicios;

public interface IStripeServicio
{
    Task<string> CrearCheckoutSessionAsync(Dentista dentista);
    Task ProcesarWebhookAsync(string json, string signature);
}

public class StripeServicio : IStripeServicio
{
    private readonly StripeConfiguracion _config;
    private readonly IDentistaRepositorio _dentistaRepo;
    private readonly ILogger<StripeServicio> _logger;

    public StripeServicio(
        IOptions<StripeConfiguracion> config,
        IDentistaRepositorio dentistaRepo,
        ILogger<StripeServicio> logger)
    {
        _config = config.Value;
        _dentistaRepo = dentistaRepo;
        _logger = logger;
        StripeConfiguration.ApiKey = _config.SecretKey;
    }

    public async Task<string> CrearCheckoutSessionAsync(Dentista dentista)
    {
        // Crear o reutilizar Stripe Customer
        var customerId = dentista.StripeCustomerId;
        if (string.IsNullOrEmpty(customerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = dentista.Email,
                Name = dentista.Nombre,
                Metadata = new Dictionary<string, string>
                {
                    { "idDentista", dentista.IdDentista.ToString() }
                }
            });
            customerId = customer.Id;
            dentista.StripeCustomerId = customerId;
            await _dentistaRepo.ActualizarAsync(dentista);
        }

        // Crear Checkout Session
        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Customer = customerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = _config.PriceId,
                    Quantity = 1,
                }
            },
            Mode = "subscription",
            SuccessUrl = _config.SuccessUrl,
            CancelUrl = _config.CancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "idDentista", dentista.IdDentista.ToString() }
            }
        });

        _logger.LogInformation("Checkout session creada para dentista {Id}: {SessionId}", dentista.IdDentista, session.Id);
        return session.Url;
    }

    public async Task ProcesarWebhookAsync(string json, string signature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _config.WebhookSecret);

        _logger.LogInformation("Stripe webhook recibido: {Type}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                await ProcesarCheckoutCompletado(stripeEvent);
                break;
            case EventTypes.InvoicePaid:
                await ProcesarFacturaPagada(stripeEvent);
                break;
            case EventTypes.CustomerSubscriptionDeleted:
                await ProcesarSuscripcionCancelada(stripeEvent);
                break;
            default:
                _logger.LogInformation("Evento Stripe no manejado: {Type}", stripeEvent.Type);
                break;
        }
    }

    private async Task ProcesarCheckoutCompletado(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null) return;

        var idDentista = int.Parse(session.Metadata["idDentista"]);
        var dentista = await _dentistaRepo.ObtenerPorIdAsync(idDentista);
        if (dentista == null) return;

        dentista.SuscripcionActiva = true;
        dentista.StripeSubscriptionId = session.SubscriptionId;
        dentista.StripeCustomerId = session.CustomerId;

        await _dentistaRepo.ActualizarAsync(dentista);
        _logger.LogInformation("Suscripción activada para dentista {Id}", idDentista);
    }

    private async Task ProcesarFacturaPagada(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var customerId = invoice.CustomerId;
        if (string.IsNullOrEmpty(customerId)) return;

        var subscriptionId = invoice.Parent?.SubscriptionDetails?.SubscriptionId;

        // Buscar dentista por StripeCustomerId
        var dentistas = await _dentistaRepo.ObtenerTodosAsync();
        var dentista = dentistas.FirstOrDefault(d => d.StripeCustomerId == customerId);
        if (dentista == null) return;

        dentista.SuscripcionActiva = true;
        dentista.StripeSubscriptionId = subscriptionId;
        dentista.FechaFinSuscripcion = DateTime.UtcNow.AddDays(30);
        await _dentistaRepo.ActualizarAsync(dentista);
        _logger.LogInformation("Factura pagada - suscripción renovada para dentista {Id}", dentista.IdDentista);
    }

    private async Task ProcesarSuscripcionCancelada(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var dentistas = await _dentistaRepo.ObtenerTodosAsync();
        var dentista = dentistas.FirstOrDefault(d => d.StripeSubscriptionId == subscription.Id);
        if (dentista == null) return;

        dentista.SuscripcionActiva = false;
        dentista.StripeSubscriptionId = null;
        dentista.FechaFinSuscripcion = null;
        await _dentistaRepo.ActualizarAsync(dentista);
        _logger.LogInformation("Suscripción cancelada para dentista {Id}", dentista.IdDentista);
    }
}
