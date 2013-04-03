using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ezyd.Models
{
    public class FBappDB : DataBase
    {
        public FBappDB(String _server, String _database, String _UID, String _password = "") 
            : base(_server, _database, _UID, _password) { }  //empty constructor 

        public bool userExists(String userName) //checks if user exists in database 
        {
            Debug.Assert(!String.IsNullOrEmpty(userName), "userName is null or empty.");

            SqlQuery("SELECT * FROM `users` WHERE `ID` = " + userName);
            if (reader.Read())

                if (Convert.ToUInt64(reader[0]) == Convert.ToUInt64(userName))
                    return true;

            return false;
        }

        public void addUser(String ID, String firstName, String surname) //true - if user added, false - if used already was in DB
        {
            //TODO dołożyć sprawdzanie poprawności danych wejsciowych - czy ID może być FB ID
            if (userExists(ID))
                throw new Exception("user already exists in DB");

            if (SqlNonQuery("INSERT INTO `users` (`ID`, `firstName`, `surname`) VALUES (" + ID + ", '" + firstName + "', '" + surname + "')") != 1)
                throw new Exception("user could not been added to database (for reasons unknown...)");
        }

        public int areFriends(String ID1, String ID2) //0=not friends, 1=friends, -1=ID1 didnt accept, -2=ID2 didnt accept
        {
            Debug.Assert(!String.IsNullOrEmpty(ID1), "ID1 is null or empty.");
            Debug.Assert(!String.IsNullOrEmpty(ID2), "ID2 is null or empty.");

            SqlQuery("SELECT (`receiverAccepted`) FROM `friends` WHERE `senderID` = " + ID1 + " AND `receiverID` = " + ID2);
            if (reader.Read())
            {
                if ((sbyte)reader[0] == 1) 
                    return 1; //friend req already accepted

                return -2; //ID2 didnt accept
            }

            SqlQuery("SELECT (`receiverAccepted`) FROM `friends` WHERE `senderID` = " + ID2 + " AND `receiverID` = " + ID1);
            if (reader.Read())
            {
                if ((sbyte)reader[0] == 1)
                    return 1; //friend req aleready accepted

                return -1; //ID1 didnt accept
            }

            return 0; //no req, no friendship
        }

        public void sendFriendInv(String senderID, String receiverID) 
        {
            Debug.Assert(!String.IsNullOrEmpty(senderID), "senderID is null or empty.");
            Debug.Assert(!String.IsNullOrEmpty(receiverID), "receiverID is null or empty.");

            int temp = areFriends(senderID, receiverID);

            if(temp == 1 || temp == -2)
                throw new Exception("cannot send invitation - its already sent or " + senderID + " and " + receiverID + " are friends");

            if (temp == -1) //sender didint accept some inv from receiver - so its like acceptance of friendship
            {
                SqlNonQuery("UPDATE `friends` SET `receiverAccepted` = 0 "
                + "WHERE `senderID` = " + receiverID + " AND `receiverID` = " + senderID);
            }
            else // ( if no friendship before )
            {                 
                SqlNonQuery("INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) "
                + "VALUES (" + senderID + ", " + receiverID + ", 0)");
            }
        }

        public void acceptFriendInv(String accepterID, String senderID)
        {
            if (areFriends(accepterID, senderID) == -1)
            {
                SqlNonQuery("UPDATE `friends` SET `receiverAccepted` = 1 WHERE `senderID` = " + senderID + " AND `receiverID` = " + accepterID);
            }
            else
                throw new Exception("there is no invitation or it is already accepted");
        }

        public void hardRemoveFriend(String userID1, String userID2) //removes friendship between 2 guys 
        {
            SqlNonQuery("DELETE FROM `friends` "
            + " WHERE (`senderID` = " + userID1 + " AND `receiverID` = " + userID2 + ")"
            + " OR (`senderID` = " + userID2 + " AND `receiverID` = " + userID1 + ")");
        }

        public void hardRemoveFriend(String userID) //removes all freiendships of this guy
        {
            SqlNonQuery("DELETE FROM `friends` " +
            " WHERE `senderID` = " + userID +
            " OR `receiverID` = " + userID);
        }

        public void acceptFriendInvTEST(String accepterID, String senderID)//with no exception //TO TESTS ONLY
        {
            if (areFriends(accepterID, senderID) == -1)
            {
                SqlNonQuery("UPDATE `friends` SET `receiverAccepted` = 1 WHERE `senderID` = " + senderID + " AND `receiverID` = " + accepterID);
            }
            else
                return;
        }

        public void sendFriendInvTEST(String senderID, String receiverID) //with no exception //TO TESTS ONLY
        {
            Debug.Assert(!String.IsNullOrEmpty(senderID), "senderID is null or empty.");
            Debug.Assert(!String.IsNullOrEmpty(receiverID), "receiverID is null or empty.");

            int temp = areFriends(senderID, receiverID);

            if (temp == 1 || temp == -2)
                return;

            if (temp == -1) //sender didint accept some inv from receiver - so its like acceptance of friendship
            {
                SqlNonQuery("UPDATE `friends` SET `receiverAccepted` = 0 "
                + "WHERE `senderID` = " + receiverID + " AND `receiverID` = " + senderID);
            }
            else // ( if no friendship before )
            {
                SqlNonQuery("INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) "
                + "VALUES (" + senderID + ", " + receiverID + ", 0)");
            }
        }

    }
}
