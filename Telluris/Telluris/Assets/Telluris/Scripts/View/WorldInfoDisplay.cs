using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WorldInfoDisplay : MonoBehaviour
{
    public Text textField;
    public WorldView worldView;

	public enum InfoFields
	{
		Ticks,
		Births,
		Deaths,
		Alive
	};

	public InfoFields field;

	// Update is called once per frame
	void Update()
	{
		var info = worldView.World.Info;

		switch(field) {
			case InfoFields.Ticks:
				textField.text = string.Format("cycle {0,0:000000}", info.ticks);
				break;
			case InfoFields.Births:
				textField.text = string.Format("births {0,0:000000}", info.births);
				break;
			case InfoFields.Deaths:
				textField.text = string.Format("deaths {0,0:000000}", info.deaths);
				break;
			case InfoFields.Alive:
				textField.text = string.Format("alive {0,0:000000}", info.births - info.deaths);
				break;
			default:
				textField.text = "No field specified";
				break;
		}
	}
}
