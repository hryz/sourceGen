using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace db
{
    public class DetachedHeadMap<TK,TV> : IReadOnlyDictionary<TK, TV>
    where TK : notnull
    {
        private readonly EqualityComparer<TK> _keyComparer = EqualityComparer<TK>.Default;
        private readonly EqualityComparer<TV> _valueComparer = EqualityComparer<TV>.Default;
        private TK? _headKey;
        private TV? _headValue;
        private ConcurrentDictionary<TK, TV>? _tail;

        [MethodImpl(AggressiveInlining)]
        public bool ZeroElements() =>
               _keyComparer.Equals(_headKey, default)
            && _valueComparer.Equals(_headValue, default)
            && _tail == null;
        
        [MethodImpl(AggressiveInlining)]
        public bool OneElement() => 
               !_keyComparer.Equals(_headKey, default)
            && !_valueComparer.Equals(_headValue, default)
            && _tail == null;
        
        [MethodImpl(AggressiveInlining)]
        public bool ManyElements() => 
               _keyComparer.Equals(_headKey, default)
            && _valueComparer.Equals(_headValue, default)
            && _tail != null;

        private const string InvalidState = "Invalid state";
        
        public bool IsEmpty
        {
            get
            {
                if (ZeroElements())
                    return true;

                if(OneElement())
                    return false;
                
                if(ManyElements())
                    return _tail!.IsEmpty; //the map can actually be empty

                throw new Exception(InvalidState);
            }
        }

        public TV this[TK key]
        {
            get // is needed for IReadOnlyDictionary
            {
                if (ZeroElements())
                {
                    throw new KeyNotFoundException(); //Concurrent dictionary does this 🤷‍
                }

                if(OneElement())
                {
                    if (_keyComparer.Equals(key, _headKey))
                    {
                        return _headValue!;
                    }
                    else
                    {
                        throw new KeyNotFoundException();
                    }
                }

                if(ManyElements())
                {
                    return _tail![key];
                }

                throw new Exception(InvalidState);
            }
            set
            {
                if (ZeroElements())
                {
                    _headKey = key;
                    _headValue = value;
                }
                else if (OneElement())
                {
                    if (_keyComparer.Equals(_headKey,key)) //update head
                    {
                        _headValue = value;
                    }
                    else //insert 2nd item
                    {
                        _tail = new ConcurrentDictionary<TK, TV>(); //create the tail
                        _tail[_headKey!] = _headValue!; //move the head into tail
                        _tail[key] = value; //add the 2nd element
                        _headKey = default; //clean the detached head
                        _headValue = default;
                    }
                }
                else if (ManyElements())
                {
                    _tail![key] = value;
                }
            }
        }

        public bool TryRemove(TK key, [MaybeNullWhen(false)]out TV value)
        {
            if (ZeroElements())
            {
                value = default;
                return false;
            }

            if (OneElement())
            {
                if (_keyComparer.Equals(_headKey,key)) //remove head
                {
                    value = _headValue!;
                    _headKey = default;
                    _headValue = default;
                    return true;
                }
                else //nothing to remove
                {
                    value = default;
                    return false;
                }
            }

            if (ManyElements())
            {
                return _tail!.TryRemove(key, out value);
            }

            throw new Exception(InvalidState);
        }

        public bool ContainsKey(TK key)
        {
            if (ZeroElements()) 
            {
                return false;
            }

            if (OneElement())
            {
                if (_keyComparer.Equals(_headKey,key)) //return head
                {
                    return true;
                }
                else //nothing to return
                {
                    return false;
                }
            }

            if (ManyElements())
            {
                return _tail!.ContainsKey(key);
            }

            throw new Exception(InvalidState);
        }

        public bool TryGetValue(TK key, [MaybeNullWhen(false)]out TV value)
        {
            if (ZeroElements())
            {
                value = default;
                return false;
            }

            if (OneElement())
            {
                if (_keyComparer.Equals(_headKey,key)) //return head
                {
                    value = _headValue!;
                    return true;
                }
                else //nothing to return
                {
                    value = default;
                    return false;
                }
            }

            if (ManyElements())
            {
                return _tail!.TryGetValue(key, out value);
            }

            throw new Exception(InvalidState);
        }


        public IEnumerable<TK> Keys
        {
            get
            {
                if(ZeroElements())
                    return Enumerable.Empty<TK>();

                if (OneElement())
                    return Enumerable.Repeat(_headKey!, 1);

                if (ManyElements())
                    return _tail!.Keys;
                
                throw new Exception(InvalidState);
            }
        }

        public IEnumerable<TV> Values
        {
            get
            {
                if(ZeroElements())
                    return Enumerable.Empty<TV>();

                if (OneElement())
                    return Enumerable.Repeat(_headValue!, 1);

                if (ManyElements())
                    return _tail!.Values;
                
                throw new Exception(InvalidState);
            }
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            if(ZeroElements())
                return Enumerable.Empty<KeyValuePair<TK, TV>>().GetEnumerator();

            if (OneElement())
                return Enumerable.Repeat(new KeyValuePair<TK, TV>(_headKey!, _headValue!), 1).GetEnumerator();

            if (ManyElements())
                return _tail!.GetEnumerator();
                
            throw new Exception(InvalidState);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                if(ZeroElements())
                    return 0;

                if (OneElement())
                    return 1;

                if (ManyElements())
                    return _tail!.Count;
                
                throw new Exception(InvalidState);
            }
        }
    }
}