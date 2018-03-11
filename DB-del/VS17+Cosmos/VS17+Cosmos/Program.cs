    using System;
using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Common;



namespace VS17_Cosmos
{
    

    public class Program
    {

        private const string EndpointUrl = "https://azucos.documents.azure.com:443/";

        private const string PrimaryKey =
            "QhoCWcDzDiXuJmOG6beIsBmveIa1mrGCDNpkGGwDlCJwPQoGb1orYWr7lbe5W494jyGYPR8EibT4XXyfNbZAlg==";
        private DocumentClient client;

       
        static void Main(string[] args)
        {


            //int colChoice;
            int menuChoise = 0;
            bool mainQuit=false;
            bool subQuit = false;




            while (mainQuit == false)
            {
                Console.Clear();

                menuChoise = 0;
                


                //subQuit = false;
                
                //meny för val av uppgifter
                Console.WriteLine("Choose duty");
                Console.WriteLine();
                Console.WriteLine("0: Quit the program");
                Console.WriteLine();
                Console.WriteLine("1: Show photos to verify");
                Console.WriteLine();
                Console.WriteLine("2: Add new users");

                
                //Prövar indatan
                int menuChoice = TryInput(3);
                

                if (menuChoice == 1)
                {
                    Console.WriteLine("show pictures for verification");
                    VerifyPhotos(menuChoice);
                }
                else if (menuChoice == 2)
                {
                   subQuit = AddUser(menuChoice, subQuit);

                }
                else if (menuChoice == 0) mainQuit = true;  

                

           }   //while ends here


        }  //method main ends here


        
        
       

       //..........................Other methods here.....................................//



        //anropar verifiering av foton + kollar anslutning till db
        private static void VerifyPhotos(int menuChoice)
        {

            try
            {
                Program prg = new Program();
                prg.StartSession().Wait();
                prg.ShowPicturesToVerify("cloud", "VerifyPhotosCollection");

            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message,
                    baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Photos delivered and DB connected without any errors :-D");

                Console.WriteLine();
                Console.WriteLine("To main menu press 3");

                //Prövar indatan
                menuChoice = TryInput(3);
                
            }
        }   //method VerifyPhotos ends here







        //inmatning av user-data + kollar anlustning till db
        private static bool AddUser(int menuChoice, bool subQuit)
        {
            

            //ny whileloop som matar in värden för nya användare och foton som skall verifieras
            
            while (subQuit == false)
            {
                Console.Clear();

                Console.WriteLine("Create new user");
                Console.WriteLine();


                //Console.WriteLine("Insert user ID");    framtida feature!!
                //string inputUserID = Console.ReadLine();

                Console.WriteLine("Insert UserCollection: Name");
                string inputName = Console.ReadLine();
                
                //kolla om input är korrekt
                subQuit = CheckName(inputName, subQuit);

                Console.WriteLine();

                Console.WriteLine("Insert UserCollection: Email");
                string inputEmail = Console.ReadLine();
                subQuit = CheckEmail(inputEmail, subQuit);

                Console.WriteLine();

                Console.WriteLine("Insert VerifyPhotosCollection: PhotoUrl");
                string inputPhotoUrl = Console.ReadLine();
                subQuit = CheckPhotoUrl(inputPhotoUrl, subQuit);

                Console.WriteLine();



                try
                {

                    Program prg = new Program();

                    Console.WriteLine("Now inserting user, please wait :D");

                    prg.StartSession().Wait();
                    prg.AddNewUsers("cloud", "UsersCollection", inputName, inputEmail).Wait();
                    prg.AddNewPhotosToVerify("cloud", "VerifyPhotosCollection", inputPhotoUrl).Wait();
                }

                catch (DocumentClientException de)
                {
                    Exception baseException = de.GetBaseException();
                    Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message,
                        baseException.Message);
                }
                catch (Exception e)
                {
                    Exception baseException = e.GetBaseException();
                    Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
                }
                finally
                {
                    Console.WriteLine();
                    Console.WriteLine("User was created and DB connected without any errors :-D");
                    //Console.ReadKey();

                    Console.WriteLine();
                    Console.WriteLine("To insert another user press 5. To main menu press 4");

                    //Prövar indatan
                    menuChoice = TryInput(4);

                    if (menuChoice == 4)
                        subQuit = true;
                    else if (menuChoice == 5)
                        ;


                }


            } //inner while ends here

            return subQuit;

        }  //method AddUser ends here






        //Starta maskineriet
        private async Task StartSession()
        {
            this.client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);

        }





       
        //visa vilka foton som finns i databasen för verifiering
        private void ShowPicturesToVerify(string databaseName, string collectionName)
        {


            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            IQueryable<object> queryInSql = this.client.CreateDocumentQuery<object>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                "SELECT q.PhotoUrl FROM  VerifyPhotos q", queryOptions);

            Console.WriteLine();
            Console.WriteLine("Database is working, please wait");
            foreach (object quest in queryInSql)
            {
                Console.WriteLine("\n {0}", quest);

            }

            //Console.WriteLine("Press any key to continue ...");
            //Console.ReadKey();
        }





       
        
        
        
        
        
        //upplägg till nya foton för verifiering
        private async Task AddNewPhotosToVerify(string databaseName, string collectionName, string inputPhotoUrl)
        {

            await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName }); 

            await this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collectionName });   

            VerifyPhotosCollection ver = new VerifyPhotosCollection();
            {
                //slumpgenererad kod för ver.id-värde!!
                string randomSequence = GetRandomAlphaNumeric();

                ver.Id = randomSequence;
                //ver.VerId = "4";            framtida feature: hämta högsta id från db och plussa på ett!!
                //ver.UserId = inputUserID;  framtida feature: hämta högsta id från db och plussa på ett!!
                ver.PhotoUrl = inputPhotoUrl;
            }



            await this.CreateVerifyDocumentIfNotExists(databaseName, collectionName, ver);

        }






       
        
        //upplägg till nya användare
        private async Task AddNewUsers(string databaseName, string collectionName, string inputName, string inputEmail)
        {

            await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName }); 

            await this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), new DocumentCollection { Id = collectionName });   




            UsersCollection user = new UsersCollection();
            {
                //slumpgenererad kod för user.id-värde!!
                string randomSequence = GetRandomAlphaNumeric();

                user.Id = randomSequence;
                //user.UserId = inputUserID;  framtida feature: hämta högsta id från db och plussa på ett!!
                user.Name = inputName;
                user.Email = inputEmail;
            }



            await this.CreateUsersDocumentIfNotExists(databaseName, collectionName, user);

        }






        
        
        //skapande av foton-som-skall-verifieras-dokument i cosmos  ;)
        public async Task CreateVerifyDocumentIfNotExists(string databaseName, string collectionName, VerifyPhotosCollection ver)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, ver.Id));
                //this.WriteToConsoleAndPromptToContinue("Found {0}", users.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), ver);
                    //this.WriteToConsoleAndPromptToContinue("Created Family {0}", users.Id);
                }
                else
                {
                    throw;
                }
            }
        }






        
        
        //skapande av användar-dokument i cosmos
        public async Task CreateUsersDocumentIfNotExists(string databaseName, string collectionName, UsersCollection users)
        {
            try
            {
                await this.client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, users.Id));
                //this.WriteToConsoleAndPromptToContinue("Found {0}", users.Id);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), users);
                    //this.WriteToConsoleAndPromptToContinue("Created Family {0}", users.Id);
                }
                else
                {
                    throw;
                }
            }
            
        }








        //kontrollerar om navigationsdata är en int
        private static int TryInput(int navInt)
        {
            int inputValue = 0;

            try
            {
                inputValue = Convert.ToInt32(Console.ReadLine());
                Single.IsNaN(inputValue);
            }
            catch (Exception e)
            {

                Console.WriteLine("E R R O R M E S S A G E :");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(e);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Must insert a number. Press " + navInt + " to main menu");
                Console.ReadLine();
                inputValue = navInt;   //returnerar det värde som krävs för att återgå till main-menu
                return inputValue;
            }
            return inputValue;

        }    //method tryInput ends here




        
        //slumpgenerator för alfanumerisk sekvens
        private static string GetRandomAlphaNumeric()
        {
            Random random= new Random();
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(chars.Select(c => chars[random.Next(chars.Length)]).Take(8).ToArray());
        }



        
        
        //kollar om namn är korrekt inmatad
        private static bool CheckName(string inputString, bool subQuit)
        {
            Regex r = new Regex("^[^±!@£$%^&*_+§¡€#¢§¶•ªº«\\/<>?:;|=.,]{1,20}$");

            //se om strängen är null eller tom
            if (string.IsNullOrEmpty(inputString))
            {
                Console.WriteLine("String cant be empty. Insert only common charachters for names");
                Console.WriteLine("Start over? To insert another user press 5. To main menu press 4");
                //Console.ReadLine();
                //Prövar indatan
                int menuChoice = TryInput(4);

                if (menuChoice == 4)
                {
                    subQuit = true;
                    Main(null);
                    
                }
                else if (menuChoice == 5)
                    AddUser(menuChoice, subQuit);


                return subQuit;
            }

                //kolla om för och efternman är korrekt
                if (!r.IsMatch(inputString))
                {
                        Console.WriteLine("Wrong type of charachters. Insert only common charachters for names");
                    Console.WriteLine("Start over? To insert another user press 5. To main menu press 4");
                    //Console.ReadLine();
                
                    int menuChoice = TryInput(4);

                    if (menuChoice == 4)
                        Main(null);
                    else if (menuChoice == 5)
                       AddUser(menuChoice, subQuit);

                    
                    return subQuit;
                    
                   
                }
            

            return subQuit;
        }




        //kollar om mail är korrekt inmatad
        private static bool CheckEmail(string inputString, bool subQuit)
        {

            Regex r = new Regex("^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$");

            //se om strängen är null eller tom
            if (string.IsNullOrEmpty(inputString))
            {
                Console.WriteLine("String cant be empty. Insert only common charachters for email");
                Console.WriteLine("Start over? To insert another user press 5. To main menu press 4");
                //Console.ReadLine();
                //Prövar indatan
                int menuChoice = TryInput(4);

                if (menuChoice == 4)
                {
                    subQuit = true;
                    Main(null);

                }
                else if (menuChoice == 5)
                    AddUser(menuChoice, subQuit);


                return subQuit;
            }

            //kolla om för och efternman är korrekt
            if (!r.IsMatch(inputString))
            {
                Console.WriteLine("Wrong type of charachters. Insert only common charachters for email");
                Console.WriteLine("Start over? To insert another user press 5. To main menu press 4");
                //Console.ReadLine();

                int menuChoice = TryInput(4);

                if (menuChoice == 4)
                    Main(null);
                else if (menuChoice == 5)
                    AddUser(menuChoice, subQuit);


                return subQuit;


            }


            return subQuit;


        }





        //kollar om foto-url:en är korrekt inmatad
        private static bool CheckPhotoUrl(string inputString, bool subQuit)
        {

            Regex r = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");

            //se om strängen är null eller tom
            if (string.IsNullOrEmpty(inputString))
            {
                Console.WriteLine("String cant be empty. Insert only common charachters for URL:s");
                Console.WriteLine("Start over? To insert another user press 5. To main menu press 4");
                //Console.ReadLine();
                //Prövar indatan
                int menuChoice = TryInput(4);

                if (menuChoice == 4)
                {
                    subQuit = true;
                    Main(null);

                }
                else if (menuChoice == 5)
                    AddUser(menuChoice, subQuit);


                return subQuit;
            }

            //kolla om för och efternman är korrekt
            if (!r.IsMatch(inputString))
            {
                Console.WriteLine("Wrong type of charachters. Insert only common charachters for URL:s");
                Console.WriteLine("Start over? To insert another user press 5. To main menu press 4");
                //Console.ReadLine();

                int menuChoice = TryInput(4);

                if (menuChoice == 4)
                    Main(null);
                else if (menuChoice == 5)
                    AddUser(menuChoice, subQuit);


                return subQuit;


            }


            return subQuit;

            
        }



    }  //class program ends here

    

    //...........................Other classes here.............................................//



    public class UsersCollection
    {
        

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        // public string UserId { get; set; }   //framtida feature
        public string Name { get; set; }
        public string Email { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }   //class userscollection ends here



    public class VerifyPhotosCollection
    {


        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        //public string VerId { get; set; }   //framtida feature
        //public string UserId { get; set; }   //framtida feature
        public string PhotoUrl { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }   //class VerifyPhotosCollection ends here




}  //namespace ends here



