using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
namespace Inworld.Collections
{
    public class LimitedSizeDictionary<TK, TV> :
        IDictionary<TK, TV>
    {
        readonly OrderedDictionary m_Inner = new OrderedDictionary();

        public LimitedSizeDictionary(int limit)
        {
            Limit = limit;
        }

        int Limit { get; }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<TK, TV>>)m_Inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TK, TV> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            m_Inner.Clear();
        }

        public void Add(TK key, TV value)
        {
            if (m_Inner.Contains(key))
                m_Inner.Remove(key);
            m_Inner.Add(key, value);
            if (m_Inner.Count < Limit)
                return;
            m_Inner.RemoveAt(0);
        }

        public bool ContainsKey(TK key)
        {
            return m_Inner.Contains(key);
        }

        public bool Remove(TK key)
        {
            bool flag = m_Inner.Contains(key);
            m_Inner.Remove(key);
            return flag;
        }

        public TV this[TK key]
        {
            get => (TV)m_Inner[key];
            set => m_Inner[key] = value;
        }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TK key, out TV value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            throw new NotImplementedException();
        }

        public int Count => m_Inner.Count;

        public bool IsReadOnly => m_Inner.IsReadOnly;

        public ICollection<TK> Keys => (ICollection<TK>)m_Inner.Keys;

        public ICollection<TV> Values => (ICollection<TV>)m_Inner.Values;
    }
}
