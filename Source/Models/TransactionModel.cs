using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ezyd.Models
{
    public class TransactionData
    {
        List<KeyValuePair<string, int>> data;

        public TransactionData(string[] idArr, string[] valueArr)
        {
            data = new List<KeyValuePair<string, int>>(idArr.Length);
            for (int i = 0; i < idArr.Length; i++)
            {
                data.Add(new KeyValuePair<string, int>(idArr[i], Convert.ToInt32(valueArr[i])));
            }
        }
    }

    public class TransactionModel
    {

    }
}