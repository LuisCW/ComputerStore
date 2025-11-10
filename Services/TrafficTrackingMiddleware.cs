using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Services;

public class TrafficTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly PathString[] _ignorePrefixes =
    [
        new PathString("/css"),
        new PathString("/js"),
        new PathString("/lib"),
        new PathString("/images"),
        new PathString("/favicon"),
        new PathString("/swagger"),
        new PathString("/healthz")
    ];

    public TrafficTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ApplicationDbContext db)
    {
        var path = context.Request.Path.ToString();
        var shouldTrack = ShouldTrack(context.Request.Method, path);

        // Ejecutar siguiente middleware primero si queremos capturar status code
        if (!shouldTrack)
        {
            await _next(context);
            return;
        }

        // Asegurar sesión (forzar cookie escribiendo un valor mínimo una sola vez)
        if (!context.Session.Keys.Contains("_tv"))
        {
            context.Session.SetString("_tv", "1");
        }
        var sessionId = context.Session.Id; // ahora sí persistirá

        var referer = context.Request.Headers["Referer"].FirstOrDefault();
        var ua = context.Request.Headers["User-Agent"].FirstOrDefault();
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var userId = context.User?.Identity?.IsAuthenticated == true ?
            ( context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ) : null;

        // UTM params
        var qs = context.Request.Query;
        var source = qs.TryGetValue("utm_source", out var _s) ? _s.ToString() : null;
        var medium = qs.TryGetValue("utm_medium", out var _m) ? _m.ToString() : null;
        var campaign = qs.TryGetValue("utm_campaign", out var _c) ? _c.ToString() : null;

        // Device detection mejorado
        string device = DetectDevice(ua);

        // Source detection mejorado si no hay UTM
        if (string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(referer))
        {
            source = DetectSourceFromReferer(referer);
            if (source != "direct") medium = "referral";
        }
        if (string.IsNullOrEmpty(source)) source = "direct";

        // Crear entidad antes del _next para mínima latencia en escritura posterior
        var tev = new TrafficEvent
        {
            Fecha = DateTime.UtcNow,
            Path = path.Length > 300 ? path[..300] : path,
            Referrer = Truncate(referer, 500),
            UserAgent = Truncate(ua, 500),
            Ip = Truncate(ip, 50),
            SessionId = sessionId,
            UserId = userId,
            Device = device,
            Source = Truncate(source, 100),
            Medium = Truncate(medium, 100),
            Campaign = Truncate(campaign, 100)
        };

        await _next(context);

        // Opcional: podríamos condicionar por status code
        var status = context.Response.StatusCode;
        if (status >= 400) return; // no registrar errores

        try
        {
            db.TrafficEvents.Add(tev);
            await db.SaveChangesAsync();
        }
        catch
        {
            // Ignorar errores de tracking
        }
    }

    private static bool ShouldTrack(string method, string path)
    {
        if (!HttpMethods.IsGet(method)) return false; // Solo GET para simplicidad
        if (string.IsNullOrWhiteSpace(path)) return false;

        // Evitar tracking de la mayor parte del admin excepto páginas de Analytics públicas internas
        if (path.StartsWith("/Admin/Api", StringComparison.OrdinalIgnoreCase)) return false; // APIs admin
        if (path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) && !path.StartsWith("/Admin/Analytics", StringComparison.OrdinalIgnoreCase)) return false;

        // Ignorar recursos estáticos
        if (_ignorePrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase))) return false;

        return true;
    }

    private static string? Truncate(string? val, int max)
    {
        if (string.IsNullOrEmpty(val)) return val;
        return val.Length <= max ? val : val[..max];
    }

    private static string DetectDevice(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return "Unknown";

        var ua = userAgent.ToLowerInvariant();

        // Detectar bots primero
        if (ua.Contains("bot") || ua.Contains("crawler") || ua.Contains("spider"))
            return "Bot";

        // iOS específico
        if (ua.Contains("iphone") || ua.Contains("ipod") || ua.Contains("ipad"))
            return "iOS";

        // Android específico
        if (ua.Contains("android"))
            return "Android";

        // Windows específico - TODAS las versiones se mapean a Windows
        if (ua.Contains("windows"))
            return "Windows";

        // macOS específico
        if (ua.Contains("mac os") || ua.Contains("macintosh"))
            return "Mac";

        // Linux específico
        if (ua.Contains("linux") && !ua.Contains("android"))
            return "Linux";

        // Si no detectamos nada específico pero parece escritorio, es Windows
        if ((ua.Contains("mozilla") || ua.Contains("firefox") || ua.Contains("chrome"))
            && !ua.Contains("mobile"))
            return "Windows";

        return "Other";
    }

    private static string DetectSourceFromReferer(string referer)
    {
        try
        {
            var uri = new Uri(referer);
            var domain = uri.Host.ToLower();

            // WhatsApp y mensajería - MEJORADO
            if (domain.Contains("whatsapp") || 
                referer.Contains("whatsapp") || 
                domain.Contains("wa.me") ||
                referer.ToLower().Contains("whatsapp"))
                return "whatsapp";
            
            if (domain.Contains("telegram") || domain.Contains("t.me"))
                return "telegram";
            
            if (domain.Contains("messenger") || domain.Contains("m.me"))
                return "messenger";

            // Redes sociales - EXPANDIDO
            if (domain.Contains("facebook.com") || domain.Contains("fb.com") || domain.Contains("fbclid"))
                return "facebook";
            if (domain.Contains("instagram.com"))
                return "instagram";
            if (domain.Contains("twitter.com") || domain.Contains("t.co") || domain.Contains("x.com"))
                return "twitter";
            if (domain.Contains("linkedin.com"))
                return "linkedin";
            if (domain.Contains("tiktok.com"))
                return "tiktok";
            if (domain.Contains("youtube.com") || domain.Contains("youtu.be"))
                return "youtube";
            if (domain.Contains("pinterest.com") || domain.Contains("pin.it"))
                return "pinterest";
            if (domain.Contains("snapchat.com"))
                return "snapchat";

            // Motores de búsqueda - EXPANDIDO
            if (domain.Contains("google.") || domain.Contains("googleusercontent."))
                return "google";
            if (domain.Contains("bing.com") || domain.Contains("msn.com"))
                return "bing";
            if (domain.Contains("yahoo.com") || domain.Contains("search.yahoo"))
                return "yahoo";
            if (domain.Contains("duckduckgo.com"))
                return "duckduckgo";
            if (domain.Contains("baidu.com"))
                return "baidu";

            // E-commerce y marketplace
            if (domain.Contains("mercadolibre.") || domain.Contains("mercadopago."))
                return "mercadolibre";
            if (domain.Contains("amazon."))
                return "amazon";
            if (domain.Contains("olx.com") || domain.Contains("olx."))
                return "olx";

            // Foros y comunidades
            if (domain.Contains("reddit.com"))
                return "reddit";
            if (domain.Contains("stackoverflow.com"))
                return "stackoverflow";
            if (domain.Contains("github.com"))
                return "github";

            // Email marketing
            if (domain.Contains("mailchimp") || domain.Contains("sendinblue") || 
                domain.Contains("constantcontact") || domain.Contains("campaign"))
                return "email";

            // Colombia específico
            if (domain.Contains("eltiempo.com") || domain.Contains("semana.com") || 
                domain.Contains("caracol.") || domain.Contains("rcn."))
                return "noticias_colombia";

            // Si es un dominio conocido pero no categorizado, usar el dominio
            if (domain.Length > 0 && domain.Contains("."))
            {
                var parts = domain.Split('.');
                if (parts.Length >= 2)
                {
                    // Extraer dominio principal (ej: google de google.com)
                    var mainDomain = parts[parts.Length - 2];
                    if (mainDomain.Length > 2) return mainDomain;
                }
            }

            return domain;
        }
        catch
        {
            return "unknown";
        }
    }
}

public static class TrafficTrackingExtensions
{
    public static IApplicationBuilder UseTrafficTracking(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TrafficTrackingMiddleware>();
    }
}