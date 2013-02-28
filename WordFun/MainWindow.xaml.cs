//Author: Brad Vidler
//Title: WordFun
//Description: A game in which players are given 6 letters 
/////////////  and must make as many words as possible within 
/////////////  the alloted time.
//Date: 4/4/2012

/*
NOTES:
 *  Timer code from: http://stackoverflow.com/questions/942207/c-sharp-simple-countdown-what-am-i-doing-wrong
 *  
 *   TODO:
 *      -Fix messages that should not be showing up
 *      -Fix scoring system
 *      -Fix initial letters given to players (instead of random letters give a jumbled 6 letter word)
 *      -Only allow letters to be used once
 *      -Don't allow new games when one is being played
 *      -Allow new players to join only when a game is not in play
 *      -Allow nicknames for players
 *      -Fix timer glitch. (Off by one second, however, players still do play for the same amount of time)
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WordFunLibrary;
using System.ServiceModel;

using System.Windows.Threading; //used for timer

namespace WordFun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        private IWordList words;
        private int callbackId = 0;
        private int playerID = 0;

        List<char> charList = new List<char>();
        List<string> wordList = new List<string>();
        List<Player> pList = new List<Player>();

        //private int _time;
        private DispatcherTimer _countdownTimer;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Configure the ABCs of using CardsLibrary
                DuplexChannelFactory<IWordList> channel = new DuplexChannelFactory<IWordList>(this, "WordFunEndPoint");

                // Activate a WordList object
                words = channel.CreateChannel();

                // Register for callbacks
                callbackId = words.RegisterForCallbacks();

                _countdownTimer = new DispatcherTimer();
                _countdownTimer.Interval = new TimeSpan(0, 0, 1);
                _countdownTimer.Tick += new EventHandler(CountdownTimerStep);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CountdownTimerStep(object sender, EventArgs e)
        {
            if (words.SecondsLeft > 0)
            {
                words.SecondsLeft--;
                this.labelTime.Content = words.SecondsLeft + "s";
            }
            else //stop timer, announce winner
            {
                _countdownTimer.Stop();
                MessageBox.Show(words.GetWinners(), "Game Over!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            if (words.SecondsLeft == 0)
            {
                charList = words.repopulateChars();
                wordList = words.repopulateWords();
                words.SecondsLeft = 60;
            }
            else
                MessageBox.Show("There is already a game in progress! Get to work!", "Game in Progress!", MessageBoxButton.OK,MessageBoxImage.Exclamation);  
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            string chars = "";
            string validword = "";

            foreach (char c in charList)
            {
                chars += c;
            }

            foreach (char c in txtWord.Text)
            {
                if (!chars.ToLower().Contains(c)) //not allowed to use this letter
                {
                    MessageBox.Show("You can only use the letters given to you!", "Invalid Letter!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    validword = "";
                    break;
                }
                else //valid letter
                {
                    validword += c;
                }
            }

            if (words.Words.Contains(validword))
            {
                //words.PStats[playerID].Score++;
                words.ChangePlayerScore(playerID, 1);
                words.Details = "Word Found!";
                //MessageBox.Show("YAY!", "YAY!", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                //Update word list to reflect removed word
                //wordList.Remove(validword);
            }

        }

        // Implement the ICallback service contract
        private delegate void ClientUpdateDelegate(CallbackInfo info);

        public void UpdateGui(CallbackInfo info)
        {
            // Only the main (dispatch) thread can change the GUI
            txtTotWords.Dispatcher.BeginInvoke(new ClientUpdateDelegate(newGameViaDispatchThread), info);
        }

        private void newGameViaDispatchThread(CallbackInfo info)
        {
                if ((info.isNewGame) && (lstPlayers.Items.Count > 0))
                {
                    let1.Content = info.letters[0];
                    let2.Content = info.letters[1];
                    let3.Content = info.letters[2];
                    let4.Content = info.letters[3];
                    let5.Content = info.letters[4];
                    let6.Content = info.letters[5];

                    charList = info.letters;
                    txtTotWords.Text = info.numWords.ToString();
                    _countdownTimer.Start();
                    wordList = info.words;
                }

                txtInfo.AppendText(info.details + '\n');

                lstPlayers.Items.Clear();
                foreach (Player player in words.PStats)
                {
                    lstPlayers.Items.Add(player.Nickname + " - " + player.Score);
                }
        }

        private void newEvent(CallbackInfo info)
        {
            txtInfo.AppendText(info.details + '\n');
        }

        private void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            playerID = words.PStats.Count;
            Player p = new Player();
            p.Nickname = "Player " + playerID;
            p.Score = 0;
            words.AddPlayer(p);
            btnJoin.IsEnabled = false;
            btnSitOut.IsEnabled = true;
        }

        private void btnSitOut_Click(object sender, RoutedEventArgs e)
        {
            words.RemovePlayer(playerID);
            btnJoin.IsEnabled = true;
            btnSitOut.IsEnabled = false;
        }
    }
}
