﻿#region

using UnityEngine;

#endregion

public class AutoRotation : MonoBehaviour, IEntityFX
{
	public bool enableInParentSpace;
	public bool enableInSelfSpace;
	//public Vector3 rotationOffsetInWorldSpace;
	public Vector3 omega;
	//public bool enableInWorldSpace;
	public Vector3 rotationOffsetInParentSpace;
	public Vector3 rotationOffsetInSelfSpace;
	public Vector3 translationOffsetInParentSpace;
	public Vector3 translationOffsetInSelfSpace;
	//public Vector3 translationOffsetInWorldSpace;

	public void Disable() { enabled = false; }

	private void Update()
	{
		if (enableInSelfSpace)
		{
			var pivot = transform.TransformPoint(translationOffsetInSelfSpace);
			var rotationOffest = Quaternion.Euler(rotationOffsetInSelfSpace);
			transform.RotateAround(pivot, transform.TransformDirection(rotationOffest * Vector3.left), omega.x * Time.deltaTime);
			transform.RotateAround(pivot, transform.TransformDirection(rotationOffest * Vector3.up), omega.y * Time.deltaTime);
			transform.RotateAround(pivot, transform.TransformDirection(rotationOffest * Vector3.forward), omega.z * Time.deltaTime);
		}
		if (enableInParentSpace)
		{
			var pivot = transform.parent.TransformPoint(translationOffsetInParentSpace);
			var rotationOffest = Quaternion.Euler(rotationOffsetInParentSpace);
			transform.RotateAround(pivot, transform.parent.TransformDirection(rotationOffest * Vector3.left), omega.x * Time.deltaTime);
			transform.RotateAround(pivot, transform.parent.TransformDirection(rotationOffest * Vector3.up), omega.y * Time.deltaTime);
			transform.RotateAround(pivot, transform.parent.TransformDirection(rotationOffest * Vector3.forward), omega.z * Time.deltaTime);
		}
		/*if (enableInWorldSpace)
		{
			var pivot = translationOffsetInWorldSpace;
			var rotationOffest = Quaternion.Euler(rotationOffsetInWorldSpace);
			transform.RotateAround(pivot, rotationOffest * Vector3.left, omega.x * Time.deltaTime);
			transform.RotateAround(pivot, rotationOffest * Vector3.up, omega.y * Time.deltaTime);
			transform.RotateAround(pivot, rotationOffest * Vector3.forward, omega.z * Time.deltaTime);
		}*/
	}
}