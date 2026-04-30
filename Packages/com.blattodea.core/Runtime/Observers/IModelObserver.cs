namespace Blattodea.Core.Observers
{
    public interface IModelObserver<in TModel>
    {
        void OnModelChanged(TModel model);
    }
}