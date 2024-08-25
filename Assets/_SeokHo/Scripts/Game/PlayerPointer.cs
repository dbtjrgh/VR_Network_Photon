using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointer : MonoBehaviour {
	private Camera cam;
	public LayerMask mask;
	private void Awake() {
		cam = Camera.main;
	}

	private void Update() {
		var ray = cam.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out var hit, 1000, mask)) {
			transform.position = hit.point;
		}


	}
}
