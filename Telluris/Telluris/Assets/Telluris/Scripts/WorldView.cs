using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldView : MonoBehaviour {
	[SerializeField]
	GameObject landSpritePrefab;
    [SerializeField]
    Sprite[] animalSpritePrefabs;
    [SerializeField]
    GameObject animalPrefab;

	World world;

    List<SpriteRenderer> landSprites;
    Dictionary<Animal, GameObject> animalSprites;

    const float TIME_BETWEEN_TICKS = 0.5f;

    const int Z_LAND = 10;
    const int Z_ANIMALS = 1;


    float timeSinceLastTick = 0f;

	// Use this for initialization
	void Awake ()
    {
        CreateWorld();
        CreateLandSprites();

        animalSprites = new Dictionary<Animal, GameObject>();
    }
    
    private void CreateWorld()
    {
        var config = new World.Config();
        config.width = 18;
        config.height = 18;
        world = new World();
        world.Create(config);
    }

	private void CreateLandSprites()
	{
        landSprites = new List<SpriteRenderer>();

        for (int y = 0; y < world.config.height; y++)
        {
            for (int x = 0; x < world.config.width; x++)
            {
                var spriteGO = (GameObject)UnityEngine.Object.Instantiate(landSpritePrefab.gameObject);
				spriteGO.transform.parent = this.transform;
                var sprite = spriteGO.GetComponent<SpriteRenderer>();
                SetSpritePosition(sprite, x, y, Z_LAND);
                landSprites.Add(sprite);
            }
        }
        UpdateLandSprites();
	}

    private void UpdateLandSprites()
    {
        int index = 0;
        for (int y = 0; y < world.config.height; y++)
        {
            for (int x = 0; x < world.config.width; x++)
            {
				var cell = world.GetCellAt(x, y);
                var sprite = landSprites[index++].GetComponent<SpriteRenderer>();

				switch (cell.landType)
				{
					case World.LandType.Dirt:
						sprite.color = new Color(0.3f, 0.2f, 0.0f);
						sprite.color = Color.Lerp(sprite.color, Color.green, cell.grassAmount / 100f);
						break;
					case World.LandType.Impassable:
						sprite.color = new Color(0.2f, 0.2f, 0.2f);
						break;
					case World.LandType.Water:
						sprite.color = new Color(0.2f, 0.2f, 0.6f);
						break;
				}
			}
        }
    }



	private void SetSpritePosition(SpriteRenderer sprite, int x, int y, int zLayer)
    {
        sprite.transform.position = new Vector3(x - world.config.width/2, y - world.config.height / 2, zLayer);
    }

    // Update is called once per frame
    void Update () {
        bool ticked = false;
        timeSinceLastTick += Time.fixedDeltaTime;
        while (timeSinceLastTick > TIME_BETWEEN_TICKS) {
            timeSinceLastTick -= TIME_BETWEEN_TICKS;
			world.Tick(1);
            ticked = true;
        }

        if (ticked)
        {
            UpdateLandSprites();
            UpdateAnimalSprites();
        }
	}

    private void UpdateAnimalSprites()
    {
        foreach(var list in world.animals) {
            foreach(var animal in list) {
                UpdateAnimalSprite(animal);
            }
        }
    }

    private void UpdateAnimalSprite(Animal animal)
    {
        if (!animalSprites.ContainsKey(animal)) {
            var newSpriteGO = (GameObject)UnityEngine.Object.Instantiate(animalPrefab);
            animalSprites.Add(animal, newSpriteGO);
            var newSprite = newSpriteGO.GetComponent<SpriteRenderer>();
            // newSprite.sprite = ...
            var animalView = newSpriteGO.AddComponent<AnimalView>();
            animalView.animal = animal;
		}
        var animalGO = animalSprites[animal];
		var sprite = animalGO.GetComponent<SpriteRenderer>();
        SetSpritePosition(sprite, animal.X, animal.Y, Z_ANIMALS);

        if (animal.Dead)
            sprite.color = Color.red;
	}
}
