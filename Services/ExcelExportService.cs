using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Table.PivotTable;
using ComputerStore.Models;

namespace ComputerStore.Services
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportProductsToExcelAsync(List<ProductEntity> products);
        Task<byte[]> ExportProductsPivotTableAsync(List<ProductEntity> products);
        Task<byte[]> ExportShipmentsToExcelAsync(List<EnvioInfo> shipments);
        Task<byte[]> ExportShipmentsAnalysisAsync(List<EnvioInfo> shipments);
        Task<byte[]> ExportOrdersToExcelAsync(List<TransaccionInfo> orders);
        Task<byte[]> ExportOrdersAnalysisAsync(List<TransaccionInfo> orders);
        Task<byte[]> ExportUsersToExcelAsync(List<ApplicationUser> users, List<ApplicationUser> admins);
        Task<byte[]> ExportUsersAnalysisAsync(List<ApplicationUser> users, List<ApplicationUser> admins);
        Task<byte[]> ExportTransactionsToExcelAsync(List<TransaccionEntity> transactions);
        Task<byte[]> ExportTransactionsAnalysisAsync(List<TransaccionEntity> transactions, decimal totalVentas, decimal costoReposicion, decimal gananciaNeta, double margenPorcentaje);
    }

    public class ExcelExportService : IExcelExportService
    {
        private readonly ILogger<ExcelExportService> _logger;

        static ExcelExportService()
        {
            // Configurar EPPlus 5.x para uso no comercial
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public ExcelExportService(ILogger<ExcelExportService> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> ExportProductsToExcelAsync(List<ProductEntity> products)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Iniciando generación de Excel con {Count} productos", products.Count);
                    
                    using var package = new ExcelPackage();
                    
                    // Crear hoja de trabajo
                    var worksheet = package.Workbook.Worksheets.Add("Productos");
                    
                    // Configurar headers
                    var headers = new string[]
                    {
                        "ID", "SKU", "Nombre", "Marca", "Categoría", 
                        "Precio Venta", "Precio Compra", "Stock", 
                        "Stock Mínimo", "Estado", "Disponibilidad",
                        "Fecha Creación", "Margen %", "Valor Inventario", "Estado Stock"
                    };

                    // Escribir headers
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[1, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Escribir datos
                    int row = 2;
                    foreach (var product in products)
                    {
                        try
                        {
                            worksheet.Cells[row, 1].Value = product.Id;
                            worksheet.Cells[row, 2].Value = CleanString(product.SKU);
                            worksheet.Cells[row, 3].Value = CleanString(product.Name);
                            worksheet.Cells[row, 4].Value = CleanString(product.Brand);
                            worksheet.Cells[row, 5].Value = CleanString(product.Category);
                            worksheet.Cells[row, 6].Value = (double)product.Price;
                            worksheet.Cells[row, 7].Value = (double)(product.PrecioCompra ?? 0);
                            worksheet.Cells[row, 8].Value = product.Stock;
                            worksheet.Cells[row, 9].Value = product.StockMinimo;
                            worksheet.Cells[row, 10].Value = product.IsActive ? "ACTIVO" : "OCULTO";
                            worksheet.Cells[row, 11].Value = CleanString(product.Availability) ?? "Disponible";
                            worksheet.Cells[row, 12].Value = product.CreatedDate;

                            // Calcular margen de ganancia
                            if (product.PrecioCompra > 0)
                            {
                                var margen = ((product.Price - product.PrecioCompra.Value) / product.PrecioCompra.Value);
                                worksheet.Cells[row, 13].Value = (double)margen;
                            }
                            else
                            {
                                worksheet.Cells[row, 13].Value = 0;
                            }

                            // Valor del inventario
                            var valorInventario = product.Stock * (product.PrecioCompra ?? product.Price);
                            worksheet.Cells[row, 14].Value = (double)valorInventario;

                            // Estado del stock
                            string estadoStock = product.Stock == 0 ? "AGOTADO" : 
                                               product.Stock <= product.StockMinimo ? "BAJO STOCK" : "NORMAL";
                            worksheet.Cells[row, 15].Value = estadoStock;

                            row++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error procesando producto ID: {ProductId}", product.Id);
                            // Continuar con el siguiente producto
                        }
                    }

                    // Formatear columnas si hay datos
                    if (products.Any())
                    {
                        var lastRow = row - 1;
                        
                        // Formatear precios como moneda
                        worksheet.Cells[2, 6, lastRow, 7].Style.Numberformat.Format = "$#,##0.00";
                        worksheet.Cells[2, 14, lastRow, 14].Style.Numberformat.Format = "$#,##0.00";
                        
                        // Formatear porcentajes
                        worksheet.Cells[2, 13, lastRow, 13].Style.Numberformat.Format = "0.00%";
                        
                        // Formatear fechas
                        worksheet.Cells[2, 12, lastRow, 12].Style.Numberformat.Format = "dd/mm/yyyy";
                    }

                    // Auto-ajustar columnas
                    worksheet.Cells.AutoFitColumns();

                    // Generar archivo
                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El archivo Excel generado está vacío");
                    }

                    _logger.LogInformation("Excel generado exitosamente: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico generando Excel");
                    
                    // Generar CSV como fallback
                    _logger.LogInformation("Generando CSV como alternativa...");
                    return GenerateSimpleCsv(products);
                }
            });
        }

        public async Task<byte[]> ExportProductsPivotTableAsync(List<ProductEntity> products)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Generando Excel con tabla dinámica real - {Count} productos", products.Count);
                    
                    using var package = new ExcelPackage();
                    
                    // HOJA 1: Datos fuente para la tabla dinámica
                    var dataSheet = package.Workbook.Worksheets.Add("Datos Fuente");
                    
                    // Headers para datos fuente
                    var sourceHeaders = new string[] { 
                        "Categoría", "Marca", "Nombre", "Precio", "PrecioCompra", 
                        "Stock", "ValorInventario", "Estado", "EstadoStock" 
                    };
                    
                    for (int i = 0; i < sourceHeaders.Length; i++)
                    {
                        var cell = dataSheet.Cells[1, i + 1];
                        cell.Value = sourceHeaders[i];
                        cell.Style.Font.Bold = true;
                    }

                    // Llenar datos fuente
                    int dataRow = 2;
                    foreach (var product in products)
                    {
                        dataSheet.Cells[dataRow, 1].Value = CleanString(product.Category);
                        dataSheet.Cells[dataRow, 2].Value = CleanString(product.Brand);
                        dataSheet.Cells[dataRow, 3].Value = CleanString(product.Name);
                        dataSheet.Cells[dataRow, 4].Value = (double)product.Price;
                        dataSheet.Cells[dataRow, 5].Value = (double)(product.PrecioCompra ?? 0);
                        dataSheet.Cells[dataRow, 6].Value = product.Stock;
                        dataSheet.Cells[dataRow, 7].Value = (double)(product.Stock * (product.PrecioCompra ?? product.Price));
                        dataSheet.Cells[dataRow, 8].Value = product.IsActive ? "ACTIVO" : "OCULTO";
                        dataSheet.Cells[dataRow, 9].Value = product.Stock == 0 ? "AGOTADO" : 
                                                          product.Stock <= product.StockMinimo ? "BAJO STOCK" : "NORMAL";
                        dataRow++;
                    }

                    // Crear tabla de datos
                    var dataRange = dataSheet.Cells[1, 1, dataRow - 1, sourceHeaders.Length];
                    var dataTable = dataSheet.Tables.Add(dataRange, "TablaProductos");
                    dataTable.TableStyle = TableStyles.Medium9;
                    
                    // Auto-ajustar columnas de datos
                    dataSheet.Cells.AutoFitColumns();

                    try 
                    {
                        // HOJA 2: Intentar crear tabla dinámica real
                        var pivotSheet = package.Workbook.Worksheets.Add("Tabla Dinámica");
                        
                        // Crear tabla dinámica
                        var pivotTable = pivotSheet.PivotTables.Add(pivotSheet.Cells["A3"], dataRange, "TablaDinamicaProductos");

                        // Configurar campos de fila
                        pivotTable.RowFields.Add(pivotTable.Fields["Categoría"]);
                        pivotTable.RowFields.Add(pivotTable.Fields["Marca"]);

                        // Configurar campos de datos
                        var stockField = pivotTable.DataFields.Add(pivotTable.Fields["Stock"]);
                        stockField.Name = "Total Stock";
                        stockField.Function = DataFieldFunctions.Sum;

                        var valorField = pivotTable.DataFields.Add(pivotTable.Fields["ValorInventario"]);
                        valorField.Name = "Valor Total";
                        valorField.Function = DataFieldFunctions.Sum;
                        valorField.Format = "$#,##0.00";

                        var countField = pivotTable.DataFields.Add(pivotTable.Fields["Nombre"]);
                        countField.Name = "Cantidad Productos";
                        countField.Function = DataFieldFunctions.Count;

                        // Campo de filtro
                        pivotTable.PageFields.Add(pivotTable.Fields["Estado"]);

                        // Título para la tabla dinámica
                        pivotSheet.Cells["A1"].Value = "TABLA DINÁMICA - ANÁLISIS DE PRODUCTOS";
                        pivotSheet.Cells["A1"].Style.Font.Size = 16;
                        pivotSheet.Cells["A1"].Style.Font.Bold = true;

                        _logger.LogInformation("Tabla dinámica real creada exitosamente");
                    }
                    catch (Exception pivotEx)
                    {
                        _logger.LogWarning(pivotEx, "No se pudo crear tabla dinámica real, creando resumen manual");
                        
                        // HOJA 2 ALTERNATIVA: Resumen manual si falla la tabla dinámica
                        var summarySheet = package.Workbook.Worksheets.Add("Resumen Agrupado");
                        
                        // Título - CORREGIDO: usar el formato correcto
                        summarySheet.Cells[1, 1].Value = "RESUMEN DE PRODUCTOS POR CATEGORÍA Y MARCA";
                        summarySheet.Cells[1, 1].Style.Font.Size = 16;
                        summarySheet.Cells[1, 1].Style.Font.Bold = true;
                        summarySheet.Cells[1, 1, 1, 5].Merge = true; // CORREGIDO: usar coordenadas numéricas
                        
                        // Headers del resumen
                        var headers = new string[] { "Categoría", "Marca", "Cantidad Productos", "Stock Total", "Valor Total" };
                        
                        for (int i = 0; i < headers.Length; i++)
                        {
                            var cell = summarySheet.Cells[3, i + 1];
                            cell.Value = headers[i];
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(144, 238, 144));
                            cell.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                        }

                        // Agrupar datos
                        var groupedData = products
                            .Where(p => p != null)
                            .GroupBy(p => new { 
                                Category = p.Category ?? "Sin Categoría", 
                                Brand = p.Brand ?? "Sin Marca" 
                            })
                            .Select(g => new
                            {
                                Categoria = g.Key.Category,
                                Marca = g.Key.Brand,
                                CantidadProductos = g.Count(),
                                StockTotal = g.Sum(p => p.Stock),
                                ValorTotal = g.Sum(p => p.Stock * (p.PrecioCompra ?? p.Price))
                            })
                            .OrderBy(x => x.Categoria)
                            .ThenBy(x => x.Marca)
                            .ToList();

                        // Llenar datos agrupados
                        int summaryRow = 4;
                        foreach (var item in groupedData)
                        {
                            summarySheet.Cells[summaryRow, 1].Value = CleanString(item.Categoria);
                            summarySheet.Cells[summaryRow, 2].Value = CleanString(item.Marca);
                            summarySheet.Cells[summaryRow, 3].Value = item.CantidadProductos;
                            summarySheet.Cells[summaryRow, 4].Value = item.StockTotal;
                            summarySheet.Cells[summaryRow, 5].Value = (double)item.ValorTotal;
                            summaryRow++;
                        }

                        // Formatear
                        if (groupedData.Any())
                        {
                            var lastSummaryRow = summaryRow - 1;
                            summarySheet.Cells[4, 5, lastSummaryRow, 5].Style.Numberformat.Format = "$#,##0.00";
                            
                            // Crear tabla
                            var summaryRange = summarySheet.Cells[3, 1, lastSummaryRow, 5];
                            var summaryTable = summarySheet.Tables.Add(summaryRange, "ResumenProductos");
                            summaryTable.TableStyle = TableStyles.Medium2;
                        }

                        summarySheet.Cells.AutoFitColumns();
                    }

                    // HOJA 3: Estadísticas adicionales
                    var statsSheet = package.Workbook.Worksheets.Add("Estadísticas");
                    
                    statsSheet.Cells[1, 1].Value = "ESTADÍSTICAS GENERALES";
                    statsSheet.Cells[1, 1].Style.Font.Size = 16;
                    statsSheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    var totalProductos = products.Count;
                    var totalStock = products.Sum(p => p.Stock);
                    var totalValor = products.Sum(p => p.Stock * (p.PrecioCompra ?? p.Price));
                    var promedioPrice = products.Average(p => p.Price);
                    var productosAgotados = products.Count(p => p.Stock == 0);
                    var productosBajoStock = products.Count(p => p.Stock <= p.StockMinimo && p.Stock > 0);
                    
                    statsSheet.Cells[3, 1].Value = "Total de Productos:"; 
                    statsSheet.Cells[3, 2].Value = totalProductos;
                    statsSheet.Cells[4, 1].Value = "Total Stock:"; 
                    statsSheet.Cells[4, 2].Value = totalStock;
                    statsSheet.Cells[5, 1].Value = "Valor Total Inventario:"; 
                    statsSheet.Cells[5, 2].Value = (double)totalValor; 
                    statsSheet.Cells[5, 2].Style.Numberformat.Format = "$#,##0.00";
                    statsSheet.Cells[6, 1].Value = "Precio Promedio:"; 
                    statsSheet.Cells[6, 2].Value = (double)promedioPrice;
                    statsSheet.Cells[6, 2].Style.Numberformat.Format = "$#,##0.00";
                    statsSheet.Cells[7, 1].Value = "Productos Agotados:"; 
                    statsSheet.Cells[7, 2].Value = productosAgotados;
                    statsSheet.Cells[8, 1].Value = "Productos Bajo Stock:"; 
                    statsSheet.Cells[8, 2].Value = productosBajoStock;
                    
                    // Formatear estadísticas - CORREGIDO: usar coordenadas numéricas
                    statsSheet.Cells[3, 1, 8, 1].Style.Font.Bold = true;
                    statsSheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El archivo de análisis está vacío");
                    }

                    _logger.LogInformation("Excel con análisis completo generado: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando análisis Excel");
                    return GenerateSimpleCsv(products);
                }
            });
        }

        // ?? EXPORTAR ENVÍOS A EXCEL
        public async Task<byte[]> ExportShipmentsToExcelAsync(List<EnvioInfo> shipments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Iniciando generación de Excel con {Count} envíos", shipments.Count);
                    
                    using var package = new ExcelPackage();
                    
                    var worksheet = package.Workbook.Worksheets.Add("Envíos");
                    
                    // Headers para envíos
                    var headers = new string[]
                    {
                        "Número Guía", "Order ID", "Transaction ID", "Estado", "Producto", 
                        "Precio Total", "Fecha Creación", "Fecha Estimada Entrega", 
                        "Dirección Envío", "Transportadora", "Peso (Kg)", "Observaciones"
                    };

                    // Escribir headers
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[1, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(34, 139, 34));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Escribir datos de envíos
                    int row = 2;
                    foreach (var shipment in shipments)
                    {
                        try
                        {
                            worksheet.Cells[row, 1].Value = CleanString(shipment.NumeroGuia);
                            worksheet.Cells[row, 2].Value = shipment.OrderId.ToString();
                            worksheet.Cells[row, 3].Value = CleanString(shipment.TransactionId);
                            worksheet.Cells[row, 4].Value = CleanString(shipment.EstadoDisplay ?? shipment.Estado);
                            worksheet.Cells[row, 5].Value = CleanString(shipment.NombreProducto);
                            worksheet.Cells[row, 6].Value = (double)shipment.PrecioTotal;
                            worksheet.Cells[row, 7].Value = shipment.FechaCreacion;
                            worksheet.Cells[row, 8].Value = shipment.FechaEstimadaEntrega;
                            worksheet.Cells[row, 9].Value = CleanString(shipment.DireccionEnvio);
                            worksheet.Cells[row, 10].Value = CleanString(shipment.Transportadora);
                            worksheet.Cells[row, 11].Value = shipment.PesoKg;
                            worksheet.Cells[row, 12].Value = CleanString(shipment.Observaciones);

                            row++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error procesando envío: {NumeroGuia}", shipment.NumeroGuia);
                        }
                    }

                    // Formatear columnas si hay datos
                    if (shipments.Any())
                    {
                        var lastRow = row - 1;
                        
                        // Formatear precios
                        worksheet.Cells[2, 6, lastRow, 6].Style.Numberformat.Format = "$#,##0.00";
                        
                        // Formatear fechas
                        worksheet.Cells[2, 7, lastRow, 8].Style.Numberformat.Format = "dd/mm/yyyy";
                    }

                    worksheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El archivo Excel de envíos está vacío");
                    }

                    _logger.LogInformation("Excel de envíos generado exitosamente: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico generando Excel de envíos");
                    return GenerateShipmentsCsv(shipments);
                }
            });
        }

        // ?? ANÁLISIS DE ENVÍOS CON TABLA DINÁMICA
        public async Task<byte[]> ExportShipmentsAnalysisAsync(List<EnvioInfo> shipments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Generando análisis de envíos - {Count} envíos", shipments.Count);
                    
                    using var package = new ExcelPackage();
                    
                    // HOJA 1: Datos fuente
                    var dataSheet = package.Workbook.Worksheets.Add("Datos Fuente");
                    
                    var sourceHeaders = new string[] { 
                        "Estado", "Transportadora", "Precio", "PesoKg", 
                        "FechaCreacion", "Mes", "Año", "DireccionEnvio"
                    };
                    
                    for (int i = 0; i < sourceHeaders.Length; i++)
                    {
                        var cell = dataSheet.Cells[1, i + 1];
                        cell.Value = sourceHeaders[i];
                        cell.Style.Font.Bold = true;
                    }

                    int dataRow = 2;
                    foreach (var shipment in shipments)
                    {
                        dataSheet.Cells[dataRow, 1].Value = CleanString(shipment.EstadoDisplay ?? shipment.Estado);
                        dataSheet.Cells[dataRow, 2].Value = CleanString(shipment.Transportadora);
                        dataSheet.Cells[dataRow, 3].Value = (double)shipment.PrecioTotal;
                        dataSheet.Cells[dataRow, 4].Value = shipment.PesoKg;
                        dataSheet.Cells[dataRow, 5].Value = shipment.FechaCreacion;
                        dataSheet.Cells[dataRow, 6].Value = shipment.FechaCreacion.ToString("MMMM yyyy");
                        dataSheet.Cells[dataRow, 7].Value = shipment.FechaCreacion.Year;
                        dataSheet.Cells[dataRow, 8].Value = CleanString(shipment.DireccionEnvio);
                        dataRow++;
                    }

                    var dataRange = dataSheet.Cells[1, 1, dataRow - 1, sourceHeaders.Length];
                    var dataTable = dataSheet.Tables.Add(dataRange, "TablaEnvios");
                    dataTable.TableStyle = TableStyles.Medium6;
                    dataSheet.Cells.AutoFitColumns();

                    // HOJA 2: Resumen por Estado
                    var summarySheet = package.Workbook.Worksheets.Add("Resumen por Estado");
                    
                    summarySheet.Cells[1, 1].Value = "ANÁLISIS DE ENVÍOS POR ESTADO";
                    summarySheet.Cells[1, 1].Style.Font.Size = 16;
                    summarySheet.Cells[1, 1].Style.Font.Bold = true;
                    summarySheet.Cells[1, 1, 1, 4].Merge = true;
                    
                    var estadoHeaders = new string[] { "Estado", "Cantidad", "Valor Total", "Porcentaje" };
                    for (int i = 0; i < estadoHeaders.Length; i++)
                    {
                        var cell = summarySheet.Cells[3, i + 1];
                        cell.Value = estadoHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(70, 130, 180));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var estadoStats = shipments
                        .GroupBy(s => s.EstadoDisplay ?? s.Estado ?? "Sin Estado")
                        .Select(g => new
                        {
                            Estado = g.Key,
                            Cantidad = g.Count(),
                            ValorTotal = g.Sum(s => s.PrecioTotal),
                            Porcentaje = (double)g.Count() / shipments.Count
                        })
                        .OrderByDescending(s => s.Cantidad)
                        .ToList();

                    int estadoRow = 4;
                    foreach (var stat in estadoStats)
                    {
                        summarySheet.Cells[estadoRow, 1].Value = stat.Estado;
                        summarySheet.Cells[estadoRow, 2].Value = stat.Cantidad;
                        summarySheet.Cells[estadoRow, 3].Value = (double)stat.ValorTotal;
                        summarySheet.Cells[estadoRow, 4].Value = stat.Porcentaje;
                        estadoRow++;
                    }

                    if (estadoStats.Any())
                    {
                        var lastEstadoRow = estadoRow - 1;
                        summarySheet.Cells[4, 3, lastEstadoRow, 3].Style.Numberformat.Format = "$#,##0.00";
                        summarySheet.Cells[4, 4, lastEstadoRow, 4].Style.Numberformat.Format = "0.00%";
                        
                        var estadoRange = summarySheet.Cells[3, 1, lastEstadoRow, 4];
                        var estadoTable = summarySheet.Tables.Add(estadoRange, "ResumenEstados");
                        estadoTable.TableStyle = TableStyles.Medium3;
                    }

                    // HOJA 3: Estadísticas Generales
                    var statsSheet = package.Workbook.Worksheets.Add("Estadísticas");
                    
                    statsSheet.Cells[1, 1].Value = "ESTADÍSTICAS GENERALES DE ENVÍOS";
                    statsSheet.Cells[1, 1].Style.Font.Size = 16;
                    statsSheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    var totalEnvios = shipments.Count;
                    var totalValor = shipments.Sum(s => s.PrecioTotal);
                    var promedioValor = shipments.Average(s => s.PrecioTotal);
                    var pesoTotal = shipments.Sum(s => s.PesoKg);
                    var enviosEntregados = shipments.Count(s => (s.Estado ?? "").Contains("ENTREGADO"));
                    var enviosEnTransito = shipments.Count(s => (s.Estado ?? "").Contains("TRANSITO"));
                    
                    statsSheet.Cells[3, 1].Value = "Total Envíos:"; 
                    statsSheet.Cells[3, 2].Value = totalEnvios;
                    statsSheet.Cells[4, 1].Value = "Valor Total:"; 
                    statsSheet.Cells[4, 2].Value = (double)totalValor;
                    statsSheet.Cells[4, 2].Style.Numberformat.Format = "$#,##0.00";
                    statsSheet.Cells[5, 1].Value = "Valor Promedio:"; 
                    statsSheet.Cells[5, 2].Value = (double)promedioValor;
                    statsSheet.Cells[5, 2].Style.Numberformat.Format = "$#,##0.00";
                    statsSheet.Cells[6, 1].Value = "Peso Total (Kg):"; 
                    statsSheet.Cells[6, 2].Value = pesoTotal;
                    statsSheet.Cells[7, 1].Value = "Envíos Entregados:"; 
                    statsSheet.Cells[7, 2].Value = enviosEntregados;
                    statsSheet.Cells[8, 1].Value = "Envíos en Transito:"; 
                    statsSheet.Cells[8, 2].Value = enviosEnTransito;
                    
                    statsSheet.Cells[3, 1, 8, 1].Style.Font.Bold = true;
                    statsSheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El análisis de envíos está vacío");
                    }

                    _logger.LogInformation("Análisis de envíos generado: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando análisis de envíos");
                    return GenerateShipmentsCsv(shipments);
                }
            });
        }

        // ?? NUEVO: EXPORTAR PEDIDOS/TRANSACCIONES A EXCEL
        public async Task<byte[]> ExportOrdersToExcelAsync(List<TransaccionInfo> orders)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Iniciando generación de Excel con {Count} pedidos", orders.Count);
                    
                    using var package = new ExcelPackage();
                    
                    var worksheet = package.Workbook.Worksheets.Add("Pedidos");
                    
                    // Headers para pedidos
                    var headers = new string[]
                    {
                        "Transaction ID", "Order ID", "Reference Code", "Estado", "Método de Pago", 
                        "Monto", "Moneda", "Fecha Transacción", "Usuario", "Email Usuario",
                        "Response Message", "Trazability Code", "Authorization Code"
                    };

                    // Escribir headers
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[1, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(153, 102, 204));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Escribir datos de pedidos
                    int row = 2;
                    foreach (var order in orders)
                    {
                        try
                        {
                            worksheet.Cells[row, 1].Value = CleanString(order.TransactionId);
                            worksheet.Cells[row, 2].Value = order.OrderId.ToString();
                            worksheet.Cells[row, 3].Value = CleanString(order.ReferenceCode);
                            worksheet.Cells[row, 4].Value = CleanString(GetEstadoDisplay(order.Estado));
                            worksheet.Cells[row, 5].Value = CleanString(order.MetodoPago);
                            worksheet.Cells[row, 6].Value = (double)order.Monto;
                            worksheet.Cells[row, 7].Value = CleanString(order.Moneda);
                            worksheet.Cells[row, 8].Value = order.FechaTransaccion;
                            worksheet.Cells[row, 9].Value = CleanString(order.UserName);
                            worksheet.Cells[row, 10].Value = CleanString(order.UserEmail);
                            worksheet.Cells[row, 11].Value = CleanString(order.ResponseMessage);
                            worksheet.Cells[row, 12].Value = CleanString(order.TrazabilityCode);
                            worksheet.Cells[row, 13].Value = CleanString(order.AuthorizationCode);

                            row++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error procesando pedido: {TransactionId}", order.TransactionId);
                        }
                    }

                    // Formatear columnas si hay datos
                    if (orders.Any())
                    {
                        var lastRow = row - 1;
                        
                        // Formatear montos
                        worksheet.Cells[2, 6, lastRow, 6].Style.Numberformat.Format = "$#,##0.00";
                        
                        // Formatear fechas
                        worksheet.Cells[2, 8, lastRow, 8].Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";
                    }

                    worksheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El archivo Excel de pedidos está vacío");
                    }

                    _logger.LogInformation("Excel de pedidos generado exitosamente: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico generando Excel de pedidos");
                    return GenerateOrdersCsv(orders);
                }
            });
        }

        // ?? NUEVO: ANÁLISIS DE PEDIDOS CON TABLA DINÁMICA
        public async Task<byte[]> ExportOrdersAnalysisAsync(List<TransaccionInfo> orders)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Generando análisis de pedidos - {Count} transacciones", orders.Count);
                    
                    using var package = new ExcelPackage();
                    
                    // HOJA 1: Datos fuente
                    var dataSheet = package.Workbook.Worksheets.Add("Datos Fuente");
                    
                    var sourceHeaders = new string[] { 
                        "Estado", "MetodoPago", "Monto", "Moneda", 
                        "FechaTransaccion", "Mes", "Año", "Usuario"
                    };
                    
                    for (int i = 0; i < sourceHeaders.Length; i++)
                    {
                        var cell = dataSheet.Cells[1, i + 1];
                        cell.Value = sourceHeaders[i];
                        cell.Style.Font.Bold = true;
                    }

                    int dataRow = 2;
                    foreach (var order in orders)
                    {
                        dataSheet.Cells[dataRow, 1].Value = CleanString(GetEstadoDisplay(order.Estado));
                        dataSheet.Cells[dataRow, 2].Value = CleanString(order.MetodoPago);
                        dataSheet.Cells[dataRow, 3].Value = (double)order.Monto;
                        dataSheet.Cells[dataRow, 4].Value = CleanString(order.Moneda);
                        dataSheet.Cells[dataRow, 5].Value = order.FechaTransaccion;
                        dataSheet.Cells[dataRow, 6].Value = order.FechaTransaccion.ToString("MMMM yyyy");
                        dataSheet.Cells[dataRow, 7].Value = order.FechaTransaccion.Year;
                        dataSheet.Cells[dataRow, 8].Value = CleanString(order.UserName);
                        dataRow++;
                    }

                    var dataRange = dataSheet.Cells[1, 1, dataRow - 1, sourceHeaders.Length];
                    var dataTable = dataSheet.Tables.Add(dataRange, "TablaPedidos");
                    dataTable.TableStyle = TableStyles.Medium7;
                    dataSheet.Cells.AutoFitColumns();

                    // HOJA 2: Resumen por Estado
                    var summarySheet = package.Workbook.Worksheets.Add("Resumen por Estado");
                    
                    summarySheet.Cells[1, 1].Value = "ANÁLISIS DE TRANSACCIONES POR ESTADO";
                    summarySheet.Cells[1, 1].Style.Font.Size = 16;
                    summarySheet.Cells[1, 1].Style.Font.Bold = true;
                    summarySheet.Cells[1, 1, 1, 4].Merge = true;
                    
                    var estadoHeaders = new string[] { "Estado", "Cantidad", "Valor Total", "Porcentaje" };
                    for (int i = 0; i < estadoHeaders.Length; i++)
                    {
                        var cell = summarySheet.Cells[3, i + 1];
                        cell.Value = estadoHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(153, 102, 204));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var estadoStats = orders
                        .GroupBy(o => GetEstadoDisplay(o.Estado))
                        .Select(g => new
                        {
                            Estado = g.Key,
                            Cantidad = g.Count(),
                            ValorTotal = g.Sum(o => o.Monto),
                            Porcentaje = (double)g.Count() / orders.Count
                        })
                        .OrderByDescending(s => s.Cantidad)
                        .ToList();

                    int estadoRow = 4;
                    foreach (var stat in estadoStats)
                    {
                        summarySheet.Cells[estadoRow, 1].Value = stat.Estado;
                        summarySheet.Cells[estadoRow, 2].Value = stat.Cantidad;
                        summarySheet.Cells[estadoRow, 3].Value = (double)stat.ValorTotal;
                        summarySheet.Cells[estadoRow, 4].Value = stat.Porcentaje;
                        estadoRow++;
                    }

                    if (estadoStats.Any())
                    {
                        var lastEstadoRow = estadoRow - 1;
                        summarySheet.Cells[4, 3, lastEstadoRow, 3].Style.Numberformat.Format = "$#,##0.00";
                        summarySheet.Cells[4, 4, lastEstadoRow, 4].Style.Numberformat.Format = "0.00%";
                        
                        var estadoRange = summarySheet.Cells[3, 1, lastEstadoRow, 4];
                        var estadoTable = summarySheet.Tables.Add(estadoRange, "ResumenEstados");
                        estadoTable.TableStyle = TableStyles.Medium4;
                    }

                    // HOJA 3: Resumen por Método de Pago
                    var paymentSheet = package.Workbook.Worksheets.Add("Resumen por Método Pago");
                    
                    paymentSheet.Cells[1, 1].Value = "ANÁLISIS POR MÉTODO DE PAGO";
                    paymentSheet.Cells[1, 1].Style.Font.Size = 16;
                    paymentSheet.Cells[1, 1].Style.Font.Bold = true;
                    paymentSheet.Cells[1, 1, 1, 4].Merge = true;

                    var paymentHeaders = new string[] { "Método de Pago", "Cantidad", "Valor Total", "Porcentaje" };
                    for (int i = 0; i < paymentHeaders.Length; i++)
                    {
                        var cell = paymentSheet.Cells[3, i + 1];
                        cell.Value = paymentHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(60, 179, 113));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var paymentStats = orders
                        .GroupBy(o => o.MetodoPago ?? "Sin Método")
                        .Select(g => new
                        {
                            Metodo = g.Key,
                            Cantidad = g.Count(),
                            ValorTotal = g.Sum(o => o.Monto),
                            Porcentaje = (double)g.Count() / orders.Count
                        })
                        .OrderByDescending(s => s.ValorTotal)
                        .ToList();

                    int paymentRow = 4;
                    foreach (var stat in paymentStats)
                    {
                        paymentSheet.Cells[paymentRow, 1].Value = stat.Metodo;
                        paymentSheet.Cells[paymentRow, 2].Value = stat.Cantidad;
                        paymentSheet.Cells[paymentRow, 3].Value = (double)stat.ValorTotal;
                        paymentSheet.Cells[paymentRow, 4].Value = stat.Porcentaje;
                        paymentRow++;
                    }

                    if (paymentStats.Any())
                    {
                        var lastPaymentRow = paymentRow - 1;
                        paymentSheet.Cells[4, 3, lastPaymentRow, 3].Style.Numberformat.Format = "$#,##0.00";
                        paymentSheet.Cells[4, 4, lastPaymentRow, 4].Style.Numberformat.Format = "0.00%";
                        
                        var paymentRange = paymentSheet.Cells[3, 1, lastPaymentRow, 4];
                        var paymentTable = paymentSheet.Tables.Add(paymentRange, "ResumenMetodos");
                        paymentTable.TableStyle = TableStyles.Medium5;
                    }

                    paymentSheet.Cells.AutoFitColumns();

                    // HOJA 4: Estadísticas Generales
                    var statsSheet = package.Workbook.Worksheets.Add("Estadísticas");
                    
                    statsSheet.Cells[1, 1].Value = "ESTADÍSTICAS GENERALES DE TRANSACCIONES";
                    statsSheet.Cells[1, 1].Style.Font.Size = 16;
                    statsSheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    var totalTransacciones = orders.Count;
                    var totalMonto = orders.Sum(o => o.Monto);
                    var promedioMonto = orders.Average(o => o.Monto);
                    var transaccionesAprobadas = orders.Count(o => o.Estado == "APPROVED");
                    var transaccionesRechazadas = orders.Count(o => o.Estado == "DECLINED" || o.Estado == "REJECTED");
                    var transaccionesPendientes = orders.Count(o => o.Estado == "PENDING");
                    var ventasPSE = orders.Where(o => (o.MetodoPago ?? "").Contains("PSE")).Sum(o => o.Monto);
                    var ventasTarjetas = orders.Where(o => (o.MetodoPago ?? "").Contains("CREDIT_CARD") || (o.MetodoPago ?? "").Contains("DEBIT_CARD")).Sum(o => o.Monto);
                    
                    statsSheet.Cells[3, 1].Value = "Total Transacciones:"; 
                    statsSheet.Cells[3, 2].Value = totalTransacciones;
                    statsSheet.Cells[4, 1].Value = "Monto Total:"; 
                    statsSheet.Cells[4, 2].Value = (double)totalMonto;
                    statsSheet.Cells[4, 2].Style.Numberformat.Format = "$#,##0.00";
                    statsSheet.Cells[5, 1].Value = "Monto Promedio:"; 
                    statsSheet.Cells[5, 2].Value = (double)promedioMonto;
                    statsSheet.Cells[5, 2].Style.Numberformat.Format = "$#,##0.00";
                    statsSheet.Cells[6, 1].Value = "Transacciones Aprobadas:"; 
                    statsSheet.Cells[6, 2].Value = transaccionesAprobadas;
                    statsSheet.Cells[7, 1].Value = "Transacciones Rechazadas:"; 
                    statsSheet.Cells[7, 2].Value = transaccionesRechazadas;
                    statsSheet.Cells[8, 1].Value = "Transacciones Pendientes:"; 
                    statsSheet.Cells[8, 2].Value = transaccionesPendientes;
                    statsSheet.Cells[9, 1].Value = "Ventas PSE:"; 
                    statsSheet.Cells[9, 2].Value = (double)ventasPSE;
                    statsSheet.Cells[9, 2].Style.Numberformat.Format = "$#,##0.00";
                    statsSheet.Cells[10, 1].Value = "Ventas Tarjetas:"; 
                    statsSheet.Cells[10, 2].Value = (double)ventasTarjetas;
                    statsSheet.Cells[10, 2].Style.Numberformat.Format = "$#,##0.00";
                    
                    statsSheet.Cells[3, 1, 10, 1].Style.Font.Bold = true;
                    statsSheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El análisis de pedidos está vacío");
                    }

                    _logger.LogInformation("Análisis de pedidos generado: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando análisis de pedidos");
                    return GenerateOrdersCsv(orders);
                }
            });
        }

        // ?? NUEVO: EXPORTAR USUARIOS A EXCEL
        public async Task<byte[]> ExportUsersToExcelAsync(List<ApplicationUser> users, List<ApplicationUser> admins)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Iniciando generación de Excel con {Count} usuarios", users.Count);
                    
                    using var package = new ExcelPackage();
                    
                    var worksheet = package.Workbook.Worksheets.Add("Usuarios");
                    
                    // Headers para usuarios
                    var headers = new string[]
                    {
                        "ID", "Usuario", "Email", "Nombre Completo", "Tipo Documento", 
                        "Número Documento", "Teléfono", "Ciudad", "Departamento", 
                        "Dirección", "Fecha Registro", "Último Acceso", "Estado", "Roles"
                    };

                    // Escribir headers
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[1, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(54, 162, 235));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Escribir datos de usuarios
                    int row = 2;
                    foreach (var user in users)
                    {
                        try
                        {
                            worksheet.Cells[row, 1].Value = CleanString(user.Id);
                            worksheet.Cells[row, 2].Value = CleanString(user.UserName);
                            worksheet.Cells[row, 3].Value = CleanString(user.Email);
                            
                            // Nombre completo
                            var nombreCompleto = $"{user.PrimerNombre} {user.SegundoNombre} {user.PrimerApellido} {user.SegundoApellido}".Trim();
                            worksheet.Cells[row, 4].Value = CleanString(nombreCompleto);
                            
                            worksheet.Cells[row, 5].Value = CleanString(user.TipoDocumento);
                            worksheet.Cells[row, 6].Value = CleanString(user.NumeroDocumento);
                            worksheet.Cells[row, 7].Value = CleanString(user.PhoneNumber);
                            worksheet.Cells[row, 8].Value = CleanString(user.Ciudad);
                            worksheet.Cells[row, 9].Value = CleanString(user.Departamento);
                            worksheet.Cells[row, 10].Value = CleanString(user.Direccion);
                            worksheet.Cells[row, 11].Value = user.FechaRegistro;
                            worksheet.Cells[row, 12].Value = user.FechaUltimoAcceso ?? DateTime.MinValue;
                            worksheet.Cells[row, 13].Value = user.IsActive ? "Activo" : "Inactivo";
                            
                            // Determinar roles
                            var roles = "Usuario";
                            if (admins.Any(a => a.Id == user.Id))
                            {
                                roles = "Administrador";
                            }
                            worksheet.Cells[row, 14].Value = roles;

                            row++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error procesando usuario: {UserId}", user.Id);
                        }
                    }

                    // Formatear columnas si hay datos
                    if (users.Any())
                    {
                        var lastRow = row - 1;
                        
                        // Formatear fechas
                        worksheet.Cells[2, 11, lastRow, 12].Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";
                    }

                    // Auto-ajustar columnas
                    worksheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El archivo Excel de usuarios está vacío");
                    }

                    _logger.LogInformation("Excel de usuarios generado exitosamente: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico generando Excel de usuarios");
                    return GenerateUsersCsv(users, admins);
                }
            });
        }

        // ?? NUEVO: ANÁLISIS DE USUARIOS CON TABLA DINÁMICA
        public async Task<byte[]> ExportUsersAnalysisAsync(List<ApplicationUser> users, List<ApplicationUser> admins)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Generando análisis de usuarios - {Count} usuarios", users.Count);
                    
                    using var package = new ExcelPackage();
                    
                    // HOJA 1: Datos fuente
                    var dataSheet = package.Workbook.Worksheets.Add("Datos Fuente");
                    
                    var sourceHeaders = new string[] { 
                        "Usuario", "Email", "TipoDocumento", "Ciudad", "Departamento", 
                        "FechaRegistro", "Mes", "Año", "Estado", "Rol"
                    };
                    
                    for (int i = 0; i < sourceHeaders.Length; i++)
                    {
                        var cell = dataSheet.Cells[1, i + 1];
                        cell.Value = sourceHeaders[i];
                        cell.Style.Font.Bold = true;
                    }

                    int dataRow = 2;
                    foreach (var user in users)
                    {
                        dataSheet.Cells[dataRow, 1].Value = CleanString(user.UserName);
                        dataSheet.Cells[dataRow, 2].Value = CleanString(user.Email);
                        dataSheet.Cells[dataRow, 3].Value = CleanString(user.TipoDocumento);
                        dataSheet.Cells[dataRow, 4].Value = CleanString(user.Ciudad);
                        dataSheet.Cells[dataRow, 5].Value = CleanString(user.Departamento);
                        dataSheet.Cells[dataRow, 6].Value = user.FechaRegistro;
                        dataSheet.Cells[dataRow, 7].Value = user.FechaRegistro.ToString("MMMM yyyy");
                        dataSheet.Cells[dataRow, 8].Value = user.FechaRegistro.Year;
                        dataSheet.Cells[dataRow, 9].Value = user.IsActive ? "Activo" : "Inactivo";
                        dataSheet.Cells[dataRow, 10].Value = admins.Any(a => a.Id == user.Id) ? "Administrador" : "Usuario";
                        dataRow++;
                    }

                    var dataRange = dataSheet.Cells[1, 1, dataRow - 1, sourceHeaders.Length];
                    var dataTable = dataSheet.Tables.Add(dataRange, "TablaUsuarios");
                    dataTable.TableStyle = TableStyles.Medium8;
                    dataSheet.Cells.AutoFitColumns();

                    // HOJA 2: Resumen por Ciudad
                    var citySheet = package.Workbook.Worksheets.Add("Resumen por Ciudad");
                    
                    citySheet.Cells[1, 1].Value = "ANÁLISIS DE USUARIOS POR CIUDAD";
                    citySheet.Cells[1, 1].Style.Font.Size = 16;
                    citySheet.Cells[1, 1].Style.Font.Bold = true;
                    citySheet.Cells[1, 1, 1, 4].Merge = true;
                    
                    var cityHeaders = new string[] { "Ciudad", "Cantidad", "Activos", "Porcentaje" };
                    for (int i = 0; i < cityHeaders.Length; i++)
                    {
                        var cell = citySheet.Cells[3, i + 1];
                        cell.Value = cityHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(54, 162, 235));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var cityStats = users
                        .GroupBy(u => u.Ciudad ?? "Sin Ciudad")
                        .Select(g => new
                        {
                            Ciudad = g.Key,
                            Cantidad = g.Count(),
                            Activos = g.Count(u => u.IsActive),
                            Porcentaje = (double)g.Count() / users.Count
                        })
                        .OrderByDescending(s => s.Cantidad)
                        .ToList();

                    int cityRow = 4;
                    foreach (var stat in cityStats)
                    {
                        citySheet.Cells[cityRow, 1].Value = stat.Ciudad;
                        citySheet.Cells[cityRow, 2].Value = stat.Cantidad;
                        citySheet.Cells[cityRow, 3].Value = stat.Activos;
                        citySheet.Cells[cityRow, 4].Value = stat.Porcentaje;
                        cityRow++;
                    }

                    if (cityStats.Any())
                    {
                        var lastCityRow = cityRow - 1;
                        citySheet.Cells[4, 4, lastCityRow, 4].Style.Numberformat.Format = "0.00%";
                        
                        var cityRange = citySheet.Cells[3, 1, lastCityRow, 4];
                        var cityTable = citySheet.Tables.Add(cityRange, "ResumenCiudades");
                        cityTable.TableStyle = TableStyles.Medium2;
                    }

                    // HOJA 3: Resumen por Departamento
                    var deptoSheet = package.Workbook.Worksheets.Add("Resumen por Departamento");
                    
                    deptoSheet.Cells[1, 1].Value = "ANÁLISIS DE USUARIOS POR DEPARTAMENTO";
                    deptoSheet.Cells[1, 1].Style.Font.Size = 16;
                    deptoSheet.Cells[1, 1].Style.Font.Bold = true;
                    deptoSheet.Cells[1, 1, 1, 4].Merge = true;

                    var deptoHeaders = new string[] { "Departamento", "Cantidad", "Activos", "Porcentaje" };
                    for (int i = 0; i < deptoHeaders.Length; i++)
                    {
                        var cell = deptoSheet.Cells[3, i + 1];
                        cell.Value = deptoHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 99, 132));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var deptoStats = users
                        .GroupBy(u => u.Departamento ?? "Sin Departamento")
                        .Select(g => new
                        {
                            Departamento = g.Key,
                            Cantidad = g.Count(),
                            Activos = g.Count(u => u.IsActive),
                            Porcentaje = (double)g.Count() / users.Count
                        })
                        .OrderByDescending(s => s.Cantidad)
                        .ToList();

                    int deptoRow = 4;
                    foreach (var stat in deptoStats)
                    {
                        deptoSheet.Cells[deptoRow, 1].Value = stat.Departamento;
                        deptoSheet.Cells[deptoRow, 2].Value = stat.Cantidad;
                        deptoSheet.Cells[deptoRow, 3].Value = stat.Activos;
                        deptoSheet.Cells[deptoRow, 4].Value = stat.Porcentaje;
                        deptoRow++;
                    }

                    if (deptoStats.Any())
                    {
                        var lastDeptoRow = deptoRow - 1;
                        deptoSheet.Cells[4, 4, lastDeptoRow, 4].Style.Numberformat.Format = "0.00%";
                        
                        var deptoRange = deptoSheet.Cells[3, 1, lastDeptoRow, 4];
                        var deptoTable = deptoSheet.Tables.Add(deptoRange, "ResumenDepartamentos");
                        deptoTable.TableStyle = TableStyles.Medium5;
                    }

                    deptoSheet.Cells.AutoFitColumns();

                    // HOJA 4: Estadísticas Generales
                    var statsSheet = package.Workbook.Worksheets.Add("Estadísticas");
                    
                    statsSheet.Cells[1, 1].Value = "ESTADÍSTICAS GENERALES DE USUARIOS";
                    statsSheet.Cells[1, 1].Style.Font.Size = 16;
                    statsSheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    var totalUsuarios = users.Count;
                    var usuariosActivos = users.Count(u => u.IsActive);
                    var usuariosInactivos = users.Count(u => !u.IsActive);
                    var totalAdmins = admins.Count;
                    var usuariosRegulares = totalUsuarios - totalAdmins;
                    var ciudadesUnicas = users.Select(u => u.Ciudad).Distinct().Count();
                    var departamentosUnicos = users.Select(u => u.Departamento).Distinct().Count();
                    var usuariosConTelefono = users.Count(u => !string.IsNullOrEmpty(u.PhoneNumber));
                    var registrosUltimos30Dias = users.Count(u => u.FechaRegistro >= DateTime.UtcNow.AddDays(-30));
                    var promedioRegistrosPorMes = users.GroupBy(u => new { u.FechaRegistro.Year, u.FechaRegistro.Month })
                                                      .Average(g => g.Count());
                    
                    statsSheet.Cells[3, 1].Value = "Total de Usuarios:"; 
                    statsSheet.Cells[3, 2].Value = totalUsuarios;
                    statsSheet.Cells[4, 1].Value = "Usuarios Activos:"; 
                    statsSheet.Cells[4, 2].Value = usuariosActivos;
                    statsSheet.Cells[5, 1].Value = "Usuarios Inactivos:"; 
                    statsSheet.Cells[5, 2].Value = usuariosInactivos;
                    statsSheet.Cells[6, 1].Value = "Total Administradores:"; 
                    statsSheet.Cells[6, 2].Value = totalAdmins;
                    statsSheet.Cells[7, 1].Value = "Usuarios Regulares:"; 
                    statsSheet.Cells[7, 2].Value = usuariosRegulares;
                    statsSheet.Cells[8, 1].Value = "Ciudades Representadas:"; 
                    statsSheet.Cells[8, 2].Value = ciudadesUnicas;
                    statsSheet.Cells[9, 1].Value = "Departamentos Representados:"; 
                    statsSheet.Cells[9, 2].Value = departamentosUnicos;
                    statsSheet.Cells[10, 1].Value = "Usuarios con Teléfono:"; 
                    statsSheet.Cells[10, 2].Value = usuariosConTelefono;
                    statsSheet.Cells[11, 1].Value = "Registros Últimos 30 días:"; 
                    statsSheet.Cells[11, 2].Value = registrosUltimos30Dias;
                    statsSheet.Cells[12, 1].Value = "Promedio Registros/Mes:"; 
                    statsSheet.Cells[12, 2].Value = Math.Round(promedioRegistrosPorMes, 1);
                    
                    statsSheet.Cells[3, 1, 12, 1].Style.Font.Bold = true;
                    statsSheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El análisis de usuarios está vacío");
                    }

                    _logger.LogInformation("Análisis de usuarios generado: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando análisis de usuarios");
                    return GenerateUsersCsv(users, admins);
                }
            });
        }

        // ?? NUEVO: EXPORTAR TRANSACCIONES A EXCEL
        public async Task<byte[]> ExportTransactionsToExcelAsync(List<TransaccionEntity> transactions)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Iniciando generación de Excel con {Count} transacciones", transactions.Count);
                    
                    using var package = new ExcelPackage();
                    
                    var worksheet = package.Workbook.Worksheets.Add("Transacciones");
                    
                    // Headers para transacciones
                    var headers = new string[]
                    {
                        "ID", "Transaction ID", "Pedido ID", "Estado", "Método de Pago", 
                        "Monto", "Moneda", "Reference Code", "Fecha Transacción", 
                        "Response Message", "Trazability Code", "Authorization Code", "Parámetros Extra"
                    };

                    // Escribir headers
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cells[1, i + 1];
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 193, 7));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // Escribir datos de transacciones
                    int row = 2;
                    foreach (var transaction in transactions)
                    {
                        try
                        {
                            worksheet.Cells[row, 1].Value = transaction.Id;
                            worksheet.Cells[row, 2].Value = CleanString(transaction.TransactionId);
                            worksheet.Cells[row, 3].Value = transaction.PedidoId?.ToString() ?? "";
                            worksheet.Cells[row, 4].Value = CleanString(GetEstadoTransaccionDisplay(transaction.Estado));
                            worksheet.Cells[row, 5].Value = CleanString(GetMetodoPagoDisplay(transaction.MetodoPago));
                            worksheet.Cells[row, 6].Value = (double)transaction.Monto;
                            worksheet.Cells[row, 7].Value = CleanString(transaction.Moneda);
                            worksheet.Cells[row, 8].Value = CleanString(transaction.ReferenceCode);
                            worksheet.Cells[row, 9].Value = transaction.FechaTransaccion;
                            worksheet.Cells[row, 10].Value = CleanString(transaction.ResponseMessage);
                            worksheet.Cells[row, 11].Value = CleanString(transaction.TrazabilityCode);
                            worksheet.Cells[row, 12].Value = CleanString(transaction.AuthorizationCode);
                            worksheet.Cells[row, 13].Value = CleanString(transaction.ExtraParametersJson);

                            row++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error procesando transacción: {TransactionId}", transaction.TransactionId);
                        }
                    }

                    // Formatear columnas si hay datos
                    if (transactions.Any())
                    {
                        var lastRow = row - 1;
                        
                        // Formatear montos
                        worksheet.Cells[2, 6, lastRow, 6].Style.Numberformat.Format = "$#,##0.00";
                        
                        // Formatear fechas
                        worksheet.Cells[2, 9, lastRow, 9].Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";
                    }

                    // Auto-ajustar columnas
                    worksheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El archivo Excel de transacciones está vacío");
                    }

                    _logger.LogInformation("Excel de transacciones generado exitosamente: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico generando Excel de transacciones");
                    return GenerateTransactionsCsv(transactions);
                }
            });
        }

        // ?? NUEVO: ANÁLISIS DE TRANSACCIONES CON RENTABILIDAD
        public async Task<byte[]> ExportTransactionsAnalysisAsync(List<TransaccionEntity> transactions, decimal totalVentas, decimal costoReposicion, decimal gananciaNeta, double margenPorcentaje)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInformation("Generando análisis de transacciones - {Count} transacciones", transactions.Count);
                    
                    using var package = new ExcelPackage();
                    
                    // HOJA 1: Datos fuente
                    var dataSheet = package.Workbook.Worksheets.Add("Datos Fuente");
                    
                    var sourceHeaders = new string[] { 
                        "Estado", "MetodoPago", "Monto", "Moneda", 
                        "FechaTransaccion", "Mes", "Año", "Trimestre", "DiaSemana"
                    };
                    
                    for (int i = 0; i < sourceHeaders.Length; i++)
                    {
                        var cell = dataSheet.Cells[1, i + 1];
                        cell.Value = sourceHeaders[i];
                        cell.Style.Font.Bold = true;
                    }

                    int dataRow = 2;
                    foreach (var transaction in transactions)
                    {
                        dataSheet.Cells[dataRow, 1].Value = CleanString(GetEstadoTransaccionDisplay(transaction.Estado));
                        dataSheet.Cells[dataRow, 2].Value = CleanString(GetMetodoPagoDisplay(transaction.MetodoPago));
                        dataSheet.Cells[dataRow, 3].Value = (double)transaction.Monto;
                        dataSheet.Cells[dataRow, 4].Value = CleanString(transaction.Moneda);
                        dataSheet.Cells[dataRow, 5].Value = transaction.FechaTransaccion;
                        dataSheet.Cells[dataRow, 6].Value = transaction.FechaTransaccion.ToString("MMMM yyyy");
                        dataSheet.Cells[dataRow, 7].Value = transaction.FechaTransaccion.Year;
                        dataSheet.Cells[dataRow, 8].Value = $"Q{((transaction.FechaTransaccion.Month - 1) / 3) + 1} {transaction.FechaTransaccion.Year}";
                        dataSheet.Cells[dataRow, 9].Value = transaction.FechaTransaccion.ToString("dddd");
                        dataRow++;
                    }

                    var dataRange = dataSheet.Cells[1, 1, dataRow - 1, sourceHeaders.Length];
                    var dataTable = dataSheet.Tables.Add(dataRange, "TablaTransacciones");
                    dataTable.TableStyle = TableStyles.Medium9;
                    dataSheet.Cells.AutoFitColumns();

                    // HOJA 2: Análisis de Rentabilidad
                    var profitabilitySheet = package.Workbook.Worksheets.Add("Análisis de Rentabilidad");
                    
                    profitabilitySheet.Cells[1, 1].Value = "ANÁLISIS DE RENTABILIDAD Y FINANCIERO";
                    profitabilitySheet.Cells[1, 1].Style.Font.Size = 18;
                    profitabilitySheet.Cells[1, 1].Style.Font.Bold = true;
                    profitabilitySheet.Cells[1, 1, 1, 4].Merge = true;
                    
                    // KPIs principales
                    profitabilitySheet.Cells[3, 1].Value = "MÉTRICAS PRINCIPALES";
                    profitabilitySheet.Cells[3, 1].Style.Font.Size = 14;
                    profitabilitySheet.Cells[3, 1].Style.Font.Bold = true;
                    profitabilitySheet.Cells[3, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    profitabilitySheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(40, 167, 69));
                    profitabilitySheet.Cells[3, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    profitabilitySheet.Cells[3, 1, 3, 4].Merge = true;
                    
                    profitabilitySheet.Cells[5, 1].Value = "Total de Ingresos:";
                    profitabilitySheet.Cells[5, 2].Value = (double)totalVentas;
                    profitabilitySheet.Cells[5, 2].Style.Numberformat.Format = "$#,##0.00";
                    profitabilitySheet.Cells[5, 2].Style.Font.Bold = true;
                    profitabilitySheet.Cells[5, 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(40, 167, 69));
                    
                    profitabilitySheet.Cells[6, 1].Value = "Costo de Reposición:";
                    profitabilitySheet.Cells[6, 2].Value = (double)costoReposicion;
                    profitabilitySheet.Cells[6, 2].Style.Numberformat.Format = "$#,##0.00";
                    profitabilitySheet.Cells[6, 2].Style.Font.Bold = true;
                    profitabilitySheet.Cells[6, 2].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(255, 193, 7));
                    
                    profitabilitySheet.Cells[7, 1].Value = "Ganancia Neta:";
                    profitabilitySheet.Cells[7, 2].Value = (double)gananciaNeta;
                    profitabilitySheet.Cells[7, 2].Style.Numberformat.Format = "$#,##0.00";
                    profitabilitySheet.Cells[7, 2].Style.Font.Bold = true;
                    profitabilitySheet.Cells[7, 2].Style.Font.Color.SetColor(gananciaNeta >= 0 ? System.Drawing.Color.FromArgb(40, 167, 69) : System.Drawing.Color.FromArgb(220, 53, 69));
                    
                    profitabilitySheet.Cells[8, 1].Value = "Margen de Ganancia:";
                    profitabilitySheet.Cells[8, 2].Value = margenPorcentaje / 100;
                    profitabilitySheet.Cells[8, 2].Style.Numberformat.Format = "0.00%";
                    profitabilitySheet.Cells[8, 2].Style.Font.Bold = true;
                    profitabilitySheet.Cells[8, 2].Style.Font.Color.SetColor(margenPorcentaje >= 20 ? System.Drawing.Color.FromArgb(40, 167, 69) : 
                                                                             margenPorcentaje >= 10 ? System.Drawing.Color.FromArgb(255, 193, 7) : 
                                                                             System.Drawing.Color.FromArgb(220, 53, 69));

                    // Formatear métricas principales
                    profitabilitySheet.Cells[5, 1, 8, 1].Style.Font.Bold = true;

                    // HOJA 3: Resumen por Método de Pago
                    var paymentSheet = package.Workbook.Worksheets.Add("Por Método de Pago");
                    
                    paymentSheet.Cells[1, 1].Value = "ANÁLISIS POR MÉTODO DE PAGO";
                    paymentSheet.Cells[1, 1].Style.Font.Size = 16;
                    paymentSheet.Cells[1, 1].Style.Font.Bold = true;
                    paymentSheet.Cells[1, 1, 1, 5].Merge = true;

                    var paymentHeaders = new string[] { "Método de Pago", "Cantidad", "Monto Total", "Monto Promedio", "Porcentaje" };
                    for (int i = 0; i < paymentHeaders.Length; i++)
                    {
                        var cell = paymentSheet.Cells[3, i + 1];
                        cell.Value = paymentHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 123, 255));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var paymentStats = transactions
                        .GroupBy(t => GetMetodoPagoDisplay(t.MetodoPago))
                        .Select(g => new
                        {
                            Metodo = g.Key,
                            Cantidad = g.Count(),
                            MontoTotal = g.Sum(t => t.Monto),
                            MontoPromedio = g.Average(t => t.Monto),
                            Porcentaje = (double)g.Sum(t => t.Monto) / (double)transactions.Sum(t => t.Monto)
                        })
                        .OrderByDescending(s => s.MontoTotal)
                        .ToList();

                    int paymentRow = 4;
                    foreach (var stat in paymentStats)
                    {
                        paymentSheet.Cells[paymentRow, 1].Value = stat.Metodo;
                        paymentSheet.Cells[paymentRow, 2].Value = stat.Cantidad;
                        paymentSheet.Cells[paymentRow, 3].Value = (double)stat.MontoTotal;
                        paymentSheet.Cells[paymentRow, 4].Value = (double)stat.MontoPromedio;
                        paymentSheet.Cells[paymentRow, 5].Value = stat.Porcentaje;
                        paymentRow++;
                    }

                    if (paymentStats.Any())
                    {
                        var lastPaymentRow = paymentRow - 1;
                        paymentSheet.Cells[4, 3, lastPaymentRow, 4].Style.Numberformat.Format = "$#,##0.00";
                        paymentSheet.Cells[4, 5, lastPaymentRow, 5].Style.Numberformat.Format = "0.00%";
                        
                        var paymentRange = paymentSheet.Cells[3, 1, lastPaymentRow, 5];
                        var paymentTable = paymentSheet.Tables.Add(paymentRange, "ResumenMetodosPago");
                        paymentTable.TableStyle = TableStyles.Medium6;
                    }

                    // HOJA 4: Resumen por Estado
                    var statusSheet = package.Workbook.Worksheets.Add("Por Estado");
                    
                    statusSheet.Cells[1, 1].Value = "ANÁLISIS POR ESTADO DE TRANSACCIÓN";
                    statusSheet.Cells[1, 1].Style.Font.Size = 16;
                    statusSheet.Cells[1, 1].Style.Font.Bold = true;
                    statusSheet.Cells[1, 1, 1, 5].Merge = true;

                    var statusHeaders = new string[] { "Estado", "Cantidad", "Monto Total", "Monto Promedio", "Porcentaje" };
                    for (int i = 0; i < statusHeaders.Length; i++)
                    {
                        var cell = statusSheet.Cells[3, i + 1];
                        cell.Value = statusHeaders[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(220, 53, 69));
                        cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    var statusStats = transactions
                        .GroupBy(t => GetEstadoTransaccionDisplay(t.Estado))
                        .Select(g => new
                        {
                            Estado = g.Key,
                            Cantidad = g.Count(),
                            MontoTotal = g.Sum(t => t.Monto),
                            MontoPromedio = g.Average(t => t.Monto),
                            Porcentaje = (double)g.Count() / transactions.Count
                        })
                        .OrderByDescending(s => s.Cantidad)
                        .ToList();

                    int statusRow = 4;
                    foreach (var stat in statusStats)
                    {
                        statusSheet.Cells[statusRow, 1].Value = stat.Estado;
                        statusSheet.Cells[statusRow, 2].Value = stat.Cantidad;
                        statusSheet.Cells[statusRow, 3].Value = (double)stat.MontoTotal;
                        statusSheet.Cells[statusRow, 4].Value = (double)stat.MontoPromedio;
                        statusSheet.Cells[statusRow, 5].Value = stat.Porcentaje;
                        statusRow++;
                    }

                    if (statusStats.Any())
                    {
                        var lastStatusRow = statusRow - 1;
                        statusSheet.Cells[4, 3, lastStatusRow, 4].Style.Numberformat.Format = "$#,##0.00";
                        statusSheet.Cells[4, 5, lastStatusRow, 5].Style.Numberformat.Format = "0.00%";
                        
                        var statusRange = statusSheet.Cells[3, 1, lastStatusRow, 5];
                        var statusTable = statusSheet.Tables.Add(statusRange, "ResumenEstados");
                        statusTable.TableStyle = TableStyles.Medium8;
                    }

                    // HOJA 5: Estadísticas Temporales
                    var timeSheet = package.Workbook.Worksheets.Add("Estadísticas Temporales");
                    
                    timeSheet.Cells[1, 1].Value = "ESTADÍSTICAS TEMPORALES";
                    timeSheet.Cells[1, 1].Style.Font.Size = 16;
                    timeSheet.Cells[1, 1].Style.Font.Bold = true;
                    
                    var totalTransacciones = transactions.Count;
                    var transaccionesAprobadas = transactions.Count(t => t.Estado == "APPROVED");
                    var transaccionesPendientes = transactions.Count(t => t.Estado == "PENDING");
                    var transaccionesRechazadas = transactions.Count(t => t.Estado == "DECLINED" || t.Estado == "REJECTED");
                    var transaccionesHoy = transactions.Count(t => t.FechaTransaccion.Date == DateTime.Today);
                    var transaccionesUltimos7Dias = transactions.Count(t => t.FechaTransaccion >= DateTime.Now.AddDays(-7));
                    var transaccionesUltimos30Dias = transactions.Count(t => t.FechaTransaccion >= DateTime.Now.AddDays(-30));
                    var montoPromedioPorTransaccion = transactions.Average(t => t.Monto);
                    var ventasPorHora = transactions.GroupBy(t => t.FechaTransaccion.Hour).Average(g => g.Count());
                    
                    timeSheet.Cells[3, 1].Value = "Total de Transacciones:"; 
                    timeSheet.Cells[3, 2].Value = totalTransacciones;
                    timeSheet.Cells[4, 1].Value = "Transacciones Aprobadas:"; 
                    timeSheet.Cells[4, 2].Value = transaccionesAprobadas;
                    timeSheet.Cells[5, 1].Value = "Transacciones Pendientes:"; 
                    timeSheet.Cells[5, 2].Value = transaccionesPendientes;
                    timeSheet.Cells[6, 1].Value = "Transacciones Rechazadas:"; 
                    timeSheet.Cells[6, 2].Value = transaccionesRechazadas;
                    timeSheet.Cells[7, 1].Value = "Transacciones Hoy:"; 
                    timeSheet.Cells[7, 2].Value = transaccionesHoy;
                    timeSheet.Cells[8, 1].Value = "Transacciones Últimos 7 días:"; 
                    timeSheet.Cells[8, 2].Value = transaccionesUltimos7Dias;
                    timeSheet.Cells[9, 1].Value = "Transacciones Últimos 30 días:"; 
                    timeSheet.Cells[9, 2].Value = transaccionesUltimos30Dias;
                    timeSheet.Cells[10, 1].Value = "Monto Promedio por Transacción:"; 
                    timeSheet.Cells[10, 2].Value = (double)montoPromedioPorTransaccion;
                    timeSheet.Cells[10, 2].Style.Numberformat.Format = "$#,##0.00";
                    timeSheet.Cells[11, 1].Value = "Promedio Transacciones/Hora:"; 
                    timeSheet.Cells[11, 2].Value = Math.Round(ventasPorHora, 2);
                    
                    timeSheet.Cells[3, 1, 11, 1].Style.Font.Bold = true;
                    timeSheet.Cells.AutoFitColumns();

                    // Auto-ajustar todas las hojas
                    paymentSheet.Cells.AutoFitColumns();
                    statusSheet.Cells.AutoFitColumns();
                    profitabilitySheet.Cells.AutoFitColumns();

                    var bytes = package.GetAsByteArray();
                    
                    if (bytes == null || bytes.Length == 0)
                    {
                        throw new InvalidOperationException("El análisis de transacciones está vacío");
                    }

                    _logger.LogInformation("Análisis de transacciones generado: {FileSize} bytes", bytes.Length);
                    return bytes;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando análisis de transacciones");
                    return GenerateTransactionsCsv(transactions);
                }
            });
        }

        private string GetEstadoTransaccionDisplay(string estado)
        {
            return estado switch
            {
                "APPROVED" => "Aprobado",
                "DECLINED" => "Rechazado",
                "REJECTED" => "Rechazado",
                "PENDING" => "Pendiente",
                "ERROR" => "Error",
                "EXPIRED" => "Expirado",
                _ => estado ?? "Desconocido"
            };
        }

        private string GetMetodoPagoDisplay(string metodoPago)
        {
            return metodoPago switch
            {
                "PSE" => "PSE",
                "CREDIT_CARD" => "Tarjeta de Crédito",
                "DEBIT_CARD" => "Tarjeta Débito",
                "EFECTY" => "Efecty",
                "NEQUI" => "Nequi",
                "BALOTO" => "Baloto",
                _ => metodoPago ?? "Desconocido"
            };
        }

        private byte[] GenerateTransactionsCsv(List<TransaccionEntity> transactions)
        {
            try
            {
                _logger.LogInformation("Generando CSV de transacciones con {Count} registros", transactions.Count);
                
                var csv = new System.Text.StringBuilder();
                
                csv.AppendLine("TransactionID;Estado;MetodoPago;Monto;Moneda;FechaTransaccion;ReferenceCode");

                foreach (var transaction in transactions)
                {
                    csv.AppendLine($"{CleanString(transaction.TransactionId)};" +
                                  $"{CleanString(GetEstadoTransaccionDisplay(transaction.Estado))};" +
                                  $"{CleanString(GetMetodoPagoDisplay(transaction.MetodoPago))};" +
                                  $"{transaction.Monto:F2};" +
                                  $"{CleanString(transaction.Moneda)};" +
                                  $"{transaction.FechaTransaccion:yyyy-MM-dd HH:mm:ss};" +
                                  $"{CleanString(transaction.ReferenceCode)}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                _logger.LogInformation("CSV de transacciones generado: {FileSize} bytes", bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV de transacciones");
                var errorCsv = "Error;No se pudieron exportar las transacciones\n";
                return System.Text.Encoding.UTF8.GetBytes(errorCsv);
            }
        }

        private string GetEstadoDisplay(string estado)
        {
            return estado switch
            {
                "APPROVED" => "Aprobado",
                "DECLINED" => "Rechazado",
                "REJECTED" => "Rechazado",
                "PENDING" => "Pendiente",
                "ERROR" => "Error",
                "EXPIRED" => "Expirado",
                _ => estado ?? "Desconocido"
            };
        }

        private string CleanString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            return input.Trim()
                       .Replace("\r\n", " ")
                       .Replace("\n", " ")
                       .Replace("\r", " ")
                       .Replace("\t", " ")
                       .Replace("\"", "'");
        }

        private byte[] GenerateSimpleCsv(List<ProductEntity> products)
        {
            try
            {
                _logger.LogInformation("Generando CSV con {Count} productos", products.Count);
                
                var csv = new System.Text.StringBuilder();
                
                csv.AppendLine("ID;SKU;Nombre;Marca;Categoría;Precio;Stock;Estado");

                foreach (var product in products)
                {
                    csv.AppendLine($"{product.Id};" +
                                  $"{CleanString(product.SKU)};" +
                                  $"{CleanString(product.Name)};" +
                                  $"{CleanString(product.Brand)};" +
                                  $"{CleanString(product.Category)};" +
                                  $"{product.Price:F2};" +
                                  $"{product.Stock};" +
                                  $"{(product.IsActive ? "ACTIVO" : "OCULTO")}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                _logger.LogInformation("CSV generado: {FileSize} bytes", bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV");
                var errorCsv = "Error;No se pudieron exportar los datos\n";
                return System.Text.Encoding.UTF8.GetBytes(errorCsv);
            }
        }

        private byte[] GenerateShipmentsCsv(List<EnvioInfo> shipments)
        {
            try
            {
                _logger.LogInformation("Generando CSV de envíos con {Count} registros", shipments.Count);
                
                var csv = new System.Text.StringBuilder();
                
                csv.AppendLine("NumeroGuia;OrderID;TransactionID;Estado;Producto;PrecioTotal;FechaCreacion;DireccionEnvio;Transportadora");

                foreach (var shipment in shipments)
                {
                    csv.AppendLine($"{CleanString(shipment.NumeroGuia)};" +
                                  $"{shipment.OrderId};" +
                                  $"{CleanString(shipment.TransactionId)};" +
                                  $"{CleanString(shipment.EstadoDisplay ?? shipment.Estado)};" +
                                  $"{CleanString(shipment.NombreProducto)};" +
                                  $"{shipment.PrecioTotal:F2};" +
                                  $"{shipment.FechaCreacion:yyyy-MM-dd};" +
                                  $"{CleanString(shipment.DireccionEnvio)};" +
                                  $"{CleanString(shipment.Transportadora)}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                _logger.LogInformation("CSV de envíos generado: {FileSize} bytes", bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV de envíos");
                var errorCsv = "Error;No se pudieron exportar los envíos\n";
                return System.Text.Encoding.UTF8.GetBytes(errorCsv);
            }
        }

        private byte[] GenerateOrdersCsv(List<TransaccionInfo> orders)
        {
            try
            {
                _logger.LogInformation("Generando CSV de pedidos con {Count} registros", orders.Count);
                
                var csv = new System.Text.StringBuilder();
                
                csv.AppendLine("TransactionID;OrderID;Estado;MetodoPago;Monto;FechaTransaccion;Usuario;Email");

                foreach (var order in orders)
                {
                    csv.AppendLine($"{CleanString(order.TransactionId)};" +
                                  $"{order.OrderId};" +
                                  $"{CleanString(GetEstadoDisplay(order.Estado))};" +
                                  $"{CleanString(order.MetodoPago)};" +
                                  $"{order.Monto:F2};" +
                                  $"{order.FechaTransaccion:yyyy-MM-dd HH:mm:ss};" +
                                  $"{CleanString(order.UserName)};" +
                                  $"{CleanString(order.UserEmail)}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                _logger.LogInformation("CSV de pedidos generado: {FileSize} bytes", bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV de pedidos");
                var errorCsv = "Error;No se pudieron exportar los pedidos\n";
                return System.Text.Encoding.UTF8.GetBytes(errorCsv);
            }
        }

        private byte[] GenerateUsersCsv(List<ApplicationUser> users, List<ApplicationUser> admins)
        {
            try
            {
                _logger.LogInformation("Generando CSV de usuarios con {Count} registros", users.Count);
                
                var csv = new System.Text.StringBuilder();
                
                csv.AppendLine("Usuario;Email;NombreCompleto;TipoDocumento;NumeroDocumento;Ciudad;Departamento;FechaRegistro;Estado;Rol");

                foreach (var user in users)
                {
                    var nombreCompleto = $"{user.PrimerNombre} {user.SegundoNombre} {user.PrimerApellido} {user.SegundoApellido}".Trim();
                    var rol = admins.Any(a => a.Id == user.Id) ? "Administrador" : "Usuario";
                    
                    csv.AppendLine($"{CleanString(user.UserName)};" +
                                  $"{CleanString(user.Email)};" +
                                  $"{CleanString(nombreCompleto)};" +
                                  $"{CleanString(user.TipoDocumento)};" +
                                  $"{CleanString(user.NumeroDocumento)};" +
                                  $"{CleanString(user.Ciudad)};" +
                                  $"{CleanString(user.Departamento)};" +
                                  $"{user.FechaRegistro:yyyy-MM-dd HH:mm:ss};" +
                                  $"{(user.IsActive ? "Activo" : "Inactivo")};" +
                                  $"{rol}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                _logger.LogInformation("CSV de usuarios generado: {FileSize} bytes", bytes.Length);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV de usuarios");
                var errorCsv = "Error;No se pudieron exportar los usuarios\n";
                return System.Text.Encoding.UTF8.GetBytes(errorCsv);
            }
        }
    }
}