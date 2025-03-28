namespace ynab.Transaction;
public class RootObject
{
    public string id { get; set; }
    public string date { get; set; }
    public int amount { get; set; }
    public object memo { get; set; }
    public string cleared { get; set; }
    public bool approved { get; set; }
    public object flag_color { get; set; }
    public object flag_name { get; set; }
    public string account_id { get; set; }
    public string account_name { get; set; }
    public string payee_id { get; set; }
    public string payee_name { get; set; }
    public string category_id { get; set; }
    public string category_name { get; set; }
    public object transfer_account_id { get; set; }
    public object transfer_transaction_id { get; set; }
    public object matched_transaction_id { get; set; }
    public string import_id { get; set; }
    public string import_payee_name { get; set; }
    public string import_payee_name_original { get; set; }
    public object debt_transaction_type { get; set; }
    public bool deleted { get; set; }
    public object[] subtransactions { get; set; }
}


public record Transaction(
    string Id,
    string Date,
    int Amount,
    object Memo,
    string Cleared,
    bool Approved,
    object FlagColor,
    object FlagName,
    string AccountId,
    string AccountName,
    string PayeeId,
    string PayeeName,
    string CategoryId,
    string CategoryName,
    object TransferAccountId,
    object TransferTransactionId,
    object MatchedTransactionId,
    string ImportId,
    string ImportPayeeName,
    string ImportPayeeNameOriginal,
    object DebtTransactionType,
    bool Deleted
);

