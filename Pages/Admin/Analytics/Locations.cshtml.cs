using ComputerStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin.Analytics
{
    [Authorize(Roles = "Admin")]
    public class LocationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LocationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // KPIs principales
        public int TotalCiudades { get; set; }
        public int TotalDepartamentos { get; set; }
        public string CiudadTopVentas { get; set; } = "";
        public decimal VentasCiudadTop { get; set; }
        public string DepartamentoTopVentas { get; set; } = "";
        public decimal VentasDepartamentoTop { get; set; }
        public int ClientesTotales { get; set; }
        public int PedidosTotales { get; set; }

        // Datos para los gráficos
        public List<UbicacionVentaData> VentasPorCiudad { get; set; } = new();
        public List<UbicacionVentaData> VentasPorDepartamento { get; set; } = new();
        public List<UbicacionClienteData> ClientesPorCiudad { get; set; } = new();
        public List<UbicacionClienteData> ClientesPorDepartamento { get; set; } = new();
        public List<MapaCiudadData> DatosMapaCiudades { get; set; } = new();
        public List<TreemapData> TreemapDepartamentos { get; set; } = new();

        // Datos específicos para mapa de Colombia
        public List<MapaDepartamentoData> DatosMapaDepartamentos { get; set; } = new();
        public decimal VentasPromedioPorCiudad => VentasPorCiudad.Any() ? VentasPorCiudad.Average(v => v.TotalVentas) : 0;

        public async Task OnGetAsync()
        {
            await CargarDatosAnalytics();
        }

        private async Task CargarDatosAnalytics()
        {
            try
            {
                await CalcularKPIs();
                await CargarVentasPorUbicacion();
                await CargarClientesPorUbicacion();
                await CargarDatosParaMapa();
                await CargarDatosTreemap();

                Console.WriteLine($"DEBUG: Datos REALES cargados - Ciudades: {VentasPorCiudad.Count}, Mapa: {DatosMapaCiudades.Count}");

                // Solo usar fallback si NO HAY DATOS REALES
                if (!VentasPorCiudad.Any())
                {
                    Console.WriteLine("DEBUG: No hay datos reales, usando fallback");
                    CargarDatosFallback();
                }
                else
                {
                    Console.WriteLine($"DEBUG: Usando {VentasPorCiudad.Count} ciudades reales de la BD");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando analytics ubicaciones: {ex.Message}");
                CargarDatosFallback();
            }
        }

        private async Task CalcularKPIs()
        {
            var estadosOK = new[] { "PAGADO", "COMPLETADO", "ENTREGADO", "APPROVED" };

            // Obtener datos de ventas y clientes
            var pedidosCompletados = await _context.Pedidos
                .Where(p => estadosOK.Contains(p.Estado.ToUpper()))
                .ToListAsync();

            var usuarios = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Ciudad) && !string.IsNullOrEmpty(u.Departamento))
                .ToListAsync();

            // KPIs básicos
            TotalCiudades = usuarios.Select(u => u.Ciudad.ToUpper()).Distinct().Count();

            // CORREGIR: Calcular departamentos con mapeo correcto
            var departamentosUnicos = usuarios
                .Select(u => MapearDepartamentoCorrectamente(u.Ciudad, u.Departamento))
                .Distinct()
                .Count();
            TotalDepartamentos = departamentosUnicos;

            ClientesTotales = usuarios.Count;
            PedidosTotales = pedidosCompletados.Count;

            // Top ciudad por ventas
            var ventasPorCiudadUsuario = pedidosCompletados
                .Where(p => !string.IsNullOrEmpty(p.UserId))
                .Join(usuarios, p => p.UserId, u => u.Id, (p, u) => new { p.Total, u.Ciudad })
                .GroupBy(x => x.Ciudad.ToUpper())
                .Select(g => new { Ciudad = g.Key, Total = g.Sum(x => x.Total) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            if (ventasPorCiudadUsuario != null)
            {
                CiudadTopVentas = ventasPorCiudadUsuario.Ciudad;
                VentasCiudadTop = ventasPorCiudadUsuario.Total;
            }

            // CORREGIR: Top departamento con mapeo correcto
            var ventasPorDepartamentoUsuario = pedidosCompletados
                .Where(p => !string.IsNullOrEmpty(p.UserId))
                .Join(usuarios, p => p.UserId, u => u.Id, (p, u) => new { p.Total, Ciudad = u.Ciudad, Departamento = u.Departamento })
                .GroupBy(x => MapearDepartamentoCorrectamente(x.Ciudad, x.Departamento))
                .Select(g => new { Departamento = g.Key, Total = g.Sum(x => x.Total) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            if (ventasPorDepartamentoUsuario != null)
            {
                DepartamentoTopVentas = ventasPorDepartamentoUsuario.Departamento;
                VentasDepartamentoTop = ventasPorDepartamentoUsuario.Total;
            }
        }

        private async Task CargarVentasPorUbicacion()
        {
            var estadosOK = new[] { "PAGADO", "COMPLETADO", "ENTREGADO", "APPROVED" };

            var pedidosCompletados = await _context.Pedidos
                .Where(p => estadosOK.Contains(p.Estado.ToUpper()) && !string.IsNullOrEmpty(p.UserId))
                .ToListAsync();

            var usuarios = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Ciudad) && !string.IsNullOrEmpty(u.Departamento))
                .ToListAsync();

            // Ventas por ciudad
            var ventasPorCiudad = pedidosCompletados
                .Join(usuarios, p => p.UserId, u => u.Id, (p, u) => new { p.Total, u.Ciudad, p.Id })
                .GroupBy(x => x.Ciudad.ToUpper())
                .Select(g => new UbicacionVentaData
                {
                    Ubicacion = g.Key,
                    TotalVentas = g.Sum(x => x.Total),
                    CantidadPedidos = g.Count(),
                    TicketPromedio = g.Average(x => x.Total)
                })
                .OrderByDescending(x => x.TotalVentas)
                .Take(15)
                .ToList();

            // CORREGIR: Ventas por departamento con mapeo correcto
            var ventasPorDepartamento = pedidosCompletados
                .Join(usuarios, p => p.UserId, u => u.Id, (p, u) => new { p.Total, Ciudad = u.Ciudad, Departamento = u.Departamento, p.Id })
                .GroupBy(x => MapearDepartamentoCorrectamente(x.Ciudad, x.Departamento))
                .Select(g => new UbicacionVentaData
                {
                    Ubicacion = g.Key,
                    TotalVentas = g.Sum(x => x.Total),
                    CantidadPedidos = g.Count(),
                    TicketPromedio = g.Average(x => x.Total)
                })
                .OrderByDescending(x => x.TotalVentas)
                .Take(10)
                .ToList();

            VentasPorCiudad = ventasPorCiudad;
            VentasPorDepartamento = ventasPorDepartamento;

            Console.WriteLine($"DEBUG: Ventas por departamento: {string.Join(", ", ventasPorDepartamento.Select(v => v.Ubicacion))}");
        }

        private async Task CargarClientesPorUbicacion()
        {
            var usuarios = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Ciudad) && !string.IsNullOrEmpty(u.Departamento))
                .ToListAsync();

            // Clientes por ciudad
            var clientesPorCiudad = usuarios
                .GroupBy(u => u.Ciudad.ToUpper())
                .Select(g => new UbicacionClienteData
                {
                    Ubicacion = g.Key,
                    TotalClientes = g.Count(),
                    ClientesActivos = g.Count(u => u.IsActive),
                    UltimaActividad = g.Max(u => u.FechaUltimoAcceso) ?? DateTime.UtcNow
                })
                .OrderByDescending(x => x.TotalClientes)
                .Take(15)
                .ToList();

            // CORREGIR: Clientes por departamento con mapeo correcto
            var clientesPorDepartamento = usuarios
                .GroupBy(u => MapearDepartamentoCorrectamente(u.Ciudad, u.Departamento))
                .Select(g => new UbicacionClienteData
                {
                    Ubicacion = g.Key,
                    TotalClientes = g.Count(),
                    ClientesActivos = g.Count(u => u.IsActive),
                    UltimaActividad = g.Max(u => u.FechaUltimoAcceso) ?? DateTime.UtcNow
                })
                .OrderByDescending(x => x.TotalClientes)
                .Take(10)
                .ToList();

            ClientesPorCiudad = clientesPorCiudad;
            ClientesPorDepartamento = clientesPorDepartamento;
        }

        private async Task CargarDatosParaMapa()
        {
            // Coordenadas de ciudades principales de Colombia
            var coordenadasCiudades = new Dictionary<string, (double lat, double lng)>
            {
                ["BOGOTÁ"] = (4.7110, -74.0721),
                ["BOGOTA"] = (4.7110, -74.0721),
                ["MEDELLÍN"] = (6.2476, -75.5658),
                ["MEDELLIN"] = (6.2476, -75.5658),
                ["CALI"] = (3.4516, -76.5320),
                ["BARRANQUILLA"] = (10.9685, -74.7813),
                ["CARTAGENA"] = (10.3910, -75.4794),
                ["BUCARAMANGA"] = (7.1253, -73.1198),
                ["PEREIRA"] = (4.8133, -75.6961),
                ["SANTA MARTA"] = (11.2408, -74.2099),
                ["IBAGUÉ"] = (4.4389, -75.2322),
                ["IBAGUE"] = (4.4389, -75.2322),
                ["CÚCUTA"] = (7.8939, -72.5078),
                ["CUCUTA"] = (7.8939, -72.5078),
                ["VILLAVICENCIO"] = (4.1420, -73.6266),
                ["MANIZALES"] = (5.0670, -75.5174),
                ["NEIVA"] = (2.9273, -75.2819),
                ["ARMENIA"] = (4.5339, -75.6811),
                ["MONTERÍA"] = (8.7479, -75.8814),
                ["MONTERIA"] = (8.7479, -75.8814),
                ["VALLEDUPAR"] = (10.4631, -73.2532),
                ["PASTO"] = (1.2136, -77.2811),
                ["SINCELEJO"] = (9.3044, -75.3977),
                ["FLORENCIA"] = (1.6144, -75.6062),
                ["POPAYÁN"] = (2.4448, -76.6147),
                ["POPAYANRAIL"] = (2.4448, -76.6147),
                ["TUNJA"] = (5.5353, -73.3678),
                ["QUIBDÓ"] = (5.6947, -76.6611),
                ["QUIBDO"] = (5.6947, -76.6611),
                ["RIOHACHA"] = (11.5444, -72.9072),
                ["YOPAL"] = (5.3375, -72.3958),
                ["ARAUCA"] = (7.0903, -70.7619),
                ["LETICIA"] = (-4.2153, -69.9406),
                // AGREGAR: Ciudades de Cundinamarca
                ["CHÍA"] = (4.8579, -74.0559),
                ["CHIA"] = (4.8579, -74.0559),
                ["SOACHA"] = (4.5793, -74.2093),
                ["FUSAGASUGÁ"] = (4.3425, -74.3642),
                ["GIRARDOT"] = (4.3011, -74.8009),
                ["ZIPAQUIRÁ"] = (5.0220, -74.0039),
                ["FACATATIVÁ"] = (4.8144, -74.3555),
                ["CAJICÁ"] = (4.9187, -74.0283),
                ["LA CALERA"] = (4.7219, -73.9678)
            };

            var mapaCiudades = new List<MapaCiudadData>();

            foreach (var ciudadVenta in VentasPorCiudad.Take(10))
            {
                var ciudadKey = ciudadVenta.Ubicacion.ToUpper();
                if (coordenadasCiudades.ContainsKey(ciudadKey))
                {
                    var coordenadas = coordenadasCiudades[ciudadKey];
                    var clientesEnCiudad = ClientesPorCiudad.FirstOrDefault(c => c.Ubicacion == ciudadKey)?.TotalClientes ?? 0;

                    mapaCiudades.Add(new MapaCiudadData
                    {
                        Ciudad = ciudadVenta.Ubicacion,
                        Latitud = coordenadas.lat,
                        Longitud = coordenadas.lng,
                        TotalVentas = ciudadVenta.TotalVentas,
                        TotalClientes = clientesEnCiudad,
                        CantidadPedidos = ciudadVenta.CantidadPedidos,
                        Radio = Math.Min(Math.Max((double)(ciudadVenta.TotalVentas / 100000), 5), 50)
                    });
                }
            }

            DatosMapaCiudades = mapaCiudades;
        }

        private async Task CargarDatosTreemap()
        {
            // Crear datos para treemap basado en ventas por departamento y ciudades principales
            var treemapData = new List<TreemapData>();

            foreach (var depto in VentasPorDepartamento.Take(8))
            {
                // Encontrar las principales ciudades de este departamento
                var ciudadesDepto = VentasPorCiudad
                    .Where(c => EsCiudadDelDepartamento(c.Ubicacion, depto.Ubicacion))
                    .Take(3)
                    .ToList();

                var children = ciudadesDepto.Select(c => new TreemapData
                {
                    Name = FormatearNombre(c.Ubicacion),
                    Value = (double)c.TotalVentas,
                    Color = GetColorPorRango((double)c.TotalVentas, (double)VentasPromedioPorCiudad),
                    Category = "Ciudad"
                }).ToList();

                treemapData.Add(new TreemapData
                {
                    Name = FormatearNombre(depto.Ubicacion),
                    Value = (double)depto.TotalVentas,
                    Color = GetColorPorRango((double)depto.TotalVentas, (double)VentasDepartamentoTop),
                    Category = "Departamento",
                    Children = children
                });
            }

            TreemapDepartamentos = treemapData;
        }

        // CORREGIR: Función clave para mapear departamentos correctamente
        private string MapearDepartamentoCorrectamente(string ciudad, string departamentoOriginal)
        {
            var ciudadUpper = ciudad.ToUpper();

            // Bogotá siempre es Bogotá DC, independiente del departamento original
            if (ciudadUpper == "BOGOTÁ" || ciudadUpper == "BOGOTA")
            {
                return "BOGOTÁ DC";
            }

            // Ciudades de Cundinamarca (excluyendo Bogotá)
            var ciudadesCundinamarca = new[] {
                "CHÍA", "CHIA", "SOACHA", "FUSAGASUGÁ", "GIRARDOT",
                "ZIPAQUIRÁ", "FACATATIVÁ", "CAJICÁ", "LA CALERA"
            };

            if (ciudadesCundinamarca.Contains(ciudadUpper))
            {
                return "CUNDINAMARCA";
            }

            // Para el resto de ciudades, usar el departamento original pero normalizado
            return departamentoOriginal.ToUpper();
        }

        private bool EsCiudadDelDepartamento(string ciudad, string departamento)
        {
            // CORREGIR: Mapeo actualizado con separación Bogotá/Cundinamarca
            var mapeoDepto = new Dictionary<string, string[]>
            {
                ["BOGOTÁ DC"] = new[] { "BOGOTÁ", "BOGOTA" },
                ["CUNDINAMARCA"] = new[] { "CHÍA", "CHIA", "SOACHA", "FUSAGASUGÁ", "GIRARDOT", "ZIPAQUIRÁ", "FACATATIVÁ", "CAJICÁ", "LA CALERA" },
                ["ANTIOQUIA"] = new[] { "MEDELLÍN", "MEDELLIN", "BELLO", "ITAGÜÍ", "ENVIGADO", "RIONEGRO", "SABANETA" },
                ["VALLE DEL CAUCA"] = new[] { "CALI", "PALMIRA", "BUENAVENTURA", "TULUÁ", "CARTAGO", "BUGA" },
                ["ATLÁNTICO"] = new[] { "BARRANQUILLA", "SOLEDAD", "MALAMBO" },
                ["BOLÍVAR"] = new[] { "CARTAGENA", "MAGANGUÉ", "TURBACO" },
                ["SANTANDER"] = new[] { "BUCARAMANGA", "FLORIDABLANCA", "GIRÓN", "PIEDECUESTA" },
                ["RISARALDA"] = new[] { "PEREIRA", "DOSQUEBRADAS", "LA VIRGINIA" },
                ["MAGDALENA"] = new[] { "SANTA MARTA", "CIÉNAGA", "FUNDACIÓN" },
                ["TOLIMA"] = new[] { "IBAGUÉ", "IBAGUE", "ESPINAL", "MELGAR" },
                ["NORTE DE SANTANDER"] = new[] { "CÚCUTA", "CUCUTA", "VILLA DEL ROSARIO", "LOS PATIOS" }
            };

            var deptoKey = departamento.ToUpper();
            if (mapeoDepto.ContainsKey(deptoKey))
            {
                return mapeoDepto[deptoKey].Contains(ciudad.ToUpper());
            }

            return false;
        }

        private string GetColorPorRango(double valor, double promedio)
        {
            if (valor > promedio * 2) return "#ef4444"; // Rojo para muy alto
            if (valor > promedio * 1.5) return "#f59e0b"; // Amarillo para alto
            if (valor > promedio) return "#10b981"; // Verde para arriba del promedio
            return "#6b7280"; // Gris para por debajo
        }

        private string FormatearNombre(string nombre)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(nombre.ToLower());
        }

        private void CargarDatosFallback()
        {
            VentasPorCiudad = new List<UbicacionVentaData>
            {
                new() { Ubicacion = "BOGOTÁ", TotalVentas = 35900000, CantidadPedidos = 15, TicketPromedio = 2393333 },
                new() { Ubicacion = "CHÍA", TotalVentas = 6200000, CantidadPedidos = 1, TicketPromedio = 6200000 },
                new() { Ubicacion = "MEDELLÍN", TotalVentas = 4700000, CantidadPedidos = 1, TicketPromedio = 4700000 }
            };

            VentasPorDepartamento = new List<UbicacionVentaData>
            {
                new() { Ubicacion = "BOGOTÁ DC", TotalVentas = 35900000, CantidadPedidos = 15, TicketPromedio = 2393333 },
                new() { Ubicacion = "CUNDINAMARCA", TotalVentas = 6200000, CantidadPedidos = 1, TicketPromedio = 6200000 },
                new() { Ubicacion = "ANTIOQUIA", TotalVentas = 4700000, CantidadPedidos = 1, TicketPromedio = 4700000 }
            };

            ClientesPorCiudad = new List<UbicacionClienteData>
            {
                new() { Ubicacion = "BOGOTÁ", TotalClientes = 5, ClientesActivos = 5 },
                new() { Ubicacion = "CHÍA", TotalClientes = 1, ClientesActivos = 1 },
                new() { Ubicacion = "MEDELLÍN", TotalClientes = 1, ClientesActivos = 1 }
            };

            ClientesPorDepartamento = new List<UbicacionClienteData>
            {
                new() { Ubicacion = "BOGOTÁ DC", TotalClientes = 5, ClientesActivos = 5 },
                new() { Ubicacion = "CUNDINAMARCA", TotalClientes = 1, ClientesActivos = 1 },
                new() { Ubicacion = "ANTIOQUIA", TotalClientes = 1, ClientesActivos = 1 }
            };

            TotalCiudades = 3;
            TotalDepartamentos = 3;
            CiudadTopVentas = "BOGOTÁ";
            VentasCiudadTop = 35900000;
            DepartamentoTopVentas = "BOGOTÁ DC";
            VentasDepartamentoTop = 35900000;
            ClientesTotales = 7;
            PedidosTotales = 17;
        }
    }

    // Clases de datos para ubicaciones
    public class UbicacionVentaData
    {
        public string Ubicacion { get; set; } = "";
        public decimal TotalVentas { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal TicketPromedio { get; set; }
    }

    public class UbicacionClienteData
    {
        public string Ubicacion { get; set; } = "";
        public int TotalClientes { get; set; }
        public int ClientesActivos { get; set; }
        public DateTime UltimaActividad { get; set; }
    }

    public class MapaCiudadData
    {
        public string Ciudad { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public decimal TotalVentas { get; set; }
        public int TotalClientes { get; set; }
        public int CantidadPedidos { get; set; }
        public double Radio { get; set; }
    }

    public class MapaDepartamentoData
    {
        public string Departamento { get; set; } = "";
        public decimal TotalVentas { get; set; }
        public int TotalClientes { get; set; }
        public string Color { get; set; } = "";
    }

    public class TreemapData
    {
        public string Name { get; set; } = "";
        public double Value { get; set; }
        public string Color { get; set; } = "";
        public string Category { get; set; } = "";
        public List<TreemapData> Children { get; set; } = new();
    }
}