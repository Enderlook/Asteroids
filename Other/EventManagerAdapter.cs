using System.Collections.Generic;

//MyA1-P1

public sealed class EventManagerAdapter
{
    private readonly EventManager eventManager = new EventManager();

    public void Subscribe(string eventId, EventManager.Callback callback) => eventManager.AddListener(eventId, callback);

    public void Unsubscribe(string eventId, EventManager.Callback callback) => eventManager.RemoveListener(eventId, callback);

    public void Trigger(string eventId, params object[] parameters) => eventManager.ExecuteCallback(eventId, new List<object>(parameters));
}
