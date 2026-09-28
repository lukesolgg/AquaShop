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
    public class TankSave
    {
        public string id;
        public string displayName;
        public int segment;
        public int capacity;
        public List<FishSave> fish = new List<FishSave>();
    }

    [Serializable]
    public class OrderSave
    {
        public string id;
        public string speciesId;
        public int qty;
        public int qtyFilled;
        public float payout;
        public float penalty;
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
        public int level = 1;
        public List<TankSave> tanks = new List<TankSave>();
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