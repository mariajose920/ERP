namespace ERP.Domain.Enums;

public enum RolUsuario
{
    Administrador = 1,
    Comprador = 2,
    Vendedor = 3,
    Bodeguero = 4,
    Contador = 5
}

public enum EstadoOrdenCompra
{
    Pendiente = 1,
    RecibidaParcial = 2,
    RecibidaTotal = 3,
    Liquidada = 4,
    Anulada = 5
}

public enum EstadoFacturaCompra
{
    Borrador = 1,
    Aprobada = 2,
    Anulada = 3
}

public enum EstadoVenta
{
    Emitida = 1,
    Pagada = 2,
    ParcialmentePagada = 3,
    Anulada = 4
}

public enum TipoMovimientoKardex
{
    EntradaCompra = 1,
    SalidaVenta = 2,
    AjusteEntrada = 3,
    AjusteSalida = 4,
    Merma = 5,
    Devolucion = 6
}

public enum TipoCuentaContable
{
    Activo = 1,
    Pasivo = 2,
    Patrimonio = 3,
    Ingreso = 4,
    Gasto = 5,
    Costo = 6
}

public enum NaturalezaCuenta
{
    Deudora = 1,
    Acreedora = 2
}
