// Copyright (c) Tellma Ltd. All rights reserved.

using System.Text.Json;

namespace Tellma.Integration.MarminAe.Tests
{
    /// <summary>
    ///     Pins the wire shape of the two payloads Tellma sends.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Two things are being protected here. The first is the property names: every member
    ///         spells its own snake_case wire name because the vendor is not internally consistent,
    ///         so a typo is invisible to the compiler and shows up as a 422 at close time.
    ///     </para>
    ///     <para>
    ///         The second is null omission. The context sets
    ///         <c>DefaultIgnoreCondition = WhenWritingNull</c>, and the connector's own notes call
    ///         that load-bearing rather than cosmetic: an unset optional field must be absent from
    ///         the body, not present and null.
    ///     </para>
    /// </remarks>
    public class MarminAeSerializationTests
    {
        private static string Serialize<T>(T value, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> info) =>
            JsonSerializer.Serialize(value, info);

        [Fact]
        public void SalesInvoice_SerializesToTheExpectedBody()
        {
            string json = Serialize(TestSamples.Invoice, MarminAeJsonContext.Default.MarminAeSalesInvoiceRequest);

            Assert.Equal(
                """
                {"invoice_type_code":"380","due_date":"2026-10-03","issue_date":"2026-09-03","profile_execution_id":"01000000","document_currency_code":"AED","accounting_customer_party":{"name":"Al Noor Trading LLC","postal_address":{"street_name":"Sheikh Zayed Road","city_name":"Dubai","country_subentity":"DU","country":"United Arab Emirates","country_code":"AE"},"email":"ap@alnoor.example","endpoint_id":"100123456700003","endpoint_scheme_id":"0235","tin":"100123456700003"},"document_lines":[{"name":"Consulting","description":"Implementation consulting, October","quantity":3,"unit_code":"HUR","price":{"base_amount":500,"base_quantity":1},"classified_tax_category":{"id":"S","percent":5,"tax_scheme":"VAT"}}],"document_number":"INV-1042"}
                """,
                json);
        }

        [Fact]
        public void SalesCreditNote_SerializesToTheExpectedBody()
        {
            string json = Serialize(TestSamples.CreditNote, MarminAeJsonContext.Default.MarminAeSalesCreditNoteRequest);

            Assert.Equal(
                """
                {"credit_note_type_code":"381","discrepancy_response":"1","billing_reference":[{"id":"INV-1042","issue_date":"2026-09-03"}],"issue_date":"2026-09-10","profile_execution_id":"01000000","document_currency_code":"AED","accounting_customer_party":{"name":"Al Noor Trading LLC","postal_address":{"street_name":"Sheikh Zayed Road","city_name":"Dubai","country_subentity":"DU","country":"United Arab Emirates","country_code":"AE"},"email":"ap@alnoor.example","endpoint_id":"100123456700003","endpoint_scheme_id":"0235","tin":"100123456700003"},"document_lines":[{"name":"Consulting","description":"Implementation consulting, October","quantity":3,"unit_code":"HUR","price":{"base_amount":500,"base_quantity":1},"classified_tax_category":{"id":"S","percent":5,"tax_scheme":"VAT"}}],"document_number":"CRN-77"}
                """,
                json);
        }

        [Fact]
        public void UnsetOptionalFields_AreAbsentRatherThanNull()
        {
            string json = Serialize(TestSamples.Invoice, MarminAeJsonContext.Default.MarminAeSalesInvoiceRequest);

            // A representative sample of optional fields the fixture never sets.
            Assert.DoesNotContain("\"note\"", json);
            Assert.DoesNotContain("\"delivery\"", json);
            Assert.DoesNotContain("\"payment_means\"", json);
            Assert.DoesNotContain("\"tax_exchange_rate\"", json);
            Assert.DoesNotContain("null", json);
        }

        [Fact]
        public void DatesAreSerializedAsPlainCalendarDays()
        {
            // Not an ISO instant: the vendor wants yyyy-MM-dd with no time and no zone, which is
            // also why issue_date is sourced from PostingDate rather than the StateAt timestamp.
            string json = Serialize(TestSamples.Invoice, MarminAeJsonContext.Default.MarminAeSalesInvoiceRequest);

            Assert.Contains("\"issue_date\":\"2026-09-03\"", json);
            Assert.Contains("\"due_date\":\"2026-10-03\"", json);
        }
    }
}
