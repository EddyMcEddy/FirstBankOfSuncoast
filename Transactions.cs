using System;

namespace FirstBankOfSunCoast
{

    public class Transaction
    {
        public string TransactionType { get; }
        public string AccountType { get; }
        public decimal Amount { get; }
        public DateTime Timestamp { get; }

        public Transaction(string transactionType, string accountType, decimal amount)
        {
            TransactionType = transactionType;
            AccountType = accountType;
            Amount = amount;
            Timestamp = DateTime.Now;
        }




    }





}