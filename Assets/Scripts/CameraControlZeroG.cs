﻿/*
 * Copyright 2014 Jason Graves (GodLikeMouse/Collaboradev)
 * http://www.collaboradev.com
 *
 * This file is part of Unity - Topology.
 *
 * Unity - Topology is free software: you can redistribute it 
 * and/or modify it under the terms of the GNU General Public 
 * License as published by the Free Software Foundation, either 
 * version 3 of the License, or (at your option) any later version.
 *
 * Unity - Topology is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with Unity - Topology. If not, see http://www.gnu.org/licenses/.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraControlZeroG : MonoBehaviour {

	public float speed = 12f;
    public Text movementSpeed;

   

    void Update () {
        Vector3 move = new Vector3();
        Vector3 rotate = new Vector3();

		if (Input.GetKey (KeyCode.LeftShift)) {
            move.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            move.y = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        }
        else
        {
            rotate.y = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            rotate.x = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        }



        //adjust speed with mouse wheel
        move.z += Input.GetAxis("Mouse ScrollWheel") * 15;
		if (speed < 5)
			speed = 5;

		movementSpeed.text = "Flyspeed: " + speed;

		move = transform.TransformDirection(move);
		transform.position += move;
        transform.Rotate(rotate);
	}
}
