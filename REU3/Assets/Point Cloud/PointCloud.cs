using UnityEngine;
using System.IO;
using System;
using System.IO.Compression;

public class PointCloud : MonoBehaviour
{


    [SerializeField] Material depthMat; //materials for depth and color output

    nuitrack.DepthFrame depthFrame = null;

    [SerializeField] int hRes;
    int frameStep;
    bool record = false;

    [SerializeField] float meshScaling = 1f;
    Texture2D depthTexture;

    //depthFrame adn depthTexture will always be same size determined by vicovr app
    Color[] depthColors;


    int[,] depthArray;


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

        depthTexture.Apply();
        Debug.Log(pointIndex);

    }

    public void OnClick()
    {
        string directory = "";
        //create directory to save pngs to

        if (!record)
        {
            record = true;
            directory = Directory.CreateDirectory(Application.persistentDataPath + "/" + DateTime.Now.ToLongTimeString()).FullName;

        }
        else
        {
            record = false;
            //include zipfile code here
        }

        while(record)
        {
            string filename = "/" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".png";
            byte[] bytes = depthTexture.EncodeToPNG();

            File.WriteAllBytes(directory + filename, bytes);
            
        }

       
      

        using (StreamWriter outfile = new StreamWriter(Application.persistentDataPath + "/depthArray.csv"))
        {
            for (int x = 0; x < depthArray.Length; x++)
            {
                string content = "";
                for (int y = 0; y < 128; y++)
                {
                    content += depthArray[x,y].ToString() + ",";
                }
                //trying to write data to csv
                outfile.WriteLine(content);
            }


        }
    }

}
