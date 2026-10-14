using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortfolioOrderApp.Application.Services;
using PortfolioOrderApp.Domain;
using PortfolioOrderApp.Tests.Fakes;

namespace PortfolioOrderApp.Tests
{
    public class OrderTicketServiceTests
    {
        // Catalogo de prueba compartido por los casos.
        private static readonly List<Instrument> Catalog = new()
        {
            new Instrument { Id = 1, Ticker = "AAPL", Name = "Apple Inc.", AssetType = "Accion", Price = 100m },
            new Instrument { Id = 2, Ticker = "TLT",  Name = "Treasury ETF", AssetType = "ETF",    Price = 50m }
        };

        // Arma el servicio con sus dobles de prueba y los devuelve para inspeccionarlos.
        private static (OrderTicketService service, FakeOrderTicketStore store, RecordingComplianceAudit audit) Build()
        {
            var store = new FakeOrderTicketStore();
            var audit = new RecordingComplianceAudit();
            return (new OrderTicketService(store, audit), store, audit);
        }


        // Agregar una linea debe dejarla en el borrador con su ticker.
        [Fact]
        public void AddLine_AgregaLaLineaAlBorrador()
        {
            var (service, store, _) = Build();

            service.AddLine(new OrderLine { InstrumentId = 1, Ticker = "AAPL", Side = OrderSide.Compra, Quantity = 3 });

            Assert.Single(store.GetDraft());
            Assert.Equal("AAPL", store.GetDraft()[0].Ticker);
        }

        // El total estimado debe ser la suma de cantidad x precio de cada linea.
        [Fact]
        public void GetEstimatedTotal_SumaCantidadPorPrecio()
        {
            var (service, _, _) = Build();
            service.AddLine(new OrderLine { InstrumentId = 1, Side = OrderSide.Compra, Quantity = 2 }); // 2 * 100
            service.AddLine(new OrderLine { InstrumentId = 2, Side = OrderSide.Venta, Quantity = 3 }); // 3 * 50

            var total = service.GetEstimatedTotal(Catalog);

            Assert.Equal(350m, total);
        }

        // Instrumentos que no estan en el catalogo no deben aportar al total.
        [Fact]
        public void GetEstimatedTotal_IgnoraInstrumentosDesconocidos()
        {
            var (service, _, _) = Build();
            service.AddLine(new OrderLine { InstrumentId = 999, Side = OrderSide.Compra, Quantity = 5 });

            Assert.Equal(0m, service.GetEstimatedTotal(Catalog));
        }


        // Confirmar la orden debe registrar una auditoria por cada linea.
        [Fact]
        public void ConfirmTicket_RegistraUnaAuditoriaPorLinea()
        {
            var (service, _, audit) = Build();
            service.AddLine(new OrderLine { InstrumentId = 1, Side = OrderSide.Compra, Quantity = 2 });
            service.AddLine(new OrderLine { InstrumentId = 2, Side = OrderSide.Venta, Quantity = 1 });

            service.ConfirmTicket(Catalog);

            Assert.Equal(2, audit.Entries.Count);
            Assert.Contains(("AAPL", OrderSide.Compra, 2), audit.Entries);
            Assert.Contains(("TLT", OrderSide.Venta, 1), audit.Entries);
        }


        // Si el instrumento no existe, la auditoria debe usar el ticker "N/A".
        [Fact]
        public void ConfirmTicket_UsaNA_CuandoElInstrumentoNoExiste()
        {
            var (service, _, audit) = Build();
            service.AddLine(new OrderLine { InstrumentId = 999, Side = OrderSide.Compra, Quantity = 1 });

            service.ConfirmTicket(Catalog);

            Assert.Single(audit.Entries);
            Assert.Equal("N/A", audit.Entries[0].Ticker);
        }

        // Al confirmar, el borrador debe quedar vacio.
        [Fact]
        public void ConfirmTicket_LimpiaElBorrador()
        {
            var (service, store, _) = Build();
            service.AddLine(new OrderLine { InstrumentId = 1, Side = OrderSide.Compra, Quantity = 1 });

            service.ConfirmTicket(Catalog);

            Assert.Empty(store.GetDraft());
        }

        // Confirmar sin lineas no debe registrar auditoria.
        [Fact]
        public void ConfirmTicket_SinLineas_NoRegistraAuditoria()
        {
            var (service, _, audit) = Build();

            service.ConfirmTicket(Catalog);

            Assert.Empty(audit.Entries);
        }
    }
}

