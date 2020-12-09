using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour{
	
	//IMPORTANTE: Esta clase NO puede ser modificada
	
	public delegate void Callback(List<object> parameters);
	
	public static EventManager Instance { get; private set; }
	
	private Dictionary<string, Callback> _callbacks = new Dictionary<string, Callback>();
	
	
	void Awake(){
		if (Instance == null) Instance = this;
		else Destroy(this);
	}
	
	public void AddListener(string eventId, Callback callback){
		if (_callbacks.ContainsKey(eventId))
			_callbacks[eventId] += callback;
		else
			_callbacks.Add(eventId, callback);
	}
	
	public void RemoveListener(string eventId, Callback callback){
		if (_callbacks.ContainsKey(eventId))
			_callbacks[eventId] -= callback;
	}
	
	public void ExecuteCallback(string eventId, List<object> parameters){
		if (_callbacks.ContainsKey(eventId))
			_callbacks[eventId](parameters);
	}
	
	
}