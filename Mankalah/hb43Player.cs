using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
namespace Mankalah
{
    public class hb43Player : Player
    {
        public hb43Player(Position pos, int timeLimit) : base(pos, "Chloe", timeLimit) { }

        public override string gloat()
        {
            return "iGloat";
        }
        public override int evaluate(Board b)
        {
            int score = b.stonesAt(13) - b.stonesAt(6);
            int stonesTotal = 0;
            int goAgainsTotal = 0;
            int capturesTotal = 0;

            // TOP loop
            for (int i = 7; i <= 12; i++)
            {
                // add all the stones in the top row
                stonesTotal += b.stonesAt(i);
                // add all the 'go-again's in the top row
                if (b.stonesAt(i) - (13 - i) == 0) goAgainsTotal += 1;

                // add all of stones that can be obtained through captures
                int target = i + b.stonesAt(i);
                if (target < 13)
                {
                    int targetStones = b.stonesAt(target);
                    if (b.whoseMove() == Position.Top)
                    {
                        if (targetStones == 0 && b.stonesAt(13 - target - 1) != 0) capturesTotal += b.stonesAt(13 - target - 1);
                    }
                }
            }
            // BOTTOM loop
            for (int i = 0; i <= 5; i++)
            {
                // subtract all the stones in the bottom row
                stonesTotal -= b.stonesAt(i);
                // add all the 'go-again's in the bottom row
                if (b.stonesAt(i) - (6 - i) == 0) goAgainsTotal -= 1;

                // subtract all of stones that can be obtained through captures
                int target = i + b.stonesAt(i);
                if (target < 6)
                {
                    int targetStones = b.stonesAt(target);
                    if (b.whoseMove() == Position.Bottom)
                    {
                        if (targetStones == 0 && b.stonesAt(13 - target - 1) != 0) capturesTotal -= b.stonesAt(13 - target - 1);
                    }
                }
            }

            // add up all of the heuristics and return what is believed the score will be
            score += stonesTotal + capturesTotal + goAgainsTotal;
            return score;
        }

        public override int chooseMove(Board b)
        {
            // Start stopwatch and set initial depth at 5
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int depth = 10;
            MoveResult move = new MoveResult(0, 0, false);

            //Staged DFS
            while (stopWatch.ElapsedMilliseconds < getTimePerMove())
            {
                try
                {
                    move = minimax(b, depth, stopWatch, int.MinValue, int.MaxValue);
                    depth++;
                }
                catch (TimeoutException toe) {; }
            }
            stopWatch.Stop();
            return move.getMove();
        }

        public override String getImage() { return "chloe.png"; }

        private MoveResult minimax(Board b, int d, Stopwatch w, int alpha, int beta)
        {
            // check to see if the time limit is up
            if (w.ElapsedMilliseconds > getTimePerMove()) throw new TimeoutException();
            // base case
            if (b.gameOver() || d == 0)
            {
                return new MoveResult(0, evaluate(b), b.gameOver());
            }
            // initialization of trackers
            int bestMove = 0;
            int bestVal;
            bool gameCompleted = false;
            // check all the the moves that top could make, and act as if it is the MAX in minimax
            if (b.whoseMove() == Position.Top) // TOP is MAX
            {
                // smallest possible value
                bestVal = Int32.MinValue;
                for (int move = 7; move <= 12 && alpha < beta; move++)
                {
                    if (b.legalMove(move))
                    {
                        Board b1 = new Board(b);                               
                        b1.makeMove(move, false);                              
                        MoveResult val = minimax(b1, d - 1, w, alpha, beta);   
                        if (val.getScore() > bestVal)                          
                        {
                            bestVal = val.getScore();
                            bestMove = move;
                            // track the current condition of the game
                            gameCompleted = val.isEndGame();
                        }
                        // alpha beta
                        if (bestVal > alpha)
                        {
                            alpha = bestVal;
                        }
                    }
                }
            }
            // check all the the moves that bottom could make, and act as if it is the MIN in minimax
            else  // BOTTOM is MIN
            {
                // largest possible value
                bestVal = Int32.MaxValue;
                for (int move = 0; move <= 5 && alpha < beta; move++)
                {
                    if (!b.legalMove(move)) continue;
                    Board b1 = new Board(b);                               
                    b1.makeMove(move, false);                              
                    MoveResult val = minimax(b1, d - 1, w, alpha, beta);   
                    if (val.getScore() < bestVal)                          
                    {
                        bestVal = val.getScore();
                        bestMove = move;
                        // track the current condition of the game
                        gameCompleted = val.isEndGame();
                    }
                    // alpha beta
                    if (bestVal < beta)
                    {
                        beta = bestVal;
                    }
                }
            }
            return new MoveResult(bestMove, bestVal, gameCompleted);
        }


        class MoveResult
        {
            private int bestMove;
            private int bestScore;
            private bool endGame;
            public MoveResult(int move, int score, bool end)
            {
                bestMove = move;
                bestScore = score;
                endGame = end;
            }

            public int getMove() { return bestMove; }

            public int getScore() { return bestScore; }

            public bool isEndGame() { return endGame; }
        }
    }
}
