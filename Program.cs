using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration; // Import the CsvHelper.Configuration namespace for CsvConfiguration.

namespace FirstBankOfSunCoast
{
    class Program
    {
        // Helper method to prompt the user for a string input and convert it to uppercase.
        static string PromptString(string prompt)
        {
            Console.WriteLine(prompt);
            var userInput = Console.ReadLine().ToUpper();
            return userInput;
        }

        // Helper method to prompt the user for a decimal number input, handle non-numeric input.
        static decimal PromptNumber(string prompt)
        {
            Console.WriteLine(prompt);
            decimal userInput;

            var isInputGood = decimal.TryParse(Console.ReadLine(), out userInput);

            if (!isInputGood)
            {
                Console.WriteLine("The Input was not a Number. We will use {0} instead", userInput);
                return 0;
            }
            else
            {
                return userInput;
            }
        }

        // Display a welcome message.
        static void Greetings()
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("Welcome to SunCoast ATM!\nOpen 24/7");
            Console.WriteLine("");
            Console.WriteLine("_________________________________________");
        }

        static void Main(string[] args)
        {
            Greetings();

            // Initialize transaction lists for savings and checking.
            var transactions = LoadTransactionsFromCsv("transactions.csv");
            var savingsTransactions = new List<Transaction>();
            var checkingTransactions = new List<Transaction>();

            bool menukeepGoing = true;

            while (menukeepGoing)
            {
                Console.WriteLine("-------------------------------");
                Console.WriteLine("");
                Console.WriteLine("ATM Menu: \n(1) Deposit To Savings \n(2) Deposit To Checking \n(3) Withdraw From Savings \n(4) Withdraw From Checking \n(5) See Savings Balance \n(6) See Checking Balance \n(7) See Savings Transactions \n(8) See Checking Transactions \n(9) Quit");
                Console.WriteLine("");
                Console.WriteLine("_________________________________");
                var menuInput = Convert.ToInt32(Console.ReadLine().ToUpper());

                if (menuInput == 9)
                {
                    SaveTransactionsToCsv(transactions, "transactions.csv"); // Save transactions before quitting
                    menukeepGoing = false;
                }
                else if (menuInput == 1)
                {
                    DepositToSavings(transactions, savingsTransactions);
                    SaveTransactionsToCsv(transactions, "transactions.csv"); // Save transactions after deposit
                }
                else if (menuInput == 2)
                {
                    DepositToChecking(transactions, checkingTransactions);
                    SaveTransactionsToCsv(transactions, "transactions.csv"); // Save transactions after deposit
                }
                else if (menuInput == 3)
                {
                    WithdrawFromSavings(transactions, savingsTransactions);
                    SaveTransactionsToCsv(transactions, "transactions.csv"); // Save transactions after withdrawal
                }
                else if (menuInput == 4)
                {
                    WithdrawFromChecking(transactions, checkingTransactions);
                    SaveTransactionsToCsv(transactions, "transactions.csv"); // Save transactions after withdrawal
                }
                else if (menuInput == 5)
                {
                    SeeSavingsBalance(transactions);
                }
                else if (menuInput == 6)
                {
                    SeeCheckingBalance(transactions);
                }
                else if (menuInput == 7)
                {
                    SeeSavingsTransaction(transactions);
                }
                else if (menuInput == 8)
                {
                    SeeCheckingTransactions(transactions);
                }
            }
        }

        // Save transactions to a CSV file.
        private static void SaveTransactionsToCsv(List<Transaction> transactions, string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false, // Specify that the CSV file has no header
            }))
            {
                csv.WriteRecords(transactions);
            }
        }

        // Load transactions from a CSV file.
        private static List<Transaction> LoadTransactionsFromCsv(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new List<Transaction>();
            }

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false, // Specify that the CSV file has no header
            }))
            {
                csv.Read(); // Skip the header row
                return csv.GetRecords<Transaction>().ToList();
            }
        }

        // Display all checking account transactions.
        private static void SeeCheckingTransactions(List<Transaction> transactions)
        {
            Console.WriteLine("All of CHECKING Account Transactions: ");

            var currentCheckingBalance = transactions.Where(value => value.AccountType == "Checking").Sum(value => value.TransactionType == "Deposit" ? value.Amount : -value.Amount);

            Console.WriteLine($"Current Savings Balance: ${currentCheckingBalance} ");

            foreach (var transaction in transactions.Where(value => value.AccountType == "Checking"))
            {
                Console.WriteLine($"Transaction Type: {transaction.TransactionType}, Account: {transaction.AccountType} with ${transaction.Amount} Amount at {transaction.Timestamp} ");
                Console.WriteLine("___________________________________________________________________________________________________________________________________________________");
            }
        }

        // Display all savings account transactions.
        private static void SeeSavingsTransaction(List<Transaction> transactions)
        {
            Console.WriteLine("All of SAVINGS Account Transactions: ");

            // Calculate total savings balance based on transactions
            var balanceSavings = transactions
                .Where(transaction => transaction.AccountType == "Savings")
                .Sum(transaction => transaction.TransactionType == "Deposit" ? transaction.Amount : -transaction.Amount);

            // Print the total savings balance before the transactions
            Console.WriteLine($"Current Savings Balance: ${balanceSavings}");

            // Print each transaction's information
            foreach (var transaction in transactions.Where(transaction => transaction.AccountType == "Savings"))
            {
                Console.WriteLine($"Transaction Type: {transaction.TransactionType}, Account: {transaction.AccountType} with ${transaction.Amount} Amount at {transaction.Timestamp} ");
                Console.WriteLine("___________________________________________________________________________________________________________________________________________________");
            }
        }

        // Display checking account balance.
        private static void SeeCheckingBalance(List<Transaction> transactions)
        {
            Console.WriteLine("Showing the Balance from CHECKING ACCOUNT: ");

            var currentCheckingBalance = transactions.Where(value => value.AccountType == "Checking").Sum(value => value.TransactionType == "Deposit" ? value.Amount : -value.Amount);

            Console.WriteLine($"Savings Balance: ${currentCheckingBalance}");
        }

        // Display savings account balance.
        private static void SeeSavingsBalance(List<Transaction> transactions)
        {
            Console.WriteLine("Showing the Balance from SAVINGS ACCOUNT: ");
            var balanceSavings = transactions.Where(transaction => transaction.AccountType == "Savings").Sum(transaction => transaction.TransactionType == "Deposit" ? transaction.Amount : -transaction.Amount);

            Console.WriteLine($"Savings Balance: ${balanceSavings}");
        }

        // Handle withdrawing money from checking account.
        private static void WithdrawFromChecking(List<Transaction> transactions, List<Transaction> checkingTransactions)
        {
            Console.WriteLine("What is the Amount You Will WITHDRAW from CHECKING:  ");
            var withdrawAmountChecking = decimal.Parse(Console.ReadLine());
            Console.WriteLine("----------------------------------------------------------");

            if (withdrawAmountChecking <= 0)
            {
                Console.WriteLine("Amount must be Positive");
            }
            else
            {
                var oldCheckingBalance = transactions.Where(value => value.AccountType == "Checking").Sum(value => value.TransactionType == "Deposit" ? value.Amount : -value.Amount);

                if (withdrawAmountChecking > oldCheckingBalance)
                {
                    Console.WriteLine("Insufficient Funds. ");
                }
                else
                {
                    transactions.Add(new Transaction("Withdrawal", "Checking", withdrawAmountChecking));
                    checkingTransactions.Add(new Transaction("Withdrawal", "Checking", withdrawAmountChecking));

                    var newCheckingBalance = oldCheckingBalance - withdrawAmountChecking;

                    Console.WriteLine("Checking Transactions: Withdrawal From Checking");

                    Console.WriteLine($"Old Checking Balance: ${oldCheckingBalance} \nWithdrawal Amount: ${withdrawAmountChecking} \nNew Checking Balance After Withdrawal: ${newCheckingBalance}");
                }
            }
        }

        // Handle withdrawing money from savings account.
        private static void WithdrawFromSavings(List<Transaction> transactions, List<Transaction> savingsTransactions)
        {
            Console.WriteLine("What is the Amount You Will WITHDRAW from SAVINGS: ");
            var withdrawAmountSavings = decimal.Parse(Console.ReadLine());
            Console.WriteLine("----------------------------------------------------------");

            if (withdrawAmountSavings <= 0)
            {
                Console.WriteLine("Amount Must Be Positive");
            }
            else
            {
                var oldSavingsBalance = transactions
                    .Where(transaction => transaction.AccountType == "Savings")
                    .Sum(transaction => transaction.TransactionType == "Deposit" ? transaction.Amount : -transaction.Amount);

                if (withdrawAmountSavings > oldSavingsBalance)
                {
                    Console.WriteLine("Insufficient Funds.");
                }
                else
                {
                    transactions.Add(new Transaction("Withdrawal", "Savings", withdrawAmountSavings));
                    savingsTransactions.Add(new Transaction("Withdrawal", "Savings", withdrawAmountSavings));

                    var newSavingsBalance = oldSavingsBalance - withdrawAmountSavings;

                    Console.WriteLine("Savings Transactions: Withdrawal From Savings");

                    Console.WriteLine($"Old Savings Balance: ${oldSavingsBalance} \nWithdrawal Amount: ${withdrawAmountSavings} \nNew Savings Balance After Withdrawal: ${newSavingsBalance}");
                }
            }
        }

        // Handle depositing money to checking account.
        private static void DepositToChecking(List<Transaction> transactions, List<Transaction> checkingTransactions)
        {
            decimal checkingsDepositAmount = PromptNumber("What is the Amount You will DEPOSIT into CHECKING in DOLLAR(S): ");
            Console.WriteLine("____________________________________________________________________________");

            if (checkingsDepositAmount <= 0)
            {
                Console.WriteLine("Amount must be Positive.");
            }
            else
            {
                var oldDepositCheckingAmount = transactions.Where(value => value.AccountType == "Checking").Sum(value => value.TransactionType == "Deposit" ? value.Amount : -value.Amount);

                transactions.Add(new Transaction("Deposit", "Checking", checkingsDepositAmount));
                checkingTransactions.Add(new Transaction("Deposit", "Checking", checkingsDepositAmount));

                var newSavingsBalance = oldDepositCheckingAmount + checkingsDepositAmount;

                Console.WriteLine("Deposit To Checking");

                Console.WriteLine($"Old Deposit Balance: ${oldDepositCheckingAmount} \nAmount Deposited: ${checkingsDepositAmount} \nNew Deposit in Savings Amount: ${newSavingsBalance}");
            }
        }

        // Handle depositing money to savings account.
        private static void DepositToSavings(List<Transaction> transactions, List<Transaction> savingsTransactions)
        {
            decimal savingsDepositAmount = PromptNumber("What is the Amount You will DEPOSIT into SAVINGS in DOLLAR(S): ");
            Console.WriteLine("____________________________________________________________________________");
            if (savingsDepositAmount <= 0)
            {
                Console.WriteLine("Amount must be Positive.");
            }
            else
            {
                var oldSavingsBalance = transactions.Where(value => value.AccountType == "Savings").Sum(value => value.TransactionType == "Deposit" ? value.Amount : -value.Amount);

                if (oldSavingsBalance > savingsDepositAmount)
                {
                    Console.WriteLine("Insufficient Funds");
                }
                else
                {
                    transactions.Add(new Transaction("Deposit", "Savings", savingsDepositAmount));
                    savingsTransactions.Add(new Transaction("Deposit", "Savings", savingsDepositAmount));
                    var newDepositBalance = savingsDepositAmount + oldSavingsBalance;

                    Console.WriteLine("Deposit To Savings");

                    Console.WriteLine($"Old Deposit Balance: ${oldSavingsBalance} \nAmount Deposited: ${savingsDepositAmount} \nNew Deposit in Savings Amount: ${newDepositBalance}");
                }
            }
        }
    }
}
