using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class BMFontEditorNew : EditorWindow
{
    [MenuItem("Tools/BMFont Maker II")]
    static public void OpenBMFontMaker()
    {
        var window = EditorWindow.GetWindow<BMFontEditorNew>(false, "BMFontMakerII", true);
        window.Show();
        window.minSize = new Vector2(450, 450);

        window.Init();
    }

    [SerializeField]
    private Font targetFont;
    [SerializeField]
    private TextAsset fntData;
    [SerializeField]
    private Material fontMaterial;
    [SerializeField]
    private Texture2D fontTexture;

    private List<Texture2D> fontTextureList;
    private List<string> fontTextList;
    private List<string> fontAascIIList;
    private string strFontName;
    private string strCustomChar;

    private BMFont bmFont = new BMFont();

    public BMFontEditorNew()
    {

    }

    public void Init()
    {
        fontTextureList = new List<Texture2D>();
        fontTextList = new List<string>();
        fontAascIIList = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            AddItem();
        }
    }

    public void AddItem()
    {
        int index = fontTextureList.Count;

        fontTextureList.Add(null);
        byte[] array = new byte[1];   //定义一组数组array
        array = System.Text.Encoding.ASCII.GetBytes(index.ToString()); //string转换的字母
        int asciicode = (short)(array[0]);
        fontAascIIList.Add(System.Convert.ToString(asciicode)); //将转换一的ASCII码转换成string型
        fontTextList.Add(index.ToString());
    }

    public void AddItem(string ch)
    {
        fontTextureList.Add(null);
        byte[] array = new byte[1];   //定义一组数组array
        array = System.Text.Encoding.ASCII.GetBytes(ch); //string转换的字母
        int asciicode = (short)(array[0]);
        fontAascIIList.Add(System.Convert.ToString(asciicode)); //将转换一的ASCII码转换成string型
        fontTextList.Add(ch);
    }

    private static void processCommand(string command, string argument)
    {
        System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo(command);
        start.Arguments = argument;
        start.CreateNoWindow = true;
        start.ErrorDialog = true;

        try
        {
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(start);
            p.StartInfo.RedirectStandardError = false;
            p.WaitForExit();
            p.Close();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    void OnGUI()
    {
        #region 生成BMFont资源
        int startChar = -1;
        int endChar = -1;
        
        //----------------
        EditorGUILayout.LabelField("输入Font的名称：");
        strFontName = EditorGUILayout.TextField(strFontName);
        EditorGUILayout.Space();
        //----------------
        EditorGUILayout.LabelField("输入对应数字图片资源：（可为空）");
        EditorGUIUtility.labelWidth = 30;
        int col = 2;
        int curCol = 0;
        int index = 0;
        for (index = 0; index < fontTextureList.Count; index++)
        {   
            if(curCol==0)
                EditorGUILayout.BeginHorizontal();
            fontTextureList[index] =
                EditorGUILayout.ObjectField(fontTextList[index], fontTextureList[index], typeof(Texture2D), false, GUILayout.Width(200)) as Texture2D;
                curCol ++;
            if (curCol == col)
            {
                EditorGUILayout.EndHorizontal();
                curCol = 0;
            }
        }
        //----------------
        EditorGUILayout.LabelField("输入自定义的字符：");
        EditorGUILayout.BeginHorizontal();
        strCustomChar = EditorGUILayout.TextField(strCustomChar);
        if (GUILayout.Button("Add One"))
        {
            AddItem(strCustomChar);
        }
        EditorGUILayout.EndHorizontal();
        //----------------

        if (GUILayout.Button("Create BMFont"))
        {
            for (int i = 0; i < fontTextureList.Count; i++)
            {
                var tex = fontTextureList[i];
                if (tex != null)
                {
                    if (startChar == -1)
                        startChar = i;
                    endChar = i;
                }
            }

            string toolDir = string.Format("{0}/../../Tools/BMFont", Application.dataPath);
            string textConf = string.Format("{0}/text.txt", toolDir);
            string bmfc = string.Format("{0}/config.bmfc", toolDir);
            string basebmfc = string.Format("{0}/baseconfig.bmfc", toolDir);

            try
            {
                // 0.创建bmfc
                {
                    if (System.IO.File.Exists(bmfc))
                        System.IO.File.Delete(bmfc);

                    System.IO.File.Copy(basebmfc, bmfc);
                    using (System.IO.StreamWriter fileWriter = System.IO.File.AppendText(bmfc))
                    {
                        fileWriter.WriteLine("\n");
                        fileWriter.WriteLine(string.Format("chars={0}-{1}", fontAascIIList[startChar], fontAascIIList[endChar]));
                        for (int i = startChar; i < fontAascIIList.Count && i <= endChar; i++)
                        {
                            Texture2D fontTexture2 = fontTextureList[i];
                            var texPath = AssetDatabase.GetAssetPath(fontTexture2);
                            texPath = string.Format("{0}{1}", Application.dataPath, texPath.Replace("Assets", ""));
                            Debug.Log(texPath);
                            fileWriter.WriteLine(string.Format("icon=\"{0}\",{1},0,0,0", texPath, fontAascIIList[i]));
                        }
                        fileWriter.Close();
                    }
                }

                // 1.创建text
                {
                    if (System.IO.File.Exists(textConf))
                        System.IO.File.Delete(textConf);

                    using (System.IO.StreamWriter fileWriter = System.IO.File.CreateText(textConf))
                    {
                        for (int i = startChar; i < fontTextList.Count && i <= endChar; i++)
                        {
                            fileWriter.Write(i.ToString());
                        }
                        fileWriter.Close();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("Create Font Error:{0}", e.Message);
            }
            finally
            {

            }
            string commandText = string.Format(" -t \"{0}\" -c \"{1}\" -o \"{2}/font.fnt\"", textConf, bmfc, toolDir);
            Debug.LogFormat("commandText:{0}", commandText);
            processCommand(
                string.Format("{0}/../../Tools/{1}", Application.dataPath, "BMFont/bmfont.exe"),
                string.Format(commandText));
            #endregion

        #region Copy目标文件

            string destDir = string.Format("{0}/{1}", Application.dataPath, "Font");
            string fontTextureName = string.Format("{0}/FontConfig/{1}/{1}.png", destDir, strFontName);
            string fntFileName = string.Format("{0}/FontConfig/{1}/{1}.fnt", destDir, strFontName);
            if (!System.IO.Directory.Exists(string.Format("{0}/FontConfig/{1}", destDir, strFontName)))
            {
                System.IO.Directory.CreateDirectory(string.Format("{0}/FontConfig/{1}", destDir, strFontName));
                AssetDatabase.Refresh();
            }
            if (System.IO.File.Exists(fntFileName))
                System.IO.File.Delete(fntFileName);
            System.IO.File.Copy(string.Format("{0}/font.fnt", toolDir), fntFileName);
            System.IO.File.Delete(string.Format("{0}/font.fnt", toolDir));

            if (System.IO.File.Exists(fontTextureName))
                System.IO.File.Delete(fontTextureName);
            System.IO.File.Copy(string.Format("{0}/font_0.png", toolDir), fontTextureName);
            System.IO.File.Delete(string.Format("{0}/font_0.png", toolDir));

            AssetDatabase.Refresh();

            fontTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fontTextureName.Replace(Application.dataPath, "Assets"));
            TextureImporter asetImp = TextureImporter.GetAtPath(fontTextureName.Replace(Application.dataPath, "Assets")) as TextureImporter;
            asetImp.textureType = TextureImporterType.GUI;
            asetImp.SaveAndReimport();

            fntData = AssetDatabase.LoadAssetAtPath<TextAsset>(fntFileName.Replace(Application.dataPath, "Assets"));
            #endregion

        #region 生成Custom Font
            // create fontsettings file.
            string targetFontPath = string.Format("Assets/Font/{0}.fontsettings", strFontName);
            if (AssetDatabase.LoadAssetAtPath<Font>(targetFontPath) != null)
            {
                AssetDatabase.DeleteAsset(targetFontPath);
            }
            targetFont = new Font(strFontName);
            AssetDatabase.CreateAsset(targetFont, targetFontPath);

            // create material file.
            string targetmatPath = string.Format("Assets/Font/FontConfig/{0}/{0}.mat", strFontName);
            if (AssetDatabase.LoadAssetAtPath<Font>(targetmatPath) != null)
            {
                AssetDatabase.DeleteAsset(targetmatPath);
            }
            fontMaterial = new Material(Shader.Find("UI/Default"));
            AssetDatabase.CreateAsset(fontMaterial, targetmatPath);

            BMFontReader.Load(bmFont, fntData.name, fntData.bytes); // 借用NGUI封装的读取类
            CharacterInfo[] characterInfo = new CharacterInfo[bmFont.glyphs.Count];
            for (int i = 0; i < bmFont.glyphs.Count; i++)
            {
                BMGlyph bmInfo = bmFont.glyphs[i];
                CharacterInfo info = new CharacterInfo();
                info.index = bmInfo.index;
                info.uv.x = (float)bmInfo.x / (float)bmFont.texWidth;
                info.uv.y = 1 - (float)bmInfo.y / (float)bmFont.texHeight;
                info.uv.width = (float)bmInfo.width / (float)bmFont.texWidth;
                info.uv.height = -1f * (float)bmInfo.height / (float)bmFont.texHeight;
                info.vert.x = 0;
                info.vert.y = -(float)bmInfo.height;
                info.vert.width = (float)bmInfo.width;
                info.vert.height = (float)bmInfo.height;
                info.width = (float)bmInfo.advance;
                characterInfo[i] = info;
            }
            targetFont.characterInfo = characterInfo;
            if (fontMaterial)
            {
                fontMaterial.mainTexture = fontTexture;
            }
            targetFont.material = fontMaterial;
            fontMaterial.shader = Shader.Find("UI/Default");//这一行很关键，如果用standard的shader，放到Android手机上，第一次加载会很慢

            Debug.Log("create font <" + targetFont.name + "> success");
            //Close();
        }
        #endregion
    }
}