using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ComputerStore.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ComputerStore.Services
{
    public class LittleCarService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LittleCarService> _logger;
        private const string CarritoKey = "Carrito";
        private const string UltimaCompraKey = "UltimaCompra";
        private const string UltimoTotalKey = "UltimoTotal";
        private const string HistorialComprasKey = "HistorialCompras";
        
        // Cache estático para persistencia entre instancias
        private static readonly ConcurrentDictionary<string, List<Products>> _staticCache = new();
        private static readonly ConcurrentDictionary<string, List<CompraHistorial>> _historialCache = new();
        private static readonly object _lock = new object();

        public LittleCarService(IHttpContextAccessor httpContextAccessor, ILogger<LittleCarService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // SOLUCIÓN: Opciones de serialización consistentes
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
            // NO usar PropertyNamingPolicy para mantener PascalCase original
        };

        // Método para obtener el carrito actual de la sesión
        public List<Products> ObtenerCarrito()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) 
                {
                    _logger.LogError("❌ No se pudo obtener la sesión HTTP");
                    return new List<Products>();
                }

                var carritoJson = session.GetString(CarritoKey);
                _logger.LogInformation("🔍 Carrito JSON de sesión: {CarritoJson}", carritoJson ?? "NULL");
                
                if (string.IsNullOrEmpty(carritoJson))
                {
                    _logger.LogInformation("📦 Carrito vacío - creando nuevo");
                    return new List<Products>();
                }

                var carrito = JsonSerializer.Deserialize<List<Products>>(carritoJson, JsonOptions);
                
                if (carrito == null)
                {
                    _logger.LogError("❌ Error al deserializar carrito - resultado nulo");
                    return new List<Products>();
                }
                
                // Validar integridad del carrito
                carrito = carrito.Where(p => p != null && p.Id > 0).ToList();
                
                _logger.LogInformation("✅ Carrito deserializado correctamente con {Count} productos", carrito.Count);
                
                // Log detallado de cada producto
                foreach (var producto in carrito)
                {
                    _logger.LogInformation("📱 Producto deserializado: ID={Id}, Nombre={Name}, Precio={Price}", 
      producto.Id, producto.Name, producto.Price);
                }
                
                return carrito;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error crítico al obtener/deserializar el carrito");
                return new List<Products>();
            }
        }

        // Método para obtener el número de productos en el carrito
        public int ObtenerCantidadProductos()
        {
            try
            {
                var carrito = ObtenerCarrito();
                var cantidad = carrito.Count;
                _logger.LogDebug("Cantidad de productos en carrito: {Cantidad}", cantidad);
                return cantidad;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cantidad de productos");
                return 0;
            }
        }

        // Método mejorado para agregar un producto al carrito
        public int AgregarAlCarrito(Products producto)
        {
       try
     {
  if (producto == null)
      {
     _logger.LogWarning("⚠️ Intento de agregar producto nulo");
    throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo");
  }

  _logger.LogInformation("🛒 Agregando producto: ID={Id}, Nombre={Name}, Precio={Price}", 
          producto.Id, producto.Name, producto.Price);

     var carrito = ObtenerCarrito();
    _logger.LogInformation("📦 Carrito actual tiene {Count} productos", carrito.Count);
      
    // Verificar si el producto ya está en el carrito
      var productoExistente = carrito.FirstOrDefault(p => p.Id == producto.Id);
  if (productoExistente != null)
    {
      _logger.LogInformation("⚠️ Producto {Id} ya existe en el carrito", producto.Id);
return carrito.Count;
  }

  // Crear una copia del producto para evitar referencias
          var productoCarrito = new Products
     {
  Id = producto.Id,
   Name = producto.Name,
  Category = producto.Category,
       Price = producto.Price,
 ImageUrl = producto.ImageUrl,
   Description = producto.Description,
  Brand = producto.Brand,
  Component = producto.Component,
     Color = producto.Color,
    Availability = producto.Availability,
 Stock = producto.Stock,
     OriginalPrice = producto.OriginalPrice,
    Details = producto.Details != null ? new Dictionary<string, string>(producto.Details) : new Dictionary<string, string>()
      };

           carrito.Add(productoCarrito);
      
    // Intentar guardar el carrito
    try
     {
      GuardarCarrito(carrito);
    _logger.LogInformation("✅ Producto {Id} agregado exitosamente. Total: {Count}", producto.Id, carrito.Count);
   }
   catch (Exception exGuardar)
          {
     _logger.LogError(exGuardar, "❌ Error al guardar carrito después de agregar producto");
    // Intentamos una vez más con un pequeño delay
      Thread.Sleep(100); // Delay síncrono
         try
           {
     GuardarCarrito(carrito);
       _logger.LogInformation("✅ Carrito guardado en segundo intento");
   }
     catch
     {
       // Si falla de nuevo, removemos el producto que acabamos de agregar
        carrito.Remove(productoCarrito);
     throw new InvalidOperationException("No se pudo guardar el carrito. Por favor intenta de nuevo.");
   }
    }
      
    // Verificación final
    var verificacion = ObtenerCarrito();
_logger.LogInformation("🔍 Verificación post-guardado: {Count} productos en carrito", verificacion.Count);
      
  return verificacion.Count;
     }
   catch (Exception ex)
   {
    _logger.LogError(ex, "💥 Error al agregar producto {Id}", producto?.Id);
   throw;
  }
 }

        // Método privado mejorado para guardar el carrito en la sesión
        private void GuardarCarrito(List<Products> carrito)
  {
     try
      {
    Console.WriteLine($"💾 [GuardarCarrito] INICIO");
   
    var httpContext = _httpContextAccessor.HttpContext;
   if (httpContext == null)
          {
       Console.WriteLine($"❌ [GuardarCarrito] HttpContext es NULO");
        _logger.LogError("❌ HttpContext es nulo");
    throw new InvalidOperationException("No se pudo obtener el contexto HTTP");
       }
    
    Console.WriteLine($"✅ [GuardarCarrito] HttpContext existe");

        var session = httpContext.Session;
    if (session == null)
                {
          Console.WriteLine($"❌ [GuardarCarrito] Session es NULO");
        _logger.LogError("❌ Session es nulo");
            throw new InvalidOperationException("No se pudo obtener la sesión HTTP");
    }
    
    Console.WriteLine($"✅ [GuardarCarrito] Session existe - ID: {session.Id}");

     // Forzar que la sesión se cargue si aún no lo está
     try
      {
    _ = session.Id; // Esto fuerza la inicialización de la sesión
       Console.WriteLine($"✅ [GuardarCarrito] Sesión inicializada");
   }
     catch (Exception ex)
        {
            Console.WriteLine($"❌ [GuardarCarrito] Error al inicializar sesión: {ex.Message}");
    _logger.LogError(ex, "❌ Error al inicializar sesión");
      }

// Validar carrito antes de guardar
        var carritoLimpio = carrito.Where(p => p != null && p.Id > 0).ToList();
      Console.WriteLine($"✅ [GuardarCarrito] Carrito limpio: {carritoLimpio.Count} productos");
    
  var carritoJson = JsonSerializer.Serialize(carritoLimpio, JsonOptions);
    Console.WriteLine($"✅ [GuardarCarrito] JSON serializado - Length: {carritoJson.Length}");

  _logger.LogInformation("💾 Guardando carrito: {Count} productos", carritoLimpio.Count);
         _logger.LogInformation("📝 JSON a guardar (primeros 200 chars): {Json}", 
      carritoJson.Length > 200 ? carritoJson.Substring(0, 200) + "..." : carritoJson);

       // Guardar en sesión
   Console.WriteLine($"💾 [GuardarCarrito] Llamando a session.SetString...");
      session.SetString(CarritoKey, carritoJson);
   Console.WriteLine($"✅ [GuardarCarrito] session.SetString completado");
    
      _logger.LogInformation("✅ Carrito guardado en sesión");
       
   // Verificación más tolerante
          try
    {
   Console.WriteLine($"🔍 [GuardarCarrito] Verificando con session.GetString...");
    var verificacion = session.GetString(CarritoKey);
      Console.WriteLine($"🔍 [GuardarCarrito] Verificación retornó: {(verificacion != null ? verificacion.Length + " chars" : "NULL")}");
  
      if (string.IsNullOrEmpty(verificacion))
  {
        Console.WriteLine($"⚠️ [GuardarCarrito] Verificación vacía");
      _logger.LogWarning("⚠️ Verificación retornó vacío, pero el guardado puede haber sido exitoso");
   }
         else
        {
    Console.WriteLine($"✅ [GuardarCarrito] Verificación exitosa");
    _logger.LogInformation("🔍 Verificación exitosa - {Length} caracteres guardados", verificacion.Length);
    
    // Prueba de deserialización
     var prueba = JsonSerializer.Deserialize<List<Products>>(verificacion, JsonOptions);
              Console.WriteLine($"🧪 [GuardarCarrito] Deserialización: {prueba?.Count ?? 0} productos");
        _logger.LogInformation("🧪 Prueba deserialización: {Count} productos", prueba?.Count ?? 0);
    }
  }
     catch (Exception ex)
     {
     Console.WriteLine($"⚠️ [GuardarCarrito] Error en verificación: {ex.Message}");
       _logger.LogWarning(ex, "⚠️ Error en verificación post-guardado, pero puede ser temporal");
    }
    
    Console.WriteLine($"✅ [GuardarCarrito] FIN");
       }
   catch (Exception ex)
   {
    Console.WriteLine($"💥 [GuardarCarrito] ERROR CRÍTICO: {ex.Message}");
    Console.WriteLine($"💥 [GuardarCarrito] Stack Trace: {ex.StackTrace}");
    _logger.LogError(ex, "💥 Error crítico al guardar el carrito");
  throw;
          }
}

        // Método mejorado para eliminar un producto del carrito
        public int EliminarDelCarrito(int id)
        {
            try
            {
                var carrito = ObtenerCarrito();
                var productoAEliminar = carrito.FirstOrDefault(p => p.Id == id);
                
                if (productoAEliminar != null)
                {
                    carrito.Remove(productoAEliminar);
                    GuardarCarrito(carrito);
                    
                    _logger.LogInformation("Producto {ProductoId} eliminado del carrito. Productos restantes: {Count}", 
                        id, carrito.Count);
                }
                else
                {
                    _logger.LogWarning("Intento de eliminar producto inexistente del carrito: {ProductoId}", id);
                }

                return carrito.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {ProductoId} del carrito", id);
                throw;
            }
        }

        // Método para vaciar el carrito
        public void VaciarCarrito()
  {
       try
       {
      var session = _httpContextAccessor.HttpContext?.Session;
      if (session != null)
              {
      var cantidadAnterior = ObtenerCantidadProductos();
    session.Remove(CarritoKey);
       
         // Limpiar cache estático
       var sessionId = session.Id;
if (!string.IsNullOrEmpty(sessionId))
      {
    lock (_lock)
                {
     _staticCache.TryRemove(sessionId, out _);
    }
           }

      _logger.LogInformation("Carrito vaciado. Productos eliminados: {Count}", cantidadAnterior);
      }
            }
      catch (Exception ex)
            {
      _logger.LogError(ex, "Error al vaciar el carrito");
        throw;
      }
        }

        // Método mejorado para obtener el total del carrito
        public decimal ObtenerTotal()
        {
            try
            {
                var carrito = ObtenerCarrito();
                // Price ya es el precio final (con descuento si aplica)
                var total = carrito.Sum(p => p.Price);
                
                _logger.LogDebug("Total del carrito calculado: {Total:C}", total);
                return total;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular el total del carrito");
                return 0;
            }
        }

        // Método mejorado para guardar la última compra
        public void GuardarUltimaCompra(List<Products> productos)
        {
            try
            {
                if (productos == null || !productos.Any())
                {
                    _logger.LogWarning("Intento de guardar compra vacía o nula");
                    return;
                }

                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                var productosJson = JsonSerializer.Serialize(productos, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var total = productos.Sum(p => p.Price);

                session.SetString(UltimaCompraKey, productosJson);
                session.SetString(UltimoTotalKey, total.ToString());

                // Agregar al historial
                AgregarAlHistorialCompras(productos, total);

                _logger.LogInformation("Última compra guardada con {Count} productos, total: {Total:C}", 
                    productos.Count, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la última compra");
            }
        }

        // Método para obtener la última compra
        public List<Products> ObtenerUltimaCompra()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return new List<Products>();

                var productosJson = session.GetString(UltimaCompraKey);
                if (string.IsNullOrEmpty(productosJson))
                {
                    return new List<Products>();
                }

                var productos = JsonSerializer.Deserialize<List<Products>>(productosJson) ?? new List<Products>();
                _logger.LogDebug("Última compra obtenida con {Count} productos", productos.Count);
                
                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la última compra");
                return new List<Products>();
            }
        }

        // Método para obtener el último total
        public decimal ObtenerUltimoTotal()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return 0;

                var totalString = session.GetString(UltimoTotalKey);
                if (decimal.TryParse(totalString, out decimal total))
                {
                    return total;
                }

                // Fallback: calcular desde la última compra
                var ultimaCompra = ObtenerUltimaCompra();
                return ultimaCompra.Sum(p => p.Price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el último total");
                return 0;
            }
        }

        // Nuevo método para agregar al historial de compras
        private void AgregarAlHistorialCompras(List<Products> productos, decimal total)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                var compra = new CompraHistorial
                {
                    Id = Guid.NewGuid().ToString(),
                    Fecha = DateTime.UtcNow,
                    Productos = new List<Products>(productos),
                    Total = total,
                    Estado = "Completada"
                };

                var historialJson = session.GetString(HistorialComprasKey);
                var historial = string.IsNullOrEmpty(historialJson) 
                    ? new List<CompraHistorial>() 
                    : JsonSerializer.Deserialize<List<CompraHistorial>>(historialJson) ?? new List<CompraHistorial>();

                historial.Add(compra);

                // Mantener solo las últimas 20 compras
                if (historial.Count > 20)
                {
                    historial = historial.OrderByDescending(c => c.Fecha).Take(20).ToList();
                }

                var nuevoHistorialJson = JsonSerializer.Serialize(historial);
                session.SetString(HistorialComprasKey, nuevoHistorialJson);

                _logger.LogInformation("Compra agregada al historial: {CompraId}", compra.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar compra al historial");
            }
        }

        // Nuevo método para obtener historial de compras
        public List<CompraHistorial> ObtenerHistorialCompras()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return new List<CompraHistorial>();

                var historialJson = session.GetString(HistorialComprasKey);
                if (string.IsNullOrEmpty(historialJson))
                {
                    return new List<CompraHistorial>();
                }

                var historial = JsonSerializer.Deserialize<List<CompraHistorial>>(historialJson) ?? new List<CompraHistorial>();
                return historial.OrderByDescending(c => c.Fecha).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de compras");
                return new List<CompraHistorial>();
            }
        }

        // Método para validar la integridad del carrito
        public bool ValidarCarrito()
        {
            try
            {
                var carrito = ObtenerCarrito();
                
                // Verificar que todos los productos tengan datos válidos
                var carritoValido = carrito.All(p => 
                    p != null && 
                    p.Id > 0 && 
                    !string.IsNullOrEmpty(p.Name) && 
                    p.Price > 0);

                if (!carritoValido)
                {
                    _logger.LogWarning("Carrito contiene productos inválidos");
                    
                    // Limpiar productos inválidos
                    var carritoLimpio = carrito.Where(p => 
                        p != null && 
                        p.Id > 0 && 
                        !string.IsNullOrEmpty(p.Name) && 
                        p.Price > 0).ToList();
                    
                    GuardarCarrito(carritoLimpio);
                }

                return carritoValido;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar el carrito");
                return false;
            }
        }

        // Método para obtener estadísticas del carrito
        public object ObtenerEstadisticasCarrito()
        {
            try
            {
                var carrito = ObtenerCarrito();
                
                return new
                {
                    CantidadProductos = carrito.Count,
                    Total = carrito.Sum(p => p.Price),
                    PromedioPrecios = carrito.Any() ? carrito.Average(p => p.Price) : 0,
                    ProductoMasCaro = carrito.OrderByDescending(p => p.Price).FirstOrDefault(),
                    ProductoMasBarato = carrito.OrderBy(p => p.Price).FirstOrDefault(),
                    CategoriasPorConteo = carrito.GroupBy(p => p.Category)
                        .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
                        .OrderByDescending(x => x.Cantidad)
                        .ToList(),
                    MarcasPorConteo = carrito.GroupBy(p => p.Brand)
                        .Select(g => new { Marca = g.Key, Cantidad = g.Count() })
                        .OrderByDescending(x => x.Cantidad)
                        .ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del carrito");
                return new { Error = "No se pudieron obtener las estadísticas" };
            }
        }

        // Método para limpiar cache antiguo
        public static void LimpiarCacheAntiguo()
        {
            lock (_lock)
            {
                var keysToRemove = new List<string>();
                
                foreach (var kvp in _staticCache)
                {
                    // Lógica para determinar si una entrada es antigua
                    // Por simplicidad, mantenemos solo las últimas 100 entradas
                    if (_staticCache.Count > 100)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove.Take(_staticCache.Count - 100))
                {
                    _staticCache.TryRemove(key, out _);
                }
            }
        }
    }

    // Clase para el historial de compras
    public class CompraHistorial
    {
        public string Id { get; set; } = string.Empty;
public DateTime Fecha { get; set; }
        public List<Products> Productos { get; set; } = new();
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
     public string? NumeroOrden { get; set; }
  public string? MetodoPago { get; set; }
    }
}