using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class LoadProcessStarter : MonoBehaviour
    {
        void Start()
        {
            //主逻辑入口
            Loader.StartLoading();
        }
    }
}
