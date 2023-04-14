using System;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace Bark
{
    public static class AssetUtils
    {

        private static string FormatPath(string path)
        {
            return path.Replace("/", ".").Replace("\\", ".");
        }

        public static byte[] ExtractEmbeddedResource(string filePath)
        {
            filePath = FormatPath(filePath);
            var baseAssembly = Assembly.GetCallingAssembly();
            using (Stream resFilestream = baseAssembly.GetManifestResourceStream(filePath))
            {
                if (resFilestream == null)
                {
                    return null;
                }
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        /// <summary>
        /// Converts an embedded resource to a Texture2D object
        /// </summary>
        public static Texture2D GetTextureFromResource(string resourceName)
        {
            string file = resourceName;
            byte[] bytes = ExtractEmbeddedResource(file);
            if (bytes == null)
            {
                Console.WriteLine("No bytes found in " + file);
                return null;
            }
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.LoadImage(bytes);

            string name = file.Substring(0, file.LastIndexOf('.'));
            if (name.LastIndexOf('.') >= 0)
            {
                name = name.Substring(name.LastIndexOf('.') + 1);
            }
            texture.name = name;

            return texture;
        }

        public static AssetBundle LoadAssetBundle(string path) // Or whatever you want to call it as
        {
            path = FormatPath(path);
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            AssetBundle bundle = AssetBundle.LoadFromStream(stream);
            stream.Close();
            return bundle;
        }

        /// <summary>
        /// Returns a list of the names of all embedded resources
        /// </summary>
        public static string[] GetResourceNames()
        {
            var baseAssembly = Assembly.GetCallingAssembly();
            string[] names = baseAssembly.GetManifestResourceNames();
            if (names == null)
            {
                Console.WriteLine("No manifest resources found.");
                return null;
            }
            return names;
        }

        public static Sprite LoadSprite(string path)
        {
            Texture2D texture = GetTextureFromResource(path);
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
        }
    }
}