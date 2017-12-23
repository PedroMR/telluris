using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldView : MonoBehaviour {
    [SerializeField]
    SpriteRenderer landSpritePrefab;

	World world;

    List<SpriteRenderer> landSprites;

    const float TIME_BETWEEN_TICKS = 0.5f;

    float timeSinceLastTick = 0f;

	// Use this for initialization
	void Awake ()
    {
        CreateWorld();
        CreateLandSprites();
    }
    
    private void CreateWorld()
    {
        var config = new World.Config();
        config.width = 8;
        config.height = 8;
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
                var gameObject = (GameObject)GameObject.Instantiate(landSpritePrefab.gameObject);
                var sprite = gameObject.GetComponent<SpriteRenderer>();
                SetSpritePosition(sprite, x, y);
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
						//if (cell.grassAmount > 0)
						{
							sprite.color = Color.Lerp(sprite.color, Color.green, cell.grassAmount / 100f);
						}
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



	private void SetSpritePosition(SpriteRenderer sprite, int x, int y)
    {
        sprite.transform.position = new Vector3(x - world.config.width/2, y - world.config.height / 2, 0);
    }

    // Update is called once per frame
    void Update () {
        timeSinceLastTick += Time.fixedDeltaTime;
        while (timeSinceLastTick > TIME_BETWEEN_TICKS) {
            timeSinceLastTick -= TIME_BETWEEN_TICKS;
			world.Tick(1);
        }
        UpdateLandSprites();
	}
}
