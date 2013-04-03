using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ezyd.Models.ezyd;
using ezyd.Models;

namespace TestProject1
{
    [TestClass]
    public class newEzydUnitTests
    {
        [TestMethod()]
        public void getTransactionID()
        {
            UInt32 Id = TransactionIdProvider.getTransactionId();

            Assert.IsNotNull(Id);
            Assert.AreNotEqual(4294967295, Id);
        }

        //WELL-WRITTEN
        [TestMethod()]
        //its checking optimization alghoritm, but only biangle optimization will be needed
        public void biangleOptimization() //friendship is not checked anyway in biangle optimization
        {
            var DB = new EzydDB("localhost", "test", "root", "1maciek");

            //deleting old transactions pending with 100 user - it will delete every transaction between 2 of these guys
            DB.SqlNonQuery(
                "DELETE FROM `transactions_pending` " + 
                "WHERE `plusGuyID` = 100 " +
                "OR " +
                "`minusGuyID` = 100");

            //adding test transactions pending
            DB.SqlNonQuery(
                "INSERT INTO `transactions_pending` " +
                "(`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) " +
                "VALUES " +
                "(501, 100, 101, 222, 'PLN', now(), ' '), " +
                "(501, 101, 100, 888, 'PLN', now(), ' ')");

            //adding friendship (it will generate new records all the time but i dont care)
            DB.SqlNonQuery(
                "INSERT INTO `friends` " +
                "(`senderID`, `receiverID`, `receiverAccepted`) "+ 
                "VALUES (100, 101, 1)");

            //optimization
            TOptimalizator.optimizeTransactionsAndApply(DB, new List<ulong>(new ulong[]{101, 100}));
            
            //checking result
            var reader = DB.SqlQuery(
                "SELECT `value` " +
                "FROM `transactions_pending` " +
                "WHERE `plusGuyID` = 101 " + 
                "AND " +
                "`minusGuyID` = 100 ");

            if(reader.Read())
                Assert.AreEqual(666, (int)((uint)reader[0]));
            else
                Assert.Fail("reader didn't found anything");
        }

        [TestMethod()]
        //its checking optimization alghoritm - only triangle optimization will be needed
        //optimize A->B, B->C to A->C, B->C
        public void triangleOptimization()
        {
            var DB = new EzydDB("localhost", "test", "root", "1maciek");

            //deleting old transactions pending with 102 and 103 user - it will delete every transaction between 3 of these guys
            DB.SqlNonQuery(
                "DELETE FROM `transactions_pending` " +
                "WHERE `plusGuyID` = 102 " +
                "OR " +
                "`minusGuyID` = 102 " +
                "OR " +
                "`plusGuyID` = 103 " +
                "OR " +
                "`minusGuyID` = 103 ");

            //adding friendship (it will generate new records all the time but i dont care)
            DB.SqlNonQuery(
                "INSERT INTO `friends` " +
                "(`senderID`, `receiverID`, `receiverAccepted`) " +
                "VALUES " +
                "(102, 103, 1), " +
                "(103, 104, 1), " +
                "(104, 102, 1)");

            //adding test transactions pending
            DB.SqlNonQuery(
                "INSERT INTO `transactions_pending` " +
                "(`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) " +
                "VALUES " +
                "(501, 102, 103, 888, 'PLN', now(), ' '), " +
                "(501, 103, 104, 222, 'PLN', now(), ' ')" );

            //optimization
            TOptimalizator.optimizeTransactionsAndApply(DB, new List<ulong>(new ulong[] { 102, 103, 104 }));

            //checking result 1 (103 -> 102 value)
            var reader = DB.SqlQuery(
                "SELECT `value` " +
                "FROM `transactions_pending` " +
                "WHERE `plusGuyID` = 102 " +
                "AND " +
                "`minusGuyID` = 103 ");
            if (reader.Read())
                Assert.AreEqual(666, (int)((uint)reader[0]));
            else
                Assert.Fail("reader didn't found anything");

            //checking result 2 (104 -> 102 value)
            reader = DB.SqlQuery(
                "SELECT `value` " +
                "FROM `transactions_pending` " +
                "WHERE `plusGuyID` = 102 " +
                "AND " +
                "`minusGuyID` = 104 ");

            if (reader.Read())
                Assert.AreEqual(222, (int)((uint)reader[0]));
            else
                Assert.Fail("reader didn't found anything (2)");
        }

                [TestMethod()]
        //its checking optimization alghoritm - only triangle optimization will be needed
        //optimize A->B. B->C to A->C, A->B
        public void triangleOptimization2()
        {
            var DB = new EzydDB("localhost", "test", "root", "1maciek");

            //deleting old transactions pending with 102 and 103 user - it will delete every transaction between 3 of these guys
            DB.SqlNonQuery(
                "DELETE FROM `transactions_pending` " +
                "WHERE `plusGuyID` = 105 " +
                "OR " +
                "`minusGuyID` = 105 " +
                "OR " +
                "`plusGuyID` = 106 " +
                "OR " +
                "`minusGuyID` = 106 ");

            //adding friendship (it will generate new records all the time but i dont care)
            DB.SqlNonQuery(
                "INSERT INTO `friends` " +
                "(`senderID`, `receiverID`, `receiverAccepted`) " +
                "VALUES " +
                "(105, 106, 1), " +
                "(105, 107, 1), " +
                "(106, 107, 1)");

            //adding test transactions pending
            DB.SqlNonQuery(
                "INSERT INTO `transactions_pending` " +
                "(`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) " +
                "VALUES " +
                "(501, 105, 106, 222, 'PLN', now(), ' '), " +
                "(501, 106, 107, 888, 'PLN', now(), ' ')" );

            //optimization
            TOptimalizator.optimizeTransactionsAndApply(DB, new List<ulong>(new ulong[] { 102, 103, 104 }));

            //checking result 1 (103 -> 102 value)
            var reader = DB.SqlQuery(
                "SELECT `value` " +
                "FROM `transactions_pending` " +
                "WHERE `plusGuyID` = 105 " +
                "AND " +
                "`minusGuyID` = 107 ");
            if (reader.Read())
                Assert.AreEqual(222, (int)((uint)reader[0]));
            else
                Assert.Fail("reader didn't found anything");

            //checking result 2 (104 -> 102 value)
            reader = DB.SqlQuery(
                "SELECT `value` " +
                "FROM `transactions_pending` " +
                "WHERE `plusGuyID` = 106 " +
                "AND " +
                "`minusGuyID` = 107 ");

            if (reader.Read())
                Assert.AreEqual(666, (int)((uint)reader[0]));
            else
                Assert.Fail("reader didn't found anything (2)");
        }

    }
}
