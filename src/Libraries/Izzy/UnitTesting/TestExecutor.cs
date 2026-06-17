using System;
using System.Collections.Generic;
using System.Reflection;

namespace TT2026.libraries.Izzy.UnitTesting
{
    public static class TestExecutor
	{
        /// <summary>
        /// Runs unit tests on every ITestable in the current assembly and returns the results
        /// </summary>
        /// <returns></returns>
        public static TestResult[] RunTests()
        {
            List<TestResult> allResults = new List<TestResult>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<TypeInfo> types = assembly.DefinedTypes;
                foreach (TypeInfo type in types)
                {
                    MethodInfo[] methods = type.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        if (method.GetCustomAttributes(typeof(TestAttribute), false).Length > 0)
                        {
                            object classInstance = Activator.CreateInstance(type.AsType());
                            TestResult result = (TestResult)method.Invoke(classInstance, null);
                            allResults.Add(result);
                        }
                    }
                }
            }
            return allResults.ToArray();
        }

        public static void LogTestResults(TestResult[] testResults)
		{
			foreach (TestResult testResult in testResults)
			{
				switch (testResult.success)
				{
					case true: DynamicLogger.Log(testResult.FullResult); break;
					case false: DynamicLogger.LogWarning(testResult.FullResult); break;
				}
			}
		}
		public static void RunTestsAndLog() => LogTestResults(RunTests());
	}
}
