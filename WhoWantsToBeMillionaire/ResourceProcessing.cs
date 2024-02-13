using System.Drawing;
using System.IO;
using System.Reflection;

namespace WhoWantsToBeMillionaire
{
    enum TypeResource
    {
        AnimationData,
        Dialogues,
        Questions,
        Sounds,
        Sums,
        Textures
    }

    static class ResourceProcessing
    {
        public static Stream GetStream(string resourceName, TypeResource type)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"WhoWantsToBeMillionaire.Resources.{type}.{resourceName}");
        }

        public static Image GetImage(string resourceName)
        {
            using (Stream stream = GetStream(resourceName, TypeResource.Textures))
                return Image.FromStream(stream);
        }

        public static string GetString(string fileName)
        {
            using (StreamReader stream = new StreamReader(GetStream(fileName, TypeResource.Dialogues)))
                return stream.ReadToEnd();
        }
    }
}
