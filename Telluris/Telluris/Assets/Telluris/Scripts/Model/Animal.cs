using UnityEngine;
using System.Collections;
using System;

public class Animal : ITickable
{
    private int _x, _y;
    private World _world;

    public Animal(World world, int x, int y) {
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

    public void Tick()
    {
        var myCell = _world.GetCellAt(X, Y);
        if (myCell.grassAmount > 0) {
            myCell.grassAmount -= 10;
        }
    }
}
