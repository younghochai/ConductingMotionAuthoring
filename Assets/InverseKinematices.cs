using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InverseKinematices : MonoBehaviour
{
	public Transform upperArm;
	public Transform forearm;
	public Transform hand;
	public Transform elbow;
	public Transform target;
	[Space(20)]
	public Vector3 uppperArm_OffsetRotation;
	public Vector3 forearm_OffsetRotation;
	public Vector3 hand_OffsetRotation;
	[Space(20)]
	public bool handMatchesTargetRotation = true;
	[Space(20)]
	public bool debug;

	float angle;
	float upperArm_Length;
	float forearm_Length;
	float arm_Length;
	float targetDistance;
	float adyacent;
	bool isPlay = false;
	int currentCnt = 0;


	List<List<float>> pattern_2_4;

	void fileRead(string file_path)
	{
		pattern_2_4 = new List<List<float>>();

		FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');

		float[] data_val = new float[4];

		for (int line = 1; line < records.Length - 1; line++)
		{
			fields = records[line].Split(',');

			List<float> list_temp = new List<float>();
			for (int val_idx = 0; val_idx < 2; val_idx++)
			{
				list_temp.Add(float.Parse(fields[val_idx]));

			}
			pattern_2_4.Add(list_temp);

		}
		Debug.Log(records.Length - 1 + "  Pattern Data reading done" + pattern_2_4.Count);
		sr.Close();
		quatStream.Close();
	}


	// Use this for initialization
	void Start()
	{
		fileRead("Assets/4_4_norm.csv");
		Debug.Log("4_4_norm.Count() " + pattern_2_4.Count);

	}

	//IEnumerator avatar_play_custom() //awinda Xsens data player
	//{
	//	for (int frame_cnt = 0; frame_cnt < load_quat_list[0][0].Count; frame_cnt++)
	//	{

	//		for (int i = 0; i < _bodyCustomJointNames.Length; i++)
	//		{
	//			smpl_module.SetWorld2LocalJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][frame_cnt]);
	//		}
	//		smpl_module.UpdateJointPositions(false);
	//		yield return new WaitForSeconds(.01667f);
	//		Debug.Log("frame_cnt : " + frame_cnt);
	//	}
	//	yield break;
	//}


	// Update is called once per frame
	void Update()
    {

		if (Input.GetKeyDown(KeyCode.Keypad0))
		{

			if (isPlay)
            {
				isPlay = false;
            }
            else
            {
				
				currentCnt = 0;
				isPlay = true;
            } 
				
		}


		if (upperArm != null && forearm != null && hand != null && elbow != null && target != null)
		{
			if(isPlay)
            {
				Debug.Log("pattern_2_4.Count() " + pattern_2_4.Count);
				
				if (currentCnt >= pattern_2_4.Count-1)
					currentCnt = 0;

				target.position = new Vector3( 5 + pattern_2_4[currentCnt][0]*-30, 30 - pattern_2_4[currentCnt][1]*50, -40);


				currentCnt++;
			}

			//if (target.position[1] > 10)
			//	target.position[1] = 0;

			//target.position = new Vector3(0, 0.1f, 0);


			upperArm.LookAt(target, elbow.position - upperArm.position);
			upperArm.Rotate(uppperArm_OffsetRotation);

			Vector3 cross = Vector3.Cross(elbow.position - upperArm.position, forearm.position - upperArm.position);

			upperArm_Length = Vector3.Distance(upperArm.position, forearm.position);
			
			
			forearm_Length = Vector3.Distance(forearm.position, hand.position);
			arm_Length = upperArm_Length + forearm_Length;
			targetDistance = Vector3.Distance(upperArm.position, target.position);
			targetDistance = Mathf.Min(targetDistance, arm_Length - arm_Length * 0.001f);

			adyacent = ((upperArm_Length * upperArm_Length) - (forearm_Length * forearm_Length) + (targetDistance * targetDistance)) / (2 * targetDistance);

			angle = Mathf.Acos(adyacent / upperArm_Length) * Mathf.Rad2Deg;

			upperArm.RotateAround(upperArm.position, cross, -angle);

			forearm.LookAt(target, cross);
			forearm.Rotate(forearm_OffsetRotation);

			//if(handMatchesTargetRotation){
			//	hand.rotation = target.rotation;
			//	hand.Rotate (hand_OffsetRotation);
			//}

			if (debug)
			{
				if (forearm != null && elbow != null)
				{
					Debug.DrawLine(forearm.position, elbow.position, Color.blue);
				}

				if (upperArm != null && target != null)
				{
					Debug.DrawLine(upperArm.position, target.position, Color.red);
				}
			}

		}
	}

	void OnDrawGizmos()
	{
		if (debug)
		{
			if (upperArm != null && elbow != null && hand != null && target != null && elbow != null)
			{
				Gizmos.color = Color.gray;
				Gizmos.DrawLine(upperArm.position, forearm.position);
				Gizmos.DrawLine(forearm.position, hand.position);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(upperArm.position, target.position);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(forearm.position, elbow.position);
			}
		}
	}

}
