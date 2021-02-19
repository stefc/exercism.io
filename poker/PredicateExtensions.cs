using System.Linq;

using Hand = System.Collections.Generic.IEnumerable<Card>;

public static partial class Poker
{
    #region Predicates
    private static bool IsStraightFlush(this Hand hand)
        => hand.IsStraight() && hand.IsFlush();

    private static bool IsFlush(this Hand hand)
        => hand.Select(card => card.Suit).Distinct().Count() == 1;

    private static bool IsFullhouse(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 2 && (hand.IsTriplet());

    private static bool IsTwoPairs(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 3
            && (hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 1));

    private static bool IsOnePair(this Hand hand)
        => hand.GroupBy(card => card.Rank).Count() == 4
            && (hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 2));

    private static bool IsQuad(this Hand hand)
        => hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 4);

    private static bool IsTriplet(this Hand hand)
        => hand.GroupBy(card => card.Rank).Any(grp => grp.Count() == 3);

    private static bool IsStraight(this Hand hand)
    {
        var x = hand.Select(card => card.Rank).OrderBy(rank => rank).Distinct();
        return
            x.Count() == 5
            && x.Last() - x.First() == 4
            || hand.HasPoorAce() && x.Last() - x.Skip(1).First() == 3;
    }

    public static bool HasPoorAce(this Hand hand)
    {
        var ranks = hand.Select(c => c.Rank).OrderBy(r => r);
        return ranks.First() == Rank.Ace && ranks.Last() == Rank.Two;
    }
    #endregion
}