using System.Numerics;

namespace SpfConverter.Utility;

public static class MathEx
{
        /// <summary>
    ///     Scales a number from one range to another range.
    /// </summary>
    /// <param name="num">The input number to be scaled.</param>
    /// <param name="min">The lower bound of the original range.</param>
    /// <param name="max">The upper bound of the original range.</param>
    /// <param name="newMin">The lower bound of the new range.</param>
    /// <param name="newMax">The upper bound of the new range.</param>
    /// <returns>The scaled number in the new range.</returns>
    /// <remarks>
    ///     This method assumes that the input number is within the original range.
    ///     No clamping or checking is performed.
    /// </remarks>
    public static double ScaleRange(
        double num,
        double min,
        double max,
        double newMin,
        double newMax
    ) => (newMax - newMin) * (num - min) / (max - min) + newMin;

    /// <summary>
    ///     Scales a number from one range to another range.
    /// </summary>
    /// <param name="num">The input number to be scaled.</param>
    /// <param name="min">The lower bound of the original range.</param>
    /// <param name="max">The upper bound of the original range.</param>
    /// <param name="newMin">The lower bound of the new range.</param>
    /// <param name="newMax">The upper bound of the new range.</param>
    /// <returns>The scaled number in the new range.</returns>
    /// <remarks>
    ///     This method assumes that the input number is within the original range.
    ///     No clamping or checking is performed.
    /// </remarks>
    public static T2 ScaleRange<T1, T2>(
        T1 num,
        T1 min,
        T1 max,
        T2 newMin,
        T2 newMax
    ) where T1: INumber<T1>
      where T2: INumber<T2> =>
        T2.CreateTruncating(
            ScaleRange(
                double.CreateTruncating(num),
                double.CreateTruncating(min),
                double.CreateTruncating(max),
                double.CreateTruncating(newMin),
                double.CreateTruncating(newMax)));
}