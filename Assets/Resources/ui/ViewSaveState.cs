using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UnitySnes
{
    public class ViewSaveState : Views
    {
        public RectTransform ItemPrefab;
        public RectTransform Container;
        public RectTransform NewItem;
        public RectTransform[] ExistItems;
        public int ItemHeight;
        public int Spacing;
        
        protected override void Start()
        {
            base.Start();

            var saves = Frontend.GetStateFilePaths();
            var list = new List<RectTransform>();
            foreach (var save in saves)
            {
                var obj = Instantiate(ItemPrefab, Container);
                obj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    Frontend.LoadState(save);
                    Frontend.OnMenuOpen("");
                });
                obj.GetComponentInChildren<Text>().text = $"{Path.GetFileNameWithoutExtension(save)}";
                list.Add(obj);
            }

            ExistItems = list.ToArray();
            NewItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                Frontend.SaveState();
                Frontend.OnMenuOpen("");
            });
            
            _Align();
        }

        public void OnTouchBack()
        {
            Frontend.OnMenuOpen("ui/menus");
        }
        
        private void _Align()
        {
            const float startX = 130f;
            var startY = -25f;
            
            NewItem.localPosition = new Vector3(startX, startY);
            for (var i = 0; i < ExistItems.Length; i++)
            {
                var existItem = ExistItems[i];
                startY -= ItemHeight + Spacing;
                existItem.localPosition = new Vector3(startX, startY);                
            }

            Container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ItemHeight * (ExistItems.Length + 1) + Spacing * ExistItems.Length);
        }
    }
}