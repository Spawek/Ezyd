using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ezyd.Models;
using MySql.Data.MySqlClient;


namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod()]
        public void MySQLdbConnection()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            Assert.IsTrue(DB.isOpen());
        }

        [TestMethod()]
        public void InsertAndReadDB()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.SqlNonQuery("DELETE FROM `123123` "
                + "WHERE `qwe` = 666666");

            DB.SqlNonQuery("INSERT INTO `123123` (`qwe`, `rew`, `ter`) "
                + "VALUES (666666, 532, 3) ");
            int expected = 532;
            int actual = -1;

            MySqlDataReader tempRdr = DB.SqlQuery(" SELECT * FROM `123123`"
            + " WHERE `qwe` = 666666");
            if (tempRdr.Read())//to nie wchodzi
                actual = (int)tempRdr["rew"];

            tempRdr.Close();

            DB.SqlNonQuery("DELETE FROM `123123` "
                + "WHERE `qwe` = 666666");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]

        public void userExistsTest()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.SqlNonQuery("DELETE FROM `users` WHERE `ID` = 666");
            DB.SqlNonQuery("INSERT INTO `users` (`ID`) VALUES (666)"); //niby dodaje, ale potem wywala error

            Assert.IsTrue(DB.userExists("666"));
        }

        [TestMethod()]

        public void userNotExistsTest()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.SqlNonQuery("DELETE FROM `users` WHERE `ID` = 666");

            Assert.IsFalse(DB.userExists("666"));
        }

        [TestMethod()]

        public void addUserWhenUserDoesntExist()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.SqlNonQuery("DELETE FROM `users` WHERE `ID` = 669");

            bool exceptionHappened = false;
            try
            {
                DB.addUser("669", "asd", "dsa");
            }
            catch (Exception e)//coz of bad balance
            {
                exceptionHappened = true;
            }

            Assert.IsFalse(exceptionHappened);
        }

        [TestMethod()]

        public void addUserWhenUserExists() //should not crash
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.SqlNonQuery("DELETE FROM `users` WHERE `ID` = 669");
            DB.SqlNonQuery("INSERT INTO `users` (`ID`) VALUES (669)"); //niby dodaje, ale potem wywala error

            bool exceptionHappened = false;
            try
            {
                DB.addUser("669", "sdf", "fds");
            }
            catch (Exception e)//coz of bad balance
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened);
        }

        [TestMethod()]

        public void isFriend_Req()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.hardRemoveFriend("777", "555");
            DB.SqlNonQuery("INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) VALUES (777, 555, 0)");

            Assert.AreEqual(-2, DB.areFriends("777", "555"));
        }

        [TestMethod()]

        public void isFriend_Req_Inverted()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.hardRemoveFriend("777", "555");
            DB.SqlNonQuery("INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) VALUES (777, 555, 0)");

            Assert.AreEqual(-1, DB.areFriends("555", "777"));
        }

        [TestMethod()]

        public void isFriend_nonFriend()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.hardRemoveFriend("444", "888");

            Assert.AreEqual(0, DB.areFriends("444", "888"));
        }


        [TestMethod()]

        public void isFriend_NormalInverted()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.hardRemoveFriend("999", "111");
            DB.SqlNonQuery("INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) VALUES (999, 111, 1)");

            Assert.AreEqual(1, DB.areFriends("111", "999"));
        }

        [TestMethod()]

        public void isFriend_Normal()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.hardRemoveFriend("999", "111");
            DB.SqlNonQuery("INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) VALUES (999, 111, 1)");

            Assert.AreEqual(1, DB.areFriends("999", "111"));
        }


        [TestMethod()]

        public void sendFriendInv()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.hardRemoveFriend("66666", "88888");
            DB.sendFriendInv("66666", "88888");

            Assert.AreEqual(-2, DB.areFriends("66666", "88888"));
        }

        [TestMethod()]

        public void sendFriendInv_whenArldyFriend_Accepted()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");

            DB.hardRemoveFriend("753", "123");
            DB.SqlNonQuery("INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) VALUES (753, 123, 1)");

            bool exceptionHappened = false;
            try
            {
                DB.sendFriendInv("753", "123");
            }
            catch (Exception e)//coz of bad balance
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened);
        }

        [TestMethod()]

        public void acceptFriendInv()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");

            DB.hardRemoveFriend("5558", "9786");

            DB.sendFriendInv("5558", "9786");
            DB.acceptFriendInv("9786", "5558");

            Assert.AreEqual(1, DB.areFriends("9786", "5558"));
        }

        [TestMethod()]

        public void removeFriendship()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");

            DB.hardRemoveFriend("5558", "9786");

            DB.sendFriendInv("5558", "9786");
            DB.acceptFriendInv("9786", "5558");

            DB.hardRemoveFriend("5558", "9786");

            Assert.AreEqual(0, DB.areFriends("9786", "5558"));
        }

        [TestMethod()]

        public void removeFriendships()
        {
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");

            DB.hardRemoveFriend("5558", "9786");

            DB.sendFriendInv("5558", "9786");
            DB.acceptFriendInv("9786", "5558");

            DB.hardRemoveFriend("5558");

            Assert.AreEqual(0, DB.areFriends("9786", "5558"));
        }


        [TestMethod()]

        public void transactionExists_inReq()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(857687658);
            DB.SqlNonQuery("INSERT INTO `transactions_reqs` "
                + "(`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`) "
                + "VALUES (857687658, 534534, 40, 'PLN', NOW(), 1)");

            Assert.AreEqual(1, DB.transactionExists("857687658"));
        }

        [TestMethod()]

        public void transactionExists_whenDoesNotExist()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(489465);

            Assert.AreEqual(0, DB.transactionExists("489465"));
        }

        [TestMethod()]

        public void transactionExists_inPending()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(674874);
            DB.SqlNonQuery("INSERT INTO `transactions_pending` "
                + "(`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`) "
                + "VALUES (674874, 8765876, 534534, 40, 'PLN', NOW())");

            Assert.AreEqual(2, DB.transactionExists("674874"));
        }

        [TestMethod()]

        public void transactionExists_inHistory()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(492646753);
            DB.SqlNonQuery("INSERT INTO `transactions_history` "
                + "(`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`) "
                + "VALUES (492646753, 8765876, 534534, 40, 'PLN', NOW())");

            Assert.AreEqual(3, DB.transactionExists("492646753"));
        }

        [TestMethod()]

        public void removeTransaction()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(86452543);
            DB.SqlNonQuery("INSERT INTO `transactions_pending` "
                + "(`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`) "
                + "VALUES (86452543, 465, 32, 634, 'PLN', NOW())");

            DB.removeTransaction(86452543, "transactions_pending");

            Assert.AreEqual(0, DB.transactionExists("86452543"));
        }

        [TestMethod()]

        public void addTransactionReqSet()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(5879879);

            TReqSet set = new TReqSet(5879879);
            set.addRecord(new TReqRec(5879879, 7654, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(5879879, 756, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(5879879, 8765, -468, "PLN", DateTime.Now));

            set.addToDB(DB);

            Assert.AreEqual(1, DB.transactionExists("5879879"));
        }

        [TestMethod()]

        public void addTransactionReqSet_notBalanced()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(5879879);

            TReqSet set = new TReqSet(2189);
            set.addRecord(new TReqRec(2189, 7654, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(2189, 756, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(2189, 8765, -85, "PLN", DateTime.Now)); //balance != 0

            bool exceptionHappened = false;
            try
            {
                set.addToDB(DB);
            }
            catch (Exception e)//coz of bad balance
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened);
        }

        [TestMethod()]

        public void addTransactionReqSet_diffID()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(4289498);
            DB.removeTransaction(765);

            TReqSet set = new TReqSet(4289498);
            set.addRecord(new TReqRec(4289498, 7654, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(765, 756, 234, "PLN", DateTime.Now)); //ID is bad
            set.addRecord(new TReqRec(4289498, 8765, -468, "PLN", DateTime.Now));

            bool exceptionHappened = false;
            try
            {
                set.addToDB(DB);
            }
            catch (Exception e)// coz of bad ID
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened);
        }

        [TestMethod()]

        public void findUserTransactionReqs()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(9768987);

            TReqSet set = new TReqSet(9768987);
            set.addRecord(new TReqRec(9768987, 741, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(9768987, 285, 234, "PLN", DateTime.Now)); //ID is bad
            set.addRecord(new TReqRec(9768987, 386, -468, "PLN", DateTime.Now));

            set.addToDB(DB);

            List<TReqSet> transactionsReqs = DB.findUserTransactionReqs("741");

            Assert.AreEqual(234, transactionsReqs[0].TRecords[0].value);
        }

        [TestMethod()]

        public void addTransactionPending_whenGood()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(489237895);

            TPenSet set = new TPenSet(489237895);
            set.addRecord(new TPenRecord(489237895, 87982, 1654, 489465, "PLN", DateTime.Now));
            set.addRecord(new TPenRecord(489237895, 1654, 87982, -489465, "PLN", DateTime.Now));

            set.addToDB(DB);

            Assert.AreEqual(2, DB.transactionExists(Convert.ToString(489237895)));
        }


        [TestMethod()]

        public void acceptTransaction_whenTheyAreNotFriends()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(736372423);

            TReqSet set = new TReqSet(736372423);
            set.addRecord(new TReqRec(736372423, 5983765, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(736372423, 265113, 234, "PLN", DateTime.Now)); //ID is bad
            set.addRecord(new TReqRec(736372423, 134643, -468, "PLN", DateTime.Now));

            set.addToDB(DB);

            DB.acceptTransaction(736372423, 5983765);
            DB.acceptTransaction(736372423, 265113);

            bool expectionHappened = false;
            try
            {
                DB.acceptTransaction(736372423, 134643);
            }
            catch (Exception e)
            {
                expectionHappened = true;
            }

            Assert.AreEqual(true, expectionHappened);
        }

        [TestMethod()]

        public void acceptTransaction_whenGood()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(5234732);

            TReqSet set = new TReqSet(5234732);
            set.addRecord(new TReqRec(5234732, 37375, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(5234732, 365, 234, "PLN", DateTime.Now)); //ID is bad
            set.addRecord(new TReqRec(5234732, 3241, -468, "PLN", DateTime.Now));

            DB.hardRemoveFriend("37375");
            DB.hardRemoveFriend("365");
            DB.hardRemoveFriend("3241");

            DB.sendFriendInv("37375", "365");
            DB.acceptFriendInv("365", "37375");
            DB.sendFriendInv("37375", "3241");
            DB.acceptFriendInv("3241", "37375");

            set.addToDB(DB);

            DB.acceptTransaction(5234732, 37375);
            DB.acceptTransaction(5234732, 365);
            DB.acceptTransaction(5234732, 3241);

            Assert.AreEqual(2, DB.transactionExists("5234732"));
        }

        [TestMethod()]

        public void acceptTransaction_whenTransactionBIG()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(746254534);

            TReqSet set = new TReqSet(746254534);
            set.addRecord(new TReqRec(746254534, 9876, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 365, 184, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 74275, -468, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 7452, -874, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 842, 876, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 3241, 243, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 25745, 355, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 5734, 643, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 856, 453, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 523, -745, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(746254534, 7892, -901, "PLN", DateTime.Now));

            DB.hardRemoveFriend("9876");
            DB.hardRemoveFriend("365");
            DB.hardRemoveFriend("74275");
            DB.hardRemoveFriend("7452");
            DB.hardRemoveFriend("842");
            DB.hardRemoveFriend("3241");
            DB.hardRemoveFriend("25745");
            DB.hardRemoveFriend("5734");
            DB.hardRemoveFriend("856");
            DB.hardRemoveFriend("523");
            DB.hardRemoveFriend("7892");

            DB.sendFriendInv("9876", "365");
            DB.acceptFriendInv("365", "9876");

            DB.sendFriendInv("365", "74275");
            DB.acceptFriendInv("74275", "365");

            DB.sendFriendInv("74275", "7452");
            DB.acceptFriendInv("7452", "74275");

            DB.sendFriendInv("7452", "842");
            DB.acceptFriendInv("842", "7452");

            DB.sendFriendInv("842", "3241");
            DB.acceptFriendInv("3241", "842");

            DB.sendFriendInv("3241", "25745");
            DB.acceptFriendInv("25745", "3241");

            DB.sendFriendInv("25745", "5734");
            DB.acceptFriendInv("5734", "25745");

            DB.sendFriendInv("5734", "856");
            DB.acceptFriendInv("856", "5734");

            DB.sendFriendInv("856", "523");
            DB.acceptFriendInv("523", "856");

            DB.sendFriendInv("523", "7892");
            DB.acceptFriendInv("7892", "523");

            DB.sendFriendInv("523", "365");
            DB.acceptFriendInv("365", "523");

            DB.sendFriendInv("9876", "25745");
            DB.acceptFriendInv("25745", "9876");

            DB.sendFriendInv("9876", "5734");
            DB.acceptFriendInv("5734", "9876");

            DB.sendFriendInv("9876", "856");
            DB.acceptFriendInv("856", "9876");

            DB.sendFriendInv("9876", "523");
            DB.acceptFriendInv("523", "9876");

            DB.sendFriendInv("9876", "7892");
            DB.acceptFriendInv("7892", "9876");

            set.addToDB(DB);

            DB.acceptTransaction(746254534, 9876);
            DB.acceptTransaction(746254534, 365);
            DB.acceptTransaction(746254534, 74275);
            DB.acceptTransaction(746254534, 7452);
            DB.acceptTransaction(746254534, 842);
            DB.acceptTransaction(746254534, 3241);
            DB.acceptTransaction(746254534, 25745);
            DB.acceptTransaction(746254534, 5734);
            DB.acceptTransaction(746254534, 856);
            DB.acceptTransaction(746254534, 523);
            DB.acceptTransaction(746254534, 7892);

            Assert.AreEqual(2, DB.transactionExists("746254534"));
        }

        [TestMethod()]

        public void acceptTransaction_whenGraphConnected_shouldNotException()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(64396365);

            TReqSet set = new TReqSet(64396365);

            set.addRecord(new TReqRec(64396365, 74253, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(64396365, 52367534, -234, "PLN", DateTime.Now));

            DB.SqlNonQuery("DELETE FROM `friends` WHERE `senderID` = 74253 OR `senderID` = 52367534");
            DB.sendFriendInvTEST("74253", "52367534");
            DB.acceptFriendInvTEST("52367534", "74253");

            set.addToDB(DB);

            DB.acceptTransaction(64396365, 74253);
            DB.acceptTransaction(64396365, 52367534);

            //checking correctivity of transactions in transactions pending
            bool transactionExists = false;
            DB.SqlQuery("SELECT * FROM `transactions_pending` WHERE `minusGuyID` = 52367534 AND `plusGuyID` = 74253 AND `value` = 234");
            if (DB.reader.Read())
                transactionExists = true;

            Assert.IsTrue(transactionExists);
        }

        [TestMethod()]

        public void acceptTransaction_whenGraphNotConnected_shouldException()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(3645432);

            TReqSet set = new TReqSet(3645432);

            set.addRecord(new TReqRec(3645432, 32745, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(3645432, 3626, -234, "PLN", DateTime.Now));

            DB.SqlNonQuery("DELETE FROM `friends` WHERE `senderID` = 32745 OR `senderID` = 3626");

            set.addToDB(DB);

            bool exceptionHappened = false;
            try
            {
                DB.acceptTransaction(3645432, 32745);
                DB.acceptTransaction(3645432, 3626);
            }
            catch //im not checking if it is that exception, coz im making also same test with no (no conncetivity) problem
            {
                exceptionHappened = true;
            }

            Assert.IsTrue(exceptionHappened);
        }

        [TestMethod()]

        public void fulfillTransaction()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(64527653);

            TReqSet set = new TReqSet(64527653);

            set.addRecord(new TReqRec(64527653, 463523, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(64527653, 654327, -234, "PLN", DateTime.Now));

            DB.hardRemoveFriend("463523", "654327");
            DB.sendFriendInvTEST("463523", "654327");
            DB.acceptFriendInvTEST("654327", "463523");

            set.addToDB(DB);

            DB.acceptTransaction(64527653, 463523);
            DB.acceptTransaction(64527653, 654327);

            //now transaction should be in pending

            DB.fulfillTransaction(64527653, 654327, 463523);

            Assert.AreEqual(3, DB.transactionExists("64527653"));
        }

        [TestMethod()]

        public void countUserBalance()
        {
            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.SqlNonQuery("DELETE FROM `users` WHERE `ID` = 18979456");
            DB.addUser("18979456", "Macu", "Piccu");

            DB.SqlNonQuery("DELETE FROM `transactions_pending` " +
                " WHERE `minusGuyID` = 18979456 OR `plusGuyID` = 18979456");

            DB.SqlNonQuery("INSERT INTO `transactions_pending` " +
                " (`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) VALUES " +
                " (543774, 18979456, 56436, 888, 'PLN', NOW(), 'beer'), " +
                " (6547658, 1568419, 18979456, 222, 'PLN', NOW(), '2 beers') "); //2 transactions (1-> +888, 2-> -222) = +666

            Assert.AreEqual(666, DB.countUserBalance("18979456"));
        }

    }
}
