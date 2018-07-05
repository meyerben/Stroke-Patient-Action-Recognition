using UnityEngine;
using System.IO;
using System;
using System.Threading;


public class PointCloud : MonoBehaviour
{

    //SKELINGTON VARIABLES
    public nuitrack.JointType[] typeJoint;
    GameObject[] CreatedJoint;
    public GameObject PrefabJoint;
    //file global variables
    string path;
    StreamWriter file;
    int frameNum = 0;


    [SerializeField] Material depthMat; //materials for depth and color output

    nuitrack.DepthFrame depthFrame = null;

    [SerializeField] int hRes;
    int frameStep;
    bool record = false;
    string recordDirectory = "";

    
    

    [SerializeField] float meshScaling = 1f;
    Texture2D depthTexture;

    //depthFrame adn depthTexture will always be same size determined by vicovr app
    Color[] depthColors;


    //DEBUG VARIABLES
    int[,] depthArray;

    //GUI VARIABLES
    string message = "";
    float min_fps;
    float avg_fps;
    float timer = 0f;
    int frames = 0;
    float measureTime = 1f;

    bool initialized = false;

    void Start()
    {
        if (!initialized) Initialize();

       
    }

    void Initialize()
    {
        initialized = true;

        nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode(); //Returns the structure in which there is resolution, FPS and FOV of the sensor

        frameStep = mode.XRes / hRes;
        if (frameStep <= 0) frameStep = 1; // frameStep must be bigger than 0
        hRes = mode.XRes / frameStep;

        
        InitMeshes(
          (mode.XRes), //Width
          (mode.YRes), //Height
          mode.HFOV);
    }

    void InitMeshes(int cols, int rows, float hfov)
    {
        depthColors = new Color[cols * rows];

        depthTexture = new Texture2D(cols, rows, TextureFormat.ARGB32, false);
        depthTexture.filterMode = FilterMode.Point;
        depthTexture.wrapMode = TextureWrapMode.Clamp;
        depthTexture.Apply();


        depthMat.mainTexture = depthTexture;

    }

    void Update()
    {
        bool haveNewFrame = false;

        if ((NuitrackManager.DepthFrame != null))
        {
            if (depthFrame != null)
            {
                haveNewFrame = (depthFrame != NuitrackManager.DepthFrame);
            }
            depthFrame = NuitrackManager.DepthFrame;


            if (haveNewFrame) ProcessFrame(depthFrame);
        }



        //FRAMERATE COUNTER


        

    }

    void ProcessFrame(nuitrack.DepthFrame depthFrame)
    {
        int pointIndex = 0;
        depthArray = new int[depthFrame.Rows, depthFrame.Cols];
         

        for (int i = 0; i < depthFrame.Rows; i += 1)
        {
            for (int j = 0; j <depthFrame.Cols; j += 1)
            {
                //take depth from the frame and put it into the depthColors array
                depthArray[i, j] = depthFrame[i, j];

                float value = depthFrame[i, j] / 16384f;


                depthColors[pointIndex].r = value;
                depthColors[pointIndex].g = value;
                depthColors[pointIndex].b = value;
                depthColors[pointIndex].a = 1;

                

                ++pointIndex;
            }
        }


        depthTexture.SetPixels(depthColors);

        //depthTexture.Apply();
        Debug.Log(pointIndex);

        if (record)
        {
            string filename = "/" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".png";
            byte[] bytes = depthTexture.EncodeToPNG();
            File.WriteAllBytes(recordDirectory + filename, bytes);
            message = "recording";
            //writeline for skeletal data will need to go in this block

            //SKELINGTON STUFF
            //SKELINGTON UPDATE
            frameNum++;
            if (CurrentUserTracker.CurrentUser != 0)
            {
                string newData = "";
                newData += frameNum + "," + System.DateTime.Now.Hour + "" + System.DateTime.Now.Minute + System.DateTime.Now.Second + System.DateTime.Now.Millisecond;


                nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;

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

  
        }

    }

    public void OnClick()
    {
        
        //create directory to save pngs to

        if (!record)
        {
            record = true;
            recordDirectory = Directory.CreateDirectory(Application.persistentDataPath + "/" + DateTime.Now.ToLongTimeString()).FullName;

            //SKELINGTON INITIALIZATION
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
            file.WriteLine(labels);


        }
        else
        {
            record = false;
            message = "Finished Recording";

            System.DateTime moment = System.DateTime.Now;
            string time = moment.Hour + "." + moment.Minute + "." + moment.Second + "." + moment.Millisecond;
            string newPath = Application.persistentDataPath;
            string fileName = "skeletalData" + ".csv";
            newPath = Path.Combine(recordDirectory, fileName);
            file.Close();
            File.Move(path, newPath);
        }

       
      
        //raw data file writer
        //using (StreamWriter outfile = new StreamWriter(Application.persistentDataPath + "/depthArray.csv"))
        //{
        //    for (int x = 0; x < depthArray.Length; x++)
        //    {
        //        string content = "";
        //        for (int y = 0; y < 128; y++)
        //        {
        //            content += depthArray[x,y].ToString() + ",";
        //        }
        //        //trying to write data to csv
        //        outfile.WriteLine(content);
        //    }
        //}
    }

    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        GUILayout.Label(message);
    }

}
