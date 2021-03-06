using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rs317.Sharp
{
	public sealed class MouseDetection : IRunnable
	{
		private IMouseInputQueryable MouseQueryable { get; }

		private ITaskDelayFactory TaskDelayFactory { get; }

		private readonly object syncObj = new object();

		public object SyncObj => syncObj;

		public int[] coordsY;
		public bool running;
		public int[] coordsX;
		public int coordsIndex;

		public MouseDetection(IMouseInputQueryable mouseQueryable, ITaskDelayFactory taskDelayFactory)
		{
			MouseQueryable = mouseQueryable ?? throw new ArgumentNullException(nameof(mouseQueryable));
			TaskDelayFactory = taskDelayFactory;
			coordsY = new int[500];
			running = true;
			coordsX = new int[500];
		}

		public async Task run()
		{
			while(running)
			{
				lock(syncObj)
				{
					if(coordsIndex < 500)
					{
						coordsX[coordsIndex] = MouseQueryable.mouseX;
						coordsY[coordsIndex] = MouseQueryable.mouseY;
						coordsIndex++;
					}
				}

				await TaskDelayFactory.Create(50);
			}
		}
	}
}
