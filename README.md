BCS Unity - The Basic Components an Systems pugin for Unity
===========================================

This is the Unity plugin which handles generated BCS scripts.
You can find the BCS code generator repository [Here](https://github.com/Clovergruff/BCS)

Usage in Unity
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

After classes are properly generated, component and entity scriptable objects can be created under `Create/Data/Animal/`. Component scriptable objects can then be added to an entity scriptable object to create a unique entity.<br /> <br />Let's create a "Boomer the dog" entity!

<img src="https://i.imgur.com/nGrQGVV.png">
<br /><br />

Each component can have custom members in them, and since each one of them have a generated editor script, creating custom inspectors for them is easy!

<img src="https://i.imgur.com/VUavGxM.png">
<br /><br />

After everything is set up, the `Boomer` entities can now be instantiated as `GameObjects` using the following code:
```csharp
AnimalFactory.Create([Boomer Scriptable Object], position, rotation);
```
