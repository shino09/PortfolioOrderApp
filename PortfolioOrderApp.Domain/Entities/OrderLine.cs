//using System;

namespace PortfolioOrderApp.Domain
{
    public enum OrderSide
    {
        Compra,
        Venta
    }

    //[Serializable] //no es necesario serializar , no se envia
    public class OrderLine
    {
        public int InstrumentId { get; set; }
        public string Ticker { get; set; } = string.Empty; // Codigo/Simbolo instrumento
        public OrderSide Side { get; set; } // Compra/Venta
        public int Quantity { get; set; }
    }
}
