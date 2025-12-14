#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Dsu.Extension
{
    public static class AwesomeUnityOpenSourceMenu
    {
        private const string url_gp101 = "https://github.com/gp101";
        private const string url1 = "https://github.com/StefanoCecere/awesome-opensource-unity";
        private const string url2 = "https://github.com/baba-s/awesome-unity-open-source-on-github";
        private const string url3 = "https://github.com/jeffreylanters/awesome-unity-packages";
	    private const string url4 = "https://github.com/proyecto26/awesome-unity";
	    private const string url5 = "https://github.com/insthync/awesome-unity3d";

        [MenuItem("Tools/Dsu Tools/Link to ChatGPT", false, 150)]
        private static void OpenUrl_ChatGPT()
        {
            Application.OpenURL("https://chatgpt.com");
        }

        [MenuItem("Tools/Dsu Tools/Link to Gemini", false, 151)]
        private static void OpenUrl_Gemini()
        {
            Application.OpenURL("https://gemini.google.com/");
        }

        [MenuItem("Tools/Dsu Tools/Link to Google AI Studio", false, 152)]
        private static void OpenUrl_GoogleAiStudio()
        {
            Application.OpenURL("https://aistudio.google.com/");
        }

        [MenuItem("Tools/Dsu Tools/Link to github-gp101", false, 153)]
        private static void OpenUrl0()
        {
            Application.OpenURL(url_gp101);
        }

        [MenuItem("Tools/Dsu Tools/Link to awesome-opensource-unity", false, 154)]
        private static void OpenUrl1()
        {
            Application.OpenURL(url1);
        }

        [MenuItem("Tools/Dsu Tools/Link to awesome-unity-open-source-on-github", false, 155)]
        private static void OpenUrl2()
        {
            Application.OpenURL(url2);
        }

        [MenuItem("Tools/Dsu Tools/Link to awesome-unity-packages", false, 156)]
        private static void OpenUrl3()
        {
            Application.OpenURL(url3);
        }

	    [MenuItem("Tools/Dsu Tools/Link to awesome-unity-games", false, 157)]
	    private static void OpenUrl4()
	    {
		    Application.OpenURL(url4);
	    }
	    
	    [MenuItem("Tools/Dsu Tools/Link to awesome-unity3d", false, 158)]
	    private static void OpenUrl5()
	    {
		    Application.OpenURL(url5);
	    }
    }
}
#endif
