using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#pragma warning disable 0414
public class NativeShare
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _TAG_ShareSimpleText(string massege);

    public static void Share(string  massege)
	{
        _TAG_ShareSimpleText(massege);	
    }
#endif
}
#pragma warning restore 0414
