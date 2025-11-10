using ComputerStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin.Api
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnalyticsModel> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _analyticsUrl;

        public AnalyticsModel(ApplicationDbContext context, ILogger<AnalyticsModel> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _analyticsUrl = Environment.GetEnvironmentVariable("ANALYTICS_URL") ?? "http://127.0.0.1:5001/api/analytics";
        }

        private sealed record AnalyticsFilters(
            HashSet<string> Categorias,
            HashSet<string> Estados,
            HashSet<string> Metodos,
            HashSet<string> Meses,
            HashSet<string> Ciudades,
            int? ProductId
        );

        private AnalyticsFilters ParseFilters()
        {
            HashSet<string> ToSet(string key)
            {
                var raw = Request.Query[key].ToString();
                if (string.IsNullOrWhiteSpace(raw)) return new();
                return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                          .Select(s => s.Trim())
                          .Where(s => s.Length > 0)
                          .Select(s => s.ToUpperInvariant())
                          .ToHashSet();
            }

            int? prodId = null; if (int.TryParse(Request.Query["prodId"], out var pid)) prodId = pid;
            return new AnalyticsFilters(
                ToSet("cat"),
                ToSet("estado"),
                ToSet("metodo"),
                ToSet("mes"),
                ToSet("ciudad"),
                prodId
            );
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var mode = Request.Query["mode"].ToString();
            var filters = ParseFilters();
            if (string.Equals(mode, "ef", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(mode))
            {
                var efPayload = await BuildAnalyticsFromEfAsync(filters: filters);
                return new JsonResult(efPayload);
            }
            if (string.Equals(mode, "demo", StringComparison.OrdinalIgnoreCase))
            {
                var demo = await BuildAnalyticsFromEfAsync(forceDemo: true, filters: filters);
                return new JsonResult(demo);
            }
            if (string.Equals(mode, "python", StringComparison.OrdinalIgnoreCase))
            {
                var api = await TryCallPythonApiAsync();
                if (api != null) return Content(api, "application/json");
                var efPayload = await BuildAnalyticsFromEfAsync(filters: filters);
                return new JsonResult(efPayload);
            }
            var payload = await BuildAnalyticsFromEfAsync(filters: filters);
            return new JsonResult(payload);
        }

        public async Task<IActionResult> OnGetHealthAsync()
        {
            var baseUrl = _analyticsUrl.EndsWith("/api/analytics", StringComparison.OrdinalIgnoreCase)
                ? _analyticsUrl.Replace("/api/analytics", "")
                : _analyticsUrl.TrimEnd('/');
            var healthUrl = baseUrl + "/healthz";
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                var res = await client.GetAsync(healthUrl);
                var ok = res.IsSuccessStatusCode;
                var body = await res.Content.ReadAsStringAsync();
                return new JsonResult(new { ok, status = (int)res.StatusCode, body, url = healthUrl, configured = _analyticsUrl });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { ok = false, error = ex.Message, url = healthUrl, configured = _analyticsUrl });
            }
        }

        private async Task<string?> TryCallPythonApiAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(8);
                var res = await client.GetAsync(_analyticsUrl);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Analytics API respondió {Status}", res.StatusCode);
                    return null;
                }
                return await res.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo contactar Analytics API en {Url}", _analyticsUrl);
                return null;
            }
        }

        private async Task<object> BuildAnalyticsFromEfAsync(bool forceDemo = false, AnalyticsFilters? filters = null)
        {
            filters ??= new AnalyticsFilters(new(), new(), new(), new(), new(), null);
            var debug = new List<string>();
            if (filters.Categorias.Count > 0) debug.Add($"F_CAT={string.Join('|', filters.Categorias)}");
            if (filters.Estados.Count > 0) debug.Add($"F_EST={string.Join('|', filters.Estados)}");
            if (filters.Metodos.Count > 0) debug.Add($"F_MET={string.Join('|', filters.Metodos)}");
            if (filters.Meses.Count > 0) debug.Add($"F_MES={string.Join('|', filters.Meses)}");
            if (filters.Ciudades.Count > 0) debug.Add($"F_CITY={string.Join('|', filters.Ciudades)}");
            if (filters.ProductId.HasValue) debug.Add($"F_PROD={filters.ProductId.Value}");

            var now = DateTime.UtcNow.Date;
            var firstMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
            var months = Enumerable.Range(0, 12).Select(i => firstMonth.AddMonths(i)).ToList();
            var monthLabels = months.Select(m => $"{m:yyyy-MM}").ToList();

            var okEstados = new[] { "PAGADO", "COMPLETADO", "ENTREGADO", "APROBADO", "APPROVED" };

            var pedidosRaw = await _context.Pedidos.AsNoTracking()
                .Select(p => new { p.Id, p.Total, Estado = p.Estado.ToUpper(), p.FechaCreacion, p.UserId, p.MetodoPago, p.CiudadEnvio, p.DepartamentoEnvio })
                .ToListAsync();
            var transRaw = await _context.Transacciones.AsNoTracking()
                .Select(t => new { t.TransactionId, Estado = t.Estado.ToUpper(), t.MetodoPago, t.Monto, t.FechaTransaccion })
                .ToListAsync();
            var detalles = await _context.PedidoDetalles.AsNoTracking()
                .Select(d => new { d.ProductoId, d.Cantidad, d.Subtotal, d.PedidoId })
                .ToListAsync();
            var prods = await _context.Productos.AsNoTracking()
                .Select(p => new { p.Id, p.Name, p.Category, p.Stock, p.StockMinimo, p.Price })
                .ToListAsync();
            var users = await _context.Users.AsNoTracking()
                .Select(u => new { u.Id, u.FechaRegistro, u.PrimerNombre, u.PrimerApellido, u.UserName, u.Email, u.Ciudad, u.Departamento })
                .ToListAsync();

            var fecha30Dias = now.AddDays(-29);
            var trafficEvents30 = await _context.TrafficEvents.AsNoTracking()
                .Where(t => t.Fecha >= fecha30Dias)
                .Select(t => new { Fecha = t.Fecha.Date, t.Source, t.Device, t.Path, t.SessionId, t.Ip, t.UserId, t.UserAgent, t.Referrer })
                .ToListAsync();

            debug.Add($"TRAFFIC_EVENTS_30D={trafficEvents30.Count}");
            debug.Add($"PEDIDOS_REALES={pedidosRaw.Count}");
            debug.Add($"TRANSACCIONES_REALES={transRaw.Count}");

            // Aplicar filtros a pedidos
            var pedidosList = pedidosRaw.ToList();
            if (filters.Meses.Count > 0)
                pedidosList = pedidosList.Where(p => filters.Meses.Contains(((DateTime)p.FechaCreacion).ToString("yyyy-MM").ToUpperInvariant())).ToList();
            if (filters.Estados.Count > 0)
                pedidosList = pedidosList.Where(p => filters.Estados.Contains(((string)p.Estado).ToUpperInvariant())).ToList();
            if (filters.Metodos.Count > 0)
                pedidosList = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.MetodoPago) && filters.Metodos.Contains(((string)p.MetodoPago).ToUpperInvariant())).ToList();
            if (filters.Ciudades.Count > 0)
                pedidosList = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.CiudadEnvio) && filters.Ciudades.Contains(((string)p.CiudadEnvio).ToUpperInvariant())).ToList();

            var pedidoIdsFiltrados = pedidosList.Select(p => (int)p.Id).ToList();
            if (filters.ProductId.HasValue)
            {
                var idsPorProducto = detalles.Where(d => d.ProductoId == filters.ProductId.Value).Select(d => d.PedidoId).Distinct().ToList();
                pedidoIdsFiltrados = pedidoIdsFiltrados.Intersect(idsPorProducto).ToList();
            }
            if (filters.Categorias.Count > 0)
            {
                var idsPorCategoria = detalles
                    .Join(prods, d => d.ProductoId, p => p.Id, (d, p) => new { d.PedidoId, p.Category })
                    .Where(x => !string.IsNullOrEmpty(x.Category) && filters.Categorias.Contains(x.Category.ToUpperInvariant()))
                    .Select(x => x.PedidoId)
                    .Distinct()
                    .ToList();
                pedidoIdsFiltrados = pedidoIdsFiltrados.Intersect(idsPorCategoria).ToList();
            }

            var pedidoIdSet = pedidoIdsFiltrados.ToHashSet();
            pedidosList = pedidosList.Where(p => pedidoIdSet.Contains((int)p.Id)).ToList();

            var detallesFiltrados = detalles.Where(d => pedidoIdSet.Contains(d.PedidoId)).ToList();

            // Filtrar transacciones
            var transFiltered = transRaw.ToList();
            if (filters.Meses.Count > 0)
                transFiltered = transFiltered.Where(t => filters.Meses.Contains(((DateTime)t.FechaTransaccion).ToString("yyyy-MM").ToUpperInvariant())).ToList();
            if (filters.Estados.Count > 0)
                transFiltered = transFiltered.Where(t => filters.Estados.Contains(((string)t.Estado).ToUpperInvariant())).ToList();
            if (filters.Metodos.Count > 0)
                transFiltered = transFiltered.Where(t => !string.IsNullOrEmpty((string)t.MetodoPago) && filters.Metodos.Contains(((string)t.MetodoPago).ToUpperInvariant())).ToList();

            // Tráfico (sin filtros de ventas)
            var visitasPorDia = Enumerable.Range(0, 30)
                .Select(i => { var fecha = now.AddDays(-29 + i).Date; return trafficEvents30.Count(t => t.Fecha == fecha); }).ToList();
            var usuariosUnicos30 = trafficEvents30.Where(t => !string.IsNullOrEmpty(t.SessionId)).Select(t => t.SessionId).Distinct().Count();
            var deviceGroups = trafficEvents30.Where(t => !string.IsNullOrEmpty(t.Device))
                .GroupBy(t => { var device = t.Device.Trim(); return device.Equals("Desktop", StringComparison.OrdinalIgnoreCase) ? "Windows" : device; })
                .Select(g => new { Device = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ToList();

            var trafficDevice = new { labels = deviceGroups.Select(d => d.Device).ToArray(), values = deviceGroups.Select(d => d.Count).ToArray() };
            var pageGroups = trafficEvents30.Where(t => !string.IsNullOrEmpty(t.Path)).GroupBy(t => t.Path).Select(g => new { Path = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).Take(10).ToList();
            var trafficTopPaths = new { labels = pageGroups.Select(p => p.Path).ToArray(), values = pageGroups.Select(p => p.Count).ToArray() };
            var sourceGroups = trafficEvents30.Where(t => !string.IsNullOrEmpty(t.Source)).GroupBy(t => t.Source).Select(g => new { Source = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).Take(8).ToList();
            var trafficSources = new { labels = sourceGroups.Select(s => s.Source).ToArray(), values = sourceGroups.Select(s => s.Count).ToArray() };

            // KPIs y series con listas filtradas
            decimal ventasMesDec = 0m, ventasTotDec = 0m;
            foreach (var p in pedidosList)
            {
                ventasTotDec += (decimal)p.Total;
                var fc = (DateTime)p.FechaCreacion;
                if (fc.Year == now.Year && fc.Month == now.Month) ventasMesDec += (decimal)p.Total;
            }
            var pedidosPend = pedidosList.Count(p => (string)p.Estado == "PENDIENTE");
            var pedidosComp = pedidosList.Count(p => okEstados.Contains((string)p.Estado));
            var txAprob = transFiltered.Count(t => (string)t.Estado == "APPROVED");
            var txPend = transFiltered.Count(t => (string)t.Estado == "PENDING");
            var txRech = transFiltered.Count(t => ((string)t.Estado == "REJECTED" || (string)t.Estado == "DECLINED"));
            var montoTxAprob = transFiltered.Where(t => (string)t.Estado == "APPROVED").Sum(t => (decimal)t.Monto);

            var ventasPorMetodo = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.MetodoPago))
                .GroupBy(p => (string)p.MetodoPago).Select(g => new { Metodo = g.Key, Monto = g.Sum(p => (decimal)p.Total) })
                .OrderByDescending(x => x.Monto).ToList();
            var metodosPago = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.MetodoPago))
                .GroupBy(p => (string)p.MetodoPago).Select(g => new { Metodo = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).ToList();

            var metodosPorDia = new { labels = new List<string>(), series = new List<object>() };
            if (metodosPago.Any())
            {
                var labels30 = Enumerable.Range(0, 30).Select(i => now.AddDays(-29 + i).ToString("yyyy-MM-dd")).ToList();
                var seriesPorMetodo = metodosPago.Select(m => new { metodo = m.Metodo, values = labels30.Select(fecha => { var d = DateTime.Parse(fecha).Date; return pedidosList.Count(p => ((DateTime)p.FechaCreacion).Date == d && (string)p.MetodoPago == m.Metodo); }).ToList() }).ToList();
                metodosPorDia = new { labels = labels30, series = seriesPorMetodo.Cast<object>().ToList() };
            }

            var txDiarias = Enumerable.Range(0, 30).Select(i => { var fecha = now.AddDays(-29 + i).Date; return transFiltered.Count(t => ((DateTime)t.FechaTransaccion).Date == fecha && (string)t.Estado == "APPROVED"); }).ToList();
            var ventasMensuales = months.Select(m => (double)pedidosList.Where(p => ((DateTime)p.FechaCreacion).Year == m.Year && ((DateTime)p.FechaCreacion).Month == m.Month).Sum(p => (decimal)p.Total)).ToList();
            var ingresosDiarios = Enumerable.Range(0, 30).Select(i => { var fecha = now.AddDays(-29 + i).Date; return (double)pedidosList.Where(p => ((DateTime)p.FechaCreacion).Date == fecha).Sum(p => (decimal)p.Total); }).ToList();
            var pedidosEstados = pedidosList.GroupBy(p => (string)p.Estado).Select(g => new { Estado = g.Key, Count = g.Count() }).ToList();

            // Top productos con IDs
            var topProductos = detallesFiltrados
                .GroupBy(d => d.ProductoId)
                .Join(prods, d => d.Key, p => p.Id, (d, p) => new { p.Id, p.Name, Cantidad = d.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
                .ToList();

            return new
            {
                source = "ef-core-REAL-DATA-ONLY",
                kpis = new { ventas_mes = (double)ventasMesDec, ventas_totales = (double)ventasTotDec, pedidos_pendientes = pedidosPend, pedidos_completados = pedidosComp, tx_aprobadas = txAprob, tx_pendientes = txPend, tx_rechazadas = txRech, monto_tx_aprobadas = (double)montoTxAprob },
                ventas_mensuales = new { labels = monthLabels, values = ventasMensuales },
                ingresos_diarios_30 = new { labels = Enumerable.Range(0, 30).Select(i => now.AddDays(-29 + i).ToString("yyyy-MM-dd")).ToList(), values = ingresosDiarios },
                pedidos_estados = new { labels = pedidosEstados.Select(p => p.Estado).ToList(), values = pedidosEstados.Select(p => p.Count).ToList() },
                ventas_por_metodo_monto = new { labels = ventasPorMetodo.Select(v => v.Metodo).ToList(), values = ventasPorMetodo.Select(v => (double)v.Monto).ToList() },
                metodos_pago = new { labels = metodosPago.Select(m => m.Metodo).ToList(), values = metodosPago.Select(m => m.Count).ToList() },
                metodos_pago_por_dia = metodosPorDia,
                top_productos = new { labels = topProductos.Select(t => t.Name).ToList(), values = topProductos.Select(t => t.Cantidad).ToList(), ids = topProductos.Select(t => t.Id).ToList() },
                ventas_por_categoria_mes = new { labels = monthLabels, series = prods.GroupBy(p => p.Category).Select(g => new { label = g.Key, values = months.Select(m => { var ventasCategoria = detallesFiltrados.Join(pedidosList.Where(p => ((DateTime)p.FechaCreacion).Year == m.Year && ((DateTime)p.FechaCreacion).Month == m.Month), d => d.PedidoId, p => p.Id, (d, p) => d).Join(prods.Where(pr => pr.Category == g.Key), d => d.ProductoId, pr => pr.Id, (d, pr) => d.Subtotal).Sum(s => (double)s); return ventasCategoria; }).ToList() }).ToList() },
                ticket_promedio_mensual = new { labels = monthLabels, values = months.Select(m => { var pedidosMes = pedidosList.Where(p => ((DateTime)p.FechaCreacion).Year == m.Year && ((DateTime)p.FechaCreacion).Month == m.Month).ToList(); return pedidosMes.Any() ? pedidosMes.Average(p => (double)p.Total) : 0.0; }).ToList() },
                top_clientes_monto = new { labels = pedidosList.GroupBy(p => (string)p.UserId).Join(users, p => p.Key, u => u.Id, (p, u) => new { Nombre = $"{u.PrimerNombre} {u.PrimerApellido}", Total = p.Sum(x => (double)x.Total) }).OrderByDescending(x => x.Total).Take(5).Select(x => x.Nombre).ToList(), values = pedidosList.GroupBy(p => (string)p.UserId).Join(users, p => p.Key, u => u.Id, (p, u) => new { Nombre = $"{u.PrimerNombre} {u.PrimerApellido}", Total = p.Sum(x => (double)x.Total) }).OrderByDescending(x => x.Total).Take(5).Select(x => x.Total).ToList() },
                clientes_nuevos_mes = new { labels = monthLabels, values = months.Select(m => users.Count(u => ((DateTime)u.FechaRegistro).Year == m.Year && ((DateTime)u.FechaRegistro).Month == m.Month)).ToList() },
                recompra = new { labels = new[] { "1 compra", "2+ compras" }, values = new[] { users.Count(u => pedidosList.Count(p => (string)p.UserId == u.Id) == 1), users.Count(u => pedidosList.Count(p => (string)p.UserId == u.Id) > 1) } },
                stock_bajo_por_categoria = new { labels = prods.Where(p => p.Stock <= p.StockMinimo).GroupBy(p => p.Category).Select(g => g.Key).ToList(), values = prods.Where(p => p.Stock <= p.StockMinimo).GroupBy(p => p.Category).Select(g => g.Count()).ToList() },
                tx_diarias_30 = new { labels = Enumerable.Range(0, 30).Select(i => now.AddDays(-29 + i).ToString("yyyy-MM-dd")).ToList(), values = txDiarias },
                traffic_diario_30 = new { labels = Enumerable.Range(0, 30).Select(i => now.AddDays(-29 + i).ToString("yyyy-MM-dd")).ToList(), visitas = visitasPorDia, usuarios = visitasPorDia.Select(v => Math.Max(0, (int)(v * 0.68))).ToList() },
                traffic_device = trafficDevice,
                traffic_top_paths = trafficTopPaths,
                traffic_sources = trafficSources,
                traffic_usuarios_unicos_30d = usuariosUnicos30,
                tx_estado_total = new { labels = transFiltered.GroupBy(t => (string)t.Estado).Select(g => g.Key).ToList(), values = transFiltered.GroupBy(t => (string)t.Estado).Select(g => g.Count()).ToList() },
                compras_por_ciudad = new { labels = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.CiudadEnvio)).GroupBy(p => (string)p.CiudadEnvio).OrderByDescending(g => g.Sum(p => (decimal)p.Total)).Take(10).Select(g => g.Key).ToList(), values = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.CiudadEnvio)).GroupBy(p => (string)p.CiudadEnvio).OrderByDescending(g => g.Sum(p => (decimal)p.Total)).Take(10).Select(g => (double)g.Sum(p => (decimal)p.Total)).ToList() },
                compras_por_departamento = new { labels = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.DepartamentoEnvio)).GroupBy(p => (string)p.DepartamentoEnvio).OrderByDescending(g => g.Sum(p => (decimal)p.Total)).Take(10).Select(g => g.Key).ToList(), values = pedidosList.Where(p => !string.IsNullOrEmpty((string)p.DepartamentoEnvio)).GroupBy(p => (string)p.DepartamentoEnvio).OrderByDescending(g => g.Sum(p => (decimal)p.Total)).Take(10).Select(g => (double)g.Sum(p => (decimal)p.Total)).ToList() },
                clientes_por_ciudad = new { labels = users.Where(u => !string.IsNullOrEmpty(u.Ciudad)).GroupBy(u => u.Ciudad).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Key).ToList(), values = users.Where(u => !string.IsNullOrEmpty(u.Ciudad)).GroupBy(u => u.Ciudad).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Count()).ToList() },
                pedidos_30d_por_ciudad = new { labels = pedidosList.Where(p => ((DateTime)p.FechaCreacion) >= now.AddDays(-30) && !string.IsNullOrEmpty((string)p.CiudadEnvio)).GroupBy(p => (string)p.CiudadEnvio).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Key).ToList(), values = pedidosList.Where(p => ((DateTime)p.FechaCreacion) >= now.AddDays(-30) && !string.IsNullOrEmpty((string)p.CiudadEnvio)).GroupBy(p => (string)p.CiudadEnvio).OrderByDescending(g => g.Count()).Take(10).Select(g => g.Count()).ToList() },
                debug
            };
        }
    }
}
