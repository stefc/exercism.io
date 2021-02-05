using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Domain.Functors;

using static Domain.Functors.F;

using Hand = System.Collections.Generic.IEnumerable<Card>;
using Hands = System.Collections.Generic.IEnumerable<string>;

public static class Poker
{
    private record Separation (Hand SameRank, Hand Kickers, string Origin);

    private static Hand ToHand(this string hand) => hand.Split().Select(Card.Create);

    private static string ToString(this Hand hand) => String.Join(' ', hand.Select(card => card.ToString()));

    public static Hands BestHands(Hands hands)
    => hands.StraightFlush() 
        | hands.Quad() 
        | hands.Fullhouse()
        | hands.Flush() 
        | hands.Straight() 
        | hands.Triplet()
        | hands.TwoPairs() 
        | hands.OnePair() 
        | hands.HighestCard() 
        | Enumerable.Empty<string>();

    
    private static Separation Separate(this string hand, byte count)
    {
        //  "4S 2H 6S 2D JH" -> "2H 2D" sameRank, "4S 6S JH" kickers  origin = .. 
        var x = hand.ToHand().GroupBy(card => card.Rank);
        return new (
            x.Where(grp => grp.Count() == count).SelectMany(grp => grp).ToArray(),
            x.Where(grp => grp.Count() != count).SelectMany(grp => grp).ToArray(),
            hand);
    }

    private static Separation Separate(this string hand)
        => Separate(hand, 1);
    private static Separation SeparatePair(this string hand)
        => Separate(hand, 2);
    private static Separation SeparateTriplet(this string hand)
        => Separate(hand, 3);
    private static Separation SeparateQuad(this string hand)
        => Separate(hand, 4);

    #region Predicates

    private static bool IsStraightFlush(this string hand)
        => hand.ToHand().IsStraightFlush();

    private static bool IsStraightFlush(this Hand hand)
        => hand.IsStraight() && hand.IsFlush();

    private static bool IsFlush(this string hand) => hand.ToHand().IsFlush();

    private static bool IsFlush(this Hand hand)
        => hand.Select(card => card.Suit).Distinct().Count() == 1;

    private static bool IsFullhouse(this string hand) => hand.ToHand().IsFullhouse();

    private static bool IsFullhouse(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 2 && (hand.IsTriplet());

    private static bool IsTwoPairs(this string hand)
        => hand.ToHand().IsTwoPairs();
    private static bool IsTwoPairs(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 3 
            && (hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 1));

    private static bool IsOnePair(this string hand)
        => hand.ToHand().IsOnePair();
    private static bool IsOnePair(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 4 
            && (hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 2));

    private static bool IsQuad(this string hand) 
        => hand.ToHand().IsQuad();
    private static bool IsQuad(this Hand hand)
        => hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 4);

    private static bool IsTriplet(this string hand)
        => hand.ToHand().IsTriplet();
    private static bool IsTriplet(this Hand hand)
        => hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 3);

    private static bool IsStraight(this string hand) => hand.ToHand().IsStraight();

    private static bool IsStraight(this Hand hand)
    {
        var x = hand.Select(card => card.Rank).OrderBy(rank => rank).Distinct();
        return 
            x.Count() == 5 
            && x.Last() - x.First() == 4
            || hand.HasPoorAce() && x.Last() - x.Skip(1).First() == 3;
    }
    #endregion

    private static Option<Hands> StraightFlush(this Hands hands)
    => hands
        .Where(IsStraightFlush)
        .Select(Separate)
        .GetResult(HandComparer.Straight);

    private static Option<Hands> Quad(this Hands hands)
    => hands
        .Where(hand => hand.ToHand().IsQuad())
        .Select(SeparateQuad)
        .GetResult(HandComparer.Default);

    private static Option<Hands> Fullhouse(this Hands hands)
    => hands
        .Where(IsFullhouse)
        .Select(SeparateTriplet)
        .GetResult(HandComparer.Default);

    private static Option<Hands> Flush(this Hands hands)
    => hands
        .Where(IsFlush)
        .Select(Separate)
        .GetResult(HandComparer.Default);

    private static Option<Hands> Straight(this Hands hands)
    => hands
        .Where(IsStraight)
        .Select(Separate)
        .GetResult(HandComparer.Straight);

    private static Option<Hands> Triplet(this Hands hands) 
    => hands
        .Where(IsTriplet)
        .Select(SeparateTriplet)
        .GetResult(HandComparer.Default);


    private static Option<Hands> TwoPairs(this Hands hands)
    => hands
        .Where(IsTwoPairs)
        .Select(SeparatePair)
        .GetResult(HandComparer.Default);
    
    private static Option<Hands> OnePair(this Hands hands)
    => hands
        .Where(IsOnePair)
        .Select(SeparatePair)
        .GetResult(HandComparer.Default);
    

    private static Option<Hands> HighestCard(this Hands hands)
    => hands
        .Select(Separate)
        .GetResult(HandComparer.Default);

    private static Option<Hands> GetResult(this IEnumerable<Separation> matches, HandComparer comparer)
    => matches.Any() ? Some<Hands>(matches
            .GroupBy(match => match.SameRank, comparer)
            .OrderBy(grp => grp.Key, comparer).FirstOrDefault()
            .GroupBy(match => match.Kickers, comparer)
            .OrderBy(grp => grp.Key, comparer).FirstOrDefault()
            .Select(match => match.Origin)
            .ToArray())
            : None;

    public static bool HasPoorAce(this Hand hand)
    {
        var ranks = hand.Select(c => c.Rank).OrderBy(r => r);
        return ranks.First() == Rank.Ace && ranks.Last() == Rank.Two;
    }
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

    public static Card Create(string stmt) => new Card(stmt);

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


public class HandComparer : IComparer<Hand>, IEqualityComparer<Hand>
{
    private readonly bool compareStraight;
    private static readonly Lazy<HandComparer> @default = new Lazy<HandComparer>(() => new HandComparer(false), true);
    private static readonly Lazy<HandComparer> straight = new Lazy<HandComparer>(() => new HandComparer(true), true);

    public HandComparer(bool compareStraight)
    {
        this.compareStraight = compareStraight;
    }

    private IEnumerable<Rank> OrderByRank(Hand hand) =>
        this.compareStraight && hand.HasPoorAce() ?
            hand.Select(c => c.Rank).OrderBy(r => r).Skip(1).Append(Rank.PoorAce)
            :
            hand.Select(c => c.Rank).OrderBy(r => r);

    public int Compare(Hand x, Hand y)
    {

        // 4D 5S 6S 8D 3C", x =  8D 6S 5S 4D 3C
        // 2S 4C 7S 9H 10H" y = 10H 9H 7S 4C 2S
        // zip                   +1 +1 +1  0 -1

        var match = OrderByRank(x)
            .Zip(OrderByRank(y), (lhs, rhs) => lhs.CompareTo(rhs));
        return (match.All(cmp => cmp == 0)) ? 0 : match.FirstOrDefault(cmp => cmp != 0);
    }

    public bool Equals(Hand x, Hand y) => this.Compare(x, y) == 0;

    public int GetHashCode([DisallowNull] Hand hand) => hand
        .OrderBy(c => c.Rank)
        .Select(c => c.Rank.GetHashCode())
        .Aggregate(13, (accu, cur) => accu ^ cur);

    public static HandComparer Default => @default.Value;
    public static HandComparer Straight => straight.Value;

    private HandComparer() { }
}