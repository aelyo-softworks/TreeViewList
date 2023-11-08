using System.Collections;
using System.Collections.Generic;

namespace TreeViewList
{
    public abstract class BaseList<T> : IEnumerable<T>
    {
        private readonly List<T> _list = new();

        protected BaseList() { }

        public int Count => _list.Count;
        public T this[int index] => _list[index];
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        protected virtual void Add(T item) => _list.Add(item);
        protected virtual void Insert(int index, T item) => _list.Insert(index, item);
        protected virtual void RemoveAt(int index) => _list.RemoveAt(index);
        protected virtual bool Remove(T item) => _list.Remove(item);
        protected virtual void Clear() => _list.Clear();
        protected virtual int IndexOf(T item) => _list.IndexOf(item);
    }
}