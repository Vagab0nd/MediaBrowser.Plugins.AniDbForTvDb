using System;

namespace MediaBrowser.Plugins.Anime
{
    public interface IOption<out T>
    {
        void Match(Action<T> some, Action none);
    }

    public abstract class Option
    {
        public static IOption<T> Optionify<T>(T item)
        {
            if (item != null)
            {
                return new Some<T>(item);
            }

            return new None<T>();
        }

        private class Some<T> : Option, IOption<T>
        {
            private readonly T _value;

            public Some(T val)
            {
                _value = val;
            }

            public void Match(Action<T> some, Action none)
            {
                some(_value);
            }

            public override bool Equals(object obj)
            {
                var result = false;

                var some = obj as Some<T>;

                if (some != null)
                {
                    if (some._value.Equals(_value))
                    {
                        result = true;
                    }
                }

                return result;
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            public override string ToString()
            {
                return $"Some<{typeof(T).Name}>({_value})";
            }
        }

        private class None<T> : Option, IOption<T>
        {
            public void Match(Action<T> some, Action none)
            {
                none();
            }

            public override bool Equals(object obj)
            {
                var result = obj is None<T>;

                return result;
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public override string ToString()
            {
                return "None";
            }
        }
    }
}