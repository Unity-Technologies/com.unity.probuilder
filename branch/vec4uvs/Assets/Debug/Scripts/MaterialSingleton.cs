using UnityEngine;
using System.Collections;

public class MaterialSingleton : MonoBehaviour
{
	public static MaterialSingleton instance;

	void Awake()
	{
		if(instance == null)
			instance = this;
		else
			GameObject.Destroy(this.gameObject);
	}

	public Material material;
}
