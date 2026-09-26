using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AquariumShop
{
    [Serializable]
    public class FishSave
    {
        public string speciesId;
        public float hunger;
    }

    [Serializable]
    public class OrderSave
    {
        public string id;
        public string speciesId;
        public float payout;
        public float hoursRemaining;
        public int status;
    }

    [Serializable]
    public class SaveData
    {
        public float money;
        public int foodUnits;
        public float gameHours;
        public int hoursUntilNextOrder;
        public int orderSerial;
        public bool won;
        public bool lost;
        public List<FishSave> fish = new List<FishSave>();
        public List<OrderSave> orders = new List<OrderSave>();
    }

    public static class SaveSystem
    {
        static string Path => System.IO.Path.Combine(Application.persistentDataPath, "shop.json");

        public static bool Exists => File.Exists(Path);

        public static void Write(SaveData data)
        {
            File.WriteAllText(Path, JsonUtility.ToJson(data, true));
            Debug.Log($"Saved {Path}");
        }

        public static SaveData Read()
        {
            if (!Exists) return null;
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
        }

        public static void Delete()
        {
            if (Exists) File.Delete(Path);
        }
    }
}