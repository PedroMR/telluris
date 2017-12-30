using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Animal : ITickable, ISpawner
{
    private int _x, _y;
    private World _world;

    private float hunger = 0f;
    private bool _dead;

	private const int mouthSize = 10;
	private const int maxHunger = 20;
	private const float energyEfficiency = 1;
    private const int stomachSize = 20;
    private const float hungerPerTick = 0.5f;
    private const float maxHungerToReproduce = 5f;
    private const float chanceToReproduce = 0.1f;
    private const float hungerFromReproducing = 7f;

    public Animal(World world, int x, int y)
    {
        _world = world;
        _x = x;
        _y = y;
    }

    public int X
    {
        get
        {
            return _x;
        }
    }

    public int Y
    {
        get
        {
            return _y;
        }
    }

    public bool Dead
    {
        get
        {
            return _dead;
        }
    }

    public void Tick()
    {
        if (_dead) return;

        var myCell = _world.GetCellAt(X, Y);

        hunger += hungerPerTick;

        if (myCell.grassAmount > 0 && hunger > 0)
        {
            var amountEaten = Mathf.Min(myCell.grassAmount, mouthSize);
            hunger -= (amountEaten * energyEfficiency);
            myCell.grassAmount -= amountEaten;
        }

        hunger = Mathf.Clamp(hunger, -stomachSize, maxHunger);

        if (hunger >= maxHunger)
        {
            Die();
        }

        ConsiderSpawning();
    }

    private void Reproduce()
    {
        _world.CreateOffspringOf(this);
    }

    public void Die()
    {
        _dead = true;
    }

    public void ConsiderSpawning()
    {
		if (hunger < maxHungerToReproduce)
		{
			if (new System.Random().NextDouble() < chanceToReproduce)
			{
				Reproduce();
			}
		}
	}
}

