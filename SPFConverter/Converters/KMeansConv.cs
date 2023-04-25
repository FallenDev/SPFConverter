namespace SPFverter.Converters;

public class KMeans<T>
{
    public T[] Centroids { get; private set; }
    private readonly Func<T, T, double> _distanceFunc;
    private readonly int _maxIterations = 100;

    public KMeans(int numCentroids, Func<T, T, double> distanceFunc)
    {
        Centroids = new T[numCentroids];
        _distanceFunc = distanceFunc;
    }

    public void Run(IEnumerable<T> data)
    {
        Random random = new Random();
        var dataArray = data.ToArray();

        // Initialize centroids randomly
        for (int i = 0; i < Centroids.Length; i++)
        {
            Centroids[i] = dataArray[random.Next(dataArray.Length)];
        }

        // Iterate until convergence or max iterations
        for (int iteration = 0; iteration < _maxIterations; iteration++)
        {
            var groups = new List<T>[Centroids.Length];
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = new List<T>();
            }

            // Assign each data point to its nearest centroid
            foreach (var item in dataArray)
            {
                int nearestIndex = FindNearestCentroidIndex(item);
                groups[nearestIndex].Add(item);
            }

            // Update centroids
            bool changed = false;
            for (int i = 0; i < Centroids.Length; i++)
            {
                T newCentroid = CalculateMean(groups[i]);
                if (!EqualityComparer<T>.Default.Equals(Centroids[i], newCentroid))
                {
                    Centroids[i] = newCentroid;
                    changed = true;
                }
            }

            // If centroids haven't changed, the algorithm has converged
            if (!changed)
            {
                break;
            }
        }
    }

    public int FindNearestCentroidIndex(T item)
    {
        int nearestIndex = 0;
        double nearestDistance = double.MaxValue;

        for (int i = 0; i < Centroids.Length; i++)
        {
            double distance = _distanceFunc(Centroids[i], item);
            if (distance < nearestDistance)
            {
                nearestIndex = i;
                nearestDistance = distance;
            }
        }

        return nearestIndex;
    }

    private T CalculateMean(List<T> items)
    {
        if (typeof(T) == typeof(System.Drawing.Color))
        {
            return (T)(object)CalculateMeanColor(items.Cast<System.Drawing.Color>());
        }

        throw new NotSupportedException($"Mean calculation is not supported for type {typeof(T)}.");
    }

    private System.Drawing.Color CalculateMeanColor(IEnumerable<System.Drawing.Color> colors)
    {
        long rSum = 0;
        long gSum = 0;
        long bSum = 0;
        long count = 0;

        foreach (var color in colors)
        {
            rSum += color.R;
            gSum += color.G;
            bSum += color.B;
            count++;
        }

        if (count == 0)
        {
            return System.Drawing.Color.Empty;
        }

        return System.Drawing.Color.FromArgb((int)(rSum / count), (int)(gSum / count), (int)(bSum / count));
    }
}