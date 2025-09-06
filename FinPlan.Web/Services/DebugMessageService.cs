using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<DebugMessageService> _logger;

        public event Action? MessagesChanged;

        public DebugMessageService(ILogger<DebugMessageService> logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<DebugMessage> Messages
        {
            get { lock (_lock) { return _messages.AsReadOnly(); } }
        }

        public void AddMessage(string message)
        {
            lock (_lock)
            {
                var dm = new DebugMessage
                {
                    MessageTime = DateTime.Now,
                    MessageText = message
                };
                _messages.Add(dm);
                if (_messages.Count > 100)
                    _messages.RemoveAt(0);

                try { _logger.LogInformation("DebugMessage added: {MessageTime} {MessageText}", dm.MessageTime, dm.MessageText); } catch { }
            }
            MessagesChanged?.Invoke();
        }

        public void Clear()
        {
            lock (_lock)
            {
                _messages.Clear();
            }
            try { _logger.LogInformation("DebugMessageService cleared messages"); } catch { }
            MessagesChanged?.Invoke();
        }
    }
}
