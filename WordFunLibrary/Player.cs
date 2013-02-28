//Author: Brad Vidler
//Name: Player.cs
//Description: Keeps track of players' nickanes and scores
//Date: 4/4/2012

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordFunLibrary
{
    public class Player
    {
        private int score_;
        public int Score
        {
            get { return score_; }
            set { score_ = value; }
        }

        //private bool ingame_;


        private string nickname_;
        public string Nickname
        {
            get { return nickname_; }
            set { nickname_ = value; }
        }

        //private int playernumber_;
    }
}
