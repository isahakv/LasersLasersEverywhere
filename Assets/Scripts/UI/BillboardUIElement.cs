using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardUIElement : MonoBehaviour
{
	Camera cameraToLook;

	void Start()
	{
		cameraToLook = Camera.main;
	}

    void Update()
    {
		transform.LookAt(Camera.main.transform, cameraToLook.transform.rotation * Vector3.up);
	}
}
