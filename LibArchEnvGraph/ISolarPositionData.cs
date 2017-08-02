namespace LibArchEnvGraph
{
    /// <summary>
    /// 太陽高度および方位角
    /// </summary>
    public interface ISolarPositionData
    {
        /// <summary>
        /// 太陽高度角 [°] hn
        /// </summary>
        double SolarElevationAngle { get; }

        /// <summary>
        /// 太陽方位角 [°] An
        /// </summary>
        double SolarAzimuth { get; }
    }
}
