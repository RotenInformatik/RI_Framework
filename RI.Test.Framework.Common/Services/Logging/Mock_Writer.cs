﻿using System;
using System.Collections.Generic;
using System.Text;

using RI.Framework.Collections;
using RI.Framework.Services.Logging;
using RI.Test.Framework.Mocks;

namespace RI.Test.Framework.Services.Logging
{
	public sealed class Mock_Writer : ILogWriter
	{
		public List<MockTuple<DateTime, int, LogLevel, string, string>> Entries { get; private set; }

		public Mock_Writer ()
		{
			this.SyncRoot = new object();
			this.Entries = new List<MockTuple<DateTime, int, LogLevel, string, string>>();
		}

		public bool IsSynchronized => true;

		public object SyncRoot { get; }

		public void Cleanup(DateTime retentionDate)
		{
			this.Entries.RemoveWhere(x => x.Item1 < retentionDate);
		}

		public void Log(DateTime timestamp, int threadId, LogLevel severity, string source, string message)
		{
			this.Entries.Add(new MockTuple<DateTime, int, LogLevel, string, string>(timestamp, threadId, severity, source, message));
		}
	}
}