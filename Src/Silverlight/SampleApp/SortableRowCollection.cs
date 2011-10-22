using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Linq;

using FileDbNs;

namespace SampleApp
{
    class SortableRowCollection: ObservableCollection<Record>, ICollectionView
    {
        CustomSortDescriptionCollection _sort;

        bool _suppressCollectionChanged = false;

        object _currentItem;

        CultureInfo _culture;

        int _currentPosition;

        Predicate<object> _filter;

        ObservableCollection<GroupDescription> _groupDescriptions;

        //-----------------------------------------------------------------------------------------
        public SortableRowCollection( Records rows )
        {
            _currentItem = null;
            _currentPosition = -1;

            foreach( Record record in rows )
                this.Add( record );
        }


        protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
        {
            if( _suppressCollectionChanged )
                return;

            base.OnCollectionChanged( e );
        }

        protected override void SetItem( int index, Record item )
        {
            base.SetItem( index, item );
        }

        protected override void InsertItem( int index, Record item )
        {
            base.InsertItem( index, item );

            if( 0 == index || null == _currentItem )
            {
                _currentItem = item;
                _currentPosition = index;
            }
        }

        public virtual object GetItemAt( int index )
        {
            if( (index >= 0) && (index < this.Count) )
            {
                return this[index];
            }

            return null;
        }


        #region ICollectionView Members

        public bool CanFilter
        {
            get { return false; }
        }

        public bool CanGroup
        {
            get { return false; }
        }

        public bool CanSort
        {
            get { return true; }
        }

        public bool Contains( object item )
        {
            if( !IsValidType( item ) )
            {
                return false;
            }

            return base.Contains( (Record) item );
        }

        private bool IsValidType( object item )
        {
            return item is Record;
        }

        public System.Globalization.CultureInfo Culture
        {
            get
            {
                return this._culture;
            }
            set
            {
                if( this._culture != value )
                {
                    this._culture = value;
                    this.OnPropertyChanged( new PropertyChangedEventArgs( "Culture" ) );
                }
            }
        }

        public event EventHandler CurrentChanged;

        public event CurrentChangingEventHandler CurrentChanging;

        public object CurrentItem
        {
            get { return this._currentItem; }
        }

        public int CurrentPosition
        {
            get { return this._currentPosition; }
        }

        public IDisposable DeferRefresh()
        {
            return new SortableCollectionDeferRefresh( this );
        }

        public Predicate<object> Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public ObservableCollection<GroupDescription> GroupDescriptions
        {
            get
            {
                //if( _groupDescriptions == null )
                //    _groupDescriptions = new ObservableCollection<GroupDescription>();
                return _groupDescriptions;
            }
        }

        public ReadOnlyObservableCollection<object> Groups
        {
            get
            {
                return new ReadOnlyObservableCollection<object>( new ObservableCollection<object>() );
            }

        }

        public bool IsCurrentAfterLast
        {
            get
            {
                if( !this.IsEmpty )
                {
                    return (this.CurrentPosition >= this.Count);
                }
                return true;
            }
        }

        public bool IsCurrentBeforeFirst
        {
            get
            {
                if( !this.IsEmpty )
                {
                    return (this.CurrentPosition < 0);
                }
                return true;
            }
        }

        protected bool IsCurrentInSync
        {
            get
            {
                if( this.IsCurrentInView )
                {
                    return (this.GetItemAt( this.CurrentPosition ) == this.CurrentItem);
                }

                return (this.CurrentItem == null);
            }
        }

        private bool IsCurrentInView
        {
            get
            {
                return ((0 <= this.CurrentPosition) && (this.CurrentPosition < this.Count));
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.Count == 0);
            }
        }

        public bool MoveCurrentTo( object item )
        {
            if( !IsValidType( item ) )
            {
                return false;
            }

            if( object.Equals( this.CurrentItem, item ) && ((item != null) || this.IsCurrentInView) )
            {
                return this.IsCurrentInView;
            }

            int index = this.IndexOf( (Record) item );

            return this.MoveCurrentToPosition( index );
        }

        public bool MoveCurrentToFirst()
        {
            return this.MoveCurrentToPosition( 0 );
        }

        public bool MoveCurrentToLast()
        {
            return this.MoveCurrentToPosition( this.Count - 1 );
        }

        public bool MoveCurrentToNext()
        {
            return ((this.CurrentPosition < this.Count) && this.MoveCurrentToPosition( this.CurrentPosition + 1 ));
        }

        public bool MoveCurrentToPrevious()
        {
            return ((this.CurrentPosition >= 0) && this.MoveCurrentToPosition( this.CurrentPosition - 1 ));
        }

        public bool MoveCurrentToPosition( int position )
        {
            if( (position < -1) || (position > this.Count) )
            {
                throw new ArgumentOutOfRangeException( "position" );
            }

            if( ((position != this.CurrentPosition) || !this.IsCurrentInSync) && this.OKToChangeCurrent() )
            {
                bool isCurrentAfterLast = this.IsCurrentAfterLast;
                bool isCurrentBeforeFirst = this.IsCurrentBeforeFirst;

                ChangeCurrentToPosition( position );

                OnCurrentChanged();

                if( this.IsCurrentAfterLast != isCurrentAfterLast )
                {
                    this.OnPropertyChanged( "IsCurrentAfterLast" );
                }

                if( this.IsCurrentBeforeFirst != isCurrentBeforeFirst )
                {
                    this.OnPropertyChanged( "IsCurrentBeforeFirst" );
                }

                this.OnPropertyChanged( "CurrentPosition" );
                this.OnPropertyChanged( "CurrentItem" );
            }

            return this.IsCurrentInView;
        }

        private void ChangeCurrentToPosition( int position )
        {
            if( position < 0 )
            {
                this._currentItem = null;
                this._currentPosition = -1;
            }
            else if( position >= this.Count )
            {
                this._currentItem = null;
                this._currentPosition = this.Count;
            }
            else
            {
                this._currentItem = this[position];
                this._currentPosition = position;
            }
        }

        protected bool OKToChangeCurrent()
        {
            CurrentChangingEventArgs args = new CurrentChangingEventArgs();
            this.OnCurrentChanging( args );
            return !args.Cancel;
        }

        protected virtual void OnCurrentChanged()
        {
            if( this.CurrentChanged != null )
            {
                this.CurrentChanged( this, EventArgs.Empty );
            }
        }

        protected virtual void OnCurrentChanging( CurrentChangingEventArgs args )
        {
            if( args == null )
            {
                throw new ArgumentNullException( "args" );
            }

            if( this.CurrentChanging != null )
            {
                this.CurrentChanging( this, args );
            }
        }

        protected void OnCurrentChanging()
        {
            this._currentPosition = -1;
            this.OnCurrentChanging( new CurrentChangingEventArgs( false ) );
        }

        protected override void ClearItems()
        {
            OnCurrentChanging();
            base.ClearItems();
        }

        private void OnPropertyChanged( string propertyName )
        {
            this.OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
        }

        public void Refresh()
        {
            IEnumerable<Record> rows = this;
            IOrderedEnumerable<Record> orderedRows = null;

            // use the OrderBy and ThenBy LINQ extension methods to
            // sort our data
            bool firstSort = true;
            for( int sortIndex = 0; sortIndex < _sort.Count; sortIndex++ )
            {
                SortDescription sort = _sort[sortIndex];
                Func<Record, object> function = record => record[sort.PropertyName];
                if( firstSort )
                {
                    orderedRows = sort.Direction == ListSortDirection.Ascending ?
                        rows.OrderBy( function ) : rows.OrderByDescending( function );

                    firstSort = false;
                }
                else
                {
                    orderedRows = sort.Direction == ListSortDirection.Ascending ?
                        orderedRows.ThenBy( function ) : orderedRows.ThenByDescending( function );
                }
            }

            _suppressCollectionChanged = true;

            // re-order this collection based on the result if there is any ordring
            if( orderedRows != null )
            {
                int index = 0;
                foreach( var record in orderedRows )
                {
                    this[index++] = record;
                }
            }

            _suppressCollectionChanged = false;

            // raise the required notification
            this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
        }

        public SortDescriptionCollection SortDescriptions
        {
            get
            {
                if( this._sort == null )
                {
                    this.SetSortDescriptions( new CustomSortDescriptionCollection() );
                }
                return this._sort;
            }
        }

        private void SetSortDescriptions( CustomSortDescriptionCollection descriptions )
        {
            if( this._sort != null )
            {
                this._sort.MyCollectionChanged -= new NotifyCollectionChangedEventHandler( this.SortDescriptionsChanged );
            }

            this._sort = descriptions;

            if( this._sort != null )
            {
                this._sort.MyCollectionChanged += new NotifyCollectionChangedEventHandler( this.SortDescriptionsChanged );
            }
        }

        private void SortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == NotifyCollectionChangedAction.Remove && e.NewStartingIndex == -1 && SortDescriptions.Count > 0 )
            {
                return;
            }
            if( ((e.Action != NotifyCollectionChangedAction.Reset) || (e.NewItems != null))
                || (((e.NewStartingIndex != -1) || (e.OldItems != null)) || (e.OldStartingIndex != -1)) )
            {
                this.Refresh();
            }
        }

        public System.Collections.IEnumerable SourceCollection
        {
            get
            {
                return this;
            }
        }

        #endregion

    }

    public class SortableCollectionDeferRefresh : IDisposable
    {
        private readonly SortableRowCollection _collectionView;

        internal SortableCollectionDeferRefresh( SortableRowCollection collectionView )
        {
            _collectionView = collectionView;
        }

        public void Dispose()
        {
            // refresh the collection when disposed.
            _collectionView.Refresh();
        }
    }


    public class CustomSortDescriptionCollection : SortDescriptionCollection
    {

        public event NotifyCollectionChangedEventHandler MyCollectionChanged
        {
            add
            {
                this.CollectionChanged += value;
            }
            remove
            {
                this.CollectionChanged -= value;
            }
        }
    }
}
