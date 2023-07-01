using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Bark.Extensions;

namespace Bark.Tools
{
    public static class TextureRipper
    {
        public static string folderName = "C:\\Users\\ultra\\Pictures\\Gorilla Tag Textures";

        public static void Rip()
        {

            string step = "Start";
            Directory.CreateDirectory(folderName);
            try
            {
                step = "Locating renderers";
                Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
                Logging.Debug("Found", renderers.Length, "renderers");
                step = "Looping through renderers";
                List<Texture> knownTextures = new List<Texture>();
                foreach (Renderer renderer in renderers)
                {
                    step = "Formatting file path";
                    step = "Storing materials";
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        try
                        {
                            step = "Getting main texutre";
                            Texture texture = materials[i].mainTexture;
                            if (texture != null && !knownTextures.Contains(texture))
                            {
                                knownTextures.Add(texture);
                                step = "Creating directory";
                                step = "Encoding to png";
                                byte[] bytes = (texture as Texture2D).Copy().EncodeToPNG();
                                step = "Getting material name";
                                string materialName = materials[i].name;
                                step = "Getting file name";
                                string filename = Path.Combine(folderName, renderer.gameObject.name + "--" + materialName + ".png");
                                step = "Writing bytes";
                                if (filename.Contains("plastickey")) continue;
                                Logging.Debug(filename, bytes);
                                File.WriteAllBytes(filename, bytes);
                            }
                        }
                        catch (Exception e)
                        {
                            Logging.Exception(e);
                        }
                    }
                }
            }
            catch (Exception e) { Logging.LogWarning("Failed at step", step); Logging.Exception(e); }
        }
    }
}
