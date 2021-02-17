using System;
using System.Linq;
using System.Collections.Generic;

namespace Domain.Functors
{
    using static F;

    public static partial class F
    {
        public static Option<T> Some<T>(T value) => new Option.Some<T>(value);
        public static Option.None None => Option.None.Default;

        public static Option<T> NoneOf<T>() => Option.None.Default;
    }

    public struct Option<T> : IEquatable<Option.None>, IEquatable<Option<T>>
    {
        readonly T value;
        readonly bool isSome;
        bool isNone => !isSome;

        private Option(T value)
        {
            if (value == null)
                throw new ArgumentNullException();
            this.isSome = true;
            this.value = value;
        }

        public static implicit operator Option<T>(Option.None _)
            => new Option<T>();
        public static implicit operator Option<T>(Option.Some<T> some)
            => new Option<T>(some.Value);

        public static implicit operator Option<T>(T value)
        => value == null ? None : Some(value);

        public R Match<R>(Func<R> None, Func<T, R> Some)
        => isSome ? Some(value) : None();

        public IEnumerable<T> AsEnumerable()
        {
            if (isSome) yield return value;
        }

        public bool Equals(Option<T> other) => this.isSome == other.isSome
            && (this.isNone || this.value.Equals(other.value));

        public bool Equals(Option.None _) => isNone;

        public static bool operator ==(Option<T> @this, Option<T> other)
            => @this.Equals(other);
        public static bool operator !=(Option<T> @this, Option<T> other)
            => !(@this == other);

        public static Option<T> operator | (Option<T> left, Option<T> right)
            => left.Or(right);

        public static T operator | (Option<T> left, T right)
            => left.GetOrElse(right);

        public override string ToString()
            => isSome ? $"{nameof(Option.Some<T>)}({value})" : nameof(Option.None);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Option.None)
                return isNone;

            if (obj is Option.Some<T>)
                return this.Equals((Option<T>)obj);

            return false;
        }

        public override int GetHashCode() => isSome ? this.value.GetHashCode() : 0;

    }

    namespace Option
    {
        public struct None
        {
            internal static readonly None Default = new None();
        }

        public struct Some<T>
        {
            internal T Value { get; }

            internal Some(T value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Cannot wrap a null value in a 'Some'; use 'None' instead");
                Value = value;
            }
        }
    }

    public static class OptionExt
    {
        public static Option<R> Map<T, R>(this Option.None _, Func<T, R> f)
        => None;

        public static Option<R> Map<T, R>(this Option.Some<T> some, Func<T, R> f)
            => Some(f(some.Value));

        public static Option<R> Map<T, R>(this Option<T> t, Func<T, R> f)
        => t.Match(
            () => None,
            t => Some(f(t)));

        public static Option<R> Bind<T, R>(this Option<T> t, Func<T, Option<R>> f)
        => t.Match(
            () => None,
            t => f(t));

        public static IEnumerable<R> Bind<T, R>(
            this IEnumerable<T> ts, Func<T, IEnumerable<R>> f)
            => ts.SelectMany(f);


        public static IEnumerable<R> Bind<T, R>(
            this IEnumerable<T> ts, Func<T, Option<R>> f)
            => ts.Bind(t => f(t).AsEnumerable());


        internal static bool IsSome<T>(this Option<T> opt)
                => opt.Match(
                   () => false,
                   (_) => true);

                   

        public static T GetOrElse<T>(this Option<T> opt, T defaultValue)
        => opt.Match(
            () => defaultValue,
            (t) => t);

        public static T GetOrElse<T>(this Option<T> opt, Func<T> fallback)
            => opt.Match(
                () => fallback(),
                (t) => t);

        public static Option<A> Or<A>(this Option<A> left, Option<A> right)
            => left.IsSome() ? left : right;

        // LINQ

        public static Option<R> Select<T, R>(this Option<T> opt, Func<T, R> f)
            => opt.Map(f);

        public static Option<T> Where<T>
            (this Option<T> optT, Func<T, bool> predicate)
            => optT.Match(
                () => None,
                (t) => predicate(t) ? optT : None);

        public static Option<RR> SelectMany<T, R, RR>
            (this Option<T> opt, Func<T, Option<R>> bind, Func<T, R, RR> project)
            => opt.Match(
                () => None,
                (t) => bind(t).Match(
                    () => None,
                    (r) => Some(project(t, r))));

        public static IEnumerable<RR> SelectMany<T, R, RR>
         (this IEnumerable<T> source
         , Func<T, Option<R>> bind
         , Func<T, R, RR> project)
         => from t in source
            let opt = bind(t)
            where opt.IsSome()
            select project(t, opt.GetOrElse(default(R)));
    }
}