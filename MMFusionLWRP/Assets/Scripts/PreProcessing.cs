using System.Collections.Generic;
using UnityEngine;

public class PreProcessing
{
    private CSVHandler csvHandler;
    public void Preprocess(List<FeatureVector> featureVector)
    {
        csvHandler.WriteCSV(featureVector);
    }

    public List<FeatureVector> LoadData(int pointsPerTrajectory, int framesBetweenTrajectoryPoints)
    {
        csvHandler.ReadCSV(pointsPerTrajectory, framesBetweenTrajectoryPoints);
        List<FeatureVector> featureVector = new List<FeatureVector>();
        return featureVector;
    }
}
