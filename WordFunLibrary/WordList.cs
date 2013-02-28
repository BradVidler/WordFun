//Author: Brad Vidler
//Name: WordList.cs
//Description: Game logic used to track words, letters, player info, events, etc...
//Date: 4/4/2012

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.ServiceModel;

namespace WordFunLibrary
{
    // A callback contract to be implemented by the client
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateGui(CallbackInfo info);
    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IWordList
    {
        [OperationContract]
        int RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]
        void UnregisterForCallbacks(int id);
        [OperationContract(IsOneWay = true)]
        void Remove(string word);
        [OperationContract(IsOneWay = true)]
        void AddPlayer(Player p);
        [OperationContract(IsOneWay = true)]
        void RemovePlayer(int id);
        [OperationContract(IsOneWay = true)]
        void ChangePlayerScore(int id, int score);
        [OperationContract]
        string GetWinners();
        int NumWords { [OperationContract] get; [OperationContract]  set; }
        string Details { [OperationContract] get; [OperationContract]  set; }
        int Players { [OperationContract] get; [OperationContract]  set; }
        int SecondsLeft { [OperationContract] get; [OperationContract]  set; }
        List<Player> PStats { [OperationContract] get; [OperationContract]  set; }
        [OperationContract] 
        List<char> repopulateChars();
        [OperationContract] 
        List<string> repopulateWords();
        List<string> Words { [OperationContract] get; [OperationContract]  set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WordList : IWordList
    {
        // Member variables
        public List<char> letters = new List<char>();
        private int wordIdx;
        private Dictionary<int, ICallback> clientCallbacks = new Dictionary<int, ICallback>();
        private int nextCallbackId = 1;
        private string details = "";
        private bool isNewGame = false;

        // C'tor
        public WordList()
        {
            wordIdx = 0;
            //repopulate();
        }

        // Public methods and properties

        /*
         * This method retrieves the specific callback proxy object for the client that
         * calls this method. The proxy object is stored in the clientCallbacks collection
         * until the [WordList] needs it to update the client. The proxy object is stored
         * along with a key value (the callback id) so that the callback proxy can be 
         * removed later if the client unregisters.
         */
        public int RegisterForCallbacks()
        {

            ICallback callback = OperationContext.Current.GetCallbackChannel<ICallback>();

            clientCallbacks.Add(nextCallbackId, callback);
            return nextCallbackId++;
        }

        /*
         * This method allows a client to remove itself from the callbacks service such that it will
         * no longer receive updates from the [WordList]. This is important so that an instance of the client
         * can shutdown without "breaking" the callback service by leaving an invalid callback proxy
         * in the clientCallbacks collection.
         */
        public void UnregisterForCallbacks(int id)
        {
            clientCallbacks.Remove(id);
        }

        //Was going to use this to remove words found from the word list
        public void Remove(string word)
        {
            //Console.WriteLine("Word found - Removing word: " + word);

            //words.Remove(word);

            // Initiate callbacks
            //updateAllClients(true);
        }

        //Keeps track of available words
        private List<string> words_ = new List<string>();
        public List<string> Words
        {
            get { return words_; }
            set
            {
                words_ = value;
            }
        }

        //Add a player to the game
        public void AddPlayer(Player p)
        {
            pStats_.Add(p);
            details = "Player Added!";
            updateAllClients(true);
        }

        //Declare winner(s)
        public string GetWinners()
        {
            //Determine highest score
            int highScore = 0;
            foreach (Player p in PStats)
            {
                if (p.Score > highScore)
                    highScore = p.Score;
            }

            //Find all players with the highest score
            List<Player> winners = new List<Player>();
            foreach (Player p in PStats)
            {
                if (p.Score == highScore)
                    winners.Add(p);
            }

            //Return who won/tied
            string winText = "";
            if (winners.Count > 1)
            {
                for (int i = 0; i < winners.Count; ++i)
                {
                    winText += winners[i].Nickname;
                    if (i < winners.Count - 1)
                        winText += " and ";
                }
                winText += " tied!";
            }
            else
                winText = winners[0].Nickname + " wins!";

            return winText;
        }

        //Remove player from game
        public void RemovePlayer(int id)
        {
            pStats_.RemoveAt(id);
            details = "Player Sat Out!";
            updateAllClients(true);
        }

        //Add to player score or wipe it back to zero
        public void ChangePlayerScore(int id, int score)
        {
            pStats_[id].Score+=score;
            updateAllClients(true);
        }

        //Number of available words based on given letters
        private int numwords_;
        public int NumWords
        {
            get { return (int)words_.Count; }
            set
            {
                numwords_ = value;
            }
        }

        //Seconds left on the clock
        private int secondsLeft_;
        public int SecondsLeft
        {
            get { return secondsLeft_; }
            set
            {
                secondsLeft_ = value;
            }
        }

        //List to keep track of all players
        private List<Player> pStats_ = new List<Player>();
        public List<Player> PStats
        {
            get { return pStats_; }
            set
            {
                pStats_ = value;
                updateAllClients(true);
            }
            
        }

        //Details to be posted to the "info" box for all players to see
        public string Details
        {
            get { return details; }
            set
            {
                details = value;
                updateAllClients(true);
            }
        }

        //Tracks number of players
        private int players_ = 0;
        public int Players
        {
            get { return (int)players_; }
            set
            {
                players_ = value;
                Console.WriteLine("Adding Player");
            }
        }

        //Give new letters to players
        public List<char> repopulateChars()
        {
            // Remove "old" words and letters
            words_.Clear();
            letters.Clear();

            //Deal out 6 new letters (2 of which are vowels)
            string vowels = "AEIOU";
            string consonants = "BCDFGHJKLMNPQRSTVWXYZ";
            string strLetters;

            var random = new Random();
            var result = new string(
                Enumerable.Repeat(vowels, 2)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            strLetters = result.ToLower();

            result = new string(
                Enumerable.Repeat(consonants, 4)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            strLetters += result.ToLower();
            letters = strLetters.ToList();
            
            isNewGame = true;
            updateAllClients(true);
            isNewGame = false;

            if (PStats.Count > 0)
            {
                Console.WriteLine("Game Started!");
                details = "Game Started!";
            }
            else
                details = "This game requires a minimum of 1 player. Please join to play.";

            return letters;
        }

        //Get all available words based on given letters
        public List<string> repopulateWords()
        {
            // Populate words with all words from text file
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader("words.txt");
            while ((line = file.ReadLine()) != null)
            {
                string validword = "";
                foreach (char c in line)
                {
                    if (!letters.Contains(c)) //not allowed to use this letter
                    {
                        validword = "";
                        break;
                    }
                    else //valid letter
                    {
                        validword += c;
                    }
                }
                if (validword != "")
                    words_.Add(line);
            }

            file.Close();
            NumWords = words_.Count;
            return words_;
        }

        /*
         * Call this to update ALL clients
         */
        private void updateAllClients(bool refresh)
        {
            CallbackInfo info = new CallbackInfo(numwords_, letters, refresh, details, isNewGame, pStats_, words_, secondsLeft_);
            foreach (ICallback callback in clientCallbacks.Values)
                callback.UpdateGui(info);
        }
    }
}
