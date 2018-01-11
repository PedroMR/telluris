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
    private const float maxHungerToReproduce = 1f;
    private const float chanceToReproduce = 0.05f;
    private const float hungerFromReproducing = 7f;
	private const int TICKS_UNTIL_DECAY = 20;
	private int ticksUntilDecay;

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

	public float PercentDecayed
	{
		get{
			return ticksUntilDecay / (float)TICKS_UNTIL_DECAY;
		}
	}

    public void Tick()
    {
		if (_dead)
		{
			ticksUntilDecay--;
			if (ticksUntilDecay <= 0) {
				_world.QueueAnimalForRemoval(this);
			}
			return;
		}


        hunger += hungerPerTick;

        TryToEat();

        if (hunger >= maxHunger)
        {
            Die();
        }

        ConsiderSpawning();
        ConsiderMoving();
    }

    private void ConsiderMoving()
    {
        var rand = new System.Random().NextDouble();
        var chanceToMove = 0.2f;
        World.Cell neighbor;
		for (var dy = -1; dy <= +1; dy++)
		{
			for (var dx = -1; dx <= +1; dx++)
			{
                if (dy * dx != 0 || (dx == 0 && dy == 0))
                    continue;
                
				neighbor = _world.GetCellAt(X + dx, Y + dy);
                if (neighbor.landType == World.LandType.Dirt) {
                    rand -= chanceToMove;
                    if (rand < 0) {
                        MoveBy(dx, dy);
                        return;
                    }
                }
			}
		}
	}

    private void MoveBy(int dx, int dy)
    {
        var neighbor = _world.GetCellAt(X + dx, Y + dy);
        if (neighbor.landType == World.LandType.Dirt)
        {
			var newX = _world.GetFixedX(X + dx);
			var newY = _world.GetFixedY(Y + dy);
            _world.MoveAnimalTo(this, newX, newY);
			this._x = newX;
			this._y = newY;
		}
    }

    private void TryToEat()
    {
        var myCell = _world.GetCellAt(X, Y);
        if (myCell.grassAmount > 0 && hunger > 0)
        {
            var amountEaten = Mathf.Min(myCell.grassAmount, mouthSize);
            hunger -= (amountEaten * energyEfficiency);
            myCell.grassAmount -= amountEaten;
        }

        hunger = Mathf.Clamp(hunger, -stomachSize, maxHunger);
    }

    private void Reproduce()
    {
        _world.CreateOffspringOf(this);
    }

    public void Die()
    {
        _dead = true;
		ticksUntilDecay = TICKS_UNTIL_DECAY;
    }

    public void ConsiderSpawning()
    {
		if (hunger < maxHungerToReproduce)
		{
            var rand = (new System.Random()).NextDouble();
			if (rand < chanceToReproduce)
			{
				Reproduce();
			}
		}
	}
}

