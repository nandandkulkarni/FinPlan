using System.Collections.Generic;

namespace FinPlan.Web.Services
{
    public class DebugMessageService
    {
        private readonly List<DebugMessage> _messages = new();
        private readonly object _lock = new();
        public IReadOnlyList<DebugMessage> Messages
        {
            get { lock (_lock) { return _messages.AsReadOnly(); } }
        }
        public void AddMessage(string message)
        {
            lock (_lock)
            {
                _messages.Add(new DebugMessage {Message=message, MessageTime = DateTime.Now });
                if (_messages.Count > 10)
                    _messages.RemoveAt(0);
            }
        }
        public void Clear()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
        }
    }

    public class DebugMessage
    {
        //add message and date
        public string Message { get; set; }

        public DateTime MessageTime { get; set; }
    }
}
