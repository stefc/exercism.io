// 2. Der immutable Typ 'Card' 
using System;

public enum Rank
{
    Ace, King, Queen, Jack,
    Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two,
    PoorAce
}

public enum Suit
{
    Clubs = 'C', Diamonds = 'D', Hearts = 'H', Spades = 'S'
}

public readonly struct Card
{
    public Rank Rank { get; init; }
    public Suit Suit { get; init; }

    // Factory Methode 
    public static Card Create(string stmt) => new Card(stmt);
    private Card(string stmt) => (Rank, Suit) = (RankFromString(stmt), SuitFromString(stmt));

    // Solid Prinzip Single-Responsibility-Prinzip
    private static Suit SuitFromString(string stmt) => stmt[^1] switch
    {
        'S' => Suit.Spades,
        'H' => Suit.Hearts,
        'D' => Suit.Diamonds,
        'C' => Suit.Clubs,
        _ => throw new ArgumentException(stmt)
    };

    private static Rank RankFromString(string stmt) => stmt[..^1] switch
    {
        "A" => Rank.Ace,
        "K" => Rank.King,
        "Q" => Rank.Queen,
        "J" => Rank.Jack,
        "10" => Rank.Ten,
        "9" => Rank.Nine,
        "8" => Rank.Eight,
        "7" => Rank.Seven,
        "6" => Rank.Six,
        "5" => Rank.Five,
        "4" => Rank.Four,
        "3" => Rank.Three,
        "2" => Rank.Two,
        _ => throw new ArgumentException(stmt)
    };


    public override string ToString()
    {

        var rank = Rank switch
        {
            Rank.Ace => "A",
            Rank.King => "K",
            Rank.Queen => "Q",
            Rank.Jack => "J",
            Rank.Ten => "10",
            Rank.Nine => "9",
            Rank.Eight => "8",
            Rank.Seven => "7",
            Rank.Six => "6",
            Rank.Five => "5",
            Rank.Four => "4",
            Rank.Three => "3",
            Rank.Two => "2"
        };

        var suit = Convert.ToChar(Suit);

        return $"{rank}{suit}";
    }
}
