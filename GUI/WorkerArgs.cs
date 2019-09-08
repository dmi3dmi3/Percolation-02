namespace GUI
{
	public class WorkerArgs
	{
		public WorkerArgs()
		{
			IsDrawOnly = false;
			IsOsm = false;
		}

		public bool IsDrawOnly { get; set; }
		public bool IsOsm { get; set; }

	}
}