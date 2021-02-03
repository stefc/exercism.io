using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Hand = System.Collections.Generic.IEnumerable<Card>;

public static class Poker
{
    public static Hand ToHand(this string hand) => hand.Split().Select(Card.Create);


    public static IEnumerable<string> BestHands(IEnumerable<string> hands)
    {

        if (hands.TryStraight(out var straight)) {
            return straight;
        } else if (hands.TryThreeOfAKind(out var threeOfaKind)) {
            return threeOfaKind;
        } else if (hands.TryTwoPairs(out var twoPairs)) {
            return twoPairs;
        } else if (hands.TryOnePair(out var onePair)) {
            return onePair;
        }

        return hands.HighestCard();
    }



    public static string ToString(this Hand hand) => String.Join( ' ', hand.Select( card => card.ToString()));

    public static Card HighestCard(this Hand hand) => hand.OrderBy(c => c.Rank ).First();



    public static (Hand sameRank, Hand kickers, string origin) Split(this string hand, int count = 2) 
    {
        //  "4S 2H 6S 2D JH" -> "2H 2D" sameRank, "4S 6S JH" kickers  origin = .. 

        //   4 - 4S
        //   2 - 2H, 2D
        //   6 - 6S 
        //   J - JH
        var x = hand.ToHand().GroupBy( card => card.Rank);
        return (
            sameRank: x.Where( grp => grp.Count() == count).SelectMany( grp => grp).ToArray(), 
            kickers:  x.Where( grp => grp.Count() != count).SelectMany( grp => grp).ToArray(),
            origin: hand);
    }   

    public static bool IsStraight(this Hand hand) {
        var x  = hand.Select( card => card.Rank).OrderBy( rank => rank).Distinct(); 
        return x.Count() != 5 ? 
            false 
            : 
            x.Last() - x.First() == 4 ? 
                true 
                : 
                x.First() == Rank.Ace && x.Last() == Rank.Two ? 
                    x.Last() - x.Skip(1).First() == 3 ? 
                        true 
                        : 
                        false 
                    : 
                    false; 
    }

    public static bool HasOnePair(this Hand hand) => hand.GroupBy( card => card.Suit).Any( grp => grp.Count() == 2);
    
    public static Suit HighestSuit(this Hand hand) => hand
        .GroupBy( card => card.Suit)
        .Where( grp => grp.Count() >= 2)
        .Select( grp => grp.Key)
        .Distinct()
        .OrderBy( suit => suit)
        .FirstOrDefault();

    public static bool TryStraight(this IEnumerable<string> hands, out IEnumerable<string> result) {

        result = Enumerable.Empty<string>();

        var matches = hands
            .Where( hand => hand.ToHand().IsStraight());

        if (matches.Any())
        {
            result = matches
                .GroupBy( hand => hand.ToHand(), HandComparer.Default)
                .OrderBy( grp => grp.Key, HandComparer.Default)
                .First()
                .ToArray();
            return true; 
        } 

        return false;
    }

    public static bool TryThreeOfAKind(this IEnumerable<string> hands, out IEnumerable<string> result) {

        result = Enumerable.Empty<string>();

        var matches = hands
            .Select( hand => hand.Split(3))
            .Where(match => match.sameRank.Count()==3);

        if (matches.Any())
        {
            result = matches
                .GroupBy( match => match.sameRank, HandComparer.Default)
                .OrderBy( grp => grp.Key, HandComparer.Default).First()
                .GroupBy( match => match.kickers, HandComparer.Default)
                .OrderBy( grp => grp.Key, HandComparer.Default).First()
                .Select( match => match.origin)
                .ToArray();

            return true; 
        }

        return false;
    }

    public static bool TryTwoPairs(this IEnumerable<string> hands, out IEnumerable<string> result) {

        result = Enumerable.Empty<string>();

        var matches = hands
            .Select( hand => hand.Split(2))
            .Where( match => match.sameRank.Count()==4);
        if (matches.Any())
        {
            result = matches
                .GroupBy( match => match.sameRank, HandComparer.Default)
                .OrderBy( grp => grp.Key, HandComparer.Default).First()
                .GroupBy( match => match.kickers, HandComparer.Default)
                .OrderBy( grp => grp.Key, HandComparer.Default).First()
                .Select( match => match.origin)
                .ToArray();

            return true; 
        }

        return false;
    }

    public static bool TryOnePair(this IEnumerable<string> hands, out IEnumerable<string> result) {

        result = Enumerable.Empty<string>();

        var matches = hands
            .Select( hand => hand.Split(2))
            .Where( match => match.sameRank.Any());
        if (matches.Any())
        {
            result = matches
                .GroupBy( match => match.sameRank, HandComparer.Default)
                .OrderBy( grp => grp.Key, HandComparer.Default).First()
                .GroupBy( match => match.kickers, HandComparer.Default)
                .OrderBy( grp => grp.Key, HandComparer.Default).First()
                .Select( match => match.origin)
                .ToArray();

            return true; 
        }

        return false;
    }

    public static IEnumerable<string> HighestCard(this IEnumerable<string> hands) 
    => hands
        .GroupBy( hand => hand.ToHand(), HandComparer.Default)
        .OrderBy( grp => grp.Key, HandComparer.Default)
        .First()
        .ToArray();
}



public enum Rank
{
    Ace,
    King,
    Queen,
    Jack,
    Ten,
    Nine,
    Eight,
    Seven,
    Six,
    Five,
    Four,
    Three,
    Two,
    PoorAce
}

public enum Suit
{
    Clubs = 'C',
    Diamonds = 'D',
    Hearts = 'H',
    Spades = 'S'
}

public readonly struct Card
{
    public Rank Rank { get; init; }
    public Suit Suit { get; init; }


    private Card(Rank rank, Suit suit) => (Rank, Suit) = (rank, suit);
    private Card(string stmt) => (Rank, Suit) = FromString(stmt);

    private static (Rank, Suit) FromString(string stmt) => (RankFromString(stmt), SuitFromString(stmt));

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
        "10"=> Rank.Ten, 
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

    public static Card Create(string stmt) => new Card(stmt);

    public override string ToString() {

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


public class HandComparer : IComparer<Hand>, IEqualityComparer<Hand>
{
    private readonly bool compareStraight;
    private static readonly Lazy<HandComparer> @default = new Lazy<HandComparer>(() => new HandComparer(false), true);
    private static readonly Lazy<HandComparer> straight = new Lazy<HandComparer>(() => new HandComparer(true), true);

    public HandComparer(bool compareStraight) {
        this.compareStraight = compareStraight;
    }

    private IEnumerable<Rank> OrderByRank( Hand hand) =>
        this.compareStraight && hand.HasPoorAce() ? 

        hand.Select( c => c.Rank).OrderBy( r => r);
    
         hand.Select( c => c.Rank).OrderBy(r => r); 

    public int Compare(Hand x, Hand y) {

        // 4D 5S 6S 8D 3C", x =  8D 6S 5S 4D 3C
        // 2S 4C 7S 9H 10H" y = 10H 9H 7S 4C 2S
        // zip                   +1 +1 +1  0 -1

        var match = OrderByRank(x)
            .Zip( OrderByRank(y), (lhs,rhs) => lhs.CompareTo(rhs));
        return (match.All( cmp => cmp == 0)) ? 0 : match.FirstOrDefault( cmp => cmp != 0); 
    }

    public bool Equals(Hand x, Hand y) => this.Compare( x,y) == 0;

    public int GetHashCode([DisallowNull] Hand hand) => hand
        .OrderBy( c => c.Rank )
        .Select( c => c.Rank.GetHashCode())
        .Aggregate( 13, (accu, cur) => accu ^ cur);

    public static HandComparer Default => @default.Value;

    private HandComparer() {}
}


