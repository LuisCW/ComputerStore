namespace ComputerStore.Models
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; } // NUEVO: Para mostrar descuentos
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty; // Marca del producto
        public string Component { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty; // Color del producto
        public string Availability { get; set; } = string.Empty;
        public int Stock { get; set; } // NUEVO: Stock disponible para validaciones en cliente/servidor
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
        
        // NUEVO: Para soporte de múltiples imágenes
        public List<ProductoImagen> Imagenes { get; set; } = new List<ProductoImagen>();

        // NUEVO: Nombre de campaña de marketing activa aplicada (si corresponde)
        public string? CampaignName { get; set; }
    }
}
