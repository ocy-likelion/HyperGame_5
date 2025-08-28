using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    // private 필드
    private Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    // singleton
    private static EventBus instance;
    public static EventBus Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("EventBus");
                go.AddComponent<EventBus>();
                DontDestroyOnLoad(go);
            }

            return instance;
        }
    }

    // 유니티 콜백
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // 메인
    public void Subscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] += listener;
        }
        else
        {
            eventDictionary[eventName] = listener;
        }
    }
    public void Unsubscribe(string eventName, Action listener)
    {
        if (eventDictionary.ContainsKey(eventName)) 
        {
            eventDictionary[eventName] -= listener;
            
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }
    public void Publish(string eventName)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName].Invoke();
        }
    }

    // events using generic
    private Dictionary<string, Delegate> eventDictionaryT = new Dictionary<string, Delegate>();
    public void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (eventDictionaryT.ContainsKey(eventName))
        {
            eventDictionaryT[eventName] = (Action<T>)eventDictionaryT[eventName] + listener;
        }
        else
        {
            eventDictionaryT[eventName] = listener;
        }
    }
    public void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        if (eventDictionaryT.ContainsKey(eventName))
        {
            eventDictionaryT[eventName] = (Action<T>)eventDictionaryT[eventName] - listener;
        }
    }
    public void Publish<T>(string eventName, T param)
    {
        if (eventDictionaryT.ContainsKey(eventName))
        {
            var action = eventDictionaryT[eventName] as Action<T>;
            action?.Invoke(param);
        }
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        if (!Application.isPlaying) // 씬 닫을 때
        {
            instance = null;
        }
    }
#endif
}