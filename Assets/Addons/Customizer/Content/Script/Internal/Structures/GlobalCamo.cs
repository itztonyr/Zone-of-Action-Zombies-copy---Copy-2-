using MFPS.Core;
using System;

[Serializable]
public class GlobalCamo : MFPSGameItem
{
    public int Price
    {
        get => Unlockability.Price;
    }

    public bool isFree() { return Price <= 0; }
}