using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
	public static InputController Instance { get; private set; }
	public Transform cameraTarget;
	public float cameraRotationSpeed = 1f, minCameraVertAngle = 45f, maxCameraVertAngle = 75f;
	public float cameraZoomSpeed, minCameraZoom, maxCameraZoom;
	
	Camera mainCamera;
	Vector2 mousePos;
	bool isInputEnabled = false;
	bool haveTouch = false;

	private void Awake()
	{
		if (Instance == null)
		{
			DontDestroyOnLoad(this);
			Instance = this;
			mainCamera = Camera.main;
			mainCamera.transform.forward = cameraTarget.position - mainCamera.transform.position;
		}
		else if (Instance != this)
			DestroyImmediate(this);
	}

	private void Update()
    {
		if (!isInputEnabled)
			return;

		TouchHandler();
		CameraMovement();
		CameraZoom();
	}

	public void EnableInput()
	{
		StartCoroutine(EnableInputCoroutine());
	}

	private IEnumerator EnableInputCoroutine()
	{
		yield return null;
		isInputEnabled = true;
	}

	public void DisableInput()
	{
		isInputEnabled = false;
	}

	private void RaycastForInteractible()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		int layerMask = 1 << LayerMask.NameToLayer("Interactible");
		layerMask |= 1 << LayerMask.NameToLayer("Obstacle");
		if (Physics.Raycast(ray, out hit, 100f, layerMask))
		{
			if (hit.collider.transform.parent.GetComponent<IInteractible>() is IInteractible interactible && interactible != null)
				GameManager.Instance.OnObjectInteracted(interactible);
			else if (hit.collider.transform.parent.GetComponent<IPlaceable>() is IPlaceable placeable && placeable != null)
				GameManager.Instance.OnObjectInteracted(placeable);
		}
	}

	private void TouchHandler()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		if (Input.GetMouseButtonDown(0))
		{
			haveTouch = true;
			mousePos = Input.mousePosition;
		}
		else if (haveTouch && Input.GetMouseButton(0) && Vector2.Distance(mousePos, Input.mousePosition) > 0.1f)
			haveTouch = false;
		else if (haveTouch && Input.GetMouseButtonUp(0))
			RaycastForInteractible();
#elif UNITY_ANDROID || UNITY_IOS
		if (Input.touchCount == 1)
		{
			if (Input.GetTouch(0).phase == TouchPhase.Began)
				haveTouch = true;
			else if (haveTouch && Input.GetTouch(0).phase == TouchPhase.Moved)
				haveTouch = false;
			else if (haveTouch && Input.GetTouch(0).phase == TouchPhase.Ended)
				RaycastForInteractible();
		}
#endif
	}

	private void CameraMovement()
	{
		Vector3 Xaxis = mainCamera.transform.right, Yaxis = -Vector3.up;

#if UNITY_EDITOR || UNITY_STANDALONE
		cameraRotationSpeed = 50f;

		float inputHorizontal = Input.GetAxis("Horizontal");
		float inputVertical = Input.GetAxis("Vertical");
		if (inputVertical != 0f) // Vertical.
		{
			float camVerticalRot = mainCamera.transform.rotation.eulerAngles.x;
			// Checking if in min-max range.
			if ((inputVertical > 0f && camVerticalRot < maxCameraVertAngle) || (inputVertical < 0f && camVerticalRot > minCameraVertAngle))
				mainCamera.transform.RotateAround(cameraTarget.position, Xaxis, inputVertical * cameraRotationSpeed * Time.deltaTime);
		}
		if (inputHorizontal != 0f) // Horizontal.
			mainCamera.transform.RotateAround(cameraTarget.position, Yaxis, inputHorizontal * cameraRotationSpeed * Time.deltaTime);
#elif UNITY_ANDROID || UNITY_IOS
		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			Vector2 input = -1 * Input.GetTouch(0).deltaPosition;
			if (input.y != 0f) // Vertical.
			{
				float camVerticalRot = mainCamera.transform.rotation.eulerAngles.x;
				// Checking if in min-max range.
				if ((input.y > 0f && camVerticalRot < maxCameraVertAngle) || (input.y < 0f && camVerticalRot > minCameraVertAngle))
					mainCamera.transform.RotateAround(cameraTarget.position, Xaxis, input.y * cameraRotationSpeed * Time.deltaTime);
			}
			if (input.x != 0f) // Horizontal.
				mainCamera.transform.RotateAround(cameraTarget.position, Yaxis, input.x * cameraRotationSpeed * Time.deltaTime);
		}
#endif
	}

	private void CameraZoom()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll == 0f)
			return;

		float fov = mainCamera.fieldOfView + scroll * cameraZoomSpeed;
		fov = Mathf.Clamp(fov, minCameraZoom, maxCameraZoom);
		mainCamera.fieldOfView = fov;
#elif UNITY_ANDROID || UNITY_IOS
		if (Input.touchCount != 2)
			return;
		Touch touchZero = Input.GetTouch(0), touchOne = Input.GetTouch(1);
		if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
		{
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float currMagnitude = (touchZero.position - touchOne.position).magnitude;

			float difference = currMagnitude - prevMagnitude;
			if (difference != 0)
			{
				float fov = mainCamera.fieldOfView - difference * cameraZoomSpeed;
				fov = Mathf.Clamp(fov, minCameraZoom, maxCameraZoom);
				mainCamera.fieldOfView = fov;
			}
		}
#endif
	}
}
