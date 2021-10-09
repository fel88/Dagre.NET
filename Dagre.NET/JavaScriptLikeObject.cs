using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dagre
{
    public class JavaScriptLikeObject : IDictionary<string, object>
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        bool _isFreezed;


        
        public ICollection<string> Keys
        {
            get => commonKeys;
        }

        List<string> commonKeys = new List<string>();
        List<int> digitsKeys = new List<int>();        
        List<string> otherKeys = new List<string>();

        public ICollection<object> Values
        {
            get
            {

                List<object> ret = new List<object>();
                foreach (var item in Keys)
                {
                    ret.Add(dic[item]);
                }
                return ret.ToArray();
            }

        }
        void deleteKey(string key)
        {
            if (key.All(char.IsDigit) && int.Parse(key).ToString() == key)
            {
                var ind = binSearchInsertIndex(int.Parse(key));
                digitsKeys.RemoveAt(ind);                
                commonKeys.RemoveAt(ind);
            }
            else
            {
                otherKeys.Remove(key);
                commonKeys.Remove(key);
            }
        }
        void insertKey(string key)
        {
            if (key.All(char.IsDigit) && int.Parse(key).ToString() == key)
            {
                var v = int.Parse(key);
                //binsearch
                int index = binSearchInsertIndex(v);
                digitsKeys.Insert(index, v);                
                commonKeys.Insert(index, key);
            }
            else
            {
                otherKeys.Add(key);
                commonKeys.Add(key);
            }
        }

        private int binSearchInsertIndex(int key)
        {

            int low = 0;

            int high = digitsKeys.Count;
            while (true)
            {
                if (low >= high) break;
                var m = (high - low) / 2 + low;
                if (digitsKeys[m] < key)
                {
                    if (low == m)
                    {
                        return low + 1;
                    }
                    low = m;
                }
                else if (digitsKeys[m] == key)
                {
                    return m;
                    //throw new DagreException("duplicate key");
                }
                else
                {
                    if (high == m)
                    {
                        throw new DagreException("err");

                    }
                    high = m;
                }

            }
            return low;

        }
        public int Count => dic.Count;

        public bool IsReadOnly => throw new System.NotImplementedException();

        public object this[string key]
        {
            get => dic[key];
            set
            {
                AddOrUpdate(key, value);
            }
        }
        public void Freeze()
        {
            _isFreezed = true;
        }
        public object Tag;
        public RankTag RankTag;
        public void AddOrUpdate(string key, object val)
        {
            if (_isFreezed) throw new DagreException("can't add to frozen object");
            if (dic.ContainsKey(key))
            {
                dic[key] = val;
                return;
            }            

            dic.Add(key, val);
            insertKey(key);            
        }
                
        public override string ToString()
        {
            return $"dic ({dic.Keys.Count})";
        }

        public bool ContainsKey(string key)
        {
            return dic.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            if (_isFreezed) throw new DagreException("can't add to frozen object");            
            dic.Add(key, value);
            insertKey(key);            
        }

        public bool Remove(string key)
        {
            if (_isFreezed) throw new DagreException("can't remove from frozen object");            
            deleteKey(key);
            var ret = dic.Remove(key);            
            return ret;
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new System.NotImplementedException();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (_isFreezed) return;            
            dic.Add(item.Key, item.Value);
            insertKey(item.Key);            
        }

        public void Clear()
        {
            dic.Clear();
            otherKeys.Clear();
            digitsKeys.Clear();
            commonKeys.Clear();            
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            //throw new NotImplementedException();

            return dic.GetEnumerator();

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //return new JavaDicEnumerator(this);
            throw new NotImplementedException();
            return dic.GetEnumerator();
        }

        public static JavaScriptLikeObject FromObject(object p)
        {
            JavaScriptLikeObject ret = new JavaScriptLikeObject();
            foreach (var item in p.GetType().GetProperties())
            {
                ret.Add(item.Name, item.GetValue(p));
            }
            return ret;
        }
    }
}
