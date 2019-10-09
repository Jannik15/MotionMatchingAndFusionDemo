﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CSVHandler
{
    private string path = "Assets/Resources/MotionMatching";
    private string fileName = "AnimData.csv";
    private static string[] csvLabels =
    {
        // General info
        "ClipName" /*[0]*/,         "Frame" /*[1]*/,

        // Pose data
        "RootVel.x" /*[2]*/,        "RootVel.z" /*[3]*/,
        "LFootVel.x" /*[4]*/,       "LFootVel.y" /*[5]*/,   "LFootVel.z" /*[6]*/,
        "RFootVel.x" /*[7]*/,       "RFootVel.y" /*[8]*/,     "RFootVel.z" /*[9]*/,

        // TrajectoryPoint data
        "RootPos.x" /*[10]*/,        "RootPos.z" /*[11]*/,
        "Forward.x" /*[12]*/,        "Forward.z"  /*[13]*/
    };
    private List<string> allClipNames;
    private List<float> allFrames;
    private List<MMPose> allPoses;
    private List<TrajectoryPoint> allPoints;

    public void WriteCSV(List<MMPose> poseData, List<TrajectoryPoint> pointData, List<string> clipNames, List<float> frames)
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
                string[] tempLine =
                {
                    // General info
                    clipNames[i], frames[i].ToString(spec, ci),

                    // Pose data
                    Mathf.Abs(poseData[i].GetRootVelocity().x).ToString(spec, ci), Mathf.Abs(poseData[i].GetRootVelocity().z).ToString(spec, ci),
                    Mathf.Abs(poseData[i].GetLefFootVelocity().x).ToString(spec, ci), Mathf.Abs(poseData[i].GetLefFootVelocity().y).ToString(spec, ci), Mathf.Abs(poseData[i].GetLefFootVelocity().z).ToString(spec, ci),
                    Mathf.Abs(poseData[i].GetRightFootVelocity().x).ToString(spec, ci), Mathf.Abs(poseData[i].GetRightFootVelocity().y).ToString(spec, ci), Mathf.Abs(poseData[i].GetRightFootVelocity().z).ToString(spec, ci), 

                    // TrajectoryPoint data
                    pointData[i].GetPoint().x.ToString(spec, ci), pointData[i].GetPoint().z.ToString(spec, ci),
                    pointData[i].GetForward().x.ToString(spec, ci), pointData[i].GetForward().z.ToString(spec, ci),
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
                    new Vector3(float.Parse(tempString[12], format),0.0f, float.Parse(tempString[13], format))));
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
                if (i + j * trajStepSize < allPoints.Count) // Out of range prevention
                    trajPoints[j] = new TrajectoryPoint(allPoints[i + j * trajStepSize].GetPoint(), allPoints[i + j * trajStepSize].GetForward());
                else
                    trajPoints[j] = new TrajectoryPoint();
            }
            featuresFromCSV.Add(new FeatureVector(allPoses[i], new Trajectory(trajPoints), i, allClipNames[i], allFrames[i]));
        }
        return featuresFromCSV;
    }
}
