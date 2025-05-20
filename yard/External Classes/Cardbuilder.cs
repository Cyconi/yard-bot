using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yard.External_Classes;

internal class Cardbuilder
{
    public int[] cardNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
    public string[] cardSuits = { "Clubs", "Spades", "Diamonds", "Hearts" };

    public int selectedNumber { get; internal set; }
    public string selectedCard { get; internal set; }

    public Cardbuilder() 
    { 
        var random = new Random();
        int indexNumber = random.Next(0, cardNumbers.Length - 1);
        int indexSuit = random.Next(0, cardSuits.Length - 1);

        selectedNumber = cardNumbers[indexNumber];
        selectedCard = cardNumbers[indexNumber] + " of " + cardSuits[indexSuit];
    }
}
