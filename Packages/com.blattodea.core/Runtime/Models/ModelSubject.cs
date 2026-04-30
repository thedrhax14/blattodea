using System;
using System.Collections.Generic;
using Blattodea.Core.Observers;

namespace Blattodea.Core.Models
{
    public sealed class ModelSubject<TModel> : IModelSubject<TModel>
    {
        private readonly List<IModelObserver<TModel>> _observers;
        private readonly IEqualityComparer<TModel> _comparer;

        public ModelSubject(TModel initialModel, IEqualityComparer<TModel> comparer = null)
        {
            Model = initialModel;
            _observers = new List<IModelObserver<TModel>>();
            _comparer = comparer ?? EqualityComparer<TModel>.Default;
        }

        public TModel Model { get; private set; }

        public void Subscribe(IModelObserver<TModel> observer)
        {
            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (_observers.Contains(observer))
            {
                throw new InvalidOperationException("Observer is already subscribed to this model subject.");
            }

            _observers.Add(observer);
            observer.OnModelChanged(Model);
        }

        public void Unsubscribe(IModelObserver<TModel> observer)
        {
            if (observer is null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (!_observers.Remove(observer))
            {
                throw new InvalidOperationException("Observer is not subscribed to this model subject.");
            }
        }

        public bool TrySetModel(TModel nextModel)
        {
            if (_comparer.Equals(Model, nextModel))
            {
                return false;
            }

            Model = nextModel;
            NotifyObservers();
            return true;
        }

        public void SetModel(TModel nextModel)
        {
            TrySetModel(nextModel);
        }

        private void NotifyObservers()
        {
            for (var observerIndex = 0; observerIndex < _observers.Count; observerIndex++)
            {
                _observers[observerIndex].OnModelChanged(Model);
            }
        }
    }
}