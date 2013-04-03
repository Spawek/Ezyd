using System.Configuration;


namespace ezyd.Controllers
{
    using System.Web.Mvc;
    using Facebook;
    using Facebook.Web;
    using Facebook.Web.Mvc;
    using System.Net;
    using System.Text;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Web.Security;
    using System.Text.RegularExpressions;
    using System.Security.Cryptography;
    using System.Dynamic;
    using ezyd.Models;
    using System.Web.Script.Serialization;
    using System.Collections.Specialized;
    using ezyd.Models.ezyd;

    /*
     * BUGS:
     *
	 * - polskie komunikaty JS nie wyświetlają się prawidłowo - są jakieś &4328 (na przykład) za miast niektórych liter - nie działa np ó
	 *
     * - transaction request wywala jakis JS error, transaction cancelled też i to chyba ten sam err
     * 
     * - transaction no is old max + 2, whats more checking if its possible to add is done twice (why???)
     *      on transactions pending adding after optimization its +1 ! - its about adding new transactions
     */

    /*
     * NOTES:
     * 
     * - tests are not working when logging DBqueries is turned on (filepath is working only on server)
     * 
     * - możliwe, że sposób w jaki jest zrobiony multilang jest strasznie wolny bo wywołuje jakąś funkcję od stringa, zamiast od razu wziąć potrzebne resource - w razie potrzeby można to w miarę łatwo zmodyfikować
     * 
     * - tutorial to samouczek???? - jak tak to przerobić tłumaczenia
     * 
     */


    /*
     * TODO:
     * 
     *   IMPORTANT:
     *   
     * - wywalić kolumne "accepted" z accepted(???) transaction history (przynajmniej z wyświetlania)
     *   
     * - zmienić koncepcje działania programu - transactions pending to będzie jakiś user ballance - wpłaty poprzez money transfer - w praktyce to tak wychodzi
     * 
     * - create sql transactions for optimiztion,
     *   
     * - dodać grupy userów, do wprowadzania transakcji, tak, żeby np dało się łatwo dodać tych samych ludzi co ostatnio, albo stworzyć samemu taką grupę
     *
     * - dodać obrazki do transactions optimization tutorial
     * 
     * - dodać możliwość odrzucenia zaproszenia do listy przyjaciół
     * 
     * - dodać suspend transaction do tutoriala //ale chyba nie to nie będzie używane
     * 
     * - w ogóle poprawić, powiększyć tutorial (ale za duży też nie powinien być)
     * 
     * - skrócić nazwy we views (display, display - WTF?)
     * 
     * - zapisywać kto wcisnął transaction cancel (ew kiedyś można dodać description dlaczego cancel)
     * 
     * - usunąć przycisk delete z transactions reqs kiedy nie ma żadnych transakcji
     *    
     * - ogarnąć indeksy w transactions_pending ????
     * 
     * - transactions reqs się strasnzie długo otwiera - ogarnąć dlaczego (w sumie to może i nie - sprawdzić to)
     * 
     * - dodać możliwość zapisania numeru konta w programie i podawania go dłużnikom
     * 
     * - dodać pobieranie imienia i nazwiska i wypisywanie go w komunikacie na górze (pobieranie przez JS)
     * 
     * - pokazywać kto usunął transakcję (dodawać jak transaction name?)
     * 
     * - w pending data nie ma żadnego sensu - z UI wywaliłem - trzeba jeszcze z DB wywalić
     * 
     * - usunąć z bazy pending description (trochę będzie z tym zabawy) - nie zapomnieć zrobić kopii WSZYSTKIEGO bezpośrednio przed tym
     * 
     * - przetłumaczyć komunikaty błędów
     * 
     * - zabezpieczyc lepiej baze danych
     * 
     * - kasować transakcje z value 0 - najlepiej nie dodawać takich transakcji w ogóle (do pending) //do req można w sumie dawać
     * 
     * - zmienić kolejność plusGuyID, minusGuyID w konstruktorze - o HUJ Z TYM CHODZI, ŻE JEST ODWROTNIE????
     *  
     * - zmienić wersję VS na express
     * 
     * - poprawić tabelki bo widać jakieś linie dziwne - nazwa transakcji też wygląda dziwnie
     * 
     * - dorobić wysyłanie błędów JS (jakichś exceptionów) do servera przez httpPost, a potem zapisywanie ich w jakimś logu
     * 
     * - naprawić syf w nazwach historii (payment history w web to transactions_history w DB)
     * 
     * - dodać captche po dodawaniu transakcji
     * 
     * - zapisywac imie i nazwisko usera - kiedy ktoś skasuje konto nie będzie widać jego długów (tzn nie będzie wiadomo jak ma na imie - będzie tylko user ID)
     * 
     * - zastanowić się, czy całej optymalizacji nie robić na słownikach
     * 
     * - przerobić DataBase na statyczną klasę (singleton?), żeby to się nie łączyło przy każdym zapytaniu
     * 
     */

    /* not important TODO
     *      
     * - kupić certyfikat ssl (nawet 10$/rok się da)
     * 
     * - kupić serwer (~100$/rok)
     *  
     * - poprawić komunikaty błędów (nie jest źle - nie pali się)
     * 
     * - dorobić datę akceptacji transakcji w DB (i jej wyświetlanie ...) //czy napewno ???
     * 
     * - zrobić tutorial - inne metody wprowadzania transakcji
     * 
     * - napisać w helpie, że przy equal_chagre można 1 osobę 2 razy wpisać i jak to wtedy działa (po prostu taka jakby smuje jej koszty) + naprawić description wtedy jak działa, bo chyba nie działa ( w sumie to najpiejw to naprawić ;] )
     * 
     * - zastanowić się nad zoptymalizowaniem wyszukiwania friendów w transakcji (teraz jest n^3 + w dużo zapytań)
     * 
     * - drop down list od waluty zajmuje chyba z 50 linii (żeby się nie zmieniało przy zmianie displaya) //FIX IT 
     * 
     */
    [HandleError]
    public class HomeController : Controller
    {
        public FacebookSession FacebookSession
        {
            get { return (new CanvasAuthorizer().Session); }
        }
        
        public ActionResult Index()
        {
            //TODO: is it needed or just old test?
            var settings = ConfigurationManager.GetSection("facebookSettings");
            var current = settings as IFacebookApplication;
            ViewData["appID"] = current.AppId;
            ViewData["canvasPage"] = current.CanvasPage;
            ViewData["canvasURL"] = current.CanvasUrl;
            ViewData["siteURL"] = current.SiteUrl;
            ViewData["isSecureConn"] = current.IsSecureConnection;

            return View();
        }

        //this is the statically typed representation of the JSON object that will get returned from: https://graph.facebook.com/me
        public class FacebookUser
        {
            public long id { get; set; } //yes. the user id is of type long...dont use int
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string name { get; set; }
            public string email { get; set; }
        }

        //this controller action will be called when Facebook redirects
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

        /* new one (mine)
        [CanvasAuthorize(Permissions = "user_about_me")]
        public ActionResult About()
        {
            CanvasAuthorizer auth = new CanvasAuthorizer(); //WHAT IS IT??? //TODO check it
            FacebookWebClient client = new FacebookWebClient(auth.FacebookWebRequest.Session.AccessToken);

            dynamic result = client.Get("me");

            ViewData["Firstname"] = result.first_name;
            ViewData["Lastname"] = result.last_name;


            return View();
        }
        */

        public class FacebookLoginModel
        {
            public string uid { get; set; }
            public string accessToken { get; set; }
        }

        [HttpPost]
        public JsonResult FacebookLogin(FacebookLoginModel model)
        {
            Session["uid"] = model.uid;
            Session["accessToken"] = model.accessToken;

            return Json(new { success = true });
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

    }
}
