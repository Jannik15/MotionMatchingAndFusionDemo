using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class CSVHandler
{
    private string csvPath = "Assets/Resources/CSV/AnimData.csv";
    private static string[] csvLabels =
    {
        // General info
        "ClipName", "Frame",
        // Pose data
        "RootVel.x", "RootVel.z",
        "LFootVel.x", "LFootVel.z",
        "RFootVel.x", "RFootVel.z",
        // TrajectoryPoint data
        "RootPos.x", "RootPos.z",
        "Forward.x", "Forward.z"
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
    public void ReadCSV()
    {
        StreamReader reader = new StreamReader(csvPath);

        bool endOfFile = false;
        bool createHeaders = true;

        while (!endOfFile)
        {
            string dataString = reader.ReadLine();

            if (dataString == null)
            {
                endOfFile = true;
                break;
            }
        }
    }
}