using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace System.Windows
{
	public class ResourceDictionary : System.Object, IDictionary
	{
		public System.Boolean Contains(System.Object param0) {
			if (innerDictionary.ContainsKey (param0))
				return true;
			if (_mergedDictionaries != null) {
				for (int i = MergedDictionaries.Count - 1; i >= 0; i--) {
					var mergedDictionary = MergedDictionaries[i];
					if (mergedDictionary != null && mergedDictionary.Contains(param0))
						return true;
				}
			}
			return false;
		}

		public ResourceDictionary() { }
		public void Add(System.Object param0, System.Object param1) { innerDictionary[param0] = param1; }

		public void Clear()
		{
			innerDictionary.Clear();
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public void Remove(object key)
		{
			innerDictionary.Remove(key);
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return innerDictionary.GetEnumerator();
		}

		private ObservableCollection<ResourceDictionary> _mergedDictionaries = null;
		public System.Collections.ObjectModel.Collection<System.Windows.ResourceDictionary> MergedDictionaries {
			get {
				if (_mergedDictionaries == null) {
					_mergedDictionaries = new ObservableCollection<ResourceDictionary>();
				}
				return _mergedDictionaries;
			}
		}

		public bool IsFixedSize => throw new NotImplementedException();

		public bool IsReadOnly => throw new NotImplementedException();

		public ICollection Keys => innerDictionary.Keys;

		public ICollection Values => innerDictionary.Values;

		public int Count => throw new NotImplementedException();

		public bool IsSynchronized => throw new NotImplementedException();

		public object SyncRoot => throw new NotImplementedException();

		public bool HasImplicitDataTemplates { get; set; }
		public bool HasImplicitStyles { get; private set; }
		public bool IsThemeDictionary { get; private set; }
		public bool IsInitialized { get; private set; }
		public bool InvalidatesImplicitDataTemplateResources { get; private set; }

		Dictionary<object, object> innerDictionary = new Dictionary<object, object>();

		public object this[object key] {
			get {
				if (innerDictionary.TryGetValue(key, out var val))
					return val;
                for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
                {
                    var merged = MergedDictionaries [i];
                    val = merged [key];
                    if (val != null)
                        return val;
                }

                return null;
			}
			set { innerDictionary[key] = value; }
		}
	}
}