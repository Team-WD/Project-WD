#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

public static class CSVtoScriptableObject
{
    #region Encoding

    public enum CSVEncoding
    {
        UTF8,
        UTF8NoBOM,
        Unicode,
        ANSI
    }

    private static Encoding GetEncoding(CSVEncoding encodingType)
    {
        switch (encodingType)
        {
            case CSVEncoding.UTF8:
                return new UTF8Encoding(true);
            case CSVEncoding.UTF8NoBOM:
                return new UTF8Encoding(false);
            case CSVEncoding.Unicode:
                return Encoding.Unicode;
            case CSVEncoding.ANSI:
                return Encoding.Default;
            default:
                return Encoding.UTF8;
        }
    }

    #endregion

    private static string[] ReadCSVFile(string csvFilePath, CSVEncoding encodingType)
    {
        Encoding encoding = GetEncoding(encodingType);
        return File.ReadAllLines(csvFilePath, encoding);
    }

    #region PlayerData

    public static void ConvertCSVtoPlayerData(string csvFilePath, CSVEncoding encodingType = CSVEncoding.UTF8)
    {
        List<PlayerData> playerDataList = new List<PlayerData>();
        Dictionary<string, int> headerIndices = new Dictionary<string, int>();

        try
        {
            string[] lines = ReadCSVFile(csvFilePath, encodingType);
            string[] headers = lines[0].Split(',');

            for (int i = 0; i < headers.Length; i++)
            {
                headerIndices[headers[i].ToLower()] = i;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                PlayerData playerData = ScriptableObject.CreateInstance<PlayerData>();

                if (headerIndices.TryGetValue("id", out int idIndex))
                    playerData.Id = ParseInt(values[idIndex]);

                if (headerIndices.TryGetValue("name", out int nameIndex))
                    playerData.Name = values[nameIndex];

                if (headerIndices.TryGetValue("level", out int levelIndex))
                    playerData.Level = ParseInt(values[levelIndex]);

                if (headerIndices.TryGetValue("exp", out int expIndex))
                    playerData.Exp = ParseInt(values[expIndex]);

                if (headerIndices.TryGetValue("maxhp", out int maxHpIndex))
                    playerData.MaxHp = ParseInt(values[maxHpIndex]);

                if (headerIndices.TryGetValue("heal", out int healIndex))
                    playerData.Heal = ParseInt(values[healIndex]);

                if (headerIndices.TryGetValue("armor", out int armorIndex))
                    playerData.Armor = ParseInt(values[armorIndex]);

                if (headerIndices.TryGetValue("speed", out int speedIndex))
                    playerData.Speed = ParseFloat(values[speedIndex]);

                playerDataList.Add(playerData);
            }

            foreach (var playerData in playerDataList)
            {
                string assetPath = $"Assets/Resources/PlayerData/Player_{playerData.Id}.asset";
                AssetDatabase.CreateAsset(playerData, assetPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully imported {playerDataList.Count} player data entries.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error importing player data: {e.Message}");
        }
    }

    #endregion

    #region EnemyData

    public static void ConvertCSVtoEnemyData(string csvFilePath, CSVEncoding encodingType = CSVEncoding.UTF8)
    {
        List<EnemyData> enemyDataList = new List<EnemyData>();
        Dictionary<string, int> headerIndices = new Dictionary<string, int>();

        try
        {
            string[] lines = ReadCSVFile(csvFilePath, encodingType);
            string[] headers = lines[0].Split(',');

            for (int i = 0; i < headers.Length; i++)
            {
                headerIndices[headers[i].ToLower()] = i;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                EnemyData enemyData = ScriptableObject.CreateInstance<EnemyData>();

                if (headerIndices.TryGetValue("id", out int idIndex))
                    enemyData.Id = ParseInt(values[idIndex]);

                if (headerIndices.TryGetValue("name", out int nameIndex))
                    enemyData.Name = values[nameIndex];

                if (headerIndices.TryGetValue("maxhp", out int maxHpIndex))
                    enemyData.MaxHp = ParseInt(values[maxHpIndex]);

                if (headerIndices.TryGetValue("damage", out int damageIndex))
                    enemyData.Damage = ParseInt(values[damageIndex]);

                if (headerIndices.TryGetValue("speed", out int speedIndex))
                    enemyData.Speed = ParseFloat(values[speedIndex]);

                if (headerIndices.TryGetValue("armor", out int armorIndex))
                    enemyData.Armor = ParseInt(values[armorIndex]);

                if (headerIndices.TryGetValue("isranged", out int isRangedIndex))
                    enemyData.IsRanged = ParseBool(values[isRangedIndex]);

                if (headerIndices.TryGetValue("attackspeed", out int attackSpeedIndex))
                    enemyData.AttackSpeed = ParseFloat(values[attackSpeedIndex]);

                if (headerIndices.TryGetValue("attackrange", out int attackRangeIndex))
                    enemyData.AttackRange = ParseFloat(values[attackRangeIndex]);

                if (headerIndices.TryGetValue("expdrop", out int expDropIndex))
                    enemyData.ExpDrop = ParseInt(values[expDropIndex]);

                enemyDataList.Add(enemyData);
            }

            foreach (var enemyData in enemyDataList)
            {
                string assetPath = $"Assets/Resources/EnemyData/Enemy_{enemyData.Id}.asset";
                AssetDatabase.CreateAsset(enemyData, assetPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully imported {enemyDataList.Count} enemy data entries.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error importing enemy data: {e.Message}");
        }
    }

    #endregion

    #region WeaponData

    public static void ConvertCSVtoWeaponData(string csvFilePath, CSVEncoding encodingType = CSVEncoding.UTF8)
    {
        List<WeaponData> weaponDataList = new List<WeaponData>();
        Dictionary<string, int> headerIndices = new Dictionary<string, int>();

        try
        {
            string[] lines = ReadCSVFile(csvFilePath, encodingType);
            string[] headers = lines[0].Split(',');

            for (int i = 0; i < headers.Length; i++)
            {
                headerIndices[headers[i].ToLower()] = i;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                WeaponData weaponData = ScriptableObject.CreateInstance<WeaponData>();

                if (headerIndices.TryGetValue("id", out int idIndex))
                    weaponData.Id = ParseInt(values[idIndex]);

                if (headerIndices.TryGetValue("name", out int nameIndex))
                    weaponData.Name = values[nameIndex];

                if (headerIndices.TryGetValue("ammo", out int ammoIndex))
                    weaponData.Ammo = ParseInt(values[ammoIndex]);

                if (headerIndices.TryGetValue("damage", out int damageIndex))
                    weaponData.Damage = ParseFloat(values[damageIndex]);

                if (headerIndices.TryGetValue("criticalrate", out int criticalRateIndex))
                    weaponData.CriticalRate = ParseFloat(values[criticalRateIndex]);

                if (headerIndices.TryGetValue("criticaldamage", out int criticalDamageIndex))
                    weaponData.CriticalDamage = ParseFloat(values[criticalDamageIndex]);

                if (headerIndices.TryGetValue("firerate", out int fireRateIndex))
                    weaponData.FireRate = ParseFloat(values[fireRateIndex]);

                if (headerIndices.TryGetValue("reloadspeed", out int reloadSpeedIndex))
                    weaponData.ReloadSpeed = ParseFloat(values[reloadSpeedIndex]);

                if (headerIndices.TryGetValue("penetratingpower", out int penetratingPowerIndex))
                    weaponData.PenetratingPower = ParseFloat(values[penetratingPowerIndex]);

                if (headerIndices.TryGetValue("range", out int rangeIndex))
                    weaponData.Range = ParseFloat(values[rangeIndex]);
                
                if (headerIndices.TryGetValue("spread", out int spreadIndex))
                    weaponData.Spread = ParseFloat(values[spreadIndex]);

                if (headerIndices.TryGetValue("tier", out int tierIndex))
                    weaponData.Tier = ParseInt(values[tierIndex]);

                weaponDataList.Add(weaponData);
            }

            foreach (var weaponData in weaponDataList)
            {
                string assetPath = $"Assets/Resources/WeaponData/Weapon_{weaponData.Id}.asset";
                AssetDatabase.CreateAsset(weaponData, assetPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully imported {weaponDataList.Count} weapon data entries.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error importing weapon data: {e.Message}");
        }
    }

    #endregion

    #region BulletData

    public static void ConvertCSVtoBulletData(string csvFilePath, CSVEncoding encodingType = CSVEncoding.UTF8)
    {
        List<BulletData> bulletDataList = new List<BulletData>();
        Dictionary<string, int> headerIndices = new Dictionary<string, int>();

        try
        {
            string[] lines = ReadCSVFile(csvFilePath, encodingType);
            string[] headers = lines[0].Split(',');

            for (int i = 0; i < headers.Length; i++)
            {
                headerIndices[headers[i].ToLower()] = i;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                BulletData bulletData = ScriptableObject.CreateInstance<BulletData>();

                if (headerIndices.TryGetValue("id", out int idIndex))
                    bulletData.Id = ParseInt(values[idIndex]);

                if (headerIndices.TryGetValue("name", out int nameIndex))
                    bulletData.Name = values[nameIndex];

                if (headerIndices.TryGetValue("damagemultiplier", out int damageMultiplierIndex))
                    bulletData.DamageMultiplier = ParseFloat(values[damageMultiplierIndex]);

                if (headerIndices.TryGetValue("shootspeed", out int shootSpeedIndex))
                    bulletData.ShootSpeed = ParseFloat(values[shootSpeedIndex]);

                bulletDataList.Add(bulletData);
            }

            foreach (var bulletData in bulletDataList)
            {
                string assetPath = $"Assets/Resources/BulletData/Bullet_{bulletData.Id}.asset";
                AssetDatabase.CreateAsset(bulletData, assetPath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully imported {bulletDataList.Count} bullet data entries.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error importing bullet data: {e.Message}");
        }
    }

    #endregion

    #region LevelUpData

    public static void ConvertCSVtoLevelUpData(string csvFilePath, CSVEncoding encodingType = CSVEncoding.UTF8)
    {
        List<LevelUpData> levelUpDataList = new List<LevelUpData>();
        Dictionary<string, int> headerIndices = new Dictionary<string, int>();

        try
        {
            string[] lines = ReadCSVFile(csvFilePath, encodingType);
            string[] headers = lines[0].Split(',');

            for (int i = 0; i < headers.Length; i++)
            {
                headerIndices[headers[i].ToLower()] = i;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                LevelUpData levelUpData = ScriptableObject.CreateInstance<LevelUpData>();

                if (headerIndices.TryGetValue("id", out int idIndex))
                {
                    levelUpData.id = int.Parse(values[idIndex]);
                }

                // if (headerIndices.TryGetValue("icon", out int iconPathIndex))
                // {
                //     levelUpData.icon = values[iconPathIndex];
                // }

                if (headerIndices.TryGetValue("flavorText", out int flavorTextIndex))
                {
                    levelUpData.flavorText = values[flavorTextIndex];
                }

                if (headerIndices.TryGetValue("type", out int typeIndex))
                {
                    levelUpData.type = values[typeIndex];
                }
                
                if (headerIndices.TryGetValue("value", out int valueIndex))
                {
                    levelUpData.value = float.Parse(values[valueIndex]);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error importing level up data: " + e.Message);
        }
    }

    #endregion
    
    #region Parse

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, out int result))
        {
            return result;
        }

        throw new FormatException($"Unable to parse '{value}' as an integer.");
    }

    private static float ParseFloat(string value)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }

        throw new FormatException($"Unable to parse '{value}' as a float.");
    }

    private static bool ParseBool(string value)
    {
        if (bool.TryParse(value, out bool result))
        {
            return result;
        }

        throw new FormatException($"Unable to parse '{value}' as a boolean.");
    }

    #endregion
}

#endif