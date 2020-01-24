using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using TheBankManagerAPI.Models;

namespace TheBankManagerAPI.Service
{
    public class BankService:Settings
    {
        public Transaction BankTransact(string bankData, int BankMessageCount, string dateCreated)
        {
            var transactionDetails = new Transaction();
            string newBankData = "";

            //change all letters to lower case 
            for (int i = 0; i < bankData.Length; i++)
            {
                char ch = bankData[i];
                if (char.IsLetter(ch) && char.IsUpper(ch))
                {
                    ch = char.ToLower(ch);
                }
                newBankData += ch;
            }

            Console.WriteLine(newBankData);

            //		check for relevant data in the message using the keyword
            //		search for amt, ngn or n(with a number beside), desc, debit or dr
            //		credit or cr, avail bal or bal
            //		if debit or dr or credit or cr and bal 
            //		and amt or time and acct or ac and desc is true then its bank sms
            //		else then its not a bank message
            string[] smsParam = new string[] { "ac", "cr", "dr", "by", "amt", "bal", "acct", "desc", "debit", "credit" };
            bool[] smsStatus = new bool[smsParam.Length];

        //loop through the sms param containing alll the strings we are searching for in the bank messgage 
        TheLoop:
            for (int i = 0; i < smsParam.Length; i++)
            {

            //loop through the lowercased bank meesage
            bigloop:
                for (int j = 0; j < newBankData.Length; j++)
                {
                    string compString = "";

                    //get the first character in the message
                    char ch1 = newBankData[j];

                    //change to a string
                    string st = "" + ch1;

                    //change the first character of the string in position i in the list to string eg 'amt' will result to 'a' as a string 
                    string letter = Convert.ToString(smsParam[i][0]);
                    //compare the first character to the searcher's first character
                    //if it matches then check all the characters\
                    //else move to the next word in the message


                    if (char.IsLetter(ch1) && st.Equals(letter))
                    {

                        //count/j is the position in the bigloop in which the search string matches a word in the bank message 
                        int count = j;

                        //loop through the search string exclusively since the first letters of the search string and the word in the message matches
                        for (int k = 0; k < smsParam[i].Length; k++)
                        {

                            char ch2 = 'a';

                            //protect the count index from becoming greater than the length of the bank message eg if the length of the bank message is 30 and count increments to 31 it will crash
                            // this crash signifies that we have reached the end of the bank message and there is no other string to be found so break of the big loop to move to the next search string in the smsparam list
                            try
                            {
                                //asssign the character of the message in position count
                                ch2 = newBankData[count];

                            }
                            catch (IndexOutOfRangeException)
                            {
                                //move to the next search string in the list
                                break;
                            }

                            //convert each character to string
                            string st1 = "" + ch2;
                            string let = Convert.ToString(smsParam[i][k]);

                            //compare the converted strings
                            if (char.IsLetter(ch2) && st1.Equals(let))
                            {
                                Console.WriteLine(let);
                                Console.WriteLine(st1);

                                //keep building the values of the matching string
                                //add the new matched string to the existing string eg am + t to make amt
                                compString = compString + let;

                                //increment the count for the next letter in the bank message
                                count = count + 1;

                                //keep checking if the builtup string matches the search string
                                //till it matches
                                //eg amt of compSt.. equals amt of the search string in the list
                                if (compString.Equals(smsParam[i]))
                                {
                                    Console.WriteLine("enter here");
                                    smsStatus[i] = true; //means that the search string(smsparam) at index i of the loop was found
                                    compString = ""; //empty the string
                                    break;
                                }
                            }
                            else
                            {
                                //the String does not match
                                //move to next search string
                                //System.out.println(let);
                                //System.out.println(st1);
                                Console.WriteLine("in here");
                                //empty compString
                                compString = "";
                                break; //skip building the string due to mismatch
                                       //and moves to the next letter/character on the text message
                            }

                        }

                    }
                }
            }

            foreach (bool s in smsStatus)
            {
                Console.WriteLine(s);
            }

            //next use the boolean data returned to know if the string is a bankSms
            //{"ac","cr","dr","amt","bal","acct","desc","by","debit","credit"};
            //the message to be a bank alert the following parameters must be true
            //(ac or acct = true) and (cr or credit = true) or (dr or debit = true)
            //desc = true && by = true("by" indicates description for first Bank)  
            //{"ac","cr","dr","by","amt","bal","acct","desc","debit","credit"};

            double[] amounts = new double[2];


            bool accountNo, desc, creditAlert, debitAlert;
            string transaction = "";
            accountNo = (smsStatus[0] == true || smsStatus[6] == true);
            creditAlert = (smsStatus[1] == true || smsStatus[9] == true);
            debitAlert = (smsStatus[2] == true || smsStatus[8] == true);
            desc = (smsStatus[3] == true || smsStatus[7] == true);


            //for the message to be alert it must have accountNo as true and (credit or debit) true  
            if (accountNo && smsStatus[8] == true)
            {
                debitAlert = true;
                creditAlert = false;
            }
            else if (accountNo && smsStatus[9] == true)
            {
                creditAlert = true;
                debitAlert = false;
            }
            else
            {
                creditAlert = false;
                debitAlert = false;
            }


            if (debitAlert == false && creditAlert == false)
            {
                transaction = "";
            }
            else
            {
                if (debitAlert == true)
                {
                    transaction = "Debit";
                }
                else
                {
                    transaction = "Credit";
                }

            }

            transactionDetails.transactionType = transaction;


            //Next extract relevant data if the message is a bank message
            //data like amount withrawn, available balance and description
            if (accountNo && creditAlert && !debitAlert && desc)
            {
                //System.out.println("Credit BankAlert!!!");
                amounts = relevantData(newBankData, smsStatus, BankMessageCount);
            }
            else if (accountNo && !creditAlert && debitAlert && desc)
            {
                //System.out.println("Debit BankAlert!!!");
                amounts = relevantData(newBankData, smsStatus, BankMessageCount);

            }
            else
            {
                amounts = null;
                //System.out.println("Not BankAlert!!!");
            }

            if (amounts == null)
            {
                transactionDetails.amounts = 0;
                transactionDetails.balance = 0;
                transactionDetails.transactionDate = dateCreated;
                transactionDetails.descrption = newBankData;
            }
            else
            {
                transactionDetails.amounts = amounts[0];
                transactionDetails.balance = amounts[1];
                transactionDetails.transactionDate = dateCreated;
                transactionDetails.descrption = newBankData;
            }

            return transactionDetails;
        }


        public double[] relevantData(string bankMsg, bool[] smsStatus, int BankMessageCount)
        {
            //Relevant Data (Description,Amount and Balance)
            //if you find string amt or bal 
            //then start looking for numbers if its not a number discard 
            //till stop seeing numbers
            //for desc, find keyword desc and pick every text until you reach a space
            //or the next search keyword

            //Keywords to search for
            double amount = 0.0;
            double bal = 0.0;
            string[] keyWords = new string[2];
            double[] bankVal = new double[2];

            if (smsStatus[3] == true && smsStatus[7] == false)
            {
                //if true it means the bank selected is first bank due to absense of balance
                keyWords[0] = "ngn";
                keyWords[1] = "acct";
            }
            else
            {
                //for other banks
                keyWords[0] = "amt";
                keyWords[1] = "bal";
            }

            for (int i = 0; i < keyWords.Length; i++)
            {
                for (int j = 0; j < bankMsg.Length; j++)
                {
                    string compString = "";
                    char ch1 = bankMsg[j];
                    string st = "" + ch1;
                    string letter = Convert.ToString(keyWords[i][0]);
                    //compare character to the searcher's first character
                    if (char.IsLetter(ch1) && st.Equals(letter))
                    {
                        int count = j;
                        for (int k = 0; k < keyWords[i].Length; k++)
                        {
                            char ch2 = bankMsg[count];
                            string st1 = "" + ch2;
                            string let = Convert.ToString(keyWords[i][k]);

                            if (char.IsLetter(ch2) && st1.Equals(let))
                            {
                                Console.WriteLine(let);
                                Console.WriteLine(st1);
                                compString = compString + let;
                                count = count + 1;
                                if (compString.Equals(keyWords[i]))
                                {
                                    //Start looking for the Numbers
                                    Console.WriteLine("enter here");
                                    Console.WriteLine(count);
                                    string Number = "";
                                    char lastChar = ' ';
                                    string fStop = ".";
                                    for (int p = count; p < bankMsg.Length; p++)
                                    {
                                        char Num = bankMsg[p];
                                        string NewChar = "" + Num;
                                        try
                                        {
                                            int newNum = int.Parse(NewChar);
                                            Number = Number + Convert.ToString(newNum);
                                            Console.WriteLine(Number);
                                        }
                                        catch (Exception)
                                        {
                                            Console.WriteLine(Num);
                                            //if the next string in the amount is a full stop then append the fullstop to the number
                                            if (NewChar.Equals(fStop, StringComparison.OrdinalIgnoreCase) && char.IsDigit(lastChar) && char.IsDigit(bankMsg[p + 1]))
                                            {
                                                Number += fStop;
                                            }
                                        }

                                        try
                                        {
                                            //if the last character is a digit and the present character is not a digit and pt1 is not a digit and number.length is greater than zero
                                            if (!char.IsDigit(lastChar) && !char.IsDigit(Num) && !char.IsDigit(bankMsg[p + 1]) && Number.Length > 0)
                                            {
                                                try
                                                {
                                                    bankVal[i] = double.Parse(Number);
                                                    Console.WriteLine("Parsed Number is " + Number + " at the " + i + "th value");



                                                }

                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e.ToString());
                                                    Console.Write(e.StackTrace);
                                                    bankVal[i] = 0.0;
                                                    Console.WriteLine("Double parse error");
                                                }
                                                Number = "";
                                                break;
                                            }
                                        }

                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.ToString());
                                            Console.Write(e.StackTrace);
                                        }

                                        lastChar = Num;
                                        if (p == (bankMsg.Length - 1))
                                        {
                                            try
                                            {
                                                bankVal[i] = double.Parse(Number);
                                                Console.WriteLine("Parsed Number is " + Number + " at the " + i + "th value");


                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e.ToString());
                                                Console.Write(e.StackTrace);
                                                bankVal[i] = 0.0;
                                                Console.WriteLine("Double parse error");
                                            }
                                            Number = "";
                                            break;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                //System.out.println(let);
                                //System.out.println(st1);
                                Console.WriteLine("in here");
                                compString = "";
                            }

                        }
                    }

                }


            }


            return bankVal;




        }

        public Dashboard GetDashboard(string username)
        {
            List<Transaction> Credits = new List<Transaction>();
            List<Transaction> Debits = new List<Transaction>();
            Dashboard dashboard = new Dashboard();
            SqlConnection sqlConnection = new SqlConnection(getConnectionSettings());
            SqlCommand sqlCommand = new SqlCommand("RetrieveAllTransactions", sqlConnection);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@username", username);
            try
            {
                sqlConnection.Open();
                DataSet dataSet = new DataSet();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataSet);

                if (dataSet.Tables.Count > 0)
                {

                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow existingRow in dataSet.Tables[0].Rows)
                        {
                            Transaction transaction = new Transaction();
                           
                            transaction.amounts= Double.Parse(existingRow["AMOUNT"].ToString());
                            transaction.BankCode = existingRow["BANKCODE"].ToString();
                            transaction.transactionDate = DateTime.Parse(existingRow["TRANSACTIONDATE"].ToString()).ToString("yyyymmdd");
                            transaction.descrption = existingRow["DESCRIPTION"].ToString();
                            transaction.transactionType = existingRow["Trans_Description"].ToString();
                            transaction.TransChannelDescription = existingRow["Trans_Channel_Description"].ToString();
                            Credits.Add(transaction);

                        }
                    }
                    if (dataSet.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow existingRow in dataSet.Tables[1].Rows)
                        {
                            Transaction transaction = new Transaction();

                            transaction.amounts = Double.Parse(existingRow["AMOUNT"].ToString());
                            transaction.BankCode = existingRow["BANKCODE"].ToString();
                            transaction.transactionDate = DateTime.Parse(existingRow["TRANSACTIONDATE"].ToString()).ToString("yyyymmdd");
                            transaction.descrption = existingRow["DESCRIPTION"].ToString();
                            transaction.transactionType = existingRow["Trans_Description"].ToString();
                            transaction.TransChannelDescription = existingRow["Trans_Channel_Description"].ToString();
                            Debits.Add(transaction);

                        }
                    }

                    dashboard.AllCredits = Credits;
                    dashboard.AllDebits = Debits;

                }


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dashboard;

        }

        public List<Transaction> saveTransactKeyValues(BankMessage bankMessage)
        {
            var Transactions = new List<Transaction>();

            for (int i = 0; i <bankMessage.BankMessageArray.Length; i++)
            {

                var result = BankTransact(bankMessage.BankMessageArray[i], i, bankMessage.BankMessageDateArray[i]);

                if (result.transactionType == "")
                {
                    continue;
                }
                else
                {
                    Transactions.Add(new Transaction
                    {
                        amounts = result.amounts,
                        balance = result.balance,
                        transactionDate = result.transactionDate,
                        transactionType = result.transactionType,
                        descrption = result.descrption

                    });
                }


            }
            return Transactions;

        }

        public bool addTransactions(List<Transaction> transactions)
        {
            int response = 2;
            List<Transaction> Transaction = transactions;
            foreach (var transaction in Transaction)
            {
                try



                {
                    using (SqlConnection con = new SqlConnection(getConnectionSettings()))
                    {
                        con.Open();
                        SqlCommand com = new SqlCommand("AddTransaction", con);
                        using (com)
                        {
                            com.Connection = con;
                            com.CommandType = CommandType.StoredProcedure;

                            com.Parameters.AddWithValue("@username", "aoyinkansola");
                            com.Parameters.AddWithValue("@bank", "GTB");
                            com.Parameters.AddWithValue("@transactionChannel", "ATM");
                            com.Parameters.AddWithValue("@amount", transaction.amounts);
                            com.Parameters.AddWithValue("@balance", transaction.amounts);
                            com.Parameters.AddWithValue("@description", transaction.descrption);
                            com.Parameters.AddWithValue("@transactionDate", DateTime.Now);
                            com.Parameters.AddWithValue("@transactionType", transaction.transactionType);
                            com.Parameters.Add(new SqlParameter("@response", SqlDbType.Int));

                            com.Parameters["@response"].Direction = ParameterDirection.Output;


                            com.ExecuteNonQuery();
                            response = Convert.ToByte(com.Parameters["@response"].Value);


                        }
                        con.Close();
                    }

                }


                catch (SqlException ex)
                {
                    throw ex;
                }

            }

            if (response == 1)
            {
                return true;

            }
           

            return false;


        }

    }
}