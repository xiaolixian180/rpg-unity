using UnityEditor;
using UnityEngine;
using UnityEditor.TestTools.TestRunner.Api;

namespace HeroQuest.Editor
{
    public static class EditModeTestRunner
    {
        private static TestRunnerApi _api;
        private static TestResultCallback _callback;

        [MenuItem("Hero Quest/Run EditMode Tests")]
        public static void Run()
        {
            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _callback = new TestResultCallback();
            _api.RegisterCallbacks(_callback);

            var filter = new Filter
            {
                testMode = TestMode.EditMode
            };

            _api.Execute(new ExecutionSettings(filter));
            Debug.Log("[TestRunner] EditMode tests started.");
        }

        private class TestResultCallback : ICallbacks
        {
            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.TestStatus == TestStatus.Failed)
                {
                    Debug.LogError($"[TestRunner] FAIL: {result.FullName} - {result.Message}");
                }
            }

            public void RunStarted(ITestAdaptor tests) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                Debug.Log($"[TestRunner] === EditMode Tests Complete: {result.PassCount} passed, {result.FailCount} failed, {result.SkipCount} skipped ===");
                if (result.FailCount > 0)
                {
                    Debug.LogError($"[TestRunner] {result.FailCount} test(s) FAILED!");
                }
            }
        }
    }
}
