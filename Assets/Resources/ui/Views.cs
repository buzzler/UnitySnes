using UnityEngine;

namespace UnitySnes
{
    public class Views : MonoBehaviour
    {
        protected Frontend Frontend;
        protected object[] Args;

        protected virtual void Start()
        {
            Frontend = FindObjectOfType<Frontend>();
        }

        public void SetArguments(params object[] args)
        {
            Args = args;
        }
    }
}