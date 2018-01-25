using System.Collections.Generic;
using System.Linq;

namespace SoundChange.StateMachines
{
    /// <summary>
    /// Provides a reversible iterator over an enumerable that may be mutated.
    /// </summary>
    /// <typeparam name="T">The type of objects to iterate over.</typeparam>
    class Window<T>
    {
        private List<T> _contents;

        private int _index = 0;

        /// <summary>
        /// Gets the complete contents.
        /// </summary>
        public IReadOnlyCollection<T> Contents
        {
            get
            {
                return _contents.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the object at the window's current position.
        /// </summary>
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

        /// <summary>
        /// Gets the current indes of the window.
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the window is positioned at the beginning.
        /// </summary>
        public bool AtBeginning
        {
            get
            {
                return _index == 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the window is positioned at the end.
        /// </summary>
        public bool AtEnd
        {
            get
            {
                return _index == _contents.Count - 1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are more contents past the current position of the window.
        /// </summary>
        public bool HasNext
        {
            get
            {
                return _index >= 0 && _index < _contents.Count - 1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the window is out of bounds.
        /// </summary>
        public bool IsOutOfBounds
        {
            get
            {
                return _index < 0 || _index >= _contents.Count;
            }
        }

        public Window(IEnumerable<T> contents)
        {
            _contents = contents.ToList();
        }

        /// <summary>
        /// Moves the window forward.
        /// </summary>
        /// <returns>True if the window is still in bounds; otherwise, false.</returns>
        public bool MoveNext(int offset = 1)
        {
            _index += offset;
            return !IsOutOfBounds;
        }

        /// <summary>
        /// Moves the window back.
        /// </summary>
        /// <returns>True if the window is still in bounds; otherwise, false.</returns>
        public bool MoveBack(int offset = 1)
        {
            _index -= offset;
            return !IsOutOfBounds;
        }

        /// <summary>
        /// Returns the item at the current position and moves the position forward.
        /// </summary>
        /// <returns></returns>
        public T Read()
        {
            return _contents[_index++];
        }

        /// <summary>
        /// Inserts an item in the contents.
        /// </summary>
        /// <param name="index">The index where the item will be inserted.</param>
        /// <param name="item">The item to insert.</param>
        public void Insert(int index, T item)
        {
            _contents.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the contents.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            _contents.RemoveAt(index);
        }
    }

    static class WindowExtensions
    {
        /// <summary>
        /// Creates a window over an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <param name="list">The list of objects to create a window over.</param>
        /// <returns></returns>
        public static Window<T> ToWindow<T>(this IEnumerable<T> list)
        {
            return new Window<T>(list);
        }
    }
}
