using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ezyd.Models
{
    public class User
    {
        public UInt64 userID { get; set; }
        public string firstname { get; set; }
        public string surname{ get; set; }
    }

    public class Friend
    {
        UInt64 senderID { get; set; }
        UInt64 receiverID { get; set; }
        bool receiverAccepted { get; set; }
    }

    public class TransactionRequest
    {
        UInt64 transactionID { get; set; }
        UInt64 userID { get; set; }
        int value { get; set; } //signed!!
        string currency { get; set; }
        DateTime date { get; set; }
        bool accepted { get; set; }
        string description { get; set; } //description
    }

    public class TransactionPending
    {
        UInt64 transactionID { get; set; } //64 for sure??
        UInt64 plusGuyID { get; set; }
        UInt64 minusGuyID { get; set; }
        UInt32 value { get; set; }
        string currency { get; set; }
        DateTime date { get; set; }
        string description { get; set; } //description
    }


    public class ezydDB : DbContext
    {
        public DbSet<User> userList { get; set; }
        public DbSet<Friend> friendsList { get; set; }
        public DbSet<TransactionRequest> transactionsRequests{ get; set; }
        public DbSet<TransactionPending> transactionsPending { get; set; }

    }
}