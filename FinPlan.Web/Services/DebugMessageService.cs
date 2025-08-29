using System.Collections.Generic;

namespace FinPlan.Web.Services
{
    public class DebugMessage
    {
        public DateTime MessageTime { get; set; }
        public string MessageText
        { get; set; } = string.Empty;
    }

    public class DebugMessageService
    {
        private readonly List<DebugMessage> _messages = new();
        private readonly object _lock = new();

        public event Action? MessagesChanged;

        public IReadOnlyList<DebugMessage> Messages
        {
            get { lock (_lock) { return _messages.AsReadOnly(); } }
        }

        public void AddMessage(string message)
        {
            lock (_lock)
            {
                _messages.Add(new DebugMessage
                {
                    MessageTime = DateTime.Now,
                    MessageText = message
                });
                if (_messages.Count > 10)
                    _messages.RemoveAt(0);
            }
            MessagesChanged?.Invoke();
        }

        public void Clear()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
            MessagesChanged?.Invoke();
        }
    }
}
