using System.Threading;

namespace SimpleAdo
{
    /// <summary> A thread. </summary>
    public class Thread
	{
        /// <summary> Sleeps. </summary>
        /// <param name="millisecondsTimeout"> The milliseconds timeout. </param>
		public static void Sleep(int millisecondsTimeout)
		{
			using (var handle = new EventWaitHandle(false, EventResetMode.ManualReset))
			{
				handle.WaitOne(millisecondsTimeout);
			}
		}
	}
}
