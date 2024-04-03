using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.Shopify
{
    public class ShopifyData
    {
        public long id { get; set; }
        public string admin_graphql_api_id { get; set; }
        public object app_id { get; set; }
        public object browser_ip { get; set; }
        public bool buyer_accepts_marketing { get; set; }
        public string cancel_reason { get; set; }
        public DateTime? cancelled_at { get; set; }
        public object cart_token { get; set; }
        public object checkout_id { get; set; }
        public object checkout_token { get; set; }
        public object client_details { get; set; }
        public object closed_at { get; set; }
        public object confirmation_number { get; set; }
        public bool confirmed { get; set; }
        public string contact_email { get; set; }
        public DateTime? created_at { get; set; }
        public string currency { get; set; }
        public string current_subtotal_price { get; set; }
        public CurrentSubtotalPriceSet current_subtotal_price_set { get; set; }
        public object current_total_additional_fees_set { get; set; }
        public string current_total_discounts { get; set; }
        public CurrentTotalDiscountsSet current_total_discounts_set { get; set; }
        public object current_total_duties_set { get; set; }
        public string current_total_price { get; set; }
        public CurrentTotalPriceSet current_total_price_set { get; set; }
        public string current_total_tax { get; set; }
        public CurrentTotalTaxSet current_total_tax_set { get; set; }
        public string customer_locale { get; set; }
        public object device_id { get; set; }
        public List<object> discount_codes { get; set; }
        public string email { get; set; }
        public bool estimated_taxes { get; set; }
        public string financial_status { get; set; }
        public string fulfillment_status { get; set; }
        public object landing_site { get; set; }
        public object landing_site_ref { get; set; }
        public object location_id { get; set; }
        public object merchant_of_record_app_id { get; set; }
        public string name { get; set; }
        public object note { get; set; }
        public List<object> note_attributes { get; set; }
        public int number { get; set; }
        public int order_number { get; set; }
        public string order_status_url { get; set; }
        public object original_total_additional_fees_set { get; set; }
        public object original_total_duties_set { get; set; }
        public List<string> payment_gateway_names { get; set; }
        public object phone { get; set; }
        public object po_number { get; set; }
        public string presentment_currency { get; set; }
        public object processed_at { get; set; }
        public object reference { get; set; }
        public object referring_site { get; set; }
        public object source_identifier { get; set; }
        public string source_name { get; set; }
        public object source_url { get; set; }
        public string subtotal_price { get; set; }
        public SubtotalPriceSet subtotal_price_set { get; set; }
        public string tags { get; set; }
        public bool tax_exempt { get; set; }
        public List<object> tax_lines { get; set; }
        public bool taxes_included { get; set; }
        public bool test { get; set; }
        public string token { get; set; }
        public string total_discounts { get; set; }
        public TotalDiscountsSet total_discounts_set { get; set; }
        public string total_line_items_price { get; set; }
        public TotalLineItemsPriceSet total_line_items_price_set { get; set; }
        public string total_outstanding { get; set; }
        public string total_price { get; set; }
        public TotalPriceSet total_price_set { get; set; }
        public TotalShippingPriceSet total_shipping_price_set { get; set; }
        public string total_tax { get; set; }
        public TotalTaxSet total_tax_set { get; set; }
        public string total_tip_received { get; set; }
        public int total_weight { get; set; }
        public DateTime? updated_at { get; set; }
        public object user_id { get; set; }
        public BillingAddress billing_address { get; set; }
        public Customer customer { get; set; }
        public List<object> discount_applications { get; set; }
        public List<object> fulfillments { get; set; }
        public List<LineItem> line_items { get; set; }
        public object payment_terms { get; set; }
        public List<object> refunds { get; set; }
        public ShippingAddress shipping_address { get; set; }
        public List<ShippingLine> shipping_lines { get; set; }
    }

    public class AttributedStaff
    {
        public string id { get; set; }
        public int quantity { get; set; }
    }

    public class BillingAddress
    {
        public string first_name { get; set; }
        public string address1 { get; set; }
        public string phone { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string last_name { get; set; }
        public object address2 { get; set; }
        public string company { get; set; }
        public object latitude { get; set; }
        public object longitude { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
        public string province_code { get; set; }
    }

    public class CurrentSubtotalPriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class CurrentTotalDiscountsSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class CurrentTotalPriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class CurrentTotalTaxSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class Customer
    {
        public long id { get; set; }
        public string email { get; set; }
        public object created_at { get; set; }
        public object updated_at { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string state { get; set; }
        public object note { get; set; }
        public bool verified_email { get; set; }
        public object multipass_identifier { get; set; }
        public bool tax_exempt { get; set; }
        public object phone { get; set; }
        public EmailMarketingConsent email_marketing_consent { get; set; }
        public object sms_marketing_consent { get; set; }
        public string tags { get; set; }
        public string currency { get; set; }
        public List<object> tax_exemptions { get; set; }
        public string admin_graphql_api_id { get; set; }
        public DefaultAddress default_address { get; set; }
    }

    public class DefaultAddress
    {
        public long id { get; set; }
        public long customer_id { get; set; }
        public object first_name { get; set; }
        public object last_name { get; set; }
        public object company { get; set; }
        public string address1 { get; set; }
        public object address2 { get; set; }
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string phone { get; set; }
        public string name { get; set; }
        public string province_code { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public bool @default { get; set; }
    }

    public class DiscountedPriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class EmailMarketingConsent
    {
        public string state { get; set; }
        public object opt_in_level { get; set; }
        public object consent_updated_at { get; set; }
    }

    public class LineItem
    {
        public object id { get; set; }
        public string admin_graphql_api_id { get; set; }
        public List<AttributedStaff> attributed_staffs { get; set; }
        public int current_quantity { get; set; }
        public int fulfillable_quantity { get; set; }
        public string fulfillment_service { get; set; }
        public object fulfillment_status { get; set; }
        public bool gift_card { get; set; }
        public int grams { get; set; }
        public string name { get; set; }
        public string price { get; set; }
        public PriceSet price_set { get; set; }
        public bool product_exists { get; set; }
        public object product_id { get; set; }
        public List<object> properties { get; set; }
        public int quantity { get; set; }
        public bool requires_shipping { get; set; }
        public string sku { get; set; }
        public bool taxable { get; set; }
        public string title { get; set; }
        public string total_discount { get; set; }
        public TotalDiscountSet total_discount_set { get; set; }
        public object variant_id { get; set; }
        public object variant_inventory_management { get; set; }
        public object variant_title { get; set; }
        public object vendor { get; set; }
        public List<object> tax_lines { get; set; }
        public List<object> duties { get; set; }
        public List<object> discount_allocations { get; set; }
    }

    public class PresentmentMoney
    {
        public string amount { get; set; }
        public string currency_code { get; set; }
    }

    public class PriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class ShippingAddress
    {
        public string first_name { get; set; }
        public string address1 { get; set; }
        public string phone { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string last_name { get; set; }
        public object address2 { get; set; }
        public string company { get; set; }
        public object latitude { get; set; }
        public object longitude { get; set; }
        public string name { get; set; }
        public string country_code { get; set; }
        public string province_code { get; set; }
    }

    public class ShippingLine
    {
        public long id { get; set; }
        public object carrier_identifier { get; set; }
        public object code { get; set; }
        public string discounted_price { get; set; }
        public DiscountedPriceSet discounted_price_set { get; set; }
        public object phone { get; set; }
        public string price { get; set; }
        public PriceSet price_set { get; set; }
        public object requested_fulfillment_service_id { get; set; }
        public string source { get; set; }
        public string title { get; set; }
        public List<object> tax_lines { get; set; }
        public List<object> discount_allocations { get; set; }
    }

    public class ShopMoney
    {
        public string amount { get; set; }
        public string currency_code { get; set; }
    }

    public class SubtotalPriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class TotalDiscountSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class TotalDiscountsSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class TotalLineItemsPriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class TotalPriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class TotalShippingPriceSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }

    public class TotalTaxSet
    {
        public ShopMoney shop_money { get; set; }
        public PresentmentMoney presentment_money { get; set; }
    }


    public class ShopifyResponse
    {
        public bool Status { get; set; }
        public List<ShopifyActivationResponse> Activations { get; set; }
    }
    public class ShopifyActivationResponse
    {
        public string ActivationCode { get; set; }
        public string ActivationQrCode { get; set; }
        public string ActivationPDFLink { get; set; }
        public string ActivatedNumber { get; set; }
        public string ActivatedPlan { get; set; }
        public string ICCID { get; set; }
        public string ActivationDate { get; set; }
        public string LPAAndroidActivation { get; set; }
        public string iPhoneSMDP { get; set; }
        public string iOSActivationCode { get; set; }
    }
}
