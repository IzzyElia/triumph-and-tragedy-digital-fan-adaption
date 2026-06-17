using System.Diagnostics;

namespace TT2026.libraries.Izzy.UnitTesting
{
    public struct TestResult
	{
		public bool success { get; private set; }
		public string message { get; private set; }
		public string methodFullName => $"{callingTypeName}.{methodName}";
		public string FullResult
		{
			get
			{
                string str = $"TEST RESULT - {methodFullName}: {success.ToString()}";
                if (message.Length > 0) { str += $" '{message}'"; }
				return str;
            }
        }
        public string callingTypeName { get; private set; }
		public string methodName { get; private set; }
		public TestResult(bool success, string message)
		{
			StackFrame stackFrame = new StackTrace().GetFrame(1);
			methodName = stackFrame.GetMethod().Name;
			callingTypeName = stackFrame.GetMethod().DeclaringType.Name;
			this.success = success;
			this.message = message;
		}
		public TestResult(bool success)
		{
			StackFrame stackFrame = new StackTrace().GetFrame(1);
			methodName = stackFrame.GetMethod().Name;
			callingTypeName = stackFrame.GetMethod().DeclaringType.Name;
			this.success = success;
			this.message = "";
		}
	}
}