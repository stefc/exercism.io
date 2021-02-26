using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Hand = System.Collections.Generic.IEnumerable<Card>;

using OrderRanks = System.Func<System.Collections.Generic.IEnumerable<Card>,System.Collections.Generic.IEnumerable<Rank>>;

public static class HigherOrderFunctions {
    public static Func<TInput1, TOutput> If<TInput1, TOutput>(this Func<TInput1, TOutput> f, 
        Func<TInput1,bool> predicate, Func<TOutput,TOutput> iif) 
    => x => predicate(x) ? iif(f(x)) : f(x);
}

public class HandComparer : IComparer<Hand>, IEqualityComparer<Hand>
{
    private readonly OrderRanks rankOrdering;
    private static readonly Lazy<HandComparer> @default = 
        new Lazy<HandComparer>(() => new HandComparer(DefaultRankOrder), true);
    private static readonly Lazy<HandComparer> straight =
        new Lazy<HandComparer>(() => new HandComparer(StraightRankOrder), true);

    private HandComparer(OrderRanks rankOrdering)
    {
        this.rankOrdering = rankOrdering;
    }

    private static OrderRanks DefaultRankOrder 
    => x => x.Select(c => c.Rank).OrderBy(r => r);

    private static OrderRanks StraightRankOrder
    => DefaultRankOrder.If(hand => hand.HasPoorAce(), ranks => ranks.Skip(1).Append(Rank.PoorAce));

    public int Compare(Hand x, Hand y)
        // 4D 5S 6S 8D 3C", x =  8D 6S 5S 4D 3C
        // 2S 4C 7S 9H 10H" y = 10H 9H 7S 4C 2S
        // zip                   +1 +1 +1  0 -1

    => rankOrdering(x)
        .Zip(rankOrdering(y), (leftRank, rightRank) => leftRank.CompareTo(rightRank))
        .FirstOrDefault(cmp => cmp is not 0);
    

    public bool Equals(Hand x, Hand y) => this.Compare(x, y) is 0;

    public int GetHashCode([DisallowNull] Hand hand) => hand
        .OrderBy(c => c.Rank)
        .Select(c => c.Rank.GetHashCode())
        .Aggregate(13, (accu, cur) => accu ^ cur);

    public static HandComparer Default => @default.Value;
    public static HandComparer Straight => straight.Value;

    private HandComparer() { }
}