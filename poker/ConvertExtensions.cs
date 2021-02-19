using Hand = System.Collections.Generic.IEnumerable<Card>;

using System;
using System.Linq;

public static partial class Poker
{
    private static Hand ToHand(this string hand) => hand.Split().Select(Card.Create);

    private static string ToString(this Hand hand) => String.Join(' ', hand.Select(card => card.ToString()));

    private record Separation (Hand SameRank, Hand Kickers, Hand Origin);

    private static Separation Separate(this Hand hand, byte count)
    {
        //  "4S 2H 6S 2D JH" -> "2H 2D" sameRank, "4S 6S JH" kickers  origin = .. 
        var x = hand.GroupBy(card => card.Rank);
        return new (
            x.Where(grp => grp.Count() == count).SelectMany(grp => grp).ToArray(),
            x.Where(grp => grp.Count() != count).SelectMany(grp => grp).ToArray(),
            hand);
    }

    private static Separation Separate(this Hand hand)
        => Separate(hand, 1);
    private static Separation SeparatePair(this Hand hand)
        => Separate(hand, 2);
    private static Separation SeparateTriplet(this Hand hand)
        => Separate(hand, 3);
    private static Separation SeparateQuad(this Hand hand)
        => Separate(hand, 4);


}

