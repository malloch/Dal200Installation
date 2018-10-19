using System.Collections.Generic;
using System.Drawing;

namespace KinectV2EmguCV.Model.Tracking
{
    /// <summary>
    /// Interface used by all the strategies that want to implement
    /// top down tracking.
    /// </summary>
    public interface ITopDownTrackingStrategy
    {
        /// <summary>
        /// Detects a single (multiple may be present) person in a given frame
        /// </summary>
        /// <param name="source">Data source for the tracking strategy</param>
        /// <returns>The position of first the person it identifies</returns>
        PointF? DetectSinglePerson(ITrackableSource source);
        /// <summary>
        /// Detects multiple people in a given frame
        /// </summary>
        /// <param name="source">Data source for the tracking strategy</param>
        /// <returns>A dictionary of unique IDs for each person 
        /// and their respective positions</returns>
        Dictionary<int,PointF?> DetectMultiplePeople(ITrackableSource source);
    }
}