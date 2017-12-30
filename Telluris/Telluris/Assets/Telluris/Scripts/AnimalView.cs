using UnityEngine;
using System.Collections;

public class AnimalView : MonoBehaviour
{
	public Animal animal;

    public float scale = 1f;
    public float offsetX, offsetY;

    public AnimalView() {
        var r = new System.Random();
		offsetX = (float)r.NextDouble();
        offsetY = (float)r.NextDouble();
    }
}
