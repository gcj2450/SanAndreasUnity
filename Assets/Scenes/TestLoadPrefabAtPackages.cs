using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static SanAndreasUnity.Importing.RenderWareStream.TwoDEffect;
using UnityEngine.UIElements;

public class TestLoadPrefabAtPackages : MonoBehaviour
{
    public RawImage rimg;
    // Start is called before the first frame update
    void Start()
    {
        //string absolute = Path.GetFullPath("Packages/com.unity.images-library/Example/Images/image.png");

        Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath("Packages/com.baidu.statusbar.package/Sprites/icon_emoji_press.png", typeof(Texture2D));
        Debug.Log(texture == null);
        rimg.texture = texture;

        GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/com.baidu.statusbar.package/Prefabs/UIStatusBar.prefab", typeof(GameObject));

        GameObject goIns = Instantiate(go);
        goIns.transform.SetParent(rimg.transform.parent);
        RectTransform rectTransform= goIns.GetComponent<RectTransform>();

        //offsetMin ÊÇvector2(left, bottom);
        //offsetMax ÊÇvector2(right, top);
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
