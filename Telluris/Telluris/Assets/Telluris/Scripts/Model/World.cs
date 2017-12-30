using System;
using System.Collections.Generic;

public class World : ITickable
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

    /** Indexed by positionIndex */
    public HashSet<Animal>[] animals;
    public Dictionary<Animal, int> offspringPositions;

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
        var numCells = config.width * config.height;
        cells = new Cell[numCells];
        animals = new HashSet<Animal>[numCells];
        for (var i = 0; i < numCells; i++) animals[i] = new HashSet<Animal>();

        offspringPositions = new Dictionary<Animal, int>();

        border = new Cell(-1, -1, LandType.Impassable);
        random = new System.Random();

        SpreadWater();
        SpreadGrass(10);
		SpawnAnimals();
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

    internal void CreateOffspringOf(Animal animal)
    {
        var child = new Animal(this, animal.X, animal.Y);
        int pos = GetPositionIndexFor(animal.X, animal.Y);

        offspringPositions.Add(child, pos);
    }

    void AddQueuedOffspring()
    {
        foreach (var child in offspringPositions.Keys) {
            animals[offspringPositions[child]].Add(child);
        }

        offspringPositions.Clear();
	}

    internal void Tick(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            Tick();
        }
    }

    public void Tick()
    {
		SpreadGrass();
        TickAnimals();
        AddQueuedOffspring();
	}

    private void TickAnimals()
    {
        foreach(var pos in animals) {
            foreach(var animal in pos) {
                animal.Tick(); 
            }
        }
    }

    private void SpawnAnimals()
    {
        int count = 0;

        while (count < 4) {
			int x = (int)(random.NextDouble() * config.width);
			int y = (int)(random.NextDouble() * config.height);
            if (GetCellAt(x, y).landType == LandType.Dirt)
            {
				int pos = GetPositionIndexFor(x, y);
                var animal = new Animal(this, x, y);
                animals[pos].Add(animal);
                count++;
            }
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

				cells[GetPositionIndexFor(x, y)] = new Cell(x, y, landType);
            }
        }
    }

    public Cell GetCellAt(int x, int y)
    {
        if (x < 0 || x >= _config.width || y < 0 || y >= _config.height)
        {
            return border;
        }

        int index = GetPositionIndexFor(x, y);
        return cells[index];
    }

    private int GetPositionIndexFor(int x, int y)
    {
        return x + y * _config.width;
    }
}
