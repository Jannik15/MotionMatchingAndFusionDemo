using UnityEngine;

public class EventListener : ScriptableObject
{
	public delegate void Listener();
	public event Listener listenerEvent;
}
