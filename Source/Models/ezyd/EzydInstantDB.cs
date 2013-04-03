using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ezyd.Models.ezyd
{
    public class EzydInstantDB : EzydDB
    {
        public EzydInstantDB()
            : base("localhost", "ezyd", "root", "1maciek")
        {
            //empty contructor - its just simplifying creating DB object
        }
    }
}