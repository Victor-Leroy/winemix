using System.Collections.Generic;

namespace Blender;
public class TankList
{
    public int Volume { get; }
    public IReadOnlyList<int> Tanks { get; }

    public int Count => Tanks.Count;
    public int this[int i] => Tanks[i];
    public int Last => Count == 0 ? -1 : Tanks[Count - 1];

    public TankList(int volume, params int[] tanks) => (Volume, Tanks) = (volume, tanks);
    public TankList(int volume, List<int> tanks) => (Volume, Tanks) = (volume, tanks);

    public bool HasTank(int n) => Tanks.Contains(n);

    public bool IsValid()
    {
        if (Count == 0)
            return true;

        int previousTank = Tanks[0];
        for (int i = 1; i < Count; i++)
        {
            int currentTank = Tanks[i];
            if (currentTank <= previousTank)
                return false;
            previousTank = currentTank;
        }

        return true;
    }
}
