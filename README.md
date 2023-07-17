BCS Unity - The Basic Components and Systems plugin for Unity
===========================================

This is the Unity plugin which handles generated BCS scripts.
You can find the BCS code generator repository [Here](https://github.com/Clovergruff/BCS)

Generated code
==============
Lets try to create an `Animal` entity with an `Alive` component. There are also some extra options that can be used, such as `-p` or `--prefix`, which allows to set up a custom prefix for the component (The default is `has`). The `-f` or `--force` options allow to overwrite existing classes, otherwise the code generator will skip them. The following code will generate an `Animal` entity with an `Alive` component.
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

Setting up entity configuration
==============

After classes are properly generated, component and entity scriptable objects can be created under `Create/Data/Animal/`. Component scriptable objects can then be added to an entity scriptable object to create a unique entity.

Let's create a "Boomer the dog" entity!

![image](https://user-images.githubusercontent.com/5364721/236442434-050fe620-4927-4710-a1b9-8b96ad225259.png)

Each component can have custom members in them, and since each one of them have a generated editor script, creating custom inspectors for them is easy!

![image](https://user-images.githubusercontent.com/5364721/236442376-4c30aecf-5b1e-4714-8fea-9cff39e91a3c.png)

Creating an entity instance at runtime
==============

After setting up the entity and its components, the scene must contain an instance of an entity manager.
In this particular case, we have generated an `AnimalEntityManager` MonoBehaviour script.

At this point we are ready to create a `GameObject` instance of our entity by using `AnimalFactory.Create(...)`. Here's an example code:
```csharp
public class AnimalSpawner : MonoBehaviour
{
	public AnimalConfig animal;
	private void Start() =>
		AnimalFactory.Create(animal, transform.position, transform.rotation);
}
```
![image](https://user-images.githubusercontent.com/5364721/236435208-b9e3b110-1c16-45d8-9511-bda4c7551bf7.png)

After running this particular scene, a `Boomer` gameobject will be created with an `Animal` component representing an entity instance, as well as `Alive` and `Superpowers` systems. These systems can contain any kind of logic your project requires.

![image](https://user-images.githubusercontent.com/5364721/236441180-603aa9ea-ceeb-4dd0-bf2d-24febc4df911.png)

If we look at the `Animal` entity component in Debug view, we can see how the entity contains enabled `isAlive` and `hasSuperpowers` booleans, as well as references to component config files and their systems.

![image](https://user-images.githubusercontent.com/5364721/236442065-3a4defbc-2334-4a8f-8b3a-4495685291af.png)

These members can now be easily accessed and used to build logic for our project!
