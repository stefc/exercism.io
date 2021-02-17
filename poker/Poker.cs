using System.Collections.Generic;
using System.Linq;

using Domain.Functors;

// 1. Type Aliasing mit 'using'
using Hands = System.Collections.Generic.IEnumerable<string>;

using static Domain.Functors.F;

public static partial class Poker
{
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

    private static Option<Hands> StraightFlush(this Hands hands)
    => hands
        .Where(IsStraightFlush)
        .Select(Separate)
        .GetResult(HandComparer.Straight);

    private static Option<Hands> Quad(this Hands hands)
    => hands
        .Where(IsQuad)
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
}