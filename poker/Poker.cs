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
    public static Hand ToHand(this string hand) => hand.Split().Select(Card.Create);

    public static string ToString(this Hand hand) => String.Join(' ', hand.Select(card => card.ToString()));

    public static Hands BestHands(Hands hands)
    => (hands.StraightFlush() | hands.Quad() | hands.Fullhouse()
        | hands.Flush() | hands.Straight() | hands.Triplet()
        | hands.TwoPairs() | hands.OnePair() | hands.HighestCard())
        .GetOrElse(Enumerable.Empty<string>());

    public static (Hand sameRank, Hand kickers, string origin) Split(this string hand, int count = 2)
    {
        //  "4S 2H 6S 2D JH" -> "2H 2D" sameRank, "4S 6S JH" kickers  origin = .. 
        var x = hand.ToHand().GroupBy(card => card.Rank);
        return (
            sameRank: x.Where(grp => grp.Count() == count).SelectMany(grp => grp).ToArray(),
            kickers: x.Where(grp => grp.Count() != count).SelectMany(grp => grp).ToArray(),
            origin: hand);
    }

    public static bool HasPoorAce(this Hand hand)
    {
        var ranks = hand.Select(c => c.Rank).OrderBy(r => r);
        return ranks.First() == Rank.Ace && ranks.Last() == Rank.Two;
    }

    public static bool IsStraightFlush(this string hand) => hand.ToHand().IsStraightFlush();


    public static bool IsStraightFlush(this Hand hand)
        => hand.IsStraight() && hand.IsFlush();


    public static bool IsFlush(this string hand) => hand.ToHand().IsFlush();

    public static bool IsFlush(this Hand hand)
        => hand.Select(card => card.Suit).Distinct().Count() == 1;

    public static bool IsFullhouse(this string hand) => hand.ToHand().IsFullhouse();

    public static bool IsFullhouse(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 2 && (hand.IsTriplet());

    public static bool IsTwoPairs(this string hand) => hand.ToHand().IsTwoPairs();
    public static bool IsTwoPairs(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 3 && (hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 1));

    public static bool IsOnePair(this string hand) => hand.ToHand().IsOnePair();
    public static bool IsOnePair(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 4 && (hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 2));

    public static bool IsQuad(this string hand) => hand.ToHand().IsQuad();
    public static bool IsQuad(this Hand hand)
        => hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 4);

    public static bool IsTriplet(this string hand) => hand.ToHand().IsTriplet();
    public static bool IsTriplet(this Hand hand)
        => hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 3);

    public static bool IsStraight(this string hand) => hand.ToHand().IsStraight();

    public static bool IsStraight(this Hand hand)
    {
        var x = hand.Select(card => card.Rank).OrderBy(rank => rank).Distinct();
        return x.Count() != 5 ?
            false
            :
            x.Last() - x.First() == 4 ?
                true
                :
                hand.HasPoorAce() ?
                    x.Last() - x.Skip(1).First() == 3 ?
                        true
                        :
                        false
                    :
                    false;
    }

    public static Option<Hands> StraightFlush(this Hands hands)
    {
        var result = hands
            .Where(IsStraightFlush)
            .Select(hand => hand.Split(1))
            .GetResult(HandComparer.Straight);
        return result.Any() ? Some<Hands>(result) : None;
    }

    public static Option<Hands> Quad(this Hands hands)
    {
        var result = hands
            .Where(hand => hand.ToHand().IsQuad())
            .Select(hand => hand.Split(4))
            .GetResult(HandComparer.Default);
        return result.Any() ? Some<Hands>(result) : None;
    }

    public static Option<Hands> Fullhouse(this Hands hands)
    {
        var result = hands
            .Where(IsFullhouse)
            .Select(hand => hand.Split(3))
            .GetResult(HandComparer.Default);
        return result.Any() ? Some<Hands>(result) : None;
    }

    public static Option<Hands> Flush(this Hands hands)
    {
        var result = hands
            .Where(IsFlush)
            .Select(hand => hand.Split(1))
            .GetResult(HandComparer.Default);

        return result.Any() ? Some<Hands>(result) : None;
    }
    public static Option<Hands> Straight(this Hands hands)
    {
        var result = hands
            .Where(IsStraight)
            .Select(hand => hand.Split(1))
            .GetResult(HandComparer.Straight);

        return result.Any() ? Some<Hands>(result) : None;
    }

    public static Option<Hands> Triplet(this Hands hands)
    {
        var result = hands
            .Where(IsTriplet)
            .Select(hand => hand.Split(3))
            .GetResult(HandComparer.Default);

        return result.Any() ? Some<Hands>(result) : None;
    }

    public static Option<Hands> TwoPairs(this Hands hands)
    {
        var result = hands
            .Where(IsTwoPairs)
            .Select(hand => hand.Split(2))
            .GetResult(HandComparer.Default);
        return result.Any() ? Some<Hands>(result) : None;
    }

    public static Option<Hands> OnePair(this Hands hands)
    {
        var result = hands
            .Where(IsOnePair)
            .Select(hand => hand.Split(2))
            .GetResult(HandComparer.Default);
        return result.Any() ? Some<Hands>(result) : None;
    }

    public static Option<Hands> HighestCard(this Hands hands)
    => Some<Hands>(hands
        .GroupBy(hand => hand.ToHand(), HandComparer.Default)
        .OrderBy(grp => grp.Key, HandComparer.Default)
        .First()
        .ToArray());

    private static Hands GetResult(this IEnumerable<(Hand sameRank, Hand kickers, string origin)> matches, HandComparer comparer)
    => matches.Any() ? matches
            .GroupBy(match => match.sameRank, comparer)
            .OrderBy(grp => grp.Key, comparer).FirstOrDefault()
            .GroupBy(match => match.kickers, comparer)
            .OrderBy(grp => grp.Key, comparer).FirstOrDefault()
            .Select(match => match.origin)
            .ToArray()
            : Enumerable.Empty<string>();
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