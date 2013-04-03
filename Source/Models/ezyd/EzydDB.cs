using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ezyd.Models
{
    public class EzydDB : FBappDB
    {
        public EzydDB(String _server, String _database, String _UID, String _password = "")
            : base(_server, _database, _UID, _password) //empty constructor 
        {
        }

        //TODO optimalize it! (to 1 query instead of 4)
        public int transactionExists(String transactionID) //0=does not exists, 1=exists in reqs, 2=exists in pending, 3=exists in history 4=exists in cancelled
        {
            SqlQuery("SELECT `transactionID` FROM `transactions_reqs` WHERE `transactionID` = " + transactionID);
            if (reader.Read())
                return 1; //transaction exists in reqs

            SqlQuery("SELECT `transactionID` FROM `transactions_pending` WHERE `transactionID` = " + transactionID);
            if (reader.Read())
                return 2; //transaction exists in pending

            SqlQuery("SELECT `transactionID` FROM `transactions_history` WHERE `transactionID` = " + transactionID);
            if (reader.Read())
                return 3; //transaction exists in history

            SqlQuery("SELECT `transactionID` FROM `transactions_cancelled` WHERE `transactionID` = " + transactionID);
            if (reader.Read())
                return 4; //transaction exists in cancelled

            return 0; //transaction does not exist in any table
        }

        public List<TReqSet> findUserTransactionReqs(String userID) //TODO przerobic to na left join, right join, ktory od razu zwroci to co trzeba - to co tu zrobilem jest bez sensu, ale szybko napisane
        {
            List<TReqSet> transactionsList = new List<TReqSet>();
            
            SqlQuery("SELECT `transactionID` FROM `transactions_reqs` WHERE `userID` = " + userID);
            List<UInt32> transactionsID = new List<UInt32>(); 
            while (reader.Read()) //this cannnot be done in 2 readers, coz my reader is build in way which allows me to make only one reader in 1 time (its always the same reader)
                transactionsID.Add((UInt32)reader[0]);

            foreach (UInt32 tempInt in transactionsID)
                transactionsList.Add(new TReqSet(this, tempInt));

            return transactionsList;
        }

        public int acceptTransaction(UInt32 _transactionID, UInt64 _userID) //0=transaction accepted, 1=transaction is all accepted(every user accepted it, so its moved to transactions_pending)
        {
            if (transactionExists(Convert.ToString(_transactionID)) != 1)
                throw new Exception("transaction does not exists in reqs");

            bool everyoneElseAccepted = true;
            bool userExistsInTransaction = false;
            SqlQuery("SELECT `accepted`, `userID` FROM `transactions_reqs` " 
                    + "WHERE `transactionID` = " + _transactionID);
            while (reader.Read())
                if ((UInt64)reader[1] == _userID)
                {
                    if ((Int16)reader[0] == 1)
                        throw new Exception("transaction already accepted by that user");
                    else
                        userExistsInTransaction = true;
                }
                else if ((Int16)reader[0] == 0)
                    everyoneElseAccepted = false;

            if (userExistsInTransaction)
                SqlNonQuery("UPDATE `transactions_reqs` SET `accepted` = 1 "
                           + "WHERE `userID` = " + _userID + " AND `transactionID` = " + _transactionID);
            else
                throw new Exception("user does not exists in that transaction");

            if (everyoneElseAccepted)
            {
                TReqSet transaction = new TReqSet(this, _transactionID);
                transaction.moveToPending(this);
            }

            return 0;
        }

        public void removeTransaction(UInt32 _transactionID, String _tableName)
        {
            SqlNonQuery("DELETE FROM `" + _tableName + "` WHERE `transactionID` = " + _transactionID);
        }

        public void removeTransaction(UInt32 _transactionID)
        {
            SqlNonQuery("DELETE FROM `transactions_reqs` WHERE `transactionID` = " + _transactionID);
            SqlNonQuery("DELETE FROM `transactions_pending` WHERE `transactionID` = " + _transactionID);
            SqlNonQuery("DELETE FROM `transactions_history`WHERE `transactionID` = " + _transactionID);
        }

        public void fulfillTransaction(UInt32 _transactionID, UInt64 receiverID, UInt64 payerID)
        {
            THisRec temp = new THisRec(this, _transactionID, receiverID, payerID);
            temp.moveToHistory(this);
            removeTransaction(_transactionID, "transactions_pending");
        }

        public int countUserBalance(String userID)
        {
            if (userExists(userID) == false)
                throw new Exception("User does not exists, so user balance can not be checked");

            UInt32 minusBalance = 0; //optimalization ...
            SqlQuery("SELECT `value` FROM `transactions_pending` WHERE `minusGuyID` = " + userID);
            while (reader.Read())
            {
                minusBalance += (UInt32)reader[0];
            }

            UInt32 plusBalance = 0;
            SqlQuery("SELECT `value` FROM `transactions_pending` WHERE `plusGuyID` = " + userID);
            while (reader.Read())
            {
                plusBalance += (UInt32)reader[0];
            }

            return (int)plusBalance - (int)minusBalance;
        }
    }
}
