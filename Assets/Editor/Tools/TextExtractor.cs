using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class TextExtractor : EditorWindow
{
    private string outputFilePath = "Assets/Fonts/ExtractedText.txt";//文件生成路径
    [MenuItem("Tools/Text Extractor")]
    public static void ShowWindow()
    {
        GetWindow<TextExtractor>("Text Extractor");
    }
    private void OnGUI()
    {
        GUILayout.Label("Text Extractor Tool", EditorStyles.boldLabel);
        //设置输出文件路径
        outputFilePath = EditorGUILayout.TextField("Output File Path", outputFilePath);
        if (GUILayout.Button("Extract Text"))
        {
            ExtractText();
        }
    }
    private void ExtractText()
    {
        //获取场景中所有的Text组件和TextMeshPro组件
        var texts = new List<string>();
        //查找所有预制体(Prefab)
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                //提取预制体中的Text和TMP组件
                ExtractTextFromGameObject(prefab, texts);
            }
        }
        //查找所有的场景
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        foreach (var guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (sceneAsset != null)
            {
                //打开场景并提取
                ExtractTextFromScene(path, texts);
            }
        }
        //将所有内容拼接成为一个字符串
        string allText = string.Join("", texts);
        //清理文本,只保留汉字和全角标点符号
        string cleanedText = CleanText(allText);
        //去重处理
        string uniqueText = RemoveDuplicateCharacters(cleanedText);
        //保存文本到指定路径
        SaveTextToFile(uniqueText);
    }
    private void ExtractTextFromGameObject(GameObject go, List<string> texts)
    {
        //提取GameObject中的Text和TextMeshPro组件
        Text[] textComponents = go.GetComponentsInChildren<Text>(true);
        foreach (var text in textComponents)
        {
            texts.Add(text.text);
        }
        TextMeshProUGUI[] tmpComponents = go.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var tmp in tmpComponents)
        {
            texts.Add(tmp.text);
        }
    }
    private void ExtractTextFromScene(string scenePath, List<string> texts)
    {
        //加载场景
        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        if (scene != null)
        {
            //仅在编辑模式下打开场景，不保存更改
            if (EditorSceneManager.GetActiveScene().path != scenePath)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
            //查找场景中的所有GameObject
            GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var go in sceneObjects)
            {
                ExtractTextFromGameObject(go, texts);
            }
            //如果场景没有是当前场景，则在操作结束后卸载场景
            if (EditorSceneManager.GetActiveScene().path != scenePath)
            {
                EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(scenePath), true);
            }
        }
    }
    private string CleanText(string input)
    {
        //正则表达式,保留汉字和全角标点符号
        string pattern = @"[^\u4e00-\u9fa5\uff00-\uffef\“\”\‘\’\、\．\？\！\，\：\；\（\）\【\】\『\』\~]";//排除所有非汉字和非全角标点的字符并添加常见的全角符号
        string cleanedText = Regex.Replace(input, pattern, "");
        return cleanedText;
    }
    private string RemoveDuplicateCharacters(string input)
    {
        //使用HashSet去重
        HashSet<char> uniqueChars = new();
        List<char> result = new();
        foreach (char c in input)
        {
            //哈希表不能够存储相同的元素,因此如果有已经存在的元素想添加进入哈希表时,会返回False
            if (uniqueChars.Add(c))//如果字符没有出现过,添加到集合并加入结果列表
            {
                result.Add(c);
            }
        }
        //将去重后的字符列表转换为字符串
        return new string(result.ToArray());
    }
    private void SaveTextToFile(string text)
    {
        try
        {
            //检查文件路径的有效性
            string directory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            //将文本写入文件
            File.WriteAllText(outputFilePath, text);
            //强制刷新AssetDatabase,确保文件被Unity正确识别
            AssetDatabase.Refresh();
            Debug.Log($"文本写入成功：{outputFilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"文本写入错误：{ex.Message}");
        }
    }
}