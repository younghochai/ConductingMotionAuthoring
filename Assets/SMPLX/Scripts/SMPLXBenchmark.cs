using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LightweightMatrixCSharp;

public class SMPLXBenchmark : MonoBehaviour
{
    public GameObject target;
    public Text bodiesText;
    public Text statusText;
    public const int NUM_BETAS = 10;
    public const int NUM_EXPRESSIONS = 10;
    public const int NUM_JOINTS = 55;

    private List<SkinnedMeshRenderer> _smrList;
    private bool _animate = false;
    private int _numShapes = 0;
    private int _numBodies = 1;
    private int _numBetaShapes;
    private int _numExpressions;
    private int _numPoseCorrectives;
    public enum BodyPose { T, A, P };
    public bool usePoseCorrectives = true;

    public ModelType modelType = ModelType.Unknown;
    public enum ModelType { Unknown, Female, Neutral, Male };
    public float[] betas = new float[NUM_BETAS];


    private Mesh _sharedMeshDefault = null;
    private bool _defaultShape = true;

    private Mesh _bakedMesh = null;
    private Vector3[] _jointPositions = null;
    private Quaternion[] _jointRotations = null;

    private SkinnedMeshRenderer _smr = null;
    string[] _bodyJointNames = new string[] { "pelvis", "left_hip", "right_hip", "spine1", "left_knee", "right_knee", "spine2", "left_ankle", "right_ankle", "spine3", "left_foot", "right_foot", "neck", "left_collar", "right_collar", "head", "left_shoulder", "right_shoulder", "left_elbow", "right_elbow", "left_wrist", "right_wrist", "jaw", "left_eye_smplhf", "right_eye_smplhf", "left_index1", "left_index2", "left_index3", "left_middle1", "left_middle2", "left_middle3", "left_pinky1", "left_pinky2", "left_pinky3", "left_ring1", "left_ring2", "left_ring3", "left_thumb1", "left_thumb2", "left_thumb3", "right_index1", "right_index2", "right_index3", "right_middle1", "right_middle2", "right_middle3", "right_pinky1", "right_pinky2", "right_pinky3", "right_ring1", "right_ring2", "right_ring3", "right_thumb1", "right_thumb2", "right_thumb3" };
    string[] _handLeftJointNames = new string[] { "left_index1", "left_index2", "left_index3", "left_middle1", "left_middle2", "left_middle3", "left_pinky1", "left_pinky2", "left_pinky3", "left_ring1", "left_ring2", "left_ring3", "left_thumb1", "left_thumb2", "left_thumb3" };
    string[] _handRightJointNames = new string[] { "right_index1", "right_index2", "right_index3", "right_middle1", "right_middle2", "right_middle3", "right_pinky1", "right_pinky2", "right_pinky3", "right_ring1", "right_ring2", "right_ring3", "right_thumb1", "right_thumb2", "right_thumb3" };
    Dictionary<string, Transform> _transformFromName;

    public void SetLocalJointRotation(string name, Quaternion quatLocal)
    {
        Transform joint = _transformFromName[name];
        joint.localRotation = quatLocal;
    }
    public void SetBodyPose(BodyPose pose)
    {
        if (pose == BodyPose.T)
        {
            ResetBodyPose();
        }
        else if (pose == BodyPose.A)
        {
            ResetBodyPose();
            SetLocalJointRotation("left_collar", Quaternion.Euler(0.0f, 0.0f, 10.0f));
            SetLocalJointRotation("left_shoulder", Quaternion.Euler(0.0f, 0.0f, 35.0f));
            SetLocalJointRotation("right_collar", Quaternion.Euler(0.0f, 0.0f, -10.0f));
            SetLocalJointRotation("right_shoulder", Quaternion.Euler(0.0f, 0.0f, -35.0f));
        }
        else if (pose == BodyPose.P)
        {
            ResetBodyPose();
            SetLocalJointRotation("left_collar", Quaternion.Euler(0.0f, 0.0f, 10.0f));
            SetLocalJointRotation("left_shoulder", Quaternion.Euler(0.0f, 0.0f, -40.0f));
            SetLocalJointRotation("left_elbow", Quaternion.Euler(0.0f, -145.0f, 0.0f));
            SetLocalJointRotation("right_collar", Quaternion.Euler(0.0f, 0.0f, -10.0f));
            SetLocalJointRotation("right_shoulder", Quaternion.Euler(0.0f, 0.0f, 40.0f));
        }

        UpdatePoseCorrectives();
        UpdateJointPositions(false);
    }
    public void ResetBodyPose()
    {
        foreach (string name in _bodyJointNames)
        {
            Transform joint = _transformFromName[name];
            joint.localRotation = Quaternion.identity;
        }

        UpdateJointPositions(false);
    }

    public bool HasBetaShapes()
    {
        return (_numBetaShapes > 0);
    }

    public bool HasExpressions()
    {
        return (_numExpressions > 0);
    }

    public bool HasPoseCorrectives()
    {
        return (_numPoseCorrectives > 0);
    }

    public Vector3[] GetJointPositions()
    {
        return _jointPositions;
    }


    public void UpdatePoseCorrectives()
    {
        //if (!usePoseCorrectives)
        //    return;

        //if (!HasPoseCorrectives())
        //    return;

        // Body joint #0 has no pose correctives
        for (int i = 1; i < _bodyJointNames.Length; i++)
        {
            string name = _bodyJointNames[i];
            Quaternion quat = _transformFromName[name].localRotation;

            // Local joint coordinate systems
            //   Unity:  X-Left,  Y-Up, Z-Back, Left-handed
            //   SMPL-X: X-Right, Y-Up, Z-Back, Right-handed
            Quaternion quatSMPLX = new Quaternion(-quat.x, quat.y, quat.z, -quat.w);
            Matrix4x4 m = Matrix4x4.Rotate(quatSMPLX);
            // Subtract identity matrix to get proper pose shape weights
            m[0, 0] = m[0, 0] - 1.0f;
            m[1, 1] = m[1, 1] - 1.0f;
            m[2, 2] = m[2, 2] - 1.0f;

            // Get corrective pose start index
            int poseStartIndex = NUM_BETAS + NUM_EXPRESSIONS + (i - 1) * 9;

            _smr.SetBlendShapeWeight(poseStartIndex + 0, 100.0f * m[0, 0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 1, 100.0f * m[0, 1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 2, 100.0f * m[0, 2]);

            _smr.SetBlendShapeWeight(poseStartIndex + 3, 100.0f * m[1, 0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 4, 100.0f * m[1, 1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 5, 100.0f * m[1, 2]);

            _smr.SetBlendShapeWeight(poseStartIndex + 6, 100.0f * m[2, 0]);
            _smr.SetBlendShapeWeight(poseStartIndex + 7, 100.0f * m[2, 1]);
            _smr.SetBlendShapeWeight(poseStartIndex + 8, 100.0f * m[2, 2]);
        }
    }

    public bool UpdateJointPositions(bool recalculateJoints = true)
    {
        if (HasBetaShapes() && recalculateJoints)
        {
            if (_sharedMeshDefault == null)
            {
                // Do not clone mesh if we haven't modified the shape parameters yet
                if (_defaultShape)
                    return false;

                // Clone default shared mesh so that we can modify later the shared mesh bind pose without affecting other shared instances.
                // Note that this will drastically increase the Unity scene file size and make Unity Editor very slow on save when multiple bodies like this are used.
                _sharedMeshDefault = _smr.sharedMesh;
                _smr.sharedMesh = (Mesh)Instantiate(_smr.sharedMesh);
                Debug.LogWarning("[SMPL-X] Cloning shared mesh to allow for joint recalculation on beta shape change [" + gameObject.name + "]. Note that this will increase the current scene file size significantly if model contains pose correctives.");
            }

            // Save pose and repose to T-Pose
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointRotations[i] = joint.localRotation;
                joint.localRotation = Quaternion.identity;
            }

            // Create beta value matrix
            Matrix betaMatrix = new Matrix(NUM_BETAS, 1);
            for (int row = 0; row < NUM_BETAS; row++)
            {
                betaMatrix[row, 0] = betas[row];
            }

            //// Apply joint regressor to beta matrix to calculate new joint positions
            string gender = "";
            //if (modelType == SMPLX.ModelType.Female)
            //    gender = "female";
            //else if (modelType == SMPLX.ModelType.Neutral)
            //    gender = "neutral";
            //else if (modelType == SMPLX.ModelType.Male)
            //    gender = "male";
            //else
            //{
            //    Debug.LogError("[SMPL-X] ERROR: Joint regressor needs model type information (Female/Neutral/Male)");
            //    return false;
            //}

            Matrix[] betasToJoints = SMPLX.JointMatrices["betasToJoints_" + gender];
            Matrix[] templateJ = SMPLX.JointMatrices["templateJ_" + gender]; ;

            Matrix newJointsX = betasToJoints[0] * betaMatrix + templateJ[0];
            Matrix newJointsY = betasToJoints[1] * betaMatrix + templateJ[1];
            Matrix newJointsZ = betasToJoints[2] * betaMatrix + templateJ[2];

            // Update joint position cache
            for (int index = 0; index < NUM_JOINTS; index++)
            {
                Transform joint = _transformFromName[_bodyJointNames[index]];

                // Convert regressor coordinate system (OpenGL) to Unity coordinate system by negating X value
                Vector3 position = new Vector3(-(float)newJointsX[index, 0], (float)newJointsY[index, 0], (float)newJointsZ[index, 0]);

                // Regressor joint positions from joint calculation are centered at origin in world space
                // Transform to game object space for correct world space position
                joint.position = gameObject.transform.TransformPoint(position);
            }

            // Set new bind pose
            Matrix4x4[] bindPoses = _smr.sharedMesh.bindposes;
            Transform[] bones = _smr.bones;
            for (int i = 0; i < bones.Length; i++)
            {
                // The bind pose is bone's inverse transformation matrix.
                // Make this matrix relative to the avatar root so that we can move the root game object around freely.
                bindPoses[i] = bones[i].worldToLocalMatrix * gameObject.transform.localToWorldMatrix;
            }
            _smr.sharedMesh.bindposes = bindPoses;

            // Restore pose
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                Transform joint = _transformFromName[_bodyJointNames[i]];
                joint.localRotation = _jointRotations[i];

                // Update joint position cache
                _jointPositions[i] = joint.position;

            }
        }
        else
        {
            for (int i = 0; i < NUM_JOINTS; i++)
            {
                // Update joint position cache
                Transform joint = _transformFromName[_bodyJointNames[i]];
                _jointPositions[i] = joint.position;
            }
        }

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        _smrList = new List<SkinnedMeshRenderer>();
        _smrList.Add(target.GetComponentInChildren<SkinnedMeshRenderer>());
        if (statusText != null)
            statusText.text = "Active Blendshapes: 0";

        // Get skinned mesh blend shape values
        _numBetaShapes = 0;
        _numExpressions = 0;
        _numPoseCorrectives = 0;

        _smr = target.GetComponentInChildren<SkinnedMeshRenderer>();

        int blendShapeCount = _smr.sharedMesh.blendShapeCount;
        for (int i = 0; i < blendShapeCount; i++)
        {
            string name = _smr.sharedMesh.GetBlendShapeName(i);
            if (name.StartsWith("Shape"))
                _numBetaShapes++;
            else if (name.StartsWith("Exp"))
                _numExpressions++;
            else if (name.StartsWith("Pose"))
                _numPoseCorrectives++;
        }

    }

    void ResetBlendshapes()
    {
        foreach (SkinnedMeshRenderer smr in _smrList)
        {
            int blendShapeCount = smr.sharedMesh.blendShapeCount;
            for (int i=0; i<blendShapeCount; i++)
                smr.SetBlendShapeWeight(i, 0.0f);
        }
    }

    int GetActiveBlendshapes()
    {
        int activeBlendshapes = 0;
        foreach (SkinnedMeshRenderer smr in _smrList)
        {
            int blendShapeCount = smr.sharedMesh.blendShapeCount;
            for (int i=0; i<blendShapeCount; i++)
            {
                if (smr.GetBlendShapeWeight(i) != 0.0f)
                    activeBlendshapes++;
            }
        }
        return activeBlendshapes;
        
    }

    void SetWeights(float value)
    {
        foreach (SkinnedMeshRenderer smr in _smrList)
        {
            for (int i=0; i<_numShapes; i++)
                smr.SetBlendShapeWeight(i, value);
        }
        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (statusText != null)
            statusText.text = string.Format("Active Blendshapes: {0}", GetActiveBlendshapes());
    }

    // Update is called once per frame
    void Update()
    {
        if (_animate)
        {
            foreach (SkinnedMeshRenderer smr in _smrList)
            {
                for (int i=0; i<_numShapes; i++)
                {
                    float value = Mathf.Sin(2*Mathf.PI * Time.time)*50 + 100.0f;

                    smr.SetBlendShapeWeight(i, value);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {

            SetBodyPose(BodyPose.T);
            UpdatePoseCorrectives();
            UpdateJointPositions(false);
        }


        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _animate = false;
            ResetBlendshapes();
            UpdateStatus();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ResetBlendshapes();
            _numShapes = 10;
            SetWeights(200.0f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ResetBlendshapes();
            _numShapes = 20;
            SetWeights(200.0f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ResetBlendshapes();
            _numShapes = 506;
            SetWeights(200.0f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _animate = !_animate;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            int newBodies = _numBodies;
            for (int i=0; i<newBodies; i++)
            {
                _numBodies++;
                float x = 2.5f * Mathf.Sin(2*Mathf.PI*(_numBodies)/20.0f);
                float y = target.transform.position.y;
                float z = _numBodies * 0.25f;
                Vector3 pos = new Vector3(x, y, z);
                GameObject go = Instantiate(target, pos, Quaternion.Euler(0.0f, 180.0f, 0.0f)) as GameObject;
                _smrList.Add(go.GetComponentInChildren<SkinnedMeshRenderer>());
            }

            if (bodiesText != null)
                bodiesText.text = string.Format("Bodies: {0}", _numBodies);

            UpdateStatus();
        }        
    }
}
