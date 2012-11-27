using System;
using System.Net.Sockets;
using JustEat.StatsD.Collections;

namespace JustEat.StatsD.Net
{
	/// <summary>
	/// A SocketAsyncEventArgs derived class that is aware that it needs to be returned to an object pool when OnCompleted has been called.
	/// </summary>
	public sealed class PoolAwareSocketAsyncEventArgs : SocketAsyncEventArgs
	{
		private readonly SimpleObjectPool<SocketAsyncEventArgs> _parentPool;

		/// <summary>	Initializes a new instance of the PooledSocketAsyncEventArgs class. </summary>
		/// <param name="parentPool">	The pool that owns this instance. </param>
		public PoolAwareSocketAsyncEventArgs(SimpleObjectPool<SocketAsyncEventArgs> parentPool)
		{
			if (null == parentPool) { throw new ArgumentNullException("parentPool"); }

			_parentPool = parentPool;
		}

		/// <summary>	Represents a method that is called when an asynchronous operation completes. </summary>
		/// <remarks>	Adds the arguments back to the pool for future use. </remarks>
		/// <param name="e">	The event that is signaled. </param>
		protected override void OnCompleted(SocketAsyncEventArgs e)
		{
			base.OnCompleted(e);
			_parentPool.Push(this);
		}
	}
}