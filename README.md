# LM962
Lobstermania slot machine Windows Console Simulator, 96.2% payback, 5.2% hit rate.

# Overview 
This project implements a Windows console simulator for the Lobstermania slot machine game. It was written from the ground up, using the reel sizes, symbol sets, per reel symbol counts, payouts, etc. as specified in the publicly available document "HarriganDixon2009-JOGIPARSheets.pdf" (it can be found in the misc directory of this project). This project verifies the expected payback percentage of 96.2%, and the line hit frequency of 5.2%. These statistics can be verified by building and running the program, selecting "Bulk Game Spins" from the Main Menu, entering a number of spins >= 259,440,000 and setting the number of pay lines to 1. The number 259,440,000 represents the number of all possible reel slot combinations on a line. You can play any number of pay lines between 1 and 15, any number of Bulk Game spins and see session statistics, or play "Individual Games" to see game winning pay lines and payouts as well as total game statistics. Slot symbols are abbreviated to a 2 character string as shown below, and all games are currently based on a bet of 1 credit per pay line.
    
    SYMBOLS123 = { "WS", "LM", "BU", "BO", "LH", "TU", "CL", "SG", "SF", "LO", "LT" }; // All 11 game symbols, Corresponds to: 
                 { "Wild", "Lobstermania", "Buoy", "Boat", "Lighthouse", "Tuna", "Clam", "Seagull", "Starfish", "Bonus", "Scatter" }
              
# Example use cases
1. As a learning tool to understand video slot machine probabilities, payouts, and algorithms.
2. As a base engine for developing a graphical version of the game.
3. As a base for developing other slot machine games by changing symbols, payouts, and game rules.

# My goals

My personal goals in writing this project included learning C#, .NET, Git, and Github as well as learning how slots work under the covers. I would appreciate any feedback (general or specific), bug reporting, etc.

I hope you enjoy this project!

-Jim
                 
                 
