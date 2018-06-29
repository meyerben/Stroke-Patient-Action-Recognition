#region Description

// The script performs a direct translation of the skeleton using only the position data of the joints.
// Objects in the skeleton will be created when the scene starts.

#endregion


using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

[AddComponentMenu("Nuitrack/Example/TranslationAvatar")]
public class NativeAvatar : MonoBehaviour
{
    //GUI global variables
    string message = "Test";

    //skeletal global variables
    public nuitrack.JointType[] typeJoint;
    GameObject[] CreatedJoint;
    public GameObject PrefabJoint;

    //file global variables
    string path;
    StreamWriter file;

    int frameNum = 0;

    void Start()
    {

        path = Application.persistentDataPath;
        path = Path.Combine(path, "skeletalData.txt");
        string labels = "ID,Timestamp";
        file = new StreamWriter(path);


        CreatedJoint = new GameObject[typeJoint.Length];
        for (int q = 0; q < typeJoint.Length; q++)
        {

            labels += "," + typeJoint[q] + "_Conf";
            labels += "," + typeJoint[q] + "_X";
            labels += "," + typeJoint[q] + "_Y";
            labels += "," + typeJoint[q] + "_Z";

            CreatedJoint[q] = Instantiate(PrefabJoint);
            CreatedJoint[q].transform.SetParent(transform);
        }
        message = "Skeleton created";

        file.WriteLine(labels);


    }

    void Update()
    {

        frameNum++;
        if (CurrentUserTracker.CurrentUser != 0)
        {
            string newData = "";
            newData += frameNum + "," + System.DateTime.Now.Hour + "" + System.DateTime.Now.Minute + System.DateTime.Now.Second + System.DateTime.Now.Millisecond;


            nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
            message = "Skeleton found";

            for (int q = 0; q < typeJoint.Length; q++)
            {
                nuitrack.Joint joint = skeleton.GetJoint(typeJoint[q]);
                Vector3 newPosition = 0.001f * joint.ToVector3();
                CreatedJoint[q].transform.localPosition = newPosition;

                newData += "," + joint.Confidence;
                newData += "," + joint.ToVector3().x;
                newData += "," + joint.ToVector3().y;
                newData += "," + joint.ToVector3().z;

            }
            file.WriteLine(newData);
        }
        else
        {
            message = "Skeleton not found";
        }


    }

    // Display the message on the screen
    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        GUILayout.Label(message);
    }

    void OnApplicationPause(bool pause)
    {
        System.DateTime moment = System.DateTime.Now;
        string time = moment.Hour + "." + moment.Minute + "." + moment.Second + "." + moment.Millisecond;
        string newPath = Application.persistentDataPath;
        string fileName = "skeletalData_"  + time + ".csv";
        newPath = Path.Combine(newPath, fileName);
        if (pause)
        {
            file.Close();
            File.Move(path, newPath);
        }
    }

}