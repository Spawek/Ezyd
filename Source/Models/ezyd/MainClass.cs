using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;

using MySql.Data.MySqlClient;

/* TODO:
 * dorobic usuwanie frienshipa (jeśli nie ma długu to bez problemu każda ze stron, jeśli jest dług, 
 * to trzeba wybrac (z listy) przyjaciela obu stron, który zgodzi się pośredniczyć w długu
 * 
 * 
 * - obsluga zapytan do serwera MySQL mogłaby odbywać się wielowątkowo (szczególnie, kiedy serwery te stały by na różnych maszynach)
 * przerobic friends na 2 tabele - reqs i normalna - wtedy w reqs beda 2 kolumny, w normalnej tez 2 kolumny - nie bedzie syfu
 * byloby fajnie jakby uzytkownikom rysowal sie graf transakcji ///ale to na poooooootem
 * zaakceptowanie przyjazni powinno odpalac transakcje, które są blokowane przez to, że nie ma tej przyjaźni
 */
namespace ezyd.Models
{
    public class MainClass 
    {
        static void Main(String []args)
        {
            /*
            FBappDB DB = new FBappDB("localhost", "test", "root", "1maciek");
            DB.readTable("123123");
           // DB.SqlNonQuery("INSERT INTO `123123` (`qwe`, `rew`, `ter`) "
           //                 + "VALUES (1, 2, 3);");
              
            */
            //System.Console.ReadKey();

            EzydDB DB = new EzydDB("localhost", "test", "root", "1maciek");

            DB.removeTransaction(5234732);

            TReqSet set = new TReqSet(5234732);
            set.addRecord(new TReqRec(5234732, 9876, 234, "PLN", DateTime.Now));
            set.addRecord(new TReqRec(5234732, 365, 234, "PLN", DateTime.Now)); //ID is bad
            set.addRecord(new TReqRec(5234732, 3241, -468, "PLN", DateTime.Now));

            DB.SqlNonQuery("DELETE FROM `friends` WHERE `senderID` = 9876 OR `senderID` = 365 OR `senderID` = 3241");


            DB.sendFriendInvTEST("9876", "365");
            DB.acceptFriendInvTEST("365", "9876");
            DB.sendFriendInvTEST("9876", "3241");
            DB.acceptFriendInvTEST("3241", "9876");

            set.addToDB(DB);

            DB.acceptTransaction(5234732, 9876);
            DB.acceptTransaction(5234732, 365);
            DB.acceptTransaction(5234732, 3241);
        }
    }
}
