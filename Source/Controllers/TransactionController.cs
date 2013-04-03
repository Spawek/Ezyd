using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ezyd.Models;
using ezyd.Models.ezyd;
using Facebook;
using Facebook.Web;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Security;
using System.Web.Helpers;
using ezyd.Models.stringHelper;
using ezyd.Models.helpers;


namespace ezyd.Controllers
{
    [HandleError]
    public class TransactionController : Controller
    {
        public FacebookSession FacebookSession
        {
            get { return (new CanvasAuthorizer().Session); }
        }

        public class FacebookUser
        {
            public long id { get; set; } //yes. the user id is of type long...dont use int
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string name { get; set; }
            public string email { get; set; }
        }

        [HttpGet]
        [ActionName("handshake")]
        public ActionResult Handshake(string code)
        {
            //after authentication, Facebook will redirect to this controller action with a QueryString parameter called "code" (this is Facebook's Session key)

            //example uri: http://www.borrowedgames.com/facebook/handshake/?code=2.DQUGad7_kFVGqKTeGUqQTQ__.3600.1273809600-1756053625|dil1rmAUjgbViM_GQutw-PEgPIg.

            //this is your Facebook App ID
            string clientId = "241822175888190";

            //this is your Secret Key
            string clientSecret = "a03f56d45568b6ebc9eb6c4503090e00";

            //we have to request an access token from the following Uri
            string url = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";

            //your redirect uri must be EXACTLY the same Uri that caused the initial authentication handshake
            string redirectUri = "http://www.borrowedgames.com/facebook/handshake/";

            //Create a webrequest to perform the request against the Uri
            WebRequest request = WebRequest.Create(string.Format(url, clientId, redirectUri, clientSecret, code));

            //read out the response as a utf-8 encoding and parse out the access_token
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader streamReader = new StreamReader(stream, encode);
            string accessToken = streamReader.ReadToEnd().Replace("access_token=", "");
            streamReader.Close();
            response.Close();

            //set the access token to some session variable so it can be used through out the session
            Session["FacebookAccessToken"] = accessToken;

            //now that we have an access token, query the Graph Api for the JSON representation of the User
            url = "https://graph.facebook.com/me?access_token={0}";

            //create the request to https://graph.facebook.com/me
            request = WebRequest.Create(string.Format(url, accessToken));

            //Get the response
            response = request.GetResponse();

            //Get the response stream
            stream = response.GetResponseStream();

            //Take our statically typed representation of the JSON User and deserialize the response stream
            //using the DataContractJsonSerializer
            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof(FacebookUser));
            FacebookUser user = new FacebookUser();
            user = dataContractJsonSerializer.ReadObject(stream) as FacebookUser;

            //close the stream
            response.Close();

            //capture the UserId
            Session["FacebookUserId"] = user.id;

            //Set the forms authentication auth cookie
            FormsAuthentication.SetAuthCookie(user.email, false);

            //redirect to home page so that user can start using your application      
            return RedirectToAction("Home", "User");
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult transactionCreator(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");
                ViewData["UserID"] = result.id;
                ViewData["UserName"] = result.name;
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return acceptedTransactionsHistory(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return acceptedTransactionsHistory(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult addTransaction(string[] ids, string[] values, string currency, string[] descs, string transactionName) //descs = descriptions //max 10 users in transaction
        {
            string acceptance = "1";
            string errorMsg = null;

            try
            {
                //TODO check integrality of input data here

                if (ids.Length > 10)
                    throw new Exception("there can't be more than 10 users in transaction");

                for (int i = 0; i < descs.Length; i++)
                {
                    BadCharsRemover.RemoveBadChars(ref descs[i]);
                    BadCharsRemover.RemoveBadChars(ref ids[i]); 
                    BadCharsRemover.RemoveBadChars(ref values[i]);
                }
                BadCharsRemover.RemoveBadChars(ref currency);
                BadCharsRemover.RemoveBadChars(ref transactionName);

                var transactionsSet = new TReqSet(TransactionIdProvider.getTransactionId(), ids, values, currency, descs, transactionName);
                transactionsSet.addToDB(new EzydInstantDB());
            }
            catch (Exception e)
            {
                acceptance = "0";
                errorMsg = e.Message;
            }
            
            return Json(new
            {
                accepted = acceptance,
                error = errorMsg
            });
        }

        public ActionResult transactionRequests(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");
                ViewData["UserID"] = Convert.ToUInt64(result.id);

                var DB = new EzydInstantDB();
                var reader = DB.SqlQuery("SELECT * " +
                    "FROM `transactions_reqs` " +
                    "WHERE `transactionID` IN ( " +
	                    "SELECT `transactionID` " +
	                    "FROM `transactions_reqs` " +
                        "WHERE `userID` = " + result.id + ")"
                    );
                var TReqs = new List<TReqRec>();

                while (reader.Read())
                    TReqs.Add(new TReqRec((UInt32)reader[0], 
                        (UInt64)reader[1], (int)reader[2], 
                        (String)reader[3], (DateTime)reader[4], 
                        (Int16)reader[5], (String)reader[6]));

                ViewData["TReqs"] = TReqs;
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return transactionRequests(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return transactionRequests(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        public ActionResult transactionCancelled(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");
                ViewData["UserID"] = Convert.ToUInt64(result.id);

                var DB = new EzydInstantDB();
                var reader = DB.SqlQuery("SELECT * " +
                    "FROM `transactions_cancelled` " +
                    "WHERE `transactionID` IN ( " +
                        "SELECT `transactionID` " +
                        "FROM `transactions_cancelled` " +
                        "WHERE `userID` = " + result.id + ")"
                    );
                var TReqs = new List<TReqRec>();

                while (reader.Read())
                    TReqs.Add(new TReqRec((UInt32)reader[0],
                        (UInt64)reader[1], (int)reader[2],
                        (String)reader[3], (DateTime)reader[4],
                        (Int16)reader[5], (String)reader[6]));

                ViewData["TReqs"] = TReqs;
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return transactionRequests(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return transactionRequests(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        public ActionResult acceptedTransactionsHistory(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");
                ViewData["UserID"] = Convert.ToUInt64(result.id);

                var DB = new EzydInstantDB();
                var reader = DB.SqlQuery("SELECT * " +
                    "FROM `transactions_accepted_history` " +
                    "WHERE `transactionID` IN ( " +
                        "SELECT `transactionID` " +
                        "FROM `transactions_accepted_history` " +
                        "WHERE `userID` = " + result.id + ")"
                    );
                var TReqs = new List<TReqRec>();

                while (reader.Read())
                    TReqs.Add(new TReqRec((UInt32)reader[0],
                        (UInt64)reader[1], (int)reader[2],
                        (String)reader[3], (DateTime)reader[4],
                        1, (String)reader[6])); //its always accepted

                ViewData["TReqs"] = TReqs;
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return acceptedTransactionsHistory(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return acceptedTransactionsHistory(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();

        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult acceptTranaction(string TId) //transactionID //for security cases i get uID from FB
        {
            BadCharsRemover.RemoveBadChars(ref TId);

            var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
            dynamic result = fbApp.Get("me");
            
            var DB = new EzydInstantDB();
            try 
	        {
                DB.acceptTransaction(Convert.ToUInt32(TId), Convert.ToUInt64(result.id));
            }
	        catch (Exception e)
	        {
                return Json (new {
                    ok = 0,
                    msg = e.Message
	             });
            }

            return Json (new {
                ok = 1
            });
        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult moveToPending(string TId)//try(!!!) to move to pending (if it wasnt auto-moved)
        {
            try 
	        {
                BadCharsRemover.RemoveBadChars(ref TId);

                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");

                var DB = new EzydInstantDB();

                var set = new TReqSet(DB, Convert.ToUInt32(TId));
                set.moveToPending(DB);
            }
	        catch (Exception e)
	        {
                return Json (new {
                    ok = 0,
                    msg = e.Message
	             });
            }

            return Json (new {
                ok = 1
            });

        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult cancelTransaction(string TId)
        {
            try 
	        {
                BadCharsRemover.RemoveBadChars(ref TId);

                var DB = new EzydInstantDB();

                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");

                var set = new TReqSet(DB, Convert.ToUInt32(TId));
                set.moveToCancelled(DB, Convert.ToUInt64(result.id));
            }
	        catch (Exception e)
	        {
                return Json (new {
                    ok = 0,
                    msg = e.Message
	             });
            }

            return Json(new
            {
                ok = 1
            });
        }

        public ActionResult transactionPending(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");
                ViewData["UserID"] = Convert.ToUInt64(result.id);
                UInt64 UId = Convert.ToUInt64(result.id);

                var DB = new EzydInstantDB();
                var reader = DB.SqlQuery("SELECT * " +
	                " FROM `transactions_pending` " +
	                " WHERE " +
		                " `plusGuyID` = " + result.id +
		                " OR " +
		                " `minusGuyID` = " + result.id 
                    );

                var TPenRecsMinus = new List<TPenRecord>();
                var TPenRecsPlus = new List<TPenRecord>();
                while (reader.Read())
                {
                    if ((UInt64)reader[2] == UId)
                    {
                        TPenRecsMinus.Add(new TPenRecord((UInt32)reader[0],
                            UId, (UInt64)reader[1], (int)reader[3],
                            (String)reader[4], (DateTime)reader[5], 
                            (String)reader[6], (Int16) reader[7])
                        );
                    }
                    else
                    {
                        TPenRecsPlus.Add(new TPenRecord((UInt32)reader[0],
                            (UInt64)reader[2], UId, (int)reader[3],
                            (String)reader[4], (DateTime)reader[5], 
                            (String)reader[6], (Int16) reader[7])
                        );
                    }
                }
                ViewData["TPenRecsMinus"] = TPenRecsMinus;
                ViewData["TPenRecsPlus"] = TPenRecsPlus;
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return transactionPending(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return transactionPending(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult approvePayment(string TId, string minusGuyID)
        {
            try
            {
                BadCharsRemover.RemoveBadChars(ref TId);
                BadCharsRemover.RemoveBadChars(ref minusGuyID);

                var DB = new EzydInstantDB();

                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");

                var historyRec = new THisRec(DB, Convert.ToUInt32(TId), Convert.ToUInt64(minusGuyID), Convert.ToUInt64(result.id));
                historyRec.moveToHistory(DB);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    ok = 0,
                    msg = e.Message
                });
            }

            return Json(new
            {
                ok = 1
            });

        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult suspendTransaction(string TId, string minusGuyID, string plusGuyID)
        {
            try
            {
                BadCharsRemover.RemoveBadChars(ref TId);
                BadCharsRemover.RemoveBadChars(ref minusGuyID);
                BadCharsRemover.RemoveBadChars(ref plusGuyID);

                var DB = new EzydInstantDB();

                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");

                if (result.id != minusGuyID && result.id == plusGuyID)
                    throw new Exception("you are not part of this transaction, so you cannot suspend it");

                DB.SqlNonQuery("UPDATE `transactions_pending` " +
                    " SET `optimizationSuspended` = 1 " +
                    " WHERE `transactionID` = " + TId +
                        " AND " +
                        " `plusGuyID` = " + plusGuyID +
                        " AND " +
                        " `minusGuyID` = " + minusGuyID);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    ok = 0,
                    msg = e.Message
                });
            }

            return Json(new
            {
                ok = 1
            });
        }

        [AjaxOnly]
        [HttpPost]
        public ActionResult resumeTransactionOptimization(string TId, string minusGuyID, string plusGuyID)
        {
            try
            {
                BadCharsRemover.RemoveBadChars(ref TId);
                BadCharsRemover.RemoveBadChars(ref minusGuyID);
                BadCharsRemover.RemoveBadChars(ref plusGuyID);

                var DB = new EzydInstantDB();

                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");

                if (result.id != minusGuyID && result.id == plusGuyID)
                    throw new Exception("you are not part of this transaction, so you cannot suspend it");

                DB.SqlNonQuery("UPDATE `transactions_pending` " +
                    " SET `optimizationSuspended` = 0 " +
                    " WHERE `transactionID` = " + TId +
                        " AND " +
                        " `plusGuyID` = " + plusGuyID +
                        " AND " +
                        " `minusGuyID` = " + minusGuyID);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    ok = 0,
                    msg = e.Message
                });
            }

            return Json(new
            {
                ok = 1
            });
        }

        public ActionResult paymentHistory(int tries = 0) //TODO naprawić syf w nazwach
        {
            try
            {
                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");
                ViewData["UserID"] = Convert.ToUInt64(result.id);
                UInt64 UId = Convert.ToUInt64(result.id);

                var DB = new EzydInstantDB();
                var reader = DB.SqlQuery(" SELECT * " +
                    " FROM `transactions_history` " +
                    " WHERE " +
                        " `plusGuyID` = " + result.id +
                        " OR " +
                        " `minusGuyID` = " + result.id
                    );

                var THisList = new List<THisRec>();
                while (reader.Read())
                {
                    THisList.Add(new THisRec((UInt32)reader[0], 
                        (UInt64)reader[1], (UInt64)reader[2], 
                        (UInt32)reader[3], (String)reader[4], 
                        (DateTime)reader[6]));
                }
                ViewData["THisList"] = THisList;
            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return paymentHistory(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return paymentHistory(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();

        }

        public ActionResult transactionOptimizator(int tries = 0)
        {
            try
            {
                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);
                dynamic result = fbApp.Get("me");
                ViewData["UserID"] = result.id;
                ViewData["UserName"] = result.name;

            }
            catch (NullReferenceException)
            {
                if (tries < 5) //5 tries
                {
                    return transactionRequests(tries + 1);
                }
                return View("uNotLogged");
            }
            catch (ArgumentNullException)
            {
                if (tries < 5)
                {
                    return transactionRequests(tries + 1);
                }
                return View("uNotLogged");
            }

            return View();
        }

        [AjaxOnly]
        [HttpPost]
        //max 5 users
        public ActionResult optimizeTransactions(string [] ids) //this user has to be in optimized transaction to be able to perform it //it should have captcha too 
        {
            try
            {
                for (int i = 0; i < ids.Length; i++)
                    BadCharsRemover.RemoveBadChars(ref ids[i]);

                var fbApp = new FacebookClient(this.FacebookSession.AccessToken);

                dynamic result = fbApp.Get("me");
                var usersList = new List<UInt64>(ids.Length);
                foreach (var item in ids)
                    usersList.Add(Convert.ToUInt64(item));

                if (!usersList.Any(r => r == Convert.ToUInt64(result.id)))
                    throw new Exception("you need to be in group of people whose transactions you want to optimize");

                TOptimalizator.optimizeTransactionsAndApply(new EzydInstantDB(), usersList);
            }
	        catch (Exception e)
	        {
                return Json (new {
                    ok = 0,
                    msg = e.Message
	             });
            }
            return Json (new {
                ok = 1
            });

        }

    }
}
