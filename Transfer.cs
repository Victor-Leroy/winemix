using System.Collections.Generic;

namespace Blender;
public class Transfer
{
    public TankList From { get; }
    public TankList To { get; }

    public Transfer(TankList from, TankList to)
    {
        From = from;
        To = to;
    }
}




