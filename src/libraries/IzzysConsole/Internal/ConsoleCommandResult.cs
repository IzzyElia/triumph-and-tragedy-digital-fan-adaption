namespace IzzysConsole.Internal
{
    public struct ConsoleCommandResult
    {
        public bool success { get; private set; }
        public object returnedObject { get; private set; }
        public string failureInfo { get; private set; }
        public ConsoleCommandResult(bool success, object returnedObject = null, string failureInfo = null)
        {
            this.success = success;
            this.returnedObject = returnedObject;
            this.failureInfo = failureInfo;
        }
    }
}