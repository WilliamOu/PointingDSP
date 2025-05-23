using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace CybSDK
{
    public class CMSAConfigReader : MonoBehaviour
    {
        public const string FilePath = "Cyberith_Config.ini";

        static string LoadValue(string identifier)
        {
            try
            {
                string fullpath = Path.GetFullPath(FilePath);

                if (!File.Exists(fullpath))
                    return null;

                using (FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (sr.Peek() != -1)
                        {
                            string line = sr.ReadLine();

                            if (!line.StartsWith(identifier))
                                continue;

                            int index = line.IndexOf('=');
                            if (index == -1 || index == line.Length - 1)
                                return null;

                            return line.Substring(index + 1).Trim();
                        }

                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static double LoadValue(string identifier, double defaultValue)
        {
            string strValue = LoadValue(identifier);

            if (strValue == null)
                return defaultValue;

            double value;
            return (!double.TryParse(strValue, NumberStyles.Float, CultureInfo.CurrentCulture, out value)) ? defaultValue : value;
        }

        public static int LoadValue(string identifier, int defaultValue)
        {
            string strValue = LoadValue(identifier);

            if (strValue == null)
                return defaultValue;

            int value;
            return (!int.TryParse(strValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out value)) ? defaultValue : value;
        }

        public static bool LoadValue(string identifier, bool defaultValue)
        {
            string strValue = LoadValue(identifier);

            if (strValue == null)
                return defaultValue;

            bool value;

            switch (strValue)
            {
                case "1": return true;
                case "0": return false;
                default:
                    return (!bool.TryParse(strValue, out value)) ? defaultValue : value;
            }
        }

        public static string LoadValue(string identifier, string defaultValue)
        {
            string strValue = LoadValue(identifier);

            return strValue ?? defaultValue;
        }

        public static CMotionSicknessAid.VisualEffect LoadValue(string identifier, CMotionSicknessAid.VisualEffect defaultValue)
        {
            string strValue = LoadValue(identifier);

            if (strValue == null)
                return defaultValue;

            CMotionSicknessAid.VisualEffect value;
            return Enum.TryParse(strValue, out value) ? value : defaultValue;
        }

        public static CMotionSicknessAid.EffectIntensity LoadValue(string identifier, CMotionSicknessAid.EffectIntensity defaultValue)
        {
            string strValue = LoadValue(identifier);

            if (strValue == null)
                return defaultValue;

            CMotionSicknessAid.EffectIntensity value;
            return Enum.TryParse(strValue, out value) ? value : defaultValue;
        }
    }
}
