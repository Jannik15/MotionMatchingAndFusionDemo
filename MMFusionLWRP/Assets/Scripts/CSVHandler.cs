using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class CSVHandler
{

    private string csvPath = "Assets/Resources/CSV/AnimData.csv";
    private static string[] csvLabels =
    {
        // General info
        "ClipName" /*[0]*/,         "Frame" /*[1]*/,

        // Pose data
        "RootVel.x" /*[2]*/,        "RootVel.z" /*[3]*/,
        "LFootVel.x" /*[4]*/,       "LFootVel.z" /*[5]*/,
        "RFootVel.x" /*[6]*/,       "RFootVel.z" /*[7]*/,

        // TrajectoryPoint data
        "RootPos.x" /*[8]*/,        "RootPos.z" /*[9]*/,
        "Forward.x" /*[10]*/,        "Forward.z"  /*[11]*/
    };
    public void WriteCSV(List<FeatureVector> dataToWrite)
    {
        MMPose[] poseData = new MMPose[dataToWrite.Count];
        TrajectoryPoint[] trajData = new TrajectoryPoint[dataToWrite.Count];

        for (int i = 0; i < dataToWrite.Count; i++)
        {
            poseData[i] = dataToWrite[i].GetPose();
            trajData[i] = dataToWrite[i].GetTrajectory().GetTrajectoryPoints()[0];
        }

        using (var file = File.CreateText(csvPath))
        {
            file.WriteLine(string.Join(",", csvLabels));

            // System language generalization
            string spec = "G";
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US");

            for (int i = 0; i < dataToWrite.Count; i++)
            {
                string[] tempLine =
                {
                    // General info
                    dataToWrite[i].GetClipName(), dataToWrite[i].GetFrame().ToString(spec, ci),
                    // Pose data
                    poseData[i].GetRootVelocity().x.ToString(spec, ci), poseData[i].GetRootVelocity().z.ToString(spec, ci),
                    poseData[i].GetLefFootVelocity().x.ToString(spec, ci), poseData[i].GetLefFootVelocity().z.ToString(spec, ci),
                    poseData[i].GetRightFootVelocity().x.ToString(spec, ci), poseData[i].GetRightFootVelocity().z.ToString(spec, ci), 
                    // TrajectoryPoint data
                    trajData[i].GetPoint().x.ToString(spec, ci), trajData[i].GetPoint().z.ToString(spec, ci),
                    trajData[i].GetForward().x.ToString(spec, ci), trajData[i].GetForward().z.ToString(spec, ci),
                };

                file.WriteLine(string.Join(",", csvLabels));
            }
        }
    }
    public List<FeatureVector> ReadCSV(int trajPointsLength, int trajStepSize)
    {
        StreamReader reader = new StreamReader(csvPath);

        bool endOfFile = false;
        bool ignoreHeaders = true;
        List<FeatureVector> featuresFromCSV = new List<FeatureVector>();
        int idIterator = 0;

        while (!endOfFile)
        {
            string dataString = reader.ReadLine();

            if (dataString == null)
            {
                endOfFile = true;
                break;
            }

            string[] tempString = dataString.Split(',');
            NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;

            if (!ignoreHeaders)
            {
                string tempClipNameValue = "";
                TrajectoryPoint[] trajPoints = new TrajectoryPoint[trajPointsLength];
                //featuresFromCSV.Add(new FeatureVector(new MMPose(new Vector3(float.Parse(tempString[2], format),0.0f,float.Parse(tempString[3], format)),
                //        new Vector3(float.Parse(tempString[4], format), 0.0f, float.Parse(tempString[5], format)), 
                //        new Vector3(float.Parse(tempString[6], format), 0.0f, float.Parse(tempString[7], format))), 
                //    new Trajectory(), 
                //    idIterator, tempString[0], tempString[1]));
                idIterator++;
            }
            else
                ignoreHeaders = false;
        }
        return featuresFromCSV;
    }
}