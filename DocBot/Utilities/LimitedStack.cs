using System.Collections;
using System.Collections.Generic;

namespace DocBot.Utilities
{
    internal class LimitedStack<T> : IEnumerable<T>
    {
        public int Count => list.Count;

        private readonly int limit;
        private readonly LinkedList<T> list;

        public LimitedStack(int limit)
        {
            this.limit = limit;
            list = new LinkedList<T>();
        }

        public void Push(T value)
        {
            if (IsFull())
                list.RemoveLast();

            list.AddFirst(value);
        }

        public bool IsFull() => list.Count == limit;

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
    }
}
