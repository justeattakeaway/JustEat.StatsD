using System;
using System.Linq;
using System.Text;

namespace JustEat.Aop
{
	public class StatsDPipe : IDisposable
	{
		private readonly Random _random = new Random();
		private readonly IUdpClient _udpClient;

		public StatsDPipe(string host, int port)
		{
			_udpClient = new TestableUdpClient(host, port);
		}

		public void Dispose()
		{
			_udpClient.Dispose();
		}

		public bool Timing(string key, int value)
		{
			return Timing(key, value, 1.0);
		}

		public bool Timing(string key, int value, double sampleRate)
		{
			return Send(sampleRate, String.Format("{0}:{1:d}|ms", key, value));
		}

		public bool Decrement(string key)
		{
			return Increment(key, -1, 1.0);
		}

		public bool Decrement(string key, int magnitude)
		{
			return Decrement(key, magnitude, 1.0);
		}

		public bool Decrement(string key, int magnitude, double sampleRate)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(key, magnitude, sampleRate);
		}

		public bool Decrement(params string[] keys)
		{
			return Increment(-1, 1.0, keys);
		}

		public bool Decrement(int magnitude, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, 1.0, keys);
		}

		public bool Decrement(int magnitude, double sampleRate, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, sampleRate, keys);
		}

		public bool Increment(string key)
		{
			return Increment(key, 1, 1.0);
		}

		public bool Increment(string key, int magnitude)
		{
			return Increment(key, magnitude, 1.0);
		}

		public bool Increment(string key, int magnitude, double sampleRate)
		{
			var stat = String.Format("{0}:{1}|c", key, magnitude);
			return Send(stat, sampleRate);
		}

		public bool Increment(int magnitude, double sampleRate, params string[] keys)
		{
			return Send(sampleRate, keys.Select(key => String.Format("{0}:{1}|c", key, magnitude)).ToArray());
		}

		protected bool Send(String stat, double sampleRate)
		{
			return Send(sampleRate, stat);
		}

		private bool Send(double sampleRate, params string[] stats)
		{
			var retval = false; // didn't send anything
			if (sampleRate < 1.0)
			{
				foreach (var stat in stats)
				{
					if (_random.NextDouble() <= sampleRate)
					{
						var statFormatted = String.Format("{0}|@{1:f}", stat, sampleRate);
						if (DoSend(statFormatted))
						{
							retval = true;
						}
					}
				}
			}
			else
			{
				foreach (var stat in stats)
				{
					if (DoSend(stat))
					{
						retval = true;
					}
				}
			}

			return retval;
		}

		private bool DoSend(string stat)
		{
			var data = Encoding.Default.GetBytes(stat + "\n");

			_udpClient.Send(data, data.Length);
			return true;
		}
	}
}
