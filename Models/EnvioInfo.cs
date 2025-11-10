using ComputerStore.Models;

namespace ComputerStore.Models
{
    public class EnvioInfo
    {
        public long OrderId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string NumeroGuia { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // ? CRÍTICO: ID del usuario propietario del envío
        public string NombreProducto { get; set; } = string.Empty; // Nombre del producto principal
        public decimal PrecioTotal { get; set; } // Precio total del envío
        public string Estado { get; set; } = "PREPARANDO";
        public string Transportadora { get; set; } = "Servientrega";
        public string DireccionEnvio { get; set; } = string.Empty; // Dirección como string simple
        public DireccionEnvio? DireccionOrigen { get; set; } // Dirección estructurada de origen
        public DireccionEnvio? DireccionDestino { get; set; } // Dirección estructurada de destino
        public decimal PesoKg { get; set; } = 2.5m; // Peso del paquete
        
        // ? MEJORA: Fechas del seguimiento con zona horaria correcta (Colombia GMT-5)
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaRecoleccion { get; set; }
        public DateTime? FechaEnvio { get; set; }
        public DateTime? FechaLlegadaCentroDistribucion { get; set; }
        public DateTime? FechaSalidaCentroDistribucion { get; set; }
        public DateTime? FechaLlegadaCiudadDestino { get; set; }
        public DateTime? FechaEnRutaEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime FechaEstimadaEntrega { get; set; } = DateTime.UtcNow.AddDays(3);
        
        public string Observaciones { get; set; } = string.Empty;
        public List<ProductoEnvio> Productos { get; set; } = new List<ProductoEnvio>();

        // ? NUEVO: Propiedades calculadas para mejor experiencia de usuario
        public string EstadoDisplay => Estado switch
        {
            "PREPARANDO" => "?? Preparando",
            "RECOLECTADO" => "?? Recolectado",
            "EN_TRANSITO_ORIGEN" => "?? En tránsito desde origen",
            "EN_CENTRO_DISTRIBUCION" => "?? En centro de distribución",
            "EN_TRANSITO_DESTINO" => "?? En tránsito a destino",
            "EN_RUTA_ENTREGA" => "?? En ruta de entrega",
            "ENTREGADO" => "? Entregado",
            _ => Estado
        };

        public string FechaCreacionFormateada => FechaCreacion.ToString("dd/MM/yyyy HH:mm");
        public string FechaEstimadaEntregaFormateada => FechaEstimadaEntrega.ToString("dd/MM/yyyy");
        
        // ? SEGURIDAD: Verificar si el envío pertenece al usuario especificado
        public bool PerteneceAUsuario(string userId) => !string.IsNullOrEmpty(UserId) && UserId == userId;
        
        // ? UTILIDAD: Obtener color del estado para UI
        public string ColorEstado => Estado switch
        {
            "PREPARANDO" => "warning",
            "RECOLECTADO" => "info",
            "EN_TRANSITO_ORIGEN" => "primary",
            "EN_CENTRO_DISTRIBUCION" => "primary",
            "EN_TRANSITO_DESTINO" => "primary",
            "EN_RUTA_ENTREGA" => "success",
            "ENTREGADO" => "success",
            _ => "secondary"
        };
    }

    public class ProductoEnvio
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; } = 1;
        
        // ? MEJORA: Propiedades calculadas
        public decimal SubTotal => Precio * Cantidad;
        public string PrecioFormateado => Precio.ToString("C0");
        public string SubTotalFormateado => SubTotal.ToString("C0");
    }

    public class DireccionEnvio
    {
        public string Direccion { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public string? Barrio { get; set; }
        
        // ? MEJORA: Dirección completa formateada
        public string DireccionCompleta
        {
            get
            {
                var direccionCompleta = Direccion;
                if (!string.IsNullOrEmpty(Complemento))
                    direccionCompleta += $", {Complemento}";
                if (!string.IsNullOrEmpty(Barrio))
                    direccionCompleta += $", {Barrio}";
                direccionCompleta += $", {Ciudad}, {Departamento}";
                if (!string.IsNullOrEmpty(CodigoPostal) && CodigoPostal != "000000")
                    direccionCompleta += $", {CodigoPostal}";
                return direccionCompleta;
            }
        }
    }
}