/*
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
using Assets.Scripts;
using System;
using System.Linq;

public class CameraControlZeroG : MonoBehaviour {

	public float speed = 12f;
    public Text movementSpeed;
    public GraphController graphController;
    private float lastMove = 0;
    public AnimationCurve[] curves = new AnimationCurve[6];
    public Text CoordControl = null;

    private void Start()
    {
        graphController = FindObjectOfType(typeof(GraphController)) as GraphController;
        graphController.MoveCamera += GraphController_MoveCamera;
    }

    private void GraphController_MoveCamera(object sender, Vector3 position, Vector3 rotation, float duration)
    {
        curves[0] = AnimationCurve.EaseInOut(Time.time, transform.position.x, Time.time + duration, position.x);
        curves[1] = AnimationCurve.EaseInOut(Time.time, transform.position.y, Time.time + duration, position.y);
        curves[2] = AnimationCurve.EaseInOut(Time.time, transform.position.z, Time.time + duration, position.z);
        curves[3] = AnimationCurve.EaseInOut(Time.time, transform.rotation.eulerAngles.x, Time.time + duration, rotation.x);
        curves[4] = AnimationCurve.EaseInOut(Time.time, transform.rotation.eulerAngles.y, Time.time + duration, rotation.y);
        curves[5] = AnimationCurve.EaseInOut(Time.time, transform.rotation.eulerAngles.z, Time.time + duration, rotation.z);
     }

    void Update () {
        //animation
        if (curves[0] != null)
        {
            transform.position = new Vector3(curves[0].Evaluate(Time.time), curves[1].Evaluate(Time.time), curves[2].Evaluate(Time.time));
            transform.rotation = Quaternion.Euler(new Vector3(curves[3].Evaluate(Time.time), curves[4].Evaluate(Time.time), curves[5].Evaluate(Time.time)));
            CoordControl.text = "Pos: " + transform.position.ToString() + "\nRot: " + transform.rotation.eulerAngles.ToString();
        }
        //input
        Vector3 move = new Vector3();
        Vector3 rotate = new Vector3();

		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            rotate.y = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            move.y = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        }
        else
        {
            move.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            move.z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        }

        //adjust speed with mouse wheel
        move.z += Input.GetAxis("Mouse ScrollWheel") * 15;
		if (speed < 5)
			speed = 5;

		movementSpeed.text = "Flyspeed: " + speed;
        move = transform.TransformDirection(move);
        var oldRot = transform.rotation.eulerAngles;
        var newRot = (transform.rotation * Quaternion.Euler(rotate)).eulerAngles;
        if (oldRot.Equals(newRot) && move.Equals(Vector3.zero))
            return;



        graphController.DoAction(new MoveCamera()
        {
            newPos = SVector3.FromVector3(transform.position + move),
            oldPos = SVector3.FromVector3(transform.position),
            newRot = SVector3.FromVector3(newRot),
            oldRot = SVector3.FromVector3(oldRot),
            duration = 0
        });



    }
}
