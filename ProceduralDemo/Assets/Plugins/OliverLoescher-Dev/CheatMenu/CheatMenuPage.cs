namespace ODev.CheatMenu
{
	public abstract class CheatMenuPage
	{
		public abstract string Name { get; }
		public abstract CheatMenuGroup Group { get; }

		public virtual int Priority => 0;

		public void Initialize()
		{
			OnInitialize();
		}

		protected virtual void OnInitialize() { }
		public virtual void OnDestroy() { }
		public virtual void OnBecameActive() { }
		public abstract void DrawGUI();
		public virtual void OnPostClose() { }

		public virtual bool IsAvailable() => true;

		protected void Close() => CheatMenu.Close();
	}
}