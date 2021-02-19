using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Hand = System.Collections.Generic.IEnumerable<Card>;

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