using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Infrastructure.Extensions
{
    public delegate void ListedItemPropertyChangedEventHandler(IList SourceList, object Item, PropertyChangedEventArgs e);
    public class DeepObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public Action<object, PropertyChangedEventArgs> Handler;

        public DeepObservableCollection() : base()
        {
            CollectionChanged += DeepObservableCollection_CollectionChanged!;
        }

        public DeepObservableCollection(IEnumerable<T> collection)
        {
            CopyFrom(collection);

            CollectionChanged += DeepObservableCollection_CollectionChanged!;
        }

        public DeepObservableCollection(IEnumerable<T> collection, Action<object, PropertyChangedEventArgs> handler)
        {
            CopyFrom(collection);

            Handler = handler;
            foreach (var item in Items)
            {
                ((INotifyPropertyChanged)item).PropertyChanged += new PropertyChangedEventHandler(Handler!);
                RecursivelyApplyEventHandler(item);
            }

            CollectionChanged += DeepObservableCollection_CollectionChanged!;
        }

        public void RecursivelyApplyEventHandler(INotifyPropertyChanged item)
        {
            foreach (var prop in item.GetType().GetProperties())
            {
                if (prop.PropertyType.IsPrimitive ||
                    Convert.GetTypeCode(prop.GetValue(item)) != TypeCode.Object) continue;
                ((prop.GetValue(item) as INotifyPropertyChanged)!).PropertyChanged += new PropertyChangedEventHandler(Handler!);
                RecursivelyApplyEventHandler((INotifyPropertyChanged)prop.GetValue(item)!);
            }
        }

        private void CopyFrom(IEnumerable<T> collection)
        {
            var items = Items;
            if (collection == null! || items == null!) return;
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }
        }

        private void DeepObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Handler == null!) return;
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= new PropertyChangedEventHandler(Handler);
                    RecursivelyApplyEventHandler((INotifyPropertyChanged)item);
                }
            }

            if (e.NewItems == null) return;
            {
                foreach (var item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += new PropertyChangedEventHandler(Handler);
                    RecursivelyApplyEventHandler((INotifyPropertyChanged)item);
                }
            }
        }
    }
}
