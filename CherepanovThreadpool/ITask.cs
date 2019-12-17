

namespace CherepanovThreadpool
{
    public interface ITask
    {
        ITask PrevTask { get; }
        bool IsInThreadpool { get; set; }
        bool ThreadpoolIsDisposed { get; set; }
        void Exec();
    }
}
