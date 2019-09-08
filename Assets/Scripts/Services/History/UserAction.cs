namespace Services.History {
	public interface UserAction {
		void Execute();
		void UnExecute();
	}
}