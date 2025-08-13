using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour
{
    // singleton
    static EventBus instance;
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

    Dictionary<string, Action> eventDictionary = new Dictionary<string, Action>();

    void Awake()
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

    void Start()
    {

    }

    void Update()
    {

    }

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
        eventDictionary[eventName] -= listener;

        if (eventDictionary[eventName] == null)
        {
            eventDictionary.Remove(eventName);
        }
    }

    public void Publish(string eventName)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName].Invoke();
        }
    }
}