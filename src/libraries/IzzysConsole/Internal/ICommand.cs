using System.Reflection;

namespace IzzysConsole.Internal
{
    public interface ICommand
    {
        public string commandName { get; }
        public MemberInfo implimentingMember { get; }
        public string format { get; }
        public string helpInfo { get; }
        public bool isScopedCommand { get; }
        public object Execute(string[] userParameters, object context);
        public object GetSubscope(string[] userParameters, object context);
    }
}