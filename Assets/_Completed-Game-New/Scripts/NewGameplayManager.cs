using UnityEngine;
using Dsu.Framework;

public class NewGameplayManager : DsuGameplayManagerBase
{
    void Start()
    {
        // How to use the NewGameplayManager in other component
        // step1. declare GameplayEventHandler variable in the component
        //   private event GameplayEventHandler gameplayHandler = null;
        // step2. register the event in the Start method
        //   NewGameplayManager gameplayManager = FindObjectOfType<NewGameplayManager>();
        //   gameplayManager?.RegisterEvent(ref gameplayHandler);
    }

    void Update()
    {
        // Per-frame update logic here
    }

    public override void OnGameplayEvent(object sender, GameplayEventArgsBase args)
    {
        GameObject gameObject = sender as GameObject;
        //if (args.Event == GameplayEvents.CustomGameplayEventFromHere)
        //{
        //    OnCustomGameplayEvent(gameObject, args);
        //}
    }

    //private void OnCustomGameplayEvent(GameObject sender, GameplayEventArgsBase args)
    //{
    //    CollisionEventArgs collArgs = args as CollisionEventArgs;
    //    Debug.Log($"Custom gameplay event received from: {sender.name}");
    //}
    public void OnPickupEvent(int eventID, Transform sender, Transform other)
    {
        //Debug.Log($"Pickup event received from: {sender.name}, picked up: {other.name}");
        int counter = other.gameObject.Counter();
        other.gameObject.SetActive(false);
        NewRuntimeGameDataManager.instance.Count += counter;
    }

}