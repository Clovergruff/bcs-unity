BCS Unity - The Basic Components and Systems plugin for Unity
===========================================

This is the Unity plugin which handles generated BCS scripts.
You can find the BCS code generator repository [Here](https://github.com/Clovergruff/BCS)

Generated code
==============
Lets try to create an `Animal` actor with an `Alive` component. There are also some extra options that can be used, such as `-p` or `--prefix`, which allows to set up a custom prefix for the component (The default is `has`). The `-f` or `--force` options allow to overwrite existing classes, otherwise the code generator will skip them. The following code will generate an `Animal` actor with an `Alive` component.
```console
./bcs Animal Alive -p is -f
```
The previously mentioned setup allows to check if an `Animal` object has the `Alive` component attached to it by using the following code. Notice how an `isAlive` boolean has been generated.
```csharp
if (animalInstance.isAlive)
	Debug.Log("Boomer will live!");
```
Other members have also been added to the `Animal` class:
```csharp
public bool isAlive { private set; get; }
public AliveSystem alive { private set; get; }
public AliveConfig aliveConfig { private set; get; }
```
As well as a couple of methods for adding and removing the component (Note that Adding an existing component will remove the previous one!)
```csharp
public AliveSystem AddAlive(AliveConfig config)
{ ... }
public void RemoveAlive()
{ ... }
```

Setting up actor configuration
==============

After classes are properly generated, component and actor scriptable objects can be created under `Create/Data/Animal/` - Component Scriptable Object assets can then be added to an actor scriptable object to create a unique actor.

Alternatively to creating separate assets for each of the components, you can also simply add a nested component to an actor by pressing the `AddComponent` button and choosing the desired component - this will nest the component scriptable object inside the actor scriptable object.

Right clicking an added asset will also reveal options for `Extracting` or `Embedding` components, so if You're migrating from an older version of BCS-Unity, it's possible to select all of the components of an Actor Scriptable Object, and then embed all of them at once.

Let's create a "Boomer the dog" actor!

![image](https://user-images.githubusercontent.com/5364721/236442434-050fe620-4927-4710-a1b9-8b96ad225259.png)

Each component can have custom members in them, and since each one of them have a generated editor script, creating custom inspectors for them is easy!

![image](https://user-images.githubusercontent.com/5364721/236442376-4c30aecf-5b1e-4714-8fea-9cff39e91a3c.png)

Creating an actor instance at runtime
==============

After setting up the actor and its components, the scene must contain an instance of an actor manager.
In this particular case, we have generated an `AnimalActorManager` MonoBehaviour script.

At this point we are ready to create a `GameObject` instance of our actor by using `AnimalFactory.Create(...)`. Here's an example code:
```csharp
public class AnimalSpawner : MonoBehaviour
{
	public AnimalConfig animal;
	private void Start() =>
		AnimalFactory.Create(animal, transform.position, transform.rotation);
}
```
![image](https://user-images.githubusercontent.com/5364721/236435208-b9e3b110-1c16-45d8-9511-bda4c7551bf7.png)

After running this particular scene, a `Boomer` gameobject will be created with an `Animal` component representing an actor instance, as well as `Alive` and `Superpowers` systems. These systems can contain any kind of logic your project requires.

![image](https://user-images.githubusercontent.com/5364721/236441180-603aa9ea-ceeb-4dd0-bf2d-24febc4df911.png)

If we look at the `Animal` actor component in Debug view, we can see how the actor contains enabled `isAlive` and `hasSuperpowers` booleans, as well as references to component config files and their systems.

![image](https://user-images.githubusercontent.com/5364721/236442065-3a4defbc-2334-4a8f-8b3a-4495685291af.png)

These members can now be easily accessed and used to build logic for our project!
