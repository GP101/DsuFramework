# DsuFramework

## Overview

When creating a game, it is very important to make each component of the game maintainable and independent. For example, if the **Player** script handles player-related mechanics while also managing **Animation** and **UI**, this is considered a bad approach. The reason is that whenever the animation or UI changes, the Player script must also be modified.

A common solution to this problem is to insert an additional **indirection layer** between the two layers. For instance, to decouple the **Model**, which manages data, and the **View**, which handles rendering, you can introduce a **Controller**. This allows the Model and View to be maintained independently. This is known as the **MVC pattern**.

When gameplay involves collisions, dependencies between objects can arise depending on how collision handling is implemented. For example, if the Player collides with an Item, you might process the collision in the Player’s `OnTriggerEnter()` method. In that case, `OnTriggerEnter()` must be implemented as part of the Player. However, the function that handles the interaction between the Player and the Item conceptually belongs to neither the Player nor the Item—it is a function that uses both objects. Implementing it inside either class is not ideal. Moreover, if the same event needs to be processed in multiple places, it further increases the Player’s coupling.
A way to solve this issue is to create **Game Events** as **ScriptableObjects**, adding an additional layer of indirection.

Unity's **GameObject** class is implemented as `sealed`, which means you cannot add new methods to it through inheritance. Therefore, when you need to process something based on the type of game object, the common approach is to rely on **Tags**. If developers were able to arbitrarily add custom properties to GameObjects, it would be more convenient. For example, if you could add a property called `count` to a GameObject and retrieve it using `GetCount()`, level editing would become much easier.
This can be achieved using **extension methods**. By designing them carefully, functions like `GetCount()` can operate in constant time and can be applied to any GameObject.


## Features

**DsuFramework provides the following features:**

1. **UI is separated from other systems**, allowing the UI code to be maintained independently. Specific handlers are invoked only when UI data changes.
2. **Gameplay-related events are kept independent**, enabling gameplay events to be processed from multiple places without creating unnecessary object coupling.
3. **Custom properties can be added to GameObjects.** For game objects that require detailed control, the `GameObjectProperty` component can be added to enable fine-grained customization.

You can download the source code for the *Rolling Ball* game from the link below.
Let’s take a look at how applying **DsuFramework** to this game can improve its structure and maintainability.

[Roll-a-Ball Project](https://assetstore.unity.com/packages/essentials/tutorial-projects/unity-learn-3d-beginner-roll-a-ball-complete-project-urp-77198)

The project includes both the original **Roll-a-Ball** implementation and the updated **Roll-a-Ball-New**, which applies **DsuFramework**. You can compare the two versions to see how the previous implementation has been improved.

![](./Documentation~/DsuFramework_Roll-a-Ball-OldAndNew.png)


## 1. UI MVC Pattern

The image below shows how the original **Roll-a-Ball** project handles its UI.

![](./Documentation~/DsuFramework_UIGameObjectWasAccessedNotInUI-Bad.png)

Under the **Canvas**, there are child objects named **Count Text** and **Win Text**, and these objects are directly referenced by the **Player**.
This is a very bad practice. Code related to the UI should only be accessed from game objects that are responsible for UI-related functionality.

In the original Roll-a-Ball project, you can see that `PlayerController.cs` declares fields that directly access Text components, as shown below.

``` csharp
public class PlayerController : MonoBehaviour {
	
	// Create public variables for player speed, and for the Text UI game objects
	public float speed;
	public Text countText;
	public Text winText;
```

In the code above, both **countText** and **winText** must be removed.
By doing so, the **PlayerController** will only handle gameplay logic related to the Player, which reduces coupling.

---

The following code shows how the **UIController**, added as a component on the **Canvas**, references **Count Text** and **Win Text**.

![](./Documentation~/DsuFramework_UIControllerForUIGameObjects-Good.png)

To separate the PlayerController from the UI, we need an intermediate layer that manages the data updated by the Player. This layer is the **RuntimeGameDataManager**.
When the Player collides with an item, it does **not** update the UI directly. Instead, it accesses and updates the data managed by **RuntimeGameDataManager**.
Meanwhile, the **UIController** checks the RuntimeGameDataManager to see whether any UI-related data has changed, and if so, it updates the UI.

You can generate code templates for **RuntimeGameDataManager** and **UIController** using the menu shown below.

![](./Documentation~/DsuFramework_GenerateRuntimeGameDataManagerScript-ContextMenu.png)

Next, create an empty GameObject named **GameDataManager** and attach the **RuntimeGameDataManager** component to it.
Then, attach the **UIController** to the **Canvas**.

![](./Documentation~/DsuFramework_RuntimeGameDataManager-DataModelLayer.png)

Add code to **RuntimeGameDataManager** so that it maintains the *count* value.


``` csharp
public class NewRuntimeGameDataManager : RuntimeGameDataManagerBase
{
    static public NewRuntimeGameDataManager instance = null;

    private int count;

    public int Count
    {
        get { return count; }
        set { _UpdateDataStamp(); count = value; }
    }
```

To ensure that the UI is updated **only when the count value changes**, the setter must call `_UpdateDataStamp()` whenever the value is modified.

In **PlayerController**, when the player collides with an item, it now updates the data by accessing the **RuntimeGameDataManager**, instead of directly modifying the UI.


``` csharp
	void OnTriggerEnter(Collider other) 
	{
		// ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
		if (other.gameObject.CompareTag ("Pick Up"))
		{
            NewRuntimeGameDataManager.instance.Count += 1;
		}
	}
```

Then, **UpdateData()** in the **UIController** will be called.
Inside this method, you implement the logic that updates the UI.

``` csharp
public class NewUIController : UIControllerBase
{
    public Text countText;
    public Text winText;

    protected override void UpdateData(int groupId)
    {
        SetCountText();
    }
```

If multiple **UIController** instances exist in the same scene, all of them will have their **UpdateData()** method invoked. To distinguish between them, you can use a **groupId**. For example, when calling the method as shown below, the value `1` will be passed to **groupId**.


``` csharp
_UpdateDataStamp(1)
```

You can use **UIReferenceFinder** to check whether all UI elements in the game are correctly configured.

![](./Documentation~/DsuFramework_Tools-UIReferenceFinder-Menu.png)

When you run the tool and click **“Find Bad UI Reference,”** it scans the current Hierarchy and reports any incorrect UI references.
Any issues found should be fixed by updating the code to use **UIController** instead.

![](./Documentation~/DsuFramework_UIReferenceFinder-FindBadUIReference.png)

The image above shows UIReferenceFinder detecting invalid UI references in the original Roll-a-Ball project.


## 2. GameEvent Pattern

Centralizing gameplay-related event handling in a single place makes the code much easier to maintain.
For example, when the **Player** collides with an **Item**, the collision logic should not be placed inside either the `PlayerController` or the `ItemController`. Instead, it should be handled in a separate **GameplayManager**.

Furthermore, if you want to process a single gameplay event in multiple ways — such as separating general gameplay logic from UI-related logic — this structure should be visually represented and manageable within Unity’s Hierarchy.


### 2.1. Event Raising

For example, suppose you want the **PlayerController** to handle an event related to item pickup.
In that case, you first add a variable of type **DsuGameEvent** to `PlayerController.cs` as shown below.

``` csharp
public class PlayerControllerNew : MonoBehaviour {
	
	public float speed;
	private Rigidbody rb;
	public DsuGameEvent pickupEvent;
```

Next, create a **DsuGameEvent** object (a ScriptableObject) and name it **PickupEvent**.

![](./Documentation~/DsuFramework_CreateGameEvent-ContextMenu.png)

Then assign this object to the **pickupEvent** reference inside the **PlayerController**.

![](./Documentation~/DsuFramework_PlayerController-PickupEvent.png)

Inside the `PlayerController`, you need to call `pickupEvent.Raise()` to trigger the event.

``` csharp

	void OnTriggerEnter(Collider other) 
	{
		// ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
		if (other.gameObject.CompareTag ("Pick Up"))
		{
			pickupEvent?.Raise(0, this.transform, other.transform);
		}
	}
```

A **DsuGameEvent** always has three parameters.
The first parameter is an arbitrary value provided by the user to distinguish the type of game event.
The second and third parameters are both of type **Transform**. Their roles can be defined according to the requirements of each gameplay event. For example, in a trigger event, the first Transform could represent the object whose `OnTriggerEnter` was invoked, and the second Transform could represent the other object involved in the collision.

Depending on the gameplay event, the Transform parameters may not be needed at all.
In such cases, you can configure the parameters so that only the first parameter (**iParam**) is used when calling `Raise()`.


### 2.2. Event Processing

Now we want to handle the **PickupEvent** in the **GameplayManager**.
To do this, add a handler method to the GameplayManager as shown below.


``` csharp
    public void OnPickupEvent(int eventID, Transform sender, Transform other)
    {
        //Debug.Log($"Pickup event received from: {sender.name}, picked up: {other.name}");
        int counter = other.gameObject.Counter();
        other.gameObject.SetActive(false);
        NewRuntimeGameDataManager.instance.Count += counter;
    }
```

Next, we need to map the **PickupEvent** so that it is handled by `GameplayManager.OnPickupEvent`.
In the Hierarchy, select **GameDataManager** and add a **DsuGameEventListener** component.
Then select **PickupEvent** and assign it to the **Event** slot of the DsuGameEventListener.

![](./Documentation~/DsuFramework_AssignPickupEventToGameEventListener.png)

After that, click the **+** button to configure the UnityEvent that will handle the event.
Set **GameDataManager** as the target object, and then assign **GameplayManager.OnPickupEvent()** as the callback method.

![](./Documentation~/DsuFramework_GameEventListener-Set-OnPickupEvent.png)

The image above shows how `NewGameplayManager.OnPickupEvent` is assigned as the event handler.

If the **PickupEvent** needs to be handled in additional locations, simply add another **DsuGameEventListener** component to the relevant object and map its event handler accordingly.
This structure allows one event to be handled by multiple handlers in appropriate places, modularizing and decoupling the program’s architecture.

---

You may also want to handle events directly inside the **GameplayManager** without using a **DsuGameEventListener** component.
In that case, you need to declare a **DsuGameEventReference** variable as shown below.
When using **DsuGameEventReference**, you can add event actions directly in the script.
In such cases, configuring the Response in the Inspector may not be necessary.

``` csharp
public class NewGameplayManager : DsuGameplayManagerBase
{
    public DsuGameEventReference pickupEventRef;

    private void OnEnable()
    {
        pickupEventRef.RegisterAction(OnPickupEvent);
    }

    private void OnDisable()
    {
        pickupEventRef.UnregisterAction(OnPickupEvent);
    }
```

As shown in the image below, **GameDataManager** does not have a **DsuGameEventListener** component attached.
Also, the **Response** for **Pickup Event Ref** has no assigned value.

![](./Documentation~/DsuFramework_DsuGameEventReference.png)

If a single GameObject needs to handle dozens of gameplay events, adding a separate **DsuGameEventListener** for each event can make the setup overly complex.
In such cases, using a **one-dimensional array of DsuGameEventReference** keeps the code much cleaner and more manageable.

---

If you prefer to process events internally without exposing them in the Hierarchy, you should use the **GameplayEvent** class defined in `DsuGameplayEvents.cs`.

``` csharp
namespace Dsu.Framework
{
    public class GameplayEvent
    {
        public int Id { get; private set; }

        public GameplayEvent(object id_)
        {
            Id = (int)id_;
        }

        public override string ToString() => Id.ToString();
        public static readonly GameplayEvent NullGameplayEvent = new GameplayEvent(0);
    }//class GameplayEvent
```

When using **GameplayEvent**, the event must be handled within `GameplayManager`’s `OnGameplayEvent()` method.

``` csharp
    public override void OnGameplayEvent(object sender, GameplayEventArgsBase args)
    {
        GameObject gameObject = sender as GameObject;
        //if (args.Event == GameplayEvents.CustomGameplayEventFromHere)
        //{
        //    OnCustomGameplayEvent(gameObject, args);
        //}
    }
```

## 3. GameObject Property

You can add **custom properties** to any GameObject.
To add a custom property, first generate a **GameObjectPropertyScript** by selecting:

**DsuFramework → Generate GameObject Property Script**

![](./Documentation~/DsuFramework_GenerateGameObjectPropertyScriptContextMenu.png)

You may create multiple GameObjectPropertyScripts, but it is recommended to have **one per project**.
After the script is generated, you can freely add any properties you need.

The example below shows how to add a property called **counter**.
Once this property is added, you can call `Counter()` on **any GameObject**.


``` csharp
namespace Dsu.Framework
{
    public partial class GameObjectPropertyData
    {
        public override void Reset()
        {
            counter = 0;
        }
        public int counter;
    }

    public static partial class DsuGameObjectExtensions
    {
        public static int Counter(this GameObject go)
        {
            _AddKey(go);
            return _objectDictionary[go.transform].propertyData.counter;
        }
    }
}
```

Calling `Counter()` is performed in constant time using a **Dictionary** data structure.
Because of this, you can call `Counter()` on **any GameObject**.

If you want to adjust the `counter` value for a specific GameObject, you must add the **GameObjectProperty** component to that object and set the counter value appropriately.
The image below shows an example where the counter value of a specific GameObject is set to **2**.

![](./Documentation~/DsuFramework_GameObjectProperty-PropertyData-Counter.png)

Now, when the **GameplayManager** handles the `PickupEvent`, it can retrieve the target object's `Counter()` value.
The code below shows how `OnPickupEvent()` can make use of the object's counter value.

``` csharp
    public void OnPickupEvent(int eventID, Transform sender, Transform other)
    {
        //Debug.Log($"Pickup event received from: {sender.name}, picked up: {other.name}");
        int counter = other.gameObject.Counter();
        other.gameObject.SetActive(false);
        NewRuntimeGameDataManager.instance.Count += counter;
    }
```

When `Counter()` is called on a specific GameObject, if that object does not already have a **GameObjectProperty** component, the component will be added automatically.
The values of the user-defined properties are then initialized using the default values provided by C#’s type system.
