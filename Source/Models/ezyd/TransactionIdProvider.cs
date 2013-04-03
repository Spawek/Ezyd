using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace ezyd.Models.ezyd
{
    public static class TransactionIdProvider
    {
        private static UInt32 lastTransactionId = 4294967295; //max for UInt32

        public static UInt32 getTransactionId()
        {
            if (lastTransactionId == 4294967295)
            {
                EzydInstantDB DB = new EzydInstantDB();
                MySqlDataReader tempRdr = DB.SqlQuery("SELECT MAX(`maxID`) as `overallMaxID` " +
                    "FROM ( " +
                    "SELECT MAX(`transactionID`) as `maxID` FROM `transactions_reqs` " +
                    "UNION " +
                    "SELECT MAX(`transactionID`) as `maxID` FROM `transactions_pending` " +
                    "UNION " +
                    "SELECT MAX(`transactionID`) as `maxID` FROM `transactions_history` " +
                    "UNION " +
                    "SELECT MAX(`transactionID`) as `maxID` FROM `transactions_cancelled` " +
                    "UNION " +
                    "SELECT MAX(`transactionID`) as `maxID` FROM `transactions_accepted_history` " +
                    ") as `A` ");
                if (tempRdr.Read())
                {
                    try
                    {
                        lastTransactionId = (UInt32)tempRdr["overallMaxID"];
                    }
                    catch (InvalidCastException)
                    {
                        lastTransactionId = 1; 
                    }
                }
                else
                    throw new Exception("cannot get no of transaction");
            }

            return ++lastTransactionId;
        }
    }
}