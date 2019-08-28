namespace Services.History
{
    public interface ICommand
    {
        void Execute();
        void UnExecute();
    }
}
