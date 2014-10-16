using UnityEngine;
using System.Collections;

/**
 * Attach this script to the top level gameObject of a prefab and it will
 * automatically rebuild the mesh when instantiated.  Useful in cases
 * where it is not possible to use ProBuilder.Instantiate().
 */
public class RebuildMeshOnWake : MonoBehaviour
{
	void Awake()
	{
		ReconstructMeshRecursive(transform);
	}
	
	private void ReconstructMeshRecursive(Transform t)
	{
		if(t.GetComponent<pb_Object>())
			t.GetComponent<pb_Object>().Verify();

		foreach(Transform child in t)
			ReconstructMeshRecursive(child);
	}
}
