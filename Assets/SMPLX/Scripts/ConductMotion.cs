using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class ConductMotion : MonoBehaviour
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
	public TextMeshProUGUI statusText;
	public Transform left_shoulder;
	public Transform left_elbow;

	float angle;
	float upperArm_Length;
	float forearm_Length;
	float arm_Length;
	float targetDistance;
	float adyacent;
	bool isPlay = false;
	int currentCnt = 0;

	float ticCount = 0;
	float ticTime = 0;
	float nextTime = 0;
	float nextConductTime = 0;
	float musicBPM = 60f;
	float stdBPM = 60f;
	float musictemp = 4f;
	float stdTemp = 4f;

	int frameCount = 0;

	bool isCueStart = false;
	bool isCueConducting = false;
	int cueStartFrame = 0;

	bool isDecreStart = false;
	int decreStartFrame = 0;

	bool isCresStart = false;
	int cresStartFrame = 0;

	bool isCut_offStart = false;
	int cut_offStartFrame = 0;


	List<List<float>> pattern_4_4;
	List<List<int>> timingGT;
	List<int> timing;
	string beatStr = "";
	string statusStr = "";

	int startFrame = 0;
	int endFrame = 0;
	List<int> timingData;
	bool isChanged = false;
	bool isStart = false;
	bool isCountingStart = false;


	float restTime = 0.01667f;


	int[] _StopFrame = new int[] {505,881,1263 };
	int[] _CueFrame = new int[] {175, 570,940,1331};
	int[] _CutoffFrame = new int[] { 1670 };


	int stopFrameIdx_max = 2;
	int cueFrameIdx_max = 3;
	int cutoffFrameIdx_max = 0;



	int stopFrameIdx = 0;
	int cueFrameIdx = 0;
	int cutoffFrameIdx = 0;




	string[] _Conduct_annotation = new string[] { "1", "2", "3", "4", "Cue", "Cut_off", "fermata_start", "fermata_end", "decrescendo_start", "decrescendo_end",
												"crescendo_start", "crescendo_end", "piano_start", "piano_end", "mezzo_forte_start", "mezzo_forte_end" };



	public List<List<Quaternion>> cue_data = new List<List<Quaternion>>();
	public List<List<Quaternion>> cut_off_data = new List<List<Quaternion>>();
	public List<List<Quaternion>> decrescendo_data = new List<List<Quaternion>>();
	public List<List<Quaternion>> crescendo_data = new List<List<Quaternion>>();


	void left_Hand_Data_Load_cres(string file_path)
	{
		crescendo_data = new List<List<Quaternion>>();
		FileStream quatStream = new FileStream(file_path, FileMode.Open);
		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');
		float[] data_val = new float[4];

		for (int line = 0; line < records.Length - 1; line++)
		{
			fields = records[line].Split(',');
			int fields_cnt = 0;
			List<Quaternion> temp = new List<Quaternion>();

			for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
			{
				for (int val_idx = 0; val_idx < 4; val_idx++)
				{
					data_val[val_idx] = float.Parse(fields[fields_cnt]);
					fields_cnt++;
				}
				temp.Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));
			}
			crescendo_data.Add(temp);
		}
		Debug.Log("left_Hand_Data_Load reading done");
		sr.Close();
		quatStream.Close();
	}

	void left_Hand_Data_Load_decre(string file_path)
	{
		decrescendo_data = new List<List<Quaternion>>();
		FileStream quatStream = new FileStream(file_path, FileMode.Open);
		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');
		float[] data_val = new float[4];

		for (int line = 0; line < records.Length - 1; line++)
		{
			fields = records[line].Split(',');
			int fields_cnt = 0;
			List<Quaternion> temp = new List<Quaternion>();

			for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
			{
				for (int val_idx = 0; val_idx < 4; val_idx++)
				{
					data_val[val_idx] = float.Parse(fields[fields_cnt]);
					fields_cnt++;
				}
				temp.Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));
			}
			decrescendo_data.Add(temp);
		}
		Debug.Log("left_Hand_Data_Load reading done");
		sr.Close();
		quatStream.Close();
	}


	void left_Hand_Data_Load_cue(string file_path)
	{
		cue_data = new List<List<Quaternion>>();
		FileStream quatStream = new FileStream(file_path, FileMode.Open);
		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');
		float[] data_val = new float[4];

		for (int line = 0; line < records.Length -1; line++)
		{
			fields = records[line].Split(',');
            int fields_cnt = 0;
			List<Quaternion> temp = new List<Quaternion>();

			for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
            {
                for (int val_idx = 0; val_idx < 4; val_idx++)
                {
                    data_val[val_idx] = float.Parse(fields[fields_cnt]);
                    fields_cnt++;
                }
				temp.Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));
            }
			cue_data.Add(temp);
        }
		Debug.Log("left_Hand_Data_Load reading done");
		sr.Close();
		quatStream.Close();
	}

	//Vector3(0.228114784,0.00335399806,-0.00475551328);
	//UnityEditor.TransformWorldPlacementJSON:{"position":{"x":-15.655487060546875,"y":15.33992862701416,"z":-26.874256134033204},"rotation":{"x":0.5793691277503967,"y":0.49729669094085696,"z":0.09225372970104218,"w":0.6391531229019165},"scale":{"x":1.0,"y":1.0,"z":1.0}}

	void left_Hand_Data_Load_cut(string file_path)
	{
		cut_off_data = new List<List<Quaternion>>();
		FileStream quatStream = new FileStream(file_path, FileMode.Open);
		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');
		float[] data_val = new float[4];

		for (int line = 0; line < records.Length - 1; line++)
		{
			fields = records[line].Split(',');
			int fields_cnt = 0;
			List<Quaternion> temp = new List<Quaternion>();

			for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
			{
				for (int val_idx = 0; val_idx < 4; val_idx++)
				{
					data_val[val_idx] = float.Parse(fields[fields_cnt]);
					fields_cnt++;
				}
				temp.Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));
			}
			cut_off_data.Add(temp);
		}
		Debug.Log("left_Hand_Data_Load reading done");
		sr.Close();
		quatStream.Close();
	}


	void timingGtLoad(string file_path)
    {
		timing = new List<int>();

		FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');

		for (int line = 0; line < records.Length - 1; line++)
		{
			fields = records[line].Split(',');

			timing.Add(int.Parse(fields[0]));

		}
		Debug.Log(records.Length - 1 + "  timingGtLoad done" + timing.Count);
		sr.Close();
		quatStream.Close();

	}

	void GroundtruthLoad(string file_path)
	{
		timingGT = new List<List<int>>();

		FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');

		int[] data_val = new int[4];

		for (int line = 0; line < records.Length - 1; line++)
		{
			fields = records[line].Split(',');


			List<int> list_temp = new List<int>();
			for (int val_idx = 0; val_idx < 16; val_idx++)
			{
				list_temp.Add(int.Parse(fields[val_idx]));

			}
			timingGT.Add(list_temp);

		}
		Debug.Log(records.Length - 1 + "  GroundtruthLoadg done" + timingGT.Count);
		sr.Close();
		quatStream.Close();
	}

	void ConductingTrajectoriesLoad(string file_path)
	{
		pattern_4_4 = new List<List<float>>();

		FileStream quatStream = new FileStream(file_path, FileMode.OpenOrCreate);

		StreamReader sr = new StreamReader(quatStream);
		string[] fields;
		string[] records = sr.ReadToEnd().Split('\n');

		float[] data_val = new float[4];

		for (int line = 0; line < records.Length-1; line++)
		{
			fields = records[line].Split(',');

			List<float> list_temp = new List<float>();
			for (int val_idx = 0; val_idx < 2; val_idx++)
			{
				list_temp.Add(float.Parse(fields[val_idx]));

			}
			pattern_4_4.Add(list_temp);

		}
		Debug.Log(records.Length - 1 + "  Pattern Data reading done" + pattern_4_4.Count);
		sr.Close();
		quatStream.Close();
	}


	// Start is called before the first frame update
	void Start()
    {

		left_Hand_Data_Load_cue("Assets/CSVs/save_cue.csv");
		Debug.Log("cue_data.Count() " + cue_data.Count);

		left_Hand_Data_Load_cut("Assets/CSVs/save_cut_off.csv");
		Debug.Log("cut_off_data.Count() " + cut_off_data.Count);

		left_Hand_Data_Load_cres("Assets/CSVs/save_cresendo.csv");
		Debug.Log("crescendo_data.Count() " + crescendo_data.Count);

		left_Hand_Data_Load_decre("Assets/CSVs/save_decresendo.csv");
		Debug.Log("decrescendo_data.Count() " + decrescendo_data.Count);


		timingData = new List<int>();
		timingGtLoad("Assets/CSVs/beat_timing.csv");

		statusText.text = "Current Frame ";
		GroundtruthLoad("Assets/CSVs/sensingdata_gt_test.csv");
		Debug.Log("timingGT.Count() " + timingGT.Count + "    "+ _Conduct_annotation.Length);


		ConductingTrajectoriesLoad("Assets/CSVs/4Beat.csv");
		Debug.Log("Load File : " + pattern_4_4.Count);
	}

	void _Animation()
	{
		if (isPlay)
		{
			StartCoroutine(conductMotionPlay());

			currentCnt++;

			if (currentCnt >= pattern_4_4.Count - 1)
				currentCnt = 0;
		}
	}

	IEnumerator conductMotionPlay() //awinda Xsens data player
	{
		target.position = new Vector3(5 + pattern_4_4[currentCnt][0] * -50, 35 - pattern_4_4[currentCnt][1] * 70, -30);
		//target.position = new Vector3(15 + pattern_4_4[currentCnt][0] * -80, 55 - pattern_4_4[currentCnt][1] * 100, -30);

		if (upperArm != null && forearm != null && hand != null && elbow != null && target != null)
		{
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

		yield return new WaitForSeconds(0.00f);
	}




	// Update is called once per frame
	void Update()
    {
		statusText.text = "Current Frame " + frameCount;
		ticTime = (stdBPM / musicBPM) * (musictemp / stdTemp);
		nextTime += Time.deltaTime;
		nextConductTime += Time.deltaTime;
		ticCount++;

        if (nextConductTime > restTime)
        {
			frameCount++;
			StartCoroutine(conductMotionPlay());

			if (isPlay)
            {
				currentCnt++;
				StartCoroutine(conductMotionPlay());
				if (currentCnt >= pattern_4_4.Count - 1)
					currentCnt = 0;

                

                //StartCoroutine(conductMotionPlay());
                //if (frameCount < timing.Count)
                //{
                //    if (isCountingStart)
                //        currentCnt++;
                //    if (currentCnt >= pattern_4_4.Count - 1)
                //        currentCnt = 0;
                //}
            }
			nextConductTime = 0;
        }

        if (nextTime > ticTime)
        {
			nextTime = 0;
			//restTime = (pattern_4_4.Count / 4 / (ticTime/ticCount));
			restTime = ((ticCount / ticTime) / (pattern_4_4.Count / ticTime * 4))/ 75;
			Debug.Log("tikTime : " + ticTime + "  tikCount : " + ticCount + "  restTime "+ restTime);

			ticCount = 0;

		}

        //if (frameCount< timing.Count)
        //      {
        //	if (timing[frameCount] == 0)
        //	{
        //		if (startFrame != frameCount)
        //		{
        //			isChanged = true;

        //			if (!isStart && !isCountingStart)
        //			{
        //				isStart = true;
        //			}
        //		}

        //		startFrame = frameCount;

        //		Debug.Log("1st " + frameCount);

        //		beatStr = "1st ";
        //		beatStr += timingData.Count.ToString();
        //		beatStr += " : ";

        //		if (isChanged)
        //		{
        //			statusStr = "";
        //			if (timingData.Count > 0)
        //				statusStr += timingData[timingData.Count - 1].ToString();
        //			statusStr += " ";
        //			statusStr += frameCount.ToString();
        //			statusStr += "  ";
        //			if (timingData.Count > 0)
        //			{
        //				statusStr += (frameCount - timingData[timingData.Count - 1]).ToString();
        //				statusStr += " ";
        //				float temp = (float)((float)(0.0416f) * (float)(frameCount - timingData[timingData.Count - 1]) / (float)75);
        //				restTime = temp;

        //				statusStr += temp;
        //			}



        //			timingData.Add(frameCount);
        //			isChanged = false;
        //		}

        //	}
        //	else if (timing[frameCount] == 1)
        //	{
        //		if (startFrame != frameCount)
        //		{
        //			isChanged = true;

        //			if (!isStart && !isCountingStart)
        //			{
        //				isStart = true;
        //			}

        //		}

        //		startFrame = frameCount;
        //		Debug.Log("2nd " + frameCount);
        //		beatStr = "2nd ";
        //		beatStr += timingData.Count.ToString();
        //		beatStr += " : ";



        //		if (isChanged)
        //		{
        //			statusStr = "";
        //			if (timingData.Count > 0)
        //				statusStr += timingData[timingData.Count - 1].ToString();
        //			statusStr += " ";
        //			statusStr += frameCount.ToString();
        //			statusStr += "  ";
        //			if (timingData.Count > 0)
        //			{
        //				statusStr += (frameCount - timingData[timingData.Count - 1]).ToString();
        //				statusStr += " ";
        //				float temp = (float)((float)(0.0416f) * (float)(frameCount - timingData[timingData.Count - 1]) / (float)75);
        //				restTime = temp;

        //				statusStr += temp;


        //			}

        //			timingData.Add(frameCount);
        //			isChanged = false;
        //		}

        //	}
        //	else if (timing[frameCount] == 2)
        //	{
        //		if (startFrame != frameCount)
        //		{
        //			isChanged = true;

        //			if (!isStart && !isCountingStart)
        //			{
        //				isStart = true;
        //			}
        //		}

        //		startFrame = frameCount;
        //		Debug.Log("3rd " + frameCount);
        //		beatStr = "3rd ";
        //		beatStr += timingData.Count.ToString();
        //		beatStr += " : ";

        //		if (isChanged)
        //		{
        //			statusStr = "";
        //			if (timingData.Count > 0)
        //				statusStr += timingData[timingData.Count - 1].ToString();
        //			statusStr += " ";
        //			statusStr += frameCount.ToString();
        //			statusStr += "  ";
        //			if (timingData.Count > 0)
        //			{
        //				statusStr += (frameCount - timingData[timingData.Count - 1]).ToString();
        //				statusStr += " ";
        //				float temp = (float)((float)(0.0416f) * (float)(frameCount - timingData[timingData.Count - 1]) / (float)75);
        //				restTime = temp;

        //				statusStr += temp;


        //			}

        //			timingData.Add(frameCount);
        //			isChanged = false;
        //		}
        //	}
        //	else if (timing[frameCount] == 3)
        //	{
        //		if (startFrame != frameCount)
        //		{
        //			isChanged = true;

        //			if (!isStart && !isCountingStart)
        //			{
        //				isStart = true;
        //			}
        //		}

        //		startFrame = frameCount;
        //		Debug.Log("4th " + frameCount);
        //		beatStr = "4th ";
        //		beatStr += timingData.Count.ToString();
        //		beatStr += " : ";


        //		if (isChanged)
        //		{
        //			statusStr = "";
        //			if (timingData.Count > 0)
        //				statusStr += timingData[timingData.Count - 1].ToString();
        //			statusStr += " ";
        //			statusStr += frameCount.ToString();
        //			statusStr += "  ";
        //			if (timingData.Count > 0)
        //			{
        //				statusStr += (frameCount - timingData[timingData.Count - 1]).ToString();
        //				statusStr += " ";
        //				float temp = (float)((float)(0.0416f) * (float)(frameCount - timingData[timingData.Count - 1]) / (float)75);
        //				restTime = temp;

        //				statusStr += temp;


        //			}

        //			timingData.Add(frameCount);
        //			isChanged = false;
        //		}

        //	}



        //	if(isStart)
        //          {
        //		isCountingStart = true;
        //		isStart = false;

        //		currentCnt = 0;

        //	}

        //	statusText.text+="  "+beatStr+ statusStr;

        //	///////////////////////////////////////////////////////////////////////////////////////// Left Hand Add
        //	if(isCueStart)
        //          {
        //		int cue_idx = frameCount - cueStartFrame;

        //		if(cue_idx >= cue_data.Count)
        //              {
        //			isCueStart = false;
        //			cueStartFrame = 0;

        //		}
        //		else
        //              {
        //			left_shoulder.rotation = cue_data[cue_idx][0];
        //			left_elbow.rotation = cue_data[cue_idx][1];
        //		}
        //	}

        //	if (isDecreStart)
        //	{
        //		int cue_idx = frameCount - decreStartFrame;

        //		if (cue_idx >= decrescendo_data.Count)
        //		{
        //			isDecreStart = false;
        //			decreStartFrame = 0;

        //		}
        //		else
        //		{
        //			left_shoulder.rotation = decrescendo_data[cue_idx][0];
        //			left_elbow.rotation = decrescendo_data[cue_idx][1];
        //		}
        //	}

        //	if (isCresStart)
        //	{
        //		int cue_idx = frameCount - cresStartFrame;

        //		if (cue_idx >= crescendo_data.Count)
        //		{
        //			isCresStart = false;
        //			cresStartFrame = 0;

        //		}
        //		else
        //		{
        //			left_shoulder.rotation = crescendo_data[cue_idx][0];
        //			left_elbow.rotation = crescendo_data[cue_idx][1];
        //		}
        //	}

        //	if (isCut_offStart)
        //	{
        //		int cue_idx = frameCount - cut_offStartFrame;

        //		if (cue_idx >= cut_off_data.Count)
        //		{
        //			isCut_offStart = false;
        //			cut_offStartFrame = 0;

        //		}
        //		else
        //		{
        //			left_shoulder.rotation = cut_off_data[cue_idx][0];
        //			left_elbow.rotation = cut_off_data[cue_idx][1];
        //		}
        //	}
        //}



        ///////////////////////////////////////////////////////////////////////////////////////// Left Hand Add
        if (isCueStart)
        {
            int cue_idx = frameCount - cueStartFrame;

			if (cue_idx > 100)
            {
				if(!isPlay)
                {
					isPlay = true;
					currentCnt = 75;
					currentCnt++;
				}
			}

            if (cue_idx >= cue_data.Count)
            {
                isCueStart = false;
                cueStartFrame = 0;

            }
            else
            {
                left_shoulder.rotation = cue_data[cue_idx][0];
                left_elbow.rotation = cue_data[cue_idx][1];
            }
        }

        if (isDecreStart)
        {
            int cue_idx = frameCount - decreStartFrame;

            if (cue_idx >= decrescendo_data.Count)
            {
                isDecreStart = false;
                decreStartFrame = 0;

            }
            else
            {
                left_shoulder.rotation = decrescendo_data[cue_idx][0];
                left_elbow.rotation = decrescendo_data[cue_idx][1];
            }
        }

        if (isCresStart)
        {
            int cue_idx = frameCount - cresStartFrame;

            if (cue_idx >= crescendo_data.Count)
            {
                isCresStart = false;
                cresStartFrame = 0;

            }
            else
            {
                left_shoulder.rotation = crescendo_data[cue_idx][0];
                left_elbow.rotation = crescendo_data[cue_idx][1];
            }
        }

        if (isCut_offStart)
        {
            int cue_idx = frameCount - cut_offStartFrame;

            if (cue_idx >= cut_off_data.Count)
            {
                isCut_offStart = false;
                cut_offStartFrame = 0;

            }
            else
            {
                left_shoulder.rotation = cut_off_data[cue_idx][0];
                left_elbow.rotation = cut_off_data[cue_idx][1];
            }
        }




        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
			if (isPlay)
			{
				currentCnt++;
			}

		}
		if (Input.GetKeyDown(KeyCode.Keypad5))
		{
			if (isPlay)
			{
				currentCnt--;
			}

		}
		if (Input.GetKeyDown(KeyCode.F1))
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

		if (Input.GetKeyDown(KeyCode.F2))
		{

			Debug.Log("BPM : 60 bpm");
			musicBPM = 60f;

		}


		if (Input.GetKeyDown(KeyCode.F3))
		{

			Debug.Log("BPM : 90 bpm");
			musicBPM = 90f;

		}


		if (Input.GetKeyDown(KeyCode.Z))
		{
			Debug.Log("Cue");
			isCueStart = true;
			cueStartFrame = frameCount;


		}

		if (Input.GetKeyDown(KeyCode.X))
		{
			Debug.Log("cut_off " + frameCount);
			isCut_offStart = true;
			cut_offStartFrame = frameCount;

		}

		if (Input.GetKeyDown(KeyCode.C))
		{
			Debug.Log("dece " + frameCount);
			isDecreStart = true;
			decreStartFrame = frameCount;

		}

		if (Input.GetKeyDown(KeyCode.V))
		{
			Debug.Log("cere " + frameCount);
			isCresStart = true;
			cresStartFrame = frameCount;

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
