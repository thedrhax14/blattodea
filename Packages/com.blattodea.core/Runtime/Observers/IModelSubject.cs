namespace Blattodea.Core.Observers
{
    public interface IModelSubject<TModel>
    {
        TModel Model { get; }

        void Subscribe(IModelObserver<TModel> observer);

        void Unsubscribe(IModelObserver<TModel> observer);
    }
}