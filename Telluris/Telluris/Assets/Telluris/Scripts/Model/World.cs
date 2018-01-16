using System;
using System.Collections.Generic;

namespace com.pedromr.telluris.model
{
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

		public struct WorldInfo {
			public long ticks;
			public long births, deaths;
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

		private const double CHANCE_GRASS_FROM_WATER = 0.05;
		private const double CHANCE_GRASS_FROM_GRASS = 0.03;
		private const float MAX_GRASS = 100;
		private const int INITIAL_GRASS_ITERATIONS = 100;

		/** Indexed by positionIndex */
		public HashSet<Animal>[] animalPositions;
		public Dictionary<Animal, int> offspringPositions;
		public HashSet<Animal> allAnimals;
		public HashSet<Animal> decayedAnimals = new HashSet<Animal>();

		Cell[] cells;
		private Config _config;
		private Cell border;
		public Random random;

		private WorldInfo _info = new WorldInfo();

		public long Ticks
		{
			get { return _info.ticks; }
		}

		public Config config
		{
			get
			{
				return _config;
			}
		}

		public WorldInfo Info
		{
			get
			{
				return _info;
			}
		}

		public void Create(Config config)
		{
			this._config = config;
			var numCells = config.width * config.height;
			cells = new Cell[numCells];
			animalPositions = new HashSet<Animal>[numCells];
			for (var i = 0; i < numCells; i++) animalPositions[i] = new HashSet<Animal>();
			allAnimals = new HashSet<Animal>();

			offspringPositions = new Dictionary<Animal, int>();

			border = new Cell(-1, -1, LandType.Impassable);
			random = new System.Random();

			SpreadWater();
			SpreadGrass(INITIAL_GRASS_ITERATIONS);
			SpawnAnimals();

            _info.ticks = 0;
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
								if (dy * dy + dx * dx > 1) continue; // avoid diagonals

								neighbor = GetCellAt(x + dx, y + dy);
								if (neighbor.grassAmount > 0)
								{
									int divider = (dx == dy) ? 1 : 4; // is it us?
									chanceOfGrass += (neighbor.grassAmount / MAX_GRASS) * CHANCE_GRASS_FROM_GRASS / divider;
								}
								if (neighbor.landType == LandType.Water) chanceOfGrass += CHANCE_GRASS_FROM_WATER;
							}
						}


						var rand = random.NextDouble();
						if (rand < chanceOfGrass)
						{
							thisCell.grassAmount = Math.Min((int)MAX_GRASS, thisCell.grassAmount + 2);
						}
					}
				}
			}
		}

		internal void MoveAnimalTo(Animal animal, int x, int y)
		{
			int sourcePos = GetPositionIndexFor(animal.X, animal.Y);
			int targetPos = GetPositionIndexFor(x, y);

			animalPositions[sourcePos].Remove(animal);
			animalPositions[targetPos].Add(animal);
		}

		internal void CreateOffspringOf(Animal animal)
		{
			var child = new Animal(this, animal.X, animal.Y);
			int pos = GetPositionIndexFor(animal.X, animal.Y);

			offspringPositions.Add(child, pos);
		}

		void AddQueuedOffspring()
		{
			foreach (var child in offspringPositions.Keys)
			{
				animalPositions[offspringPositions[child]].Add(child);
				allAnimals.Add(child);
				_info.births++;
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
            _info.ticks++;
			SpreadGrass();
			TickAnimals();
			AddQueuedOffspring();
			RemoveDecayedAnimals();
		}

		private void RemoveDecayedAnimals()
		{
			foreach (var animal in decayedAnimals)
			{
				RemoveAnimal(animal);
			}
			decayedAnimals.Clear();
		}

		private void TickAnimals()
		{
			foreach (var animal in allAnimals)
			{
				animal.Tick();
			}
		}

		private void SpawnAnimals()
		{
			int count = 0;

			while (count < 4)
			{
				int x = (int)(random.NextDouble() * config.width);
				int y = (int)(random.NextDouble() * config.height);
				if (GetCellAt(x, y).landType == LandType.Dirt)
				{
					CreateAnimalAt(x, y);
					count++;
				}
			}
		}

		private Animal CreateAnimalAt(int x, int y)
		{
			var animal = new Animal(this, x, y);
			AddAnimal(animal, x, y);
			_info.births++;
			return animal;
		}

		private void AddAnimal(Animal animal, int x, int y)
		{
			int pos = GetPositionIndexFor(x, y);
			animalPositions[pos].Add(animal);
			allAnimals.Add(animal);
		}

		private void RemoveAnimal(Animal animal)
		{
			int pos = GetPositionIndexFor(animal.X, animal.Y);
			animalPositions[pos].Remove(animal);
			allAnimals.Remove(animal);
		}

		public void QueueAnimalForRemoval(Animal animal)
		{
			decayedAnimals.Add(animal);
			_info.deaths++;
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

		public int GetFixedX(int x) { return Util.mod(x, _config.width); }
		public int GetFixedY(int y) { return Util.mod(y, _config.height); }

		public Cell GetCellAt(int x, int y)
		{
			x = GetFixedX(x);
			y = GetFixedY(y);

			if (x < 0 || x >= _config.width || y < 0 || y >= _config.height)
			{
				return border;
			}

			int index = GetPositionIndexFor(x, y);
			return cells[index];
		}

		private int GetPositionIndexFor(int x, int y)
		{
			x = GetFixedX(x);
			y = GetFixedY(y);
			return x + y * _config.width;
		}
	}
}