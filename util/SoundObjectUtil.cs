using System.Linq;
using UnityEngine;

namespace PridePerception.util
{
    public class SoundObjectUtil
    {
        public static void logNames()
        {
            SoundObject[] soundObjects = SoundObjectUtil.getEvery();

            for (int i = 0; i < soundObjects.Length; i++)
                Debug.Log(soundObjects[i].name);
        }

        public static SoundObject[] getEvery()
        {
            return Resources.FindObjectsOfTypeAll<SoundObject>();
        }

        public static SoundObject fromName(string name)
        {
            return SoundObjectUtil.getEvery().First(soundObject => soundObject.name == name);
        }
    }
}