using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Animal : ITickable
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
    }

    public void Die()
    {
        _dead = true;
    }
}

