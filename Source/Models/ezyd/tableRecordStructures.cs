using System;
using System.Collections.Generic;
using System.Linq;
using ezyd.Models.ezyd;
using System.Threading;

namespace ezyd.Models
{
    public class TReqRec //TRANSACTION REQEST RECORD
    {
        public UInt32 transactionID;
        public UInt64 userID;
        public int value;
        public String currency;
        public DateTime date;
        public Int16 accepted;
        public String description;
        public int tempListPosition; //remove if not used //used in TransactionReqRecord to build set
        public bool isBiconnectedComponent; //punkt artykulacji //TODO remove it
        public bool visited; //needed to DFS - can be easly removed - but for what ;)

        public TReqRec(UInt32 _transactionID, UInt64 _userID, int _value, String _currency, DateTime _date, String _description)
        {
            transactionID = _transactionID;
            userID = _userID;
            value = _value;
            currency = _currency;
            date = _date;
            description = _description;
        }

        public TReqRec(UInt32 _transactionID, UInt64 _userID, int _value, String _currency, DateTime _date, Int16 _accepted, String _description)
        {
            transactionID = _transactionID;
            userID = _userID;
            value = _value;
            currency = _currency;
            date = _date;
            accepted = _accepted;
            description = _description;
        }

        public TReqRec(UInt32 _transactionID, UInt64 _userID, int _value, String _currency, DateTime _date, Int16 _accepted = 0)
        {
            transactionID = _transactionID;
            userID = _userID;
            value = _value;
            currency = _currency;
            date = _date;
            accepted = _accepted;
        }

        public TReqRec(UInt32 _transactionID, UInt64 _userID, int _value, String _currency)
        {
            transactionID = _transactionID;
            userID = _userID;
            value = _value;
            currency = _currency;
            date = DateTime.Now;
            accepted = 0;
        }

        public String addToDBstring() //generates string (after value), to add transaction to DB
        {
            if (description == null || description.Equals(String.Empty))
                return " (" + transactionID + ", " + Convert.ToString(userID) + ", " + Convert.ToString(value)
                      + ", '" + currency + "', NOW(), " + Convert.ToString(accepted) + ", " + "' ') ";
            else
                return " (" + transactionID + ", " + Convert.ToString(userID) + ", " + Convert.ToString(value)
                    + ", '" + currency + "', NOW(), " + Convert.ToString(accepted) + ", '" + description + "') ";
        }
    }

    public class TReqSet //TRANSACTION REQUEST SET - it contains transactions graph making and optimalization of that
    {
        private UInt32 transactionID;
        public List<TReqRec> TRecords = new List<TReqRec>();

        public TReqSet(EzydDB DB, UInt32 _transactionID)//read transaction from DB
        {
            transactionID = _transactionID;
            DB.SqlQuery("SELECT `transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc` "
                        + "FROM `transactions_reqs` WHERE `transactionID` = " + Convert.ToString(transactionID));
            while (DB.reader.Read())
                addRecord(new TReqRec((UInt32)DB.reader[0],
                    (UInt64)DB.reader[1], (int)DB.reader[2],
                    (String)DB.reader[3], (DateTime)DB.reader[4],
                    (Int16)DB.reader[5], (String)DB.reader[6]
                    ));
        }

        public TReqSet(UInt32 _transactionID)//create new set
        {
            transactionID = _transactionID;
        }

        public TReqSet(List<TReqRec> _TRecords) //be aware of that - its not creating new TReqRecs
        {
            if (_TRecords.Count == 0)
                throw new Exception("transactions list is empty");

            transactionID = _TRecords[0].transactionID;
            TRecords.AddRange(_TRecords);
        }

        public TReqSet(UInt32 _transactionID, string[] ids, string[] values, string currency, string[] descriptions, string transactionName)
        {
            if (ids == null)
                throw new Exception("ids are null");

            if (values == null)
                throw new Exception("values are null");

            if (currency == null)
                throw new Exception("currency is null");

            if (descriptions == null)
                throw new Exception("description is null");

            if (ids.Length != values.Length || values.Length != descriptions.Length)
                throw new Exception("ids and values arrays lengths are not equal");

            if (transactionName == null)
                transactionName = string.Empty;

            transactionID = _transactionID;

            for (int i = 0; i < ids.Length; i++)
            {
                TRecords.Add(new TReqRec(_transactionID, (UInt64)Convert.ToInt64(ids[i]), Convert.ToInt32(values[i]), currency, DateTime.Now, descriptions[i]));
            }
            TRecords.Add(new TReqRec(_transactionID, 0, 0, currency, DateTime.Now, 1, transactionName)); //adding transaction name (as user 0 (kind of tricky ;], but kind of lame too ;[ ) )
        }

        public bool addRecord(TReqRec _record) //0=error, 1=OK
        {
            if (_record.transactionID != transactionID)
                return false;

            TRecords.Add(_record);
            TRecords.Last().tempListPosition = TRecords.Count - 1; //setting transaction position of this record

            return true;
        }

        public void addToDB(EzydDB DB)
        {
            if (DB.transactionExists(Convert.ToString(transactionID)) > 0)
                throw new Exception("transaction already on list");

            if (TRecords.Count < 2)
                throw new Exception("transaction set should have 2 or more elements");

            //checking ballance and currency
            int balance = 0;
            String currency = TRecords[0].currency;

            foreach (TReqRec record in TRecords)
            {
                if (currency != record.currency) //currency in whole transaction has to be the same
                    throw new Exception("there are different currencies in 1 transaction");

                balance += record.value;
            }
            if (balance != 0)
                throw new Exception("balance of transaction should be equal 0");

            String query;
            { //creating SQL query string
                query = "INSERT INTO `transactions_reqs` "
                    + "(`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) "
                    + "VALUES ";

                bool is1st = true;
                foreach (TReqRec record in TRecords)
                {
                    if (!is1st)
                        query += ", ";
                    else
                        is1st = false;

                    query += record.addToDBstring();
                }
            }

            DB.SqlNonQuery(query);
        }

        public void moveToCancelled(EzydDB DB, UInt64 removerID)
        {
            if (!TRecords.Any(r => r.userID == removerID))
                throw new Exception("You can't cancel transaction, which you dont belong to");

            String query;
            { //creating SQL query string
                query = "INSERT INTO `transactions_cancelled` "
                    + "(`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) "
                    + "VALUES ";

                bool is1st = true;
                foreach (TReqRec record in TRecords)
                {
                    if (!is1st)
                        query += ", ";
                    else
                        is1st = false;

                    query += record.addToDBstring();
                }
            }

            DB.SqlNonQuery(query);

            DB.removeTransaction(transactionID, "transactions_reqs");
        }

        public void moveToAcceptedTransactionHistory(EzydDB DB) //these to methods are almost the same
        {
            String query;
            { //creating SQL query string
                query = "INSERT INTO `transactions_accepted_history` "
                    + "(`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) "
                    + "VALUES ";

                bool is1st = true;
                foreach (TReqRec record in TRecords)
                {
                    if (!is1st)
                        query += ", ";
                    else
                        is1st = false;

                    query += record.addToDBstring();
                }
            }

            DB.SqlNonQuery(query);
        }

        /*
         * standard DFS algorithm - later transactions will be optimalized
         * needed in DFS algorithm in moveToPending(EzydDB DB)
         * it makes transactions while returning from loafs of DFS track
         */

        private void DFSvisitNode(List<KeyValuePair<TReqRec, List<TReqRec>>> TFriends, TPenSet outputSet, TReqRec friendToVisit)
        {
            friendToVisit.visited = true;
            foreach (TReqRec guyFriend in TFriends[friendToVisit.tempListPosition].Value) //for each friend of this guy
            {
                if (guyFriend.visited == false)
                {
                    DFSvisitNode(TFriends, outputSet, guyFriend);
                    outputSet.addRecord(TGenerator(guyFriend, friendToVisit)); //adding transaction to set

                    //adding information about money changes to TFriends (values has changed after that transaction (it is why this transaction was made))
                    friendToVisit.value += guyFriend.value;
                    guyFriend.value = 0;
                }
            }
        }

        /*
         * COMPLEXITY:
         * if none optimalization has been found its n^2
         * if some optimalization has been found its probably much less than n^2 (to 1st optimalization found)
         *      but it starts looking for new optimalization again so more than n^2
         */

        [Obsolete]
        private bool optimalizeTransactions(TPenSet set, List<KeyValuePair<TReqRec, List<TReqRec>>> TFriends) //true if some optimalization was done
        {
            /*
             * biangle(:D) optimalization (find A->B, A->B and make A->B of it)
             * COMPLEXITY:
             * n^2
             */
            foreach (TPenRecord rec in set.transactionRecords)
            {
                List<TPenRecord> tempRecsList = set.transactionRecords.FindAll(
                        rec2 =>
                        (rec2 != rec) && (
                            (rec2.minusGuyID == rec.minusGuyID) && (rec2.plusGuyID == rec.plusGuyID) //possibility 1
                            ||
                            (rec2.minusGuyID == rec.plusGuyID) && (rec2.plusGuyID == rec.minusGuyID) //possibility 2
                        )
                    );

                foreach (var tempRec in tempRecsList)
                {
                    //they has to be friends, because they has transactions between each other

                    if (tempRec != null)
                    {
                        if (tempRec.minusGuyID == rec.minusGuyID) //possibility 1
                            set.addRecord(new TPenRecord(this.transactionID, rec.minusGuyID, rec.plusGuyID, (rec.value + tempRec.value), rec.currency));
                        else //possibility 2
                            if (rec.value > tempRec.value)
                                set.addRecord(new TPenRecord(this.transactionID, rec.minusGuyID, rec.plusGuyID, (rec.value - tempRec.value), rec.currency));
                            else
                                set.addRecord(new TPenRecord(this.transactionID, tempRec.minusGuyID, tempRec.plusGuyID, (tempRec.value - rec.value), rec.currency));

                        //removing old transactions;
                        set.transactionRecords.Remove(rec);
                        set.transactionRecords.Remove(tempRec);

                        //to not check anything else in foreach
                        return true;
                    }
                }
            }

            /*
             * triangle optimalisation (find A->B, B->C and make A->C, B->C of it)
             * need to calculate so much to avaid bugs when from A->B, B->C u make A->C, C->B, and later A->B, B->C again and again
             *
             * COMPLEXITY:
             * n^2
             */
            foreach (TPenRecord rec in set.transactionRecords)
            {
                List<TPenRecord> tempTransactions = set.transactionRecords.FindAll(rec2 => rec2.plusGuyID == rec.minusGuyID);
                foreach (var transaction in tempTransactions)
                {
                    if (transaction != null) //if found something to optimalize
                    {
                        //A->B, B->C
                        TReqRec guyA = TFriends.Find(rec3 => rec3.Key.userID == rec.minusGuyID).Key;
                        TReqRec guyB = TFriends.Find(rec3 => rec3.Key.userID == rec.plusGuyID).Key;
                        TReqRec guyC = TFriends.Find(rec3 => rec3.Key.userID == transaction.plusGuyID).Key;

                        if (TFriends[guyA.tempListPosition].Value.Find(rec3 => rec3.userID == guyC.userID) != null) //if A and C are friends
                        {
                            if (rec.value == transaction.value)
                            { //then make A->C of it
                                set.addRecord(new TPenRecord(this.transactionID, guyA.userID, guyC.userID, rec.value, rec.currency));
                            }
                            else if (rec.value > transaction.value)
                            { //then make A->B, A->C of it
                                set.addRecord(new TPenRecord(this.transactionID, guyA.userID, guyB.userID, (rec.value - transaction.value), rec.currency));
                                set.addRecord(new TPenRecord(this.transactionID, guyA.userID, guyC.userID, transaction.value, rec.currency));
                            }
                            else
                            { //then make A->C, B->C of it
                                set.addRecord(new TPenRecord(this.transactionID, guyA.userID, guyC.userID, rec.value, rec.currency));
                                set.addRecord(new TPenRecord(this.transactionID, guyB.userID, guyC.userID, (transaction.value - rec.value), rec.currency));
                            }

                            //removing old transactions
                            set.transactionRecords.Remove(rec);
                            set.transactionRecords.Remove(transaction);

                            //to not check anything else in foreach
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void moveToPending(EzydDB DB) //removes transaction from req table (after everyone accept it) (and deletes it from reqs)
        {
            if (TRecords.Any(rec => rec.accepted == 0))
            {
                throw new Exception("not everyone has accepted transaction");
            }

            try //TODO fix it - its adding it as accepted, but when its not accepted its removing it from DB //IMPORTANT
            {
                moveToAcceptedTransactionHistory(DB);

                TRecords.RemoveAll(t => t.userID == 0); //in other case they will be error that user 0 (transaction title) has no friends - transaction title is no longer needed

                //list that contains every transaction guy and a list of his friends (only friends, who belongs to that transaction are important, so other ones are removed)
                List<KeyValuePair<TReqRec, List<TReqRec>>> TFriends = findTransactionFriends(DB); //TRANSACTION FRIENDS

                TPenSet outputSet = new TPenSet(transactionID);

                /*
                 * algorithm which is just making DFS on connections graph - it makes all transaction that way and later it optimalizes transactions by looking for triangles transactions
                 * it was suppposed to be much faster than alghoritm below (which is n^2), but now its even slower than that (DFS is n, but optimalization is much worse than n^2)
                 * but anyway this algorithm gives very good resoults (good set of output transactions), so speed of it can be improved in case of need
                 * starting optimalization from the beggining again and again is very slow //TODO improve it
                 */
                {
                    foreach (KeyValuePair<TReqRec, List<TReqRec>> guy in TFriends)
                        guy.Key.visited = false;

                    DFSvisitNode(TFriends, outputSet, TFriends[0].Key); // i use reccurrsion in here - so DFS is done

                    if (TFriends.Any(rec => rec.Key.visited == false))
                        throw new Exception("not every guy in transaction was visited by DFS - transaction error");

                    if (TFriends.Any(rec => rec.Key.value != 0))
                        throw new Exception("not every guy in transaction has value equal to 0 - before optimalization");

                    //OLD ONE
                    /*
                    //transactions optimalization
                    while (optimalizeTransactions(outputSet, TFriends) == true) ; //if some optimalization was done - it tries to make some optimalization again  (tail raccurence)
                    //TODO check speed of doing that (possibly some limit is needed)

                    outputSet.addToDB(DB); //adding transactions to pending table
                    */

                    //NEW ONE
                    //transactions optimalization
                    TOptimalizator.optimizeTransactionsAndApply(DB, outputSet, TFriends);

                    if (TFriends.Any(rec => rec.Key.value != 0))
                        throw new Exception("not every guy in transaction has value equal to 0 - after optimalization");
                }

                /* algorithm which is looking for biconnected components (and dont want to delete them)
                while (TFriends.Any(rec => rec.Key.value != 0))
                {
                    //it also removes connections with guys with ballace == 0
                    //looking Biconnected components (punktow artykulacji)
                    findBiconnectedComponents(TFriends); //it saves information about it in TReqRec's which belongs to TFriends

                    //TODO change var name
                    int tempInt;
                    {   //it uses bicoonnected components var from reqs to count it - it not simply counts all this components
                        int minValue = TFriends.Min(rec => countAvitableFriends(rec.Key, rec.Value)); //finds the guy with the smallest no of friends //lambda
                        tempInt = TFriends.FindIndex(rec => countAvitableFriends(rec.Key, rec.Value) == minValue);
                    }

                    int temp_maxDiffValue;
                    {
                        int tempIntGuybalance = Math.Sign(TFriends[tempInt].Key.value); //-1=minus, 1=plus //CHECK IT -> i hope it works in that way - i have no internet now

                        if (tempIntGuybalance == -1)
                            temp_maxDiffValue = TFriends[tempInt].Value.Max(record => record.value); //lambda expession //finds friend with biggest vallue difference from tempInt
                        else if (tempIntGuybalance == 1)
                            temp_maxDiffValue = TFriends[tempInt].Value.Min(record => record.value);
                        else
                            throw new Exception("unhandled value ( \" 0 \" )");
                    }

                    //friend with biggest value diff to temp guy
                    int tempInt2 = TFriends[tempInt].Value.Find(rec => (rec.value == temp_maxDiffValue)).tempListPosition; //nont use findIndex from value, because its not the same index

                    outputSet.addRecord(TGenerator(TFriends[tempInt].Key, TFriends[tempInt2].Key)); //adding transaction between temp1 and temp2 to the list
                    int tempValue = TFriends[tempInt].Key.value;
                    TFriends[tempInt].Key.value = 0;
                    TFriends[tempInt2].Key.value += tempValue; //like giving tempInt guy debt/money to
                }
                */

                DB.removeTransaction(transactionID, "transactions_reqs"); //dont remove it (again ;))
            }
            catch (Exception) //TODO TRY REMOVE IT!!!!! //or change way it works (and looks like)
            {
                DB.removeTransaction(transactionID, "transactions_accepted_history");
                throw;
            }
        }

        //OLD ALGORITHM
        //TODO make it looking better (and maybe working a little bit faster);
        private int visitNode(int n, int number, int father, bool[] visited, int[] preOrder, int[] low, List<KeyValuePair<TReqRec, List<TReqRec>>> TFriends)
        {
            visited[n] = true;
            preOrder[n] = number;
            number++;
            low[n] = preOrder[n];

            int counter = 0;
            foreach (TReqRec rec in TFriends[n].Value)
            {
                if (rec.value != 0) //dont check guys who already did their transactions
                {
                    if (!visited[rec.tempListPosition])
                    {
                        number = visitNode(rec.tempListPosition, number, n, visited, preOrder, low, TFriends);
                        if (low[n] > low[rec.tempListPosition])
                            low[n] = low[rec.tempListPosition];

                        counter++;
                    }
                    else if (rec.tempListPosition != father)
                    {
                        if (low[n] > preOrder[rec.tempListPosition])
                            low[n] = preOrder[rec.tempListPosition];
                    }
                }
            }

            if (n == 0 && counter != 1) //if root(n==0) have 2 sons it is biconnected component too
                TFriends[0].Key.isBiconnectedComponent = true;

            return number;
        }

        //OLD ALGORITHM
        //REMEMBER checking if root is biconnected comp is done in visitNode
        private void findBiconnectedComponents(List<KeyValuePair<TReqRec, List<TReqRec>>> TFriends)
        {
            bool[] visited = new bool[TFriends.Count];
            int[] preOrder = new int[TFriends.Count]; //order of nodes (found by DFS)
            int[] low = new int[TFriends.Count];

            for (int i = 0; i < TFriends.Count; i++) //initialising arrays
            {
                visited[i] = false;
                preOrder[i] = -1;
            }

            foreach (KeyValuePair<TReqRec, List<TReqRec>> guy in TFriends) //re-initialising to false in every run
                guy.Key.isBiconnectedComponent = false;

            visitNode(0, 1, -1, visited, preOrder, low, TFriends); //DFS (numerating every node)

            foreach (KeyValuePair<TReqRec, List<TReqRec>> guy in TFriends)
            {
                if (guy.Key.value != 0)//dont check guys who already did their transactions //TODO make it working faster and looking better
                {
                    foreach (TReqRec friend in guy.Value)
                    {
                        if (friend.value != 0)//dont check guys who already did their transactions
                        {
                            if (low[friend.tempListPosition] >= preOrder[guy.Key.tempListPosition])
                                guy.Key.isBiconnectedComponent = true;
                        }
                    }
                }
            }
        }

        //OLD ALGORITHM
        private int countAvitableFriends(TReqRec guy, List<TReqRec> friends)
        {
            int temp = 0;

            if (guy.value == 0)
                temp += 1000; // very big number -> guy with value 0 dont have to make any aditional transactions

            foreach (TReqRec rec in friends)
                if (rec.value != 0)
                    temp++;

            if (temp == 0)
                return 666666; //some very big number -> guy with min friends will be next transactor, but guy without friends cannot be transactor
            else if (guy.isBiconnectedComponent)
                return temp + 10000; //for being biconnected component

            return temp;
        }

        private TPenRecord TGenerator(TReqRec zeroingBallanceGuy, TReqRec guy2)//TRANSACTION GENERATOR
        {
            if (zeroingBallanceGuy.value < 0)
                return new TPenRecord(zeroingBallanceGuy.transactionID, zeroingBallanceGuy.userID, guy2.userID, -zeroingBallanceGuy.value, guy2.currency);
            else if (zeroingBallanceGuy.value > 0)
                return new TPenRecord(zeroingBallanceGuy.transactionID, guy2.userID, zeroingBallanceGuy.userID, zeroingBallanceGuy.value, guy2.currency);
            else
                throw new Exception("value of guy in TRANSACTIONS GENERATOR is equal 0");
        }

        //TODO optimize it (its like m*n^2 now!!!)
        //TODO make it looking better (120 lines of code in 1 method ...)
        //this also checks if there is connection between every 2 nodes of graph of friend connections (czy graf jest spójny)
        private List<KeyValuePair<TReqRec, List<TReqRec>>> findTransactionFriends(EzydDB DB) //1=everyone has at least 1 friend in transaction list, 0=at least 1 person has no any friend in transaction list
        {
            // if(transactionRecords.Count == 2) //if both of guys have to accept transaction anyway they dont need friendship
            //    return true;

            if (TRecords.Count < 2)
                throw new Exception("There are less than 2 guys in transaction");

            //list that contains records like (guy + list of his friends)
            List<KeyValuePair<TReqRec, List<TReqRec>>> tempList = new List<KeyValuePair<TReqRec, List<TReqRec>>>();

            //add transactors to the list
            foreach (TReqRec req in TRecords)
                tempList.Add(new KeyValuePair<TReqRec, List<TReqRec>>(req, new List<TReqRec>()));

            //TODO TEST IT (its not tested well)
            /******************************** NEW ALGHORITM  ******************************/
            //dont know if correct but (i hope so) its faster
            /*
             * COMPLEXITY:
             * part 1 - unknown
             * part 2 - overall no of frienships between ppl in transaction * no of users in transaction (almost like n^3) ;/
             *
             */
            /*
            //part 1 - sql query
            var reader = DB.SqlQuery(
            "SELECT `senderID`, `receiverID` " +
            "FROM `friends` " +
            "WHERE " +
                "`senderID` IN " +
                    "(SELECT `userID` " +
                    "FROM `transactions_reqs` " +
                    "WHERE `transactionID` = " + this.transactionID + " ) " +
                "AND " +
                "`receiverID` IN " +
                    "(SELECT `userID` " +
                    "FROM `transactions_reqs` " +
                    "WHERE `transactionID` = " + this.transactionID + " ) " +
                "AND " +
                "`receiverAccepted` = 1 ");

            //part 2 - saving it to list
            var pairOfFriends = new List<KeyValuePair<UInt64, UInt64>>();
            while(reader.Read())
            {
                pairOfFriends.Add(new KeyValuePair<UInt64, UInt64>((UInt64)reader[0], (UInt64)reader[1]));
            }
            foreach (var item in pairOfFriends)
            {
                //TODO hash table would work incredibly fast in here (n instead of n^2) //IMPORTANT TODO!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //find index will last 1, instead of n
                int tempIndex = tempList.FindIndex(r => r.Key.userID == item.Key);
                int tempIndex2 = tempList.FindIndex(r => r.Key.userID == item.Value);
                tempList[tempIndex].Value.Add(tempList[tempIndex2].Key);
                tempList[tempIndex2].Value.Add(tempList[tempIndex].Key);
            }
            */

            /****************************** END OF NEW ALGORITHM  ***************************** */

            /******************************** OLD ALGHORITM (WORKING!!!!!!) ******************************/
            //working but slowly ....

            //* COMPLEXITY:
            //* part 1 - no of users in transaction * 1 fast query
            //* part 2 - n^3?
            //*

            //find friends of everone (add only friends who are on the list)
            foreach (KeyValuePair<TReqRec, List<TReqRec>> pair in tempList)
            {
                DB.SqlQuery("SELECT `senderID`, `receiverID` FROM `friends` "
                            + "WHERE ( `senderID` = " + Convert.ToString(pair.Key.userID)
                            + " OR `receiverID` = " + Convert.ToString(pair.Key.userID)
                            + ") AND `receiverAccepted` = 1"); //TODO po przerobieniu tabel to AND bedzie trzeba usunac

                while (DB.reader.Read()) //write friends(only these who exists in that transaction) to lists
                { //TODO THIS HAS O(n) = n^3 //TRY TO FIX IT
                    int tempFriendIndex;
                    if ((UInt64)DB.reader[0] == pair.Key.userID)
                        tempFriendIndex = tempList.FindIndex(rec => rec.Key.userID == (UInt64)DB.reader[1]); //dont missmatch "rec" and "req" //TODO change that names
                    else
                        tempFriendIndex = tempList.FindIndex(rec => rec.Key.userID == (UInt64)DB.reader[0]);

                    if (tempFriendIndex != -1) //if not on list (FindIndex returns -1 when not found)
                        pair.Value.Add(tempList[tempFriendIndex].Key);
                }
            }
            /***************************** END OF OLD ALGORITHM (WORKING!!!!!) ***************************** */

            /* //its written just few line higher - in much simplier way (and it works - this bellow does not) ;) //TODO REMOVE IT ALL
            //TODO DO SOMETHING WITH IT - its like O(n) = n^3!!!! (transaction guys no * (no of friends of each guy)^2 )
            //it may be possible to do it from SQL - its indexed so it can be much faster(but lists are much bigger)
            //pomocnicza lista
            List<UInt64> transactors = new List<UInt64>();
            foreach (TReqRec req in transactionRecords)
            {
                transactors.Add(req.userID);
            }
            //drops every friend from friends list, who is not in this transaction //TODO N^3 !!!!
            foreach (KeyValuePair<TReqRec, List<TReqRec>> pair in tempList)
            {
                foreach (TReqRec friendRec in pair.Value)
                {
                    if (!transactors.Contains(friendRec.userID))
                        pair.Value.RemoveAll(i => i.userID == friendRec.userID); //LAMBDA EXPRESSION (my 1st one)
                }
            }
            */

            //looking for guys who dont have even 1 friend in transactors
            List<UInt64> guysWithNoFriends = new List<UInt64>(); //;(
            bool guysWithNoFriendsExists = false; //ofc false ;)
            foreach (KeyValuePair<TReqRec, List<TReqRec>> pair in tempList)
            {
                if (pair.Value.Count == 0)
                {
                    guysWithNoFriendsExists = true;
                    guysWithNoFriends.Add(pair.Key.userID);
                }
            }

            if (guysWithNoFriendsExists)
            {
                string temp = "";
                foreach (UInt64 tempID in guysWithNoFriends)
                {
                    temp += Convert.ToString(tempID);
                    temp += " ";
                }
                throw new Exception("guys with no friends exists - they are: " + temp);
            }

            //building connections graph

            //TODO include visited bool to record to dont make this list
            bool[] visited = new bool[tempList.Count]; //information about every user in tempList (ABOUT USER WITH THE SAME NUMBER) - if he was visited by graph search engine
            for (int i = 1; i < visited.Length; i++)
                visited[i] = false; //initialisation of visited list

            visited[0] = true; //go to 1st node

            List<int> frontier = new List<int>(tempList.Count); //40 - random number
            frontier.Add(0);

            while (frontier.Count != 0)
            {
                int temp = frontier.Last();
                frontier.RemoveAt(frontier.Count - 1); //remove last element
                foreach (TReqRec rec in tempList[temp].Value) //for each friend
                {
                    if (visited[rec.tempListPosition] == false) //if guy was not visited
                    {
                        visited[rec.tempListPosition] = true;
                        frontier.Add(rec.tempListPosition);
                    }
                }
            }

            //TODO make exception message telling who is not friend of who and what to do to
            //make possibility to make this transaction in this group (what friends are needed)

            if (visited.Any(placeVisited => placeVisited == false))
            {
                string exceptionString = "people who have no friends in other part of transaction group: ";
                for (int i = 0; i < tempList.Count; i++)
                {
                    if (visited[i] == false)
                    {
                        exceptionString += tempList[i].Key.userID;
                        exceptionString += " ";
                    }
                }
                throw new Exception(exceptionString);
            }

            //IF U REACHED HERE EVERYTHING SHOULD BE OK

            return tempList;
        }
    }

    public class TPenRecord //TRANSACTION PENDING RECORD
    {
        public UInt32 transactionID;
        public UInt64 plusGuyID;
        public UInt64 minusGuyID;
        public int value; //it HAS TO be signed (not like in other tables)
        public String currency;
        public DateTime date;
        public String description;
        public Int16 optimizationSuspended;

        public TPenRecord(UInt32 _transactionID, UInt64 _minusGuyID, UInt64 _plusGuyID, int _value, String _currency, String _description)
        {
            transactionID = _transactionID;
            minusGuyID = _minusGuyID;
            plusGuyID = _plusGuyID;
            value = _value;
            currency = _currency;
            date = DateTime.Now;
            description = _description;
        }

        public TPenRecord(UInt32 _transactionID, UInt64 _minusGuyID, UInt64 _plusGuyID, int _value, String _currency, DateTime _date, String _description = "", Int16 _optimizationSuspended = 0)
        {
            transactionID = _transactionID;
            minusGuyID = _minusGuyID;
            plusGuyID = _plusGuyID;
            value = _value;
            currency = _currency;
            date = _date;
            description = _description;
            optimizationSuspended = _optimizationSuspended;
        }

        public TPenRecord(UInt32 _transactionID, UInt64 _minusGuyID, UInt64 _plusGuyID, int _value, String _currency)
        {
            transactionID = _transactionID;
            minusGuyID = _minusGuyID;
            plusGuyID = _plusGuyID;
            value = _value;
            currency = _currency;
            date = DateTime.Now;
        }

        public String addToDBstring()
        {
            if (description == null || description.Equals(String.Empty))
                return " (" + transactionID + ", " + Convert.ToString(plusGuyID) + ", " + Convert.ToString(minusGuyID)
                    + ", " + Convert.ToString(value) + ", '" + currency + "', NOW(), ' ' ) ";

            return " (" + transactionID + ", " + Convert.ToString(plusGuyID) + ", " + Convert.ToString(minusGuyID)
                + ", " + Convert.ToString(value) + ", '" + currency + "', NOW(), '" + description + "' ) ";
        }
    }

    public class TPenSet //TRANSACTION PENDING SET
    {
        private UInt32 transactionID;
        public List<TPenRecord> transactionRecords = new List<TPenRecord>();

        public TPenSet(EzydDB DB, UInt32 _transactionID)//read transaction from DB
        {
            transactionID = _transactionID;
            DB.SqlQuery("SELECT `transactionID`, `plusGuyID`, `minusGuyId`, `value`, `currency`, `date` "
                + "FROM `transactions_pending` WHERE `transactionID` = " + Convert.ToString(transactionID));
            while (DB.reader.Read())
                addRecord(new TPenRecord((UInt32)DB.reader[0], (UInt64)DB.reader[1],
                    (UInt64)DB.reader[2], (int)DB.reader[3],
                    (String)DB.reader[4], (DateTime)DB.reader[5]
                    ));
        }

        public TPenSet(UInt32 _transactionID)
        {
            transactionID = _transactionID;
        }

        public TPenSet(List<TPenRecord> _transactionRecords)
        {
            transactionRecords = _transactionRecords;

            if (transactionRecords.Count != 0)
                transactionID = transactionRecords[0].transactionID;
            else
                transactionID = 999999987; //random ;]
        }

        public bool addRecord(TPenRecord _record) //0=error, 1=OK
        {
            if (_record.transactionID != transactionID)
                return false;

            transactionRecords.Add(_record);
            return true;
        }

        public void addToDB(EzydDB DB)
        {
            if (DB.transactionExists(Convert.ToString(transactionID)) == 2)
                throw new Exception("transaction already exsists on pending table");

            if (DB.transactionExists(Convert.ToString(transactionID)) == 3)
                throw new Exception("transaction already exists on history");

            String temp = "INSERT INTO `transactions_pending` (`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) VALUES ";

            bool is1st = true;
            bool anythingAdded = false;
            foreach (TPenRecord rec in transactionRecords)
            {
                anythingAdded = true;

                if (!is1st)
                    temp += ", ";
                else
                    is1st = false;

                temp += rec.addToDBstring();
            }

            if (anythingAdded)
                DB.SqlNonQuery(temp);
            //else do nothing
        }
    }

    public class THisRec //TRANSACTION HISTORY RECORD
    {
        public UInt32 transactionID;
        public UInt64 plusGuyID;
        public UInt64 minusGuyID;
        public UInt32 value;
        public String currency;
        public DateTime fulfillDate;

        //MINUSGUYID IS IMPORTANT!!!! (dont remove it - it makes recognising transaction)
        public THisRec(EzydDB DB, UInt32 _transactionID, UInt64 _minusGuyID, UInt64 _plusGuyID) //it ALWAYS gets data from transactions_pending //TODO wyjebać to
        { //TODO optimalize it by finding a way to just move record from pending to history
            transactionID = _transactionID;
            minusGuyID = _minusGuyID;
            plusGuyID = _plusGuyID;

            DB.SqlQuery("SELECT `value`, `currency` FROM `transactions_pending` " +
                " WHERE `transactionID` = " + _transactionID +
                " AND `minusGuyID` = " + _minusGuyID + //to chybe nie potrzebne (no chyba, że ktoś nie lubi, kiedy jego długo są spłacane
                " AND `plusGuyID` = " + _plusGuyID);
            if (DB.reader.Read())
            {
                value = (UInt32)DB.reader[0];
                currency = (String)DB.reader[1];
            }
            else throw new Exception("transaction does not exist so it cannot be moved to history");
        }

        public THisRec(UInt32 _transactionID, UInt64 _plusGuyID, UInt64 _minusGuyID, UInt32 _value, String _currency, DateTime _fulfillDate)
        {
            transactionID = _transactionID;
            plusGuyID = _plusGuyID;
            minusGuyID = _minusGuyID;
            value = _value;
            currency = _currency;
            fulfillDate = _fulfillDate;
        }

        private String addToDBstring() //this 'refactor' is not needed
        {
            return " (" + transactionID + ", " + Convert.ToString(plusGuyID) + ", " + Convert.ToString(minusGuyID)
                + ", " + Convert.ToString(value) + ", '" + currency + "', NOW() ) ";
        }

        public void moveToHistory(EzydDB DB)
        {
            //adding record to history
            String query = "INSERT INTO `transactions_history` " +
                "(`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `fulfillDate`) " +
                "VALUES ";

            query += addToDBstring();

            DB.SqlNonQuery(query);

            //removing record from pending
            query = "DELETE FROM `transactions_pending` " +
                "WHERE " +
                    " `transactionID` = " + Convert.ToString(this.transactionID) +
                    " AND `plusGuyID` = " + Convert.ToString(this.plusGuyID) +
                    " AND `minusGuyID` = " + Convert.ToString(this.minusGuyID);
            DB.SqlNonQuery(query);
        }
    }

    //WELL-WRITTEN
    public static class TOptimalizator //its same optimalization as in TReqSet, but you got different input data (u have to take everything from DB) //and it looks much much better ;]
    {
        //optimization should not be done, when diffrent optimization is working - they can interfeer verry badly //TODO: create sql transactions!
        private static bool optimizationOnboard = false;

        private static List<KeyValuePair<UInt64, List<UInt64>>> findUsersFriends(EzydDB DB, List<UInt64> usersList) //returns list of (every guy in transaction and list of his friends)
        {
            //decalring and filling in output list
            var output = new List<KeyValuePair<UInt64, List<UInt64>>>();
            foreach (var item in usersList)
            {
                output.Add(new KeyValuePair<UInt64, List<UInt64>>(item, new List<UInt64>()));
            }

            //generating string to needed to query
            string userListString = "( ";
            bool is1st = true;
            foreach (var item in usersList)
            {
                if (!is1st)
                {
                    userListString += ",";
                }
                is1st = false;

                userListString += "'" + Convert.ToString(item) + "'";
            }
            userListString += " )";

            /*
             * COMPLEXITY:
             * -unknown
             * probably n*m
             * //TODO: search senderID with condition and later search result with condition on receiverID
             */
            var reader = DB.SqlQuery(
                "SELECT `senderID`, `receiverID` " +
                "FROM `friends` " +
                "WHERE " +
                    "`senderID` IN " + userListString +
                    "AND " +
                    "`receiverID` IN " + userListString +
                    "AND " +
                    "`receiverAccepted` = 1 ");

            /*
             * COMPLEXITY:
             * n^2
             */
            while (reader.Read())
            {
                output.Find(r => r.Key == (UInt64)reader[0]).Value
                    .Add((UInt64)reader[1]);
                output.Find(r => r.Key == (UInt64)reader[1]).Value
                    .Add((UInt64)reader[0]);
            }

            return output;
        }

        private static List<TPenRecord> findUsersTransactions(EzydDB DB, List<UInt64> usersList, string currency = null) //returns list of transactions between guys on list //if currency is defined it looks only for thansactions with specified currency
        {
            var output = new List<TPenRecord>();

            string userListString = "( "; //TODO its done 2nd time in here //unneceserly //FIX IT
            bool is1st = true;
            foreach (var item in usersList)
            {
                if (!is1st)
                {
                    userListString += ",";
                }
                is1st = false;

                userListString += "'" + Convert.ToString(item) + "'";
            }
            userListString += " )";

            MySql.Data.MySqlClient.MySqlDataReader reader;
            if (currency == null)
            {
                reader = DB.SqlQuery(
                    "SELECT * " +
                    "FROM `transactions_pending` " +
                    "WHERE " +
                        "`plusGuyID` IN " + userListString +
                        " AND " +
                        "`minusGuyID` IN " + userListString +
                        " AND " +
                        " `optimizationSuspended` = 0");
            }
            else
            {
                reader = DB.SqlQuery(
                    "SELECT * " +
                    "FROM `transactions_pending` " +
                    "WHERE " +
                        "`plusGuyID` IN " + userListString +
                        " AND " +
                        "`minusGuyID` IN " + userListString +
                        " AND " +
                        "`currency` = '" + currency + "'" +
                        " AND " +
                        " `optimizationSuspended` = 0");
            }

            while (reader.Read())
            {
                output.Add(
                    new TPenRecord((UInt32)DB.reader[0], (UInt64)DB.reader[2], //TODO o huj chodzi z tą kolejnością - zmienić to
                    (UInt64)DB.reader[1], (int)((UInt32)DB.reader[3]), //WTF?? //TODO zły typ w transaction pending (niby ...) nie wiem co z tym zrobić - może kiedyś
                    (String)DB.reader[4], (DateTime)DB.reader[5]
                    ));
            }

            return output;
        }

        private static bool biangleOptimization(List<KeyValuePair<ulong, List<ulong>>> friends, List<TPenRecord> transactions) //looks for A->B, A->B OR A->B, B->A and optimizes it
        {
            foreach (TPenRecord rec in transactions)
            {
                List<TPenRecord> tempRecsList = transactions.FindAll(
                        rec2 =>
                        (rec2 != rec) && (
                            (rec2.minusGuyID == rec.minusGuyID) && (rec2.plusGuyID == rec.plusGuyID) //possibility 1
                            ||
                            (rec2.minusGuyID == rec.plusGuyID) && (rec2.plusGuyID == rec.minusGuyID) //possibility 2
                        )
                    );

                foreach (var tempRec in tempRecsList)
                {
                    //check - if they are not friends optimization cant be done
                    if (!friends.Find(r => r.Key == rec.minusGuyID).Value.Any(r => r == rec.plusGuyID))
                        continue;

                    if (tempRec != null)
                    {
                        if (tempRec.minusGuyID == rec.minusGuyID) //possibility 1 (A->B, A->B -----> A->B)
                        {
                            if (rec.value + tempRec.value != 0) //so 0 value wont be added
                                transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), rec.minusGuyID, rec.plusGuyID, (rec.value + tempRec.value), rec.currency));
                        }
                        else //possibility 2 (A->B, B->A ------> A->B or B->A
                            if (rec.value != tempRec.value) //so 0 value wont be added (watch added values formulas)
                            {
                                if (rec.value > tempRec.value)
                                    transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), rec.minusGuyID, rec.plusGuyID, (rec.value - tempRec.value), rec.currency));
                                else
                                    transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), tempRec.minusGuyID, tempRec.plusGuyID, (tempRec.value - rec.value), rec.currency));
                            }

                        //removing old transactions;
                        transactions.Remove(rec);
                        transactions.Remove(tempRec);

                        //to not check anything else in foreach
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool triangleOptimization(List<KeyValuePair<ulong, List<ulong>>> friends, List<TPenRecord> transactions) //looks for A->B, B->C and optimizes it
        {
            foreach (TPenRecord rec in transactions)
            {
                List<TPenRecord> tempTransactions = transactions.FindAll(rec2 => rec2.plusGuyID == rec.minusGuyID);
                foreach (var transaction in tempTransactions)
                {
                    if (transaction != null) //if found something to optimalize
                    {
                        //A->B, B->C
                        ulong guyA = transaction.minusGuyID;
                        ulong guyB = transaction.plusGuyID;
                        ulong guyC = rec.plusGuyID;

                        //+ zero values wont be added in here anyway (so don't try to fix it again ;] )
                        if (friends.Find(r => r.Key == guyA).Value.Any(r => r == guyC))//if A and C are friends
                        {
                            if (rec.value == transaction.value)
                            { //then make A->C of it
                                transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), guyA, guyC, rec.value, rec.currency));
                            }
                            else if (rec.value > transaction.value) //if A debt to B is bigger than B debt to C
                            { //then make A->B, A->C of it
                                transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), guyA, guyC, transaction.value, rec.currency)); //A gives part if B debt to C
                                transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), guyB, guyC, (rec.value - transaction.value), rec.currency));
                            }
                            else//if A debt to B is smaller than B debt to C
                            { //then make A->C, B->C of it //
                                transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), guyA, guyB, (transaction.value - rec.value), rec.currency));
                                transactions.Add(new TPenRecord(TransactionIdProvider.getTransactionId(), guyA, guyC, rec.value, rec.currency)); //A gives all B debt to C
                            }

                            //removing old transactions
                            transactions.Remove(rec);
                            transactions.Remove(transaction);

                            //to not check anything else in foreach
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static void updateDBTransactions(EzydDB DB, List<TPenRecord> transactions, List<TPenRecord> oldTransactions)//it removes differences between DB and set -  removes old transactions, add new, leaves not-changed ones
        {
            //removing old transactions from transactions list
            for (int i = 0; i < transactions.Count; i++)
            {
                var temp = oldTransactions.Find(r => r.minusGuyID == transactions[i].minusGuyID
                                                  && r.plusGuyID == transactions[i].plusGuyID
                                                  && r.value == transactions[i].value); //so its like the same transaction
                if (temp != null) //if nothing has changed - remove transaction from both lists
                {
                    if (!transactions.Remove(temp))
                        throw new Exception("transaction cannot be removed, becouse it does not exists");

                    if (!oldTransactions.Remove(temp))
                        throw new Exception("transaction cannot be removed, becouse it does not exists (2)");
                }
            }

            //adding new transactions
            var transactionsSet = new TPenSet(transactions);
            transactionsSet.addToDB(DB);

            //removing old transacitons
            foreach (var item in oldTransactions)
            {
                DB.SqlNonQuery(
                    "DELETE FROM `transactions_pending` " +
                    "WHERE " +
                        " `transactionID` = " + item.transactionID +
                        " AND " +
                        " `minusGuyId` = " + item.minusGuyID +
                        " AND " +
                        " `plusGuyID` = " + item.plusGuyID);
            }
        }

        /// <summary>
        /// DO NEVER USE THAT FOO - there is transactionsSet.addToDB(DB) method
        /// </summary>
        /// <param name="DB"></param>
        /// <param name="transactions"></param>
        [Obsolete]
        private static void addTransactionsToDB(EzydDB DB, List<TPenRecord> transactions)//it just adds new transactions
        {
            
            //adding new transactions
            string addQuery = "INSERT INTO `transactions_pending` " +
                " (`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) " +
                " VALUES";
            for (int i = 0; i < transactions.Count; i++)
            {
                addQuery += transactions[i].addToDBstring();
                if (i != transactions.Count - 1)
                    addQuery += ", ";
            }
            DB.SqlNonQuery(addQuery);
        }

        private static void optimize(EzydDB DB, List<KeyValuePair<UInt64, List<UInt64>>> friends, List<TPenRecord> transactions, List<TPenRecord> transactionsInDB) //optimize transactions and write changes to DB
        {
            bool someOptimizationDone = true; //its bit fat lie right now ;( //but it works ;]
            int noOfOptimizationsDone = 0;

            while (someOptimizationDone)
            {
                someOptimizationDone = false;
                if (biangleOptimization(friends, transactions))
                {
                    someOptimizationDone = true;
                    noOfOptimizationsDone++;
                    continue;
                }

                if (triangleOptimization(friends, transactions))
                {
                    someOptimizationDone = true;
                    noOfOptimizationsDone++;
                    continue;
                }
            }

            updateDBTransactions(DB, transactions, transactionsInDB);
        }

        //its similar to optimization from TReqs
        //in this optimization transactions are already added to DB
        //version for optimalizations view
        public static void optimizeTransactionsAndApply(EzydDB DB, List<UInt64> usersList)
        {
            //optimization should not be done, when diffrent optimization is working - they can interfeer verry badly //it is also used in overload of this foo!!!
            while (optimizationOnboard)
                Thread.Sleep(1);
            optimizationOnboard = true;                

            if (usersList.Count < 2)
                throw new Exception("you need to add more than 1 user");

            List<KeyValuePair<UInt64, List<UInt64>>> friends = findUsersFriends(DB, usersList);

            if (friends.All(r => r.Value.Count == 0))
                throw new Exception("noone has friends in this group");

            List<TPenRecord> transactions = findUsersTransactions(DB, usersList);

            if (transactions.Count == 0)
                throw new Exception("there are no transactions between people in this group");

            //doing optimization for every currency
            while (transactions.Count != 0)
            {
                var specCurrencyTransactions = new List<TPenRecord>(); //specifiedCurrencyTransactions

                //choosing currency as transaction[0] currency and moving it to specCurrencyTransactions
                specCurrencyTransactions.Add(transactions[0]);
                transactions.RemoveAt(0);

                for (int i = 0; i < transactions.Count; i++)
                {
                    if (transactions[i].currency == specCurrencyTransactions[0].currency)
                    {
                        specCurrencyTransactions.Add(transactions[i]);
                        transactions.RemoveAt(i);
                        i--;
                    }
                }

                //all of these transactions are in DB
                var transactionsInDB = new List<TPenRecord>(specCurrencyTransactions.Count);
                transactionsInDB.AddRange(specCurrencyTransactions);

                optimize(DB, friends, specCurrencyTransactions, transactionsInDB); //optimize transactions and write changes to DB

                //let other optimization threads work
                optimizationOnboard = false;
            }
        }

        //version for adding new transactions (different data on input) //+ these transactions are not in DB
        public static void optimizeTransactionsAndApply(EzydDB DB, TPenSet set, List<KeyValuePair<TReqRec, List<TReqRec>>> TFriends)
        {
            //optimization should not be done, when diffrent optimization is working - they can interfeer verry badly //it is also used in overload of this foo!!!
            while (optimizationOnboard)
                Thread.Sleep(1);
            optimizationOnboard = true; 

            //getting friends from TFriends
            var friends = new List<KeyValuePair<UInt64, List<UInt64>>>();
            foreach (var item in TFriends)
            {
                friends.Add(new KeyValuePair<UInt64, List<UInt64>>(item.Key.userID, new List<UInt64>()));
                foreach (var friend in item.Value)
                    friends[friends.Count - 1].Value.Add(friend.userID);
            }
            if (friends.All(r => r.Value.Count == 0))
                throw new Exception("noone has friends in this group");

            //creating user list //TODO its unnecessery - refactor it in some way
            var userList = new List<ulong>(friends.Count);
            foreach (var item in friends)
                userList.Add(item.Key);

            //getting old transactions from DB
            List<TPenRecord> transactionsInDB = findUsersTransactions(DB, userList, set.transactionRecords[0].currency); //selectts transaction with specified currency

            var allTransactions = new List<TPenRecord>(); //new transactions to add

            //adding new transactions (from set)
            foreach (var item in set.transactionRecords)
                allTransactions.Add(item);

            //adding transactions from DB to all transactions
            allTransactions.AddRange(transactionsInDB);

            //optimize transactions and write changes to DB
            optimize(DB, friends, allTransactions, transactionsInDB);

            //let other optimization threads work
            optimizationOnboard = false;
        }
    }
}