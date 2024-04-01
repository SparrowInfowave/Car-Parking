using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RotateZ : MonoBehaviour
{
	Vector3 angle;
	[SerializeField] float speed = 100;
	void Start()
	{
		angle = transform.eulerAngles;
	}

	void Update()
	{
		angle.z -= Time.deltaTime * speed;
		transform.eulerAngles = angle;
	}
}
