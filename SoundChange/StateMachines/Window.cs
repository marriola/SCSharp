using System.Collections.Generic;

namespace SoundChange.StateMachines
{
    class Window<T>
    {
        private List<T> _contents;

        private int _index = 0;

        public List<T> Contents
        {
            get
            {
                return _contents;
            }
        }

        public T Current
        {
            get
            {
                if (IsOutOfBounds)
                {
                    return default(T);
                }
                else
                {
                    return _contents[_index];
                }
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }
        }

        public bool AtBeginning
        {
            get
            {
                return _index == 0;
            }
        }

        public bool AtEnd
        {
            get
            {
                return _index == _contents.Count - 1;
            }
        }

        public bool IsOutOfBounds
        {
            get
            {
                return _index < 0 || _index >= _contents.Count;
            }
        }

        public Window(List<T> contents)
        {
            _contents = contents;
        }

        public bool MoveNext()
        {
            ++_index;
            return !IsOutOfBounds;
        }

        public bool MoveBack()
        {
            --_index;
            return !IsOutOfBounds;
        }
    }

    static class WindowExtensions
    {
        public static Window<T> ToWindow<T>(this List<T> list)
        {
            return new Window<T>(list);
        }
    }
}
