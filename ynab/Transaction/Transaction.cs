using System.Text.Json.Serialization;

namespace ynab.Transaction;
public class Transaction
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; }
    
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    
    [JsonPropertyName("memo")]
    public string Memo { get; set; }
    
    [JsonPropertyName("cleared")]
    public string Cleared { get; set; }
    
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }
    
    [JsonPropertyName("flag_color")]
    public string FlagColor { get; set; }
    
    [JsonPropertyName("flag_name")]
    public string FlagName { get; set; }
    
    [JsonPropertyName("account_id")]
    public int AccountId { get; set; }
    
    [JsonPropertyName("account_name")]
    public string AccountName { get; set; }
    
    [JsonPropertyName("payee_id")]
    public int PayeeId { get; set; }
    
    [JsonPropertyName("payee_name")]
    public string PayeeName { get; set; }
    
    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }
    
    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; }
    
    [JsonPropertyName("transfer_account_id")]
    public int TransferAccountId { get; set; }
    
    [JsonPropertyName("transfer_transaction_id")]
    public int TransferTransactionId { get; set; }
    
    [JsonPropertyName("matched_transaction_id")]
    public int MatchedTransactionId { get; set; }
    
    [JsonPropertyName("import_id")]
    public string ImportId { get; set; }
    
    [JsonPropertyName("import_payee_name")]
    public string ImportPayeeName { get; set; }
    
    [JsonPropertyName("import_payee_name_original")]
    public string ImportPayeeNameOriginal { get; set; }
    
    [JsonPropertyName("debt_transaction_type")]
    public string DebtTransactionType { get; set; }
    
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
    
    [JsonPropertyName("subtransactions")]
    public object[] Subtransactions { get; set; }
}