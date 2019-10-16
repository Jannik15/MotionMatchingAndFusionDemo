using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CSVHandler
{
    private string path = "Assets/Resources/MotionMatching";
    private string fileName = "AnimData.csv";
    private string mmFileName = "MMAnimData.csv";
    private string idleFileName = "IdleAnimData.csv";
    private static string[] csvLabels =
    {
        // General info
        "ClipName" /*[0]*/,         "Frame" /*[1]*/,

        // Pose data
        "RootPos.x" /*[2]*/,        "RootPos.z" /*[3]*/,
        "LFootPos.x" /*[4]*/,       "LFootPos.y" /*[5]*/,   "LFootPos.z" /*[6]*/,
        "RFootPos.x" /*[7]*/,       "RFootPos.y" /*[8]*/,     "RFootPos.z" /*[9]*/,

        // TrajectoryPoint data
        "RootPos.x" /*[10]*/,        "RootPos.z" /*[11]*/,  // TODO: Remove this root pos since we have duplicates, but make sure to edit the correct references to the other
        "Forward.x" /*[12]*/,        "Forward.z"  /*[13]*/
    };
    private List<string> allClipNames;
    private List<float> allFrames;
    private List<MMPose> allPoses;
    private List<TrajectoryPoint> allPoints;
    private bool ignoreFrame = false;


    public void WriteCSV(List<MMPose> poseData, List<Trajectory> pointData, List<string> clipNames, List<float> frames)
    {
	    if (!AssetDatabase.IsValidFolder(path))
	    {
		    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
		    {
			    AssetDatabase.CreateFolder("Assets", "Resources");
		    }
		    AssetDatabase.CreateFolder("Assets/Resources", "MotionMatching");
	    }
        using (var file = File.CreateText(path + "/" + fileName))
        {
            file.WriteLine(string.Join(",", csvLabels));

            // System language generalization
            string spec = "G";
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");

            for (int i = 0; i < poseData.Count; i++)
            {
	            Matrix4x4 charSpace = new Matrix4x4();
				charSpace.SetTRS(pointData[0].GetRootPoint().GetPoint(), pointData[i].GetRotation(), Vector3.one);

                string[] tempLine =
                {
                    // General info
                    clipNames[i], frames[i].ToString(spec, ci),

                    // Pose data
                    poseData[i].GetRootPos().x.ToString(spec, ci),
                    poseData[i].GetRootPos().z.ToString(spec, ci),
                    charSpace.MultiplyPoint3x4(poseData[i].GetLeftFootPos()).x.ToString(spec, ci),
                    charSpace.MultiplyPoint3x4(poseData[i].GetLeftFootPos()).y.ToString(spec, ci),
                    charSpace.MultiplyPoint3x4(poseData[i].GetLeftFootPos()).z.ToString(spec, ci),
                    charSpace.MultiplyPoint3x4(poseData[i].GetRightFootPos()).x.ToString(spec, ci),
                    charSpace.MultiplyPoint3x4(poseData[i].GetRightFootPos()).y.ToString(spec, ci),
                    charSpace.MultiplyPoint3x4(poseData[i].GetRightFootPos()).z.ToString(spec, ci),
					
                    //poseData[i].GetRootVelocity().x.ToString(spec, ci),
                    //poseData[i].GetRootVelocity().z.ToString(spec, ci),
                    //poseData[i].GetLefFootVelocity().x.ToString(spec, ci), 
                    //poseData[i].GetLefFootVelocity().y.ToString(spec, ci), 
                    //poseData[i].GetLefFootVelocity().z.ToString(spec, ci),
                    //poseData[i].GetRightFootVelocity().x.ToString(spec, ci), 
                    //poseData[i].GetRightFootVelocity().y.ToString(spec, ci), 
                    //poseData[i].GetRightFootVelocity().z.ToString(spec, ci), 

                    // TrajectoryPoint data
                    pointData[i].GetRootPoint().GetPoint().x.ToString(spec, ci), 
                    pointData[i].GetRootPoint().GetPoint().z.ToString(spec, ci),
                    pointData[i].GetRootPoint().GetForward().x.ToString(spec, ci), 
                    pointData[i].GetRootPoint().GetForward().z.ToString(spec, ci)
                };

                file.WriteLine(string.Join(",", tempLine));
            }
        }
    }
    public List<FeatureVector> ReadCSV(int trajPointsLength, int trajStepSize)
    {
        StreamReader reader = new StreamReader(path + "/" + fileName);

        bool ignoreHeaders = true;

        allClipNames = new List<string>();
        allFrames = new List<float>();
        allPoses = new List<MMPose>();
        allPoints = new List<TrajectoryPoint>();
        List<FeatureVector> featuresFromCSV = new List<FeatureVector>();

        while (true) // True until break is called within the loop
        {
            string dataString = reader.ReadLine(); // Reads a line (or row) in the CSV file

            if (dataString == null) // No more data to be read, so break from the while loop
            {
                break;
            }
            string[] tempString = dataString.Split(','); // line is split into each column
            NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;

            if (!ignoreHeaders) // Iterates for each row in the CSV aside from the first (header) row
            {
                allClipNames.Add(tempString[0]);
                allFrames.Add(float.Parse(tempString[1], format));
                allPoses.Add(new MMPose(new Vector3(float.Parse(tempString[2], format), 0.0f, float.Parse(tempString[3], format)),
                    new Vector3(float.Parse(tempString[4], format), float.Parse(tempString[5], format), float.Parse(tempString[6], format)),
                    new Vector3(float.Parse(tempString[7], format), float.Parse(tempString[8], format), float.Parse(tempString[9], format))));
                allPoints.Add(new TrajectoryPoint(new Vector3(float.Parse(tempString[10], format), 0.0f, float.Parse(tempString[11], format)),
                    new Vector3(float.Parse(tempString[12], format), 0.0f, float.Parse(tempString[13], format))));
            }
            else
                ignoreHeaders = false;
        }

        // Convert data to FeatureVector
        for (int i = 0; i < allClipNames.Count; i++)
        {
            TrajectoryPoint[] trajPoints = new TrajectoryPoint[trajPointsLength];
            for (int j = 0; j < trajPointsLength; j++)
            {
                if (i + j * trajStepSize < allClipNames.Count)
                {
                    if (allFrames[i] < allFrames[i + j * trajStepSize]) // clip 3 at frame 45 out of 70 with a trajStepSize of 10 goes 45, 55, 65, X, X
                    {
                        //if (i + j * trajStepSize < allPoints.Count) // Out of range prevention
                        trajPoints[j] = new TrajectoryPoint(allPoints[i + j * trajStepSize].GetPoint(), allPoints[i + j * trajStepSize].GetForward());
                    }
                    else
                    {
                        trajPoints[j] = new TrajectoryPoint();
                        //ignoreFrame = true;
                    }
                }
                else
                {
                    trajPoints[j] = new TrajectoryPoint();
                    //ignoreFrame = true;
                }
            }
            if (!ignoreFrame)
                featuresFromCSV.Add(new FeatureVector(allPoses[i], new Trajectory(trajPoints), i, allClipNames[i], allFrames[i]));
            ignoreFrame = false;
        }
        return featuresFromCSV;
    }

}
