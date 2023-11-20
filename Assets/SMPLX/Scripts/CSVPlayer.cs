using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Text;

public class CSVPlayer : MonoBehaviour
{
    bool zDown;
    bool bDown;
    public VideoPlayer video;
    public TextMeshProUGUI frameText;


    public List<List<List<Quaternion>>> load_quat_list = new List<List<List<Quaternion>>>();

    public List<List<Quaternion>> save_quat_list = new List<List<Quaternion>>();
    public List<List<Vector3>> save_euler_list = new List<List<Vector3>>();


    int startIdx = 0;
    int endIdx = 4284;
    bool isSaved = false;

    int fileNumCount = 0;
    int fileMax = 4;

    string prefix = "Assets/Instructions/";
    string save_prefix = "Assets/Instructions/save/";


    public SMPLX smpl_module;

    string[] _bodyJointNames = new string[] {"pelvis", "left_hip", "right_hip", "spine1", "left_knee",
                                             "right_knee", "spine2", "left_ankle", "right_ankle", "spine3",
                                             "left_foot", "right_foot", "neck", "left_collar", "right_collar",
                                             "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow",
                                             "left_wrist", "right_wrist" };  //AMASS 22 Joint

    string[] _bodyCustomJointNames = new string[] { "pelvis","spine2","right_shoulder","right_elbow", "left_shoulder",
                                                    "left_elbow"}; //awinda 10 Joint

    void Start()
    {
    }
    void write_csv_file(string file_path)
    {

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(file_path))
        {
           
            Debug.Log("save test frm cnt : " + save_quat_list.Count);
            for (int frame = 0; frame < save_quat_list.Count; frame++)
            {
                var builder = new StringBuilder();

                for (int joint = 0; joint < save_quat_list[frame].Count; joint++)
                {
                    builder.Append(save_quat_list[frame][joint].w.ToString() + ',');
                    builder.Append(save_quat_list[frame][joint].x.ToString() + ',');
                    builder.Append(save_quat_list[frame][joint].y.ToString() + ',');
                    if (joint == save_quat_list[frame].Count - 1)
                        builder.Append(save_quat_list[frame][joint].z.ToString());
                    else
                        builder.Append(save_quat_list[frame][joint].z.ToString() + ',');

                }
                file.WriteLine(builder.ToString());

                builder.Clear();
            }

        }

    }



    // Update is called once per frame
    void Update()
    {
        GetInput();
        
        _Animation();

        
        ////// Conducting Senesing Data Load 
        if (Input.GetKeyDown(KeyCode.U))
        {
            fileNumCount++;
            Debug.Log("FileCount"+fileNumCount);

            if (fileMax < fileNumCount)
                fileNumCount = 1;

            fileRead();


        }


            
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if(save_quat_list.Count >0 && isSaved ==false)
            {
                write_csv_file("");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            video.Play();

            //string keyValue = "";
            //foreach (string Key in SMPLX.JointMatrices.Keys)
            //{
            //    Debug.Log("**keys****  "+Key);
            //    keyValue = Key;
            //}
            //Debug.Log(SMPLX.JointMatrices.Values.Count);
            //Debug.Log(SMPLX.JointMatrices.Keys.Count);

           
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            video.Pause();
        }

    }

    void GetInput()
    {
        zDown = Input.GetKeyDown(KeyCode.Alpha7);
        bDown = Input.GetKeyDown(KeyCode.Alpha8);
    }

    void fileRead()
    {

        string filePath = "";
        filePath += prefix;
        filePath += fileNumCount.ToString("000");
        filePath += ".csv";
        TXTReader(filePath);
       
        Debug.Log("CSV file load done  :  " + filePath);

        StartCoroutine(avatar_play_custom());

    }

    void _Animation()
    {
        if(bDown)
        {
            StartCoroutine(avatar_play_custom());
        }
    }

    IEnumerator avatar_play_custom() //awinda Xsens data player
    {

        save_quat_list.Clear();
        save_euler_list.Clear();


        for (int frame_cnt = 0; frame_cnt < load_quat_list[0][0].Count; frame_cnt++)
        {
            frameText.text = frame_cnt.ToString();

            for (int i = 0; i < _bodyCustomJointNames.Length; i++)
            {
                smpl_module.SetWorld2LocalJointRotation(_bodyCustomJointNames[i], load_quat_list[0][i][frame_cnt]);
            }
            smpl_module.UpdateJointPositions(false);



            ///////////////////////////////////////////////////////first Image / Last Image saved Part
            //if(frame_cnt==1)
            //{
            //    string imgSavePath = "";
            //    imgSavePath += save_prefix;
            //    imgSavePath += "smpl_";
            //    imgSavePath += fileNumCount.ToString("000");
            //    imgSavePath += "_1.png";
            //    ScreenCapture.CaptureScreenshot(imgSavePath);
            //}
            //else if (frame_cnt== load_quat_list[0][0].Count-1)
            //{
            //    string imgSavePath = "";
            //    imgSavePath += save_prefix;
            //    imgSavePath += "smpl_";
            //    imgSavePath += fileNumCount.ToString("000");
            //    imgSavePath += "_2.png";
            //    ScreenCapture.CaptureScreenshot(imgSavePath);
            //}

            


            if (frame_cnt == startIdx)
                isSaved = true;

            if (frame_cnt == endIdx)
                isSaved = false;

            List<Quaternion> tempQaut = new List<Quaternion>();
            List<Vector3> tempEuler = new List<Vector3>();

            Transform joint3 = smpl_module._transformFromName["pelvis"];
            tempQaut.Add(joint3.localRotation);
            tempEuler.Add(joint3.localRotation.eulerAngles);

            Transform joint4 = smpl_module._transformFromName["spine2"];
            tempQaut.Add(joint4.localRotation);
            tempEuler.Add(joint4.localRotation.eulerAngles);
            //tempQaut.Add(joint4.rotation);
            //tempEuler.Add(joint4.eulerAngles);

            Transform joint5 = smpl_module._transformFromName["right_shoulder"];
            tempQaut.Add(joint5.localRotation);
            tempEuler.Add(joint5.localRotation.eulerAngles);

            Transform joint6 = smpl_module._transformFromName["right_elbow"];
            tempQaut.Add(joint6.localRotation);
            tempEuler.Add(joint6.localRotation.eulerAngles);


            Transform joint1 = smpl_module._transformFromName["left_shoulder"];
            tempQaut.Add(joint1.localRotation);
            tempEuler.Add(joint1.localRotation.eulerAngles);

            Transform joint2 = smpl_module._transformFromName["left_elbow"];
            tempQaut.Add(joint2.localRotation);
            tempEuler.Add(joint1.localRotation.eulerAngles);

            save_quat_list.Add(tempQaut);
            save_euler_list.Add(tempEuler);

            float time = 0.01667f;
            if (frame_cnt == 0)
                time = 5.000f;
            else
                time = 0.01667f;

            yield return new WaitForSeconds(time);
        }

        string filePath = "";
        filePath += save_prefix;
        filePath += "smpl_";
        filePath += fileNumCount.ToString("000");
        filePath += ".csv";

        write_csv_file(filePath);
        yield break;

        
    }

    void TXTReader(string file_path) 
    {
        load_quat_list.Clear();

        FileStream quatStream = new FileStream(file_path, FileMode.Open);

        StreamReader sr = new StreamReader(quatStream);
        string[] fields;
        string[] records = sr.ReadToEnd().Split('\n');

        List<List<Quaternion>> load_quat_buf = new List<List<Quaternion>>();

        for (int i = 0; i < _bodyJointNames.Length; i++)
        {
            load_quat_buf.Add(new List<Quaternion>());
        }

        float[] data_val = new float[4];

        for (int line = 0; line < records.Length; line++)
        {
            fields = records[line].Split(',');

            int fields_cnt = 0;

            for (int device_idx = 0; device_idx < (fields.Length / 4); device_idx++)
            {
                for (int val_idx = 0; val_idx < 4; val_idx++)
                {
                    data_val[val_idx] = float.Parse(fields[fields_cnt]);
                    fields_cnt++;
                }
                load_quat_buf[device_idx].Add(new Quaternion(data_val[1], data_val[2], data_val[3], data_val[0]));
            }
        }
        Debug.Log("quaternion reading done");
        sr.Close();
        quatStream.Close();

        load_quat_list.Add(load_quat_buf);

        return;
    }
}
