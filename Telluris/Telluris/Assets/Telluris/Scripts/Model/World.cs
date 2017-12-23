using System;
using System.Collections.Generic;

public class World
{
    public struct Config
    {
        public int width, height;
    }

    public enum LandType
    {
        Impassable,
        Dirt,
        Water
    }

    public class Cell
    {
        public int x, y;
        public LandType landType = LandType.Dirt;
        public int grassAmount;

        public Cell(int x, int y, LandType landType)
        {
            this.x = x;
            this.y = y;
            this.landType = landType;
            this.grassAmount = 0;
        }
    }

    Cell[] cells;
    private Config _config;
    private Cell border;
    Random random;

	public Config config
    {
        get
        {
            return _config;
        }
    }

    public void Create(Config config)
    {
        this._config = config;
        cells = new Cell[config.width * config.height];

        border = new Cell(-1, -1, LandType.Impassable);
        random = new System.Random();

        SpreadWater();
        SpreadGrass(10);

    }

    private void SpreadGrass(int iterations = 1)
    {
        for (int k = 0; k < iterations; k++)
        {
            for (int y = 0; y < _config.height; y++)
            {
                for (int x = 0; x < _config.width; x++)
                {
                    var thisCell = GetCellAt(x, y);
                    if (thisCell.landType != LandType.Dirt) continue;

                    double chanceOfGrass = 0;
					
                    Cell neighbor;
                    for (var dy = -1; dy <= +1; dy++)
                    {
                        for (var dx = -1; dx <= +1; dx++)
                        {
                            if (dy * dy + dx * dx > 1) continue;
							neighbor = GetCellAt(x+dx, y+dy);
							if (neighbor.grassAmount > 0) chanceOfGrass += 0.03;
							if (neighbor.landType == LandType.Water) chanceOfGrass += 0.05;
						}
                    }


					var rand = random.NextDouble();
                    if (rand < chanceOfGrass) {
                        thisCell.grassAmount = Math.Min(100, thisCell.grassAmount + 2);
                    }					
                }
            }
        }
    }

    internal void Tick(int iterations = 1)
    {
        for (int i = 0; i < iterations; i++)
        {
            SpreadGrass();
        }
    }

    private void SpreadWater()
    {
        for (int y = 0; y < _config.height; y++)
        {
            for (int x = 0; x < _config.width; x++)
            {
				var rand = random.NextDouble();
				var landType = (rand < 0.1f) ? LandType.Water : LandType.Dirt;

				cells[GetIndexFor(x, y)] = new Cell(x, y, landType);
            }
        }
    }

    public Cell GetCellAt(int x, int y)
    {
        if (x < 0 || x >= _config.width || y < 0 || y >= _config.height)
        {
            return border;
        }

        int index = GetIndexFor(x, y);
        return cells[index];
    }

    private int GetIndexFor(int x, int y)
    {
        return x + y * _config.width;
    }
}
