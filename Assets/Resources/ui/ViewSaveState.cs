using System;
using System.Collections.Generic;
using System.Globalization;
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
            _Refresh();
        }

        public void OnTouchBack()
        {
            Frontend.OnMenuOpen("ui/menus");
        }

        private void _Refresh()
        {
            _Genrate();
            _Align();
        }
        
        private void _Genrate()
        {
            if (ExistItems != null)
            {
                foreach (var existItem in ExistItems)
                    DestroyImmediate(existItem.gameObject);
                ExistItems = null;
            }

            var us = new CultureInfo("en-US");
            var saves = Frontend.GetStateFilePaths();
            var list = new List<RectTransform>();
            foreach (var save in saves)
            {
                var filename = Path.GetFileNameWithoutExtension(save);
                var time = filename.Substring(filename.Length - 17, 17);
                var date = DateTime.ParseExact(time, "yyyy'-'MM'-'dd'_'HHmmss", us);
                var obj = Instantiate(ItemPrefab, Container);
                var item = obj.GetComponent<ViewStateItem>();
                item.LoadButton.onClick.AddListener(() =>
                {
                    Frontend.LoadState(save);
                    Frontend.OnMenuOpen("");
                });
                item.DeleteButton.onClick.AddListener(() =>
                {
                    if (File.Exists(save))
                        File.Delete(save);
                    _Refresh();
                });
                item.Label.text = $"{date:G}";
                list.Add(obj);
            }

            ExistItems = list.ToArray();
            NewItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                Frontend.SaveState();
                _Refresh();
            });
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