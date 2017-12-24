using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Animal : ITickable
{
    private int _x, _y;
    private World _world;

    private int hunger = 0;
    private bool _dead;

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

        hunger++;

        if (myCell.grassAmount > 0 && hunger > 0)
        {
            hunger -= Mathf.Min(myCell.grassAmount, 10);
            myCell.grassAmount -= 10;
        }

        hunger = Mathf.Clamp(hunger, 0, 100);

        if (hunger >= 20)
        {
            Die();
        }
    }

    public void Die()
    {
        _dead = true;
    }
}

